using RDapter.Extends.Enum;
using System;
namespace RDapter.Extends
{
    public static partial class Global
    {
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
}