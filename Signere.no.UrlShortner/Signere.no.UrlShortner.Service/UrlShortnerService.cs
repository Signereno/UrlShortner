﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Signere.no.UrlShortner.Core;

namespace Signere.no.UrlShortner.Service
{
    public class UrlShortnerService : IUrlShortnerService,IDisposable
    {
        private readonly string baseUrl;
        protected readonly CloudTableClient tableClient;
        
        protected CloudStorageAccount storageAccount;
        protected TableRequestOptions tableRequestOptions = new TableRequestOptions()
        {
            PayloadFormat = TablePayloadFormat.JsonNoMetadata,
            RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5), 10)
        };

        protected MemoryCache Cache;
        private RandomStringGenerator randomStringGenerator;
        private string[] tables = new[] { "sRq", "fWQ", "xAl", "sFR", "oBT", "ael", "JCp", "VOQ", "BAW", "czr", "eHS", "SGJ", "FJX", "nhx", "XQt", "EqJ", "Bdj", "Avz", "TGq", "qcO", "tqa", "hiN", "jsf", "fvz", "Xfo", "ydJ", "wal", "cWK", "EQF", "rAZ", "QmP", "vXk", "iRV", "czA", "UEY", "tlX", "rAB", "NEY", "oWe", "Sym", "anW", "mNY", "YUk", "ACx", "yOO", "ZwR", "mQX", "JTF", "ALb", "ALx", "NOH", "LeM", "vcg", "ntx", "hVE", "NAX", "BUz", "VsE", "Epv", "Ghs", "RSj", "CzL", "Kzj", "pAQ", "EhX", "lzF", "Qza", "VNw", "MFZ", "Foa", "ozo", "QTu", "Lam", "TNB", "sSh", "ooV", "WEZ", "WrG", "wca", "olp", "ZYU", "zDX", "pLi", "TZN", "fHT", "VfT", "Nid", "JKw", "fyz", "Fcc", "zTq", "Uzm", "nGB", "zYv", "VNc", "BJb", "mdj", "zEY", "fpz", "oNX", "KJF" };
        private Random rnd = new Random();
        private IDictionary<string,CloudTable> tableList=new Dictionary<string, CloudTable>();

        private static string _masterToken;

        public UrlShortnerService(string storageAccountName, string storageAccountSecret,string baseUrl, string masterToken,bool useMemoryCache=true)
        {
            
            this.storageAccount = new CloudStorageAccount(new StorageCredentials(storageAccountName, storageAccountSecret), true);
            ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(storageAccount.TableEndpoint);
            tableServicePoint.UseNagleAlgorithm = false;
            tableClient = storageAccount.CreateCloudTableClient();
            if(useMemoryCache)
                this.Cache=MemoryCache.Default;

            randomStringGenerator=new RandomStringGenerator();

            foreach (var s in tables)
            {
                var table = tableClient.GetTableReference(s);
                table.CreateIfNotExists(tableRequestOptions);
                tableList.Add(s,table);
            }

            this.baseUrl = baseUrl.ToLowerInvariant();
            if (!this.baseUrl.EndsWith("/"))
                this.baseUrl += "/";

            if (string.IsNullOrWhiteSpace(masterToken))
                _masterToken = randomStringGenerator.GetRandomString(100);
            else
                _masterToken = masterToken;
        }

        public async Task<UrlEntityResponse> Create(string url, DateTime? Expires = null, bool BlockiFrame = false, bool permanentRedirect = false, string prefix = null, string accessToken = null)
        {
            UrlEntityInternal newEntityInternal=new UrlEntityInternal()
            {
                Url = url,
                Expires = Expires,
                BlockiFrame = BlockiFrame,
                AccessToken =string.IsNullOrWhiteSpace( accessToken) ? randomStringGenerator.GetRandomStringAlfaNumeric(12):accessToken,
                PartitionKey = randomStringGenerator.GetRandomStringAlfa(3),
                RowKey = randomStringGenerator.GetRandomStringAlfaNumeric(rnd.Next(3,6)),
                PermanentRedirect = permanentRedirect,
                Prefix = prefix,
                
            };
            TableOperation op = TableOperation.Insert(newEntityInternal);

            var randomTable = Table;

            string id = string.Format("{0}{1}",  randomTable.Name, newEntityInternal.Id);
            string shortUrl =string.IsNullOrWhiteSpace(prefix) ? string.Format("{0}{1}", baseUrl,id): string.Format("{0}{1}/{2}", baseUrl,prefix, id);

            if (url.Contains("http://"))
                shortUrl = shortUrl.Replace("https://", "http://");

            try
            {
                await randomTable.ExecuteAsync(op);

                if (Cache != null)
                {
                    DateTimeOffset cacheExpires = DateTimeOffset.UtcNow.AddDays(1);
                    if (Expires < DateTime.UtcNow.AddDays(1))
                    {
                        cacheExpires = Expires.Value;
                    }
                    Cache.Add(id, newEntityInternal, cacheExpires);
                }

                return new UrlEntityResponse()
                {
                    AccessToken = newEntityInternal.AccessToken,
                    ShortUrl = shortUrl,
                    Id = id
                };
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException e)
            {
                if (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Contains(Microsoft.WindowsAzure.Storage.Table.Protocol.TableErrorCodeStrings.EntityAlreadyExists) 
                    || e.RequestInformation.ExtendedErrorInformation.ErrorMessage.ToLowerInvariant().Contains("already exists"))
                {
                    return await Create(url,Expires,BlockiFrame);
                }
                else
                {
                    throw e;
                }
            }
            catch (Exception e)
            {                
                throw e;
            }
 

        }

        public Task<UrlEntityResponse> Create(UrlEntityRequest request)
        {
            return Create(request.Url, request.Expires, request.BlockiFrame,request.PermanentRedirect,request.Prefix, request.AccessToken);
        }

        public  Task Update(string id,string AccessToken, DateTime? Expires = null, bool BlockiFrame = false, string prefix = null)
        {
            return Update(id, AccessToken, null, Expires, BlockiFrame,prefix);
        }

        public async Task Update(string id, string AccessToken, string url, DateTime? Expires = null, bool BlockiFrame = false, string prefix = null)
        {
            var tableRef = GetTableRefFromId(id);

            UrlEntityInternal entity = await GetEntity(id, AccessToken, tableRef);

            if(Expires.HasValue)
                entity.Expires = Expires;
            entity.BlockiFrame = BlockiFrame;
            if(!string.IsNullOrWhiteSpace(prefix))
                entity.Prefix = prefix;
            entity.ETag = "*";
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!string.IsNullOrWhiteSpace(entity.Url))
                    entity.UpdateLog += (!string.IsNullOrWhiteSpace(entity.UpdateLog) ? "," : "") + entity.Url;

                if (Encoding.UTF8.GetByteCount(entity.UpdateLog) > 64000)
                    entity.UpdateLog = entity.Url;

                entity.Url = url;
            }

            if (Cache != null && Cache.Contains(id))
            {
                Cache[id] = entity;                
            }

            await tableRef.ExecuteAsync(TableOperation.Merge(entity));
        }

        public Task Update(string id, string AccessToken, UrlEntityRequest request)
        {
            return Update(id, AccessToken, request.Url, request.Expires, request.BlockiFrame);
        }

        public async Task Delete(string id,string AccessToken)
        {
            var tableRef = GetTableRefFromId(id);

            var entity = await GetEntity(id, AccessToken, tableRef);
            entity.ETag = "*";

            if (Cache != null && Cache.Contains(id))
            {
               Cache.Remove(id);
            }

            await tableRef.ExecuteAsync(TableOperation.Delete(entity));
        }

        private CloudTable GetTableRefFromId(string id)
        {
            if(!tableList.ContainsKey(id.Substring(0, 3)))
                throw new NotFoundException(id);

            var tableRef = tableList[id.Substring(0, 3)];
            if (tableRef == null)
                throw new NotFoundException(id);
            return tableRef;
        }

        private static async Task<UrlEntityInternal> GetEntity(string id, string AccessToken, CloudTable tableRef)
        {

            TableResult retrievedResult = null;

            TableOperation retrieveOperation = TableOperation.Retrieve<UrlEntityInternal>( id.Substring(3, 3), id.Substring(6));
            retrievedResult = await tableRef.ExecuteAsync(retrieveOperation);
            var entity = retrievedResult.Result as UrlEntityInternal;

            if (entity == null)
                throw new NotFoundException(id);

            if(AccessToken!=null)
                if (!entity.AccessToken.Equals(AccessToken) && !AccessToken.Equals(_masterToken))
                    throw new UnAuthorizedException(id);

            return entity;
        }

        public async Task<string> Get(string id)
        {

            var entity = await GetEntity(id);

            if (entity.Expires.HasValue && entity.Expires < DateTime.UtcNow)
                throw new ExpiredException(id);

            return entity.Url;
        }

        public async Task<UrlEntity> GetEntity(string id)
        {
            UrlEntityInternal entityInternal = null;
            if (Cache != null && Cache.Contains(id))
            {
                entityInternal = Cache[id] as UrlEntityInternal;           
            }
            else
            {
                var tableRef = GetTableRefFromId(id);
                if (tableRef == null)
                    return null;

                entityInternal = await GetEntity(id, null, tableRef);
            }
            
            return new UrlEntity()
            {
                Expires = entityInternal.Expires,
                BlockiFrame = entityInternal.BlockiFrame,
                AccessToken = entityInternal.AccessToken,
                Url = entityInternal.Url,
                Id = entityInternal.Id,
                Prefix = entityInternal.Prefix,
                PermanentRedirect = entityInternal.PermanentRedirect,
                UpdateLog = entityInternal.UpdateLog
            };
        }

        public async Task<UrlEntity> GetEntityUpdateLog(string id, string AccessToken)
        {
            var tableRef = GetTableRefFromId(id);
            if (tableRef == null)
                return null;

            UrlEntityInternal entityInternal = await GetEntity(id, AccessToken, tableRef);
           
            return new UrlEntity()
            {
                Expires = entityInternal.Expires,
                BlockiFrame = entityInternal.BlockiFrame,
                AccessToken = entityInternal.AccessToken,
                Url = entityInternal.Url,
                Id = entityInternal.Id,
                Prefix = entityInternal.Prefix,
                PermanentRedirect = entityInternal.PermanentRedirect,
                UpdateLog = entityInternal.UpdateLog
            };
        }

        private CloudTable Table
        {
            get
            {
                var index = rnd.Next(0, 100);
                return tableList[tables[index]];
            }
        }

        public void Dispose()
        {
            if(randomStringGenerator!=null)
                randomStringGenerator.Dispose();
            
            
        }

        public void CleanUpTable()
        {
            foreach (var s in tables)
            {
                var table = tableClient.GetTableReference(s);
                table.Delete();                
            }
        }
    }
}