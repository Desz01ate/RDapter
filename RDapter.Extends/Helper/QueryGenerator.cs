using RDapter.Entities;
using RDapter.Extends.Builder;
using RDapter.Extends.Enum;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDapter.Extends.Helper
{
    /// <summary>
    /// Provide extension for SQL generate.
    /// </summary>
    internal static partial class QueryGenerate
    {
        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="top"></param>
        /// <returns></returns>
        public static string SelectQueryGenerate<T>(this DbConnection connector, int? top = null)
            where T : class
        {
            var tableName = RDapter.Global.GetSchemaConstraint<T>().TableName;
            var query = $"SELECT {(top.HasValue ? $"TOP({top.Value})" : "")} * FROM {tableName}";
            return query;
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) SelectQueryGenerate<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, int? top = null)
            where T : class
        {
            var tableName = RDapter.Global.GetSchemaConstraint<T>().TableName;
            var translator = new ExpressionTranslator<T>();
            var translateResult = translator.Translate(predicate);
            var query = string.Format("SELECT {0} * FROM {1} WHERE {2}", top.HasValue ? $"TOP({top.Value})" : "", tableName, translateResult.Expression);
            return (query, translateResult.Parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) SelectQueryGenerate<T>(this DbConnection connector, object primaryKey)
            where T : class
        {
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var pkName = constraint.PrimaryKey;
            var query = $"SELECT * FROM {tableName} WHERE {pkName.Name} = @{pkName.Name}";
            var parameter = new DatabaseParameter(pkName.Name, primaryKey);
            return (query, new[] { parameter });
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) InsertQueryGenerate<T>(this DbConnection connector, T obj)
            where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var tableName = RDapter.Global.GetSchemaConstraint<T>().TableName;
            var kvMapper = InsertUpdateMapper(obj, SqlType.Insert);
            var query = $@"INSERT INTO {tableName}
                              ({string.Join(",", kvMapper.Select(field => field.Key))})
                              VALUES
                              ({string.Join(",", kvMapper.Select(field => $"@{field.Key}"))})";
            var parameters = kvMapper.Select(field => new DatabaseParameter($"@{field.Key}", field.Value));
            return (query, parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) InsertQueryGenerate<T>(this DbConnection connector, IEnumerable<T> source)
            where T : class
        {
            var tableName = RDapter.Global.GetSchemaConstraint<T>().TableName;
            var enumerable = source as T[] ?? source.ToArray();
            var parametersMap = InsertUpdateMapper(enumerable.First(), SqlType.Insert);

            //values placeholder before append.
            var values = new List<string>();

            var parameters = new List<DatabaseParameter>();
            var query = new StringBuilder($@"INSERT INTO {tableName}");
            query.Append("(");
            query.Append($"{string.Join(",", parametersMap.Select(field => field.Key))}");
            query.AppendLine(")");
            query.AppendLine("VALUES");
            for (var idx = 0; idx < enumerable.Count(); idx++)
            {
                var obj = enumerable.ElementAt(idx);
                var mapper = InsertUpdateMapper(obj, SqlType.Insert);
                // ReSharper disable AccessToModifiedClosure
                var currentValueStatement = $"({string.Join(",", mapper.Select(x => $"@{x.Key}{idx}"))})";
                values.Add(currentValueStatement);
                var currentParameters = mapper.Select(x => new DatabaseParameter($"@{x.Key}{idx}", x.Value));
                parameters.AddRange(currentParameters);
                // ReSharper restore AccessToModifiedClosure

            }
            query.AppendLine(string.Join(",", values));
            return (query.ToString(), parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) UpdateQueryGenerate<T>(this DbConnection connector, T obj)
            where T : class
        {
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var primaryKey = constraint.PrimaryKey;
            var parametersMap = InsertUpdateMapper(obj, SqlType.Update);

            var parameters = new List<DatabaseParameter>();


            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"UPDATE {tableName} SET");
            var setter = new List<string>();
            foreach (var parameter in parametersMap)
            {
                //if the parameter is not a primary key, add it to the SET block.
                if (parameter.Key != primaryKey.Name)
                    setter.Add($"{parameter.Key} = @{parameter.Key}");
                parameters.Add(new DatabaseParameter($"@{parameter.Key}", parameter.Value));
            }
            stringBuilder.AppendLine(string.Join(",", setter));
            stringBuilder.AppendLine("WHERE");
            stringBuilder.AppendLine($"{primaryKey.Name} = @{primaryKey.Name}");

            var query = stringBuilder.ToString();

            return (query, parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) DeleteQueryGenerate<T>(this DbConnection connector, T obj)
            where T : class
        {
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var primaryKey = constraint.PrimaryKey.Name;

            var query = $"DELETE FROM {tableName} WHERE {primaryKey} = @{primaryKey}";
            var value = typeof(T).GetProperty(primaryKey).GetValue(obj);
            var parameters = new[] {
                    new DatabaseParameter(primaryKey,value)
            };
            return (query, parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) DeleteQueryGenerate<T>(this DbConnection connector, object primaryKey)
            where T : class
        {
            var constraint = RDapter.Global.GetSchemaConstraint<T>();

            var tableName = constraint.TableName;
            var pkName = constraint.PrimaryKey;
            var query = $"DELETE FROM {tableName} WHERE {pkName.Name} = @{pkName.Name}";
            var parameters = new[] {
                    new DatabaseParameter(pkName.Name,primaryKey)
                };
            return (query, parameters);
        }

        /// <summary>
        /// Generate SQL query with sql parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static (string query, IEnumerable<DatabaseParameter> parameters) DeleteQueryGenerate<T>(this DbConnection connector, Expression<Func<T, bool>> predicate)
            where T : class
        {
            var tableName = RDapter.Global.GetSchemaConstraint<T>().TableName;
            var translator = new ExpressionTranslator<T>();
            var translateResult = translator.Translate(predicate);
            var query = $@"DELETE FROM {tableName} WHERE {translateResult.Expression}";
            return (query, translateResult.Parameters);
        }
        /// <summary>
        /// Generate SQL create table query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static string GenerateCreateTableStatement<T>(this DbConnection connector)
            where T : class
        {
            //throw new NotImplementedException();
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var fields = new List<string>();
            var properties = typeof(T).GetProperties(constraint.ResolveBindingFlags);
            //var referenceProperties = typeof(T).ForeignKeyAttributeValidate();
            foreach (var property in properties)
            {
                var field = constraint.GetField(property.Name);
                var propertyName = field.SqlName;//AttributeExtension.FieldNameAttributeValidate(property);
                var IsNotNull = field.NotNull;//AttributeExtension.NotNullAttributeValidate(property);
                var primaryKeyPostfix = field.IsPrimaryKey ? " PRIMARY KEY " : "";
                var notNullPostfix = IsNotNull ? " NOT NULL " : "";
                var sqlType = RDapter.Extends.Global.DefaultSqlTypeMap(property.PropertyType);//connector.CompatibleSQLType(property.PropertyType);
                fields.Add($"{propertyName} {sqlType} {primaryKeyPostfix} {notNullPostfix}");
            }
            //foreach (var foreignKey in referenceProperties)
            //{
            //    var propertyName = AttributeExtension.FieldNameAttributeValidate(foreignKey);
            //    var targetTable = foreignKey.DeclaringType.TableNameAttributeValidate();
            //    fields.Add($"CONSTRAINT fk_{typeof(T).Name}_{targetTable} FOREIGN KEY ({foreignKey.ForeignKeyName}) REFERENCES {targetTable} ({propertyName})");
            //}
            return $"CREATE TABLE {tableName}({string.Join(",", fields)})";
        }
        private static Dictionary<string, object> InsertUpdateMapper<T>(T obj, SqlType type)
            where T : class
        {
            var values = new Dictionary<string, object>();
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            foreach (var property in typeof(T).GetProperties(constraint.ResolveBindingFlags))
            {
                var primaryKey = constraint.PrimaryKey;//property.GetCustomAttribute<PrimaryKeyAttribute>(true);
                var isPrimaryKey = primaryKey != null;
                var field = constraint.GetField(property.Name);//property.GetCustomAttribute<IgnoreFieldAttribute>(true);
                var isIgnoreField = field != null;
                if (isIgnoreField || isPrimaryKey)
                {
                    var disallowInsert = (isIgnoreField && field.IgnoreInsert) || (isPrimaryKey && primaryKey.AutoIncrement);
                    var disallowUpdate = (isIgnoreField && field.IgnoreUpdate);
                    if (type == SqlType.Insert && disallowInsert
                        || type == SqlType.Update && disallowUpdate)
                    {
                        continue;
                    }
                }
                var value = property.GetValue(obj);
                var name = field.SqlName;//property.FieldNameAttributeValidate();
                values.Add(name, value ?? DBNull.Value);
            }
            return values;
        }

    }
}
