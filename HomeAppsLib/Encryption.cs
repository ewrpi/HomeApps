using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib
{
    public class Encryption
    {
        private byte[] bKey = new byte[] { (byte)31, (byte)245, (byte)135, (byte)80, (byte)7, (byte)65, (byte)87, (byte)24, (byte)36, (byte)219, (byte)132, (byte)198, (byte)73, (byte)55, (byte)46, (byte)11 };
        private byte[] bIV = new byte[] { (byte)220, (byte)31, (byte)19, (byte)154, (byte)174, (byte)68, (byte)19, (byte)241 };

        public static byte[] ConvertStringToByteArray(string stringToConvert)
        {
            return (new ASCIIEncoding()).GetBytes(stringToConvert);
        }


        public string Encrypt(string inVal)
        {
            try
            {
                //System.Text.Encoder encoding;
                //System.Text.Encoder encoding = System.Text.Encoding.ASCII

                System.IO.MemoryStream MSout = new System.IO.MemoryStream();

                //Create variables to help with read and write.
                byte[] bin; //This is intermediate storage for the encryption.
                System.Security.Cryptography.SymmetricAlgorithm encAlg = System.Security.Cryptography.SymmetricAlgorithm.Create("RC2");
                System.Security.Cryptography.CryptoStream encStream = new System.Security.Cryptography.CryptoStream(MSout, encAlg.CreateEncryptor(bKey, bIV), System.Security.Cryptography.CryptoStreamMode.Write);

                bin = ConvertStringToByteArray(inVal);
                encStream.Write(bin, 0, inVal.Length);
                encStream.Close();
                bin = MSout.ToArray();
                MSout.Close();

                return formatHexString(bin);
            }
            catch (System.Exception ex)
            {
                // Log Error
                throw ex;
            }
        }

        private byte[] DeformatHexString(string s)
        {
            try
            {
                Char c;
                byte b;
                byte[] retArray = new byte[Convert.ToInt16(s.Length / 2)];

                for (int x = 0; x < s.Length; x = x + 2)
                {
                    c = (char)(s.Substring(x, 1)[0]);
                    if (c >= 'A')
                        b = (byte)(((Convert.ToInt32(c) - Convert.ToInt32('A')) + 10) * 16);
                    else
                        b = (byte)((Convert.ToInt32(c) - Convert.ToInt32('0')) * 16);

                    c = (char)(s.Substring(x + 1, 1)[0]);
                    if (c >= 'A')
                        b += (byte)((Convert.ToInt32(c) - Convert.ToInt32('A')) + 10);
                    else
                        b += (byte)((Convert.ToInt32(c) - Convert.ToInt32('0')));

                    retArray[Convert.ToInt32(x / 2)] = b;
                }
                return retArray;
            }
            catch (System.Exception ex)
            {
                // Log Error
                throw ex;
            }
        }

        public string Decrypt(string inVal)
        {
            try
            {
                System.IO.MemoryStream MSout = new System.IO.MemoryStream();
                byte[] bin;
                byte[] retArr;

                //Create variables to help with read and write.
                System.Security.Cryptography.SymmetricAlgorithm encAlg = System.Security.Cryptography.SymmetricAlgorithm.Create("RC2");
                System.Security.Cryptography.CryptoStream DecStream = new System.Security.Cryptography.CryptoStream(MSout, encAlg.CreateDecryptor(bKey, bIV), System.Security.Cryptography.CryptoStreamMode.Write);
                bin = DeformatHexString(inVal);

                DecStream.Write(bin, 0, bin.Length);
                DecStream.Close();
                retArr = MSout.ToArray();

                MSout.Close();
                System.Text.ASCIIEncoding getStr = new ASCIIEncoding();

                return getStr.GetString(retArr);
            }
            catch (System.Exception ex)
            {
                // Log Error
                throw ex;
            }
        }



        private string formatHexString(byte[] a)
        {
            try
            {
                string retStr = string.Empty;
                byte sixteens;
                byte singles;

                for (int x = 0; x < a.Length; x++)
                {
                    sixteens = (byte)(Convert.ToInt16(a[x] / 16));
                    if (sixteens >= 10)
                        retStr += Convert.ToChar((sixteens - 10) + Convert.ToInt32('A'));
                    else
                        retStr += Convert.ToChar(sixteens + Convert.ToInt32('0'));

                    singles = (byte)(a[x] % 16);
                    if (singles >= 10)
                        retStr += Convert.ToChar((singles - 10) + Convert.ToInt32('A'));
                    else
                        retStr += Convert.ToChar(singles + Convert.ToInt32('0'));
                }
                return retStr;
            }
            catch (System.Exception ex)
            {
                // Log Error
                throw ex;
            }
        }
    }
}
