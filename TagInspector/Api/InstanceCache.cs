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

        /// <summary>
        ///  Static singleton in memory cache reference
        /// </summary>
        private static MemoryCache Cache
        {
            get { return LazyMemCache.Value; }
        }

        /// <summary>
        ///  Create a refernce to the Instance cache
        /// </summary>
        /// <param name="cacheDuration">Default duration of cached object</param>
        public InstanceCache(TimeSpan? cacheDuration = null)
        {
            this.CacheDuration = cacheDuration;
        }

        /// <summary>
        ///  Modify the default Cache Duration
        /// </summary>
        public TimeSpan? CacheDuration { get; set; }

        /// <summary>
        ///  Try and get cached value
        /// </summary>
        /// <typeparam name="T">Type to deserialize into</typeparam>
        /// <param name="key">key for lookup</param>
        /// <returns>Value in cache or null if not found</returns>
        public T Get<T>(string key) where T : class
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentException("key");
            return ((Cache[key] as string).IfNotNull(_ => JsonConvert.DeserializeObject<T>(_)));
        }

        /// <summary>
        ///  Cache and item
        /// </summary>
        /// <typeparam name="T">Objec Type</typeparam>
        /// <param name="key">Key for querying cache</param>
        /// <param name="value">Value to serialize and store as JSON</param>
        /// <param name="cacheDuration">cache Duration</param>
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

        /// <summary>
        ///  Remove Item from Cache
        /// </summary>
        /// <param name="key">string Key</param>
        public void Unset(string key)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentException("key");
            if (Cache.Contains(key))
            {
                Cache.Remove(key);
            }
        }

        /// <summary>
        ///  Remove all items from cache
        /// </summary>
        public void Clear()
        {
            foreach (var kvPair in Cache) Unset(kvPair.Key);
        }
    }
}