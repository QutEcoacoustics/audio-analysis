Imports QUTSamfordUploader.localhost.Service
Imports QutSensors
Imports System.IO
Imports System.Data.SqlClient
Module Module1
    Const connString As String = "Data Source=localhost;Initial Catalog=Qutsensors;Integrated Security=True;Pooling=False;"
    Const mainDirectory As String = "D:\stargate\home\stuart"
    Dim _sensor As Sensor

    Dim conn As New SqlConnection(connString)
    Dim cmd As New SqlCommand("", conn)
    Dim reader As SqlDataReader

    Dim serv As New localhost.Service


    WithEvents timer1 As New System.Timers.Timer

    Sub Main()
        Try
            StartSync()
            timer1.Interval = 10000 '1000 = 1sec
            timer1.Enabled = True
            timer1.AutoReset = True
            While Console.ReadLine <> "q"
            End While
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
            MsgBox(ex.ToString)
        End Try

    End Sub

    Private Sub timer_tick(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles timer1.Elapsed
        StartSync()
    End Sub

    Private Sub SyncSensor(ByVal sPath As String)
        Dim sname As String = Path.GetFileName(sPath)
        conn.Open()
        cmd.CommandText = "SELECT SensorID from Sensors where Name='" & sname & "'"
        reader = cmd.ExecuteReader
        If reader.Read Then
            _sensor.ID = reader.GetGuid(0)
        Else
            Dim sGuid As Guid
            sGuid = Guid.NewGuid
            cmd.CommandText = "INSERT INTO Sensors Values(@SID, @Name, @Friendlyname, @Description)"
            Dim sid As New SqlParameter("@SID", SqlDbType.UniqueIdentifier)
            Dim name As New SqlParameter("@Name", SqlDbType.VarChar)
            Dim Friendlyname As New SqlParameter("@FriendlyName", SqlDbType.VarChar)
            Dim Description As New SqlParameter("@Description", SqlDbType.VarChar)
            sid.Value = sGuid
            name.Value = sname
            Friendlyname.Value = ""
            Description.Value = ""
            cmd.Parameters.Add(sid)
            cmd.Parameters.Add(name)
            cmd.Parameters.Add(Friendlyname)
            cmd.Parameters.Add(Description)
            reader.Close()
            cmd.ExecuteNonQuery()
            _sensor.ID = sGuid

            'Check sensor on the server
            Dim sensorExist As Boolean
            sensorExist = serv.findSensor(sname)
        End If

        Console.WriteLine(_sensor.ID.Value)
        conn.Close()

        Dim sPhoto, sSound As String
        sPhoto = System.IO.Path.Combine(sPath, "picture")
        sSound = System.IO.Path.Combine(sPath, "sound")
        Console.WriteLine("Processing photo readings in {0} ", sPhoto)
        SyncPhoto(sPhoto)
        SyncSound(sSound)
        'Console.WriteLine("Processing sound readings in {0} ", sSound)

    End Sub

    Private Sub SyncPhoto(ByVal pPath As String)
        Console.WriteLine("Synchronising Photos")
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

            Console.WriteLine("{0} {1}", fn, pDate)
            'Console.WriteLine()
            'Console.WriteLine(fn)
            'Console.WriteLine(pDate)
            pCmd.CommandText = "SELECT * FROM Photoreadings WHERE SensorID = @SID and Time = @pTime"
            pCmd.Parameters("@SID").Value = _sensor.ID.Value
            pCmd.Parameters("@pTime").Value = pDate
            conn.Open()
            reader = pCmd.ExecuteReader
            If reader.Read Then
                'Console.WriteLine(reader.GetValue(0))
                Console.WriteLine("Picture found")
            Else
                Console.WriteLine("Picture not found")
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
        Console.WriteLine("Synchronising Audios")
        Dim fn As String
        Dim pDate As DateTime
        Dim pCmd As New SqlCommand("", conn)
        pCmd.Parameters.Add("@SID", SqlDbType.UniqueIdentifier)
        pCmd.Parameters.Add("@pTime", SqlDbType.DateTime)

        Console.WriteLine(sPath)

        For Each fn In System.IO.Directory.GetFiles(sPath)
            DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fn).Substring(4), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeLocal, pDate)

            Dim stream As FileStream
            stream = File.Open(fn, FileMode.Open)
            Dim bReader As New BinaryReader(stream)
            Dim buffer As Byte() = bReader.ReadBytes(1500 * 1024)

            Console.Write("{0} {1}", fn, pDate)
            Console.WriteLine()

            pCmd.CommandText = "SELECT * FROM AudioReadings WHERE SensorID = @SID and Time = @pTime"
            pCmd.Parameters("@SID").Value = _sensor.ID.Value
            pCmd.Parameters("@pTime").Value = pDate
            conn.Open()
            reader = pCmd.ExecuteReader
            If reader.Read Then
                'Console.WriteLine(reader.GetValue(0))
                Console.WriteLine("Sound found")
            Else
                Console.WriteLine("Sound updated")
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
        Console.WriteLine("-------Starting-------")
        For Each dir In System.IO.Directory.GetDirectories(mainDirectory)
            _sensor = New Sensor(Path.GetFileName(dir))
            Console.WriteLine("Found sensors: {0} ", dir)
            SyncSensor(dir)
        Next
    End Sub
End Module
