using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//This class was taken from StackOverflow, with minimal changes
public static class Security
{
    public static string Encrypt(string textToEncrypt)
    {
        try
        {
            string ToReturn;
            //The secret key to use for the symmetric algorithm
            string _key = "ay$a5%&jwrtmnh;lasjdf98787";
            //The initialization vector to use for the symmetric algorithm.
            string _iv = "abc@98797hjkas$&asd(*$%";
            // The secret key as byte
            byte[] _ivByte = { };
            _ivByte = System.Text.Encoding.UTF8.GetBytes(_iv.Substring(0, 8));
            //The initialization vector as byte
            byte[] _keybyte = { };
            _keybyte = System.Text.Encoding.UTF8.GetBytes(_key.Substring(0, 8));
            MemoryStream memStream = null; 
            CryptoStream crypStrem = null;
            //The password as byte
            byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
            //Using the Encrytion service to make sure it desposes
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                //Initializes the memStream
                memStream = new MemoryStream();
                //Initializes a new instance of CryptoStream with a target data stream.
                crypStrem = new CryptoStream(memStream, des.CreateEncryptor(_keybyte, _ivByte), CryptoStreamMode.Write);
                //Writes the encrytion
                crypStrem.Write(inputbyteArray, 0, inputbyteArray.Length);
                crypStrem.FlushFinalBlock();
                ToReturn = Convert.ToBase64String(memStream.ToArray());
            }
            return ToReturn;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
    }
    public static string Decrypt(string textToDecrypt)
    {
        try
        {
            string ToReturn;
            //The secret key to use for the symmetric algorithm
            string _key = "ay$a5%&jwrtmnh;lasjdf98787";
            //The initialization vector to use for the symmetric algorithm.
            string _iv = "abc@98797hjkas$&asd(*$%";
            // The secret key as byte
            byte[] _keybyte = { };
            _keybyte = System.Text.Encoding.UTF8.GetBytes(_key.Substring(0, 8));
            //The initialization vector as byte
            byte[] _ivByte = { };
            _ivByte = System.Text.Encoding.UTF8.GetBytes(_iv.Substring(0, 8));
            MemoryStream memStream = null;
            CryptoStream crypStrem = null;
            //The password as byte
            byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
            inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
            //Using the Encrytion service to make sure it desposes
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                //Initializes the memStream
                memStream = new MemoryStream();
                //Initializes a new instance of CryptoStream with a target data stream.
                crypStrem = new CryptoStream(memStream, des.CreateDecryptor(_keybyte, _ivByte), CryptoStreamMode.Write);
                //Writes the decrytion
                crypStrem.Write(inputbyteArray, 0, inputbyteArray.Length);
                crypStrem.FlushFinalBlock();
                Encoding encoding = Encoding.UTF8;
                ToReturn = encoding.GetString(memStream.ToArray());
            }
            return ToReturn;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
    }
}