Imports System.Data.SqlClient
Imports QutSensors
Imports System.IO
Public Class Service1

    'Move this part into the app.config
    Dim connString As String = My.Settings.connString
    Dim mainDirectory As String = My.Settings.mainDirectory

    Dim _sensor As Sensor

    Dim conn As New SqlConnection(connString)
    Dim cmd As New SqlCommand("", conn)
    Dim reader As SqlDataReader

    Dim serv As New uploadService.Service


    Dim oFile As System.IO.File
    Dim writer As New System.IO.StreamWriter("d:\temp\log.txt", True)
    Dim oReader As System.IO.StreamReader

    WithEvents timer1 As New System.Timers.Timer
    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.        
        'StartSync()
        writer.Write(connString)
        writer.WriteLine(mainDirectory)
        writer.Write(DateTime.Now.ToString)
        writer.WriteLine(" - Service started")
        writer.Flush()

        timer1.Interval = 180000
        'timer1.Start()

    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        writer.Write(DateTime.Now.ToString)
        writer.WriteLine(" - Service stopped")
        writer.Close()
    End Sub

    Sub timer_tick(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles timer1.Elapsed
            StartSync()
    End Sub


    Private Sub SyncSensor(ByVal sPath As String)
        Dim sName As String = Path.GetFileName(sPath)
        _sensor = Sensor.GetSensor(sName)
        'conn.Open()
        'cmd.CommandText = "SELECT SensorID from Sensors where Name='" & sname & "'"
        'reader = cmd.ExecuteReader
        If _sensor Is Nothing Then
            _sensor = New Sensor(sName)
            _sensor.Save()

            'Check sensor on the server
            Dim sensorExist As Boolean
            Dim ds As New DataSet
            ds = serv.FindSensor(sName)
            If ds.Tables(0).Rows.Count > 0 Then sensorExist = True Else sensorExist = False
            'sensorExist = serv.FindSensor(sName)
        End If

        writer.WriteLine("Found sensor: {0}", _sensor.ID.Value)
        conn.Close()

        Dim sPhoto, sSound As String
        sPhoto = System.IO.Path.Combine(sPath, "picture")
        sSound = System.IO.Path.Combine(sPath, "sound")
        WriteLog("Processing photo readings in " & sPhoto)
        SyncPhoto(sPhoto)
        SyncSound(sSound)
        'WriteLog("Processing sound readings in {0} ", sSound)

    End Sub

    Private Sub SyncPhoto(ByVal pPath As String)
        WriteLog("Synchronising Photos")
        Dim fn As String
        Dim pDate As DateTime
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@SID", SqlDbType.UniqueIdentifier)
        pCmd.Parameters.Add("@pTime", SqlDbType.DateTime)

        'This part check for local file
        For Each fn In System.IO.Directory.GetFiles(pPath, "*.jpg")
            DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fn).Substring(4), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeLocal, pDate)

            Dim stream As FileStream
            stream = File.Open(fn, FileMode.Open)
            Dim bReader As New BinaryReader(stream)
            Dim buffer As Byte() = bReader.ReadBytes(1500 * 1024)
            stream.Close()

            WriteLog(fn & " " & pDate)
            'WriteLog()
            'WriteLog(fn)
            'WriteLog(pDate)
            pCmd.CommandText = "SELECT * FROM Photoreadings WHERE SensorID = @SID and Time = @pTime"
            pCmd.Parameters("@SID").Value = _sensor.ID.Value
            pCmd.Parameters("@pTime").Value = pDate
            conn.Open()
            reader = pCmd.ExecuteReader
            If reader.Read Then
                'WriteLog(reader.GetValue(0))
                WriteLog("Picture found")
            Else
                WriteLog("Picture not found")
                Dim reading As New PhotoReading(_sensor.ID.Value)
                reading.Time = pDate
                reading.Save()
                reading.UpdateData(buffer)
            End If
            conn.Close()


            'This part does the webservice
            Dim pGuid As New Guid(_sensor.ID.Value.ToString)
            Dim pExist As Boolean
            pExist = serv.FindPhotoReading(pGuid, pDate)

            If Not pExist Then
                serv.addPhotoReading(_sensor.ID.Value, pDate, buffer)
            End If

        Next
        'Console.ReadLine()
    End Sub

    Private Sub SyncSound(ByVal sPath As String)
        WriteLog("Synchronising Audios")

        Dim fn As String
        Dim pDate As DateTime
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@SID", SqlDbType.UniqueIdentifier)
        pCmd.Parameters.Add("@pTime", SqlDbType.DateTime)

        WriteLog(sPath)

        For Each fn In System.IO.Directory.GetFiles(sPath)
            DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fn).Substring(4), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeLocal, pDate)

            Dim stream As FileStream
            stream = File.Open(fn, FileMode.Open)
            Dim bReader As New BinaryReader(stream)
            Dim buffer As Byte() = bReader.ReadBytes(1500 * 1024)

            WriteLog(fn & " " & pDate)

            pCmd.CommandText = "SELECT * FROM AudioReadings WHERE SensorID = @SID and Time = @pTime"
            pCmd.Parameters("@SID").Value = _sensor.ID.Value
            pCmd.Parameters("@pTime").Value = pDate
            conn.Open()
            reader = pCmd.ExecuteReader
            If reader.Read Then
                'WriteLog(reader.GetValue(0))
                WriteLog("Sound found")
            Else
                WriteLog("Sound updated")
                Dim reading As New AudioReading(_sensor.ID.Value)
                reading.Time = pDate
                reading.Save()
                reading.UpdateData(buffer)
            End If
            conn.Close()

            'This part does the webservice
            Dim pGuid As New Guid(_sensor.ID.Value.ToString)
            Dim pExist As Boolean
            pExist = serv.FindAudioReading(pGuid, pDate)

            If Not pExist Then
                serv.addAudioReading(_sensor.ID.Value, pDate, buffer)
            End If

        Next
        'Console.ReadLine()
    End Sub


    Private Sub StartSync()
        Dim dir As String
        For Each dir In System.IO.Directory.GetDirectories(mainDirectory)
            _sensor = New Sensor(Path.GetFileName(dir))
            WriteLog("Found sensors: " & dir)
            SyncSensor(dir)
        Next
    End Sub

    Private Sub WriteLog(ByVal log As String)
        writer.Write(Now.ToString)
        writer.WriteLine(" - " & log)
    End Sub
End Class
