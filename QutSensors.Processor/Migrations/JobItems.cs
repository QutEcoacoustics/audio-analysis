using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RikMigrations;

namespace QutSensors.Processor.Migrations
{
	class JobItemsInitialiser : IMigration
	{
		#region IMigration Members
		public void Up(DbProvider db)
		{
			var t = db.AddTable("Processor_JobItems");
			t.AddColumn<int>("JobItemID").AutoGenerate().PrimaryKey();
			t.AddColumn<int>("JobID").NotNull().References("Processor_Jobs", "JobID");
			t.AddColumn<Guid>("AudioReadingID").NotNull().References("AudioReadings", "AudioReadingID");
			t.AddColumn<int>("StartTime");
			t.AddColumn<int>("StopTime");
			t.AddColumn<byte>("Status").NotNull().Default(0);
			t.Save();
		}

		public void Down(DbProvider db)
		{
			db.DropTable("Processor_JobItems");
		}
		#endregion
	}
}