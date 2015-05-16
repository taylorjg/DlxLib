using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    /// <summary>
    /// Extension methods to perform validations on various parameters, expressions, etc.
    /// </summary>
    public static class Validations
    {
        /// <summary>
        /// Validator throws exception if argument is null.
        /// </summary>
        public static void MustNotBeNull(this object obj, string name)
        {
            if (null == obj)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
