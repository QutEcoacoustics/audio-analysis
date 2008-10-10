﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.Threading;
using System.IO;

namespace Test
{
	class Program
	{
		static ManualResetEvent completed = new ManualResetEvent(false);
		static FileStream target;

		[MTAThread]
		static void Main(string[] args)
		{
			using (target = new FileStream(@"C:\Users\masonr\Desktop\Joe.mp3", FileMode.Create, FileAccess.Write))
			using (var stream = AudioTools.DShowConverter.ToMp3(@"C:\Users\masonr\Desktop\BAC10_20081009-045000.wv", MimeTypes.WavpackMimeType, 
			//using (var stream = AudioTools.DShowConverter.ToMp3(@"C:\Users\masonr\Desktop\Top_Knoll_-_St_Bees_20081007-163000.wv", MimeTypes.WavpackMimeType, 
				500000, 600000))
			{
				stream.ReceivedData += new AudioTools.DirectShow.ReceiveData(stream_ReceivedData);
				stream.WaitForCompletion();
				Console.Write(count); count = 0;
				target.Dispose();
			}

			return;
			Console.ReadLine();
			using (target = new FileStream(@"C:\Users\masonr\Desktop\Joe2.mp3", FileMode.Create, FileAccess.Write))
			using (var stream = AudioTools.DShowConverter.ToMp3(@"C:\Users\masonr\Desktop\Top_Knoll_-_St_Bees_20081007-163000.wv", MimeTypes.WavpackMimeType,
				15000, 25000))
			{
				stream.ReceivedData += new AudioTools.DirectShow.ReceiveData(stream_ReceivedData);
				stream.WaitForCompletion();
				Console.Write(count);
				target.Dispose();
			}

			GC.Collect();
			Console.WriteLine("Done");
			Console.ReadLine();
		}

		static int count;
		static void stream_ReceivedData(int size, byte[] data)
		{
			count++;
			if (size == -1)
			{
				Console.Write("J");
				completed.Set();
			}
			else
				target.Write(data, 0, size);
		}
	}
}