using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDapter.DataBuilder.Helper
{
    internal static class List
    {
        /// Implementation of Dapper's AsList (https://github.com/StackExchange/Dapper/blob/master/Dapper/SqlMapper.cs) Licensed as http://www.apache.org/licenses/LICENSE-2.0
        /// <summary>
        /// Obtains the data as a list; if it is *already* a list, the original object is returned without
        /// any duplication; otherwise, ToList() is invoked.
        /// </summary>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <param name="source">The enumerable to return as a list.</param>
        internal static List<T> AsList<T>(this IEnumerable<T> source) => (source == null || source is List<T>) ? (List<T>)source : source.ToList();

    }
}
