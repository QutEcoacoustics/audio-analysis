Imports System.ServiceProcess
Imports QutSensors

Public Class DatabaseSync
	WithEvents timer As New System.Timers.Timer

	Protected Overrides Sub OnStart(ByVal args() As String)
		timer.Interval = 10000 '1000 = 1sec
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
			For Each reading As AudioReading In _sensor.GetAudioReadingsNotUploaded()
				Try
					service.AddAudioReading(_sensor.ID.ToString(), reading.ID.ToString(), reading.Time, reading.GetData())
					reading.MarkAsUploaded()
				Catch
				End Try
			Next

			For Each reading As PhotoReading In _sensor.GetPhotoReadingsNotUploaded()
				Try
					service.AddPhotoReading(_sensor.ID.ToString(), reading.ID.ToString(), reading.Time, reading.GetData())
					reading.MarkAsUploaded()
				Catch
				End Try
			Next
		Next
	End Sub
End Class
