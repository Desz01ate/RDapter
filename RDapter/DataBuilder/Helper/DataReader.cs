using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;

namespace RDapter.DataBuilder.Helper
{
    internal static class DataReader
    {
        /// <summary>
        /// Convert IDataReader into dynamic object.
        /// </summary>
        /// <param name="row">data reader to convert to dynamic object</param>
        /// <returns></returns>
        internal static dynamic RowBuilder(this IDataReader row)
        {
            var rowInstance = new ExpandoObject() as IDictionary<string, object>;
            for (var idx = 0; idx < row.FieldCount; idx++)
            {
                rowInstance.Add(row.GetName(idx), row[idx]);
            }
            return rowInstance;
        }
    }
}
