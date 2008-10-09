// Media Segment Extractor Filter based on source by Kovalev Maxim - http://www.codeproject.com/KB/audio-video/DirectShowMediaTrim.aspx
// Used with permission. Original code licensed under Code Project Open License (CPOL) 1.02 - http://www.codeproject.com/info/cpol10.aspx
#include "MediaSegmentExtractor.h"

// Setup data - allows the self-registration to work
const AMOVIESETUP_MEDIATYPE sudPinTypes =
	{ &MEDIATYPE_NULL // clsMajorType
	, &MEDIASUBTYPE_NULL // clsMinorType
	};

const AMOVIESETUP_PIN psudPins [] = {
	{ L"Input" // strName
	  , FALSE // bRendered
	  , FALSE // bOutput
	  , FALSE // bZero
	  , FALSE // bMany
	  , &CLSID_NULL // clsConnectsToFilter
	  , L"" // strConnectsToPin
	  , 1 // nTypes
	  , &sudPinTypes // lpTypes
	}
	,
	{ L"Output" // strName
	, FALSE // bRendered
	, TRUE // bOutput
	, FALSE // bZero
	, FALSE // bMany
	, &CLSID_NULL // clsConnectsToFilter
	, L"" // strConnectsToPin
	, 1 // nTypes
	, &sudPinTypes // lpTypes
	}
	};

const AMOVIESETUP_FILTER sudTransformSample =
	{ &CLSID_MediaSegmentExtractor // clsID
	, L"Media Segment Extractor Filter" // strName
	, MERIT_DO_NOT_USE // dwMerit
	, 2 // nPins
	, psudPins // lpPin
	};

// Needed for the CreateInstance mechanism
CFactoryTemplate g_Templates [] = {
	{
		L"Media Segment Extractor Filter",
		&CLSID_MediaSegmentExtractor,
		CMediaSegmentExtractor::CreateInstance,
		NULL,
		&sudTransformSample
	}
	};

int g_cTemplates = sizeof (g_Templates) / sizeof (g_Templates [0]);

// Exported entry points for registration and unregistration
STDAPI DllRegisterServer()
{
	return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2(FALSE);
}

//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, DWORD  dwReason, LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}