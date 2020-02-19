using System;
using System.Collections.Generic;
using System.Text;
using RDapter.Entities;

namespace RDapter.DataBuilder.Helper
{
    internal class Parameter
    {
        internal static DatabaseParameter[] ExtractDatabaseParameter(object o)
        {
            if (o == null) return Array.Empty<DatabaseParameter>();
            var properties = o.GetType().GetProperties();
            var param = new DatabaseParameter[properties.Length];
            for (var idx = 0; idx < properties.Length; idx++)
            {
                var property = properties[idx];
                var name = property.Name;
                var value = property.GetValue(o);
                var parameter = new DatabaseParameter(name, value);
                param[idx] = parameter;
            }
            return param;
        }

    }
}
