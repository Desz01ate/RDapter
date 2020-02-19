using RDapter.Extends.Enum;
using System;
namespace RDapter.Extends
{
    public static partial class Global
    {
        //    private static Dictionary<string, string> defaultTableNameMap = new Dictionary<string, string>();
        //    public static void AddDefaultTableNameMap<T>(string tableName)
        //    {
        //        var type = typeof(T);
        //        defaultTableNameMap.Add(type.Name, tableName);
        //    }
        //    public static string GetDefaultTableNameMap<T>()
        //    {
        //        var type = typeof(T);
        //        if (defaultTableNameMap.TryGetValue(type.Name, out var name))
        //        {
        //            return name;
        //        }
        //        return type.Name;
        //    }
        internal static Func<SqlFunction, string> DefaultSqlFunctionMap { get; private set; } = DefaultMapFunction;
        private static string DefaultMapFunction(SqlFunction arg)
        {
            return arg switch
            {
                SqlFunction.Length => "LEN",
                _ => throw new NotSupportedException(arg.ToString())
            };
        }
        public static void SetDefaultDataTypeMapper(Func<SqlFunction, string> func)
        {
            DefaultSqlFunctionMap = func;
        }
        internal static Func<Type, string> DefaultSqlTypeMap { get; private set; } = (type) =>
        {
            if (type == typeof(string))
            {
                return "NVARCHAR(1024)";
            }
            else if (type == typeof(char) || type == typeof(char?))
            {
                return "NCHAR(1)";
            }
            else if (type == typeof(short) || type == typeof(short?) || type == typeof(ushort) || type == typeof(ushort?))
            {
                return "SMALLINT";
            }
            else if (type == typeof(int) || type == typeof(int?) || type == typeof(uint) || type == typeof(uint?))
            {
                return "INT";
            }
            else if (type == typeof(long) || type == typeof(long?) || type == typeof(ulong) || type == typeof(ulong?))
            {
                return "BIGINT";
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                return "REAL";
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return "FLOAT";
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                return "BIT";
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return "MONEY";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return "DATETIME";
            }
            else if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return "UNIQUEIDENTIFIER";
            }
            else if (type == typeof(byte) || type == typeof(byte?) || type == typeof(sbyte) || type == typeof(sbyte?))
            {
                return "TINYINT";
            }
            else if (type == typeof(byte[]))
            {
                return "VARBINARY";
            }
            else
            {
                throw new NotSupportedException($"Unable to map type {type.FullName} (Not declared).");
            }
        };
        public static void SetDefaultSqlTypeMap(Func<Type, string> func)
        {
            DefaultSqlTypeMap = func;
        }
    }

    //    private static Dictionary<Type, BindingFlags> defaultTypeBflagsMap = new Dictionary<Type, BindingFlags>();
    //    public static void SetDefaultTypeBindingFlags<T>(BindingFlags bindingFlags)
    //    {
    //        var type = typeof(T);
    //        defaultTypeBflagsMap[type] = bindingFlags;
    //    }
    //    internal static BindingFlags GetDefaultTypeBindingFlags<T>()
    //    {
    //        var type = typeof(T);
    //        if (defaultTypeBflagsMap.TryGetValue(type, out var bflag))
    //        {
    //            return bflag;
    //        }
    //        return BindingFlags.Public | BindingFlags.Instance;
    //    }

    //    private static Dictionary<Type, string, bool> defaultTypePrimaryKeyMap = new Dictionary<Type, string, bool>();
    //    public static void SetPrimaryKey<T>(string primaryKeyColumnName, bool autoIncrement)
    //    {
    //        var type = typeof(T);
    //        defaultTypePrimaryKeyMap[type] = (primaryKeyColumnName, autoIncrement);
    //    }
    //    internal static bool IsPrimaryKey<T>(string propertyName, out (string columnName, bool isAutoIncrement) result)
    //    {
    //        if (defaultTypePrimaryKeyMap.TryGetValue(typeof(T), out var v))
    //        {
    //            result = v;
    //            return v.Equals(propertyName);
    //        }
    //        result = (string.Empty, false);
    //        return false;
    //    }
    //    internal static (string columnName, bool isAutoIncrement) GetPrimaryKey<T>()
    //    {
    //        var type = typeof(T);
    //        if (defaultTypePrimaryKeyMap.TryGetValue(type, out var v)) return v;
    //        throw new Exception($"{type.FullName} is not map with primary key.");
    //    }
    //    private static Dictionary<Type, bool, bool> defaultIgnoreMap = new Dictionary<Type, bool, bool>();
    //    public static void SetIgnorance<T>(bool OnInsert, bool OnUpdate)
    //    {
    //        defaultIgnoreMap.Add(typeof(T), (OnInsert, OnUpdate));
    //    }
    //    internal static bool IsIgnorance<T>(out (bool IgnoreInsert, bool IgnoreUpdate) result)
    //    {
    //        if (defaultIgnoreMap.TryGetValue(typeof(T), out var pair))
    //        {
    //            result = pair;
    //            return true;
    //        }
    //        result = (false, false);
    //        return false;
    //    }
    //}
}
