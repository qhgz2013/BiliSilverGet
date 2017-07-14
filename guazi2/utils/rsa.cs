using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace guazi2
{
    public class RSA
    {
        public static RSAParameters ConvertFromPemPublicKey(string pemFileContent)
        {
            if (string.IsNullOrEmpty(pemFileContent)) throw new ArgumentNullException("penFileContent");
            pemFileContent = pemFileContent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\r", "").Replace("\n", "").Replace("\\r", "").Replace("\\n", "");
            var keyData = Convert.FromBase64String(pemFileContent);

            var para = new RSAParameters();
            byte[] pemModules, pemPublicExponent;

            switch (keyData.Length)
            {
                case 162:
                    //RSA 1024
                    pemModules = new byte[128];
                    pemPublicExponent = new byte[3];
                    Array.Copy(keyData, 29, pemModules, 0, 128);
                    Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
                    break;
                case 94:
                    //RSA 512
                    pemModules = new byte[64];
                    pemPublicExponent = new byte[3];
                    Array.Copy(keyData, 25, pemModules, 0, 64);
                    Array.Copy(keyData, 91, pemPublicExponent, 0, 3);
                    break;
                default:
                    throw new ArgumentException("Invalid PEM Length");
            }
            para.Modulus = pemModules;
            para.Exponent = pemPublicExponent;
            return para;
        }
        private static RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();
        public static byte[] EncryptRSAData(byte[] data, string public_key)
        {
            _rsa.ImportParameters(ConvertFromPemPublicKey(public_key));
            return _rsa.Encrypt(data, false);
        }
    }
}
