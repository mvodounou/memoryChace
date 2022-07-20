using System;
using System.Collections.Generic;
using System.Linq;

namespace Finbourne.memoryCache
{
    public sealed class MemoryCaheService
    {
        private static readonly Lazy<MemoryCaheService> lazyInstance = new Lazy<MemoryCaheService>(new MemoryCaheService());

        public static MemoryCaheService Instance { get => lazyInstance.Value; }

        private readonly Dictionary<string, object> _cache;

        private readonly Dictionary<string, int> _cacheDataUsageCount;

        private readonly List<string> _cacheLog;

        private int _cacheLimit = 10;

        public Action<object> ItemRemoved;
        public Action<object> ItemUpdated;

        public MemoryCaheService()
        {
            _cache = new();
            _cacheLog = new();
            _cacheDataUsageCount = new();
        }

        private void Add(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (_cache.ContainsKey(key))
                throw new InvalidOperationException(nameof(key));

            //Check least used
            if (_cache.Count == _cacheLimit)
            {
                var CacheToList = _cacheDataUsageCount.ToList();
                CacheToList.Sort((cacheData1, cacheData2) => cacheData1.Value.CompareTo(cacheData2.Value));
                ;

                Remove(CacheToList[0].Key);
            }

            _cache.Add(key, data);
            _cacheLog.Add($"{key} => added at {DateTime.UtcNow.Date} {DateTime.UtcNow.TimeOfDay}");
        }

        private object GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_cache.ContainsKey(key))
                throw new KeyNotFoundException(nameof(key));

            object value;
            _cache.TryGetValue(key, out value);

            if (value != null)
            {
                if (_cacheDataUsageCount.ContainsKey(key))
                {
                    _cacheDataUsageCount[key] = _cacheDataUsageCount.GetValueOrDefault(key) + 1;
                }
                else
                {
                    _cacheDataUsageCount.Add(key, 1);
                }
            }

            return (T)value;
        }

        private void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_cache.ContainsKey(key))
                throw new KeyNotFoundException(nameof(key));

            _cache.Remove(key);
            _cacheLog.Add($"{key} => removed at {DateTime.UtcNow.Date} {DateTime.UtcNow.TimeOfDay}");
            ItemRemoved?.Invoke(key);
        }

        private void ClearAll()
        {
            if (_cache.Count == 0)
                return;

            _cache.Clear();
            _cacheLog.Add($"cache cleared at {DateTime.UtcNow.Date} {DateTime.UtcNow.TimeOfDay}");
        }


        private IEnumerable<object> CacheChangelogs()
        {
            return _cacheLog;
        }

        private void SetCacheLimit(int itemCountLimit)
        {
            _cacheLimit = itemCountLimit;
            _cache.EnsureCapacity(itemCountLimit);
        }

        private void Update(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!_cache.ContainsKey(key))
                throw new KeyNotFoundException(nameof(key));

            _cache[key] = data;
            _cacheLog.Add($"{key} => updated at {DateTime.UtcNow.Date} {DateTime.UtcNow.TimeOfDay}");
            ItemUpdated.Invoke(key);
        }
    }
}

