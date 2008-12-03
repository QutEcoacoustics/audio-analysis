using System;
using System.Collections.Generic;
using System.Linq;
using QutSensors.Data;

namespace QutSensors.Processor
{
	public static class JobManager
	{
		public static Job Add(ReadingsFilter filter, string owner, Type classifier, string parameters)
		{
			return new Job(owner, filter, classifier, parameters);
		}

		public static IEnumerable<Job> GetIncompleteJobs()
		{
			using (var db = new QutSensors.Data.Linq.QutSensors())
				return db.Processor_Jobs.Select(j => new Job(j));
		}

		public static IEnumerable<Job> GetOwnersJobs(string owner)
		{
			using (var db = new QutSensors.Data.Linq.QutSensors())
				return db.Processor_Jobs.Where(j => j.Owner == owner).Select(j => new Job(j));
		}
	}
}