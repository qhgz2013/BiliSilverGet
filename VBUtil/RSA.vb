Imports System.Security.Cryptography
Namespace RSA
    Public Module RSA_Converter
        Public Function ConvertFromPemPublicKey(ByVal pemFileConent As String) As System.Security.Cryptography.RSAParameters
            If String.IsNullOrEmpty(pemFileConent) Then
                Throw New ArgumentNullException("pemFileContent", "This arg cann't be empty.")
            End If
            pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "").Replace(vbCr, "").Replace(vbLf, "")
            Dim keyData() As Byte = Convert.FromBase64String(pemFileConent)


            Dim para As New System.Security.Cryptography.RSAParameters
            Dim pemModules() As Byte
            Dim pemPublicExponent() As Byte


            Select Case keyData.Length
                Case 162
                    'RSA 1024 bit
                    ReDim pemModules(127), pemPublicExponent(2)
                    Array.Copy(keyData, 29, pemModules, 0, 128)
                    Array.Copy(keyData, 159, pemPublicExponent, 0, 3)

                Case 94
                    'RSA 512 bit
                    ReDim pemModules(63), pemPublicExponent(2)
                    Array.Copy(keyData, 25, pemModules, 0, 64)
                    Array.Copy(keyData, 91, pemPublicExponent, 0, 3)

                Case Else
                    ReDim pemModules(0), pemPublicExponent(0)
                    Throw New ArgumentException("pem file content is incorrect.")

            End Select

            para.Modulus = pemModules
            para.Exponent = pemPublicExponent

            Return para

        End Function

        Private _rsa As RSACryptoServiceProvider
        Sub New()
            _rsa = New RSACryptoServiceProvider
        End Sub

        Public Function EncryptRSAData(ByVal data As Byte(), ByVal public_key_str As String) As Byte()
            _rsa.ImportParameters(ConvertFromPemPublicKey(public_key_str))
            Return _rsa.Encrypt(data, False)
        End Function
    End Module
End Namespace