using System;
using System.Threading;
using AG.WebHelpers.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AG.WebHelpers.Tests
{
    [TestClass]
    public class ItemsCacheTests
    {
        [TestMethod]
        public void Get_returns_up_to_date_item()
        {
            TestRemoteItemsGetter testRemoteItemsGetter = new TestRemoteItemsGetter();
            int callNumber = 0;
            testRemoteItemsGetter.OnGetRemoteItem = k =>
            {
                callNumber++;
                return k + ":" + callNumber.ToString();
            };
            ItemsCache<string, string> itemsCache = new ItemsCache<string, string>(testRemoteItemsGetter);

            var actual = itemsCache.Get("k1");
            var expected = "k1:1";
            Assert.AreEqual(expected, actual);

            actual = itemsCache.Get("k1");
            expected = "k1:2";
            Assert.AreEqual(expected, actual);

            actual = itemsCache.Get("k2");
            expected = "k2:3";
            Assert.AreEqual(expected, actual);

            actual = itemsCache.Get("k2");
            expected = "k2:4";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Get_returns_non_cached_item_1st_time()
        {
            TestRemoteItemsGetter testRemoteItemsGetter = new TestRemoteItemsGetter();
            int callNumber = 0;
            testRemoteItemsGetter.OnGetRemoteItem = k =>
            {
                callNumber++;
                return k + ":" + callNumber.ToString();
            };
            ItemsCache<string, string> itemsCache = new ItemsCache<string, string>(testRemoteItemsGetter);

            var gottenCacheItem = itemsCache.Get("k1", 1);
            var timeWhenGottenFromRemote = DateTime.UtcNow;

            Assert.AreEqual("k1:1", gottenCacheItem.Item);
            Assert.AreEqual(false, gottenCacheItem.FromCache);
            //Assert.AreEqual("k1", gottenCacheItem.Key);
            Assert.AreEqual(timeWhenGottenFromRemote.TrimMilliseconds(), gottenCacheItem.LastUpdate.TrimMilliseconds());
            Assert.AreEqual(timeWhenGottenFromRemote.TrimMilliseconds(), gottenCacheItem.LastChange.TrimMilliseconds());
        }

        [TestMethod]
        public void Get_returns_up_to_date_item_when_interval_is_zero()
        {
            TestRemoteItemsGetter testRemoteItemsGetter = new TestRemoteItemsGetter();
            int callNumber = 0;
            testRemoteItemsGetter.OnGetRemoteItem = k =>
            {
                callNumber++;
                return k + ":" + callNumber.ToString();
            };
            ItemsCache<string, string> itemsCache = new ItemsCache<string, string>(testRemoteItemsGetter);

            var gottenCacheItem = itemsCache.Get("k1", 1);
            var timeWhenGottenFromRemote = gottenCacheItem.LastUpdate;

            Thread.Sleep(10);
            gottenCacheItem = itemsCache.Get("k1", 0);


            Assert.AreEqual("k1:2", gottenCacheItem.Item);
            Assert.AreEqual(false, gottenCacheItem.FromCache);
            //Assert.AreEqual("k1", gottenCacheItem.Key);
            Assert.AreNotEqual(timeWhenGottenFromRemote, gottenCacheItem.LastUpdate);
            Assert.AreNotEqual(gottenCacheItem.LastChange, gottenCacheItem.LastUpdate);
        }

        [TestMethod]
        public void Get_returns_cached_item()
        {
            TestRemoteItemsGetter testRemoteItemsGetter = new TestRemoteItemsGetter();
            int callNumber = 0;
            testRemoteItemsGetter.OnGetRemoteItem = k =>
            {
                callNumber++;
                return k + ":" + callNumber.ToString();
            };
            ItemsCache<string, string> itemsCache = new ItemsCache<string, string>(testRemoteItemsGetter);

            var gottenCacheItem = itemsCache.Get("k1", 1);
            var timeWhenGottenFromRemote = gottenCacheItem.LastUpdate;

            Thread.Sleep(1);
            gottenCacheItem = itemsCache.Get("k1", 5);


            Assert.AreEqual("k1:1", gottenCacheItem.Item);
            Assert.AreEqual(true, gottenCacheItem.FromCache);
            //Assert.AreEqual("k1", gottenCacheItem.Key);
            Assert.AreEqual(timeWhenGottenFromRemote, gottenCacheItem.LastUpdate);
        }
    }
}
