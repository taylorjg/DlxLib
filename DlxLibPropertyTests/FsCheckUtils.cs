using System.Collections.Generic;
using System.Linq;
using FsCheck;
using Microsoft.FSharp.Collections;

namespace DlxLibPropertyTests
{
    public static class FsCheckUtils
    {
        public static Property Label(bool b, string label)
        {
            return b.Label(label);
        }

        public static Property AndAll(params Property[] properties)
        {
            return Prop.ofTestable(ListModule.OfSeq(properties));
        }

        public static Property AndAll(IEnumerable<Property> properties)
        {
            return AndAll(properties.ToArray());
        }

        public static Gen<List<T>> PickValues<T>(int n, params T[] l)
        {
            var allIdxs = Enumerable.Range(0, l.Length).ToArray();
            return Gen.Elements(allIdxs).ListOf(n)
                .Where(idxs => idxs.Distinct().Count() == n)
                .Select(idxs => idxs.Select(idx => l[idx]).ToList());
        }

        public static Gen<List<T>> PickValues<T>(int n, IEnumerable<T> l)
        {
            return PickValues(n, l.ToArray());
        }
    }
}
