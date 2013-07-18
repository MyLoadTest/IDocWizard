using System;
using System.Linq;

namespace MyLoadTest.IDoc.KeyGen
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}