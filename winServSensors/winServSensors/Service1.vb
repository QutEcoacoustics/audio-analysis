Imports System.ServiceProcess
Public Class EmptyService
    Inherits System.ServiceProcess.ServiceBase
    Private worker As New worker()
    Protected Overrides Sub OnStart(ByVal args() As String)
        System.IO.Directory.SetCurrentDirectory(GetMyDir)
        Dim wt As System.Threading.Thread
        Dim ts As System.Threading.ThreadStart
        ts = AddressOf worker.DoWork
        wt = New System.Threading.Thread(ts)
        wt.Start()
    End Sub
    Protected Overrides Sub OnStop()
        worker.StopWork()
    End Sub
    Function GetMyDir() As String
        Dim fi As System.IO.FileInfo
        Dim di As System.IO.DirectoryInfo
        Dim pc As System.Diagnostics.Process
        Try
            pc = System.Diagnostics.Process.GetCurrentProcess
            fi = New System.IO.FileInfo(pc.MainModule.FileName)
            di = fi.Directory
            GetMyDir = di.FullName
        Finally
            fi = Nothing
            di = Nothing
            pc = Nothing
        End Try
    End Function
End Class

Public Class Worker
    Private m_thMain As System.Threading.Thread
    Private m_booMustStop As Boolean = False
    Private m_rndGen As New Random(Now.Millisecond)
    Public Sub StopWork()
        m_booMustStop = True
        If Not m_thMain Is Nothing Then
            If Not m_thMain.Join(100) Then
                m_thMain.Abort()
            End If
        End If
    End Sub
    Public Sub DoWork()
        m_thMain = System.Threading.Thread.CurrentThread
        Dim i As Integer = m_rndGen.Next
        m_thMain.Name = "Thread" & i.ToString
        While Not m_booMustStop
            System.Diagnostics.EventLog.WriteEntry("EmptyService", "Start work: " & m_thMain.Name)
            System.Threading.Thread.Sleep(10000)
            System.Diagnostics.EventLog.WriteEntry("EmptyService", "Finish work: " & m_thMain.Name)
        End While
    End Sub
End Class