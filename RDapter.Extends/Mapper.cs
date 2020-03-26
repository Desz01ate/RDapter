using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RDapter;
using RDapter.Entities;
using RDapter.Extends.Builder;
using RDapter.Extends.Helper;

namespace RDapter.Extends
{
    /// <summary>
    /// Default mapper execute-methods collection.
    /// </summary>
    public static partial class Mapper
    {
        /// <summary>
        /// Select all rows from table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="top">Specified TOP(n) rows.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this DbConnection connector, int? top = null, DbTransaction? transaction = null, bool buffered = true)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(top);
            var ec = new ExecutionCommand(query, null, System.Data.CommandType.Text, transaction, buffered: buffered);
            IEnumerable<T> result = connector.ExecuteReader<T>(ec);
            return result;
        }

        /// <summary>
        /// Select one row from table from given primary key (primary key can be set by [PrimaryKey] attribute, table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="primaryKey">Primary key of specific row</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static T Query<T>(this DbConnection connector, object primaryKey, DbTransaction? transaction = null, bool buffered = true)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.SelectQueryGenerate<T>(primaryKey);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction, buffered: buffered);
            T result = connector.ExecuteReader<T>(ec).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Select first row from table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static T QueryFirst<T>(this DbConnection connector, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(top: 1);
            var ec = new ExecutionCommand(query, null, System.Data.CommandType.Text, transaction, buffered: buffered);
            T result = connector.ExecuteReader<T>(ec).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Select first row from table by using matched predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static T QueryFirst<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(predicate, 1);
            var ec = new ExecutionCommand(query.query, query.parameters, System.Data.CommandType.Text, transaction, buffered: buffered);
            T result = connector.ExecuteReader<T>(ec).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Insert row into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">Object to insert.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an insert.</returns>
        public static int Insert<T>(this DbConnection connector, T obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            if (obj == null) return -1;
            var preparer = connector.InsertQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            var result = connector.ExecuteNonQuery(ec);
            return result;
        }

        /// <summary>
        /// Insert rows into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">IEnumrable to insert.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an insert.</returns>
        public static int InsertMany<T>(this DbConnection connector, IEnumerable<T> obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            if (obj == null || !obj.Any()) return -1;
            var preparer = connector.InsertQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = connector.ExecuteNonQuery(ec);
            return result;
        }

        /// <summary>
        /// Update specific object into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">Object to update.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an update.</returns>
        public static int Update<T>(this DbConnection connector, T obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.UpdateQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            var value = connector.ExecuteNonQuery(ec);
            return value;
        }

        /// <summary>
        /// Delete given object from table by inference of [PrimaryKey] attribute. (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj"></param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static int Delete<T>(this DbConnection connector, T obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = connector.ExecuteNonQuery(ec);
            return result;
        }

        /// <summary>
        /// Select all rows from table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="top">Specified TOP(n) rows.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>IEnumerable of object</returns>
        public static async Task<IEnumerable<T>> QueryAsync<T>(this DbConnection connector, int? top = null, DbTransaction? transaction = null, bool buffered = true)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(top);
            var ec = new ExecutionCommand(query, null, transaction: transaction, buffered: buffered);
            var result = await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Select one row from table from given primary key (primary key can be set by [PrimaryKey] attribute, table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="primaryKey">Primary key of specific row</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static async Task<T> QueryAsync<T>(this DbConnection connector, object primaryKey, DbTransaction? transaction = null, bool buffered = true)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.SelectQueryGenerate<T>(primaryKey);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction, buffered: buffered);

            var result = (await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false)).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Select first row from table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static async Task<T> QueryFirstAsync<T>(this DbConnection connector, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(top: 1);
            var ec = new ExecutionCommand(query, null, System.Data.CommandType.Text, transaction, buffered: buffered);

            T result = (await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false)).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Select first row from table by using matched predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns>Object of given class</returns>
        public static async Task<T> QueryFirstAsync<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>(predicate, 1);
            var ec = new ExecutionCommand(query.query, query.parameters, System.Data.CommandType.Text, transaction, buffered: buffered);

            T result = (await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false)).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// Insert row into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">Object to insert.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an insert.</returns>
        public static async Task<int> InsertAsync<T>(this DbConnection connector, T obj, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.InsertQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Insert rows into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">Object to insert.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an insert.</returns>
        public static async Task<int> InsertManyAsync<T>(this DbConnection connector, IEnumerable<T> obj, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.InsertQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Update specific object into table (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj">Object to update.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>Affected row after an update.</returns>
        public static async Task<int> UpdateAsync<T>(this DbConnection connector, T obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.UpdateQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Delete given object from table by inference of [PrimaryKey] attribute. (table name is a class name or specific [Table] attribute, an attribute has higher priority).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="obj"></param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this DbConnection connector, T obj, DbTransaction? transaction = null)
            where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(obj);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);

            var result = await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Select data from table by using matched predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="top">Specified TOP(n) rows.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, int? top = null, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.SelectQueryGenerate<T>(predicate, top);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction, buffered: buffered);

            var result = connector.ExecuteReader<T>(ec);
            return result;
        }

        /// <summary>
        /// Delete data from table by using matched predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static int Delete<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(predicate);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            return connector.ExecuteNonQuery(ec);
        }

        /// <summary>
        /// Select data from table by using matched predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="top">Specified TOP(n) rows.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAsync<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, int? top = null, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.SelectQueryGenerate<T>(predicate, top);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction, buffered: buffered);
            var result = await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Select data from table by using matched predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="predicate">Predicate of data in LINQ manner</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this DbConnection connector, Expression<Func<T, bool>> predicate, DbTransaction? transaction = null) where T : class, new()
        {
            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(predicate);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            return await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
        }

        /// <summary>
        /// Select data from table by using primary key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="primaryKey">Specified primary key.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static int Delete<T>(this DbConnection connector, object primaryKey, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(primaryKey);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            return connector.ExecuteNonQuery(ec);
        }

        /// <summary>
        /// Select data from table by using primary key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="primaryKey">Specified primary key.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this DbConnection connector, object primaryKey, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var preparer = connector.DeleteQueryGenerate<T>(primaryKey);
            var query = preparer.query;
            var parameters = preparer.parameters;
            var ec = new ExecutionCommand(query, parameters, System.Data.CommandType.Text, transaction);
            return await connector.ExecuteNonQueryAsync(ec).ConfigureAwait(false);
        }


        /// <summary>
        /// Returns rows count from specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Count<T>(this DbConnection connector) where T : class
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var query = $"SELECT COUNT(*) FROM {tableName}";
            var count = connector.ExecuteScalar(query);
            var countAsString = count.ToString();
            return int.Parse(countAsString);
        }
        /// <summary>
        /// Returns rows count from specified table in an asynchronous manner.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<int> CountAsync<T>(this DbConnection connector) where T : class
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var query = $"SELECT COUNT(*) FROM {tableName}";
            var count = await connector.ExecuteScalarAsync(query).ConfigureAwait(false);
            var countAsString = count.ToString();
            return int.Parse(countAsString);
        }
        /// <summary>
        /// Returns rows count on specific condition from specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Count<T>(this DbConnection connector, Expression<Func<T, bool>> predicate) where T : class
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var exprTranslator = new ExpressionTranslator<T>();
            var translateResult = exprTranslator.Translate(predicate);
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE {translateResult.Expression}";
            var ec = new ExecutionCommand(query, translateResult.Parameters, System.Data.CommandType.Text, null);

            var count = connector.ExecuteScalar(ec);
            var countAsString = count.ToString();
            return int.Parse(countAsString);
        }
        /// <summary>
        /// Returns rows count on specific condition from specified table in an asynchronous manner.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<int> CountAsync<T>(this DbConnection connector, Expression<Func<T, bool>> predicate) where T : class
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var tableName = constraint.TableName;
            var exprTranslator = new ExpressionTranslator<T>();
            var translateResult = exprTranslator.Translate(predicate);
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE {translateResult.Expression}";
            var ec = new ExecutionCommand(query, translateResult.Parameters, System.Data.CommandType.Text, null);

            var count = await connector.ExecuteScalarAsync(ec).ConfigureAwait(false);
            var countAsString = count.ToString();
            return int.Parse(countAsString);
        }
    }
    public static partial class Mapper
    {
        /// <summary>
        /// Select rows from table by skipping rows by specified offset and take limit rows (SQL Server syntax).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="offset">The amount of rows to be offset (skip).</param>
        /// <param name="limit">The amount of rows to be take.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static IEnumerable<T> QueryOffset<T>(this DbConnection connector, int offset, int limit, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var query = connector.SelectQueryGenerate<T>();
            var primaryKey = constraint.PrimaryKey;
            var orderBy = primaryKey.Name;
            var queryAppender = new StringBuilder(query);
            queryAppender.AppendLine($" ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY");
            var queryAppendString = queryAppender.ToString();
            var ec = new ExecutionCommand(queryAppendString, null, transaction: transaction, buffered: buffered);
            var result = connector.ExecuteReader<T>(ec);
            return result;
        }
        /// <summary>
        /// Select rows from table by skipping rows by specified offset and take limit rows (SQL Server syntax).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="orderBy">Order by column.</param>
        /// <param name="offset">The amount of rows to be offset (skip).</param>
        /// <param name="limit">The amount of rows to be take.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static IEnumerable<T> QueryOffset<T>(this DbConnection connector, string orderBy, int offset, int limit, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>();
            var queryAppender = new StringBuilder(query);
            queryAppender.AppendLine($" ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY");
            var ec = new ExecutionCommand(queryAppender.ToString(), null, transaction: transaction, buffered: buffered);

            var result = connector.ExecuteReader<T>(ec);
            return result;
        }
        /// <summary>
        /// Select rows from table by skipping rows by specified offset and take limit rows (SQL Server syntax).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="offset">The amount of rows to be offset (skip).</param>
        /// <param name="limit">The amount of rows to be take.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryOffsetAsync<T>(this DbConnection connector, int offset, int limit, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var constraint = RDapter.Global.GetSchemaConstraint<T>();
            var query = connector.SelectQueryGenerate<T>();
            var primaryKey = constraint.PrimaryKey;
            var orderBy = primaryKey.Name;
            var queryAppender = new StringBuilder(query);
            queryAppender.AppendLine($" ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY");
            var ec = new ExecutionCommand(queryAppender.ToString(), null, transaction: transaction, buffered: buffered);
            var result = await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false);
            return result;
        }
        /// <summary>
        /// Select rows from table by skipping rows by specified offset and take limit rows (SQL Server syntax).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="orderBy">Order by column.</param>
        /// <param name="offset">The amount of rows to be offset (skip).</param>
        /// <param name="limit">The amount of rows to be take.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether the data should be cached in memory.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryOffsetAsync<T>(this DbConnection connector, string orderBy, int offset, int limit, DbTransaction? transaction = null, bool buffered = true) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.SelectQueryGenerate<T>();
            var queryAppender = new StringBuilder(query);
            queryAppender.AppendLine($" ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY");
            var ec = new ExecutionCommand(queryAppender.ToString(), null, transaction: transaction, buffered: buffered);
            var result = await connector.ExecuteReaderAsync<T>(ec).ConfigureAwait(false);
            return result;
        }
        /// <summary>
        /// Create table from model object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static int CreateTable<T>(this DbConnection connector, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.GenerateCreateTableStatement<T>();
            var result = connector.ExecuteNonQuery(query, transaction: transaction);
            return result;
        }
        /// <summary>
        /// Create table from model object in an asynchronous manner.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static async Task<int> CreateTableAsync<T>(this DbConnection connector, DbTransaction? transaction = null) where T : class, new()
        {

            if (connector == null) throw new InvalidCastException($"{connector.GetType().FullName} cannot be use with this extension (expected to get instance of {typeof(DbConnection).FullName}");
            var query = connector.GenerateCreateTableStatement<T>();
            var result = await connector.ExecuteNonQueryAsync(query, transaction: transaction).ConfigureAwait(false);
            return result;
        }
    }
}
