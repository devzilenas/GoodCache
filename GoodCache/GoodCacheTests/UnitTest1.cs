using NUnit.Framework;
using GoodCache;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

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
        public void CreateCache()
        {
            var gc = new Cache();
            Assert.That(gc, Is.Empty);
        }

    }
}