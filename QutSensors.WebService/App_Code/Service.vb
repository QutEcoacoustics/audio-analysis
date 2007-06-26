Imports System
Imports System.Web.Services
Imports QutSensors

<WebService(Namespace:="http://mquter.qut.edu.au/sensors/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Service
	Inherits System.Web.Services.WebService

	Private Function EnsureSensorExists(ByVal sensorID As Guid) As Sensor
		Dim mySensor As Sensor
		mySensor = Sensor.GetSensor(sensorID)
		If mySensor Is Nothing Then
			mySensor = New Sensor(sensorID)
			mySensor.Save()
		End If
		Return mySensor
	End Function

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

	<WebMethod()> _
	Public Function FindSensor(ByVal sensorGUID As String) As Sensor
		Return Sensor.GetSensor(New Guid(sensorGUID))
	End Function

	<WebMethod()> _
	Public Sub UpdateSensor(ByVal sensorGUID As String, ByVal name As String, ByVal friendlyName As String, ByVal description As String)
		Dim sensorID As New Guid(sensorGUID)
		Dim _sensor As Sensor = EnsureSensorExists(sensorID)
		_sensor.Name = name
		_sensor.FriendlyName = friendlyName
		_sensor.Description = Description
		_sensor.Save()
	End Sub

	<WebMethod()> _
	Public Sub AddSensorStatus(ByVal sensorGUID As String, ByVal time As DateTime, ByVal batteryLevel As Byte)
		Dim sensorID As New Guid(sensorGUID)
		Dim _sensor As Sensor = EnsureSensorExists(sensorID)

		_sensor.AddStatus(time, batteryLevel)
	End Sub
End Class
