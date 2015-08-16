using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace TagInspector.Api
{
    public class InstanceCache
    {

        private static readonly Lazy<MemoryCache> LazyMemCache = new Lazy<MemoryCache>(() =>
        {
            var setting = new NameValueCollection
            {
                {"cacheMemoryLimitMegabytes", "500"},
                {"physicalMemoryLimitPercentage", "50"},
            };

            return new MemoryCache("InstanceCache", setting);
        });

        private static MemoryCache Cache
        {
            get { return LazyMemCache.Value; }
        }

        public InstanceCache(TimeSpan? cacheDuration = null)
        {
            this.CacheDuration = cacheDuration;
        }

        public TimeSpan? CacheDuration { get; set; }

        public T Get<T>(string key) where T : class
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentException("key");
            return ((Cache[key] as string).IfNotNull(_ => JsonConvert.DeserializeObject<T>(_)));
        }

        public void Set<T>(string key, T value, TimeSpan? cacheDuration = null) where T : class
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentException("key");
            if (value.IsNull()) throw new ArgumentNullException("value");
            var policy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = DateTimeOffset.Now
                    .AddSeconds(CacheDuration.HasValue ? CacheDuration.Value.TotalSeconds : 3600),
            };
            Cache.Set(key, JsonConvert.SerializeObject(value, Formatting.None), policy);
        }

        public void Unset(string key)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentException("key");
            if (Cache.Contains(key))
            {
                Cache.Remove(key);
            }
        }
    }
}