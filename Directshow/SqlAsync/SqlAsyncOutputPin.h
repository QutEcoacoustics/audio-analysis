#ifndef __SQLASYNCOUTPUTPIN__
#define __SQLASYNCOUTPUTPIN__

#include <streams.h>
#include <vector>
#include "ReadRequest.h"

class CSqlAsyncFilter;

class CSqlAsyncOutputPin : public CBasePin, IAsyncReader
{
public:
	CSqlAsyncOutputPin(HRESULT *phr, CSqlAsyncFilter *pFilter, CCritSec *pLock);
	~CSqlAsyncOutputPin();

	DECLARE_IUNKNOWN

	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void **ppv);

	// CBasePin
	HRESULT CheckMediaType(const CMediaType *);
	HRESULT CheckConnect(IPin *pPin);
	HRESULT CompleteConnect(IPin *pReceivePin);
	HRESULT BreakConnect();
	HRESULT GetMediaType(int iPosition, CMediaType *pMediaType);
	HRESULT Active(void);
	HRESULT Inactive(void);

	// IAsyncReader
	STDMETHODIMP RequestAllocator(IMemAllocator* pPreferred, ALLOCATOR_PROPERTIES* pProps, IMemAllocator ** ppActual);
	STDMETHODIMP Request(IMediaSample* pSample, DWORD_PTR dwUser);
	STDMETHODIMP WaitForNext(DWORD dwTimeout, IMediaSample** ppSample, DWORD_PTR * pdwUser);
	STDMETHODIMP SyncReadAligned(IMediaSample* pSample);
	STDMETHODIMP SyncRead(LONGLONG llPosition, LONG lLength, BYTE* pBuffer);
	STDMETHODIMP Length(LONGLONG* pTotal, LONGLONG* pAvailable);
	STDMETHODIMP BeginFlush(void);
	STDMETHODIMP EndFlush(void);

	// Utilities
	STDMETHODIMP InitAllocator(IMemAllocator **ppAlloc);
	bool Load();
private:
	std::vector<ReadRequest *> m_requests;
	
	HANDLE m_fileHandle;
	CSqlAsyncFilter *m_filter;
	ULARGE_INTEGER m_totalLength;
	bool m_bQueriedForAsyncReader;
};

#endif