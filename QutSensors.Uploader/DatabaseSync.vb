Imports System.ServiceProcess
Imports QutSensors

Public Class DatabaseSync
	WithEvents timer As New System.Timers.Timer
	Declare Function AllocConsole Lib "kernel32.dll" () As Boolean

	' The main entry point for the process (Normally in Designer.vb but then you can't edit it...)
	<MTAThread()> _
	<System.Diagnostics.DebuggerNonUserCode()> _
	Shared Sub Main(ByVal args() As String)
		If args.Length > 0 And args(0).ToLower() = "debug" Then
			DebugRun()
		Else
			Dim ServicesToRun() As System.ServiceProcess.ServiceBase
			ServicesToRun = New System.ServiceProcess.ServiceBase() {New DatabaseSync}

			System.ServiceProcess.ServiceBase.Run(ServicesToRun)
		End If
	End Sub

	Shared Sub DebugRun()
		Dim service As New DatabaseSync

		AllocConsole()

		service.DebugStart()

		Console.WriteLine("Running database sync service in debug mode." + Environment.NewLine + "Hit ENTER to stop.")
		Console.ReadLine()

		service.DebugStop()
	End Sub

	Sub DebugStart()
		OnStart(Nothing)
	End Sub

	Sub DebugStop()
		OnStop()
	End Sub

	Protected Overrides Sub OnStart(ByVal args() As String)
		timer.Interval = My.Settings.SyncRate '1000 = 1sec
		timer.Enabled = True
		timer.AutoReset = True
	End Sub

	Protected Overrides Sub OnStop()
	End Sub

	Private Sub timer_tick(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles timer.Elapsed
		SyncSensors()
	End Sub

	Private Sub SyncSensors()
		Dim service As New QutSensors.Services.Service

		For Each _sensor As Sensor In Sensor.GetAllSensors()
			Console.WriteLine("Synchronising sensor: {0} ({1})", _sensor.Name, _sensor.ID)
			For Each reading As AudioReading In _sensor.GetAudioReadingsNotUploaded()
				Try
					Console.Write("Synchronising audio reading @{0:g}...", reading.Time)
					service.AddAudioReading(_sensor.ID.ToString(), reading.ID.ToString(), reading.Time, reading.GetData())
					reading.MarkAsUploaded()
					Console.WriteLine("Success")
				Catch e As Exception
					Console.WriteLine("Failed - {0}", e)
				End Try
			Next

			For Each reading As PhotoReading In _sensor.GetPhotoReadingsNotUploaded()
				Try
					Console.Write("Synchronising image reading @{0:g}...", reading.Time)
					service.AddPhotoReading(_sensor.ID.ToString(), reading.ID.ToString(), reading.Time, reading.GetData())
					reading.MarkAsUploaded()
					Console.WriteLine("Success")
				Catch e As Exception
					Console.WriteLine("Failed - {0}", e)
				End Try
			Next
		Next
	End Sub
End Class
