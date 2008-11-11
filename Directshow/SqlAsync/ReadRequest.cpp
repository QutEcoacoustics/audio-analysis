#include "ReadRequest.h"

ReadRequest::ReadRequest(BYTE *pBuffer, long bufferLength, DWORD_PTR newDwUser, IMediaSample *extraData) : 
	m_bufferLength(bufferLength), m_isReady(false), m_buffer(pBuffer),
	m_hFile(NULL), dwUser(newDwUser), data(extraData)
{
	ZeroMemory(&m_overlapped, sizeof(OVERLAPPED));
}

bool ReadRequest::BeginRead(HANDLE hFile, LONGLONG offset)
{
	m_hFile = hFile;
	LARGE_INTEGER li;
	memcpy(&li, &offset, sizeof(LONGLONG));
	m_overlapped.Offset = li.LowPart;
	m_overlapped.OffsetHigh = li.HighPart;

	m_isReady = ReadFile(m_hFile, m_buffer, m_bufferLength, &m_read, &m_overlapped);
	if (!m_isReady)
	{
		DWORD err = GetLastError();
		return err == ERROR_IO_PENDING;
	}
	return true;
}

bool ReadRequest::IsReady()
{
	if (m_isReady)
		return true;
	m_isReady = GetOverlappedResult(m_hFile, &m_overlapped, &m_read, FALSE);
	return m_isReady;
}

void ReadRequest::WaitTillReady()
{
	if (!m_isReady)
		GetOverlappedResult(m_hFile, &m_overlapped, &m_read, TRUE);
}