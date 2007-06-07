Public Class PDA
    Private Const SETPOWERMANAGEMENT As Int32 = 6147

    Declare Function ExtEscapeSet Lib "coredll" Alias "ExtEscape" (ByVal hdc As IntPtr, _
                                              ByVal nEscape As Int32, _
                                              ByVal cbInput As Int32, _
                                              ByVal plszInData As Byte(), _
                                              ByVal cbOutput As Int32, _
                                              ByVal lpszOutData As IntPtr) As Int32

    Declare Function GetDC Lib "coredll" (ByVal hwnd As IntPtr) As IntPtr

    Private Enum VideoPowerState As Integer
        VideoPowerOn = 1
        VideoPowerStandBy
        VideoPowerSuspend
        VideoPowerOff
    End Enum

    Public Shared Sub PowerOffScreen()
        Dim hdc As IntPtr = GetDC(IntPtr.Zero)
        Dim vpm() As Byte = {12, 0, 0, 0, 1, 0, 0, 0, VideoPowerState.VideoPowerOff, 0, 0, 0, 0}
        ExtEscapeSet(hdc, SETPOWERMANAGEMENT, 12, vpm, 0, IntPtr.Zero)
    End Sub

    Public Shared Sub PowerOnScreen()
        Dim hdc As IntPtr = GetDC(IntPtr.Zero)
        Dim vpm() As Byte = {12, 0, 0, 0, 1, 0, 0, 0, VideoPowerState.VideoPowerOn, 0, 0, 0, 0}
        ExtEscapeSet(hdc, SETPOWERMANAGEMENT, 12, vpm, 0, IntPtr.Zero)
    End Sub

    Private Const FILE_DEVICE_HAL As Integer = &H101
    Private Const METHOD_BUFFERED As Integer = 0
    Private Const FILE_ANY_ACCESS As Integer = 0

    Shared Function CTL_CODE( _
      ByVal DeviceType As Integer, _
      ByVal Func As Integer, _
      ByVal Method As Integer, _
      ByVal Access As Integer) As Integer

        Return (DeviceType << 16) Or (Access << 14) Or (Func << 2) Or Method

    End Function

    <Runtime.InteropServices.DllImport("Coredll.dll")> _
    Private Shared Function KernelIoControl _
    ( _
        ByVal dwIoControlCode As Integer, _
        ByVal lpInBuf As IntPtr, _
        ByVal nInBufSize As Integer, _
        ByVal lpOutBuf As IntPtr, _
        ByVal nOutBufSize As Integer, _
        ByRef lpBytesReturned As Integer _
    ) As Integer
    End Function

    Shared Function SoftReset() As Integer
        Dim bytesReturned As Integer = 0
        Dim IOCTL_HAL_REBOOT As Integer = CTL_CODE(FILE_DEVICE_HAL, _
          15, METHOD_BUFFERED, FILE_ANY_ACCESS)
        Return KernelIoControl(IOCTL_HAL_REBOOT, IntPtr.Zero, 0, _
          IntPtr.Zero, 0, bytesReturned)
    End Function

End Class


