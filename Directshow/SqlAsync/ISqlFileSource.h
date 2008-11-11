#ifndef __ISQLFILESOURCE__
#define __ISQLFILESOURCE__

#include "initguid.h"

//
// ISqlFileSource's GUID
//
// {82339922-9B28-4303-A95D-FE26384EB345}
DEFINE_GUID(IID_ISqlFileSource, 
	0x82339922, 0x9b28, 0x4303, 0xa9, 0x5d, 0xfe, 0x26, 0x38, 0x4e, 0xb3, 0x45);

//
// ISqlFileSource
// This defines the interface for passing the transaction context for SQL filestream access.
//
DECLARE_INTERFACE_(ISqlFileSource, IUnknown) {
	STDMETHOD(set_TransactionContext) (THIS_ PBYTE context, LONG length) PURE;
};

#endif // __ISQLFILESOURCE__