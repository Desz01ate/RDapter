using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RDapter
{
    public static partial class Global
    {
        private static Dictionary<Type, Dictionary<string, string>> defaultTypeMap;
        static Global()
        {
            defaultTypeMap = new Dictionary<Type, Dictionary<string, string>>();
        }
        public static void AddDefaultTypeMap<T>(Func<PropertyInfo, string> action)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = action(property);
                AddDefaultTypeMap<T>(property.Name, value);
            }
        }
        public static void AddDefaultTypeMap<T>(string property, string value)
        {
            var type = typeof(T);
            if (defaultTypeMap.TryGetValue(type, out var dictValue))
            {
                dictValue[property] = value;
            }
            else
            {
                dictValue = new Dictionary<string, string>();
                dictValue[property] = value;
                defaultTypeMap.Add(type, dictValue);
            }
        }
        public static string GetDefaultTypeMap<T>(string property)
        {
            var type = typeof(T);
            return GetDefaultTypeMap(type, property);
        }
        public static string GetDefaultTypeMap(Type type, string property)
        {
            if (defaultTypeMap.TryGetValue(type, out var dictValue))
            {
                if (dictValue.TryGetValue(property, out var value))
                {
                    return value;
                }
                return property;
            }
            return property;
        }
        public static void ClearTypeMap()
        {
            defaultTypeMap.Clear();
        }
    }
}
