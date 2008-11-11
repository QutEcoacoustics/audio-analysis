#ifndef __SQLASYNCFILTER__
#define __SQLASYNCFILTER__

#include <streams.h>
#include "ISqlFileSource.h"
#include "SqlAsyncOutputPin.h"
#include "initguid.h"

// {0C14D74A-64DD-4b7d-A1E9-B54FDDB54C54}
DEFINE_GUID(CLSID_SqlAsync,
			0xc14d74a, 0x64dd, 0x4b7d, 0xa1, 0xe9, 0xb5, 0x4f, 0xdd, 0xb5, 0x4c, 0x54);

class CSqlAsyncFilter : public CBaseFilter, IFileSourceFilter, ISqlFileSource
{
	friend CSqlAsyncOutputPin;

public:
	static CUnknown * WINAPI CreateInstance(LPUNKNOWN, HRESULT *);

	CSqlAsyncFilter(LPUNKNOWN pUnk);

	DECLARE_IUNKNOWN

	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void **ppv);

	// CBaseFilter
	int GetPinCount();
	CBasePin *GetPin(int);
	STDMETHODIMP Stop();
	STDMETHODIMP Run(REFERENCE_TIME tStart);

	// IFileSourceFilter
	STDMETHODIMP Load(LPCOLESTR lpwszFileName, const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt);

	// ISqlFileSource
	STDMETHODIMP set_TransactionContext(PBYTE context, LONG length);
	
private:
	CSqlAsyncOutputPin *m_pin;
	CMediaType m_mt;
	LPWSTR m_pFileName;

	// ISqlFileSource
	PBYTE m_txContext;
	LONG m_txContextLength;
	CCritSec m_pMyLock;
};
#endif