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
    public partial class DlxLibMatrixTests
    {
        #region basic object creation and adding to rows/columns
        [Test]
        public void RootProperties()
        {
            var sutRoot = RootObject.Create();
            Assert.That(sutRoot.Root, Is.EqualTo(sutRoot));
            Assert.That(sutRoot.Kind, Is.EqualTo("Root"));
            Assert.That(sutRoot.ColumnHeader, Is.EqualTo(sutRoot));
            Assert.Throws<NotImplementedException>(() => { var _ = sutRoot.ColumnCover; });
            Assert.That(sutRoot.RowIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.RowHeader, Is.EqualTo(sutRoot));
            Assert.That(sutRoot.ColumnHeader, Is.EqualTo(sutRoot));
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.Elements, Is.Empty);
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x0]"));
        }

        [Test]
        public void RowProperties()
        {
            var root = RootObject.Create();
            var sutRow = root.NewRow();

            Assert.That(sutRow.Root, Is.EqualTo(root));
            Assert.That(sutRow.RowHeader, Is.EqualTo(sutRow));
            Assert.That(sutRow.ColumnHeader, Is.EqualTo(root));
            Assert.That(sutRow.Kind, Is.EqualTo("Row"));
            Assert.That(sutRow.RowIndex, Is.EqualTo(0));
            Assert.That(sutRow.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRow.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRow.ToString(), Is.EqualTo("Row[0]"));

            Assert.That(root.Elements, Is.EquivalentTo(new DataObject[] { sutRow }));
        }

        [Test]
        public void PrimaryColumnProperties()
        {
            var root = RootObject.Create();
            var sutColumn = root.NewColumn(ColumnCover.Primary); // TODO: Test Secondary
            (root as IRow).Append(sutColumn);

            Assert.That(sutColumn.Root, Is.EqualTo(root));
            Assert.That(sutColumn.Kind, Is.EqualTo("Column"));
            Assert.That(sutColumn.RowHeader, Is.EqualTo(root));
            Assert.That(sutColumn.ColumnHeader, Is.EqualTo(sutColumn));
            Assert.That(sutColumn.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn.ColumnIndex, Is.EqualTo(0));
            Assert.That(sutColumn.ColumnCover, Is.EqualTo(ColumnCover.Primary));
            Assert.That(sutColumn.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutColumn.ToString(), Is.EqualTo("Column[0,Primary]"));

            Assert.That(root.Elements, Is.EquivalentTo(new DataObject[] { sutColumn }));
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

            var sutElement = sutRoot.NewElement(sutRow3, sutColumn5);
            Assert.That(sutElement, Is.Not.Null);

            Assert.That(sutElement.Root, Is.EqualTo(sutRoot));
            Assert.That(sutElement.ColumnHeader, Is.EqualTo(sutColumn5));
            Assert.That(sutElement.RowIndex, Is.EqualTo(3));
            Assert.That(sutElement.ColumnIndex, Is.EqualTo(5));
            Assert.That(sutElement.Kind, Is.EqualTo("Element"));
            Assert.That(sutElement.ToString(), Is.EqualTo("Element[3,5]"));

            sutRow3.Append(sutElement);
            sutColumn5.Append(sutElement);
            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3).Concat(new DataObject[] { sutElement })));
        }

        [Test]
        public void BareColumnAndRowCreation()
        {
            var sutRoot = RootObject.Create();
            var sutColumn = sutRoot.NewColumn(ColumnCover.Primary);
            var sutRow = sutRoot.NewRow();

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

            Assert.That(sutRoot.ToCoordinates(), Is.Empty);
            var array = sutRoot.ToArray();
            DlxLibEnumerable2DArrayTests.IsEqual2D(array, new bool[3, 3]);
        }

        [Test]
        public void CreateDiagonal3x3MatrixByIndex()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            var sutRoot = sut.Item1;
            var sutElt00 = sutRoot.NewElement(0, 0);
            var sutElt11 = sutRoot.NewElement(1, 1);
            var sutElt22 = sutRoot.NewElement(2, 2);

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3).Concat(new DataObject[] { sutElt00, sutElt11, sutElt22 })));
            Assert.That(sutElt00.RowHeader, Is.EqualTo(sutRoot.GetRow(0)));
            Assert.That(sutElt00.ColumnHeader, Is.EqualTo(sutRoot.GetColumn(0)));
            Assert.That(sutElt11.RowHeader, Is.EqualTo(sutRoot.GetRow(1)));
            Assert.That(sutElt11.ColumnHeader, Is.EqualTo(sutRoot.GetColumn(1)));
            Assert.That(sutElt22.RowHeader, Is.EqualTo((sutRoot as IRoot).GetRow(2)));
            Assert.That(sutElt22.ColumnHeader, Is.EqualTo((sutRoot as IRoot).GetColumn(2)));

            Assert.That(sutRoot.ToCoordinates(), Is.EqualTo(new RootObject.ElementCoordinate[] {
                new RootObject.ElementCoordinate(0,0),
                new RootObject.ElementCoordinate(1,1),
                new RootObject.ElementCoordinate(2,2),
            }));
            var array = sutRoot.ToArray();
            DlxLibEnumerable2DArrayTests.IsEqual2D(array, new bool[3, 3] { { true, false, false }, { false, true, false }, { false, false, true } });
        }

        [Test]
        public void CreateAntiDiagonal3x3MatixByArray()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            var sutRoot = sut.Item1;
            var antiDiagonal3x3 = new bool[3, 3]{
                { false, false, true },
                { false, true, false },
                { true, false, false }
            };
            sutRoot.AddMatrix(antiDiagonal3x3);

            // Now, round-trip it
            var array = sutRoot.ToArray();
            DlxLibEnumerable2DArrayTests.IsEqual2D(array, antiDiagonal3x3);
        }

        [Test]
        public void RootAddMatrixFailsIfRootHasAnyElements()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            var sutRoot = sut.Item1;
            var sutElt11 = sutRoot.NewElement(1, 1);

            var antiDiagonal3x3 = new bool[3, 3]{
                { false, false, true },
                { false, true, false },
                { true, false, false }
            };

            Assert.Throws<InvalidOperationException>(() => sutRoot.AddMatrix(antiDiagonal3x3));
        }

        [Test]
        public void RootAddMatrixFailsIfMatrixHasTooManyRows()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            var sutRoot = sut.Item1;

            var antiDiagonal3x3 = new bool[4, 3]{
                { false, false, true },
                { false, true, false },
                { true, false, false },
                { true, false, true }
            };

            Assert.Throws<ArgumentException>(() => sutRoot.AddMatrix(antiDiagonal3x3));
        }

        [Test]
        public void RootAddMatrixFailsIfMatrixHasTooManyColumns()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            var sutRoot = sut.Item1;

            var antiDiagonal3x3 = new bool[3, 4]{
                { false, false, true, false },
                { false, true, false, true },
                { true, false, false, true },
            };

            Assert.Throws<ArgumentException>(() => sutRoot.AddMatrix(antiDiagonal3x3));
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

            var elt1 = sutRoot.NewElement(sutRow0, sutColumn5);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1);

            var elt2 = sutRoot.NewElement(sutRow2, sutColumn5);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2);

            var elt3 = sutRoot.NewElement(sutRow4, sutColumn5);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2, elt3);
        }

        [Test]
        public void OutOfOrderColumnAppend()
        {
            const int colIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow0 = sutRoot.GetRow(0);
            var sutRow2 = sutRoot.GetRow(2);
            var sutColumn5 = sutRoot.GetColumn(colIndex);

            Assert.That(sutColumn5, Is.Not.Null);
            Assert.That(sutColumn5.ColumnIndex, Is.EqualTo(colIndex));

            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary);

            var elt2 = sutRoot.NewElement(sutRow2, sutColumn5);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt2);

            Assert.Throws<ArgumentException>(() => sutRoot.NewElement(sutRow0, sutColumn5));
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

            var elt1 = sutRoot.NewElement(sutRow5, sutColumn0);
            ValidateRow(sutRow5, rowIndex, elt1);
            var elt2 = sutRoot.NewElement(sutRow5, sutColumn2);
            ValidateRow(sutRow5, rowIndex, elt1, elt2);
            var elt3 = sutRoot.NewElement(sutRow5, sutColumn4);
            ValidateRow(sutRow5, rowIndex, elt1, elt2, elt3);
        }

        [Test]
        public void OutOfOrderRowAppend()
        {
            const int rowIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow5 = sutRoot.GetRow(rowIndex);
            var sutColumn0 = sutRoot.GetColumn(0);
            var sutColumn2 = sutRoot.GetColumn(2);

            Assert.That(sutRow5, Is.Not.Null);
            Assert.That(sutRow5.RowIndex, Is.EqualTo(rowIndex));

            ValidateRow(sutRow5, rowIndex);

            var elt2 = sutRoot.NewElement(sutRow5, sutColumn2);
            ValidateRow(sutRow5, rowIndex, elt2);
            Assert.Throws<ArgumentException>(() => sutRoot.NewElement(sutRow5, sutColumn0));
        }

        [Test]
        public void OnlyAppendTheCorrectThings()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow4 = sutRoot.GetRow(4);
            var sutColumn4 = sutRoot.GetColumn(4);

            Assert.Throws<ArgumentException>(() => { (sutRoot as IRow).Append(sutRow4); } );
            Assert.Throws<ArgumentException>(() => { (sutRoot as IColumn).Append(sutColumn4); } );
        }

        [Test]
        public void SecondaryColumns()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutColumn7 = sutRoot.NewColumn(ColumnCover.Secondary);
            var sutColumn8 = sutRoot.NewColumn(ColumnCover.Secondary);

            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(7));
            Assert.That(sutRoot.NumberOfOriginalPrimaryColumns, Is.EqualTo(7));
            Assert.That(sutRoot.NumberOfOriginalSecondaryColumns, Is.EqualTo(2));

            Assert.That(sutColumn7.RowHeader, Is.EqualTo(sutRoot));
            Assert.That(sutColumn7.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn7.ColumnIndex, Is.EqualTo(7));

            Assert.That(sutColumn8.RowHeader, Is.EqualTo(sutRoot));
            Assert.That(sutColumn8.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn8.ColumnIndex, Is.EqualTo(8));

            // Note that secondary Columns are _not_ in Elements.
            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3)));
        }


        [Test]
        public void EmptyMatrixWithSecondaryColumns()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7, 2);
            var sutRoot = sut.Item1;
            var sutColumn7 = sutRoot.GetColumn(7);
            var sutColumn8 = sutRoot.GetColumn(8);

            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(7));
            Assert.That(sutRoot.NumberOfOriginalPrimaryColumns, Is.EqualTo(7));
            Assert.That(sutRoot.NumberOfOriginalSecondaryColumns, Is.EqualTo(2));

            Assert.That(sutColumn7.RowHeader, Is.EqualTo(sutRoot));
            Assert.That(sutColumn7.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn7.ColumnIndex, Is.EqualTo(7));

            Assert.That(sutColumn8.RowHeader, Is.EqualTo(sutRoot));
            Assert.That(sutColumn8.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn8.ColumnIndex, Is.EqualTo(8));

            // Note that secondary Columns are _not_ in Elements - but they are in Item3.
            var elements = sut.Item2.Cast<DataObject>().Concat(sut.Item3);
            elements = elements.Where(dto => dto.ColumnIndex != 7).Where(dto => dto.ColumnIndex != 8);

            Assert.That(sutRoot.Elements, Is.EquivalentTo(elements));
        }

        [Test]
        public void InvalidRowAndColumnIndex()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;

            Assert.Throws<IndexOutOfRangeException>(() => sutRoot.GetRow(-1));
            Assert.Throws<IndexOutOfRangeException>(() => sutRoot.GetRow(7));
            Assert.Throws<IndexOutOfRangeException>(() => sutRoot.GetColumn(-1));
            Assert.Throws<IndexOutOfRangeException>(() => sutRoot.GetColumn(7));
        }

        [Test]
        public void HighestRowAndColumn()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;

            Assert.That(sutRoot.HighestRow, Is.EqualTo(6));
            Assert.That(sutRoot.HighestColumn, Is.EqualTo(6));

            var sutColumn7 = sutRoot.NewColumn(ColumnCover.Secondary);
            Assert.That(sutRoot.HighestColumn, Is.EqualTo(6));

            var sutColumn8 = sutRoot.NewColumn(ColumnCover.Secondary);
            Assert.That(sutRoot.HighestColumn, Is.EqualTo(6));
        }

        class DHeaderObject : HeaderObject
        {
            public DHeaderObject()
                : base()
            {

            }

            public override IEnumerable<DataObject> Elements
            {
                get { throw new NotImplementedException(); }
            }

            public override IRow RowHeader
            {
                get { throw new NotImplementedException(); }
            }

            public override IColumn ColumnHeader
            {
                get { throw new NotImplementedException(); }
            }

            public override int RowIndex
            {
                get { throw new NotImplementedException(); }
            }

            public override int ColumnIndex
            {
                get { throw new NotImplementedException(); }
            }
        }

        [Test]
        public void Useof0ArgumentHeaderObjectConstructorIsRestricted()
        {
            Assert.Throws<ArgumentException>(() => new DHeaderObject());
        }

        [Test]
        public void ToStringForOnlyCurrentMatrixNothingCovered()
        {
            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;

            var resEmpty = sutRoot.ToString(RootObject.Display.OnlyCurrentMatrix);
            Assert.That(resEmpty, Is.EqualTo(""));

            var antiDiagonal3x3 = new bool[3, 3]{
                { false, false, true },
                { false, true, false },
                { true, false, false }
            };
            sutRoot.AddMatrix(antiDiagonal3x3);
            var resAntiDiagonal = sutRoot.ToString(RootObject.Display.OnlyCurrentMatrix);
            Assert.That(resAntiDiagonal, Is.EqualTo(@"0: 2
1: 1
2: 0
"));

            sutRoot.NewElement(1, 2);
            sutRoot.NewElement(2, 2);
            var resAntiDiagonalPlusTwo = sutRoot.ToString(RootObject.Display.OnlyCurrentMatrix);
            Assert.That(resAntiDiagonalPlusTwo, Is.EqualTo(@"0: 2
1: 1 2
2: 0 2
"));
        }
        #endregion
    }
}
