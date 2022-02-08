Imports System.IO
Imports System.Security.Cryptography

''' <summary>
''' Enum of all the Keys
''' </summary>
''' <remarks></remarks>
Public Enum AESKey
    BasicLibrary = 0
    SpecialOffspring = 1
    IntegratorGuide = 2
    MinistryAvenue = 3
End Enum

Public Class Encryption

    ''' <summary>
    '''  The Key used by iCensus
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared BasicLibraryAESKey() As Byte = System.Text.Encoding.UTF8.GetBytes("932eid*rieu&af3%3g%ed(p.{'Jew&1h")
    Private Shared SpecialOffspringAESKey() As Byte = System.Text.Encoding.UTF8.GetBytes("93keid*rieu&hfy%3D%eo(p.{'Jeh&1h")
    Private Shared IntegratorGuideAESKey() As Byte = System.Text.Encoding.UTF8.GetBytes("i39ejdue93*73jE7&en3*3w)sbe#ne!k")
    Private Shared MinistryAvenueAESKey() As Byte = System.Text.Encoding.UTF8.GetBytes("3ide0ke3#&emdA*3md!mdl;snd&hdns@")

    ''' <summary>
    ''' Encrypts the input string with 256 bit AES encryption
    ''' </summary>
    ''' <param name="strInput">The string to be encrypted</param>
    ''' <param name="eAESKey">The key to use</param>
    ''' <param name="IV">The IV to use</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Encrypt(ByVal strInput As String, ByVal eAESKey As AESKey, ByVal IV() As Byte) As Byte()

        Return Encrypt(System.Text.Encoding.UTF8.GetBytes(strInput), eAESKey, IV)

    End Function

    Public Shared Function Encrypt(ByVal Input() As Byte, ByVal eAESKey As AESKey, ByVal IV() As Byte) As Byte()
        Return Encrypt(Input, GetAESKey(eAESKey), IV)
    End Function

    Public Shared Function Encrypt(ByVal strInput As String, ByVal AESKey() As Byte, ByVal IV() As Byte) As Byte()
        Return Encrypt(System.Text.Encoding.UTF8.GetBytes(strInput), AESKey, IV)
    End Function
    ''' <summary>
    ''' Encrypts the input with 256 bit AES encryption
    ''' </summary>
    ''' <param name="Input">The byte() to be encrypted</param>
    ''' <param name="AESKey">The key to use</param>
    ''' <param name="IV">The IV to use</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Encrypt(ByVal Input() As Byte, ByVal AESKey() As Byte, ByVal IV() As Byte) As Byte()

        ' Check arguments.
        If Input Is Nothing OrElse Input.Length <= 0 Then
            Throw New ArgumentNullException("Input is Null")
        End If
        If IV Is Nothing OrElse Not IV.Length = 32 Then
            Throw New ArgumentException("IV length is not 32")
        End If

        ' Declare the streams used
        ' to encrypt to an in memory
        ' array of bytes.
        Dim msEncrypt As MemoryStream = Nothing
        Dim csEncrypt As CryptoStream = Nothing

        ' Declare the RijndaelManaged object
        ' used to encrypt the data.
        Dim aesAlg As RijndaelManaged = Nothing

        ' Declare the bytes used to hold the
        ' encrypted data.
        Dim encrypted As Byte() = Nothing

        Try
            ' Create a RijndaelManaged object
            ' with the specified key and IV.
            aesAlg = New RijndaelManaged()
            aesAlg.KeySize = 256
            aesAlg.Key = AESKey
            aesAlg.BlockSize = 256
            aesAlg.IV = IV

            ' Create a decrytor to perform the stream transform.
            Dim encryptor As ICryptoTransform = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV)

            ' Create the streams used for encryption.
            msEncrypt = New MemoryStream()
            csEncrypt = New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)

            'Write all data to the stream.
            csEncrypt.Write(Input, 0, Input.Length)

        Finally
            ' Clean things up.
            ' Close the streams.

            If Not (csEncrypt Is Nothing) Then
                csEncrypt.Close()
            End If
            If Not (msEncrypt Is Nothing) Then
                msEncrypt.Close()
            End If
            ' Clear the RijndaelManaged object.
            If Not (aesAlg Is Nothing) Then
                aesAlg.Clear()
            End If
        End Try

        ' Return the encrypted bytes from the memory stream.
        Return msEncrypt.ToArray()

    End Function

    Public Shared Function GetNewIV() As Byte()
        Dim objRij As New RijndaelManaged()
        objRij.BlockSize = 256
        Return (objRij.IV)
    End Function

    ''' <summary>
    ''' retrieves the correct key for the enum 
    ''' </summary>
    ''' <param name="eAESKey"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetAESKey(ByVal eAESKey As AESKey) As Byte()

        Select Case eAESKey
            Case AESKey.BasicLibrary
                Return BasicLibraryAESKey
            Case AESKey.SpecialOffspring
                Return SpecialOffspringAESKey
            Case AESKey.IntegratorGuide
                Return IntegratorGuideAESKey
            Case AESKey.MinistryAvenue
                Return MinistryAvenueAESKey
            Case Else
                Throw New ApplicationException("No key available for " & eAESKey.ToString)

        End Select

    End Function

    ''' <summary>
    ''' Decrypts the Cipher byte() 
    ''' </summary>
    ''' <param name="Cipher">The byte() to be decrypted</param>
    ''' <param name="eAESKey">The key to use</param>
    ''' <param name="IV">The IV to use</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DecryptToString(ByVal Cipher() As Byte, ByVal eAESKey As AESKey, ByVal IV() As Byte) As String

        Return System.Text.Encoding.UTF8.GetString(DecryptToByteArray(Cipher, GetAESKey(eAESKey), IV))

    End Function
    Public Shared Function DecryptToString(ByVal Cipher() As Byte, ByVal AESKey() As Byte, ByVal IV() As Byte) As String

        Return System.Text.Encoding.UTF8.GetString(DecryptToByteArray(Cipher, AESKey, IV))

    End Function

    Public Shared Function DecryptToByteArray(ByVal Cipher() As Byte, ByVal eAESKey As AESKey, ByVal IV() As Byte) As Byte()
        Return DecryptToByteArray(Cipher, GetAESKey(eAESKey), IV)
    End Function

    ''' <summary>
    ''' Unencrypts the Cipher byte() 
    ''' </summary>
    ''' <param name="Cipher">The byte() to be unencrypted</param>
    ''' <param name="AESKey">The key to use</param>
    ''' <param name="IV">The IV to use</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DecryptToByteArray(ByVal Cipher() As Byte, ByVal AESKey() As Byte, ByVal IV() As Byte) As Byte()

        ' Check arguments.
        If Cipher Is Nothing OrElse Cipher.Length <= 0 Then
            Throw New ArgumentNullException("cipher is null")
        End If

        ' TDeclare the streams used
        ' to decrypt to an in memory
        ' array of bytes.
        Dim msDecrypt As MemoryStream = Nothing
        Dim csDecrypt As CryptoStream = Nothing

        ' Declare the RijndaelManaged object
        ' used to decrypt the data.
        Dim aesAlg As RijndaelManaged = Nothing

        Dim baDeCrypted() As Byte
        Dim iDeCryptedByteCount As Integer

        Try
            ' Create a RijndaelManaged object
            ' with the specified key and IV.
            aesAlg = New RijndaelManaged()
            aesAlg.KeySize = 256
            aesAlg.Key = AESKey
            aesAlg.BlockSize = 256
            aesAlg.IV = IV


            ' Create a decrytor to perform the stream transform.
            Dim decryptor As ICryptoTransform = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV)

            ' Create the streams used for decryption.
            msDecrypt = New MemoryStream(Cipher)
            csDecrypt = New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)

            ReDim baDeCrypted(Cipher.Length)
            iDeCryptedByteCount = csDecrypt.Read(baDeCrypted, 0, Cipher.Length)
            ReDim Preserve baDeCrypted(iDeCryptedByteCount - 1)

        Catch ex As Exception
            Dim message As String = ex.Message

        Finally
            ' Clean things up.
            ' Close the streams.


            If csDecrypt IsNot Nothing Then
                csDecrypt.Close()
            End If
            If Not (msDecrypt Is Nothing) Then
                msDecrypt.Close()
            End If
            ' Clear the RijndaelManaged object.
            If Not (aesAlg Is Nothing) Then
                aesAlg.Clear()
            End If

        End Try

        Return baDeCrypted

    End Function


End Class
