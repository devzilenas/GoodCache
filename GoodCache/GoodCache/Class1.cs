using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodCache
{
    public class CachedObject : ICacheable
    {
        private string Id { get; }

        public CachedObject()
        {
            Id = Guid.NewGuid().ToString();
        }
        public virtual string GetId()
        {
            return Id;
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

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }
    }

    public interface ICacheable
    {
        string GetId();
    }

    class CacheEntry<T> where T : ICacheable
    {
        public T Value { get; private set; }
        public DateTime CachedOn { get; private set; }
        
        public void Reset()
        {
            CachedOn = DateTime.Now;
        }

        public void ResetTo(T cacheable)
        {
            Value = cacheable;
            Reset();
        }

        public CacheEntry(T o, DateTime when)
        {
            Value = o;
            CachedOn = when;
        }

        public string GetId() => Value.GetId();
    }
    public class Cache<T> : ICollection<T> where T : ICacheable
    {
        private Dictionary<string, CacheEntry<T>> Entries { get; }

        public bool Remove(T cacheable) => Entries.Remove(cacheable.GetId());        

        public ICacheSweeper CacheSweeper { get; private set; }

        public int Count => Entries.Count();

        public bool IsReadOnly => ((ICollection<T>)Entries).IsReadOnly;        

        public T Get(string key)
        {
            if (Entries.TryGetValue(key, out CacheEntry<T> cacheEntry))
            {
                return cacheEntry.Value;
            }
            else
            {
                return default;
            }
        }
        private CacheEntry<T> Entry(T o)
        {
            if (Entries.TryGetValue(o.GetId(), out CacheEntry<T> cacheEntry))
            {
                return cacheEntry;
            }
            else
            {
                return default;
            }
        }

        public void AddOrUpdate(T o)
        {
            CacheEntry<T> entry = Entry(o);            
            if (entry != null)
            {
                if (entry.Value.Equals(o))
                {
                    entry.Reset();
                }
                else
                {
                    entry.ResetTo(o);
                }
            }
            else
            {
                Entries.Add(o.GetId(), new CacheEntry<T>(o,DateTime.Now));
            }
        } 

        public void AddOrUpdate(ICollection<T> cacheables)
        {
            foreach (var c in cacheables)
            {
                AddOrUpdate(c);
            }
        }            

        public void Add(T item)
        {
            if (Contains(item))
            {
                throw new InvalidOperationException("Already exists");
            }
            else
            {
                AddOrUpdate(item);
            }
        }

        public void Clear() => Entries.Clear();        

        public bool Contains(T item) => Get(item.GetId()) != null;

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Entries).GetEnumerator();
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection)Entries).CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Entries).GetEnumerator();

        public Cache()
        {
            Entries = new Dictionary<string, CacheEntry<T>>();
        }
    }

    public interface ICacheSweeper
    {
        void Run();
    }

    public class CacheSweeper<T> : ICacheSweeper
    {
        public ISweepingStrategy<T> SweepingStrategy { get; private set; }
        public ICollection<T> SweepOver { get; private set; }
        public CacheSweeper(ICollection<T> sweepOver)
        {
            SweepOver = sweepOver;
        }
        public void Run()
        {
            foreach (T item in SweepOver)
            {
                if (SweepingStrategy.ShouldRemove(item))
                {
                    SweepOver.Remove(item);
                }
            }            
        }
    }

    public interface ISweepingStrategy<T>
    {
        bool ShouldRemove(T cacheable);
    }

    public class StartSweepingEventArgs : EventArgs
    {
        public Cache<ICacheable> Cache { get; set; }
    }
    public class CacheKeeper
    {             
        private IKeepingStrategy KeepingStrategy { get; set; }

        public CacheKeeper(Cache<ICacheable> cache) : this(new KeepingStrategyTimed(cache,TimeSpan.FromSeconds(3)))
        {
        }

        public CacheKeeper(IKeepingStrategy keepingStrategy)
        {         
            KeepingStrategy = keepingStrategy;
            KeepingStrategy.OnEvent += KeepingStrategy_OnEvent1;            
        }

        private void KeepingStrategy_OnEvent1(Cache<ICacheable> cache)
        {         
            cache.CacheSweeper.Run();
        }             
    }

    public delegate void OnEventHandler(Cache<ICacheable> cacheables); 

    public class KeepingStrategyTimed : IKeepingStrategy
    {
        private TimeSpan timeSpan;
        private Cache<ICacheable> Cache { get; set; }
        private System.Timers.Timer Timer { get; set; }

        public event OnEventHandler OnEvent;

        public KeepingStrategyTimed(Cache<ICacheable> cache, TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            Cache = cache;
            SetTimer();
            Timer.Start();
        }

        private void SetTimer()
        {
            Timer = new System.Timers.Timer(timeSpan.TotalMilliseconds);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Enabled = true;            
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (OnEvent != null)
            {
                OnEvent?.Invoke(Cache);
            }            
        }        
    }

    public interface IKeepingStrategy
    {
        event OnEventHandler OnEvent;
    }
}
