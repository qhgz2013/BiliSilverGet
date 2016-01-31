Namespace Utils
    Namespace NetUtils

        Public Module Constants
            '默认接受数据流类型
            Public Const DEFAULT_ACCEPT_ENCODING As String = "gzip,deflate"
            '默认接受文本类型
            Public Const DEFAULT_ACCEPT_TYPE As String = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
            '默认请求方法
            Public Const DEFAULT_GET_METHOD As String = "GET"
            Public Const DEFAULT_POST_METHOD As String = "POST"
            '默认超时时间(ms)
            Public Const DEFAULT_TIMEOUT As Integer = 60000
            '默认代理URL
            Public Const DEFAULT_PROXY_URL As String = ""
            '默认User Agent
            Public Const DEFAULT_USER_AGENT As String = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36"
            '默认发送数据类型
            Public Const DEFAULT_CONTENT_TYPE As String = "application/x-www-form-urlencoded; charset=" & DEFAULT_ENCODING
            '默认接受语言
            Public Const DEFAULT_ACCEPT_LANGUAGE As String = "zh/cn,zh,en"
            '默认Set-Cookie的响应头标识符
            Public Const DEFAULT_SETCOOKIE_HEADER As String = "Set-Cookie"
            '默认编码类型
            Public Const DEFAULT_ENCODING As String = "utf-8"

            '默认连接重试次数
            Public Const DEFAULT_RETRY_TIMES As Integer = 3
            '默认重试等待时间
            Public Const DEFAULT_RETRY_DELAY As Integer = 0

            '默认保存cookie的文件名
            Public Const DEFAULT_COOKIE_FILE As String = "cookie.dat"
        End Module
        Public Module Variables
            Public Property DefaultCookieContainer As System.Net.CookieContainer
            Public Delegate Sub test()
            Sub New()
                DefaultCookieContainer = New Net.CookieContainer(10000, 500, 65535)
                End Sub
        End Module
        Public Module Functions
            ''' <summary>
            ''' 将字符串转为等价的符合URL转义字符串
            ''' </summary>
            ''' <param name="s">字符串</param>
            ''' <param name="Upper">转义字符大写</param>
            ''' <returns>URL转义字符串</returns>
            ''' <remarks>Debug pass, v1.0, last updated: 20151121</remarks>
            Public Function ToURLCharacter(ByVal s As String, Optional ByVal Upper As Boolean = True) As String
                Dim b As New System.Text.StringBuilder
                For Each c As Char In s
                    Select Case c
                        Case " "c
                            b.Append("%20")
                        Case """"c
                            b.Append("%22")
                        Case "#"c
                            b.Append("%23")
                        Case "%"c
                            b.Append("%25")
                        Case "&"c
                            b.Append("%26")
                        Case "("c
                            b.Append("%28")
                        Case ")"c
                            b.Append("%29")
                        Case "+"c
                            b.Append(If(Upper, "%2B", "%2b"))
                        Case ","c
                            b.Append(If(Upper, "%2C", "%2c"))
                        Case "/"c
                            b.Append(If(Upper, "%2F", "%2f"))
                        Case ":"c
                            b.Append(If(Upper, "%3A", "%3a"))
                        Case ";"c
                            b.Append(If(Upper, "%3B", "%3b"))
                        Case "<"c
                            b.Append(If(Upper, "%3C", "%3c"))
                        Case "="c
                            b.Append(If(Upper, "%3D", "%3d"))
                        Case ">"c
                            b.Append(If(Upper, "%3E", "%3e"))
                        Case "?"c
                            b.Append(If(Upper, "%3F", "%3f"))
                        Case "@"c
                            b.Append("%40")
                        Case "\"c
                            b.Append(If(Upper, "%5C", "%5c"))
                        Case "|"c
                            b.Append(If(Upper, "%7C", "%7c"))
                        Case Else
                            b.Append(c)
                    End Select
                Next
                Return b.ToString
            End Function
            ''' <summary>
            ''' 将URL转义字符串转为字符串
            ''' </summary>
            ''' <param name="s">URL转义字符串</param>
            ''' <returns>字符串</returns>
            ''' <remarks>Debug pass, v1.0, last updated: 20151121</remarks>
            Public Function FromURLCharacter(ByVal s As String) As String
                Dim n As New System.Text.StringBuilder
                For i As Integer = 0 To s.Length - 1
                    If s(i) <> "%" Then
                        n.Append(s(i))
                    Else
                        n.Append(Chr(CDec("&H" & s(i + 1) & s(i + 2))))
                        i += 2
                    End If
                Next
                Return n.ToString
            End Function
            Public Sub SaveCookie(Optional ByVal file As String = DEFAULT_COOKIE_FILE)
                Dim stream As System.IO.Stream = System.IO.File.Create(file)
                Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
                formatter.Serialize(stream, DefaultCookieContainer)
                stream.Close()
            End Sub
            Public Sub LoadCookie(Optional ByVal file As String = DEFAULT_COOKIE_FILE)
                If System.IO.File.Exists(file) = False Then Return
                Dim stream As System.IO.Stream = System.IO.File.Open(file, IO.FileMode.Open)
                Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
                DefaultCookieContainer = formatter.Deserialize(stream)
                stream.Close()
            End Sub
        End Module
        ''' <summary>
        ''' URL附加参数
        ''' </summary>
        ''' <remarks>Debug pass, v1.0, last updated: 20151121</remarks>
        Public Class Parameters
            Implements ICollection(Of KeyValuePair(Of String, String))

            Private _d As List(Of KeyValuePair(Of String, String))
            Public Sub New()
                _d = New List(Of KeyValuePair(Of String, String))
            End Sub
            Public Sub Add(Of T)(ByVal name As String, ByVal val As T)
                _d.Add(New KeyValuePair(Of String, String)(name, val.ToString))
            End Sub
            Public Function Remove(ByVal name As String) As Boolean
                For i As Integer = 0 To _d.Count
                    If _d(i).Key = name Then
                        _d.RemoveAt(i)
                        Return True
                    End If
                Next
                Return False
            End Function
            Public Function Contains(ByVal name As String) As Boolean
                For i As Integer = 0 To _d.Count
                    If _d(i).Key = name Then
                        Return True
                    End If
                Next
                Return False
            End Function
            Public Sub SortParameters()
                Dim n As New List(Of KeyValuePair(Of String, String))

                '第一次用Linq  = = 感觉SQL白学了，有种被欺骗的感觉

                'Dim test_linq = From k In _d Order By k.Key Ascending

                'For Each e In test_linq
                For Each e As KeyValuePair(Of String, String) In From k In _d Order By k.Key Ascending
                    n.Add(e)
                Next
                _d = n
            End Sub
            Public Sub SortParametersDsc()
                Dim n As New List(Of KeyValuePair(Of String, String))
                For Each e As KeyValuePair(Of String, String) In From k In _d Order By k.Key Descending
                    n.Add(e)
                Next
                _d = n
            End Sub
            Public Overrides Function ToString() As String
                Return "{Parameters}"
            End Function
            Public Function BuildURLQuery() As String
                Dim sb As New System.Text.StringBuilder
                For Each e As KeyValuePair(Of String, String) In _d
                    sb.Append(e.Key)
                    sb.Append("="c)
                    sb.Append(e.Value)
                    sb.Append("&"c)
                Next
                sb.Remove(sb.Length - 1, 1)
                Return sb.ToString
            End Function

            Public Sub Add(item As KeyValuePair(Of String, String)) Implements ICollection(Of KeyValuePair(Of String, String)).Add
                _d.Add(item)
            End Sub

            Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, String)).Clear
                _d.Clear()
            End Sub

            Public Function Contains(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Contains
                Return _d.Contains(item)
            End Function

            Public Sub CopyTo(array() As KeyValuePair(Of String, String), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, String)).CopyTo

            End Sub

            Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, String)).Count
                Get
                    Return _d.Count
                End Get
            End Property

            Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).IsReadOnly
                Get
                    Return False
                End Get
            End Property

            Public Function Remove(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Remove
                Return _d.Remove(item)
            End Function

            Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String)) Implements IEnumerable(Of KeyValuePair(Of String, String)).GetEnumerator
                Return _d.GetEnumerator
            End Function

            Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
                Return _d.GetEnumerator
            End Function
        End Class
        Public Class NetStream
            Public Property HTTP_Request As System.Net.HttpWebRequest
            Public Property HTTP_Response As System.Net.HttpWebResponse
            Public Property Stream As System.IO.Stream
            Public Property UseCookie As Boolean
            Public Property Proxy As System.Net.WebProxy
            Public Property Timeout As Integer
            Public Property AcceptEncoding As String
            Public Property AcceptLanguage As String
            Public Property AcceptType As String
            Public Property UserAgent As String
            Public Property ContentType As String
            Public Property RetryTimes As Integer
            Public Property RetryDelay As Integer

            Private Function parseCookie(ByVal header As String, ByVal defaultDomain As String) As Net.CookieCollection
                Dim ret As New Net.CookieCollection
                Dim i As Integer = 0

                While i < Len(header)
                    skipChar(header, i)
                    'read cookie name
                    Dim name As String = ""
                    While i < Len(header) AndAlso header(i) <> "="c
                        name &= header(i)
                        i += 1
                    End While

                    i += 1

                    'read cookie value [ps:没有试过parse带有";"的value 报错我可不管啊
                    Dim value As String = ""
                    While i < Len(header) AndAlso header(i) <> ";"c AndAlso header(i) <> ","c
                        value &= header(i)
                        i += 1
                    End While
                    i += 1

                    Dim has_exp_time As Boolean = False, has_max_age As Boolean = False, has_path As Boolean = False, has_domain As Boolean = False
                    Dim exp_time As Date = New Date(1900, 1, 1, 0, 0, 0)
                    Dim max_age As Integer
                    Dim path As String = ""
                    Dim domain As String = ""
                    Dim http_only As Boolean = False
                    Dim version As String = ""
                    While i < Len(header) AndAlso header(i) <> ","c

                        skipChar(header, i)
                        'expire time
                        If Mid(header, i + 1, 7).ToLower = "expires" Then
                            has_exp_time = True
                            i += 8 'skip "expires="
                            Dim days As String = Mid(header, i + 1, 3)
                            i += 4 'skip day("Sun" "Mon" etc)
                            skipChar(header, i)

                            'date, format="dd-MMM-yy HH:mm:ss [GMT]"
                            exp_time = exp_time.AddDays(Integer.Parse(Mid(header, i + 1, 2)) - 1)
                            i += 3
                            Select Case Mid(header, i + 1, 3)
                                Case "Jan"
                                    exp_time = exp_time.AddMonths(0)
                                Case "Feb"
                                    exp_time = exp_time.AddMonths(1)
                                Case "Mar"
                                    exp_time = exp_time.AddMonths(2)
                                Case "Apr"
                                    exp_time = exp_time.AddMonths(3)
                                Case "May"
                                    exp_time = exp_time.AddMonths(4)
                                Case "Jun"
                                    exp_time = exp_time.AddMonths(5)
                                Case "Jul"
                                    exp_time = exp_time.AddMonths(6)
                                Case "Aug"
                                    exp_time = exp_time.AddMonths(7)
                                Case "Sep"
                                    exp_time = exp_time.AddMonths(8)
                                Case "Oct"
                                    exp_time = exp_time.AddMonths(9)
                                Case "Nov"
                                    exp_time = exp_time.AddMonths(10)
                                Case "Dec"
                                    exp_time = exp_time.AddMonths(11)
                                Case Else
                                    Throw New Exception("Invalid Month Name: " & Mid(header, i + 1, 2))
                            End Select
                            i += 4
                            If IsNumeric(Mid(header, i + 1, 4)) Then
                                exp_time = exp_time.AddYears(Integer.Parse(Mid(header, i + 1, 4)) - 1900)
                                i += 4
                            Else
                                exp_time = exp_time.AddYears(Integer.Parse(Mid(header, i + 1, 2)))
                                i += 2
                                While Mid(exp_time.DayOfWeek.ToString, 1, 3) <> days
                                    exp_time = exp_time.AddYears(100)
                                End While
                            End If

                            skipChar(header, i)
                            exp_time = exp_time.AddHours(Integer.Parse(Mid(header, i + 1, 2)))
                            i += 3
                            exp_time = exp_time.AddMinutes(Integer.Parse(Mid(header, i + 1, 2)))
                            i += 3
                            exp_time = exp_time.AddSeconds(Integer.Parse(Mid(header, i + 1, 2)))
                            i += 2
                            skipChar(header, i)
                            If Mid(header, i + 1, 3).ToUpper = "GMT" Then
                                i += 3
                                skipChar(header, i)
                            End If
                            If i < Len(header) AndAlso header(i) <> ";" AndAlso header(i) <> ","c Then Throw New Exception("Invalid splitter after expires")
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1

                            'max age
                        ElseIf Mid(header, i + 1, 7).ToLower = "max-age" Then
                            has_max_age = True
                            i += 8
                            Dim j As Integer = i
                            While (IsNumeric(header(j)))
                                j += 1
                            End While
                            max_age = Integer.Parse(Mid(header, i + 1, j - i))
                            i = j
                            skipChar(header, i)
                            If i < Len(header) AndAlso header(i) <> ";" AndAlso header(i) <> ","c Then Throw New Exception("Invalid splitter after expires")
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1

                            'path
                        ElseIf Mid(header, i + 1, 4).ToLower = "path" Then
                            has_path = True
                            i += 5
                            While i < Len(header) AndAlso header(i) <> ";"c AndAlso header(i) <> ","c
                                path &= header(i)
                                i += 1
                            End While
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1

                            'domain 
                        ElseIf Mid(header, i + 1, 6).ToLower = "domain" Then
                            has_domain = True
                            i += 7
                            While i < Len(header) AndAlso header(i) <> ";"c AndAlso header(i) <> ","c
                                domain &= header(i)
                                i += 1
                            End While
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1
                            'httponly
                        ElseIf Mid(header, i + 1, 8).ToLower = "httponly" Then
                            http_only = True
                            i += 8
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1
                        ElseIf Mid(header, i + 1, 7).ToLower = "version" Then
                            i += 8
                            While i < Len(header) AndAlso header(i) <> ";"c AndAlso header(i) <> ","c
                                version &= header(i)
                                i += 1
                            End While
                            If i < Len(header) AndAlso header(i) = ";" Then i += 1
                        End If
                    End While
                    If i >= Len(header) OrElse header(i) = "," Then
                        i += 1
                        Dim c As New Net.Cookie(name, value, If(has_path, path, "/"), If(has_domain, domain, defaultDomain))
                        'c.Domain = domain
                        c.HttpOnly = http_only
                        If has_exp_time Then c.Expires = exp_time 'Else c.Expires = Now
                        ret.Add(c)
                    End If
                End While


                Return ret
            End Function
            Private Sub skipChar(ByRef str As String, ByRef index As Integer)
                While str(index) = " "
                    index += 1
                End While
            End Sub

            Public Sub New(Optional ByVal useCookie As Boolean = True, Optional ByVal timeout As Integer = DEFAULT_TIMEOUT, Optional ByVal RetryTimes As Integer = DEFAULT_RETRY_TIMES, Optional ByVal RetryDelay As Integer = DEFAULT_RETRY_DELAY, Optional ByVal proxy As String = DEFAULT_PROXY_URL)
                Me.UseCookie = useCookie
                Me.Timeout = timeout
                Me.RetryTimes = RetryTimes
                Me.RetryDelay = RetryDelay
                If proxy IsNot Nothing AndAlso proxy.Length Then Me.Proxy = New System.Net.WebProxy(proxy)

                AcceptEncoding = DEFAULT_ACCEPT_ENCODING
                AcceptLanguage = DEFAULT_ACCEPT_LANGUAGE
                AcceptType = DEFAULT_ACCEPT_TYPE
                UserAgent = DEFAULT_USER_AGENT
                ContentType = DEFAULT_CONTENT_TYPE
            End Sub
            Public Sub HttpGet(ByVal url As String, Optional ByVal headerParam As Parameters = Nothing, Optional ByVal urlParam As Parameters = Nothing)
                Dim cur_times As Integer = 0
                Do
                    Try
                        'url params
                        Dim postUrl As String = url
                        If urlParam IsNot Nothing Then
                            postUrl &= "?" & urlParam.BuildURLQuery
                        End If

                        HTTP_Request = System.Net.HttpWebRequest.Create(postUrl)
                        'header params
                        HTTP_Request.Headers(System.Net.HttpRequestHeader.AcceptEncoding) = AcceptEncoding
                        HTTP_Request.Headers(System.Net.HttpRequestHeader.AcceptLanguage) = AcceptLanguage

                        'appends header
                        If headerParam IsNot Nothing Then
                            For Each e As KeyValuePair(Of String, String) In headerParam
                                HTTP_Request.Headers.Add(e.Key, e.Value)
                            Next
                        End If

                        'accept type
                        HTTP_Request.Accept = AcceptType
                        'user agent
                        HTTP_Request.UserAgent = UserAgent

                        'method (get / post)
                        HTTP_Request.Method = DEFAULT_GET_METHOD
                        HTTP_Request.Timeout = Timeout

                        HTTP_Request.ContentLength = 0

                        HTTP_Request.Date = Now

                        'proxy
                        If Proxy IsNot Nothing Then HTTP_Request.Proxy = Proxy

                        'cookie
                        If UseCookie Then HTTP_Request.CookieContainer = Variables.DefaultCookieContainer


                        '获取http响应，然而并没有线程调用
                        HTTP_Response = HTTP_Request.GetResponse

                        'Variables.DefaultCookieContainer.Add(HTTP_Response.ResponseUri, parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))
                        Variables.DefaultCookieContainer.Add(parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))

                        Select Case HTTP_Response.ContentEncoding
                            Case "gzip"
                                Stream = New System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case "deflate"
                                Stream = New System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case Else
                                Stream = HTTP_Response.GetResponseStream
                        End Select

                        Exit Do
                    Catch ex As Exception
                        cur_times += 1
                        If RetryTimes >= 0 AndAlso cur_times > RetryTimes Then Throw ex
                        If RetryDelay > 0 Then Threading.Thread.Sleep(RetryDelay)
                    End Try

                Loop

            End Sub

            Public Sub HttpPost(ByVal url As String, ByVal postData As Byte(), Optional ByVal headerParam As Parameters = Nothing, Optional urlParam As Parameters = Nothing)
                Dim cur_times As Integer = 0

                While RetryTimes < 0 OrElse cur_times <= RetryTimes
                    Try
                        Dim postUrl As String = url
                        'url params
                        If urlParam IsNot Nothing Then
                            postUrl &= "?" & urlParam.BuildURLQuery
                        End If

                        HTTP_Request = System.Net.HttpWebRequest.Create(postUrl)
                        'header params
                        HTTP_Request.Headers(System.Net.HttpRequestHeader.AcceptEncoding) = AcceptEncoding
                        HTTP_Request.Headers(System.Net.HttpRequestHeader.AcceptLanguage) = AcceptLanguage

                        'appends header
                        If headerParam IsNot Nothing Then
                            For Each e As KeyValuePair(Of String, String) In headerParam
                                HTTP_Request.Headers.Add(e.Key, e.Value)
                            Next
                        End If

                        'accept type
                        HTTP_Request.Accept = AcceptType
                        'user agent
                        HTTP_Request.UserAgent = UserAgent

                        'method (get / post)
                        HTTP_Request.Method = DEFAULT_POST_METHOD
                        HTTP_Request.Timeout = Timeout

                        HTTP_Request.Date = Now

                        'proxy
                        If Proxy IsNot Nothing Then HTTP_Request.Proxy = Proxy

                        'cookie
                        If UseCookie Then HTTP_Request.CookieContainer = Variables.DefaultCookieContainer

                        'post data
                        HTTP_Request.ContentType = ContentType
                        HTTP_Request.ContentLength = postData.Length
                        HTTP_Request.GetRequestStream.Write(postData, 0, postData.Length)

                        '获取http响应，然而并没有线程调用
                        HTTP_Response = HTTP_Request.GetResponse

                        'Variables.DefaultCookieContainer.Add(HTTP_Response.ResponseUri, parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))
                        Variables.DefaultCookieContainer.Add(parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))

                        Select Case HTTP_Response.ContentEncoding
                            Case "gzip"
                                Stream = New System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case "deflate"
                                Stream = New System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case Else
                                Stream = HTTP_Response.GetResponseStream
                        End Select

                        Exit While
                    Catch ex As Exception
                        cur_times += 1
                        If RetryTimes >= 0 AndAlso cur_times > RetryTimes Then Throw ex
                        If RetryDelay > 0 Then Threading.Thread.Sleep(RetryDelay)
                    End Try

                End While

            End Sub
            Public Sub HttpPost(ByVal url As String, ByVal postParam As Parameters, Optional ByVal headerParam As Parameters = Nothing, Optional urlParam As Parameters = Nothing)
                HttpPost(url, System.Text.Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(postParam.BuildURLQuery), headerParam, urlParam)
            End Sub
            Protected Overrides Sub Finalize()
                If Stream IsNot Nothing Then Stream.Close()
                If HTTP_Response IsNot Nothing Then HTTP_Response.Close()
                MyBase.Finalize()
            End Sub
            Public Sub Close()
                If Stream IsNot Nothing Then Stream.Close()
                If HTTP_Response IsNot Nothing Then HTTP_Response.Close()
            End Sub
        End Class
    End Namespace
End Namespace