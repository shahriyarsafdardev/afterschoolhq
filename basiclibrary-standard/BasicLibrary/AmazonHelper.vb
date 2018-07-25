Imports System.IO

Public Class AmazonHelper

    Private _accessKeyId As String
    Public Property AccessKeyId() As String
        Get
            Return _accessKeyId
        End Get
        Set(ByVal value As String)
            _accessKeyId = value
        End Set
    End Property
    Private _SecretAccessKeyId As String
    Public Property SecretAccessKeyId() As String
        Get
            Return _SecretAccessKeyId
        End Get
        Set(ByVal value As String)
            _SecretAccessKeyId = value
        End Set
    End Property

    Private _bucketName As String
    Public Property BucketName() As String
        Get
            Return _bucketName
        End Get
        Set(ByVal value As String)
            _bucketName = value
        End Set
    End Property

    Private _RegionEndpoint As Amazon.RegionEndpoint
    Public Property RegionEndPoint() As Amazon.RegionEndpoint
        Get
            Return _RegionEndpoint
        End Get
        Set(ByVal value As Amazon.RegionEndpoint)
            _RegionEndpoint = value
        End Set
    End Property

    Public ReadOnly Property AuthenticatedUrl As String
        Get
            Return "http://" + Me.AccessKeyId + ":" + System.Uri.EscapeDataString(Me.SecretAccessKeyId) + "@" + Me.BucketName + ".s3.amazonaws.com/"
        End Get
    End Property

    Public Shared Function ValidateCertificate(ByVal sender As Object, ByVal certificate As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal errors As System.Net.Security.SslPolicyErrors)
        If certificate.Subject = "CN=*.s3.amazonaws.com, O=Amazon.com Inc., L=Seattle, S=Washington, C=US" Then
            Return True
        Else
            Return True
        End If
    End Function

    Private Function GetS3Client()
        Return New Amazon.S3.AmazonS3Client(Me.AccessKeyId, Me.SecretAccessKeyId, Me.RegionEndPoint)
    End Function

    Public Sub New()

    End Sub
    Public Sub New(ByVal bucketName As String)
        Me.BucketName = bucketName
    End Sub
    Public Sub New(ByVal bucketName As String, ByVal accessKeyId As String, ByVal secretAccessKeyId As String)
        Me.BucketName = bucketName
        Me.AccessKeyId = accessKeyId
        Me.SecretAccessKeyId = secretAccessKeyId
    End Sub
    Public Sub New(ByVal bucketName As String, ByVal accessKeyId As String, ByVal secretAccessKeyId As String, ByVal regionEndPoint As Amazon.RegionEndpoint)
        Me.BucketName = bucketName
        Me.AccessKeyId = accessKeyId
        Me.SecretAccessKeyId = secretAccessKeyId
        Me.RegionEndPoint = regionEndPoint
    End Sub

    Public Function AddFile(ByVal fromFileLocation As String, ByVal amazonLocation As String) As SystemMessages
        Return AddFile(fromFileLocation, amazonLocation, False)
    End Function
    Public Function AddFile(ByVal fromFileLocation As String, ByVal amazonLocation As String, ByVal makePublic As Boolean) As SystemMessages

        Dim msgs As SystemMessages = New SystemMessages()
        Try

            System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AmazonHelper.ValidateCertificate)

            Dim client As New Amazon.S3.AmazonS3Client(Me.AccessKeyId, Me.SecretAccessKeyId)

            If client.IsEmpty() = False Then
                Dim tu As Amazon.S3.Transfer.TransferUtility = New Amazon.S3.Transfer.TransferUtility(client)

                Dim request As New Amazon.S3.Transfer.TransferUtilityUploadRequest With {
                    .FilePath = fromFileLocation,
                    .BucketName = Me.BucketName,
                    .Key = amazonLocation
                }

                'request.WithFilePath(fromFileLocation).WithBucketName(Me.BucketName).WithKey(amazonLocation)

                If makePublic Then
                    request.CannedACL = Amazon.S3.S3CannedACL.PublicRead
                End If

                tu.Upload(request)
            End If

        Catch ex As Exception

            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function


    Public Function RemoveFile(ByVal amazonLocation As String) As SystemMessages
        Dim msgs As SystemMessages = New SystemMessages()

        Try

            Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim request As Amazon.S3.Model.DeleteObjectRequest = New Amazon.S3.Model.DeleteObjectRequest()
                request.BucketName = Me.BucketName
                request.Key = amazonLocation

                client.DeleteObjectAsync(request)
            End If

        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function

    Public Function AddFile(ByVal inputStream As System.IO.Stream, ByVal amazonLocation As String) As SystemMessages
        Return AddFile(inputStream, amazonLocation, False)
    End Function
    Public Function AddFile(ByVal inputStream As System.IO.Stream, ByVal amazonLocation As String, ByVal makePublic As Boolean) As SystemMessages

        Dim msgs As SystemMessages = New SystemMessages()
        Try

            System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AmazonHelper.ValidateCertificate)

            Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim tu As Amazon.S3.Transfer.TransferUtility = New Amazon.S3.Transfer.TransferUtility(client)

                Dim request As Amazon.S3.Transfer.TransferUtilityUploadRequest = New Amazon.S3.Transfer.TransferUtilityUploadRequest()
                request.BucketName = Me.BucketName
                request.Key = amazonLocation
                request.InputStream = inputStream

                If makePublic Then
                    request.CannedACL = Amazon.S3.S3CannedACL.PublicRead
                End If

                tu.Upload(request)
            End If

        Catch ex As Exception

            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function

    Public Function GetFile(ByVal amazonFileLocation As String, ByVal msgs As SystemMessages) As MemoryStream
        Dim file As Byte() = Nothing
        Dim ms As MemoryStream = New MemoryStream()

        Try

            System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AmazonHelper.ValidateCertificate)

            Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then

                Dim request As New Amazon.S3.Model.GetObjectRequest()
                request.BucketName = Me.BucketName
                request.Key = amazonFileLocation

                Dim t As Task(Of Amazon.S3.Model.GetObjectResponse) = Task.Run(
                    Async Function()
                        Return Await client.GetObjectAsync(request)
                    End Function
                )
                t.Wait()

                Dim response As Amazon.S3.Model.GetObjectResponse = t.Result
                Dim s As Stream = response.ResponseStream

                Dim data As Byte()
                ReDim data(32768)
                Dim bytesRead As Integer = 0
                Do
                    bytesRead = s.Read(data, 0, data.Length)
                    ms.Write(data, 0, bytesRead)
                Loop While bytesRead > 0

                ms.Flush()

            End If

        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return ms
    End Function
    Public Function GetFileAndWrite(ByVal amazonFileLocation As String, ByVal localFileLocation As String) As SystemMessages
        Dim msgs As SystemMessages = New SystemMessages()

        System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AmazonHelper.ValidateCertificate)

        Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

        Dim request As Amazon.S3.Model.GetObjectRequest = New Amazon.S3.Model.GetObjectRequest()
        request.BucketName = Me.BucketName
        request.Key = amazonFileLocation

        Try
            Dim t As Task(Of Amazon.S3.Model.GetObjectResponse) = Task.Run(
                Async Function()
                    Return Await client.GetObjectAsync(request)
                End Function
            )
            t.Wait()

            Dim response As Amazon.S3.Model.GetObjectResponse = t.Result

            If System.IO.File.Exists(localFileLocation) = False Then
                response.WriteResponseStreamToFileAsync(localFileLocation, False, Nothing)
            End If

        Catch ex As Exception

            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function

    Public Function GetFileKeys(ByVal amazonLocation As String) As List(Of String)
        Dim msgs As SystemMessages = New SystemMessages()
        Dim list As List(Of String) = New List(Of String)

        Try
            System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AmazonHelper.ValidateCertificate)

            Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then

                Dim request As Amazon.S3.Model.ListObjectsRequest = New Amazon.S3.Model.ListObjectsRequest()
                request.BucketName = Me.BucketName
                request.Prefix = amazonLocation
                request.Delimiter = "/"

                Dim t As Task(Of Amazon.S3.Model.ListObjectsResponse) = Task.Run(
                    Async Function()
                        Return Await client.ListObjectsAsync(request)
                    End Function
                )
                t.Wait()

                Dim response As Amazon.S3.Model.ListObjectsResponse = t.Result

                For Each item As Amazon.S3.Model.S3Object In response.S3Objects
                    list.Add(item.Key)
                Next

                If response.CommonPrefixes.Count > 0 Then
                    For Each prefix As String In response.CommonPrefixes
                        list.AddRange(GetFileKeys(prefix))
                    Next
                End If

            End If


        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return list

    End Function

    Public Function GetFileLink(ByVal amazonLocation As String, Optional ByVal useHTTPS As Boolean = False, Optional ByVal expirationDate As Date? = Nothing, Optional IsPublic As Boolean = False) As String
        Dim msgs As SystemMessages = New SystemMessages()
        Dim link As String = ""

        If expirationDate Is Nothing Then
            expirationDate = DateTime.Now.AddMinutes(1)
        End If

        Try

            Dim client As Amazon.S3.AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim request As Amazon.S3.Model.GetPreSignedUrlRequest = New Amazon.S3.Model.GetPreSignedUrlRequest()
                request.BucketName = BucketName
                request.Key = amazonLocation
                request.Expires = expirationDate
                request.Protocol = Amazon.S3.Protocol.HTTP
                If useHTTPS Then
                    request.Protocol = Amazon.S3.Protocol.HTTPS
                End If
                link = client.GetPreSignedURL(request)

                If IsPublic Then
                    link = link.Split("?")(0)
                End If
            End If

        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return link

    End Function


End Class