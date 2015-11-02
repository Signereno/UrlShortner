using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Signere.no.UrlShortner.Core;
using Signere.no.UrlShortner.Service;

namespace Signere.no.UrlShortner.Test
{
    [TestFixture]
    public class RandomStringGeneratorTest
    {

        [Test]
        public void CreateTableName()
        {
            StringBuilder sb=new StringBuilder();
            RandomStringGenerator randomStringGenerator=new RandomStringGenerator();
            for (int i = 0; i <= 100; i++)
            {
                sb.AppendFormat("\"{0}\"",randomStringGenerator.GetRandomStringAlfa(3));
                if (i != 100)
                    sb.Append(",");
            }

            Console.WriteLine(sb);
        }

        [Test]
        public void TestDate()
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(DateTime.Now));
        }

        [Test]
        public void TestSimpleJsonParse()
        {
            var response =
                Signere.no.UrlShortner.Client.SimpleJSON.JSON.Parse(
                    "{\"ShortUrl\":\"http://urlshortner.azurewebsites.net/olpxDWHszd\",\"AccessToken\":\"O2HHKkySwX19\",\"Id\":\"olpxDWHszd\"}")
                   ;
            
            ;



            Console.WriteLine(response.ToString());
            var tmp=new Signere.no.UrlShortner.Client.SimpleJSON.JSONClass();
            tmp.Add("Url", "http://wiki.unity3d.com/index.php/SimpleJSON");
            tmp.Add("Expires",DateTime.UtcNow.AddDays(1).ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            tmp.Add("BlockiFrame",true.ToString().ToLowerInvariant());

            Console.WriteLine(tmp.ToJSON(0));
        }
    }
}
