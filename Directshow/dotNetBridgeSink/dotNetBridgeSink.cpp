// Summary
//
// This is a sink filter which will pass its data to an external client. It's designed to 
// marshal DS data through to .NET, initially for reencoding audio on the fly and streaming out
// via ASP.NET
//
// It is based on the Dump sample filter from the Windows SDK.
//
// Files
//
// dotNetBridgeSink.cpp             Main implementation of the dotNetBridgeSink renderer
// dotNetBridgeSink.def             What APIs the DLL will import and export
// dotNetBridgeSink.h               Class definition of the derived renderer
// dotNetBridgeSink.rc              Version information for the sample DLL
// dotNetBridgeSinkuids.h           CLSID for the dotNetBridgeSink filter
// makefile             How to build it...
//
//
// Base classes used
//
// CBaseFilter          Base filter class supporting IMediaFilter
// CRenderedInputPin    An input pin attached to a renderer
// CUnknown             Handle IUnknown for our IFileSinkFilter
// CPosPassThru         Passes seeking interfaces upstream
// CCritSec             Helper class that wraps a critical section
//
//

#include <windows.h>
#include <commdlg.h>
#include <streams.h>
#include <initguid.h>
#include <strsafe.h>

#include "IBridgeTarget.h"
#include "IBridgeSink.h"
#include "dotNetBridgeSinkuids.h"
#include "dotNetBridgeSink.h"


// Setup data

const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
    &MEDIATYPE_NULL,            // Major type
    &MEDIASUBTYPE_NULL          // Minor type
};

const AMOVIESETUP_PIN sudPins =
{
    L"Input",                   // Pin string name
    FALSE,                      // Is it rendered
    FALSE,                      // Is it an output
    FALSE,                      // Allowed none
    FALSE,                      // Likewise many
    &CLSID_NULL,                // Connects to filter
    L"Output",                  // Connects to pin
    1,                          // Number of types
    &sudPinTypes                // Pin information
};

const AMOVIESETUP_FILTER suddotNetBridgeSink =
{
    &CLSID_dotNetBridgeSink,                // Filter CLSID
    L"dotNetBridgeSink",                    // String name
    MERIT_DO_NOT_USE,           // Filter merit
    1,                          // Number pins
    &sudPins                    // Pin details
};


//
//  Object creation stuff
//
CFactoryTemplate g_Templates[]= {
    L"dotNetBridgeSink", &CLSID_dotNetBridgeSink, CdotNetBridgeSink::CreateInstance, NULL, &suddotNetBridgeSink
};
int g_cTemplates = 1;


// Constructor

CdotNetBridgeSinkFilter::CdotNetBridgeSinkFilter(CdotNetBridgeSink *pdotNetBridgeSink,
                         LPUNKNOWN pUnk,
                         CCritSec *pLock,
                         HRESULT *phr) :
    CBaseFilter(NAME("CdotNetBridgeSinkFilter"), pUnk, pLock, CLSID_dotNetBridgeSink),
    m_pdotNetBridgeSink(pdotNetBridgeSink)
{
}


//
// GetPin
//
CBasePin * CdotNetBridgeSinkFilter::GetPin(int n)
{
    if (n == 0) {
        return m_pdotNetBridgeSink->m_pPin;
    } else {
        return NULL;
    }
}


//
// GetPinCount
//
int CdotNetBridgeSinkFilter::GetPinCount()
{
    return 1;
}


//
// Stop
//
// Overriden to close the dump file
//
STDMETHODIMP CdotNetBridgeSinkFilter::Stop()
{
    CAutoLock cObjectLock(m_pLock);
    
    return CBaseFilter::Stop();
}

//
// Pause
//
// Overriden to open the dump file
//
STDMETHODIMP CdotNetBridgeSinkFilter::Pause()
{
    CAutoLock cObjectLock(m_pLock);

    return CBaseFilter::Pause();
}


//
// Run
//
// Overriden to open the dump file
//
STDMETHODIMP CdotNetBridgeSinkFilter::Run(REFERENCE_TIME tStart)
{
    CAutoLock cObjectLock(m_pLock);

    return CBaseFilter::Run(tStart);
}


//
//  Definition of CdotNetBridgeSinkInputPin
//
CdotNetBridgeSinkInputPin::CdotNetBridgeSinkInputPin(CdotNetBridgeSink *pdotNetBridgeSink,
                             LPUNKNOWN pUnk,
                             CBaseFilter *pFilter,
                             CCritSec *pLock,
                             CCritSec *pReceiveLock,
                             HRESULT *phr) :

    CRenderedInputPin(NAME("CdotNetBridgeSinkInputPin"),
                  pFilter,                   // Filter
                  pLock,                     // Locking
                  phr,                       // Return code
                  L"Input"),                 // Pin name
    m_pReceiveLock(pReceiveLock),
    m_pdotNetBridgeSink(pdotNetBridgeSink),
    m_tLast(0),
	m_pTarget(NULL)
{
}


//
// CheckMediaType
//
// Check if the pin can support this specific proposed type and format
//
HRESULT CdotNetBridgeSinkInputPin::CheckMediaType(const CMediaType *)
{
    return S_OK;
}


//
// BreakConnect
//
// Break a connection
//
HRESULT CdotNetBridgeSinkInputPin::BreakConnect()
{
    if (m_pdotNetBridgeSink->m_pPosition != NULL) {
        m_pdotNetBridgeSink->m_pPosition->ForceRefresh();
    }

    return CRenderedInputPin::BreakConnect();
}


//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CdotNetBridgeSinkInputPin::ReceiveCanBlock()
{
    return S_OK;
}


//
// Receive
//
// Do something with this media sample
//
STDMETHODIMP CdotNetBridgeSinkInputPin::Receive(IMediaSample *pSample)
{
	printf(".");
    CheckPointer(pSample,E_POINTER);

    CAutoLock lock(m_pReceiveLock);
    PBYTE pbData;

    // Has the filter been stopped yet?
    if (!m_pTarget)
	{
		printf("f");
		return NOERROR;
	}

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    DbgLog((LOG_TRACE, 1, TEXT("tStart(%s), tStop(%s), Diff(%d ms), Bytes(%d)"),
           (LPCTSTR) CDisp(tStart),
           (LPCTSTR) CDisp(tStop),
           (LONG)((tStart - m_tLast) / 10000),
           pSample->GetActualDataLength()));

    m_tLast = tStart;

    // Copy the data to the file
    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr))
		return hr;

	if (pbData[0] == 0x52 && pbData[1] == 0x49)
		printf("RIFF");

	if (pSample->IsDiscontinuity() == S_OK)
	{
		//pSample->GetMediaTime()
		printf("DISCONT! - %s, %s", (LPCTSTR) CDisp(tStart), (LPCTSTR) CDisp(tStop));
	}
	//printf("%d\n", pSample->GetActualDataLength());
	//m_pTarget->Receive(pSample->GetActualDataLength(), pbData);
	//m_pTarget->Receive(pSample->GetActualDataLength());
	m_pTarget(pSample->GetActualDataLength(), pbData);
	return S_OK;
}

//
// EndOfStream
//
STDMETHODIMP CdotNetBridgeSinkInputPin::EndOfStream(void)
{
    CAutoLock lock(m_pReceiveLock);
	if (m_pTarget)
		m_pTarget(-1, NULL);
    return CRenderedInputPin::EndOfStream();
} // EndOfStream


//
// NewSegment
//
// Called when we are seeked
//
STDMETHODIMP CdotNetBridgeSinkInputPin::NewSegment(REFERENCE_TIME tStart,
                                       REFERENCE_TIME tStop,
                                       double dRate)
{
	m_tLast = 0;
    return S_OK;

} // NewSegment


//
//  CdotNetBridgeSink class
//
CdotNetBridgeSink::CdotNetBridgeSink(LPUNKNOWN pUnk, HRESULT *phr) :
    CUnknown(NAME("CdotNetBridgeSink"), pUnk),
    m_pFilter(NULL),
    m_pPin(NULL),
    m_pPosition(NULL)
{
    ASSERT(phr);
    
    m_pFilter = new CdotNetBridgeSinkFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

    m_pPin = new CdotNetBridgeSinkInputPin(this,GetOwner(),
                               m_pFilter,
                               &m_Lock,
                               &m_ReceiveLock,
                               phr);
    if (m_pPin == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }
}

// Destructor

CdotNetBridgeSink::~CdotNetBridgeSink()
{
    delete m_pPin;
    delete m_pFilter;
    delete m_pPosition;
}

//
// CreateInstance
//
// Provide the way for COM to create a dump filter
//
CUnknown * WINAPI CdotNetBridgeSink::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CdotNetBridgeSink *pNewObject = new CdotNetBridgeSink(punk, phr);
    if (pNewObject == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return pNewObject;

} // CreateInstance


//
// NonDelegatingQueryInterface
//
// Override this to say what interfaces we support where
//
STDMETHODIMP CdotNetBridgeSink::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);

    // Do we have this interface

    if (riid == IID_IBridgeSink)
	{
		return GetInterface((IBridgeSink *)this, ppv);
	}
	else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist) {
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    }
    else if (riid == IID_IMediaPosition || riid == IID_IMediaSeeking) {
        if (m_pPosition == NULL) 
        {

            HRESULT hr = S_OK;
            m_pPosition = new CPosPassThru(NAME("Dump Pass Through"),
                                           (IUnknown *) GetOwner(),
                                           (HRESULT *) &hr, m_pPin);
            if (m_pPosition == NULL) 
                return E_OUTOFMEMORY;

            if (FAILED(hr)) 
            {
                delete m_pPosition;
                m_pPosition = NULL;
                return hr;
            }
        }

        return m_pPosition->NonDelegatingQueryInterface(riid, ppv);
    } 

    return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterSever
//
// Handle the registration of this filter
//
STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2( TRUE );

} // DllRegisterServer


//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
    return AMovieDllRegisterServer2( FALSE );

} // DllUnregisterServer


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

