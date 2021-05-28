using ChurchWebSiteNetCore.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Util
{
    public class AppUtil
    {
        public static IEnumerable<Item> GetItemList<TKey, TValue>(Dictionary<TKey, TValue> keyValuePairs, string defaultElement = "--- All ---", bool addDefault = true)
        {
            var list = new List<Item>();
            
            if (addDefault)
                list.Add(new Item() { Id = null, Name = defaultElement });

            foreach(KeyValuePair<TKey, TValue> kvp in keyValuePairs)
            {
                list.Add(new Item { Id = int.Parse(kvp.Key.ToString()), Name = kvp.Value.ToString() });
            }

            return list;
        }
    }
}
