using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Security.Permissions;

namespace AudioTools.DirectShow
{
	public static class DSUtilities
	{
		static readonly Guid BridgeSinkGuid = new Guid("{380C8E3F-8ADF-4a2c-9451-3EB19ED0EB90}");
		public static IBridgeSink CreateBridgeSink(IFilterGraph2 graphBuilder)
		{
			return (IBridgeSink)AddFilterFromClsid(graphBuilder, BridgeSinkGuid, "Bridge Sink");
		}

		static readonly Guid SqlSourceGuid = new Guid("{0C14D74A-64DD-4b7d-A1E9-B54FDDB54C54}");
		public static IBaseFilter CreateSqlSource(IGraphBuilder graphBuilder)
		{
			return AddFilterFromClsid(graphBuilder, SqlSourceGuid, "SQL Source");
		}

		static readonly Guid WavpackDecoderGuid = new Guid("{4B420C26-B393-48B3-8A84-BC60827689E8}");
		public static IBaseFilter CreateWavpackDecoder()
		{
			return Activator.CreateInstance(Type.GetTypeFromCLSID(WavpackDecoderGuid)) as IBaseFilter;
		}

		static readonly Guid WavpackSplitterGuid = new Guid("{D8CF6A42-3E09-4922-A452-21DFF10BEEBA}");
		public static IBaseFilter CreateWavpackSplitter()
		{
			return Activator.CreateInstance(Type.GetTypeFromCLSID(WavpackSplitterGuid)) as IBaseFilter;
		}

		static readonly Guid Mp3DecoderGuid = new Guid("{38BE3000-DBF4-11D0-860E-00A024CFEF6D}");
		static readonly Guid Mp3DecoderDMO = new Guid("BBEEA841-0A63-4F52-A7AB-A9B3A84ED38A");
		static readonly Guid DmoCategoryForMp3Decoder = new Guid("{57F2DB8B-E6BB-4513-9D43-DCD2A6593125}");

		public static IBaseFilter CreateMp3Decoder()
		{
			IBaseFilter retVal = null;
			// Try for the directshow filter (pre-vista)
			try
			{
				retVal = Activator.CreateInstance(Type.GetTypeFromCLSID(Mp3DecoderGuid)) as IBaseFilter;
			}
			catch { }

			if (retVal == null)
			{
				// Assume the DMO version is available instead.
				retVal = (IBaseFilter)new DMOWrapperFilter();
				IDMOWrapperFilter dmoWrapper = (IDMOWrapperFilter)retVal;
				int hr = dmoWrapper.Init(Mp3DecoderDMO, DmoCategoryForMp3Decoder);
				if (hr != 0)
				{
					Marshal.FinalReleaseComObject(retVal);
					return null;
				}
			}
			return retVal;
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		static IBaseFilter AddFilterFromClsid(IGraphBuilder graphBuilder, Guid clsid, string name)
		{
			int hr = 0;
			IBaseFilter filter = null;

			if (graphBuilder == null)
			{
				throw new ArgumentNullException("graphBuilder");
			}

			try
			{
				Type type = Type.GetTypeFromCLSID(clsid);
				var j = Activator.CreateInstance(type);
				filter = (IBaseFilter)j;

				hr = graphBuilder.AddFilter(filter, name);
				DsError.ThrowExceptionForHR(hr);
			}
			catch
			{
				if (filter != null)
				{
					Marshal.ReleaseComObject(filter);
					filter = null;
				}
			}

			return filter;
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void RemoveAllFilters(IGraphBuilder graphBuilder)
		{
			int hr = 0;
			IEnumFilters enumFilters;
			List<IBaseFilter> filtersArray = new List<IBaseFilter>();

			if (graphBuilder == null)
				throw new ArgumentNullException("graphBuilder");

			hr = graphBuilder.EnumFilters(out enumFilters);
			DsError.ThrowExceptionForHR(hr);

			try
			{
				IBaseFilter[] filters = new IBaseFilter[1];
				IntPtr fetched = IntPtr.Zero;

				while (enumFilters.Next(filters.Length, filters, fetched) == 0)
					filtersArray.Add(filters[0]);
			}
			catch
			{
				return;
			}
			finally
			{
				Marshal.ReleaseComObject(enumFilters);
			}

			foreach (IBaseFilter filter in filtersArray)
			{
				hr = graphBuilder.RemoveFilter(filter);
				Marshal.ReleaseComObject(filter);
			}
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void ReceiveData(int size, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]byte[] data);

	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1AB856B5-CF52-4979-85EF-2228CB767ADD")]
	public interface IBridgeSink
	{
		//STDMETHOD(set_Target) (THIS_ IBridgeTarget *target) PURE;
		void set_Target([MarshalAs(UnmanagedType.FunctionPtr)]ReceiveData target);
	}
}
