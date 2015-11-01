using System;
using System.Security.Cryptography;
using System.Text;

namespace Signere.no.UrlShortner.Service
{
    public class RandomStringGenerator:IDisposable
    {
        private  readonly RNGCryptoServiceProvider Rand;

        public RandomStringGenerator()
        {
            this.Rand=new RNGCryptoServiceProvider();
        }

        public  string GetRandomString(int length, params char[] chars)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                byte[] intBytes = new byte[4];
                Rand.GetBytes(intBytes);
                uint randomInt = BitConverter.ToUInt32(intBytes, 0);
                s.Append(chars[randomInt % chars.Length]);
            }
            return s.ToString();

        }

        public  string GetRandomString(int length)
        {
            return GetRandomString(length, "abcdefghijklmnopqrstuvwzxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!_-".ToCharArray());
        }

        public string GetRandomStringAlfa(int length)
        {
            return GetRandomString(length, "abcdefghijklmnopqrstuvwzxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
        }

        public  string GetRandomStringAlfaNumeric(int length)
        {
            return GetRandomString(length, "abcdefghijklmnopqrstuvwzxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray());
        }

     

     

        public void Dispose()
        {
            Rand.Dispose();
        }
    }
}