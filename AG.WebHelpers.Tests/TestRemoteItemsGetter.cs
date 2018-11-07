using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AG.WebHelpers.Caching;

namespace AG.WebHelpers.Tests
{
    public class TestRemoteItemsGetter : SimpleRemoteItemsGetterBase<string, string>
    {
        public Func<string, string> OnGetRemoteItem;
        public Func<string[], string[]> OnGetRemoteItems;

        public override string GetRemoteItem(string key)
        {
            if (OnGetRemoteItem == null)
                return ExpectedItemForKey(key);
            else
                return OnGetRemoteItem(key);
        }

        public override string[] GetRemoteItems(string[] keys)
        {
            if (OnGetRemoteItems == null)
                return base.GetRemoteItems(keys);
            else
                return OnGetRemoteItems(keys);
        }

        public string ExpectedItemForKey(string key)
        {
            return "KEY=" + key;
        }
    }
}
