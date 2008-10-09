// Media Segment Extractor Filter based on source by Kovalev Maxim - http://www.codeproject.com/KB/audio-video/DirectShowMediaTrim.aspx
// Used with permission. Original code licensed under Code Project Open License (CPOL) 1.02 - http://www.codeproject.com/info/cpol10.aspx
#ifndef __MEDIASEGMENTEXTRACTOR_INTERFACES_H__
#define __MEDIASEGMENTEXTRACTOR_INTERFACES_H__

// {6CF9565F-5544-49f8-B63B-A7ACD4D4F9E2}
DEFINE_GUID(CLSID_MediaSegmentExtractor, 
0x6cf9565f, 0x5544, 0x49f8, 0xb6, 0x3b, 0xa7, 0xac, 0xd4, 0xd4, 0xf9, 0xe2);

// {7D273EC9-A7BC-44e3-ACE2-15868959F760}
DEFINE_GUID(IID_IMediaSegmentExtractor, 
0x7d273ec9, 0xa7bc, 0x44e3, 0xac, 0xe2, 0x15, 0x86, 0x89, 0x59, 0xf7, 0x60);

interface IMediaSegmentExtractor : public IUnknown
{
	STDMETHOD (SetInterval) (LONGLONG startTime, LONGLONG endTime) = 0;
};

#endif //__MEDIASEGMENTEXTRACTOR_INTERFACES_H__