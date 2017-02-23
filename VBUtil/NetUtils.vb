Imports System.IO

Namespace Utils
    Namespace NetUtils

        Public Module Constants
            'key name:

            '默认请求方法
            Public Const DEFAULT_GET_METHOD As String = "GET"
            Public Const DEFAULT_POST_METHOD As String = "POST"
            Public Const DEFAULT_HEAD_METHOD As String = "HEAD"
            '默认Set-Cookie的响应头标识符
            Public Const DEFAULT_SETCOOKIE_HEADER As String = "Set-Cookie"
            '其他默认的header属性
            Public Const STR_ACCEPT_ENCODING As String = "Accept-Encoding"
            Public Const STR_HOST As String = "Host"
            Public Const STR_CONNECTION As String = "Connection"
            Public Const STR_ACCEPT As String = "Accept"
            Public Const STR_USER_AGENT As String = "User-Agent"
            Public Const STR_REFERER As String = "Referer"
            Public Const STR_ACCEPT_LANGUAGE As String = "Accept-Language"
            Public Const STR_PRAGMA As String = "Pragma"
            Public Const STR_CONTENT_TYPE As String = "Content-Type"
            Public Const STR_CONTENT_LENGTH As String = "Content-Length"
            Public Const STR_ORIGIN As String = "Origin"
            Public Const STR_COOKIE As String = "Cookie"
            Public Const STR_EXPECT As String = "Expect"
            Public Const STR_DATE As String = "Date"
            Public Const STR_IF_MODIFIED_SINCE As String = "If-Modified-Since"
            Public Const STR_RANGE As String = "Range"
            Public Const STR_TRANSFER_ENCODING As String = "Transfer-Encoding"

            Public Const STR_CONNECTION_KEEP_ALIVE As String = "keep-alive"
            Public Const STR_CONNECTION_CLOSE As String = "close"

            Public Const STR_GZIP_ENCODING As String = "gzip"
            Public Const STR_DEFLATE_ENCODING As String = "deflate"
            'value

            '默认接受数据流类型
            Public Const DEFAULT_ACCEPT_ENCODING As String = STR_GZIP_ENCODING & "," & STR_DEFLATE_ENCODING
            '默认接受文本类型
            Public Const DEFAULT_ACCEPT As String = "*/*" '"text/html,application/json,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"

            '默认超时时间(ms)
            Public Const DEFAULT_TIMEOUT As Integer = Integer.MaxValue
            Public Const DEFAULT_READ_WRITE_TIMEOUT As Integer = 60000
            '默认代理URL
            Public Const DEFAULT_PROXY_URL As String = ""
            '默认User Agent
            Public Const DEFAULT_USER_AGENT As String = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36"
            '默认发送数据类型
            Public Const DEFAULT_PARAM_CONTENT_TYPE As String = "application/x-www-form-urlencoded; charset=" & DEFAULT_ENCODING
            Public Const DEFAULT_BINARY_CONTENT_TYPE As String = "application/octet-stream"
            '默认接受语言
            Public Const DEFAULT_ACCEPT_LANGUAGE As String = "zh/cn,zh,en"
            '默认编码类型
            Public Const DEFAULT_ENCODING As String = "utf-8"

            '默认连接重试次数
            Public Const DEFAULT_RETRY_TIMES As Integer = 1
            '默认重试等待时间
            Public Const DEFAULT_RETRY_DELAY As Integer = 0

            '默认保存cookie的文件名
            Public Const DEFAULT_COOKIE_FILE As String = "cookie.dat"
        End Module
        '默认保存cookie的模块
        Public Module [Global]
            Public Property DefaultCookieContainer As System.Net.CookieContainer
            Sub New()
                DefaultCookieContainer = New Net.CookieContainer()
            End Sub
            '保存cookie到文件中
            Public Sub SaveCookie(Optional ByVal file As String = DEFAULT_COOKIE_FILE)
                Dim stream As System.IO.Stream = System.IO.File.Create(file)
                Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
                formatter.Serialize(stream, DefaultCookieContainer)
                stream.Close()
            End Sub
            '从文件中读取cookie
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
                Return "{" & BuildURLQuery() & "}"
            End Function
            Public Function BuildURLQuery(Optional ByVal EnableURLEncode As Boolean = True) As String
                Dim sb As New System.Text.StringBuilder
                For Each e As KeyValuePair(Of String, String) In _d
                    sb.Append(e.Key)
                    sb.Append("="c)
                    If EnableURLEncode Then
                        sb.Append(System.Uri.EscapeDataString(e.Value))
                    Else
                        sb.Append(e.Value)
                    End If
                    sb.Append("&"c)
                Next
                sb.Remove(sb.Length - 1, 1)
                Return sb.ToString
            End Function

            Public Property Items(ByVal name As String) As String
                Get
                    For Each e As KeyValuePair(Of String, String) In _d
                        If e.Key = name Then
                            Return e.Value
                        End If
                    Next
                    Return ""
                End Get
                Set(value As String)
                    Dim i As Integer = _d.FindIndex(Function(x)
                                                        If x.Key = name Then Return True Else Return False
                                                    End Function)
                    If i = -1 Then Throw New KeyNotFoundException()
                    _d(i) = New KeyValuePair(Of String, String)(name, value)
                End Set
            End Property
            Public Property Items(ByVal index As Integer) As String
                Get
                    Return _d(index).Value
                End Get
                Set(value As String)
                    _d(index) = New KeyValuePair(Of String, String)(_d(index).Key, value)
                End Set
            End Property
            'Impleted functions
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
                _d.CopyTo(array, arrayIndex)
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
            Public Property HTTP_Request As System.Net.HttpWebRequest 'HTTP请求
            Public Property HTTP_Response As System.Net.HttpWebResponse 'HTTP响应
            Public Property Stream As System.IO.Stream 'HTTP响应的数据流
            Public Property UseCookie As Boolean '是否使用cookie
            Public Property Proxy As System.Net.WebProxy '代理
            Public Property Timeout As Integer '超时时间(ms)
            Public Property AcceptEncoding As String '接受的编码类型(utf-8)
            Public Property AcceptLanguage As String '接受的语言(zh-cn/en)
            Public Property Accept As String '接受的文件mime类型 */*
            Public Property UserAgent As String 'user agent
            Public Property ContentType As String '文件类型(application/x-www-form-urlencoded)
            Public Property RetryTimes As Integer '重试次数
            Public Property RetryDelay As Integer '重试延时(ms)
            Public Property ReadWriteTimeout As Integer '读写超时

            ''' <summary>
            ''' 解析set cookie的http response header
            ''' </summary>
            ''' <param name="header">含set cookie的header字符串</param>
            ''' <param name="defaultDomain">默认的域名</param>
            ''' <returns>与该字符串等价的cookie collection</returns>
            ''' <remarks></remarks>
            Public Shared Function parseCookie(ByVal header As String, ByVal defaultDomain As String) As Net.CookieCollection
                Dim ret As New Net.CookieCollection
                Dim i As Integer = 0
                Dim arg As New Dictionary(Of String, String)

                While i < Len(header)
                    skipChar(header, i)
                    'read cookie name
                    Dim name As String = parseCookieValue(header, i, ";;name")
                    i += 1
                    skipChar(header, i)
                    'read cookie value [ps:没有试过parse带有";"的value 报错我可不管啊
                    Dim value As String = parseCookieValue(header, i, ";;value")
                    While i < Len(header) AndAlso header(i) <> ","c
                        i += 1
                        skipChar(header, i)
                        Dim pkey As String = parseCookieValue(header, i, ";;name").ToLower
                        i += 1
                        skipChar(header, i)
                        Dim pvalue As String = parseCookieValue(header, i, pkey)
                        skipChar(header, i)
                        arg.Add(pkey, pvalue)
                    End While
                    If i >= Len(header) OrElse header(i) = "," Then
                        i += 1
                        Dim skipflg As Boolean = False
                        If Not arg.ContainsKey("path") Then skipflg = True
                        Dim c As New Net.Cookie(name, value, arg("path"), If(arg.ContainsKey("domain"), arg("domain"), defaultDomain))
                        c.HttpOnly = arg.ContainsKey("httponly")

                        If arg.ContainsKey("max-age") Then
                            c.Expires = Now.AddSeconds(Integer.Parse(arg("max-age")))
                        ElseIf arg.ContainsKey("maxage") Then
                            c.Expires = Now.AddSeconds(Integer.Parse(arg("maxage")))
                        ElseIf arg.ContainsKey("expires") Then
                            c.Expires = parseCookieExpireTime(arg("expires"))
                        Else
                            skipflg = True
                        End If
                        arg.Clear()
                        If (Not skipflg) Then ret.Add(c)
                    End If
                End While


                Return ret
            End Function
            ''' <summary>
            ''' 解析cookie的值
            ''' </summary>
            ''' <param name="header">含set cookie的header字符串</param>
            ''' <param name="i">当前解析的字符串下标</param>
            ''' <returns>cookie的值</returns>
            ''' <remarks></remarks>
            Private Shared Function parseCookieValue(ByVal header As String, ByRef i As Integer, ByVal propertyName As String) As String
                Dim value As String = ""
                Dim limitStr As String
                If propertyName = ";;name" Then
                    limitStr = ";,="
                ElseIf propertyName = ";;value" Then
                    limitStr = ";,"
                ElseIf propertyName = "expires" Then
                    limitStr = ";="
                Else
                    limitStr = ";,"
                End If
                While i < Len(header) AndAlso InStr(limitStr, header(i)) = 0
                    value &= header(i)
                    i += 1
                End While
                value = value.Trim("""")
                Return value
            End Function
            Private Shared Function parseCookieExpireTime(ByVal str As String) As Date
                'format: [Day], dd-MMM-yy[yy] HH:mm:ss [GMT[+/-x]]
                Dim i As Integer = 4
                skipChar(str, i)
                Dim day As Integer = Integer.Parse(Mid(str, i + 1, 2))
                i += 3
                Dim smonth As String = Mid(str, i + 1, 3)
                i += 4
                Dim csmonth As String = "JanFebMarAprMayJunJulAugSepOctNovDec"
                Dim month As Integer = (InStr(csmonth, smonth, CompareMethod.Text) + 2) / 3
                If month > 12 Or month < 1 Then Throw New ArgumentException("Cookie的月份数值出错")
                Dim year As Integer
                If IsNumeric(Mid(str, i + 1, 4)) Then
                    year = Integer.Parse(Mid(str, i + 1, 4))
                    i += 5
                Else
                    year = Integer.Parse(Mid(str, i + 1, 2)) + Int(Now.Year / 100) * 100
                    i += 3
                End If
                skipChar(str, i)
                Dim hour As Integer = Integer.Parse(Mid(str, i + 1, 2))
                i += 3
                Dim minute As Integer = Integer.Parse(Mid(str, i + 1, 2))
                i += 3
                Dim second As Integer = Integer.Parse(Mid(str, i + 1, 2))
                i += 3
                skipChar(str, i)
                'Dim globalHourOffset As Integer = 0
                Dim gmt_marker As Boolean = False
                If i < str.Length Then
                    Dim marker As String = Mid(str, i + 1, 3)
                    If String.Equals(marker, "GMT", StringComparison.CurrentCultureIgnoreCase) Then gmt_marker = True
                    i += 4
                End If
                Dim d As New Date(0, If(gmt_marker, DateTimeKind.Utc, DateTimeKind.Unspecified))
                d = d.AddYears(year - 1).AddMonths(month - 1).AddDays(day - 1)
                d = d.AddHours(hour).AddMinutes(minute).AddSeconds(second)
                Return d
            End Function
            ''' <summary>
            ''' 跳过空白字符
            ''' </summary>
            ''' <param name="str">cookie header</param>
            ''' <param name="index">下标</param>
            ''' <remarks></remarks>
            Private Shared Sub skipChar(ByRef str As String, ByRef index As Integer)
                While index < str.Length AndAlso str(index) = " "c
                    index += 1
                End While
            End Sub

            Public Sub New(Optional ByVal useCookie As Boolean = True, Optional ByVal Timeout As Integer = DEFAULT_READ_WRITE_TIMEOUT, Optional ByVal RetryTimes As Integer = DEFAULT_RETRY_TIMES, Optional ByVal RetryDelay As Integer = DEFAULT_RETRY_DELAY, Optional ByVal proxy As String = DEFAULT_PROXY_URL)
                Me.UseCookie = useCookie
                Me.ReadWriteTimeout = DEFAULT_READ_WRITE_TIMEOUT
                Me.Timeout = Timeout
                Me.RetryTimes = RetryTimes
                Me.RetryDelay = RetryDelay
                If proxy IsNot Nothing AndAlso proxy.Length Then Me.Proxy = New System.Net.WebProxy(proxy)

                AcceptEncoding = DEFAULT_ACCEPT_ENCODING
                AcceptLanguage = DEFAULT_ACCEPT_LANGUAGE
                Accept = DEFAULT_ACCEPT
                UserAgent = DEFAULT_USER_AGENT
                ContentType = DEFAULT_PARAM_CONTENT_TYPE
            End Sub
            ''' <summary>
            ''' 新的HTTP GET请求，HTTP响应的数据流指定为本类的Stream属性，使用后务必调用Close()释放资源
            ''' </summary>
            ''' <param name="url">URL</param>
            ''' <param name="headerParam">往HTTP请求头里塞的参数</param>
            ''' <param name="urlParam">往URL里塞的参数(就是x.com/test?key=value这种类型的)</param>
            ''' <param name="range">返回数据流的范围(默认为-1即忽略，注意一些http请求不支持AddRange功能，修改该值可能会引发异常</param>
            ''' <remarks></remarks>
            Public Sub HttpGet(ByVal url As String, Optional ByVal headerParam As Parameters = Nothing, Optional ByVal urlParam As Parameters = Nothing, Optional ByVal range As Integer = -1)
                Dim cur_times As Integer = 0
                Do
                    Try
                        '构建URL参数
                        Dim postUrl As String = url
                        If urlParam IsNot Nothing Then
                            postUrl &= "?" & urlParam.BuildURLQuery
                        End If
                        '创建http request
                        HTTP_Request = System.Net.HttpWebRequest.Create(postUrl)
                        HTTP_Request.KeepAlive = True

                        '将headerParam中的参数添加到web request中
                        If headerParam IsNot Nothing Then
                            For Each e As KeyValuePair(Of String, String) In headerParam
                                Select Case e.Key
                                    Case STR_ACCEPT
                                        HTTP_Request.Accept = e.Value
                                    Case STR_CONNECTION
                                        Select Case e.Value
                                            Case STR_CONNECTION_KEEP_ALIVE
                                                'todo: 测试
                                                HTTP_Request.Connection = ""
                                                HTTP_Request.KeepAlive = True
                                            Case STR_CONNECTION_CLOSE
                                                HTTP_Request.Connection = Nothing
                                            Case Else
                                                Throw New ArgumentException("Connection属性无效")
                                        End Select
                                    Case STR_CONTENT_LENGTH
                                        HTTP_Request.ContentLength = e.Value
                                    Case STR_CONTENT_TYPE
                                        HTTP_Request.ContentType = e.Value
                                    Case STR_EXPECT
                                        HTTP_Request.Expect = e.Value
                                    Case STR_DATE
                                        HTTP_Request.Date = e.Value
                                    Case STR_HOST
                                        HTTP_Request.Host = e.Value
                                    Case STR_IF_MODIFIED_SINCE
                                        HTTP_Request.IfModifiedSince = e.Value
                                    Case STR_RANGE
                                        Dim rangesplit() As String = e.Value.Split("-")
                                        If rangesplit.Length <> 2 Then Throw New ArgumentException("Range无法识别")
                                        If String.IsNullOrEmpty(rangesplit(0)) Then
                                            If String.IsNullOrEmpty(rangesplit(1)) Then
                                                Throw New ArgumentNullException("Range为空")
                                            Else
                                                HTTP_Request.AddRange(-Long.Parse(rangesplit(1)))
                                            End If
                                        Else
                                            If String.IsNullOrEmpty(rangesplit(1)) Then
                                                HTTP_Request.AddRange(Long.Parse(rangesplit(0)))
                                            Else
                                                HTTP_Request.AddRange(Long.Parse(rangesplit(0)), Long.Parse(rangesplit(1)))
                                            End If
                                        End If
                                    Case STR_REFERER
                                        HTTP_Request.Referer = e.Value
                                    Case STR_TRANSFER_ENCODING
                                        HTTP_Request.TransferEncoding = e.Value
                                    Case STR_USER_AGENT
                                        HTTP_Request.UserAgent = e.Value
                                    Case Else
                                        HTTP_Request.Headers.Add(e.Key, e.Value)
                                End Select
                            Next
                        End If
                        '追加默认参数
                        Dim keyList As List(Of String) = HTTP_Request.Headers.AllKeys.ToList
                        If Not keyList.Contains(STR_ACCEPT) Then HTTP_Request.Accept = Accept
                        If Not keyList.Contains(STR_ACCEPT_ENCODING) Then HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding)
                        If Not keyList.Contains(STR_ACCEPT_LANGUAGE) Then HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage)
                        If Not keyList.Contains(STR_DATE) Then HTTP_Request.Date = Now
                        If Not keyList.Contains(STR_USER_AGENT) Then HTTP_Request.UserAgent = UserAgent
                        'If Not keyList.Contains(STR_CONNECTION) Then HTTP_Request.Connection = ""

                        'proxy
                        If Proxy IsNot Nothing Then HTTP_Request.Proxy = Proxy
                        'cookie
                        If UseCookie AndAlso Not keyList.Contains(STR_COOKIE) Then HTTP_Request.CookieContainer = DefaultCookieContainer

                        'method (get / post)
                        HTTP_Request.Method = DEFAULT_GET_METHOD
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeout
                        HTTP_Request.Timeout = Timeout

                        HTTP_Request.ContentLength = 0

                        HTTP_Request.Date = Now

                        'range
                        If range >= 0 Then
                            If keyList.Contains(STR_RANGE) Then Throw New InvalidOperationException("HTTP请求头中已包含Range参数，参数range冲突")
                            HTTP_Request.AddRange(range)
                        End If

                        '获取http响应(同步)
                        HTTP_Response = HTTP_Request.GetResponse

                        DefaultCookieContainer.Add(parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))

                        If HTTP_Response.StatusCode = Net.HttpStatusCode.OK Then
                            Select Case HTTP_Response.ContentEncoding
                                Case STR_GZIP_ENCODING
                                    Stream = New System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                                Case STR_DEFLATE_ENCODING
                                    Stream = New System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                                Case Else
                                    Stream = HTTP_Response.GetResponseStream
                            End Select
                        End If

                        Exit Do
                    Catch ex As Exception
                        cur_times += 1
                        If RetryTimes >= 0 AndAlso cur_times > RetryTimes Then Throw ex
                        If RetryDelay > 0 Then Threading.Thread.Sleep(RetryDelay)
                    End Try

                Loop

            End Sub
            ''' <summary>
            ''' 新的HTTP HEAD请求，无HTTP响应数据，使用后务必调用Close()释放资源
            ''' </summary>
            ''' <param name="url">URL</param>
            ''' <param name="headerParam">往HTTP请求头里塞的参数</param>
            ''' <param name="urlParam">往URL里塞的参数(就是x.com/test?key=value这种类型的)</param>
            Public Sub HttpHead(ByVal url As String, Optional ByVal headerParam As Parameters = Nothing, Optional ByVal urlParam As Parameters = Nothing)
                Dim cur_times As Integer = 0
                Do
                    Try
                        '构建URL参数
                        Dim postUrl As String = url
                        If urlParam IsNot Nothing Then
                            postUrl &= "?" & urlParam.BuildURLQuery
                        End If
                        '创建http request
                        HTTP_Request = System.Net.HttpWebRequest.Create(postUrl)

                        '将headerParam中的参数添加到web request中
                        If headerParam IsNot Nothing Then
                            For Each e As KeyValuePair(Of String, String) In headerParam
                                Select Case e.Key
                                    Case STR_ACCEPT
                                        HTTP_Request.Accept = e.Value
                                    Case STR_CONNECTION
                                        Select Case e.Value
                                            Case STR_CONNECTION_KEEP_ALIVE
                                                'todo: 测试
                                                HTTP_Request.Connection = ""
                                                HTTP_Request.KeepAlive = True
                                            Case STR_CONNECTION_CLOSE
                                                HTTP_Request.Connection = Nothing
                                            Case Else
                                                Throw New ArgumentException("Connection属性无效")
                                        End Select
                                    Case STR_CONTENT_LENGTH
                                        HTTP_Request.ContentLength = e.Value
                                    Case STR_CONTENT_TYPE
                                        HTTP_Request.ContentType = e.Value
                                    Case STR_EXPECT
                                        HTTP_Request.Expect = e.Value
                                    Case STR_DATE
                                        HTTP_Request.Date = e.Value
                                    Case STR_HOST
                                        HTTP_Request.Host = e.Value
                                    Case STR_IF_MODIFIED_SINCE
                                        HTTP_Request.IfModifiedSince = e.Value
                                    Case STR_RANGE
                                        Throw New InvalidDataException("HEAD请求中无法添加Range")
                                    Case STR_REFERER
                                        HTTP_Request.Referer = e.Value
                                    Case STR_TRANSFER_ENCODING
                                        HTTP_Request.TransferEncoding = e.Value
                                    Case STR_USER_AGENT
                                        HTTP_Request.UserAgent = e.Value
                                    Case Else
                                        HTTP_Request.Headers.Add(e.Key, e.Value)
                                End Select
                            Next
                        End If
                        '追加默认参数
                        Dim keyList As List(Of String) = HTTP_Request.Headers.AllKeys.ToList
                        If Not keyList.Contains(STR_ACCEPT) Then HTTP_Request.Accept = Accept
                        If Not keyList.Contains(STR_ACCEPT_ENCODING) Then HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding)
                        If Not keyList.Contains(STR_ACCEPT_LANGUAGE) Then HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage)
                        If Not keyList.Contains(STR_DATE) Then HTTP_Request.Date = Now
                        If Not keyList.Contains(STR_USER_AGENT) Then HTTP_Request.UserAgent = UserAgent

                        'proxy
                        If Proxy IsNot Nothing Then HTTP_Request.Proxy = Proxy
                        'cookie
                        If UseCookie AndAlso Not keyList.Contains(STR_COOKIE) Then HTTP_Request.CookieContainer = DefaultCookieContainer

                        'method (get / post)
                        HTTP_Request.Method = DEFAULT_HEAD_METHOD
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeout
                        HTTP_Request.Timeout = Timeout

                        HTTP_Request.ContentLength = 0

                        HTTP_Request.Date = Now

                        '获取http响应(同步)
                        HTTP_Response = HTTP_Request.GetResponse

                        DefaultCookieContainer.Add(parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))
                        'HEAD只返回http header，理应没有数据流
                        Stream = Nothing

                        Exit Do
                    Catch ex As Exception
                        cur_times += 1
                        If RetryTimes >= 0 AndAlso cur_times > RetryTimes Then Throw ex
                        If RetryDelay > 0 Then Threading.Thread.Sleep(RetryDelay)
                    End Try

                Loop
            End Sub
            ''' <summary>
            ''' 新的HTTP POST请求，HTTP响应的数据流指定为本类的Stream属性，使用后务必调用Close()释放资源
            ''' </summary>
            ''' <param name="url">URL</param>
            ''' <param name="postData">要POST的数据</param>
            ''' <param name="postContentType">要POST的数据的文件MIME类型，默认为application/octet-stream</param>
            ''' <param name="headerParam">往HTTP请求头里塞的参数</param>
            ''' <param name="urlParam">往URL里塞的参数(就是x.com/test?key=value这种类型的)</param>
            ''' <remarks></remarks>
            Public Sub HttpPost(ByVal url As String, ByVal postData As Byte(), Optional ByVal postContentType As String = DEFAULT_BINARY_CONTENT_TYPE, Optional ByVal headerParam As Parameters = Nothing, Optional urlParam As Parameters = Nothing)
                Dim stream As Stream = HttpPost(url, postData.Length, postContentType, headerParam, urlParam)
                stream.Write(postData, 0, postData.Length)
                stream.Close()
                HttpPostClose()
            End Sub
            ''' <summary>
            ''' 新的HTTP POST请求，HTTP响应的数据流指定为本类的Stream属性，使用后务必调用Close()释放资源
            ''' </summary>
            ''' <param name="url">URL</param>
            ''' <param name="postParam">要POST的参数</param>
            ''' <param name="postContentType">要POST的数据的文件MIME类型，默认为application/x-www-form-urlencoded类型</param>
            ''' <param name="headerParam">往HTTP请求头里塞的参数</param>
            ''' <param name="urlParam">往URL里塞的参数(就是x.com/test?key=value这种类型的)</param>
            ''' <remarks></remarks>
            Public Sub HttpPost(ByVal url As String, ByVal postParam As Parameters, Optional ByVal postContentType As String = DEFAULT_PARAM_CONTENT_TYPE, Optional ByVal headerParam As Parameters = Nothing, Optional ByVal urlParam As Parameters = Nothing)
                HttpPost(url, System.Text.Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(postParam.BuildURLQuery), postContentType, headerParam, urlParam)
            End Sub
            ''' <summary>
            ''' 新的HTTP POST请求，通过指定的数据长度创建POST数据流，在POST结束后调用HttpPostClose()终止POST数据流并获取HTTP响应，使用后再调用Close释放(好长！）
            ''' </summary>
            ''' <param name="url">URL</param>
            ''' <param name="postLength">进行POST请求的数据内容的长度</param>
            ''' <param name="postContentType">要POST的数据的文件MIME类型，默认为application/octet-stream</param>
            ''' <param name="headerParam">往HTTP请求头里塞的参数</param>
            ''' <param name="urlParam">往URL里塞的参数(就是x.com/test?key=value这种类型的)</param>
            ''' <returns>返回HTTP Request Stream，往里面写入数据即可post</returns>
            ''' <remarks></remarks>
            Public Function HttpPost(ByVal url As String, ByVal postLength As ULong, Optional ByVal postContentType As String = DEFAULT_BINARY_CONTENT_TYPE, Optional ByVal headerParam As Parameters = Nothing, Optional ByVal urlParam As Parameters = Nothing) As Stream
                Dim cur_times As Integer = 0
                Do
                    Try
                        '构建URL参数
                        Dim postUrl As String = url
                        If urlParam IsNot Nothing Then
                            postUrl &= "?" & urlParam.BuildURLQuery
                        End If
                        '创建http request
                        HTTP_Request = System.Net.HttpWebRequest.Create(postUrl)

                        '将headerParam中的参数添加到web request中
                        If headerParam IsNot Nothing Then
                            For Each e As KeyValuePair(Of String, String) In headerParam
                                Select Case e.Key
                                    Case STR_ACCEPT
                                        HTTP_Request.Accept = e.Value
                                    Case STR_CONNECTION
                                        Select Case e.Value
                                            Case STR_CONNECTION_KEEP_ALIVE
                                                'todo: 测试
                                                HTTP_Request.Connection = ""
                                                HTTP_Request.KeepAlive = True
                                            Case STR_CONNECTION_CLOSE
                                                HTTP_Request.Connection = Nothing
                                            Case Else
                                                Throw New ArgumentException("Connection属性无效")
                                        End Select
                                    Case STR_CONTENT_LENGTH
                                        HTTP_Request.ContentLength = e.Value
                                    Case STR_CONTENT_TYPE
                                        HTTP_Request.ContentType = e.Value
                                    Case STR_EXPECT
                                        HTTP_Request.Expect = e.Value
                                    Case STR_DATE
                                        HTTP_Request.Date = e.Value
                                    Case STR_HOST
                                        HTTP_Request.Host = e.Value
                                    Case STR_IF_MODIFIED_SINCE
                                        HTTP_Request.IfModifiedSince = e.Value
                                    Case STR_RANGE
                                        Throw New InvalidDataException("POST请求中无法添加Range")
                                    Case STR_REFERER
                                        HTTP_Request.Referer = e.Value
                                    Case STR_TRANSFER_ENCODING
                                        HTTP_Request.TransferEncoding = e.Value
                                    Case STR_USER_AGENT
                                        HTTP_Request.UserAgent = e.Value
                                    Case Else
                                        HTTP_Request.Headers.Add(e.Key, e.Value)
                                End Select
                            Next
                        End If
                        '追加默认参数
                        Dim keyList As List(Of String) = HTTP_Request.Headers.AllKeys.ToList
                        If Not keyList.Contains(STR_ACCEPT) Then HTTP_Request.Accept = Accept
                        If Not keyList.Contains(STR_ACCEPT_ENCODING) Then HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding)
                        If Not keyList.Contains(STR_ACCEPT_LANGUAGE) Then HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage)
                        If Not keyList.Contains(STR_DATE) Then HTTP_Request.Date = Now
                        If Not keyList.Contains(STR_USER_AGENT) Then HTTP_Request.UserAgent = UserAgent

                        'proxy
                        If Proxy IsNot Nothing Then HTTP_Request.Proxy = Proxy
                        'cookie
                        If UseCookie AndAlso Not keyList.Contains(STR_COOKIE) Then HTTP_Request.CookieContainer = DefaultCookieContainer

                        'method (get / post)
                        HTTP_Request.Method = DEFAULT_POST_METHOD
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeout
                        HTTP_Request.Timeout = Timeout

                        HTTP_Request.Date = Now

                        'post data
                        HTTP_Request.ContentType = postContentType
                        HTTP_Request.ContentLength = postLength
                        Return HTTP_Request.GetRequestStream

                        Exit Do
                    Catch ex As Exception
                        cur_times += 1
                        If RetryTimes >= 0 AndAlso cur_times > RetryTimes Then Throw ex
                        If RetryDelay > 0 Then Threading.Thread.Sleep(RetryDelay)
                    End Try

                Loop

                '错误返回！
                Return Nothing
            End Function
            ''' <summary>
            ''' HTTP POST结束，获取HTTP响应及其数据流（通过本类的Stream属性返回），在释放时记得Close释放资源
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub HttpPostClose()
                If HTTP_Request Is Nothing Then Return
                Try
                    If HTTP_Request.GetRequestStream.CanWrite Then
                        HTTP_Request.GetRequestStream.Close()
                    End If
                Catch ex As Exception
                End Try

                Try
                    '获取http响应
                    HTTP_Response = HTTP_Request.GetResponse

                    DefaultCookieContainer.Add(parseCookie(HTTP_Response.Headers(DEFAULT_SETCOOKIE_HEADER), HTTP_Response.ResponseUri.Host))

                    If HTTP_Response.StatusCode = Net.HttpStatusCode.OK Then
                        Select Case HTTP_Response.ContentEncoding
                            Case STR_GZIP_ENCODING
                                Stream = New System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case STR_DEFLATE_ENCODING
                                Stream = New System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream, IO.Compression.CompressionMode.Decompress)
                            Case Else
                                Stream = HTTP_Response.GetResponseStream
                        End Select
                    End If

                Catch ex As Exception
                    Throw
                End Try
            End Sub
            Protected Overrides Sub Finalize()
                If Stream IsNot Nothing Then Stream.Close()
                If HTTP_Response IsNot Nothing Then HTTP_Response.Close()
                MyBase.Finalize()
            End Sub
            Public Sub Close()
                Try
                    If Stream IsNot Nothing Then
                        Stream.Close()
                        Stream = Nothing
                    End If
                Catch ex As Exception
                End Try
                Try
                    If HTTP_Response IsNot Nothing Then
                        HTTP_Response.Close()
                        HTTP_Response = Nothing
                    End If
                Catch ex As Exception
                End Try
                HTTP_Request = Nothing
            End Sub

            Public Function ReadResponseString() As String
                If Stream Is Nothing OrElse Stream.CanRead = False Then Return ""
                Return ReadToEnd(Stream)
            End Function
        End Class
    End Namespace
End Namespace