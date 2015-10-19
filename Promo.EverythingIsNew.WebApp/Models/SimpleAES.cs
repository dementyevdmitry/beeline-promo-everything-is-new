using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{



    public class SimpleAES
    {
        //  ключи должны быть по 32 байта

        // Change these keys
        private byte[] Key = { 5,66,22,11,22,33,44,55,65,3,23,1,62,96,43,64,14,28,32,12,54,32,56,89,09,98,47,74,85,35,35,3,1,14,141,174,127,233,248, 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
        private byte[] Vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 25, 21, 112, 79, 32, 114, 156 };


        private ICryptoTransform EncryptorTransform, DecryptorTransform;
        private System.Text.UTF8Encoding UTFEncoder;

        public SimpleAES()
        {
            //This is our encryption method
            RijndaelManaged rm = new RijndaelManaged();

            //Create an encryptor and a decryptor using our encryption method, key, and vector.
            EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
            DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

            //Used to translate bytes to text and vice versa
            UTFEncoder = new System.Text.UTF8Encoding();
        }




        /// Encrypt some text and return an encrypted byte array.
        public byte[] Encrypt(string TextValue)
        {
            //Translates our text value into a byte array.
            Byte[] bytes = UTFEncoder.GetBytes(TextValue);

            //Used to stream the data in and out of the CryptoStream.
            MemoryStream memoryStream = new MemoryStream();

            /*
             * We will have to write the unencrypted bytes to the stream,
             * then read the encrypted result back from the stream.
             */
            #region Write the decrypted value to the encryption stream
            CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            #endregion

            #region Read encrypted value back out of the stream
            memoryStream.Position = 0;
            byte[] encrypted = new byte[memoryStream.Length];
            memoryStream.Read(encrypted, 0, encrypted.Length);
            #endregion

            //Clean up.
            cs.Close();
            memoryStream.Close();

            return encrypted;
        }

        /// The other side: Decryption methods
        public string DecryptString(string EncryptedString)
        {
            return Decrypt(StrToByteArray(EncryptedString));
        }

        /// Decryption when working with byte arrays.    
        public string Decrypt(byte[] EncryptedValue)
        {
            #region Write the encrypted value to the decryption stream
            MemoryStream encryptedStream = new MemoryStream();
            CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
            decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
            decryptStream.FlushFinalBlock();
            #endregion

            #region Read the decrypted value from the stream.
            encryptedStream.Position = 0;
            Byte[] decryptedBytes = new Byte[encryptedStream.Length];
            encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
            encryptedStream.Close();
            #endregion
            return UTFEncoder.GetString(decryptedBytes);
        }

        /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array from a string using an ASCII encoding (like so).
        //      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        //      return encoding.GetBytes(str);
        // However, this results in character values that cannot be passed in a URL.  So, instead, I just
        // lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
        public byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
                throw new Exception("Invalid string value in StrToByteArray");

            byte val;
            byte[] byteArr = new byte[str.Length / 3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            }
            while (i < str.Length);
            return byteArr;
        }










    }


    public static class CryptoStreamHelper
    {
        private static SymmetricAlgorithm GetAlgorithm(string password)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, null);

            SymmetricAlgorithm csp = new RijndaelManaged();

            csp.BlockSize = csp.LegalBlockSizes[0].MaxSize;
            csp.Key = pdb.GetBytes(csp.LegalKeySizes[0].MaxSize / 8);
            csp.IV = pdb.GetBytes(csp.BlockSize / 8);
            
            return csp;
        }

        public static CryptoStream GetStreamToWrite(Stream baseStream, string password)
        {
            return new CryptoStream(baseStream, GetAlgorithm(password).CreateEncryptor(), CryptoStreamMode.Write);
        }

        public static CryptoStream GetStreamToRead(Stream baseStream, string password)
        {
            return new CryptoStream(baseStream, GetAlgorithm(password).CreateDecryptor(), CryptoStreamMode.Read);



           



        }


        //private SymmetricAlgorithm alg;

        //public void A()
        //{
        //    alg = (SymmetricAlgorithm)RijndaelManaged.Create(); //пример создания класса RijndaelManaged

        //    PasswordDeriveBytes pdb = new PasswordDeriveBytes("", null); //класс, позволяющий генерировать ключи на базе паролей
        //    pdb.HashName = "SHA512"; //будем использовать SHA512
        //    int keylen = (int)10; //получаем размер ключа из ComboBox’а
        //    alg.KeySize = keylen; //устанавливаем размер ключа
        //    alg.Key = pdb.GetBytes(keylen >> 3); //получаем ключ из пароля
        //    alg.Mode = CipherMode.CBC; //используем режим CBC
        //    alg.IV = new Byte[alg.BlockSize >> 3]; //и пустой инициализационный вектор
        //    ICryptoTransform tr = alg.CreateEncryptor(); //создаем encryptor

        //    FileStream instream = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read);
        //    FileStream outstream = new FileStream("", FileMode.Create, FileAccess.Write, FileShare.None);
        //    int buflen = ((2 << 16) / alg.BlockSize) * alg.BlockSize;
        //    byte[] inbuf = new byte[buflen];
        //    byte[] outbuf = new byte[buflen];
        //    int len;
        //    while ((len = instream.Read(inbuf, 0, buflen)) == buflen)
        //    {
        //        int enclen = tr.TransformBlock(inbuf, 0, buflen, outbuf, 0); //собственно шифруем
        //        outstream.Write(outbuf, 0, enclen);
        //    }
        //    instream.Close();
        //    outbuf = tr.TransformFinalBlock(inbuf, 0, len); //шифруем финальный блок
        //    outstream.Write(outbuf, 0, outbuf.Length);
        //    outstream.Close();
            
        //}





    }
}