Imports System.Data
Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports QutSensors



<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Service
    Inherits System.Web.Services.WebService

    Dim conn As New SqlConnection(QutSensors.DB.ConnectionString)
    Dim reader As SqlDataReader


    <WebMethod()> _
    Public Function addPhotoReading(ByVal pGuid As Guid, ByVal pDate As Date, ByVal buffer As Byte()) As String
        Dim reading As New QutSensors.PhotoReading(pGuid)
        reading.Time = pDate
        reading.Save()
        reading.UpdateData(buffer)
        Return "Success"
    End Function

    <WebMethod()> _
    Public Function FindPhotoReading(ByVal pGuid As Guid, ByVal pDate As Date) As Boolean
        conn.Open()
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@SID", SqlDbType.UniqueIdentifier)
        pCmd.Parameters.Add("@pTime", SqlDbType.DateTime)
        pCmd.CommandText = "SELECT * FROM Photoreadings WHERE SensorID = @SID and Time = @pTime"
        pCmd.Parameters("@SID").Value = pGuid
        pCmd.Parameters("@pTime").Value = pDate
        reader = pCmd.ExecuteReader
        If reader.Read Then
            conn.Close()
            Return True
        Else
            conn.Close()
            Return False
        End If
    End Function

    <WebMethod()> _
    Public Function FindAudioReading(ByVal pGuid As Guid, ByVal pDate As Date) As Boolean
        conn.Open()
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@SID", SqlDbType.UniqueIdentifier)
        pCmd.Parameters.Add("@pTime", SqlDbType.DateTime)
        pCmd.CommandText = "SELECT * FROM Audioreadings WHERE SensorID = @SID and Time = @pTime"
        pCmd.Parameters("@SID").Value = pGuid
        pCmd.Parameters("@pTime").Value = pDate
        reader = pCmd.ExecuteReader
        If reader.Read Then
            conn.Close()
            Return True
        Else
            conn.Close()
            Return False
        End If
    End Function

    <WebMethod()> _
   Public Function addAudioReading(ByVal aGuid As Guid, ByVal adate As Date, ByVal buffer As Byte()) As String
        Dim reading As New QutSensors.AudioReading(aGuid)
        reading.Time = adate
        reading.Save()
        reading.UpdateData(buffer)
        Return "Success"
    End Function

    <WebMethod()> _
    Public Function getPhotoreading(ByVal sensorName As String, ByVal pDate As Date) As DataTable
        Dim adapter As New SqlDataAdapter("SELECT Sensors.SensorID, Sensors.Name, Time, Data FROM PhotoReadings,Sensors Where PhotoReadings.SensorID = Sensors.SensorID and Name='" & sensorName & "'", conn)
        Dim dt As New DataTable("PhotoReadings")
        adapter.Fill(dt)
        Return dt
    End Function

    <WebMethod()> _
    Public Function getAudioreading(ByVal sensorName As String, ByVal pDate As Date) As DataTable
        Dim adapter As New SqlDataAdapter("SELECT Sensors.SensorID, Sensors.Name, Time, Data FROM AudioReadings,Sensors Where AudioReadings.SensorID = Sensors.SensorID and Name='" & sensorName & "'", conn)
        Dim dt As New DataTable("AudioReadings")
        adapter.Fill(dt)
        Return dt
    End Function
    <WebMethod()> _
      Public Function getConnectionString() As String
        Return conn.ConnectionString
    End Function

    <WebMethod()> _
    Public Function testAddAudio(ByVal aGuid As Guid, ByVal buffer As Byte()) As String
        Dim cmd As New SqlCommand("", conn)
        cmd.CommandText = "INSERT INTO AudioReadings (AudioReadingID, Data, Time) Values (@AID, @Data, @Time)"
        cmd.Parameters.Add("@AID", SqlDbType.UniqueIdentifier)
        cmd.Parameters.Add("@Data", SqlDbType.Image)
        cmd.Parameters.Add("@Time", SqlDbType.DateTime)

        cmd.Parameters.Item("@AID").Value = aGuid
        cmd.Parameters.Item("@Data").Value = buffer
        cmd.Parameters.Item("@Time").Value = Now

        conn.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            Return ex.ToString
        End Try
        conn.Close()
        Return "Success"
    End Function

    <WebMethod()> _
Public Function addSensor(ByVal SID As Guid, ByVal sName As String) As String
        Dim cmd As New SqlCommand("", conn)
        cmd.CommandText = "INSERT INTO Sensors (SensorID, name) Values (@sid, @sName)"
        cmd.Parameters.Add("@sID", SqlDbType.UniqueIdentifier)
        cmd.Parameters.Add("@name", SqlDbType.VarChar)

        cmd.Parameters("sID").Value = SID
        cmd.Parameters("@name").Value = sName

        conn.Open()
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            Return ex.ToString
        End Try

        Return "Success"
    End Function

    <WebMethod()> _
        Public Function FindSensor(ByVal sName As String) As DataTable
        Dim dt As New DataTable("Sensors")
        Dim dcID As New DataColumn("SensorID")
        Dim dcName As New DataColumn("Name")
        dt.Columns.Add(dcID)
        dt.Columns.Add(dcName)

        conn.Open()
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@sName", SqlDbType.VarChar)
        pCmd.CommandText = "SELECT * FROM Sensors WHERE Name = @sName"
        pCmd.Parameters("@sName").Value = sName
        reader = pCmd.ExecuteReader
        Dim dr As DataRow = dt.NewRow
        If reader.Read Then
            dr("sensorID") = reader.GetValue(0)
            dr("Name") = reader.GetString(1)
            dt.Rows.Add(dr)
            Return dt
            conn.Close()
        Else            
            conn.Close()
            Return Nothing
        End If
    End Function

    <WebMethod()> _
       Public Function ListAllPhotos() As DataTable
        Dim dt As New DataTable("PhotoReadings")
        Dim dcID As New DataColumn("PhotoreadingID")
        Dim dcTime As New DataColumn("Time")
        dt.Columns.Add(dcID)
        dt.Columns.Add(dcTime)

        conn.Open()
        Dim pCmd As New SqlCommand("", conn)
        pCmd.CommandText = "SELECT * FROM PhotoReadings Order by Time DESC"
        reader = pCmd.ExecuteReader
        Dim dr As DataRow
        While reader.Read
            dr = dt.NewRow
            dr("PhotoReadingID") = reader.GetValue(0)
            dr("Time") = reader.GetValue(2)
            dt.Rows.Add(dr)
        End While
        Return dt
        conn.Close()
    End Function
End Class
