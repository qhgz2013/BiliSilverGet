Namespace Utils
    Namespace ByteUtils
        Public Module Functions
            Public Function ByteToUInt(ByVal x As Byte()) As UInteger
                Return System.BitConverter.ToUInt32(x.Reverse.ToArray, 0)
            End Function
            Public Function UIntToByte(ByVal x As UInteger) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToInt(ByVal x As Byte()) As Integer
                Return System.BitConverter.ToInt32(x.Reverse.ToArray, 0)
            End Function
            Public Function IntToByte(ByVal x As Integer) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToUShort(ByVal x As Byte()) As UShort
                Return System.BitConverter.ToUInt16(x.Reverse.ToArray, 0)
            End Function
            Public Function UShortToByte(ByVal x As UShort) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToShort(ByVal x As Byte()) As Short
                Return System.BitConverter.ToInt16(x.Reverse.ToArray, 0)
            End Function
            Public Function ShortToByte(ByVal x As Short) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToULong(ByVal x As Byte()) As ULong
                Return System.BitConverter.ToUInt64(x.Reverse.ToArray, 0)
            End Function
            Public Function ULongToByte(ByVal x As ULong) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToLong(ByVal x As Byte()) As Long
                Return System.BitConverter.ToInt64(x.Reverse.ToArray, 0)
            End Function
            Public Function LongToByte(ByVal x As Long) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToSingle(ByVal x As Byte()) As Single
                Return System.BitConverter.ToSingle(x.Reverse.ToArray, 0)
            End Function
            Public Function SingleToByte(ByVal x As Single) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function
            Public Function ByteToDouble(ByVal x As Byte()) As Double
                Return System.BitConverter.ToDouble(x.Reverse.ToArray, 0)
            End Function
            Public Function DoubleToByte(ByVal x As Double) As Byte()
                Return System.BitConverter.GetBytes(x).Reverse.ToArray
            End Function

        End Module
    End Namespace
    Namespace StreamUtils
        Public Module Functions
            Public Function ReadUI16(ByVal s As IO.Stream) As UShort
                Dim b(1) As Byte
                s.Read(b, 0, 2)
                Return ByteUtils.ByteToUShort(b)
            End Function
            Public Function ReadI16(ByVal s As IO.Stream) As Short
                Dim b(1) As Byte
                s.Read(b, 0, 2)
                Return ByteUtils.ByteToShort(b)
            End Function
            Public Function ReadUI24(ByVal s As IO.Stream) As UInteger
                Dim b(3) As Byte
                s.Read(b, 1, 3)
                Return ByteUtils.ByteToUInt(b)
            End Function
            Public Function ReadI24(ByVal s As IO.Stream) As Integer
                Dim b(3) As Byte
                s.Read(b, 1, 3)
                Return ByteUtils.ByteToInt(b)
            End Function
            Public Function ReadUI32(ByVal s As IO.Stream) As UInteger
                Dim b(3) As Byte
                s.Read(b, 0, 4)
                Return ByteUtils.ByteToUInt(b)
            End Function
            Public Function ReadI32(ByVal s As IO.Stream) As Integer
                Dim b(3) As Byte
                s.Read(b, 0, 4)
                Return ByteUtils.ByteToInt(b)
            End Function
            Public Function ReadUI64(ByVal s As IO.Stream) As ULong
                Dim b(7) As Byte
                s.Read(b, 0, 8)
                Return ByteUtils.ByteToULong(b)
            End Function
            Public Function ReadI64(ByVal s As IO.Stream) As Long
                Dim b(7) As Byte
                s.Read(b, 0, 8)
                Return ByteUtils.ByteToLong(b)
            End Function
            Public Function ReadSingle(ByVal s As IO.Stream) As Single
                Dim b(3) As Byte
                s.Read(b, 0, 4)
                Return ByteUtils.ByteToSingle(b)
            End Function
            Public Function ReadDouble(ByVal s As IO.Stream) As Double
                Dim b(7) As Byte
                s.Read(b, 0, 8)
                Return ByteUtils.ByteToDouble(b)
            End Function

            Public Function ReadToEnd(ByVal s As IO.Stream) As String
                Dim sr As New IO.StreamReader(s)
                Return sr.ReadToEnd
            End Function

            Public Function ReadAllBytes(ByVal s As IO.Stream) As Byte()
                Dim ms As New IO.MemoryStream
                Dim buf(4095) As Byte
                Dim nread As Integer
                Do
                    nread = s.Read(buf, 0, 4096)
                    ms.Write(buf, 0, nread)
                Loop Until nread = 0
                Return ms.ToArray
            End Function
        End Module
        Public Module Subs
            Public Sub WriteUI16(ByVal s As IO.Stream, ByVal x As UShort)
                Dim b() As Byte = ByteUtils.UShortToByte(x)
                s.Write(b, 0, 2)
            End Sub
            Public Sub WriteI16(ByVal s As IO.Stream, ByVal x As Short)
                Dim b() As Byte = ByteUtils.ShortToByte(x)
                s.Write(b, 0, 2)
            End Sub
            Public Sub WriteUI24(ByVal s As IO.Stream, ByVal x As UInteger)
                Dim b() As Byte = ByteUtils.UIntToByte(x)
                s.Write(b, 1, 3)
            End Sub
            Public Sub WriteI24(ByVal s As IO.Stream, ByVal x As Integer)
                Dim b() As Byte = ByteUtils.IntToByte(x)
                s.Write(b, 1, 3)
            End Sub
            Public Sub WriteUI32(ByVal s As IO.Stream, ByVal x As UInteger)
                Dim b() As Byte = ByteUtils.UIntToByte(x)
                s.Write(b, 0, 4)
            End Sub
            Public Sub WriteI32(ByVal s As IO.Stream, ByVal x As Integer)
                Dim b() As Byte = ByteUtils.IntToByte(x)
                s.Write(b, 0, 4)
            End Sub
            Public Sub WriteUI64(ByVal s As IO.Stream, ByVal x As ULong)
                Dim b() As Byte = ByteUtils.ULongToByte(x)
                s.Write(b, 0, 8)
            End Sub
            Public Sub WriteI64(ByVal s As IO.Stream, ByVal x As Long)
                Dim b() As Byte = ByteUtils.LongToByte(x)
                s.Write(b, 0, 8)
            End Sub
            Public Sub WriteSingle(ByVal s As IO.Stream, ByVal x As Single)
                Dim b() As Byte = ByteUtils.SingleToByte(x)
                s.Write(b, 0, 4)
            End Sub
            Public Sub WriteDouble(ByVal s As IO.Stream, ByVal x As Double)
                Dim b() As Byte = ByteUtils.DoubleToByte(x)
                s.Write(b, 0, 8)
            End Sub
        End Module
    End Namespace
    Namespace Others
        Public Module Functions
            ''' <summary>
            ''' 将字节大小自动转为合适的显示格式(如KB,MB,GB)
            ''' </summary>
            ''' <param name="size">字节大小</param>
            ''' <returns>等效的字符串</returns>
            ''' <remarks></remarks>
            Public Function FormatByteSize(ByVal size As Integer) As String
                If size < 1024 Then Return size & "B"
                If size >= 1024 AndAlso size < 1024 ^ 2 Then Return Format(size / 1024, "0.00") & "KB"
                If size >= 1024 ^ 2 AndAlso size < 1024 ^ 3 Then Return Format(size / 1024 ^ 2, "0.00") & "MB"
                Return Format(size / 1024 ^ 3, "0.00") & "GB"
            End Function
            Public Function ToUnixTimestamp(ByVal time As Date) As Double
                If time.Ticks = 0 Then Return 0
                Return Double.Parse((time.ToUniversalTime().Ticks / 10000000 - 62135596800))
            End Function
            Public Function FromUnixTimeStamp(ByVal time As Double) As Date
                Return (New DateTime(System.Convert.ToInt64((time + 62135596800) * 10000000))).ToLocalTime
            End Function
            Public rand As Random
            Sub New()
                rand = New Random(ToUnixTimestamp(Now))
            End Sub
            Public Function Hex(ByVal b As Byte()) As String
                Dim sb As New System.Text.StringBuilder
                For Each e As Byte In b
                    sb.Append(Right("0" & Microsoft.VisualBasic.Hex(e), 2))
                Next
                Return sb.ToString
            End Function

        End Module
    End Namespace
End Namespace