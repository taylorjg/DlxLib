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
    public partial class DlxLibMatrixTests
    {
        internal void ValidateColumn(ColumnObject sut, int columnIndex, ColumnCover cover, params DataObject[] columnObjects)
        {
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.ColumnIndex, Is.EqualTo(columnIndex), "Have {0} testing ColumnIndex", sut);
            Assert.That(sut.RowIndex, Is.EqualTo(-1), "Have {0} testing RowIndex", sut);
            Assert.That(sut.ColumnCover, Is.EqualTo(cover), "Have {0} testing ColumnCover", sut);

            if (0 == columnObjects.Length)
            {
                Assert.That(sut.Down, Is.EqualTo(sut), "Have {0} testing Down on empty column", sut);
                Assert.That(sut.Up, Is.EqualTo(sut), "Have {0} testing Up on empty column", sut);
            }
            else
            {
                Assert.That(sut.Down, Is.EqualTo(columnObjects[0]), "Have {0} testing Down to first column object {1}", sut, columnObjects[0]);
                Assert.That(columnObjects[0].Up, Is.EqualTo(sut), "Have {0} testing Up from first column object {1}", sut, columnObjects[0]);
                Assert.That(columnObjects[0].ColumnIndex, Is.EqualTo(columnIndex), "Have {0} testing column index", columnObjects[0]);

                for (int i = 1; i < columnObjects.Length - 1; i++)
                {
                    Assert.That(columnObjects[i].Up, Is.EqualTo(columnObjects[i - 1]), "Have {0} testing Up", columnObjects[i]);
                    Assert.That(columnObjects[i].Down, Is.EqualTo(columnObjects[i + 1]), "Have {0} testing Down", columnObjects[i]);
                    Assert.That(columnObjects[i].ColumnIndex, Is.EqualTo(columnIndex), "Have {0} testing column index", columnObjects[i]);
                    Assert.That(columnObjects[i].Up.RowIndex, Is.LessThan(columnObjects[i].RowIndex), "Have {0} testing monotonically increasing row index (a)", columnObjects[i]);
                    Assert.That(columnObjects[i].RowIndex, Is.LessThan(columnObjects[i].Down.RowIndex), "Have {0} testing monotonically increasing row index (b)", columnObjects[i]);
                }

                Assert.That(columnObjects[columnObjects.Length - 1].Down, Is.EqualTo(sut), "Have {0} testing down from last column object {1}", sut, columnObjects[columnObjects.Length - 1]);
                Assert.That(sut.Up, Is.EqualTo(columnObjects[columnObjects.Length - 1]), "Have {0} testing up to last column object {1}", sut, columnObjects[columnObjects.Length - 1]);
                Assert.That(columnObjects[columnObjects.Length - 1].ColumnIndex, Is.EqualTo(columnIndex), "Have {0} testing column index", columnObjects[columnObjects.Length - 1]);
            }
        }

        internal void ValidateRow(RowObject sut, int rowIndex, params DataObject[] rowObjects)
        {
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.RowIndex, Is.EqualTo(rowIndex), "Have {0} testing RowIndex", sut);
            Assert.That(sut.ColumnIndex, Is.EqualTo(-1), "Have {0} testing ColumnIndex", sut);

            if (0 == rowObjects.Length)
            {
                Assert.That(sut.Right, Is.EqualTo(sut), "Have {0} testing right on empty row", sut);
                Assert.That(sut.Left, Is.EqualTo(sut), "Have {0} testing left on empty row", sut);
            }
            else
            {
                Assert.That(sut.Right, Is.EqualTo(rowObjects[0]), "Have {0} testing Right to first row object {1}", sut, rowObjects[0]);
                Assert.That(rowObjects[0].Left, Is.EqualTo(sut), "Have {0} testing Left from first row object {1}", sut, rowObjects[0]);
                Assert.That(rowObjects[0].RowIndex, Is.EqualTo(rowIndex), "Have {0} testing row index", rowObjects[0]);

                for (int i = 1; i < rowObjects.Length - 1; i++)
                {
                    Assert.That(rowObjects[i].Left, Is.EqualTo(rowObjects[i - 1]), "Have {0} testing left", rowObjects[i]);
                    Assert.That(rowObjects[i].Right, Is.EqualTo(rowObjects[i + 1]), "Have {0} testing right", rowObjects[i]);
                    Assert.That(rowObjects[i].RowIndex, Is.EqualTo(rowIndex), "Have {0} testing row index", rowObjects[i]);
                    Assert.That(rowObjects[i].Left.ColumnIndex, Is.LessThan(rowObjects[i].ColumnIndex), "Have {0} testing monotonically increasing row index (1)", rowObjects[i]);
                    Assert.That(rowObjects[i].ColumnIndex, Is.LessThan(rowObjects[i].Right.ColumnIndex), "Have {0} testing monotonically increasing row index (2)", rowObjects[i]);
                }

                Assert.That(rowObjects[rowObjects.Length - 1].Right, Is.EqualTo(sut), "Have {0} testing right from last row object {1}", sut, rowObjects[rowObjects.Length - 1]);
                Assert.That(sut.Left, Is.EqualTo(rowObjects[rowObjects.Length - 1]), "Have {0} testing left to last row object {1}", sut, rowObjects[rowObjects.Length - 1]);
                Assert.That(rowObjects[rowObjects.Length - 1].RowIndex, Is.EqualTo(rowIndex), "Have {0} testing row index", rowObjects[rowObjects.Length - 1]);
            }
        }
    }
}
