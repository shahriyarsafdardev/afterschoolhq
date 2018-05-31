Public Class Slack

    Private _WebHookUrl As String
    Public Property WebHookUrl() As String
        Get
            Return _WebHookUrl
        End Get
        Set(ByVal value As String)
            _WebHookUrl = value
        End Set
    End Property

    Private _Channel As String
    Public Property Channel() As String
        Get
            Return _Channel
        End Get
        Set(ByVal value As String)
            _Channel = value
        End Set
    End Property

    Private _Text As String
    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal value As String)
            _Text = value
        End Set
    End Property

    Private _BotName As String
    Public Property BotName() As String
        Get
            Return _BotName
        End Get
        Set(ByVal value As String)
            _BotName = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Function Send() As SystemMessages

        Dim msgs As New SystemMessages

        Try

            Dim obj = New With {.channel = Channel, .username = BotName, .text = Text}

            Dim rc As New RestClient(WebHookUrl)
            rc.Parameters.Add("payload", obj.ToJSON())
            rc.RequestMethod = RestClientRequestMethod.POST
            rc.ContentType = RestClientContentTypeEnum.URLEncoded
            Dim rtn As String = rc.DoRequest()

            msgs.Add(rtn, MessageType.Notice)

        Catch ex As Exception
            msgs.Add("Error connecting to slack", MessageType.Notice)
        End Try

        Return msgs

    End Function

End Class
