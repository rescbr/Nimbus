/**
* Copyright (C) 2013 Terry Chia

* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
* WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
/* https://github.com/Ayrx/C--Authentication/blob/master/Authentication */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Nimbus.Web.Security
{
    /**
     * The GoogleAuthenticator Class provides an API for implementing HOTP
     * and TOTP functionality based on the Google Authenticator. The algorithms
     * for HOTP and TOTP are based upon RFC 4226 and RFC 6238 respectively.
     *
     * QR code encoding is done with the QRCode.Net library that can be found
     * here: http://qrcodenet.codeplex.com/
     **/

    public class OneTimePassword
    {
        /**
         * The GenerateHotpPassword() function is an implementation of HOTP
         * based upon RFC 4226.
         **/

        private static string GenerateHotpPassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }

            byte[] key = Base32.ToBytes(secret);
            HMACSHA1 hmac = new HMACSHA1(key, true);
            byte[] hash = hmac.ComputeHash(counter);
            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, digits);
            return password.ToString(new string('0', digits));
        }

        /**
         * The GetCurrentCounter() function generates a counter based upon
         * the current time.
         **/

        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long GetCurrentCounter()
        {
            long counter = (long)(DateTime.UtcNow - UNIX_EPOCH).TotalSeconds / 30;
            return counter;
        }

        /**
         * The GenerateTotpPassword() function is an implementation of TOTP
         * based upon RFC 6238. The function extends GenerateHotpPassword().
         **/

        private static string GenerateTotpPassword(string secret, long counter)
        {
            return GenerateHotpPassword(secret, counter);
        }

        /**
         * The IsValid() function is used for verification of the one-time
         * password. The function is overloaded depending on whether HOTP
         * or TOTP is used.
         **/

        public static bool IsValid(string secret, string password, int checkAdjacent = 1)
        {
            if (password == GenerateTotpPassword(secret, GetCurrentCounter()))
            {
                return true;
            }

            for (int i = 1; i <= checkAdjacent; i++)
            {
                if (password == GenerateTotpPassword(secret, GetCurrentCounter() + i))
                {
                    return true;
                }

                if (password == GenerateTotpPassword(secret, GetCurrentCounter() - i))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValid(long iterationNumber, string secret, string password, int checkAdjacent = 1)
        {
            if (password == GenerateHotpPassword(secret, iterationNumber))
            {
                return true;
            }

            for (int i = 1; i <= checkAdjacent; i++)
            {
                if (password == GenerateHotpPassword(secret, iterationNumber + i))
                {
                    return true;
                }

                if (password == GenerateHotpPassword(secret, iterationNumber - i))
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * The GenerateSecret() function is used to generate a random
         * Base32 string for use in the HOTP and TOTP functions.
         **/

        public static string GenerateSecret()
        {
            RNGCryptoServiceProvider random_generator = new RNGCryptoServiceProvider();
            byte[] secret = new byte[6];
            random_generator.GetBytes(secret);

            return Base32.ToString(secret);
        }

        /**
         * The GenerateTotpQrCode() and GenerateHotpQrCode() functions are used
         * to generate a QR-Code in the form of a Bitmap.
         **/

        //public static Bitmap GenerateTotpQrCode(string secret, string user)
        //{
        //    string formatted_secret = string.Format("otpauth://totp/{0}?secret={1}", user, secret);
        //    QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.H);
        //    QrCode encoded_secret = encoder.Encode(formatted_secret);

        //    MemoryStream ms = new MemoryStream();

        //    Renderer renderer = new Renderer(5, Brushes.Black, Brushes.White);
        //    renderer.WriteToStream(encoded_secret.Matrix, ms, ImageFormat.Bmp);
        //    Bitmap bmp = new Bitmap(ms);
        //    return bmp;
        //}

        //public static Bitmap GenerateHotpQrCode(string secret, string user, long counter = 1)
        //{
        //    string formatted_secret = string.Format("otpauth://hotp/{0}?secret={1}&counter={2}", user, secret, counter.ToString());
        //    QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.H);
        //    QrCode encoded_secret = encoder.Encode(formatted_secret);

        //    MemoryStream ms = new MemoryStream();

        //    Renderer renderer = new Renderer(5, Brushes.Black, Brushes.White);
        //    renderer.WriteToStream(encoded_secret.Matrix, ms, ImageFormat.Bmp);
        //    Bitmap bmp = new Bitmap(ms);
        //    return bmp;
        //}
    }
}