using System;
using System.Collections.Generic;
using System.Linq;
using QutSensors.Data;
using QutSensors.Data.Linq;

namespace QutSensors.Processor
{
	public class Job
	{
		internal Job(string owner, ReadingsFilter filter, Type classifier, string parameters)
		{
			Owner = owner;
			Filter = filter;
			Classifier = classifier;
			Parameters = parameters;

			using (var db = new QutSensors.Data.Linq.QutSensors())
			{
				var job = new Processor_Jobs() { Filter = Filter.SaveAsString(), Owner = Owner, Classifier = Classifier.FullName, Parameters = Parameters };
				db.Processor_Jobs.InsertOnSubmit(job);
				db.SubmitChanges();
				JobID = JobID = job.JobID;
			}
		}

		internal Job(Processor_Jobs job)
		{
			JobID = job.JobID;
			Owner = job.Owner;
			Filter = ReadingsFilter.Load(job.Filter);
			Classifier = Type.GetType(job.Classifier);
			Parameters = job.Parameters;
		}

		public int JobID { get; private set; }
		public string Owner { get; set; }
		public string Parameters { get; set; }
		public ReadingsFilter Filter { get; set; }
		public Type Classifier { get; set; }

		public IEnumerable<JobItem> GetIncompleteItems()
		{
			using (var db = new QutSensors.Data.Linq.QutSensors())
				return db.Processor_JobItems.Where(j => j.JobID == JobID).Select(j => new JobItem(this, j));
		}

		public JobItem AddItem(AudioReadings reading, TimeSpan startTime, TimeSpan stopTime)
		{
			return new JobItem(this, startTime, stopTime, reading.AudioReadingID);
		}
	}
}