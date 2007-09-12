using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenNETCF.WindowsCE.Notification;
using System.Reflection;

namespace QUT
{
	public class Utilities
	{
		const int MaxLogFiles = 5;
		const int MaxFileSize = 10 * 1024; // 10K
		const string LogFilePattern = "{0}LogFile_{1}.txt";

		public static string GetLogPath(int index)
		{
			return string.Format(LogFilePattern, Settings.LogPath, index);
		}

		static StreamWriter OpenLogFile()
		{
			return new StreamWriter(GetLogPath(GetLogIndex()), true);
		}

		private static int GetLogIndex()
		{
			int index = 0;
			while (File.Exists(GetLogPath(index)))
			{
				if (new FileInfo(GetLogPath(index)).Length < MaxFileSize)
					break;
				index++;
			}

			if (index >= MaxLogFiles)
			{
				index = 0;
				DateTime fileTime = File.GetLastWriteTime(GetLogPath(0));
				for (int i = 1; i < MaxLogFiles; i++)
				{
					DateTime newFileTime = File.GetLastWriteTime(GetLogPath(i));
					if (newFileTime < fileTime)
					{
						index = i;
						File.Delete(GetLogPath(index));
						break;
					}
				}
			}
			return index;
		}

		public static void Log(string format, params object[] args)
		{
			try
			{
				using (StreamWriter writer = OpenLogFile())
				{
					writer.Write(DateTime.Now.ToString("dd/MM HH:mm:ss"));
					writer.Write(": ");
					writer.WriteLine(format, args);
				}
			}
			catch { } // Faulty logging should not affect the rest of the app.
		}

		public static void Log(Exception e, string position)
		{
			try
			{
				using (StreamWriter writer = OpenLogFile())
				{
					writer.Write(DateTime.Now.ToString("dd/MM HH:mm:ss"));
					writer.Write(": ");
					writer.Write("Error at {0} - ", position);
					writer.WriteLine(e);
				}
			}
			catch { } // Faulty logging should not affect the rest of the app.
		}

		public static string LoadLog()
		{
			string path = GetLogPath(GetLogIndex());
			if (File.Exists(path))
				using (StreamReader reader = new StreamReader(path))
					return reader.ReadToEnd();
			else
				return null;
		}

		public static void QueueNextAppRun(DateTime time)
		{
			QueueNextAppRun(time, Assembly.GetExecutingAssembly().GetName().CodeBase);
		}

		public static void QueueNextAppRunForRecorder(DateTime time)
		{
			QueueNextAppRun(time, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "QUTSensors\\CFRecorder.exe"));
		}

		public static void QueueNextAppRun(DateTime time, string path)
		{
			Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, time);
			Log("Queue @ {0:dd/MM HH:mm:ss}", time);
		}
	}
}