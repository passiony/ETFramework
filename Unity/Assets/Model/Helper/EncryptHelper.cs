using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ETModel
{
    /// <summary>
    /// AES 对称加密算法
    /// </summary>
    public static class EncryptHelper
    {
        private const string EncryptKey="ABCDEFGHIJKLMN0PQ";

        public static byte[] Encrypt(string Data)
        {
#if ENCRYPT
            return EncryptToBytes(Data, EncryptKey);
#else
            return Encoding.UTF8.GetBytes(Data);
#endif
        }

        public static string Decrypt(byte[] Data)
        {
#if ENCRYPT
            return DecryptFromBytes(Data, EncryptKey);
#else
            return Encoding.UTF8.GetString(Data);
#endif
        }

        public static byte[] EncryptBytes(byte[] Data)
        {
#if ENCRYPT
            return EncryptBytes(Data, EncryptKey);
#else
            return Data;
#endif
        }

        public static byte[] DecryptBytes(byte[] Data)
        {
#if ENCRYPT
            return DecryptBytes(Data, EncryptKey);
#else
            return Data;
#endif
        }

        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="plainBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        private static string EncryptToString(string Data, string Key)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
            var bytes = EncryptBytes(plainBytes, Key);
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="encryptedBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        private static string DecryptFromString(string Data, string Key)
        {
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(Data);
            var ret = DecryptBytes(encryptedBytes, Key);
            return Encoding.UTF8.GetString(ret);
        }

        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="plainBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        private static byte[] EncryptToBytes(string Data, string Key)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
            return EncryptBytes(plainBytes, Key);
        }


        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="encryptedBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        private static string DecryptFromBytes(byte[] encryptedBytes, string Key)
        {
            byte[] bytes = DecryptBytes(encryptedBytes, Key);
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="plainBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        private static byte[] EncryptBytes(byte[] bytes, string Key)
        {
            MemoryStream mStream = new MemoryStream();
            RijndaelManaged aes = new RijndaelManaged();

            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();

                return mStream.ToArray();
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }


        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="encryptedBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        private static byte[] DecryptBytes(byte[] bytes, string Key)
        {
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(bytes);
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] tmp = new byte[bytes.Length + 32];
                int len = cryptoStream.Read(tmp, 0, bytes.Length + 32);
                byte[] ret = new byte[len];
                Array.Copy(tmp, 0, ret, 0, len);
                return ret;
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }
    }
}