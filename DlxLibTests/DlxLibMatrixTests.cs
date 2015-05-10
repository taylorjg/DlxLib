﻿using System;
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
    public partial class DlxLibMatrixTests
    {
        #region basic object creation and adding to rows/columns
        [Test]
        public void RootProperties()
        {
            var sutRoot = new RootObject();
            Assert.That(sutRoot.Root, Is.EqualTo(sutRoot));
            Assert.That(sutRoot.Kind, Is.EqualTo("Root"));
            Assert.That(sutRoot.ColumnHeader, Is.EqualTo(sutRoot));
            Assert.Throws<NotImplementedException>(() => { var _ = sutRoot.ColumnCover; });
            Assert.That(sutRoot.RowIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x0]"));

            Assert.DoesNotThrow(() => sutRoot.ValidateRowIndexAvailableInColumn(sutRoot, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRoot.ValidateRowIndexAvailableInColumn(sutRoot, 0, 0));
            Assert.DoesNotThrow(() => sutRoot.ValidateColumnIndexAvailableInRow(sutRoot, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRoot.ValidateColumnIndexAvailableInRow(sutRoot, 0, 0));
        }

        [Test]
        public void RowProperties()
        {
            var root = new RootObject();
            var sutRow = new RowObject(root, 0);
            (root as IColumn).Append(sutRow);

            Assert.That(sutRow.Root, Is.EqualTo(root));
            Assert.That(sutRow.ColumnHeader, Is.EqualTo(root));
            Assert.That(sutRow.Kind, Is.EqualTo("Row"));
            Assert.That(sutRow.RowIndex, Is.EqualTo(0));
            Assert.That(sutRow.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRow.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRow.ToString(), Is.EqualTo("Row[0]"));

            Assert.DoesNotThrow(() => sutRow.ValidateRowIndexAvailableInColumn(root, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRow.ValidateRowIndexAvailableInColumn(root, 0, 0));
            Assert.DoesNotThrow(() => sutRow.ValidateColumnIndexAvailableInRow(root, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRow.ValidateColumnIndexAvailableInRow(root, 0, 0));
        }

        [Test]
        public void PrimaryColumnProperties()
        {
            var root = new RootObject();
            var sutColumn = new ColumnObject(root, 0, ColumnCover.Primary); // TODO: Test Secondary
            (root as IRow).Append(sutColumn);

            Assert.That(sutColumn.Root, Is.EqualTo(root));
            Assert.That(sutColumn.Kind, Is.EqualTo("Column"));
            Assert.That(sutColumn.ColumnHeader, Is.EqualTo(sutColumn));
            Assert.That(sutColumn.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn.ColumnIndex, Is.EqualTo(0));
            Assert.That(sutColumn.ColumnCover, Is.EqualTo(ColumnCover.Primary));
            Assert.That(sutColumn.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutColumn.ToString(), Is.EqualTo("Column[0,Primary]"));
        }

        [Test]
        public void ElementProperties()
        {
            const int rowIndex = 3;
            const int colIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow3 = sutRoot.GetRow(rowIndex);
            var sutColumn5 = sutRoot.GetColumn(colIndex);

            var sutElement = new ElementObject(sutRoot, sutRow3, sutColumn5);
            Assert.That(sutElement, Is.Not.Null);
            Assert.That(sutElement.Root, Is.EqualTo(sutRoot));
            Assert.That(sutElement.ColumnHeader, Is.EqualTo(sutColumn5));
            Assert.That(sutElement.RowIndex, Is.EqualTo(3));
            Assert.That(sutElement.ColumnIndex, Is.EqualTo(5));
            Assert.That(sutElement.Kind, Is.EqualTo("Element"));
            Assert.That(sutElement.ToString(), Is.EqualTo("Element[3,5]"));

        }

        [Test]
        public void BareColumnAndRowCreation()
        {
            var sutRoot = new RootObject();
            var sutColumn = new ColumnObject(sutRoot, 0, ColumnCover.Primary);
            var sutRow = new RowObject(sutRoot, 0);

            ValidateColumn(sutColumn, 0, ColumnCover.Primary);
            ValidateRow(sutRow, 0);
        }


        [Test]
        public void CreateEmpty0x0Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(0, 0);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x0]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(0)); // no rows
            Assert.That(sut.Item3.Length, Is.EqualTo(0)); // no columns

            Assert.That(sutRoot.Elements, Is.Empty);

            Assert.Throws<IndexOutOfRangeException>(() => { (sutRoot as IRoot).GetRow(0); });
            Assert.Throws<IndexOutOfRangeException>(() => { (sutRoot as IRoot).GetColumn(0); });
        }

        [Test]
        public void CreateEmpty0x3Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(0, 3);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(3));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x3]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(0)); // no rows
            Assert.That(sut.Item3.Length, Is.EqualTo(3)); // 3 columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, ColumnObject, object>(sut.Item3, (n, col) => { ValidateColumn(col, n, ColumnCover.Primary); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(0); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(3); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetColumn(i), Is.EqualTo(sut.Item3[i]));
            }

        }

        [Test]
        public void CreateEmpty3x0Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 0);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(3));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[3x0]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(3)); // 3 rows
            Assert.That(sut.Item3.Length, Is.EqualTo(0)); // no columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, RowObject, object>(sut.Item2, (n, row) => { ValidateRow(row, n); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(3); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(0); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetRow(i), Is.EqualTo(sut.Item2[i]));
            }
        }

        [Test]
        public void CreateEmpty3x3Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(3));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(3));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[3x3]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(3)); // 3 rows
            Assert.That(sut.Item3.Length, Is.EqualTo(3)); // 3 columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, RowObject, object>(sut.Item2, (n, row) => { ValidateRow(row, n); return null; }).Last();
            Enumerable.Range(0, 3).Zip<int, ColumnObject, object>(sut.Item3, (n, col) => { ValidateColumn(col, n, ColumnCover.Primary); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(3); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(3); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetRow(i), Is.EqualTo(sut.Item2[i]));
            }

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetColumn(i), Is.EqualTo(sut.Item3[i]));
            }
        }

        [Test]
        public void BareColumnCreation()
        {
            const int colIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow0 = sutRoot.GetRow(0);
            var sutRow2 = sutRoot.GetRow(2);
            var sutRow4 = sutRoot.GetRow(4);
            var sutColumn5 = sutRoot.GetColumn(colIndex);

            Assert.That(sutColumn5, Is.Not.Null);
            Assert.That(sutColumn5.ColumnIndex, Is.EqualTo(colIndex));

            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary);

            var elt1 = new ElementObject(sutRoot, sutRow0, sutColumn5);
            sutColumn5.Append(elt1);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1);

            var elt2 = new ElementObject(sutRoot, sutRow2, sutColumn5);
            sutColumn5.Append(elt2);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2);

            var elt3 = new ElementObject(sutRoot, sutRow4, sutColumn5);
            sutColumn5.Append(elt3);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2, elt3);
        }

        [Test]
        public void BareRowCreation()
        {
            const int rowIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow5 = sutRoot.GetRow(rowIndex);
            var sutColumn0 = sutRoot.GetColumn(0);
            var sutColumn2 = sutRoot.GetColumn(2);
            var sutColumn4 = sutRoot.GetColumn(4);

            Assert.That(sutRow5, Is.Not.Null);
            Assert.That(sutRow5.RowIndex, Is.EqualTo(rowIndex));

            ValidateRow(sutRow5, rowIndex);

            var elt1 = new ElementObject(sutRoot, sutRow5, sutColumn0);
            sutRow5.Append(elt1);
            ValidateRow(sutRow5, rowIndex, elt1);
            var elt2 = new ElementObject(sutRoot, sutRow5, sutColumn2);
            sutRow5.Append(elt2);
            ValidateRow(sutRow5, rowIndex, elt1, elt2);
            var elt3 = new ElementObject(sutRoot, sutRow5, sutColumn4);
            sutRow5.Append(elt3);
            ValidateRow(sutRow5, rowIndex, elt1, elt2, elt3);
        }

        #endregion
    }
}
