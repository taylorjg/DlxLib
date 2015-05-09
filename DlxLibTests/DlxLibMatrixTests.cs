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
        public void BareColumnCreation()
        {
            const int colIndex = 5;

            // Start with empty column
            var dummyRoot = new RootObject();
            var sutColumn = new ColumnObject(dummyRoot, colIndex, ColumnCover.Primary);
            ValidateColumn(sutColumn, colIndex, ColumnCover.Primary);

            // Add some elements
            var elt1 = new ElementObject(dummyRoot, sutColumn, 0, colIndex);  // TODO: ElementObject to get columnIndex from column header?  But can't for row index since sparse - but should be monotonically increasing
            ValidateColumn(sutColumn, colIndex, ColumnCover.Primary, elt1);

            var elt2 = new ElementObject(dummyRoot, sutColumn, 2, colIndex);
            ValidateColumn(sutColumn, colIndex, ColumnCover.Primary, elt1, elt2);

            var elt3 = new ElementObject(dummyRoot, sutColumn, 4, colIndex);
            ValidateColumn(sutColumn, colIndex, ColumnCover.Primary, elt1, elt2, elt3);
        }
        #endregion
    }
}
