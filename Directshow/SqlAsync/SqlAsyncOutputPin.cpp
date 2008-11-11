#include "SqlAsyncOutputPin.h"
#include "SqlAsyncFilter.h"
#include "sqlncli.h"

using namespace std;

CSqlAsyncOutputPin::CSqlAsyncOutputPin(HRESULT *phr, CSqlAsyncFilter *pFilter, CCritSec *pLock) : 
	CBasePin(NAME("SQL Async Output Pin"), pFilter, pLock, phr, L"Output", PINDIR_OUTPUT),
	m_requests(), m_filter(pFilter), m_fileHandle(NULL)
{ }

CSqlAsyncOutputPin::~CSqlAsyncOutputPin()
{
	if (m_fileHandle)
	{
		CloseHandle(m_fileHandle);
		m_fileHandle = NULL;
	}
}

HRESULT CSqlAsyncOutputPin::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
	if (riid == IID_IAsyncReader)
	{
		m_bQueriedForAsyncReader = true;
		return GetInterface((IAsyncReader *)this, ppv);
	}
	else
		return CBasePin::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CSqlAsyncOutputPin::GetMediaType(int iPosition, CMediaType *pMediaType)
{
	if (iPosition < 0)
		return E_INVALIDARG;
	if (iPosition > 0)
		return VFW_S_NO_MORE_ITEMS;

	CheckPointer(pMediaType, E_POINTER);

	*pMediaType = m_filter->m_mt;

	return S_OK;
}

HRESULT CSqlAsyncOutputPin::CheckMediaType(const CMediaType *pType)
{
	CAutoLock lck(m_pLock);

	/*  We treat MEDIASUBTYPE_NULL subtype as a wild card */
	return ((m_filter->m_mt.majortype == MEDIATYPE_NULL || m_filter->m_mt.majortype == pType->majortype) &&
		(m_filter->m_mt.subtype == MEDIASUBTYPE_NULL || m_filter->m_mt.subtype == pType->subtype))
		? S_OK : S_FALSE;
}

// Clear the flag so we see if IAsyncReader is queried for
HRESULT CSqlAsyncOutputPin::CheckConnect(IPin *pPin)
{
	m_bQueriedForAsyncReader = false;
	return CBasePin::CheckConnect(pPin);
}

// See if it was asked for
HRESULT CSqlAsyncOutputPin::CompleteConnect(IPin *pReceivePin)
{
	if (m_bQueriedForAsyncReader)
		return CBasePin::CompleteConnect(pReceivePin);
	else
	{
#ifdef VFW_E_NO_TRANSPORT
		return VFW_E_NO_TRANSPORT;
#else
		return E_FAIL;
#endif
	}
}

HRESULT CSqlAsyncOutputPin::BreakConnect()
{
	m_bQueriedForAsyncReader = false;
	return CBasePin::BreakConnect();
}

HRESULT CSqlAsyncOutputPin::Active()
{
	if (!m_fileHandle)
		Load();
	return CBasePin::Active();
}

HRESULT CSqlAsyncOutputPin::Inactive()
{
	if (m_fileHandle)
	{
		CloseHandle(m_fileHandle);
		m_fileHandle = NULL;
	}
	return CBasePin::Inactive();
}

// --- IAsyncReader methods ---
// pass in your preferred allocator and your preferred properties.
// method returns the actual allocator to be used. Call GetProperties
// on returned allocator to learn alignment and prefix etc chosen.
// this allocator will be not be committed and decommitted by
// the async reader, only by the consumer.
HRESULT CSqlAsyncOutputPin::RequestAllocator(IMemAllocator* pPreferred, ALLOCATOR_PROPERTIES* pProps, IMemAllocator **ppActual)
{
	CheckPointer(pPreferred,E_POINTER);
	CheckPointer(pProps,E_POINTER);
	CheckPointer(ppActual,E_POINTER);

	ALLOCATOR_PROPERTIES Actual;
	HRESULT hr;

	if (pPreferred)
	{
		hr = pPreferred->SetProperties(pProps, &Actual);

		if (SUCCEEDED(hr))
		{
			pPreferred->AddRef();
			*ppActual = pPreferred;
			return S_OK;
		}
	}

	// create our own allocator
	IMemAllocator *pAlloc;
	hr = InitAllocator(&pAlloc);
	if (FAILED(hr))
		return hr;

	// we need to release our refcount on pAlloc, and addref
	// it to pass a refcount to the caller - this is a net nothing.
	*ppActual = pAlloc;
	return S_OK;
}

HRESULT CSqlAsyncOutputPin::InitAllocator(IMemAllocator **ppAlloc)
{
	CheckPointer(ppAlloc, E_POINTER);

	HRESULT hr = NOERROR;
	CMemAllocator *pMemObject = NULL;
	*ppAlloc = NULL;

	/* Create a default memory allocator */
	pMemObject = new CMemAllocator(NAME("Base memory allocator"), NULL, &hr);
	if (pMemObject == NULL)
		return E_OUTOFMEMORY;
	if (FAILED(hr))
	{
		delete pMemObject;
		return hr;
	}

	/* Get a reference counted IID_IMemAllocator interface */
	hr = pMemObject->QueryInterface(IID_IMemAllocator, (void **)ppAlloc);
	if (FAILED(hr))
	{
		delete pMemObject;
		return E_NOINTERFACE;
	}

	ASSERT(*ppAlloc != NULL);
	return NOERROR;
}

// queue a request for data.
// media sample start and stop times contain the requested absolute
// byte position (start inclusive, stop exclusive).
// may fail if sample not obtained from agreed allocator.
// may fail if start/stop position does not match agreed alignment.
// samples allocated from source pin's allocator may fail
// GetPointer until after returning from WaitForNext.
HRESULT CSqlAsyncOutputPin::Request(IMediaSample* pSample, DWORD_PTR dwUser)
{
	CheckPointer(pSample, E_POINTER);

	REFERENCE_TIME tStart, tStop;
	HRESULT hr = pSample->GetTime(&tStart, &tStop);
	if (FAILED(hr))
		return hr;

	LONGLONG llPos = tStart / UNITS;

	BYTE* pBuffer;
	hr = pSample->GetPointer(&pBuffer);
	if (FAILED(hr))
		return hr;
	long bufferLength = pSample->GetSize();

	ReadRequest *request = new ReadRequest(pBuffer, bufferLength, dwUser, pSample);
	if (request->BeginRead(m_fileHandle, llPos))
	{
		m_requests.push_back(request);
		return S_OK;
	}
	return E_FAIL;
}

// block until the next sample is completed or the timeout occurs.
// timeout (millisecs) may be 0 or INFINITE. Samples may not
// be delivered in order. If there is a read error of any sort, a
// notification will already have been sent by the source filter,
// and STDMETHODIMP will be an error.
HRESULT CSqlAsyncOutputPin::WaitForNext(DWORD dwTimeout, IMediaSample** ppSample, DWORD_PTR *pdwUser)
{
	while (true)
	{
		for (vector<ReadRequest *>::iterator i = m_requests.begin(); i != m_requests.end(); i++)
		{
			if ((*i)->IsReady())
			{
				// Fill sample
				*pdwUser = (*i)->dwUser;
				*ppSample = (*i)->data;
				(*ppSample)->SetActualDataLength((*i)->get_ReadLength());

				return S_OK;
			}
		}
	}
	return E_FAIL;
}

// sync read of data. Sample passed in must have been acquired from
// the agreed allocator. Start and stop position must be aligned.
// equivalent to a Request/WaitForNext pair, but may avoid the
// need for a thread on the source filter.
HRESULT CSqlAsyncOutputPin::SyncReadAligned(IMediaSample* pSample)
{
	CheckPointer(pSample, E_POINTER);

	REFERENCE_TIME tStart, tStop;
	HRESULT hr = pSample->GetTime(&tStart, &tStop);
	if (FAILED(hr))
		return hr;

	LONGLONG llPos = tStart / UNITS;

	BYTE* pBuffer;
	hr = pSample->GetPointer(&pBuffer);
	if (FAILED(hr))
		return hr;
	long bufferLength = pSample->GetSize();

	ReadRequest *request = new ReadRequest(pBuffer, bufferLength, NULL, pSample);
	if (!request->BeginRead(m_fileHandle, llPos))
		return E_FAIL;

	request->WaitTillReady();
	pSample->SetActualDataLength(request->get_ReadLength());

	return S_OK;
}


// sync read. works in stopped state as well as run state.
// need not be aligned. Will fail if read is beyond actual total
// length.
HRESULT CSqlAsyncOutputPin::SyncRead(LONGLONG llPosition, LONG lLength, BYTE* pBuffer)
{
	CheckPointer(pBuffer, E_POINTER);

	ReadRequest *request = new ReadRequest(pBuffer, lLength, NULL, NULL);
	if (!request->BeginRead(m_fileHandle, llPosition))
		return E_FAIL;

	request->WaitTillReady();

	return S_OK;
}

// return total length of stream, and currently available length.
// reads for beyond the available length but within the total length will
// normally succeed but may block for a long period.
STDMETHODIMP CSqlAsyncOutputPin::Length(LONGLONG* pTotal, LONGLONG* pAvailable)
{
	*pAvailable = *pTotal = m_totalLength.QuadPart;
	return S_OK;
}

// cause all outstanding reads to return, possibly with a failure code
// (VFW_E_TIMEOUT) indicating they were cancelled.
// these are defined on IAsyncReader and IPin
STDMETHODIMP CSqlAsyncOutputPin::BeginFlush(void)
{
	// TODO: Implement
	return E_FAIL;
}

STDMETHODIMP CSqlAsyncOutputPin::EndFlush(void)
{
	// TODO: Implement
	return E_FAIL;
}

bool CSqlAsyncOutputPin::Load()
{
	m_fileHandle = OpenSqlFilestream(m_filter->m_pFileName, SQL_FILESTREAM_READ, 0, m_filter->m_txContext, m_filter->m_txContextLength, 0);
	if (m_fileHandle == INVALID_HANDLE_VALUE) 
		return false;

	m_totalLength.LowPart = GetFileSize(m_fileHandle, &m_totalLength.HighPart);
	/*CloseHandle(m_fileHandle);
	m_fileHandle = NULL;*/
	return true;
}