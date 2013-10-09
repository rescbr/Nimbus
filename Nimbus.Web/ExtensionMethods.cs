﻿using System;
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
    }
}