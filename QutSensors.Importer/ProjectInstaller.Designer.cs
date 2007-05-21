namespace QutSensors.Importer
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.importerInstaller = new System.ServiceProcess.ServiceInstaller();
			this.spectrumInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller
			// 
			this.serviceProcessInstaller.Password = null;
			this.serviceProcessInstaller.Username = null;
			// 
			// importerInstaller
			// 
			this.importerInstaller.Description = "Imports data from the file system into a QUT Sensors database.";
			this.importerInstaller.DisplayName = "QUT Sensors Data Importer";
			this.importerInstaller.ServiceName = "SensorsImporter";
			// 
			// spectrumInstaller
			// 
			this.spectrumInstaller.Description = "Generates visualisation data from a QUT Sensors database";
			this.spectrumInstaller.DisplayName = "QUT Sensors Visualisation Generator";
			this.spectrumInstaller.ServiceName = "SensorsVisualisationGenerator";
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.importerInstaller,
            this.spectrumInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller importerInstaller;
		private System.ServiceProcess.ServiceInstaller spectrumInstaller;
	}
}