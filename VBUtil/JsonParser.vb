' rewrite from VBA JSON project
' author: Michael Glaser (vbjson@ediy.co.nz)
' source code: http://code.google.com/p/vba-json
' BSD Licensed

' translated into vb.net : pandasxd (qhgz2011@hotmail.com)
' version 1.0.0 beta [debugged]
' READ ONLY!! Output part is under construction

Public Class JsonElement
    'Private _obj As Object
    Default Public Property Item(ByVal str As String) As JsonElement
        Get
            Return CType(Me, JsonObject)(str)
        End Get
        Set(value As JsonElement)
            CType(Me, JsonObject)(str) = value
        End Set
    End Property
    Default Public Property Item(ByVal index As Integer) As JsonElement
        Get
            Return CType(Me, JsonArray)(index)
        End Get
        Set(value As JsonElement)
            CType(Me, JsonArray)(index) = value
        End Set
    End Property
    Public Property Value() As Object
        Get
            Return CType(Me, JsonValue).Value
        End Get
        Set(value As Object)
            CType(Me, JsonValue).Value = value
        End Set
    End Property
    Public Function BuildJsonString() As String
        Return ""
    End Function
End Class
Public Class JsonArray : Inherits JsonElement
    Private _obj As List(Of JsonElement)

    Public Sub Add(ByVal element As JsonElement)
        _obj.Add(element)
    End Sub
    Public Sub Insert(ByVal index As Integer, ByVal element As JsonElement)
        _obj.Insert(index, element)
    End Sub
    Default Public Overloads Property Item(ByVal index As Integer) As JsonElement
        Get
            Return _obj(index)
        End Get
        Set(value As JsonElement)
            _obj(index) = value
        End Set
    End Property
    Public Sub Remove(ByVal index As Integer)
        _obj.RemoveAt(index)
    End Sub
    Public Function ContainsElement(ByVal element As JsonElement) As Boolean
        Return _obj.Contains(element)
    End Function
    Public Sub New()
        _obj = New List(Of JsonElement)
    End Sub
    Public Overrides Function ToString() As String
        Return "{Count = " & _obj.Count & "}"
    End Function
    Public Overloads Function BuildJsonString() As String
        Dim a As New System.Text.StringBuilder
        a.Append("[")
        For i As Integer = 0 To _obj.Count - 1
            a.Append(_obj(i).BuildJsonString())
        Next
        a.Append("]")
        Return a.ToString
    End Function
End Class
Public Class JsonObject : Inherits JsonElement
    Private _obj As Dictionary(Of String, JsonElement)
    Public Sub Add(ByVal key As String, ByVal element As JsonElement)
        _obj.Add(key, element)
    End Sub
    Default Public Overloads Property Item(ByVal key As String) As JsonElement
        Get
            Return _obj(key)
        End Get
        Set(value As JsonElement)
            _obj(key) = value
        End Set
    End Property
    Public Function Remove(ByVal key As String) As Boolean
        Return _obj.Remove(key)
    End Function
    Public Function ContainsKey(ByVal key As String) As Boolean
        Return _obj.ContainsKey(key)
    End Function
    Public Function ContainsElement(ByVal element As JsonElement) As Boolean
        Return _obj.ContainsValue(element)
    End Function
    Public Sub New()
        _obj = New Dictionary(Of String, JsonElement)
    End Sub
    Public Overrides Function ToString() As String
        Return "{Count = " & _obj.Count & "}"
    End Function
    Public Overloads Function BuildJsonString() As String
        Dim a As New System.Text.StringBuilder

        Return a.ToString
    End Function
End Class
Public Class JsonValue : Inherits JsonElement
    Private _obj As Object
    Public Overloads Property Value As Object
        Get
            Return _obj
        End Get
        Set(value As Object)
            _obj = value
        End Set
    End Property
    Public Sub New()

    End Sub
    Public Sub New(ByVal obj As Object)
        _obj = obj
    End Sub
    Public Overrides Function ToString() As String
        Return "{" & _obj.ToString & "}"
    End Function
End Class
Public Class JsonParser
    'Const INVALID_JSON As Integer = 1
    'Const INVALID_OBJECT As Integer = 2
    'Const INVALID_ARRAY As Integer = 3
    'Const INVALID_BOOLEAN As Integer = 4
    'Const INVALID_NULL As Integer = 5
    'Const INVALID_KEY As Integer = 6
    'Const INVALID_RPC_CALL As Integer = 7

    Private psErrors As String
    Public Function GetParserErrors() As String
        Return psErrors
    End Function
    Public Sub ClearParserError()
        psErrors = "*"
    End Sub
    Private root As JsonElement
    Public Sub Open(ByVal file As String)
        Dim sr As New IO.StreamReader(file)
        Dim str As String = sr.ReadToEnd()
        OpenJSON(str)
    End Sub
    Public Sub OpenJSON(ByVal jsonStr As String)
        root = parse(jsonStr)
    End Sub
    Public Property Items() As JsonElement
        Get
            Return root
        End Get
        Set(value As JsonElement)
            root = value
        End Set
    End Property
    'parse string and create JSON object
    Private Function parse(ByRef str As String) As JsonElement
        Dim index As Long = 1
        psErrors = "*"
        skipChar(str, index)
        Select Case Mid(str, index, 1)
            Case "{"
                Return parseObject(str, index)
            Case "["
                Return parseArray(str, index)
            Case Else
                psErrors = "Invalid JSON"
                Return Nothing
        End Select
    End Function
    'parse collections of key/value
    Private Function parseObject(ByRef str As String, ByRef index As Long) As JsonObject
        Dim ret As New JsonObject
        Dim sKey As String

        '"{"
        skipChar(str, index)
        If Mid(str, index, 1) <> "{" Then
            psErrors &= "Invalid Object at position " & index & ":" & Mid(str, index) & vbCrLf
            Return ret
        End If

        index += 1

        Do
            skipChar(str, index)
            If Mid(str, index, 1) = "}" Then
                index += 1
                Exit Do
            ElseIf Mid(str, index, 1) = "," Then
                index += 1
                skipChar(str, index)
            ElseIf index > Len(str) Then
                psErrors &= "Missing '}': " & Right(str, 20) & vbCrLf
                Exit Do
            End If

            'add key/value pair
            sKey = parseKey(str, index)
            ret.Add(sKey, parseValue(str, index))
            If Err.Number <> 0 Then
                psErrors &= Err.Description & ": " & sKey & vbCrLf
                Exit Do
            End If
        Loop
eh:
        Return ret
    End Function
    'parse list
    Private Function parseArray(ByRef str As String, ByRef index As Long) As JsonArray
        Dim ret As New JsonArray

        '"{"
        skipChar(str, index)

        If Mid(str, index, 1) <> "[" Then
            psErrors &= "Invalid Object at position " & index & ":" & Mid(str, index) & vbCrLf
            Return ret
        End If

        index += 1

        Do
            skipChar(str, index)
            If Mid(str, index, 1) = "]" Then
                index += 1
                Exit Do
            ElseIf Mid(str, index, 1) = "," Then
                index += 1
                skipChar(str, index)
            ElseIf index > Len(str) Then
                psErrors &= "Missing '}': " & Right(str, 20) & vbCrLf
                Exit Do
            End If

            'add value
            ret.Add(parseValue(str, index))

            If Err.Number <> 0 Then
                psErrors = psErrors & Err.Description & ": " & Mid(str, index, 20) & vbCrLf
                Exit Do
            End If
        Loop

        Return ret
    End Function
    'parse string/number/object/array/boolean/null
    Private Function parseValue(ByRef str As String, ByRef index As Long) As JsonElement
        skipChar(str, index)
        Select Case Mid(str, index, 1)
            Case "{"
                Return parseObject(str, index)
            Case "["
                Return parseArray(str, index)
            Case """", "'"
                Return parseString(str, index)
            Case "t", "f"
                Return parseBoolean(str, index)
            Case "n"
                Return parseNull(str, index)
            Case Else
                Return parseNumber(str, index)
        End Select
    End Function
    'parse string
    Private Function parseString(ByRef str As String, ByRef index As Long) As JsonValue
        Dim quote, chr, code As String
        Dim sb As New System.Text.StringBuilder

        skipChar(str, index)
        quote = Mid(str, index, 1)
        index += 1

        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            Select Case chr
                Case "\"
                    index += 1
                    chr = Mid(str, index, 1)
                    Select Case chr
                        Case """", "\", "/", """"
                            sb.Append(chr)
                            index += 1
                        Case "b"
                            sb.Append(vbBack)
                            index += 1
                        Case "f"
                            sb.Append(vbFormFeed)
                            index += 1
                        Case "n"
                            sb.Append(vbLf)
                            index += 1
                        Case "r"
                            sb.Append(vbCr)
                            index += 1
                        Case "t"
                            sb.Append(vbTab)
                            index += 1
                        Case "u"
                            index += 1
                            code = Mid(str, index, 4)
                            sb.Append(ChrW(Val("&h" & code)))
                            index += 4
                    End Select
                Case quote
                    index += 1

                    Return New JsonValue(sb.ToString)
                Case Else
                    sb.Append(chr)
                    index += 1
            End Select
        End While

        Return New JsonValue(sb.ToString)
    End Function
    'parse number
    Private Function parseNumber(ByRef str As String, ByRef index As Long) As JsonValue
        Dim value As String = "", chr As String
        skipChar(str, index)
        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            If InStr("+-0123456789.eE", chr) Then
                value &= chr
                index += 1
            Else
                Return New JsonValue(CDec(value))
            End If
        End While
        Return New JsonValue(CDec(value))
    End Function
    'parse true/false
    Private Function parseBoolean(ByRef str As String, ByRef index As Long) As JsonValue

        skipChar(str, index)
        If Mid(str, index, 4) = "true" Then
            index += 4
            Return New JsonValue(True)
        ElseIf Mid(str, index, 5) = "false" Then
            index += 5
            Return New JsonValue(False)
        Else
            psErrors *= "Invalid Boolean at position " & index & " : " & Mid(str, index) & vbCrLf
        End If
        Return New JsonValue(False)
    End Function
    'parse null
    Private Function parseNull(ByRef str As String, ByRef index As Long) As JsonValue

        skipChar(str, index)
        If Mid(str, index, 4) = "null" Then
            index += 4
            Return New JsonValue(Nothing)
        Else
            psErrors &= "Invalid null value at position " & index & " : " & Mid(str, index) & vbCrLf
        End If
        Return New JsonValue(Nothing)
    End Function
    Private Function parseKey(ByRef str As String, ByRef index As Long) As String
        Dim dquote, squote As Boolean
        Dim ret As String = ""
        Dim chr As String
        skipChar(str, index)
        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            Select Case chr
                Case """"
                    dquote = Not dquote
                    index += 1
                    If Not dquote Then
                        skipChar(str, index)
                        If Mid(str, index, 1) <> ":" Then
                            psErrors &= "Invalid Key at position " & index & " : " & ret & vbCrLf
                            Exit While
                        End If
                    End If
                Case "'"
                    squote = Not squote
                    index += 1
                    If Not squote Then
                        skipChar(str, index)
                        If Mid(str, index, 1) <> ":" Then
                            psErrors &= "Invalid Key at position " & index & " : " & ret & vbCrLf
                            Exit While
                        End If
                    End If
                Case ":"
                    index += 1
                    If Not dquote AndAlso Not squote Then
                        Exit While
                    Else
                        ret &= chr
                    End If
                Case Else
                    If InStr(vbCrLf & vbCr & vbLf & vbTab & "", chr) Then
                    Else
                        ret &= chr
                    End If
                    index += 1
            End Select
        End While
        Return ret
    End Function
    'skip special character
    Private Sub skipChar(ByRef str As String, ByRef index As Long)
        Dim bComment, bStartComment, bLongComment As Boolean
        While index > 0 AndAlso index <= Len(str)
            Select Case Mid(str, index, 1)
                Case vbCr, vbLf
                    If Not bLongComment Then
                        bStartComment = False
                        bComment = False
                    End If
                Case vbTab, " ", "(", ")"

                Case "/"
                    If Not bLongComment Then
                        If bStartComment Then
                            bStartComment = False
                            bComment = True
                        Else
                            bStartComment = True
                            bComment = False
                            bLongComment = False
                        End If
                    Else
                        If bStartComment Then
                            bLongComment = False
                            bStartComment = False
                            bComment = False
                        End If
                    End If
                Case "*"
                    If bStartComment Then
                        bStartComment = False
                        bComment = True
                        bLongComment = True
                    Else
                        bStartComment = True
                    End If

                Case Else
                    If Not bComment Then
                        Exit While
                    End If
            End Select

            index += 1
        End While
    End Sub

End Class

Public Module Generic
    Public Function ParseJsonStr(ByVal JsonStr As String) As JsonElement
        Dim a As New JsonParser
        a.OpenJSON(JsonStr)
        Return a.Items
    End Function
    Public Function ParseJsonFile(ByVal JsonFile As String) As JsonElement
        Dim a As New JsonParser
        a.Open(JsonFile)
        Return a.Items
    End Function
End Module