using RikMigrations;

namespace QutSensors.Processor.Migrations
{
	class ResultsInitialiser : IMigration
	{
		#region IMigration Members
		public void Up(DbProvider db)
		{
			var t = db.AddTable("Processor_Results");
			t.AddColumn<int>("JobItemID").NotNull().PrimaryKey();
			t.AddColumn<int>("PeriodicHits").NotNull();
			t.AddColumn<double>("BestHitScore").NotNull();
			t.AddColumn<int>("BestHitLocation").NotNull();
			t.Save();
		}
	
		public void Down(DbProvider db)
		{
			db.DropTable("Processor_Results");
		}
		#endregion
	}
}