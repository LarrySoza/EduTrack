using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EduTrack.Infrastructure
{
    public class Util
    {
        private const string keyEncrypt = "#'%$kilo2016";

        /// <summary>
        /// Encrypta una cadena usando doble metodo de cifrado
        /// </summary>
        /// <returns></returns>
        /// <param name="toEncrypt">cadena ah encriptar</param>
        /// <param name="useHashing">usar hashing? </param>
        /// <param name="key">Clave de encriptacion</param>
        public static string Encrypt(string toEncrypt, bool useHashing, string key = keyEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            //System.Windows.Forms.MessageBox.Show(key);
            if (useHashing)
            {
                MD5 hashmd5 = MD5.Create();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(key);

            TripleDES tdes = TripleDES.Create();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// DesEncrypta una cadena encriptada con doble metodo de cifrado
        /// </summary>
        /// <returns></returns>
        /// <param name="cipherString">cadena encriptada</param>
        /// <param name="useHashing">si se uso hash? para encriptar clave</param>
        /// <param name="key">Clave de encriptacion</param>
        public static string Decrypt(string cipherString, bool useHashing, string key = keyEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            if (useHashing)
            {
                MD5 hashmd5 = MD5.Create();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(key);

            TripleDES tdes = TripleDES.Create();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return Encoding.UTF8.GetString(resultArray);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Sha1Enconde(string? Sha1EncondeData)
        {
            if (string.IsNullOrEmpty(Sha1EncondeData))
                Sha1EncondeData = "";

            byte[] bytes = Encoding.UTF8.GetBytes(Sha1EncondeData);

            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);
        }

        public static byte[] Sha1HashBytes(string Sha1EncondeData)
        {
            if (Sha1EncondeData == null)
                Sha1EncondeData = "";

            byte[] bytes = Encoding.UTF8.GetBytes(Sha1EncondeData);

            var sha1 = SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static bool IsValidDni(string input)
        {
            var regex = new Regex(@"^[\d]{8}$");

            return regex.IsMatch(input ?? "");
        }
    }
}
