using System;
using System.Collections.Generic;
using System.Text;
using CFRecorder;

namespace QUT
{
	public class DeviceManager
	{
		// Indicates how many minutes a record
		const int NearbyRecordingMinutes = 5;

		public static void Start()
		{
			Utilities.Log("Device manager started");

			// Gather current state - currently time, in future could take into account battery state or other parameters.
			DateTime nextRecordingEnd;
			DateTime nextRecording = CalculateNextRecordingTime(out nextRecordingEnd);

			Utilities.Log("Next recording planned for: {0}", nextRecording);

			// Take recording if appropriate time to do so.
			bool recordingTaken = false;
			if (nextRecording < DateTime.Now.AddMinutes(NearbyRecordingMinutes))
			{
				Utilities.Log("Taking recording: {0:dd/MM HH:mm:ss}", nextRecording);
				RecordAndUpload(ref nextRecordingEnd, ref nextRecording);
				recordingTaken = true;
			}

			UploadOldRecordings(ref recordingTaken);

			nextRecording = CalculateNextRecordingTime(out nextRecordingEnd);
			Utilities.QueueNextAppRun(nextRecording.AddSeconds(-30));

			if (recordingTaken)
				PDA.Hardware.SoftReset();
		}

		// Upload extra recordings - check between each upload if it's time to record again.
		private static void UploadOldRecordings(ref bool recordingTaken)
		{
			bool uploadedOldRecording;
			do
			{
				DateTime nextRecordingEnd;
				DateTime nextRecording = CalculateNextRecordingTime(out nextRecordingEnd);
				if (nextRecording < DateTime.Now.AddMinutes(NearbyRecordingMinutes))
				{
					Utilities.Log("Queuing another recording immediately. No time for a reset: {0:dd/MM HH:mm:ss}", nextRecording);
					if (DateTime.Now < nextRecording.AddSeconds(-31))
						Utilities.QueueNextAppRun(nextRecording.AddSeconds(-30));
					RecordAndUpload(ref nextRecordingEnd, ref nextRecording);
					recordingTaken = true;
				}

				uploadedOldRecording = DataUploader.ProcessFailures(1) > 0;
			} while (uploadedOldRecording);
		}

		private static void RecordAndUpload(ref DateTime nextRecordingEnd, ref DateTime nextRecording)
		{
			Recording recording = TakeRecording(nextRecording, nextRecordingEnd);
			if (recording != null)
			{
				Settings.LastRecordingTime = nextRecording;
				DataUploader.Upload(recording);
			}
		}

		private static DateTime CalculateNextRecordingTime(out DateTime endTime)
		{
			DateTime? retVal = Settings.LastRecordingTime;
			if (retVal == null)
				retVal = DateTime.Now.AddSeconds(30); // Just choose sometime soon to base our start position on. This should only happen once per device
			else
			{
				retVal = retVal.Value.AddMilliseconds(Settings.ReadingFrequency);

				while (retVal.Value < DateTime.Now.AddMilliseconds(-1 * Settings.ReadingDuration))
				{
					Utilities.Log("Missed recording: {0}", retVal.Value);
					retVal = retVal.Value.AddMilliseconds(Settings.ReadingFrequency);
				}
			}

			endTime = retVal.Value.AddMilliseconds(Settings.ReadingDuration);
			return retVal.Value;
		}

		public static Recording TakeRecording(DateTime startTime, DateTime stopTime)
		{
			try
			{
				Recording recording = new Recording();
				recording.TakeRecording(startTime, stopTime);
				return recording;
			}
			catch (InvalidOperationException) // Likely because we started too late
			{
				return null;
			}
		}
	}
}