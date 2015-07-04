using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Fluent;
using Microsoft.FSharp.Collections;

namespace DlxLibPropertyTests
{
    using Property = Gen<Rose<Result>>;

    public static class FsCheckUtils
    {
        public static Property Label(Property p, string label)
        {
            return Prop.label<Property>(label).Invoke(p);
        }

        public static Property Label(bool b, string label)
        {
            return Label(Prop.ofTestable(b), label);
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
            return Gen.elements(allIdxs).MakeListOfLength(n)
                .Where(idxs => idxs.Distinct().Count() == n)
                .Select(idxs => idxs.Select(idx => l[idx]).ToList());
        }

        public static Gen<List<T>> PickValues<T>(int n, IEnumerable<T> l)
        {
            return PickValues(n, l.ToArray());
        }
    }
}
