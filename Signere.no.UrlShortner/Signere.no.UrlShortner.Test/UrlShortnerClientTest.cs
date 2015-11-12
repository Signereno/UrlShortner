using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Signere.no.UrlShortner.Client;

namespace Signere.no.UrlShortner.Test
{
    [TestFixture]
    public class UrlShortnerClientTest
    {
        private UrlShortner.Client.UrlShortnerClient _client;
        private string testUrl1 = "https://www.signere.no/product/Signere_offline";
        private string testUrl2 = "http://www.vg.no/nyheter/innenriks/krim/mann-paagrepet-etter-doedsfall-paa-aandalsnes/a/23553019/";

        [TestFixtureSetUp]
        public void Setup()
        {
            _client = new UrlShortnerClient();
        }

        [Test]
        public async  void TestCreate_should_not_return_null()
        {
            var response = await _client.Create(testUrl1, DateTime.UtcNow.AddDays(1), false);
            Assert.IsNotNull(response);

            Console.WriteLine(JsonConvert.SerializeObject(response));
        }


        [Test]
        public async void TestCreate_and_delete()
        {
            var response = await _client.Create(testUrl1, DateTime.UtcNow.AddDays(1), false);
            Assert.IsNotNull(response);


            await _client.Delete(response.Id, response.AccessToken);

            Console.WriteLine(JsonConvert.SerializeObject(response));


        }

        [Test]
        public async void TestCreate_and_delete_with_inncorrect_access_token_should_throw_exception()
        {
            var response = await _client.Create(testUrl1, DateTime.UtcNow.AddDays(1), false);
            Assert.IsNotNull(response);


           var ex=Assert.Throws<Exception>(async()=> await _client.Delete(response.Id, response.AccessToken+"!"));
            Assert.AreEqual(ex.Message,string.Format("Not authorized to update url with id: {0}",response.Id));
            Console.WriteLine(ex);


        }

        [Test]
        public async void TestCreate_and_update()
        {
            var response = await _client.Create(testUrl1, DateTime.UtcNow.AddDays(1), false);
            Assert.IsNotNull(response);


            await _client.Update(response.Id, response.AccessToken,testUrl2);

            Console.WriteLine(JsonConvert.SerializeObject(response));


        }

        [Test]
        public async void TestCreate_and_update_with_inncorrect_access_token_should_throw_exception()
        {
            var response = await _client.Create(testUrl1, DateTime.UtcNow.AddDays(1), false);
            Assert.IsNotNull(response);


            var ex = Assert.Throws<Exception>(async () => await _client.Update(response.Id, response.AccessToken + "!",null,DateTime.UtcNow.AddMonths(1)));
            Assert.AreEqual(ex.Message, string.Format("Not authorized to update url with id: {0}", response.Id));
            Console.WriteLine(ex);


        }
    }
}
