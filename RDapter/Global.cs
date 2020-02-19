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
                 constraint.SetTableName(type.Name);
                 constraint.SetBindingFlags(BindingFlags.Public | BindingFlags.Instance);
                 foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                 {
                     constraint.SetField(property.Name, property.Name, false, false);
                 }
             });
            return GetSchemaConstraint(type);
        }
    }
    //public static partial class Global
    //{
    //    private static Dictionary<Type, Dictionary<string, string>> defaultTypeMap;
    //    static Global()
    //    {
    //        defaultTypeMap = new Dictionary<Type, Dictionary<string, string>>();
    //    }
    //    public static void AddDefaultTypeMap<T>(Func<PropertyInfo, string> action)
    //    {
    //        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    //        foreach (var property in properties)
    //        {
    //            var value = action(property);
    //            AddDefaultTypeMap<T>(property.Name, value);
    //        }
    //    }
    //    public static void AddDefaultTypeMap<T>(string property, string value)
    //    {
    //        var type = typeof(T);
    //        if (defaultTypeMap.TryGetValue(type, out var dictValue))
    //        {
    //            dictValue[property] = value;
    //        }
    //        else
    //        {
    //            dictValue = new Dictionary<string, string>();
    //            dictValue[property] = value;
    //            defaultTypeMap.Add(type, dictValue);
    //        }
    //    }
    //    public static string GetDefaultTypeMap<T>(string property)
    //    {
    //        var type = typeof(T);
    //        return GetDefaultTypeMap(type, property);
    //    }
    //    public static string GetDefaultTypeMap(Type type, string property)
    //    {
    //        if (defaultTypeMap.TryGetValue(type, out var dictValue))
    //        {
    //            if (dictValue.TryGetValue(property, out var value))
    //            {
    //                return value;
    //            }
    //            return property;
    //        }
    //        return property;
    //    }
    //    public static void ClearTypeMap()
    //    {
    //        defaultTypeMap.Clear();
    //    }

    //}
}
