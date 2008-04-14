#include "stdafx.h"
#include "Wininet.h"
#include "Wireless.h"
#include <windows.h>
#include <commctrl.h>
#include <service.h>

#pragma comment(lib, "Wininet.lib")

_GetWirelessDevices		pGetWirelessDevices = NULL;
_ChangeRadioState		pChangeRadioState = NULL;
_FreeDeviceList			pFreeDeviceList = NULL;

HINSTANCE	g_DllWrlspwr;

BOOL InitDLL()
{
	g_DllWrlspwr = LoadLibrary(TEXT("ossvcs.dll"));
	if (g_DllWrlspwr == NULL)
		return FALSE;
	pGetWirelessDevices   = (_GetWirelessDevices)GetProcAddress(g_DllWrlspwr,MAKEINTRESOURCE(GetWirelessDevice_ORDINAL));
	if (pGetWirelessDevices == NULL)
		return FALSE;
	
	pChangeRadioState   = (_ChangeRadioState)GetProcAddress(g_DllWrlspwr,MAKEINTRESOURCE(ChangeRadioState_ORDINAL));
	if (pChangeRadioState == NULL)
		return FALSE;
	
	pFreeDeviceList	   = (_FreeDeviceList)GetProcAddress(g_DllWrlspwr,MAKEINTRESOURCE(FreeDeviceList_ORDINAL));
	if (pFreeDeviceList == NULL)
		return FALSE;
	return TRUE;
}

//set the status of the desired wireless device
DWORD SetWDevState(DWORD dwDevice, DWORD dwState)
{
	RDD * pDevice = NULL;
    RDD * pTD;
    HRESULT hr;
	DWORD retval = 0;

//	InitDLL();
    hr = pGetWirelessDevices(&pDevice, 0);
	if(hr != S_OK) return -1;
    
    if (pDevice)
    {
        pTD = pDevice;

        // loop through the linked list of devices
        while (pTD)
        {
          if  (pTD->DeviceType == dwDevice)
          {
              hr = pChangeRadioState(pTD, dwState, RADIODEVICES_PRE_SAVE);
			  retval = 0;
          }
          
            pTD = pTD->pNext;
            
        }
        // Free the list of devices retrieved with    
        // GetWirelessDevices()
		pFreeDeviceList(pDevice);
    }

	if(hr == S_OK)return retval;
	
	return -2;
}

//get status of all wireless devices at once
DWORD GetWDevState(DWORD* bWifi, DWORD* bPhone, DWORD* bBT)
{
	RDD * pDevice = NULL;
    RDD * pTD;

    HRESULT hr;
	DWORD retval = 0;
	
    hr = pGetWirelessDevices(&pDevice, 0);

	if(hr != S_OK) return -1;
	
    if (pDevice)
    {
	    pTD = pDevice;

        // loop through the linked list of devices
		while (pTD)
		{
			switch (pTD->DeviceType)
			{
				case RADIODEVICES_MANAGED:
				*bWifi = pTD->dwState;
				break;
				case RADIODEVICES_PHONE:
				*bPhone = pTD->dwState;
				break;
				case RADIODEVICES_BLUETOOTH:
				*bBT = pTD->dwState;
				break;
				default:
				break;
			}
			pTD = pTD->pNext; 
	    }
        // Free the list of devices retrieved with    
        // GetWirelessDevices()
        pFreeDeviceList(pDevice);
    }

	if(hr == S_OK)return retval;
	
	return -2;
}



extern BOOL CleanupRadios(){
	return FreeLibrary(g_DllWrlspwr);
}

extern int RadioStates(){
	DWORD	dwWifi, dwPhone, dwBT;
	GetWDevState(&dwWifi, &dwPhone, &dwBT);

	return(dwWifi + (dwPhone*2) + (dwBT*4));
}

extern BOOL EnableRadio(int devices, bool turnOn){
	if (devices%2 == 1){
		SetWDevState( RADIODEVICES_MANAGED, turnOn);
		devices--;
	}
	if (devices%4 == 2){
		SetWDevState( RADIODEVICES_PHONE, turnOn);
		devices--;
		devices--;
	}
	if (devices%8 == 4){
		SetWDevState( RADIODEVICES_BLUETOOTH, turnOn);
	}	
	return true;
}

int _tmain(int argc, _TCHAR* argv[])
{
	// Load ossvcs.dll
	InitDLL();

	DWORD	dwWifi, dwPhone, dwBT;
	GetWDevState(&dwWifi, &dwPhone, &dwBT);

	//start bluetooth
	SetWDevState( RADIODEVICES_BLUETOOTH, 1);
	//start phone
	SetWDevState( RADIODEVICES_PHONE, 1);
	//start WIFI
	SetWDevState( RADIODEVICES_MANAGED, 1);

	// Free ossvcs.dll
	FreeLibrary(g_DllWrlspwr);

	return 0;
}
