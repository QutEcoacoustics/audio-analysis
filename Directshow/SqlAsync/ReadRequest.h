#ifndef __REQUEST__
#define __REQUEST__

#include "windows.h"
#include <streams.h>

class ReadRequest
{
public:
	ReadRequest(BYTE *pBuffer, long bufferLength, DWORD_PTR dwUser, IMediaSample *extraData);

	bool BeginRead(HANDLE hFile, LONGLONG offset);
	bool IsReady();
	void WaitTillReady();
	const byte *get_Buffer() { return m_buffer; }
	const DWORD get_ReadLength() { return m_read; }

	IMediaSample *data;
	DWORD_PTR dwUser;

private:
	HANDLE m_hFile;
	OVERLAPPED m_overlapped;

	byte *m_buffer;
	bool m_isReady;
	DWORD m_read;
	LONG m_bufferLength;
};

#endif