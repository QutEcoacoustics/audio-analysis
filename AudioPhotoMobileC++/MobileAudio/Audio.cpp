// ASFWriter.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "AudioPhoto.h"
#include "CPropertyBag.h"
#include <windows.h>
#include <commctrl.h>
#include <streams.h>
#include <dmodshow.h>
#include <dmoreg.h>
#include <wmcodecids.h>
#include <dshow.h>
#include <strmif.h>
#include <evcode.h>

#define MAX_LOADSTRING 100

// GDI Escapes for ExtEscape()
#define QUERYESCSUPPORT    8

// The following are unique to CE
#define GETVFRAMEPHYSICAL   6144
#define GETVFRAMELEN    6145
#define DBGDRIVERSTAT    6146
#define SETPOWERMANAGEMENT   6147
#define GETPOWERMANAGEMENT   6148


typedef enum _VIDEO_POWER_STATE {
	VideoPowerOn = 1,
	VideoPowerStandBy,
	VideoPowerSuspend,
	VideoPowerOff
} VIDEO_POWER_STATE, *PVIDEO_POWER_STATE;


typedef struct _VIDEO_POWER_MANAGEMENT {
	ULONG Length;
	ULONG DPMSVersion;
	ULONG PowerState;
} VIDEO_POWER_MANAGEMENT, *PVIDEO_POWER_MANAGEMENT;

// Forward declarations of functions included in this code module:

//BOOL			InitializeAudioRecording();


//HDC hdc2, hdcTemp;


// Declare pointers to DirectShow interfaces
CComPtr<ICaptureGraphBuilder2>  pCaptureGraphBuilder;
CComPtr<IBaseFilter>            pVideoCap, pAsfWriter,
pVideoEncoder, pAudioDecoder, pAudioCaptureFilter;
CComPtr<IDMOWrapperFilter>      pVideoWrapperFilter, pAudioWrapperFilter;
CComPtr<IPersistPropertyBag>    pPropertyBag;
CComPtr<IGraphBuilder>          pGraph;
CComPtr<IMediaControl>          pMediaControl;
CComPtr<IMediaEvent>            pMediaEvent;
CComPtr<IMediaSeeking>          pMediaSeeking;
CComPtr<IFileSinkFilter>        pFileSink;


VIDEOINFOHEADER *pVih;

LONGLONG dwEnd, dwStart =0;
long    lEventCode, lParam1, lParam2;
int count = 0;

HRESULT hr = S_OK;

CComVariant   varCamName;
CPropertyBag  PropBag;	

//
//   FUNCTION: InitInstance(HINSTANCE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
extern BOOL InitializeAudioRecording()
{
	CoInitialize( NULL );

	// Create the graph builder and the filtergraph
	pCaptureGraphBuilder.CoCreateInstance( CLSID_CaptureGraphBuilder );
	pGraph.CoCreateInstance( CLSID_FilterGraph );
	pCaptureGraphBuilder->SetFiltergraph( pGraph );

	// Query all the interfaces needed later
	pGraph.QueryInterface( &pMediaControl );
	pGraph.QueryInterface( &pMediaEvent );
	pGraph.QueryInterface( &pMediaSeeking );

	return TRUE;
}

extern BOOL BeginAudioRecording(LPWSTR str){
	pMediaControl->Stop();
	pGraph.Release();
	pCaptureGraphBuilder.Release();
	pMediaControl.Release();
	pMediaEvent.Release();
	pMediaSeeking.Release();
	pCaptureGraphBuilder.CoCreateInstance( CLSID_CaptureGraphBuilder );
	pGraph.CoCreateInstance( CLSID_FilterGraph );
	pCaptureGraphBuilder->SetFiltergraph( pGraph );

	// Query all the interfaces needed later
	pGraph.QueryInterface( &pMediaControl );
	pGraph.QueryInterface( &pMediaEvent );
	pGraph.QueryInterface( &pMediaSeeking );

	// Initialize the video capture filter
	//pVideoCap.CoCreateInstance( CLSID_VideoCapture ); 
	//pVideoCap.QueryInterface( &pPropertyBag );
	//varCamName = L"CAM1:";
	//if(( varCamName.vt == VT_BSTR ) == NULL ) {
	//  return E_OUTOFMEMORY;
	//}
	//PropBag.Write( L"VCapName", &varCamName );   
	//pPropertyBag->Load( &PropBag, NULL );
	//pPropertyBag.Release();
	//pGraph->AddFilter( pVideoCap, L"Video capture source" );

	// Initialize the audio capture filter
	hr = pAudioCaptureFilter.CoCreateInstance( CLSID_AudioCapture );
	hr = pAudioCaptureFilter.QueryInterface( &pPropertyBag );
	hr = pPropertyBag->Load( NULL, NULL );
	hr = pGraph->AddFilter( pAudioCaptureFilter, L"Audio Capture Filter" );

	// Initialize the Video DMO Wrapper
	//hr = pVideoEncoder.CoCreateInstance( CLSID_DMOWrapperFilter );
	//hr = pVideoEncoder.QueryInterface( &pVideoWrapperFilter );

	// Load the WMV9 encoder in the DMO Wrapper. 
	// To encode in MPEG, replace CLSID_CWMV9EncMediaObject with the 
	// CLSID of your DMO
	//hr = pVideoWrapperFilter->Init( CLSID_CWMV9EncMediaObject,
	//						   DMOCATEGORY_VIDEO_ENCODER );
	//hr = pGraph->AddFilter( pVideoEncoder, L"WMV9 DMO Encoder" );

	// Load ASF multiplexer. 
	// To create a MPEG file, change the CLSID_ASFWriter into the GUID
	// of your multiplexer
	//hr = pAsfWriter.CoCreateInstance( CLSID_ASFWriter );
	//hr = pAsfWriter->QueryInterface( IID_IFileSinkFilter, (void**) &pFileSink );
	//hr = pFileSink->SetFileName( L"\\Storage Card\\My Documents\\test0.asf", NULL );
	
	
	//Setting string info this way should allow us to clean it up, but sending
	//as a char* in c# is unsafe.  OHNOES!
	//LPCSTR ansistr = strcat("\\Storage Card\\", filename);
	//int a = lstrlenA(ansistr);
	//BSTR unicodestr = SysAllocStringLen(NULL, 40);
	//BSTR unicodestr = L"\\Storage Card\\audio.asf";
	//MultiByteToWideChar(CP_ACP, 0, ansistr, -1, unicodestr, sizeof unicodestr / sizeof (WCHAR));
	//USES_CONVERSION;

	hr = pCaptureGraphBuilder->SetOutputFileName(
		&MEDIASUBTYPE_Asf,   // Create a Windows Media file.
		str,   // File name.
		&pAsfWriter,         // Receives a pointer to the filter.
		&pFileSink);  // Receives an IFileSinkFilter interface pointer (optional).

	// Connect the preview pin to the video renderer
	//hr = pCaptureGraphBuilder->RenderStream( &PIN_CATEGORY_PREVIEW,
	//									&MEDIATYPE_Video, pVideoCap,
	//									NULL, NULL );

	//Setting video capture properties, may change later to <CComPtr> to make more consistant with the rest of the implementation
	//IAMStreamConfig *pConfig = NULL;
	//hr = pCaptureGraphBuilder->FindInterface(&PIN_CATEGORY_CAPTURE, 0, pVideoCap, IID_IAMStreamConfig,  (void**)&pConfig);  //Find just where the setting info is hiding
	//																													//in our little chain
	////hr=pCapture->QueryInterface(&pConfig);
	//int iCount = 0, iSize = 0;
	//hr = pConfig->GetNumberOfCapabilities(&iCount, &iSize);		//Retrieve what our camera is capable of

	//// Check the size to make sure we pass in the correct structure.
	//if (iSize == sizeof(VIDEO_STREAM_CONFIG_CAPS))	{
	//	// Use the video capabilities structure.
	//	for (int iFormat = 0; iFormat < iCount; iFormat++)		{
	//		VIDEO_STREAM_CONFIG_CAPS scc;
	//		AM_MEDIA_TYPE *pmtConfig;		//A media configeration type holder that we can test
	//		hr = pConfig->GetStreamCaps(iFormat, &pmtConfig, (BYTE*)&scc);  //A configuration to test
	//		if (SUCCEEDED(hr))			{
	//		//	if ((pmtConfig->majortype == MEDIATYPE_Video) &&						//This test needs to be worked on more.  Video is cycled through without it.
	//		//					(pmtConfig->subtype == MEDIASUBTYPE_RGB24) &&			//Would be best for clarity and different devices.
	//		//					(pmtConfig->formattype == FORMAT_VideoInfo) &&
	//		//					(pmtConfig->cbFormat >= sizeof (VIDEOINFOHEADER)) &&
	//		//					(pmtConfig->pbFormat != NULL))
	//		//	{			
	//				pVih = (VIDEOINFOHEADER*)pmtConfig->pbFormat;  //Get display (bitmap) info of currently tested capability
	//				// pVih contains the detailed format information.
	//				if(pVih->bmiHeader.biWidth == 176){  //Our 320x240 resolution
	//					hr = pConfig->SetFormat(pmtConfig);  //Force our camera capture to use this confguration
	//				}
	//			//}	 
	//		}
	//           // Delete the media type.
	//           DeleteMediaType(pmtConfig);
	//       }
	//}

	// Connect the video capture pin to the multiplexer through the
	// video renderer. 
	//hr = pCaptureGraphBuilder->RenderStream( &PIN_CATEGORY_CAPTURE,
	//	&MEDIATYPE_Video, pVideoCaptureFilter, NULL, pAsfWriter );

	// Connect the audio capture pin to the multiplexer through the
	// audio renderer. 
	hr = pCaptureGraphBuilder->RenderStream( &PIN_CATEGORY_CAPTURE,
		&MEDIATYPE_Audio, pAudioCaptureFilter, NULL, pAsfWriter );


	// Block the capture.
	hr = pCaptureGraphBuilder->ControlStream( &PIN_CATEGORY_CAPTURE,
		&MEDIATYPE_Audio, pAudioCaptureFilter,
		0, 0 ,0,0 );


	// Let's run the graph and wait for a bit before dumping to file. 
	//TODO: See how this works without the sleep.
	pMediaControl->Run();
	Sleep( 1000 );

	dwEnd=MAXLONGLONG;

	//Finally start audio capture proper
	pCaptureGraphBuilder->ControlStream( &PIN_CATEGORY_CAPTURE,
		&MEDIATYPE_Audio, pAudioCaptureFilter,
		&dwStart, &dwEnd, 0, 0 );
	return true;

}

extern BOOL EndAudioRecording(){
	OutputDebugString( L"Stopping the capture" );
	pMediaSeeking->GetCurrentPosition( &dwEnd );
	//	pCaptureGraphBuilder->ControlStream( &PIN_CATEGORY_CAPTURE,
	//										 &MEDIATYPE_Video, pVideoCap,
	//										 &dwStart, &dwEnd, 1, 2 );
	pCaptureGraphBuilder->ControlStream( &PIN_CATEGORY_CAPTURE,
		&MEDIATYPE_Audio, pAudioCaptureFilter,
		&dwStart, &dwEnd, 1, 2 );

	// Wait for the ControlStream event. 
	// Since data isn't being encoded ATM this doesn't need to be run through, instead cut
	// the audio off at throat and let the blood spill.
	//do
	//{
	//	pMediaEvent->GetEvent( &lEventCode, &lParam1, &lParam2, INFINITE );
	//	pMediaEvent->FreeEventParams( lEventCode, lParam1, lParam2 );

	//	if( lEventCode == EC_STREAM_CONTROL_STOPPED ) {
	//		OutputDebugString( L"Received a control stream stop event" );
	//		count++;
	//	}
	//} while( count < 1);
	
	return TRUE;
}

extern BOOL PowerOffDisplay(){
	//Why does turning off work through wrapper while turning on does not??
	HDC gdc;
	int iESC=SETPOWERMANAGEMENT;

	gdc = ::GetDC(NULL);

	if (ExtEscape(gdc, QUERYESCSUPPORT, sizeof(int), (LPCSTR)&iESC, 
		0, NULL)==0)     {
			gdc = ::GetDC(NULL);
	}
	VIDEO_POWER_MANAGEMENT vpm;
	vpm.Length = sizeof(VIDEO_POWER_MANAGEMENT);
	vpm.DPMSVersion = 0x0001;
	vpm.PowerState = VideoPowerOff;
	// Power off the display
	ExtEscape(gdc, SETPOWERMANAGEMENT, vpm.Length, (LPCSTR) &vpm, 
		0, NULL);
	return TRUE;
}

extern BOOL PowerOnDisplay(){
	HDC gdc;
	int iESC=SETPOWERMANAGEMENT;

	gdc = ::GetDC(NULL);

	if (ExtEscape(gdc, QUERYESCSUPPORT, sizeof(int), (LPCSTR)&iESC, 
		0, NULL)==0)     {
			gdc = ::GetDC(NULL);
	}
	VIDEO_POWER_MANAGEMENT vpm;
	vpm.Length = sizeof(VIDEO_POWER_MANAGEMENT);
	vpm.DPMSVersion = 0x0001;
	vpm.PowerState = VideoPowerOn;
	ExtEscape(gdc, SETPOWERMANAGEMENT, vpm.Length, (LPCSTR) &vpm, 
		0, NULL);	
	return TRUE;
}

