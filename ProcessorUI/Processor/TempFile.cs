using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QutSensors.Processor
{
	public class TempFile : IDisposable
	{
		public TempFile(string extension)
		{
			string temp = Utilities.GetTempFileName();
			FileName = temp + extension;
		}

		public TempFile(byte[] data, string extension)
		{
			string temp = Utilities.GetTempFileName();
			FileName = temp + extension;
			File.WriteAllBytes(FileName, data);
		}

		public string FileName { get; protected set; }

		const int BufferSize = 100 * 1024;
		public void CopyStream(Stream source)
		{
			using (var target = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
			{
				var buffer = new byte[BufferSize];
				int read;
				do
				{
					read = (int)source.Read(buffer, 0, BufferSize);
					if (read > 0)
						target.Write(buffer, 0, read);
				} while (read > 0);
			}
		}

		#region IDisposable Members
		~TempFile()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			File.Delete(FileName);
		}
		#endregion
	}
}