using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodCache
{

    public class CachedObject : AbstractCachedObject
    {
        private string Id { get; }
        public override string GetId()
        {
            return Id;
        }
        public CachedObject()
        {
            Id = Guid.NewGuid().ToString();
        }
        public override bool Equals(object obj)
        {            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                var o = (CachedObject)obj;
                return Id.Equals(o.Id);
            }
        }
    }

    public class AbstractCachedObject : ICacheable
    {
        public virtual string GetId()
        {
            throw new NotImplementedException();
        }
    }

    public interface ICacheable
    {
        string GetId();
    }

    public class CacheEntry<ICacheable>
    {
        public ICacheable Value { get; }
        public DateTime CachedOn { get; }        

        public CacheEntry(ICacheable o, DateTime when)
        {
            Value = o;
            CachedOn = when;
        }
    }
    public class Cache : ICollection
    {
        Dictionary<string, CacheEntry<ICacheable>> Entries { get; }

        public int Count => ((ICollection)Entries).Count;

        public object SyncRoot => ((ICollection)Entries).SyncRoot;

        public bool IsSynchronized => ((ICollection)Entries).IsSynchronized;

        public CacheEntry<ICacheable> Get(string key)
        {
            CacheEntry<ICacheable> cacheEntry = null;
            if (Entries.TryGetValue(key, out cacheEntry))
            {
                return cacheEntry;
            }
            else
            {
                return null;
            }
        }
        public void AddOrUpdate(ICacheable o)
        {
            CacheEntry<ICacheable> ce = null;
            if (Entries.TryGetValue(key, out ce))
            {
                
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)Entries).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((ICollection)Entries).GetEnumerator();
        }

        public Cache()
        {
            Entries = new Dictionary<string, CacheEntry<ICacheable>>();
        }
    }
}
