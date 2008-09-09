#ifndef __IBRIDGESINK__
#define __IBRIDGESINK__

#ifdef __cplusplus
extern "C" {
#endif

//
// IBridgeSink's GUID
//
// {1AB856B5-CF52-4979-85EF-2228CB767ADD}
DEFINE_GUID(IID_IBridgeSink, 
0x1ab856b5, 0xcf52, 0x4979, 0x85, 0xef, 0x22, 0x28, 0xcb, 0x76, 0x7a, 0xdd);

typedef void (_stdcall *ReceiveData)(LONG size, BYTE *data);

//
// IBridgeSink
// This defines the interface for registering a bridge target.
//
DECLARE_INTERFACE_(IBridgeSink, IUnknown) {
	//STDMETHOD(set_Target) (THIS_ IBridgeTarget *target) PURE;
	STDMETHOD(set_Target) (THIS_ ReceiveData target) PURE;
};


#ifdef __cplusplus
}
#endif

#endif // __IBRIDGESINK__