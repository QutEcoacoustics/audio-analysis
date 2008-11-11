#include "SqlAsyncFilter.h"
#include "strsafe.h"

CUnknown * WINAPI CSqlAsyncFilter::CreateInstance(LPUNKNOWN pUnk, HRESULT *phr)
{
	ASSERT(phr);

	//  DLLEntry does the right thing with the return code and
	//  the returned value on failure

	return new CSqlAsyncFilter(pUnk);
}

CSqlAsyncFilter::CSqlAsyncFilter(LPUNKNOWN pUnk) :
	CBaseFilter(NAME("SQL Async Filter"), pUnk, &m_pMyLock, CLSID_SqlAsync, NULL)
{
	HRESULT hr;
	m_pin = new CSqlAsyncOutputPin(&hr, this, &m_pMyLock);
}

int CSqlAsyncFilter::GetPinCount()
{
	return 1;
}

CBasePin *CSqlAsyncFilter::GetPin(int i)
{
	return i == 0 ? m_pin : NULL;
}

STDMETHODIMP CSqlAsyncFilter::Stop()
{
	return CBaseFilter::Stop();
}

STDMETHODIMP CSqlAsyncFilter::Run(REFERENCE_TIME tStart)
{
	return CBaseFilter::Run(tStart);
}

STDMETHODIMP CSqlAsyncFilter::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
	if (riid == IID_IFileSourceFilter)
		return GetInterface((IFileSourceFilter *)this, ppv);
	else if (riid == IID_IAsyncReader)
		return GetInterface((IAsyncReader *)this, ppv);
	else if (riid == IID_ISqlFileSource)
		return GetInterface((ISqlFileSource *)this, ppv);
	else
		return CBaseFilter::NonDelegatingQueryInterface(riid, ppv);
}

// IFileSourceFilter
STDMETHODIMP CSqlAsyncFilter::Load(LPCOLESTR lpwszFileName, const AM_MEDIA_TYPE *pmt)
{
	CheckPointer(lpwszFileName, E_POINTER);

	CAutoLock lck(m_pLock);

	if (NULL == pmt)
	{
		m_mt.SetType(&MEDIATYPE_NULL);
		m_mt.SetSubtype(&MEDIASUBTYPE_NULL);
	}
	else
		m_mt = *pmt;
	m_mt.bTemporalCompression = TRUE;
	m_mt.lSampleSize = 1;

	int length = lstrlenW(lpwszFileName);
	m_pFileName = new WCHAR[length + 1];
	if (m_pFileName != NULL)
		StringCchCopyW(m_pFileName, length + 1, lpwszFileName);

	return m_pin->Load() ? S_OK : E_FAIL;
}

STDMETHODIMP CSqlAsyncFilter::GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt)
{
	CheckPointer(ppszFileName, E_POINTER);
	*ppszFileName = NULL;

	if (m_pFileName!=NULL) {
		DWORD n = sizeof(WCHAR) * (1 + lstrlenW(m_pFileName));

		*ppszFileName = (LPOLESTR)CoTaskMemAlloc(n);
		if (*ppszFileName)
			CopyMemory(*ppszFileName, m_pFileName, n);
	}

	if (pmt)
		CopyMediaType(pmt, &m_mt);

	return NOERROR;
}

// ISqlAsyncFilter
STDMETHODIMP CSqlAsyncFilter::set_TransactionContext(PBYTE context, LONG length)
{
	m_txContext = new byte[length];
	CopyMemory(m_txContext, context, length);
	m_txContextLength = length;

	return NOERROR;
}