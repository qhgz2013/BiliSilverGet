'转载信息:

'CRC计算 代码源自 http://www.cnblogs.com/clso/archive/2010/11/29/1891082.html
'博客: http://clso.cnblogs.com/
'主页: http://clso.tk/


Public Class ClsoCRC
    Public Shared ReadOnly Table As UInteger()

    Shared Sub New()
        ReDim Table(256)
        For i As UInteger = 0 To 255
            Dim r As UInteger = i
            For j As Integer = 0 To 7
                If (r And 1) <> 0 Then
                    r = (r >> 1) Xor 3988292384
                Else
                    r >>= 1
                End If
            Next
            Table(i) = r
        Next
    End Sub

    Private _value As UInteger = UInteger.MaxValue

    ''' <summary>初始化类，重新计算CRC前请先调用本过程。</summary>
    Public Sub Init()
        _value = UInt32.MaxValue
    End Sub

    ''' <summary>更新并计算单个字节。(追加方式)</summary>
    ''' <param name="b">数据字节。</param>
    Public Sub UpdateByte(ByVal b As Byte)
        _value = Table(UInt32toByte(_value) Xor b) Xor (_value >> 8)
    End Sub

    ''' <summary>更新类并开始计算CRC。</summary>
    ''' <param name="data">需要计算的数据组。</param>
    ''' <param name="offset">偏移，一般设为0。</param>
    ''' <param name="size">大小，一般设为数据组的Length。</param>
    Public Sub Update(ByVal data As Byte(), ByVal offset As UInt32, ByVal size As UInt32)
        If size = 0 Then Return
        For i As UInteger = 0 To size - 1
            _value = Table(UInt32toByte(_value) Xor data(offset + i)) Xor (_value >> 8)
        Next
    End Sub

    ''' <summary>获取CRC数据。</summary>
    Public Function GetDigest() As UInt32
        Return _value Xor UInt32.MaxValue
    End Function

    ''' <summary>计算并获取CRC数据。(利用静态实例化实现)</summary>
    ''' <param name="data">需要计算的数据组。</param>
    ''' <param name="offset">偏移，一般设为0。</param>
    ''' <param name="size">大小，一般设为数据组的Length。</param>
    Public Shared Function CalculateDigest(ByVal data As Byte(), ByVal offset As UInt32, ByVal size As UInt32) As UInt32
        Dim crc As New ClsoCRC
        'crc.Init()
        crc.Update(data, offset, size)
        Return crc.GetDigest
    End Function

    ''' <summary>校验摘要</summary>
    ''' <param name="digest">摘要</param>
    ''' <param name="data">数据组</param>
    ''' <param name="offset">偏移</param>
    ''' <param name="size">大小</param>
    Private Shared Function VerifyDigest(ByVal digest As UInt32, ByVal data As Byte(), ByVal offset As UInt32, ByVal size As UInt32) As Boolean
        Return CalculateDigest(data, offset, size) = digest
    End Function

    ''' <summary>返回BYTE (CLE添加)</summary>
    Private Shared Function UInt32toByte(ByVal UInt32Value As UInteger) As Byte
        Return UInt32Value Mod 256
    End Function

End Class