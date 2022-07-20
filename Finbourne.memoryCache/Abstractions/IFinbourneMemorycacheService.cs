using System.Collections.Generic;

namespace Finbourne.memoryCache.Abstractions
{
    public interface IFinbourneMemorycacheService
    {
        void Add(string key, object data);
        void Update(string key, object data);
        void Clear(string key);
        void ClearAll();
        void GetCacheInfo();
        IEnumerable<object> CacheChangelogs();
        void SetCacheLimit(int itemCountLimit);
    }
}

