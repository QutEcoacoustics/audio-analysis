using RikMigrations;

namespace QutSensors.Processor.Migrations
{
	class JobsInitialiser : IMigration
	{
		#region IMigration Members
		public void Up(DbProvider db)
		{
			var t = db.AddTable("Processor_Jobs");
			t.AddColumn<int>("JobID").AutoGenerate().PrimaryKey();
			t.AddColumn<string>("Owner", 256).NotNull().References("aspnet_Users", "UserName");
			t.AddColumn<string>("Filter", int.MaxValue).NotNull();
			t.AddColumn<string>("Classifier", 256).NotNull();
			t.AddColumn<string>("Parameters", int.MaxValue).NotNull();
			t.Save();
		}

		public void Down(DbProvider db)
		{
			db.DropTable("Jobs");
		}
		#endregion
	}
}