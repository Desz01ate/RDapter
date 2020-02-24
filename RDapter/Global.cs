using RDapter.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RDapter
{
    public static partial class Global
    {
        private static Dictionary<Type, DTOSchemaConstraint> defaultMapConstraint = new Dictionary<Type, DTOSchemaConstraint>();
        public static void SetSchemaConstraint<T>(Action<DTOSchemaConstraint> action)
        {
            var type = typeof(T);
            SetSchemaConstraint(type, action);
        }
        public static void SetSchemaConstraint(Type type, Action<DTOSchemaConstraint> action)
        {
            var constraint = new DTOSchemaConstraint();
            constraint.SetTableName(type.Name);
            action(constraint);
            constraint.Apply();
            defaultMapConstraint[type] = constraint;
        }
        public static DTOSchemaConstraint GetSchemaConstraint<T>()
        {
            var type = typeof(T);
            return GetSchemaConstraint(type);
        }
        public static DTOSchemaConstraint GetSchemaConstraint(Type type)
        {
            if (defaultMapConstraint.TryGetValue(type, out var v))
            {
                return v;
            }
            SetSchemaConstraint(type, (constraint) =>
             {
                 constraint.SetBindingFlags(BindingFlags.Public | BindingFlags.Instance);
                 foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                 {
                     constraint.SetField(property.Name, property.Name, false, false);
                 }
             });
            return GetSchemaConstraint(type);
        }
    }
}
