using NUnit.Framework;
using GoodCache;
using System.Collections.Generic;
using System.Threading;
using System;

namespace Tests
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
    public class Tests
    {
        Cache<ICacheable> Cache { get; set; }

        private IList<ICacheable> CachedObjects(int count)
        {
            var list = new List<ICacheable>();
            for (int i = 0; i < count; i++)
            {
                var co = new CachedObject();
                list.Add(co);
            }
            return list;
        }

        [SetUp]
        public void Setup()
        {
            Cache = new Cache<ICacheable>();
        }

        [Test]
        public void TestGet()
        {
            var cacheKeeper = new CacheKeeper(Cache);
                        
            var list = CachedObjects(10000);

            foreach(var item in list)
            {
                Cache.AddOrUpdate(item);
            }            

            for (int i = 0; i< 10;i++)
            {
                var item = list[i];
                var co = (CachedObject)Cache.Get(item.GetId());
                Assert.That(item, Is.EqualTo(co));
            }
        }

        [Test]
        public void TestAddOrUpdate()
        {
            var co = new CachedObject();
            Cache.AddOrUpdate(co);
            Assert.That(Cache.Count, Is.Not.Zero);
            Assert.That(Cache.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestCreateCache()
        {            
            Assert.That(Cache.Count, Is.Zero);
        }

        [Test]
        public void TestRemove()
        {            
            var list = CachedObjects(10000);
            foreach (var item in list)
            {
                Cache.AddOrUpdate(item);
            }
            
            for (int i = 0; i < list.Count; i++)
            {
                Cache.Remove(list[i]);
            }
        }

        [Test]
        public void TestSweepingFull()
        {            
            var cacheKeeper = new CacheKeeper(Cache);            
            Cache.RemovalStrategy = new RemovalStrategy(TimeSpan.FromSeconds(11));

            var list = CachedObjects(10000);

            foreach (var item in list)
            {
                Cache.AddOrUpdate(item);
            }

            Thread.Sleep(10000);

            Assert.That(Cache.Count, Is.EqualTo(10000));
        }

        [Test]
        public void TestSweepingEmpty()
        {
            var cacheKeeper = new CacheKeeper(Cache);            
            Cache.RemovalStrategy = new RemovalStrategy(TimeSpan.FromSeconds(1));

            var list = CachedObjects(10000);

            foreach (var item in list)
            {
                Cache.AddOrUpdate(item);
            }

            Thread.Sleep(10000);

            Assert.That(Cache.Count, Is.EqualTo(0));
        }
    }
}