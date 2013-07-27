using System;
using System.Linq;

namespace MyLoadTest.CommandLine
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}