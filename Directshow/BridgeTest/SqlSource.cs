using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BridgeTest
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("82339922-9B28-4303-A95D-FE26384EB345")]
	public interface ISqlFileSource
	{
		int set_TransactionContext([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] context, int length);
	}
}