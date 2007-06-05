Imports System.Data
Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Collections.Generic
Imports QutSensors

<WebService(Namespace:="http://mquter.qut.edu.au/sensors/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Service
	Inherits System.Web.Services.WebService

	Private Sub EnsureSensorExists(ByVal sensorID As Guid)
		Dim mySensor As Sensor
		mySensor = Sensor.GetSensor(sensorID)
		If mySensor Is Nothing Then
			mySensor = New Sensor(sensorID)
			mySensor.Save()
		End If
	End Sub

	<WebMethod()> _
	Public Sub AddPhotoReading(ByVal sensorGuid As String, ByVal readingGuid As String, ByVal time As Date, ByVal buffer As Byte())
		Dim sensorID As New Guid(sensorGuid)
		EnsureSensorExists(sensorID)

		Dim reading As PhotoReading
		If readingGuid Is Nothing Then
			reading = New PhotoReading(sensorID)
		Else
			reading = New PhotoReading(sensorID, New Guid(readingGuid))
		End If

		reading.Time = time
		reading.Save()
		reading.UpdateData(buffer)
	End Sub

	<WebMethod()> _
	Public Sub AddAudioReading(ByVal sensorGuid As String, ByVal readingGuid As String, ByVal time As Date, ByVal buffer As Byte())
		Dim sensorID As New Guid(sensorGuid)
		EnsureSensorExists(sensorID)

		Dim reading As AudioReading
		If readingGuid Is Nothing Then
			reading = New AudioReading(sensorID)
		Else
			reading = New AudioReading(sensorID, New Guid(readingGuid))
		End If

		reading.Time = time
		reading.Save()
		reading.UpdateData(buffer)
	End Sub

	'<WebMethod()> _
	'Public Function FindPhotoReading(ByVal sensorID As Guid, ByVal time As Date) As Boolean
	'	' TODO: What do we need this function for?
	'	' TODO: Replace with dedicated SQL procedure
	'	Dim reading As PhotoReading
	'	For Each reading In Sensor.GetSensor(sensorID).GetPhotoReadings
	'		If reading.Time = time Then
	'			Return True
	'		End If
	'	Next
	'	Return False
	'End Function

	'<WebMethod()> _
	'Public Function FindAudioReading(ByVal sensorID As Guid, ByVal time As Date) As Boolean
	'	' TODO: What do we need this function for?
	'	' TODO: Replace with dedicated SQL procedure
	'	Dim reading As AudioReading
	'	For Each reading In Sensor.GetSensor(sensorID).GetAudioReadings
	'		If reading.Time = time Then
	'			Return True
	'		End If
	'	Next
	'	Return False
	'End Function

	'<WebMethod()> _
	'Public Function GetPhotoreading(ByVal sensorName As String, ByVal pDate As Date) As DataTable
	'	Dim adapter As New SqlDataAdapter("SELECT Sensors.SensorID, Sensors.Name, Time, Data FROM PhotoReadings,Sensors Where PhotoReadings.SensorID = Sensors.SensorID and Name='" & sensorName & "'", conn)
	'	Dim dt As New DataTable("PhotoReadings")
	'	adapter.Fill(dt)
	'	Return dt
	'End Function

	'<WebMethod()> _
	'Public Function GetAudioreading(ByVal sensorName As String, ByVal pDate As Date) As DataTable
	'	Dim adapter As New SqlDataAdapter("SELECT Sensors.SensorID, Sensors.Name, Time, Data FROM AudioReadings,Sensors Where AudioReadings.SensorID = Sensors.SensorID and Name='" & sensorName & "'", conn)
	'	Dim dt As New DataTable("AudioReadings")
	'	adapter.Fill(dt)
	'	Return dt
	'End Function

	<WebMethod()> _
	Public Sub AddSensor(ByVal sensorID As Guid, ByVal name As String)
		Dim sensor As New Sensor(sensorID, name)
		sensor.Save()
	End Sub

	<WebMethod()> _
	Public Function FindSensor(ByVal name As String) As Sensor
		Return Sensor.GetSensor(name)
	End Function

	'<WebMethod()> _
	'Public Function ListAllPhotos() As List(Of PhotoReading)

	'	Dim dt As New DataTable("PhotoReadings")
	'	Dim dcID As New DataColumn("PhotoreadingID")
	'	Dim dcTime As New DataColumn("Time")
	'	dt.Columns.Add(dcID)
	'	dt.Columns.Add(dcTime)

	'	conn.Open()
	'	Dim pCmd As New SqlCommand("", conn)
	'	pCmd.CommandText = "SELECT * FROM PhotoReadings Order by Time DESC"
	'	reader = pCmd.ExecuteReader
	'	Dim dr As DataRow
	'	While reader.Read
	'		dr = dt.NewRow
	'		dr("PhotoReadingID") = reader.GetValue(0)
	'		dr("Time") = reader.GetValue(2)
	'		dt.Rows.Add(dr)
	'	End While
	'	Return dt
	'	conn.Close()
	'End Function
End Class
