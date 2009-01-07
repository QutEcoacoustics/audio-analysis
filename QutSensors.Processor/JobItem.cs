using System;
using System.Linq;
using QutSensors.Data.Linq;

namespace QutSensors.Processor
{
	public enum JobStatus
	{
		Ready,
		Running,
		Complete,
	}

	public class JobItem
	{
		internal JobItem(Job job, TimeSpan? startTime, TimeSpan? stopTime, Guid audioReadingID)
		{
			Job = job;
			StartTime = startTime;
			StopTime = stopTime;
			AudioReadingID = audioReadingID;

			using (var db = new QutSensors.Data.Linq.QutSensors())
			{
				var item = new Processor_JobItems() { JobID = job.JobID, AudioReadingID = AudioReadingID};
				if (startTime != null)
					item.StartTime = (int)startTime.Value.TotalMilliseconds;
				if (stopTime != null)
					item.StopTime = (int)stopTime.Value.TotalMilliseconds;
				db.Processor_JobItems.InsertOnSubmit(item);
				db.SubmitChanges();
				JobItemID = item.JobItemID;
			}
		}

		internal JobItem(Job job, Processor_JobItems item)
		{
			Job = job;
			JobItemID = item.JobItemID;
			if (item.StartTime != null)
				StartTime = TimeSpan.FromMilliseconds(item.StartTime.Value);
			if (item.StopTime != null)
				StopTime = TimeSpan.FromMilliseconds(item.StopTime.Value);
			Status = (JobStatus)item.Status;
			AudioReadingID = item.AudioReadingID;
		}

		#region Properties
		public Job Job { get; private set; }
		public int JobItemID { get; private set; }
		public TimeSpan? StartTime { get; private set; }
		public TimeSpan? StopTime { get; private set; }
		public JobStatus Status { get; private set; }
		public Guid AudioReadingID { get; private set; }
		#endregion

		public Result GetResult()
		{
			using (var db = new QutSensors.Data.Linq.QutSensors())
				return db.Processor_Results.Where(r => r.JobItemID == JobItemID).Select(r => new Result(r)).FirstOrDefault();
		}

		public void Run()
		{
			throw new System.NotImplementedException();
		}
	}
}