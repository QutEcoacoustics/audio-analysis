using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BridgeTest
{
	class WMInterfaces
	{
		public enum WMT_TRANSPORT_TYPE
		{
			WMT_Transport_Type_Unreliable,
			WMT_Transport_Type_Reliable
		};

		public enum WMT_VERSION
		{
			WMT_VER_4_0 = 0x00040000,
			WMT_VER_7_0 = 0x00070000,
			WMT_VER_8_0 = 0x00080000,
			WMT_VER_9_0 = 0x00090000,
		};

		[ComImport]
		[Guid("96406BDB-2B2B-11d3-B36B-00C04F6108FF")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMProfile
		{
			void GetVersion([Out] out WMT_VERSION pdwVersion);
			void GetName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
						 [In, Out] ref uint pcchName);
			void SetName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszName);
			void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszDescription,
								[In, Out] ref uint pcchDescription);
			void SetDescription([In, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription);
			void GetStreamCount([Out] out uint pcStreams);
			void GetStream([In] uint dwStreamIndex, [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);
			void GetStreamByNumber([In] ushort wStreamNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);
			void RemoveStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);
			void RemoveStreamByNumber([In] ushort wStreamNum);
			void AddStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);
			void ReconfigStream([In, MarshalAs(UnmanagedType.Interface)] IWMStreamConfig pConfig);
			void CreateNewStream([In] ref Guid guidStreamType,
								 [Out, MarshalAs(UnmanagedType.Interface)] out IWMStreamConfig ppConfig);
			void GetMutualExclusionCount([Out] out uint pcME);
			void GetMutualExclusion([In] uint dwMEIndex,
									[Out, MarshalAs(UnmanagedType.Interface)] out IWMMutualExclusion ppME);
			void RemoveMutualExclusion([In, MarshalAs(UnmanagedType.Interface)] IWMMutualExclusion pME);
			void AddMutualExclusion([In, MarshalAs(UnmanagedType.Interface)] IWMMutualExclusion pME);
			void CreateNewMutualExclusion([Out, MarshalAs(UnmanagedType.Interface)] out IWMMutualExclusion ppME);
		}

		[ComImport]
		[Guid("96406BDE-2B2B-11d3-B36B-00C04F6108FF")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMMutualExclusion : IWMStreamList
		{
			//IWMStreamList
			new void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
			 [In, Out] ref ushort pcStreams);
			new void AddStream([In] ushort wStreamNum);
			new void RemoveStream([In] ushort wStreamNum);
			//IWMMutualExclusion
			void GetType([Out] out Guid pguidType);

			void SetType([In] ref Guid guidType);
		};

		[ComImport]
		[Guid("96406BDD-2B2B-11d3-B36B-00C04F6108FF")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMStreamList
		{

			void GetStreams([Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
							[In, Out] ref ushort pcStreams);

			void AddStream([In] ushort wStreamNum);

			void RemoveStream([In] ushort wStreamNum);
		};

		[ComImport]
		[Guid("d16679f2-6ca0-472d-8d31-2f5d55aee155")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMProfileManager
		{
			void CreateEmptyProfile([In] WMT_VERSION dwVersion,
									[Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);
			void LoadProfileByID([In] ref Guid guidProfile,
								 [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);
			void LoadProfileByData([In, MarshalAs(UnmanagedType.LPWStr)] string pwszProfile,
								   [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);
			void SaveProfile([In, MarshalAs(UnmanagedType.Interface)] IWMProfile pIWMProfile,
							 [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszProfile,
							 [In, Out] ref uint pdwLength);
			void GetSystemProfileCount([Out] out uint pcProfiles);
			void LoadSystemProfile([In] uint dwProfileIndex,
								   [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfile ppProfile);
		}

		[ComImport]
		[Guid("96406BDC-2B2B-11d3-B36B-00C04F6108FF")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMStreamConfig
		{
			void GetStreamType([Out] out Guid pguidStreamType);
			void GetStreamNumber([Out] out ushort pwStreamNum);
			void SetStreamNumber([In] ushort wStreamNum);
			void GetStreamName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszStreamName,
							   [In, Out] ref ushort pcchStreamName);
			void SetStreamName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszStreamName);
			void GetConnectionName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszInputName,
								   [In, Out] ref ushort pcchInputName);
			void SetConnectionName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszInputName);
			void GetBitrate([Out] out uint pdwBitrate);
			void SetBitrate([In] uint pdwBitrate);
			void GetBufferWindow([Out] out uint pmsBufferWindow);
			void SetBufferWindow([In] uint msBufferWindow);
		};

		[ComImport]
		[Guid("7688D8CB-FC0D-43BD-9459-5A8DEC200CFA")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMStreamConfig2 : IWMStreamConfig
		{
			//IWMStreamConfig
			new void GetStreamType([Out] out Guid pguidStreamType);
			new void GetStreamNumber([Out] out ushort pwStreamNum);
			new void SetStreamNumber([In] ushort wStreamNum);
			new void GetStreamName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszStreamName,
			 [In, Out] ref ushort pcchStreamName);
			new void SetStreamName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszStreamName);
			new void GetConnectionName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszInputName,
			 [In, Out] ref ushort pcchInputName);
			new void SetConnectionName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszInputName);
			new void GetBitrate([Out] out uint pdwBitrate);
			new void SetBitrate([In] uint pdwBitrate);
			new void GetBufferWindow([Out] out uint pmsBufferWindow);
			new void SetBufferWindow([In] uint msBufferWindow);
			//IWMStreamConfig2
			void GetTransportType([Out] out WMT_TRANSPORT_TYPE pnTransportType);
			void SetTransportType([In] WMT_TRANSPORT_TYPE nTransportType);
			void AddDataUnitExtension([In] Guid guidExtensionSystemID,
									  [In] ushort cbExtensionDataSize,
									  [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbExtensionSystemInfo,
									  [In] uint cbExtensionSystemInfo);

			void GetDataUnitExtensionCount([Out] out ushort pcDataUnitExtensions);
			void GetDataUnitExtension([In] uint wDataUnitExtensionNumber,
									  [Out] out Guid pguidExtensionSystemID,
									  [Out] out ushort pcbExtensionDataSize,
				/*[out, size_is( *pcbExtensionSystemInfo )]*/ IntPtr pbExtensionSystemInfo,
									  [In, Out] ref uint pcbExtensionSystemInfo);
			void RemoveAllDataUnitExtensions();
		}

		[ComImport]
		[Guid("CB164104-3AA9-45a7-9AC9-4DAEE131D6E1")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IWMStreamConfig3 : IWMStreamConfig2
		{
			//IWMStreamConfig
			new void GetStreamType([Out] out Guid pguidStreamType);
			new void GetStreamNumber([Out] out ushort pwStreamNum);
			new void SetStreamNumber([In] ushort wStreamNum);
			new void GetStreamName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszStreamName,
			 [In, Out] ref ushort pcchStreamName);
			new void SetStreamName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszStreamName);
			new void GetConnectionName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszInputName,
			 [In, Out] ref ushort pcchInputName);
			new void SetConnectionName([In, MarshalAs(UnmanagedType.LPWStr)] string pwszInputName);
			new void GetBitrate([Out] out uint pdwBitrate);
			new void SetBitrate([In] uint pdwBitrate);
			new void GetBufferWindow([Out] out uint pmsBufferWindow);
			new void SetBufferWindow([In] uint msBufferWindow);
			//IWMStreamConfig2
			new void GetTransportType([Out] out WMT_TRANSPORT_TYPE pnTransportType);
			new void SetTransportType([In] WMT_TRANSPORT_TYPE nTransportType);
			new void AddDataUnitExtension([In] Guid guidExtensionSystemID,
			  [In] ushort cbExtensionDataSize,
			  [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbExtensionSystemInfo,
			  [In] uint cbExtensionSystemInfo);

			new void GetDataUnitExtensionCount([Out] out ushort pcDataUnitExtensions);
			new void GetDataUnitExtension([In] uint wDataUnitExtensionNumber,
			 [Out] out Guid pguidExtensionSystemID,
			 [Out] out ushort pcbExtensionDataSize,
				/*[out, size_is( *pcbExtensionSystemInfo )]*/ IntPtr pbExtensionSystemInfo,
			 [In, Out] ref uint pcbExtensionSystemInfo);
			new void RemoveAllDataUnitExtensions();
			//IWMStreamConfig3
			void GetLanguage([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszLanguageString,
							 [In, Out] ref ushort pcchLanguageStringLength);
			void SetLanguage([In, MarshalAs(UnmanagedType.LPWStr)] string pwszLanguageString);
		}
	}
}