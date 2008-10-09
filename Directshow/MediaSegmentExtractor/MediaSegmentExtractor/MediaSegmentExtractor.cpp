// Media Segment Extractor Filter based on source by Kovalev Maxim - http://www.codeproject.com/KB/audio-video/DirectShowMediaTrim.aspx
// Used with permission. Original code licensed under Code Project Open License (CPOL) 1.02 - http://www.codeproject.com/info/cpol10.aspx

#include "MediaSegmentExtractor.h"

HRESULT CMediaSegmentExtractor::Receive(IMediaSample *pSample)
{
    /*  Check for other streams and pass them on */
    AM_SAMPLE2_PROPERTIES * const pProps = m_pInput->SampleProps();
    if (pProps->dwStreamId != AM_STREAM_MEDIA)
        return m_pOutput->Deliver(pSample);
    
    // Start timing the TransInPlace (if PERF is defined)
    MSR_START(m_idTransInPlace);

    if (UsingDifferentAllocators()) {

        // We have to copy the data.
        pSample = Copy(pSample);

        if (pSample==NULL) {
            MSR_STOP(m_idTransInPlace);
            return E_UNEXPECTED;
        }
    }

    // have the derived class transform the data
    HRESULT hr = Transform(pSample);

    // Stop the clock and log it (if PERF is defined)
    MSR_STOP(m_idTransInPlace);

    if (FAILED(hr)) {
        DbgLog((LOG_TRACE, 1, TEXT("Error from TransInPlace")));
        if (UsingDifferentAllocators()) {
            pSample->Release();
        }
        return hr;
    }


    // the Transform() function can return S_FALSE to indicate that the
    // sample should not be delivered; we only deliver the sample if it's
    // really S_OK (same as NOERROR, of course.)
    if (hr == NOERROR)
	    hr = m_pOutput->Deliver(pSample);
	else
	{
        // S_FALSE returned from Transform is a PRIVATE agreement
        // We should return NOERROR from Receive() in this cause because returning S_FALSE
        // from Receive() means that this is the end of the stream and no more data should
        // be sent.
        if (S_FALSE == hr || (S_FALSE + 1) == hr)
		{
            //  Release the sample before calling notify to avoid
            //  deadlocks if the sample holds a lock on the system
            //  such as DirectDraw buffers do
            m_bSampleSkipped = TRUE;
            if (!m_bQualityChanged)
			{
                NotifyEvent(EC_QUALITY_CHANGE,0,0);
                m_bQualityChanged = TRUE;
            }
			// S_FALSE + 1 indicates we've finished and can stop the graph
			return ((S_FALSE + 1) == hr) ? S_FALSE : NOERROR;
        }
    }

    // release the output buffer. If the connected pin still needs it,
    // it will have addrefed it itself.
    if (UsingDifferentAllocators())
        pSample->Release();

    return hr;
}

HRESULT CMediaSegmentExtractor::Transform(IMediaSample *pSample)
{
	LONGLONG sampleStartTime = 0;
	LONGLONG sampleEndTime = 0;
	pSample->GetTime(&sampleStartTime, &sampleEndTime);

	// Summary time, for which the sample have to be moved
	LONGLONG totalDelta = 0;
	if (sampleEndTime < startTime)
		return S_FALSE;
	else if (sampleEndTime > endTime)
		return S_FALSE + 1;
	else if (!startedSending)
	{
		startedSending = true;
		startTime = sampleStartTime;
		return NOERROR;
	}
	else
	{
		LONGLONG newSampleStartTime = sampleStartTime - (startTime);
		LONGLONG newSampleEndTime = sampleEndTime - (startTime);
		pSample->SetTime(&newSampleStartTime, &newSampleEndTime);

		return NOERROR;
	}
}

CUnknown *WINAPI CMediaSegmentExtractor::CreateInstance (LPUNKNOWN pUnk, HRESULT *pHr)
{
	CMediaSegmentExtractor *pNewObject = new CMediaSegmentExtractor(NAME("Media Segment Extractor"), pUnk, pHr);
	if (pNewObject == NULL)
		*pHr = E_OUTOFMEMORY;
	return pNewObject;
}

STDMETHODIMP CMediaSegmentExtractor::NonDelegatingQueryInterface (REFIID riid, void **ppv)
{
	if (riid == IID_IMediaSegmentExtractor)
		return GetInterface(static_cast <IMediaSegmentExtractor *> (this), ppv);
	return CTransInPlaceFilter::NonDelegatingQueryInterface (riid, ppv);
}