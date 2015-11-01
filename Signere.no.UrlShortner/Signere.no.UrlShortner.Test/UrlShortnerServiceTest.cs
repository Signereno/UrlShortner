using System;
using System.Diagnostics;
using NUnit.Framework;
using Signere.no.UrlShortner.Core;
using Signere.no.UrlShortner.Service;

namespace Signere.no.UrlShortner.Test
{
    [TestFixture]
    public class UrlShortnerServiceTest
    {
        private IUrlShortnerService service;
        private string testUrl1 = "https://www.signere.no/product/Signere_offline";
        private string testUrl2= "http://www.vg.no/nyheter/innenriks/krim/mann-paagrepet-etter-doedsfall-paa-aandalsnes/a/23553019/";

        [TestFixtureSetUp]
        public void Setup()
        {
            service=new UrlShortnerService("signereunittest", "mF3/PAvi3dXLB84iezk4KL047DrlkSF7RN7B8lg4pakzgGAGAfalPmDym8s3hOXMtS5Sw5cID7Kssz3TJX4O+A==","https://s.signere.no",false);
        }

        [Test]
        public async void CreateShortUrl_with_expires_should_not_return_null()
        {
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing
            stopwatch.Start();
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(1));

            // Stop timing
            stopwatch.Stop();

            // Write result
            Console.WriteLine("Time elapsed: {0}",
                stopwatch.Elapsed);

            Assert.IsNotNullOrEmpty(result.AccessToken);
            Assert.IsNotNullOrEmpty(result.ShortUrl);
            
            Console.WriteLine(result.ShortUrl);
        }

        [Test]
        public async void CreateShortUrl_accesstoken_should_be_12_chars()
        {
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(1));
            Assert.IsNotNullOrEmpty(result.AccessToken);
            Assert.AreEqual(12,result.AccessToken.Length);
            
        }

        [Test]
        public async void GetShortUrl_should_not_be_null()
        {
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(1));
            var response=await service.Get(result.Id);
            
            Assert.IsNotNullOrEmpty(response);

        }

        [Test]
        public async void Create_and_update_entity_should_alter_expires  ()
        {
            var result = await service.Create(testUrl1,new DateTime(2015,01,01));
            var response = await service.GetEntity(result.Id);
            Assert.IsTrue(response.Expires<DateTime.UtcNow);
            Assert.IsTrue(response.Expired());
            await service.Update(result.Id, result.AccessToken, DateTime.UtcNow.AddDays(1));

            response = await service.GetEntity(result.Id);

            Assert.IsFalse(response.Expires < DateTime.UtcNow);
            

        }

        [Test]
        public async void Create_and_update_entity_should_should_throw_exception_with_invalid_accesstoken()
        {
            var result = await service.Create(testUrl1, new DateTime(2015, 01, 01));
            
           
            var ex=Assert.Throws<UnAuthorizedException>(async()=>   await service.Update(result.Id, new RandomStringGenerator().GetRandomStringAlfa(12), DateTime.UtcNow.AddDays(1)));
            var response = await service.GetEntity(result.Id);

            Assert.AreEqual(response.Expires.Value, new DateTime(2015, 01, 01));

            Console.WriteLine(ex);


        }

        [Test]
        public async void Create_and_delete_entity_should_should_throw_exception_with_invalid_accesstoken()
        {
            var result = await service.Create(testUrl1, new DateTime(2015, 01, 01));


            var ex = Assert.Throws<UnAuthorizedException>(async () => await service.Delete(result.Id, new RandomStringGenerator().GetRandomStringAlfa(12)));
            var response = await service.GetEntity(result.Id);

            Assert.IsNotNull(response);

            Console.WriteLine(ex);


        }

        [Test]
        public async void Create_and_delete_should_throw_exception_on_get()
        {
            var result = await service.Create(testUrl1, new DateTime(2015, 01, 01));
            await service.Delete(result.Id, result.AccessToken);

            var ex = Assert.Throws<NotFoundException>(async () => await service.Get(result.Id));
            Console.WriteLine(ex);

        }

        [Test]
        public async void GetShortUrl_entity_should_not_be_null()
        {
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(1));
            var response = await service.GetEntity(result.Id);

            Assert.NotNull(response);
            Assert.IsNotNullOrEmpty(response.AccessToken);
            Assert.IsNotNullOrEmpty(response.Url);
            Assert.IsNotNullOrEmpty(response.Id);
        }

        [Test]
        public async void CreateShortUrl_with_expires_should_throw_expires()
        {
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(-1));
            var ex = Assert.Throws<ExpiredException>(async () => await service.Get(result.Id));

            Console.WriteLine(ex.Message);
        }


        [Test]
        public async void CreateShortUrl_with_http_should_retur_shorturl_with_http()
        {
            var result = await service.Create(testUrl2, DateTime.UtcNow.AddHours(1));
            
            Assert.IsNotNullOrEmpty(result.ShortUrl);
            Assert.IsTrue(result.ShortUrl.Contains("http://"));

            Console.WriteLine(result.ShortUrl);
        }

        [Test]
        public async void CreateShortUrl_with_https_should_retur_shorturl_with_https()
        {
            var result = await service.Create(testUrl1, DateTime.UtcNow.AddHours(1));

            Assert.IsNotNullOrEmpty(result.ShortUrl);
            Assert.IsTrue(result.ShortUrl.Contains("https://"));

            Console.WriteLine(result.ShortUrl);
        }



        [TestFixtureTearDown]
        public void CleanUp()
        {
            //((UrlShortnerService) service).CleanUpTable();
        }


    }
}
