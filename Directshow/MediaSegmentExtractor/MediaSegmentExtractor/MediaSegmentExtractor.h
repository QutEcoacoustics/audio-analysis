// Media Segment Extractor Filter based on source by Kovalev Maxim - http://www.codeproject.com/KB/audio-video/DirectShowMediaTrim.aspx
// Used with permission. Original code licensed under Code Project Open License (CPOL) 1.02 - http://www.codeproject.com/info/cpol10.aspx
#ifndef __MEDIASEGMENTEXTRACTOR_H__
#define __MEDIASEGMENTEXTRACTOR_H__

#pragma warning (disable:4312)

#include <streams.h> // DirectShow (includes windows.h)
#include <initguid.h> // Declares DEFINE_GUID to declare an EXTERN_C const
#include "Interfaces.h"

class CMediaSegmentExtractor : public CTransInPlaceFilter, IMediaSegmentExtractor
{
	public:
		static CUnknown *WINAPI CreateInstance (LPUNKNOWN pUnk, HRESULT *pHr);
		DECLARE_IUNKNOWN;
		
		STDMETHODIMP NonDelegatingQueryInterface (REFIID riid, void **ppv);

		// IMediaSegmentExtractor Implementation
		STDMETHODIMP SetInterval(LONGLONG startTime, LONGLONG endTime)
			{ this->startTime = startTime * 10000; this->endTime = endTime * 10000; return S_OK; }

	private:
		CMediaSegmentExtractor (TCHAR *tszName, LPUNKNOWN pUnk, HRESULT *pHr)
			: CTransInPlaceFilter (tszName, pUnk, CLSID_MediaSegmentExtractor, pHr), startedSending(false), complete(true) {}

		HRESULT Transform (IMediaSample *pSample);
		HRESULT Receive(IMediaSample *pSample);

		HRESULT CheckInputType (const CMediaType *pMediaTypeIn) { return S_OK; }

		LONGLONG startTime, endTime;
		bool startedSending, complete;
};
#endif //__MEDIASEGMENTEXTRACTOR_H__