
using System.Security.Cryptography;


namespace HealthRecordAPI.Help
{
    public class PasswordEncrypt
    {

        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 20;
        private static readonly int Iterations = 10000;

        public enum EncryptionType
        {
            MD5,
            sha1,
            base64

        }
        public static string Encrypt(string inputString, EncryptionType encryptionType)
        {
            string strReturn = null;
            try
            {
                switch (encryptionType)
                {
                    //case EncryptionType.MD5:
                    //    strReturn = FormsAuthentication.HashPasswordForStoringInConfigFile(inputString, "md5");
                    //    break;
                    //case EncryptionType.sha1:
                    //    strReturn = FormsAuthentication.HashPasswordForStoringInConfigFile(inputString, "sha1");
                    //    break;
                    //case EncryptionType.base64:
                    //    strReturn = EncryptAsBase64(inputString);
                    //    break;
                }
            }
            catch (Exception ex)
            {
                strReturn = ex.Message.ToString();
            }
            return strReturn;
        }

        public static string PasswordEncryption(string password)
        {

            byte[] salt;
            rng.GetBytes(salt = new byte[SaltSize]);
            var key = new Rfc2898DeriveBytes(password,salt,Iterations);
            var hash = key.GetBytes(HashSize);

            var hashbyte = new byte[SaltSize + HashSize];

            Array.Copy(salt, 0, hashbyte, 0, SaltSize);
            Array.Copy(hash, 0, hashbyte, SaltSize,HashSize);
            var base65Hash = Convert.ToBase64String(hashbyte);
            return base65Hash;
        }

        public static bool VerifyPassword(string password, string base64Hash)  
        {
            var hashBytes = Convert.FromBase64String(base64Hash);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            var key = new Rfc2898DeriveBytes(password,salt, Iterations);
            byte[] hash = key.GetBytes(HashSize);
            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }





    }
}
