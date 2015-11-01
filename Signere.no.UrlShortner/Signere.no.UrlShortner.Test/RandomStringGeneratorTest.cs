using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
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
    }
}
