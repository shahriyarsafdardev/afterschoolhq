'Public Class SystemRegularExpressions
'    Public Shared ReadOnly Property Email() As RegularExpressionObject
'        Get
'            Dim reo As RegularExpressionObject = New RegularExpressionObject("(?<user>(?:(?:[^ \t\(\)\<\>@,;\:\\\""\.\[\]\r\n]+)|(?:\""(?:(?:[^\""\\\r\n])|(?:\\.))*\""))(?:\.(?:(?:[^ \t\(\)\<\>@,;\:\\\""\.\[\]\r\n]+)|(?:\""(?:(?:[^\""\\\r\n])|(?:\\.))*\"")))*)@(?<domain>(?:(?:[^ \t\(\)\<\>@,;\:\\\""\.\[\]\r\n]+)|(?:\[(?:(?:[^\[\]\\\r\n])|(?:\\.))*\]))(?:\.(?:(?:[^ \t\(\)\<\>@,;\:\\\""\.\[\]\r\n]+)|(?:\[(?:(?:[^\[\]\\\r\n])|(?:\\.))*\])))*)", "Please enter a valid e-mail address. ex. yourname@domain.com", True)

'            Return reo
'        End Get
'    End Property

'End Class

'Public Class RegularExpressionObject

'    Private _RegularExpressionText As String
'    Public ReadOnly Property RegularExpressionText() As String
'        Get
'            Return Me._RegularExpressionText
'        End Get
'    End Property

'    Private _regularExpression As Text.RegularExpressions.Regex
'    Public Property RegularExpression() As Text.RegularExpressions.Regex
'        Get
'            Return _regularExpression
'        End Get
'        Set(ByVal value As Text.RegularExpressions.Regex)
'            _regularExpression = value
'        End Set
'    End Property

'    Private _ruleDescription As String
'    Public Property RuleDescription() As String
'        Get
'            Return _ruleDescription
'        End Get
'        Set(ByVal value As String)
'            _ruleDescription = value
'        End Set
'    End Property

'    Private _ignoreCase As Boolean
'    Public Property IgnoreCase() As Boolean
'        Get
'            Return _ignoreCase
'        End Get
'        Set(ByVal value As Boolean)
'            _ignoreCase = value
'        End Set
'    End Property

'    Public Sub New()
'        Me.ignoreCase = False
'    End Sub
'    Public Sub New(ByVal pRegEx As String, ByVal pRuleDescription As String, ByVal pIgnoreCase As Boolean)
'        MyBase.New()
'        Me._RegularExpressionText = pRegEx
'        If pIgnoreCase = True Then
'            Me.RegularExpression = New Text.RegularExpressions.Regex(pRegEx, Text.RegularExpressions.RegexOptions.IgnoreCase)
'        Else
'            Me.RegularExpression = New Text.RegularExpressions.Regex(pRegEx)
'        End If
'        Me.IgnoreCase = pIgnoreCase
'        Me.RuleDescription = pRuleDescription
'    End Sub
'    Public Sub New(ByVal pRegEx As Text.RegularExpressions.Regex, ByVal pRuleDescription As String)
'        Me.RegularExpression = pRegEx
'        Me._RegularExpressionText = Me.RegularExpression.ToString
'        Me.RuleDescription = pRuleDescription
'    End Sub

'End Class