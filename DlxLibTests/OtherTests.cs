using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using DlxLib;

namespace DlxLibTests
{
    [TestFixture]
    public class OtherTests
    {
        [Test]
        public void MustNotBeNull()
        {
            string notNull = "hi there";
            string isNull = null;

            Assert.DoesNotThrow(() => notNull.MustNotBeNull("notNull"));
            Assert.Throws<ArgumentNullException>(() => isNull.MustNotBeNull("isNull"));
        }

        [Test]
        public void MyMin()
        {
            Assert.That(new int[]{ 17, 99, 23, 5, 188, 17, 6, 42 }.Min(id => id), Is.EqualTo(5));

            var col = new int[] { 17,    99,    23,    5,      188,   5,    6,     42 }.Zip(new string[]
                                { "abc", "def", "aaa", "xray", "zzz", "abc", "wxy", "mmm" }, (a, b) => Tuple.Create(a, b));

            Assert.That(col.Min(t => t.Item1, Comparer<int>.Default), Is.EqualTo(Tuple.Create(5, "xray")));
            Assert.That(col.Min(t => t.Item2, Comparer<string>.Default), Is.EqualTo(Tuple.Create(23, "aaa")));
            Assert.That(col.Min(t => t, Comparer<Tuple<int,string>>.Default), Is.EqualTo(Tuple.Create(5,"abc")));
        }

        [Test]
        public void ArrayForEach()
        {
            var digits = "0123456789";
            var sut = new int[,] {
                { 2, 1, 3, 7 },
                { 4, 0, 6, 1 },
                { 0, 3, 2, 5 }
            };
            var actual = sut.ForEach(i => new String(digits[i], i));
            var expected = "2213337777777444466666613332255555";

            Assert.That(String.Join("", actual.Cast<string>()), Is.EqualTo(expected));
        }
    }
}
