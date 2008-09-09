#ifndef __IBRIDGETARGET__
#define __IBRIDGETARGET__

#ifdef __cplusplus
extern "C" {
#endif

//
// IBridgeTarget's GUID
//
// {BEAED04C-746A-42fe-A2B5-B712E587D2FB}
DEFINE_GUID(IID_IBridgeTarget, 
0xbeaed04c, 0x746a, 0x42fe, 0xa2, 0xb5, 0xb7, 0x12, 0xe5, 0x87, 0xd2, 0xfb);

//
// IBridgeTarget
// This defines the interface which will receive data from the bridge.
//
DECLARE_INTERFACE_(IBridgeTarget, IUnknown) {
	STDMETHOD(Test) (THIS_) PURE;
	//STDMETHOD(Receive) (THIS_ LONG dataLength, PBYTE data) PURE;
	STDMETHOD(Receive) (THIS_ LONG dataLength) PURE;
};


#ifdef __cplusplus
}
#endif

#endif // __IBRIDGETARGET__