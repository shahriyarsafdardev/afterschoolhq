Imports System.IO
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports Amazon
Imports Amazon.S3
Imports Amazon.S3.Model
Imports Amazon.S3.Transfer

Public Class AmazonHelper
    Private Property AccessKeyId As String
    Private Property SecretAccessKeyId As String
    Private Property BucketName As String
    Private Property RegionEndPoint As RegionEndpoint

    Private Shared Function ValidateCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain,
                                                errors As SslPolicyErrors)
        If certificate.Subject = "CN=*.s3.amazonaws.com, O=Amazon.com Inc., L=Seattle, S=Washington, C=US" Then
            Return True
        Else
            Return True
        End If
    End Function

    Private Function GetS3Client()
        Return New AmazonS3Client(AccessKeyId, SecretAccessKeyId, RegionEndPoint)
    End Function

    Public Sub New(bucketName As String, accessKeyId As String, secretAccessKeyId As String,
                   regionEndPoint As RegionEndpoint)
        Me.BucketName = bucketName
        Me.AccessKeyId = accessKeyId
        Me.SecretAccessKeyId = secretAccessKeyId
        Me.RegionEndPoint = regionEndPoint
    End Sub

    Public Function AddFile(fromFileLocation As String, amazonLocation As String, makePublic As Boolean) _
        As SystemMessages

        Dim msgs = New SystemMessages()
        Try

            ServicePointManager.ServerCertificateValidationCallback =
                New RemoteCertificateValidationCallback(AddressOf ValidateCertificate)

            Dim client As New AmazonS3Client(AccessKeyId, SecretAccessKeyId)

            If client.IsEmpty() = False Then
                Dim tu = New TransferUtility(client)

                Dim request As New TransferUtilityUploadRequest With {
                        .FilePath = fromFileLocation,
                        .BucketName = BucketName,
                        .Key = amazonLocation
                        }

                If makePublic Then
                    request.CannedACL = S3CannedACL.PublicRead
                End If

                tu.Upload(request)
            End If

        Catch ex As Exception

            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function


    Public Function RemoveFile(amazonLocation As String) As SystemMessages
        Dim msgs = New SystemMessages()

        Try

            Dim client As AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim request = New DeleteObjectRequest()
                request.BucketName = BucketName
                request.Key = amazonLocation

                client.DeleteObjectAsync(request)
            End If

        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return msgs
    End Function

    Public Function AddFile(inputStream As Stream, amazonLocation As String) As SystemMessages
        Return AddFile(inputStream, amazonLocation, False)
    End Function

    Public Function AddFile(inputStream As Stream, amazonLocation As String, makePublic As Boolean) As SystemMessages

        Dim msgs = New SystemMessages()
        Try

            ServicePointManager.ServerCertificateValidationCallback =
                New RemoteCertificateValidationCallback(AddressOf ValidateCertificate)

            Dim client As AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim tu = New TransferUtility(client)

                Dim request As New TransferUtilityUploadRequest()
                request.BucketName = BucketName
                request.Key = amazonLocation
                request.InputStream = inputStream

                If makePublic Then
                    request.CannedACL = S3CannedACL.PublicRead
                End If

                tu.Upload(request)
            End If

        Catch ex As Exception

            msgs.Add(ex.Message, MessageType.Error)

        End Try

        Return msgs
    End Function

    Public Function GetFileLink(amazonLocation As String, Optional ByVal expirationDate As Date? = Nothing,
                                Optional isPublic As Boolean = True) As String
        Dim msgs = New SystemMessages()
        Dim link = ""

        If expirationDate Is Nothing Then
            expirationDate = DateTime.Now.AddMinutes(1)
        End If

        Try

            Dim client As AmazonS3Client = GetS3Client()

            If client.IsEmpty() = False Then
                Dim request = New GetPreSignedUrlRequest()
                request.BucketName = BucketName
                request.Key = amazonLocation
                request.Expires = expirationDate
                request.Protocol = Protocol.HTTPS
                link = client.GetPreSignedURL(request)

                If isPublic Then
                    link = link.Split("?")(0)
                End If
            End If

        Catch ex As Exception
            msgs.Add(ex.Message, MessageType.Error)
        End Try

        Return link
    End Function
End Class