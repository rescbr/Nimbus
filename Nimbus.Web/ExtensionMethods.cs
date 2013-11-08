using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Extensions
{
    public static class ExtensionMethods
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IList<T> ShuffleCopy<T>(this IEnumerable<T> input)
        { 
            List<T> list = new List<T>(input);
            list.Shuffle();
            return list;
        }      
    
    }


}