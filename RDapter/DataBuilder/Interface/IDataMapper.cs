using System;
using System.Collections.Generic;
using System.Text;

namespace RDapter.DataBuilder.Interface
{
    internal interface IDataMapper<out T> where T : new()
    {
        T GenerateObject();
        IEnumerable<T> GenerateObjects();
    }
}
