using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Nimbus.Web.Security
{
    public static class NSPHash
    {
        private static const int PBKDF2_ITERS = 1024;
        private static const int SALT_BYTES = 16;
        public static int Version { get { return 1; } }
        public static int Iters { get { return PBKDF2_ITERS; } }

        private int _hashIters;
        private string _b64salt;
        private string _b64hash;

        /// <summary>
        /// Gerador de Salt seguro de 128 bits.
        /// </summary>
        /// <returns>Salt encodado em base64</returns>
        public static string GenerateSalt()
        {
            byte[] rndSalt = new byte[SALT_BYTES];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(rndSalt);
            }

            return Convert.ToBase64String(rndSalt);
        }

        /// <summary>
        /// Backs Secure and System hashes.
        ///
        /// Uses PBKDF2 internally, as implemented by the Rfc2998DeriveBytes class.
        ///
        /// See http://en.wikipedia.org/wiki/PBKDF2
        /// and http://msdn.microsoft.com/en-us/library/bwx8t0yt.aspx
        /// </summary>
        public static string Pbkdf2Hash(string value, string salt, int iters = PBKDF2_ITERS)
        {
            var idxIters = salt.IndexOf(':');
            if (idxIters != -1)
            {
                iters = int.Parse(salt.Substring(0, idxIters));
                salt = salt.Substring(idxIters + 1);    
            }

            if (iters < 1024) throw new Exception("Too few iters.");

            

            using (var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(value), Convert.FromBase64String(salt), iters))
            {
                var key = pbkdf2.GetBytes(24);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// Inicializa um NSPHash versão 1. Para hashear uma string, veja PlaintextPassword
        /// </summary>
        /// <param name="hash"></param>
        public NSPHash(string hash)
        {
            try
            {
                string[] hashparts = hash.Split(':');
                if (hashparts[0] == "nsp")
                    if (hashparts[1] == "1")
                    {
                        _hashIters = int.Parse(hashparts[2]);
                        _b64salt = hashparts[3];
                        _b64hash = hashparts[4];
                    }
            }
            catch
            {
                throw new Exception("Invalid NSPHash version 1");
            }
        }

        public bool Equals(PlaintextPassword pt)
        {
            if ((object)pt == null) return false;
            //faz o hash do plaintext
            string ptHash = Pbkdf2Hash(pt.Plaintext, _b64salt, _hashIters);
            if (ptHash == _b64hash) return true;

            return false;
        }

    }

    public class PlaintextPassword
    {
        /// <summary>
        /// Cria novo hash
        /// </summary>
        public string Hash
        {
            get
            {
                string salt = NSPHash.GenerateSalt();
                string hash = NSPHash.Pbkdf2Hash(_plaintext, salt, NSPHash.Iters);

                return "nsp:" + NSPHash.Version + ":" + NSPHash.Iters + ":" + salt + ":" + hash;
            }
        }

        private string _plaintext;
        public string Plaintext
        {
            get { return _plaintext; }
        }

        /// <summary>
        /// Cria uma senha NSP 1
        /// </summary>
        /// <param name="password">Senha a ser verificada</param>
        /// <returns></returns>
        public PlaintextPassword(string password)
        {
            _plaintext = password;
        }



    }
}