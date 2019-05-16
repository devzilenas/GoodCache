using NUnit.Framework;
using GoodCache;
using System.Collections.Generic;

namespace Tests
{
    public class Tests
    {

        private IList<ICacheable> CachedObjects(int count)
        {
            var list = new List<ICacheable>();
            for (int i = 0; i < 10000; i++)
            {
                var co = new CachedObject();
                list.Add(co);
            }
            return list;
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestGet()
        {
            var gc = new Cache<CachedObject>();
            
            var ck = new CacheKeeper(gc,);
            var list = CachedObjects(10000);

            foreach(var item in list)
            {
                gc.AddOrUpdate(item);
            }

            for (int i = 0; i< 10;i++)
            {
                var item = list[i];
                var co =  (CachedObject)gc.Get(item.GetId()).Value;
                Assert.That(item, Is.EqualTo(co));
            }
        }

        [Test]
        public void TestAddOrUpdate()
        {
            var gc = new Cache();
            var co = new CachedObject();
            gc.AddOrUpdate(co);
            Assert.That(gc, Is.Not.Empty);
            Assert.That(gc, Has.Exactly(1).Items);
        }

        [Test]
        public void TestCreateCache()
        {
            var gc = new Cache();
            Assert.That(gc, Is.Empty);
        }

        [Test]
        public void TestRemove()
        {
            var gc = new Cache();
            var list = CachedObjects(10000);
            gc.AddOrUpdate(list);
            for (int i = 0; i < list.Count; i++)
            {
                gc.Remove(list[i]);
            }
        }
    }
}