Imports System.Web.HttpContext

<Serializable()> _
Public Class SystemMessage

    Private _exception As Exception
    Public Property Exception() As Exception
        Get
            Return _exception
        End Get
        Set(ByVal value As Exception)
            _exception = value
        End Set
    End Property

    Private _text As String
    Public Property Text() As String
        Get
            Return _text
        End Get
        Set(ByVal value As String)
            _text = value
        End Set
    End Property

    Private _messageType As MessageType
    Public Property MessageType() As MessageType
        Get
            Return _messageType
        End Get
        Set(ByVal value As MessageType)
            _messageType = value
        End Set
    End Property

    Public ReadOnly Property IsError() As Boolean
        Get
            Return MessageType = MessageType.ApiError Or MessageType = MessageType.ApiUsageError Or MessageType = MessageType.Error Or MessageType = MessageType.UserError
        End Get
    End Property

    Public Sub New()
        Me.MessageType = MessageType.Success
        Me.Text = "Success"
    End Sub

    Public Sub New(ByVal pText As String, ByVal pMessageType As MessageType)
        Me.MessageType = pMessageType
        Me.Text = pText
    End Sub
    Public Sub New(ByVal pText As String, ByVal pMessageType As MessageType, ByVal pException As Exception)
        Me.New(pText, pMessageType)
        Me.Exception = pException
    End Sub
End Class

<Serializable()> _
Public Class SystemMessages
    Implements IEnumerable

    Dim _items As List(Of SystemMessage)

    Public ReadOnly Property Count() As Integer
        Get
            Return Me._items.Count
        End Get
    End Property

    Default Public ReadOnly Property Item(ByVal index As Integer) As SystemMessage
        Get
            Return Me._items(index)
        End Get
    End Property

    Private _errorMessagesColor As Drawing.Color
    Public Property ErrorMessagesColor() As Drawing.Color
        Get
            Return _errorMessagesColor
        End Get
        Set(ByVal value As Drawing.Color)
            _errorMessagesColor = value
        End Set
    End Property

    Private _UserErrorMessagesColor As Drawing.Color
    Public Property UserErrorMessagesColor() As Drawing.Color
        Get
            Return _UserErrorMessagesColor
        End Get
        Set(ByVal value As Drawing.Color)
            _UserErrorMessagesColor = value
        End Set
    End Property

    Private _warningMessagesColor As Drawing.Color
    Public Property WarningMessagesColor() As Drawing.Color
        Get
            Return _warningMessagesColor
        End Get
        Set(ByVal value As Drawing.Color)
            _warningMessagesColor = value
        End Set
    End Property

    Private _successMessagesColor As Drawing.Color
    Public Property SuccessMessagesColor() As Drawing.Color
        Get
            Return _successMessagesColor
        End Get
        Set(ByVal value As Drawing.Color)
            _successMessagesColor = value
        End Set
    End Property

    Private _confirmationMessagesColor As Drawing.Color
    Public Property ConfirmationMessagesColor() As Drawing.Color
        Get
            Return _confirmationMessagesColor
        End Get
        Set(ByVal value As Drawing.Color)
            _confirmationMessagesColor = value
        End Set
    End Property

    Public Sub New()
        Me._items = New List(Of SystemMessage)
        Me.SuccessMessagesColor = Drawing.Color.Blue
        Me.WarningMessagesColor = Drawing.Color.OrangeRed
        Me.ErrorMessagesColor = Drawing.Color.Red
        Me.ConfirmationMessagesColor = Drawing.Color.Orange
        Me.UserErrorMessagesColor = Drawing.Color.Red
    End Sub

    Public Shared ReadOnly Property ErrorTypes() As MessageType()
        Get
            Return New MessageType() {MessageType.Error, MessageType.UserError, MessageType.ApiError, MessageType.ApiUsageError}
        End Get
    End Property
    Public Shared ReadOnly Property NonErrorTypes() As MessageType()
        Get
            Return New MessageType() {MessageType.Confirmation, MessageType.Notice, MessageType.Success, MessageType.Warning}
        End Get
    End Property

    Public Sub Clear()
        Me._items.Clear()
    End Sub
    Public Sub Clear(ByVal pMessageType As MessageType)
        Dim msgs As New List(Of SystemMessage)
        For i As Integer = 0 To Me._items.Count - 1
            If Me._items(i).MessageType = pMessageType Then
                msgs.Add(Me._items(i))
            End If
        Next

        For i As Integer = 0 To msgs.Count - 1
            Me._items.Remove(msgs(i))
        Next
    End Sub
    Public Sub Clear(ByVal pMessageTypes() As MessageType)
        For Each mt As MessageType In pMessageTypes
            Clear(mt)
        Next
    End Sub
    Public Function Has(ByVal pMessageType As MessageType) As Boolean
        Dim rtn As Boolean = False
        For i As Integer = 0 To Me._items.Count - 1
            If Me._items(i).MessageType = pMessageType Then
                rtn = True
                Exit For
            End If
        Next
        Return rtn
    End Function
    Public Function HasAny(ByVal pMessageTypes() As MessageType) As Boolean
        Dim rtn As Boolean = False
        For Each mt As MessageType In pMessageTypes
            If Me.Has(mt) Then
                rtn = True
                Exit For
            End If
        Next
        Return rtn
    End Function

    Public Function HasNo(ByVal pMessageTypes() As MessageType) As Boolean
        Return Not HasAny(pMessageTypes)
    End Function

    Public Function HasErrors() As Boolean
        Return HasAny(SystemMessages.ErrorTypes)
    End Function

    Public Function HasNoErrors() As Boolean
        Return HasNo(SystemMessages.ErrorTypes)
    End Function

    Public Function HasOnly(ByVal pMessageType As MessageType) As Boolean
        Dim rtn As Boolean = True
        For i As Integer = 0 To Me._items.Count - 1
            If Me._items(i).MessageType <> pMessageType Then
                rtn = False
                Exit For
            End If
        Next
        Return rtn
    End Function


    Public Function Add(ByVal item As SystemMessage) As Integer
        Me._items.Add(item)
        Return Me._items.Count - 1
    End Function

    Public Function Add(ByVal message As String, ByVal messageType As MessageType)
        Return Me.Add(New SystemMessage(message, messageType))
    End Function
    Public Function Add(ByVal message As String, ByVal messageType As MessageType, ByVal ex As Exception)
        Return Me.Add(New SystemMessage(message, messageType, ex))
    End Function

    Public Sub Insert(ByVal index As Integer, ByVal item As SystemMessage)
        Me._items.Insert(index, item)
    End Sub

    Public Sub Insert(ByVal index As Integer, ByVal message As String, ByVal messageType As MessageType)
        Me._items.Insert(index, New SystemMessage(message, messageType))
    End Sub

    Public Sub Insert(ByVal index As Integer, ByVal message As String, ByVal messageType As MessageType, ByVal ex As Exception)
        Me._items.Insert(index, New SystemMessage(message, messageType, ex))
    End Sub

    Public Sub AddRange(ByRef pMessages As List(Of SystemMessage))
        For i As Integer = 0 To pMessages.Count - 1
            Me.Add(pMessages(i))
        Next
    End Sub
    Public Sub AddRange(ByVal pMessages As SystemMessages)
        For i As Integer = 0 To pMessages.Count - 1
            Me.Add(pMessages(i))
        Next
    End Sub

    Public Function GetByMessageType(ByVal pMessageType As MessageType) As SystemMessages
        Dim msgs As New SystemMessages

		For i As Integer = 0 To Me._items.Count - 1
			If Me._items(i).MessageType = pMessageType Then
				msgs.Add(Me._items(i))
			End If
		Next
        Return msgs
    End Function

    Public Function GetByMessageType(ByVal pMessageTypes() As MessageType) As SystemMessages
        Dim msgs As New SystemMessages

        For Each mt As MessageType In pMessageTypes
            msgs.AddRange(GetByMessageType(mt))
        Next

        Return msgs
    End Function

    Public Function HasMessageContainingText(ByVal text As String) As Boolean
        For Each msg As SystemMessage In Me._items
            If msg.Text.ToLower().Contains(text.ToLower()) Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function GetMessageColor(ByVal pMessageType As MessageType) As Drawing.Color
        Select Case pMessageType
            Case MessageType.Error
                Return Me.ErrorMessagesColor
            Case MessageType.Success
                Return Me.SuccessMessagesColor
            Case MessageType.Warning
                Return Me.WarningMessagesColor
            Case MessageType.Confirmation
                Return Me.ConfirmationMessagesColor
            Case MessageType.UserError
                Return Me.UserErrorMessagesColor
            Case Else
                Return Drawing.Color.Black
        End Select
    End Function
    Public Function RenderMessageList() As String
        Return RenderMessageList(MessageType.NotSet, MessageOutputType.Html)
    End Function
    Public Function RenderMessageList(ByVal messageType As MessageType) As String
        Return RenderMessageList(messageType, MessageOutputType.Html)
    End Function
    Public Function RenderMessageList(ByVal textType As MessageOutputType) As String
        Return RenderMessageList(MessageType.NotSet, textType)
    End Function
    Public Function RenderMessageList(ByVal messageType As MessageType, ByVal textType As MessageOutputType) As String
        Dim rtn As String = ""

        Select Case textType
            Case MessageOutputType.Text
                Dim pMessageTypes As MessageType() = New MessageType() {messageType}
                Dim tempHolder As String = ""
                For i As Integer = 0 To Me.Count - 1
                    If Array.IndexOf(pMessageTypes, Me(i).MessageType) > -1 OrElse (pMessageTypes.Count = 1 AndAlso pMessageTypes(0) = MessageType.NotSet) Then
                        If tempHolder.Contains(Me(i).Text & ",") = False Then
                            Dim temp As String = Me(i).Text
                            If Me(i).Exception IsNot Nothing Then
                                temp &= " " & Me(i).Exception.Message
                                temp &= " " & Me(i).Exception.StackTrace
                            End If
                            tempHolder &= temp & ","
                            rtn &= Me(i).MessageType.ToString.SplitByUpperCase & " - " & temp & vbCrLf
                        End If
                    End If
                Next
            Case MessageOutputType.Json
                rtn = Newtonsoft.Json.JsonConvert.SerializeObject(Me._items)

            Case MessageOutputType.Html
                Dim pMessageTypes As MessageType() = New MessageType() {messageType}
                Dim tempHolder As String = ""
                For i As Integer = 0 To Me.Count - 1
                    If Array.IndexOf(pMessageTypes, Me(i).MessageType) > -1 OrElse (pMessageTypes.Count = 1 AndAlso pMessageTypes(0) = MessageType.NotSet) Then
                        'If tempHolder.Contains(Me(i).Text & ",") = False Then
                        Dim temp As String = "<p>" & Me(i).Text & "<br />"
                        If Me(i).Exception IsNot Nothing Then
                            temp &= Me(i).Exception.Message & "<br />"
                            temp &= Me(i).Exception.StackTrace & "<br />"
                        End If
                        rtn &= Me(i).MessageType.ToString.SplitByUpperCase & "<br />" & temp & "</p>"
                        'End If
                    End If
                Next
        End Select

        Return rtn

    End Function

    Public Function ToList() As List(Of SystemMessage)
        Return _items
    End Function
    Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return _items.GetEnumerator()
    End Function
End Class

<Serializable()>
Public Class DateRange

    Private _StartDate As DateTime
    Public Property StartDate() As DateTime
        Get
            Return _StartDate
        End Get
        Set(ByVal value As DateTime)
            _StartDate = value
        End Set
    End Property

    Private _EndDate As DateTime
    Public Property EndDate() As DateTime
        Get
            Return _EndDate
        End Get
        Set(ByVal value As DateTime)
            _EndDate = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(startDate As DateTime, endDate As DateTime)
        Me.StartDate = startDate
        Me.EndDate = endDate
    End Sub

    ''' <summary>
    ''' http://stackoverflow.com/questions/325933/determine-whether-two-date-ranges-overlap#325964
    ''' var doesOverlap = (startDate1 > startDate2 ? startDate1  startDate2) islessthan (endDate1 islessthan endDate2 ? endDate1 : endDate2);
    ''' </summary>
    ''' <param name="bDateRange"></param>
    ''' <returns></returns>
    Public Function Overlaps(ByVal bDateRange As DateRange) As Boolean
        Return If(Me.StartDate > bDateRange.StartDate, Me.StartDate, bDateRange.StartDate) < If(Me.EndDate < bDateRange.EndDate, Me.EndDate, bDateRange.EndDate)
    End Function

    Public Function IsValid() As Boolean
        Return If(StartDate < EndDate, True, False)
    End Function

    Public Shared Function HasOverlappingDates(list1 As IEnumerable(Of DateRange)) As Boolean
        For i As Integer = 0 To list1.Count - 1
            For j As Integer = i + 1 To list1.Count - 1
                If list1(i).Overlaps(list1(j)) Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    Public Shared Function HasOverlappingDates(list1 As IEnumerable(Of DateRange), list2 As IEnumerable(Of DateRange)) As Boolean
        For i As Integer = 0 To list1.Count - 1
            For j As Integer = 0 To list2.Count - 1
                If list1(i).Overlaps(list2(j)) Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    Public Shared Function HasOverlappingDates(list As IEnumerable(Of IEnumerable(Of DateRange))) As Boolean
        For i As Integer = 0 To list.Count - 1
            For j As Integer = i + 1 To list.Count - 1
                If HasOverlappingDates(list(i), list(j)) Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

End Class

Public Class RestClientFile
    Private _FilePath As String
    Private Property FilePath() As String
        Get
            Return _FilePath
        End Get
        Set(ByVal value As String)
            _FilePath = value
        End Set
    End Property
    Private _uploadParamName As String
    Public Property UploadParameterName() As String
        Get
            Return _uploadParamName
        End Get
        Set(ByVal value As String)
            _uploadParamName = value
        End Set
    End Property

    Private _FileInfo As System.IO.FileInfo
    Private ReadOnly Property FileInfo() As System.IO.FileInfo
        Get
            If _FileInfo.IsEmpty Then
                _FileInfo = New System.IO.FileInfo(FilePath)
            End If
            Return _FileInfo
        End Get
    End Property

    Private _stream As System.IO.Stream
    Public Property Stream() As System.IO.Stream
        Get
            Return _stream
        End Get
        Set(ByVal value As System.IO.Stream)
            _stream = value
        End Set
    End Property

    Private _filename As String
    Public Property Filename() As String
        Get
            Return _filename
        End Get
        Set(ByVal value As String)
            _filename = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(filePath As String, uploadParameterName As String)
        Me.FilePath = filePath
        Me.UploadParameterName = uploadParameterName
        Me.Stream = System.IO.File.OpenRead(filePath)
        Me.Filename = FileInfo.Name
    End Sub

    Public Sub New(stream As System.IO.Stream, filename As String, uploadParameterName As String)
        Me.Stream = stream
        Me.Filename = filename
        Me.UploadParameterName = uploadParameterName
    End Sub

End Class
Public Class RestClientMimePart
    Private _headers As System.Collections.Specialized.NameValueCollection = New System.Collections.Specialized.NameValueCollection()
    Public ReadOnly Property Headers() As System.Collections.Specialized.NameValueCollection
        Get
            Return _headers
        End Get
    End Property

    Private _header As Byte()
    Public Property Header() As Byte()
        Get
            Return _header
        End Get
        Protected Set(value As Byte())
            _header = value
        End Set
    End Property

    Private _data As System.IO.Stream
    Public Property Data As System.IO.Stream
        Get
            Return _data
        End Get
        Set(value As System.IO.Stream)
            _data = value
        End Set
    End Property

    Public Function GenerateHeaderFooterData(boundary As String) As Long
        Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()

        sb.Append("--")
        sb.Append(boundary)
        sb.AppendLine()
        For Each key As String In _headers.AllKeys
            sb.Append(key)
            sb.Append(": ")
            sb.AppendLine(_headers(key))
        Next
        sb.AppendLine()

        Header = System.Text.Encoding.UTF8.GetBytes(sb.ToString())

        Return Header.Length + Data.Length + 2
    End Function

End Class
Public Class RestClient


    Private _url As String
    Public Property Url() As String
        Get
            Return _url
        End Get
        Set(ByVal value As String)
            _url = value
        End Set
    End Property

    Private _Parameters As System.Collections.Specialized.NameValueCollection
    Public Property Parameters() As System.Collections.Specialized.NameValueCollection
        Get
            Return _Parameters
        End Get
        Set(ByVal value As System.Collections.Specialized.NameValueCollection)
            _Parameters = value
        End Set
    End Property

    Private _content As String
    Public Property Content() As String
        Get
            Return _content
        End Get
        Set(ByVal value As String)
            _content = value
        End Set
    End Property

    'deprecated
    Public Property PostVariables() As String
        Get
            Return Content
        End Get
        Set(ByVal value As String)
            Content = value
        End Set
    End Property

    Private _contentType As RestClientContentTypeEnum
    Public Property ContentType() As RestClientContentTypeEnum
        Get
            Return _contentType
        End Get
        Set(ByVal value As RestClientContentTypeEnum)
            _contentType = value
        End Set
    End Property

    Private _headers As System.Collections.Specialized.NameValueCollection
    Public Property Headers() As System.Collections.Specialized.NameValueCollection
        Get
            Return _headers
        End Get
        Set(ByVal value As System.Collections.Specialized.NameValueCollection)
            _headers = value
        End Set
    End Property

    Private _results As String
    Public Property Results() As String
        Get
            Return _results
        End Get
        Set(ByVal value As String)
            _results = value
        End Set
    End Property

    Private _resultsXML As XDocument
    Public ReadOnly Property ResultsXML() As XDocument
        Get
            Try
                Return XDocument.Parse(Me.Results)
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
    End Property

    Private _requestMethod As RestClientRequestMethod
    Public Property RequestMethod() As RestClientRequestMethod
        Get
            Return _requestMethod
        End Get
        Set(ByVal value As RestClientRequestMethod)
            _requestMethod = value
        End Set
    End Property

    Private _files As List(Of RestClientFile)
    Public Property Files() As List(Of RestClientFile)
        Get
            Return _files
        End Get
        Set(ByVal value As List(Of RestClientFile))
            _files = value
        End Set
    End Property

    Private _RequestTimeout As Integer
    Public Property RequestTimeout() As Integer
        Get
            Return _RequestTimeout
        End Get
        Set(ByVal value As Integer)
            _RequestTimeout = value
        End Set
    End Property


    Public Sub New()
        RequestMethod = RestClientRequestMethod.GET
        Headers = New Specialized.NameValueCollection()
        Parameters = New Specialized.NameValueCollection()
        Files = New List(Of RestClientFile)
        RequestTimeout = 180000
    End Sub

    Public Sub New(ByVal url As String)
        Me.New()
        Me.Url = url
    End Sub

    Public Sub New(ByVal url As String, ByVal content As String)
        Me.New(url)
        Me.Content = content
    End Sub

    Private Sub LoadParameters()

        If Parameters.Count = 0 And Content.IsEmpty() = False Then
            For Each kvp As String In Content.Split("&")
                If Not kvp.IsEmpty And kvp.Split("=").Length = 2 Then
                    Dim key As String = kvp.Split("=")(0)
                    Dim value As String = kvp.Split("=")(1)

                    Parameters.Add(key, value)

                End If
            Next
        End If

    End Sub

    Private Sub LoadContent()
        Content = ""
        If Parameters.Count > 0 Then
            For Each key As String In Parameters.Keys
                If Not Content.IsEmpty Then Content &= "&"
                Content &= String.Format("{0}={1}", key, Parameters(key))
            Next
        End If
    End Sub

    Public Function DoRequest() As String

        Dim rtn As String = ""

        Try

            If ContentType = RestClientContentTypeEnum.MultiPartFormData Or ContentType = RestClientContentTypeEnum.URLEncoded Then
                LoadParameters()
                LoadContent()
            End If

            Dim address As String

            If RequestMethod = RestClientRequestMethod.GET And Not Content.IsEmpty Then

                If Url.IndexOf("?") > -1 Then
                    address = Url & "&"
                Else
                    address = Url & "?"
                End If

                address &= Content

            Else
                address = Url
            End If

            Dim request As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(address), System.Net.HttpWebRequest)
            request.Method = Me.RequestMethod.ToString
            request.Timeout = RequestTimeout
            request.Headers.Add(Headers)

            If RequestMethod = RestClientRequestMethod.POST Or RequestMethod = RestClientRequestMethod.PUT Then

                If Files.Count > 0 Then

                    Dim mimeParts As List(Of RestClientMimePart) = New List(Of RestClientMimePart)
                    For Each key As String In Parameters.Keys
                        Dim part As New RestClientMimePart()
                        part.Headers("Content-Disposition") = String.Format("form-data; name=""{0}""", key)
						part.Data = New System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Parameters(key).IfEmpty("")))
                        mimeParts.Add(part)
                    Next

                    For Each file As RestClientFile In Files
                        Dim part As New RestClientMimePart()
                        part.Headers("Content-Disposition") = String.Format("form-data; name=""{0}""; filename=""{1}""", file.UploadParameterName, file.Filename)
                        part.Headers("Content-Type") = "application/octect-stream"
                        part.Data = file.Stream
                        mimeParts.Add(part)
                    Next

                    Dim boundary As String = "----------" + DateTime.Now.Ticks.ToString("x")
                    request.ContentType = "multipart/form-data; boundary=" + boundary

                    Dim contentLength As Long = 0
                    Dim _footer As Byte() = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--" + vbCrLf)

                    For Each part As RestClientMimePart In mimeParts
                        contentLength += part.GenerateHeaderFooterData(boundary)
                    Next

                    request.ContentLength = contentLength + _footer.Length

                    Dim buffer As Byte()
                    ReDim buffer(8192)
                    Dim afterFile As Byte() = System.Text.Encoding.UTF8.GetBytes(vbCrLf)
                    Dim read As Integer

                    Dim s As System.IO.Stream = request.GetRequestStream()
                    For Each part As RestClientMimePart In mimeParts
                        s.Write(part.Header, 0, part.Header.Length)

                        read = part.Data.Read(buffer, 0, buffer.Length)
                        Do While read > 0
                            s.Write(buffer, 0, read)
                            read = part.Data.Read(buffer, 0, buffer.Length)
                        Loop

                        part.Data.Dispose()

                        s.Write(afterFile, 0, afterFile.Length)
                    Next

                    s.Write(_footer, 0, _footer.Length)

                Else

                    request.ContentType = New RestClientContentType(ContentType).ToString()
                    request.ContentLength = Content.Length

                    Dim writer As New System.IO.StreamWriter(request.GetRequestStream())
                    writer.Write(Content)
                    writer.Close()

                End If
            End If

            Dim response As System.Net.HttpWebResponse = CType(request.GetResponse(), System.Net.HttpWebResponse)
            Dim responseStream As New IO.StreamReader(response.GetResponseStream())
            rtn = responseStream.ReadToEnd()
            Me.Results = rtn
            responseStream.Close()

        Catch ex As System.Net.WebException

            If Not ex.Response.IsEmpty Then
                Dim responseStream As New IO.StreamReader(ex.Response.GetResponseStream())

                rtn = responseStream.ReadToEnd()
                Me.Results = rtn
            End If

            Throw ex

        End Try

        Return rtn
    End Function
End Class

Public Enum RestClientRequestMethod
    [GET]
    POST
    PUT
End Enum

Public Enum RestClientContentTypeEnum
    URLEncoded = 1
    MultiPartFormData
    TextJson
    ApplicationJson
    Custom
    TextXml
End Enum

Public Class RestClientContentType
    Private _ContentType As RestClientContentTypeEnum
    Public Property ContentType() As RestClientContentTypeEnum
        Get
            Return _ContentType
        End Get
        Set(ByVal value As RestClientContentTypeEnum)
            _ContentType = value
        End Set
    End Property

    Private _CustomContentType As String
    Public Property CustomContentType() As String
        Get
            Return _CustomContentType
        End Get
        Set(ByVal value As String)
            _CustomContentType = value
        End Set
    End Property


    Public Sub New()

    End Sub
    Public Sub New(ByVal contentType As RestClientContentTypeEnum)
        Me.ContentType = contentType
    End Sub
    Public Sub New(ByVal contentType As RestClientContentTypeEnum, ByVal customContentType As String)
        Me.New(contentType)
        Me.CustomContentType = customContentType
    End Sub

    Public Overrides Function ToString() As String
        Select Case Me.ContentType
            Case RestClientContentTypeEnum.MultiPartFormData
                Return "multipart/form-data"
            Case RestClientContentTypeEnum.URLEncoded
                Return "application/x-www-form-urlencoded"
            Case RestClientContentTypeEnum.TextJson
                Return "text/json"
            Case RestClientContentTypeEnum.ApplicationJson
                Return "application/json"
            Case RestClientContentTypeEnum.TextXml
                Return "text/xml"
            Case RestClientContentTypeEnum.Custom
                Return CustomContentType
        End Select
        Return ""
    End Function
End Class

Public Class Utilities

    Public Const EmailRegEx As String = "^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,63}))$"
    Public Const PhoneRegEx As String = "^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:(\s*(\(?([2-9]{1})([0-9]{2}))\)?\s*)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})$"
    Public Const FormattedPhoneRegEx As String = "^(\(?\d{3}\)?)([ .-])(\d{3})([ .-])(\d{4})$"
    Public Const DomainRegEx As String = "^((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
    Public Const SpecialCharsRegEx As String = "[^\w-\.]+"
    Public Const SpecialCharsRegExAllowWhiteSpace As String = "[^\w^\s-\.]+"
    Public Const UrlFriendlyRegEx As String = "[^\w-]+"
    Public Const NoAlphaNumericCharactersRegEx As String = "[^A-Za-z0-9]"
    Public Const QuestionsRegEx As String = "((?<=\.|\?|\!)|^)[\s]?[\w\s\d\,\-\$#@%&\*\(\)\+\=\{\}\[\]\|:;\<\>~`/\^'""]*\?"
    Public Const NonNumericCharsRegEx As String = "[$,A-Za-z\(\)]"
    Public Const WebsiteRegEx As String = "^(http(s)?\://)?[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,15}(/\S*)?$"
    ''' <summary>
    ''' This will support both youtube and vimeo url validations
    ''' </summary>
    Public Const ProfileVideoSpecificUrlRegEx As String = "^(http:\/\/|https:\/\/)(vimeo\.com|youtu\.be|www\.youtube\.com)\/([\w\/]+)([\?].*)?$"

    Private Shared _connectionString As String
    Public Shared Property ConnectionString() As String
        Get
            Return _connectionString
        End Get
        Set(ByVal value As String)
            _connectionString = value
        End Set
    End Property

    Public Shared Function SplitStringByUpperCaseChars(ByVal str As String) As String

        Dim rtn As String = ""
        If str IsNot Nothing AndAlso str <> String.Empty Then
            str = str.Replace(" ", "")
            For i As Integer = 0 To str.Length - 1
                If i > 0 And Char.IsUpper(str.Chars(i)) = True AndAlso ((Char.IsLower(str.Chars(i - 1)) = True) Or (i < str.Length - 1 AndAlso Char.IsLower(str.Chars(i + 1)) = True)) Then
                    rtn &= " " & str.Chars(i).ToString
                Else
                    rtn &= str.Chars(i).ToString
                End If
            Next
        End If

        Return rtn

    End Function

    Public Shared Function RandomNumber(ByVal MaxNumber As Integer, Optional ByVal MinNumber As Integer = 0) As Integer

        'initialize random number generator
        Dim r As New Random(System.DateTime.Now.Millisecond)

        'if passed incorrect arguments, swap them
        'can also throw exception or return 0

        If MinNumber > MaxNumber Then
            Dim t As Integer = MinNumber
            MinNumber = MaxNumber
            MaxNumber = t
        End If

        Return r.Next(MinNumber, MaxNumber)

    End Function


    Public Shared Function MD5(ByVal Value As String) As String
        Dim x As System.Security.Cryptography.MD5CryptoServiceProvider = New System.Security.Cryptography.MD5CryptoServiceProvider()
        Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(Value)
        data = x.ComputeHash(data)
        Dim ret As String = ""
        For i As Integer = 0 To data.Length - 1
            ret += data(i).ToString("x2").ToLower()
        Next
        Return ret
    End Function

    Public Shared ReadOnly Property IPAddressRegEx() As Text.RegularExpressions.Regex
        Get
            Return New Text.RegularExpressions.Regex("^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$")
        End Get
    End Property

    Public Shared Function GetMimeType(ByVal fileExtension As String) As String

        Select Case fileExtension
            Case "mov"
                Return "video/quicktime"
            Case "mp4", "mpeg", "ogv", "webm"
                Return "video/" + fileExtension
            Case "oga", "mp3", "m4a", "wav", "aiff"
                Return "audio/" + fileExtension
            Case "jpg", "jpeg", "gif", "png", "tiff", "bmp"
                Return "image/" + fileExtension
            Case "txt", "css", "js", "html", "htm", "xml", "csv", "sql", "ini", "cs", "vb", "php"
                Return "text/" + fileExtension
            Case Else
                Return "application/" + fileExtension
        End Select

    End Function

    Public Shared ReadOnly Property VideoFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.Add("flv")
            list.Add("mov")
            list.Add("mp4")
            list.Add("avi")
            list.Add("mpeg")
            list.Add("ogv")
            list.Add("webm")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property ImageFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.Add("jpg")
            list.Add("jpeg")
            list.Add("gif")
            list.Add("png")
            list.Add("tiff")
            list.Add("bmp")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property AudioFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.Add("mp3")
            list.Add("m4a")
            list.Add("wav")
            list.Add("aiff")
            list.Add("oga")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property TextFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.Add("txt")
            list.Add("css")
            list.Add("js")
            list.Add("html")
            list.Add("htm")
            list.Add("xml")
            list.Add("csv")
            list.Add("sql")
            list.Add("ini")
            list.Add("cs")
            list.Add("vb")
            list.Add("php")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property OfficeFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.AddRange(OfficeFileExtensionsOnly)
            list.AddRange(TextFileExtensions.ToList())

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property OfficeFileExtensionsOnly() As String()
        Get
            Dim list As New List(Of String)

            list.AddRange(WordFileExtensionsOnly)
            list.AddRange(ExcelFileExtensionsOnly)
            list.AddRange(PowerPointFileExtensions)
            list.Add("pub")
            list.Add("pubx")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property WordFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.AddRange(WordFileExtensionsOnly)
            list.AddRange(TextFileExtensions.ToList())

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property WordFileExtensionsOnly() As String()
        Get
            Dim list As New List(Of String)

            list.Add("doc")
            list.Add("docx")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property PowerPointFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.Add("ppt")
            list.Add("pptx")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property ExcelFileExtensions() As String()
        Get
            Dim list As New List(Of String)

            list.AddRange(ExcelFileExtensionsOnly)
            list.Add("csv")

            Return list.ToArray()
        End Get
    End Property
    Public Shared ReadOnly Property ExcelFileExtensionsOnly() As String()
        Get
            Dim list As New List(Of String)

            list.Add("xls")
            list.Add("xlsx")

            Return list.ToArray()
        End Get
    End Property

    Public Shared Function GetFileExtension(ByVal filename As String) As String
        Dim ext As String = ""
        If filename.LastIndexOf(".") > -1 Then
            ext = filename.Substring(filename.LastIndexOf(".") + 1)
        End If
        Return ext
    End Function

    Public Shared Function ConvertUrlsToLinks(ByVal str As String) As String
        Return ConvertUrlsToLinks(str, False)
    End Function

    Public Shared Function ConvertUrlsToLinks(ByVal str As String, ByVal openInNewWin As Boolean) As String
        Dim rtn As String = str

        If str.IsEmpty() = False Then
            'Dim linkRegEx As String = "(http(s)?://)?[A-Za-z0-9\.]+(?<!\.)\.[A-Za-z]{2,4}(/[A-Za-z0-9#&=%\/\-\.\?_]+)?(?!\s.)"
            'Dim linkRegEx As String = "(http(s)?://)?([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?"
            Dim linkRegEx As String = "(http(s)?://)?[\w\-\.]+(?<!\.)\.[A-Za-z]{2,3}((/|\?|#)([A-Za-z0-9#:&=%\/\-\.\?_]+)?)?"
            Dim r As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex(linkRegEx, Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Multiline)
            Dim targetStr As String = ""
            Dim matches As System.Text.RegularExpressions.MatchCollection = r.Matches(str)

            If matches.Count > 0 Then
                If openInNewWin Then
                    targetStr = " target=""_blank"" "
                End If

                rtn = r.Replace(rtn, "<a href=""$&""" & targetStr & ">$&</a>")
            End If

            r = New System.Text.RegularExpressions.Regex("(?<="")[^""http""]" & linkRegEx & "(?="")", Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Multiline)
            matches = r.Matches(rtn)
            If matches.Count > 0 Then
                rtn = r.Replace(rtn, "http://$&")
            End If
        End If

        Return rtn
    End Function

    Public Shared Function RemoveHTMLAndEntities(ByVal html As String, Optional ByVal removeWhiteSpace As Boolean = True) As String
        Dim reg As Text.RegularExpressions.Regex = New Text.RegularExpressions.Regex("<.+?>|</.+?>|<.+?/>")
        Dim reg2 As Text.RegularExpressions.Regex = New Text.RegularExpressions.Regex("[ ]{2,}", Text.RegularExpressions.RegexOptions.None)
        Dim reg3 As Text.RegularExpressions.Regex = New Text.RegularExpressions.Regex("\s")
        If html.IsEmpty() = False Then
            If removeWhiteSpace Then
                html = Web.HttpUtility.HtmlDecode(html).Replace(vbCrLf, " ")
            End If
            html = reg.Replace(html, " ")
            If removeWhiteSpace Then
                html = reg3.Replace(html, " ")
                html = reg2.Replace(html, " ")
            End If
        End If
        Return html
    End Function

    Public Shared Function GetStateList() As String
        Return "Alabama,Alaska,Arizona,Arkansas,California,Colorado,Connecticut,Delaware,District of Columbia,Florida,Georgia,Hawaii,Idaho,Illinois,Indiana,Iowa,Kansas,Kentucky,Louisiana,Maine,Maryland,Massachusetts,Michigan,Minnesota,Mississippi,Missouri,Montana,Nebraska,Nevada,New Hampshire,New Jersey,New Mexico,New York,North Carolina,North Dakota,Ohio,Oklahoma,Oregon,Pennsylvania,Rhode Island,South Carolina,South Dakota,Tennessee,Texas,Utah,Vermont,Virginia,Washington,West Virginia,Wisconsin,Wyoming"
    End Function

    Public Shared Function GetStates() As List(Of State)
        Dim states = New List(Of State)
        states.Add(New State("Alabama", "AL"))
        states.Add(New State("Alaska", "AK"))
        states.Add(New State("Arizona", "AZ"))
        states.Add(New State("Arkansas", "AR"))
        states.Add(New State("California", "CA"))
        states.Add(New State("Colorado", "CO"))
        states.Add(New State("Connecticut", "CT"))
        states.Add(New State("Delaware", "DE"))
        states.Add(New State("District of Columbia", "DC"))
        states.Add(New State("Florida", "FL"))
        states.Add(New State("Georgia", "GA"))
        states.Add(New State("Hawaii", "HI"))
        states.Add(New State("Idaho", "ID"))
        states.Add(New State("Illinois", "IL"))
        states.Add(New State("Indiana", "IN"))
        states.Add(New State("Iowa", "IA"))
        states.Add(New State("Kansas", "KS"))
        states.Add(New State("Kentucky", "KY"))
        states.Add(New State("Louisiana", "LA"))
        states.Add(New State("Maine", "ME"))
        states.Add(New State("Maryland", "MD"))
        states.Add(New State("Massachusetts", "MA"))
        states.Add(New State("Michigan", "MI"))
        states.Add(New State("Minnesota", "MN"))
        states.Add(New State("Mississippi", "MS"))
        states.Add(New State("Missouri", "MO"))
        states.Add(New State("Montana", "MT"))
        states.Add(New State("Nebraska", "ME"))
        states.Add(New State("Nevada", "NV"))
        states.Add(New State("New Hampshire", "NH"))
        states.Add(New State("New Jersey", "NJ"))
        states.Add(New State("New Mexico", "NM"))
        states.Add(New State("New York", "NY"))
        states.Add(New State("North Carolina", "NC"))
        states.Add(New State("North Dakota", "ND"))
        states.Add(New State("Ohio", "OH"))
        states.Add(New State("Oklahoma", "OK"))
        states.Add(New State("Oregon", "OR"))
        states.Add(New State("Pennsylvania", "PA"))
        states.Add(New State("Rhode Island", "RI"))
        states.Add(New State("South Carolina", "SC"))
        states.Add(New State("South Dakota", "SD"))
        states.Add(New State("Tennessee", "TN"))
        states.Add(New State("Texas", "TX"))
        states.Add(New State("Utah", "UT"))
        states.Add(New State("Vermont", "VT"))
        states.Add(New State("Virginia", "VA"))
        states.Add(New State("Washington", "WA"))
        states.Add(New State("West Virginia", "WV"))
        states.Add(New State("Wisconsin", "WI"))
        states.Add(New State("Wyoming", "WY"))
        Return states
    End Function

End Class

Public Class State
    Private _Name As String
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property

    Private _Abbreviation As String
    Public Property Abbreviation() As String
        Get
            Return _Abbreviation
        End Get
        Set(ByVal value As String)
            _Abbreviation = value
        End Set
    End Property

    Public Sub New(ByVal name As String, ByVal abbreviation As String)
        Me.Name = name
        Me.Abbreviation = abbreviation
    End Sub
End Class

''' <summary>
''' Provides methods for converting <see cref="DateTime"/> structures to and from the equivalent RFC 3339 string representation.
''' </summary>
''' <remarks></remarks>
Public Class Rfc3339DateTime

#Region "Private Members"
    ''' <summary>
    ''' Private member to hold array of formats that RFC 3339 date-time representations conform to.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared formats(0) As String
    ''' <summary>
    ''' Private member to hold the DateTime format string for representing a DateTime in the RFC 3339 format.
    ''' </summary>
    ''' <remarks></remarks>
    Private Const format As String = "yyyy-MM-dd'T'HH:mm:ss.fffK"
#End Region

    '//============================================================
    '//  Public Properties
    '//============================================================
#Region "Rfc3339DateTimeFormat"
    ''' <summary>
    ''' Gets the custom format specifier that may be used to represent a <see cref="DateTime"/> in the RFC 3339 format.
    ''' </summary>
    ''' <value>A <i>DateTime format string</i> that may be used to represent a <see cref="DateTime"/> in the RFC 3339 format.</value>
    ''' <returns></returns>
    ''' <remarks>
    ''' <para>
    ''' This method returns a string representation of a <see cref="DateTime"/> that 
    ''' is precise to the three most significant digits of the seconds fraction; that is, it represents 
    ''' the milliseconds in a date and time value. The <see cref="Rfc3339DateTimeFormat"/> is a valid 
    ''' date-time format string for use in the <see cref="DateTime.ToString(String, IFormatProvider)"/> method.
    ''' </para>
    ''' </remarks>
    Public Shared ReadOnly Property Rfc3339DateTimeFormat() As String
        Get
            Return format
        End Get
    End Property

#End Region

#Region "Rfc3339DateTimePatterns"
    ''' <summary>
    ''' Gets an array of the expected formats for RFC 3339 date-time string representations.
    ''' </summary>
    ''' <value>
    ''' An array of the expected formats for RFC 3339 date-time string representations 
    ''' that may used in the <see cref="DateTime.TryParseExact(String, string[], IFormatProvider, DateTimeStyles, out DateTime)"/> method.
    ''' </value>
    Public ReadOnly Property Rfc3339DateTimePatterns() As String()
        Get

            If (formats.Length > 0) Then

                Return formats

            Else

                ReDim formats(11)

                '// Rfc3339DateTimePatterns
                formats(0) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK"
                formats(1) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK"
                formats(2) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK"
                formats(3) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK"
                formats(4) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"
                formats(5) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK"
                formats(6) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK"
                formats(7) = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK"

                '// Fall back patterns
                formats(8) = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK" '// RoundtripDateTimePattern
                formats(9) = System.Globalization.DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern
                formats(10) = System.Globalization.DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern

                Return formats
            End If
        End Get
    End Property
#End Region

#Region "Parse(string s)"
    ''' <summary>
    ''' Converts the specified string representation of a date and time to its <see cref="DateTime"/> equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a date and time to convert.</param>
    ''' <returns>A <see cref="DateTime"/> equivalent to the date and time contained in <paramref name="s"/>.</returns>
    ''' <remarks>
    ''' The string <paramref name="s"/> is parsed using formatting information in the <see cref="DateTimeFormatInfo.InvariantInfo"/> object.
    ''' </remarks>
    ''' <exception cref="ArgumentNullException"><paramref name="s"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
    ''' <exception cref="FormatException"><paramref name="s"/> does not contain a valid RFC 3339 string representation of a date and time.</exception>
    Public Shared Function Parse(ByVal s As String) As DateTime

        '//------------------------------------------------------------
        '//  Validate parameter
        '//------------------------------------------------------------
        If (s Is Nothing) Then

            Throw New ArgumentNullException("s")
        End If

        Dim result As DateTime
        If (Rfc3339DateTime.TryParse(s, result)) Then
            Return result
        Else
            Throw New FormatException(String.Format(Nothing, "{0} is not a valid RFC 3339 string representation of a date and time.", s))
        End If
    End Function

#End Region

#Region "ToString(DateTime utcDateTime)"
    ''' <summary>
    ''' Converts the value of the specified <see cref="DateTime"/> object to its equivalent string representation.
    ''' </summary>
    ''' <param name="utcDateTime">The Coordinated Universal Time (UTC) <see cref="DateTime"/> to convert.</param>
    ''' <returns>A RFC 3339 string representation of the value of the <paramref name="utcDateTime"/>.</returns>
    ''' <remarks>
    ''' <para>
    ''' This method returns a string representation of the <paramref name="utcDateTime"/> that 
    ''' is precise to the three most significant digits of the seconds fraction; that is, it represents 
    ''' the milliseconds in a date and time value.
    ''' </para>
    ''' <para>
    ''' While it is possible to display higher precision fractions of a second component of a time value, 
    ''' that value may not be meaningful. The precision of date and time values depends on the resolution 
    ''' of the system clock. On Windows NT 3.5 and later, and Windows Vista operating systems, the clock's 
    ''' resolution is approximately 10-15 milliseconds.
    ''' </para>
    ''' </remarks>
    ''' <exception cref="ArgumentException">The specified <paramref name="utcDateTime"/> object does not represent a <see cref="DateTimeKind.Utc">Coordinated Universal Time (UTC)</see> value.</exception>
    Public Shared Function ToRfc3339String(ByVal utcDateTime As DateTime) As String

        If (utcDateTime.Kind <> DateTimeKind.Utc) Then
            Throw New ArgumentException("utcDateTime")
        End If

        Return utcDateTime.ToString(Rfc3339DateTime.Rfc3339DateTimeFormat, System.Globalization.DateTimeFormatInfo.InvariantInfo)
    End Function

#End Region

#Region "TryParse(string s, out DateTime result)"
    ''' <summary>
    ''' Converts the specified string representation of a date and time to its <see cref="DateTime"/> equivalent.
    ''' </summary>
    ''' <param name="s">A string containing a date and time to convert.</param>
    ''' <param name="result">
    ''' When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time 
    ''' contained in <paramref name="s"/>, if the conversion succeeded, 
    ''' or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed. 
    ''' The conversion fails if the s parameter is a <b>null</b> reference (Nothing in Visual Basic), 
    ''' or does not contain a valid string representation of a date and time. 
    ''' This parameter is passed uninitialized.
    ''' </param>
    ''' <returns><b>true</b> if the <paramref name="s"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
    ''' <remarks>
    ''' The string <paramref name="s"/> is parsed using formatting information in the <see cref="DateTimeFormatInfo.InvariantInfo"/> object.
    ''' </remarks>
    ''' 
    Public Shared Function TryParse(ByVal s As String, ByRef result As Date) As Boolean
        '//------------------------------------------------------------
        '//  Attempt to convert string representation
        '//------------------------------------------------------------
        Dim wasConverted As Boolean = False
        result = DateTime.MinValue

        If (Not String.IsNullOrEmpty(s)) Then

            Dim parseResult As Date
            If (DateTime.TryParseExact(s, Rfc3339DateTime.formats, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, parseResult)) Then
                result = DateTime.SpecifyKind(parseResult, DateTimeKind.Utc)
                wasConverted = True
            End If
        End If

        Return wasConverted
    End Function


#End Region
End Class

Public Class Email

    Public Shared Function Send(ByRef msg As System.Net.Mail.MailMessage, ByVal host As String, ByVal username As String, ByVal password As String) As SystemMessages

        Dim msgs As New SystemMessages

        Try

            msg.Headers("PRECEDENCE") = "bulk"

            Dim smtp As System.Net.Mail.SmtpClient = New System.Net.Mail.SmtpClient
            smtp.Host = host
            smtp.Credentials = New Net.NetworkCredential(username, password)
            smtp.Port = 25
            smtp.DeliveryMethod = Net.Mail.SmtpDeliveryMethod.Network
            smtp.Send(msg)
        Catch ex As Exception
            'Throw ex
            msgs.Add(New SystemMessage("Error sending email", MessageType.Error, ex))
        End Try

        Return msgs

    End Function


End Class

Public Class IniFile

    Private _contents As String
    Public Property Contents() As String
        Get
            Return _contents
        End Get
        Set(ByVal value As String)
            _contents = value
        End Set
    End Property

    Private _filename As String
    Public Property Filename() As String
        Get
            Return _filename
        End Get
        Set(ByVal value As String)
            _filename = value
        End Set
    End Property

    Private _sections As Dictionary(Of String, Dictionary(Of String, String))
    Public Property Sections() As Dictionary(Of String, Dictionary(Of String, String))
        Get
            Return _sections
        End Get
        Set(ByVal value As Dictionary(Of String, Dictionary(Of String, String)))
            _sections = value
        End Set
    End Property

    Private _allProperties As Dictionary(Of String, String)
    Private Property AllProperties() As Dictionary(Of String, String)
        Get
            Return _allProperties
        End Get
        Set(ByVal value As Dictionary(Of String, String))
            _allProperties = value
        End Set
    End Property


    Public Sub New()
        AllProperties = New Dictionary(Of String, String)
        Sections = New Dictionary(Of String, Dictionary(Of String, String))
    End Sub
    Public Sub New(ByVal filename As String)
        Me.New()
        Me.Filename = filename
        Me.LoadFile(filename)
    End Sub

    Public Sub LoadFile(ByVal filename As String)
        Me.Filename = filename
        If Me.Filename.IsEmpty = False Then

            If IO.File.Exists(Me.Filename) Then

                Try
                    Contents = IO.File.ReadAllText(Me.Filename)
                Catch ex As Exception
                    Throw New Exception("Error reading file")
                End Try

            Else
                Throw New Exception("File does not exists")
            End If
        End If

        Parse()
    End Sub

    Public Sub WriteFile()

        If Sections.Count > 0 Then

            Dim sb As New Text.StringBuilder

            For Each section As String In Sections.Keys
                sb.Append("[")
                sb.Append(section)
                sb.Append("]")
                sb.Append(vbCrLf)

                For Each key As String In Sections(section).Keys

                    sb.Append("")

                Next

            Next

        End If

    End Sub

    Public Sub Parse()

        If Contents.IsEmpty = False Then

            Dim iniArray As String() = Contents.Split(vbCr)
            Dim currentSectionProperties As Dictionary(Of String, String) = Nothing

            For i As Integer = 0 To iniArray.Count - 1
                Dim item As String = iniArray(i).Trim
                If item.IsEmpty = False Then
                    If item(0) = ";" Then
                    ElseIf item(0) = "[" Then

                        Dim currentSection As String = item.Replace("[", "").Replace("]", "")
                        currentSectionProperties = New Dictionary(Of String, String)
                        Sections.Add(currentSection, currentSectionProperties)

                    ElseIf item.Contains("=") Then

                        Dim keyValueArray As String() = item.Split("=")
                        If keyValueArray.Length = 2 Then
                            Dim key As String = keyValueArray(0).Trim
                            Dim value As String = keyValueArray(1).Trim.Replace("""", "")
                            AllProperties(key) = value
                            If currentSectionProperties.IsEmpty() = False Then
                                currentSectionProperties(key) = value
                            End If
                        End If

                    End If
                End If
            Next
        End If

    End Sub

    Public Function Value(ByVal key As String) As String
        If Contents.IsEmpty Then
            Parse()
        End If
        If KeyExists(key) Then
            Return AllProperties(key)
        Else
            Return Nothing
        End If
    End Function

    Public Function KeyExists(ByVal key As String) As Boolean
        If Contents.IsEmpty Then
            Parse()
        End If
        Return AllProperties.Keys.Contains(key)
    End Function

End Class

Public Class CsvFile

    Public Shared Function Create(ByVal table As List(Of List(Of String))) As String

        Dim csv As New Text.StringBuilder("")

        For i As Integer = 0 To table.Count - 1
            For j As Integer = 0 To table(i).Count - 1
                If j > 0 Then
                    csv.Append(",")
                End If
                csv.Append("""")
                csv.Append(table(i)(j))
                csv.Append("""")
            Next
            csv.Append(vbCrLf)
        Next

        Return csv.ToString()

    End Function

End Class

Namespace Desktop
    Public Class ListItem

        Private _value As String
        Public Property Value() As String
            Get
                Return _value
            End Get
            Set(ByVal value As String)
                _value = value
            End Set
        End Property


        Private _text As String
        Public Property Text() As String
            Get
                Return _text
            End Get
            Set(ByVal value As String)
                _text = value
            End Set
        End Property

        Public Sub New()
        End Sub
        Public Sub New(ByVal Text As String, ByVal Value As String)
            Me.New()
            Me.Text = Text
            Me.Value = Value
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function

    End Class
End Namespace