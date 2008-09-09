class CdotNetBridgeSinkInputPin;
class CdotNetBridgeSink;
class CdotNetBridgeSinkFilter;

#define BYTES_PER_LINE 20
#define FIRST_HALF_LINE TEXT  ("   %2x %2x %2x %2x %2x %2x %2x %2x %2x %2x")
#define SECOND_HALF_LINE TEXT (" %2x %2x %2x %2x %2x %2x %2x %2x %2x %2x")


// Main filter object

class CdotNetBridgeSinkFilter : public CBaseFilter
{
    CdotNetBridgeSink * const m_pdotNetBridgeSink;

public:

    // Constructor
    CdotNetBridgeSinkFilter(CdotNetBridgeSink *pdotNetBridgeSink,
                LPUNKNOWN pUnk,
                CCritSec *pLock,
                HRESULT *phr);

    // Pin enumeration
    CBasePin * GetPin(int n);
    int GetPinCount();

    // Open and close the file as necessary
    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();
};


//  Pin object

class CdotNetBridgeSinkInputPin : public CRenderedInputPin
{
    CdotNetBridgeSink    * const m_pdotNetBridgeSink;           // Main renderer object
    CCritSec * const m_pReceiveLock;    // Sample critical section
    REFERENCE_TIME m_tLast;             // Last sample receive time

public:
	//IBridgeTarget *m_pTarget;
	ReceiveData m_pTarget;

    CdotNetBridgeSinkInputPin(CdotNetBridgeSink *pdotNetBridgeSink,
                  LPUNKNOWN pUnk,
                  CBaseFilter *pFilter,
                  CCritSec *pLock,
                  CCritSec *pReceiveLock,
                  HRESULT *phr);

    // Do something with this media sample
    STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream(void);
    STDMETHODIMP ReceiveCanBlock();

    // Check if the pin can support this specific proposed type and format
    HRESULT CheckMediaType(const CMediaType *);

    // Break connection
    HRESULT BreakConnect();

    // Track NewSegment
    STDMETHODIMP NewSegment(REFERENCE_TIME tStart,
                            REFERENCE_TIME tStop,
                            double dRate);
};


//  CdotNetBridgeSink object which has filter and pin members

class CdotNetBridgeSink : public CUnknown, public IBridgeSink
{
    friend class CdotNetBridgeSinkFilter;
    friend class CdotNetBridgeSinkInputPin;

    CdotNetBridgeSinkFilter   *m_pFilter;       // Methods for filter interfaces
    CdotNetBridgeSinkInputPin *m_pPin;          // A simple rendered input pin

    CCritSec m_Lock;                // Main renderer critical section
    CCritSec m_ReceiveLock;         // Sublock for received samples

    CPosPassThru *m_pPosition;      // Renderer position controls

public:

    DECLARE_IUNKNOWN

    CdotNetBridgeSink(LPUNKNOWN pUnk, HRESULT *phr);
    ~CdotNetBridgeSink();

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

	// IBridgeSink
	//STDMETHODIMP set_Target(IBridgeTarget *target) {target->AddRef(); m_pPin->m_pTarget = target; target->Test(); target->Test(); return S_OK;}
	STDMETHODIMP set_Target(ReceiveData target) {m_pPin->m_pTarget = target; return S_OK;}
	

private:

    // Overriden to say what interfaces we support where
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);
};

