#pragma once
#include "resourceppc.h"

//-----

#define ChangeRadioState_ORDINAL				273
#define GetWirelessDevice_ORDINAL				276
#define FreeDeviceList_ORDINAL					280

// Types of radio device
typedef enum _RADIODEVTYPE
{
    RADIODEVICES_MANAGED = 1,
    RADIODEVICES_PHONE,
    RADIODEVICES_BLUETOOTH,
} RADIODEVTYPE;

// whether to save before or after changing state
typedef enum _SAVEACTION
{
    RADIODEVICES_DONT_SAVE = 0,
    RADIODEVICES_PRE_SAVE,
    RADIODEVICES_POST_SAVE,
} SAVEACTION;

// Details of radio devices
struct RDD 
{
    RDD() : pszDeviceName(NULL), pNext(NULL), pszDisplayName(NULL) {}
    ~RDD() { LocalFree(pszDeviceName); LocalFree(pszDisplayName); }
    LPTSTR   pszDeviceName;  // Device name for registry setting.
    LPTSTR   pszDisplayName; // Name to show the world
    DWORD    dwState;        // ON/off/[Discoverable for BT]
    DWORD    dwDesired;      // desired state - used for setting registry etc.
    RADIODEVTYPE    DeviceType;         // Managed, phone, BT etc.
    RDD * pNext;    // Next device in list
}; //radio device details

typedef LRESULT (CALLBACK* _GetWirelessDevices)(RDD **pDevices, DWORD dwFlags);
typedef LRESULT (CALLBACK* _ChangeRadioState)(RDD* pDev, DWORD dwState, SAVEACTION sa);
typedef LRESULT (CALLBACK* _FreeDeviceList)(RDD *pRoot);
