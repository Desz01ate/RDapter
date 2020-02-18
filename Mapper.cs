using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using RDapter.DataBuilder;
using RDapter.DataBuilder.Helper;
using RDapter.DataBuilder.Interface;
using RDapter.Entities;

namespace RDapter
{
    /// <summary>
    /// Default mapper execute-methods collection.
    /// </summary>
    public static partial class Mapper
    {
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static IEnumerable<T> ExecuteReader<T>(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T : class, new() => ExecuteReader<T>(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, buffered));

        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Connection">Database connection.</param>
        /// <param name="execCommand">Execute command definition.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static IEnumerable<T> ExecuteReader<T>(this DbConnection Connection, ExecutionCommand execCommand)
            where T : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = execCommand.CommandText;
            command.Transaction = execCommand.Transaction;
            command.CommandType = execCommand.CommandType;
            if (execCommand.Parameters != null)
            {
                foreach (var parameter in execCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = command.ExecuteReader();

            var deferred = DataReaderBuilder<T>(cursor);
            if (execCommand.Buffered) deferred = deferred.AsList();
            return deferred;
        }

        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of dynamic object
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <returns>IEnumerable of dynamic object</returns>

        public static IEnumerable<dynamic> ExecuteReader(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true) => ExecuteReader(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, buffered));

        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of dynamic object
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns>IEnumerable of dynamic object</returns>

        public static IEnumerable<dynamic> ExecuteReader(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = executionCommand.CommandText;
            command.Transaction = executionCommand.Transaction;
            command.CommandType = executionCommand.CommandType;
            if (executionCommand.Parameters != null)
            {
                foreach (var parameter in executionCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }

            var cursor = command.ExecuteReader();

            var deferred = DataReaderDynamicBuilder(cursor);
            if (executionCommand.Buffered) deferred = deferred.AsList();
            return deferred;
        }

        /// <summary>
        /// Execute SELECT SQL query and return a scalar object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static T ExecuteScalar<T>(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar<T>(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }

        /// <summary>
        /// Execute SELECT SQL query and return a scalar object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static T ExecuteScalar<T>(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            T result;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter);
                    }
                }

                result = (T)command.ExecuteScalar();
            }
            return result;
        }

        /// <summary>
        /// Execute any non-DML SQL Query
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static int ExecuteNonQuery(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQuery(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute any non-DML SQL Query
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static int ExecuteNonQuery(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            int result;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter);
                    }
                }

                result = command.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <returns>IEnumerable of POCO</returns>

        public static Task<IEnumerable<T>> ExecuteReaderAsync<T>(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T : class, new()
        {
            return ExecuteReaderAsync<T>(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, buffered));
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns>IEnumerable of POCO</returns>

        public static async Task<IEnumerable<T>> ExecuteReaderAsync<T>(this DbConnection Connection, ExecutionCommand executionCommand)
            where T : class, new()
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            using var command = Connection.CreateCommand();
            command.CommandText = executionCommand.CommandText;
            command.Transaction = executionCommand.Transaction;
            command.CommandType = executionCommand.CommandType;
            if (executionCommand.Parameters != null)
            {
                foreach (var parameter in executionCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }

            var cursor = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var deferred = DataReaderBuilder<T>(cursor);
            if (executionCommand.Buffered) deferred = deferred.AsList();
            return deferred;
        }

        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of dynamic object
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="transaction"></param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <returns>IEnumerable of dynamic object</returns>
        public static Task<IEnumerable<dynamic>> ExecuteReaderAsync(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
        {
            return ExecuteReaderAsync(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, buffered));
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of dynamic object
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns>IEnumerable of dynamic object</returns>
        public static async Task<IEnumerable<dynamic>> ExecuteReaderAsync(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            using var command = Connection.CreateCommand();

            command.CommandText = executionCommand.CommandText;
            command.Transaction = executionCommand.Transaction;
            command.CommandType = executionCommand.CommandType;
            if (executionCommand.Parameters != null)
            {
                foreach (var parameter in executionCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }

            var cursor = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var deferred = DataReaderDynamicBuilder(cursor);
            if (executionCommand.Buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return a scalar object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static Task<T> ExecuteScalarAsync<T>(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarAsync<T>(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }

        /// <summary>
        /// Execute SELECT SQL query and return a scalar object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static async Task<T> ExecuteScalarAsync<T>(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            T result;

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter);
                    }
                }

                result = (T)(await command.ExecuteScalarAsync().ConfigureAwait(false));
            }
            return result;
        }

        /// <summary>
        /// Execute any non-DML SQL Query
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static Task<int> ExecuteNonQueryAsync(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQueryAsync(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute any non-DML SQL Query
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static async Task<int> ExecuteNonQueryAsync(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            int result;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter);
                    }
                }

                result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            return result;
        }
        /// <summary>
        /// Execute SELECT SQL query and return a string
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static object ExecuteScalar(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute SELECT SQL query and return a string
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static object ExecuteScalar(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            object result;

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter);
                    }
                }

                result = command.ExecuteScalar();
            }
            return result;
        }
        /// <summary>
        /// Execute SELECT SQL query and return a string in asynchronous manner
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>

        public static Task<object> ExecuteScalarAsync(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalarAsync(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute SELECT SQL query and return a string in asynchronous manner
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>

        public static async Task<object> ExecuteScalarAsync(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            object result;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = executionCommand.CommandText;
                command.Transaction = executionCommand.Transaction;
                command.CommandType = executionCommand.CommandType;
                if (executionCommand.Parameters != null)
                {
                    foreach (var parameter in executionCommand.Parameters)
                    {
                        var compatibleParameter = command.CreateParameter();
                        compatibleParameter.ParameterName = parameter.ParameterName;
                        compatibleParameter.Value = parameter.Value;
                        compatibleParameter.Direction = parameter.Direction;
                        parameter.SetBindingRedirection(compatibleParameter);
                        command.Parameters.Add(compatibleParameter); command.Parameters.Add(parameter);
                    }
                }

                result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            return result;
        }
        /// <summary>
        /// Execute SELECT SQL query and return DataTable
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static DataTable ExecuteReaderAsDataTable(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteReaderAsDataTable(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute SELECT SQL query and return DataTable
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>
        public static DataTable ExecuteReaderAsDataTable(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            using var command = Connection.CreateCommand();
            command.CommandText = executionCommand.CommandText;
            command.Transaction = executionCommand.Transaction;
            command.CommandType = executionCommand.CommandType;
            if (executionCommand.Parameters != null)
            {
                foreach (var parameter in executionCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }

            var cursor = command.ExecuteReader();

            var dataTable = new DataTable();
            dataTable.Load(cursor);
            return dataTable;
        }
        /// <summary>
        /// Execute SELECT SQL query and return DataTable in an asynchronous manner
        /// </summary>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns></returns>
        public static Task<DataTable> ExecuteReaderAsDataTableAsync(this DbConnection Connection, string sql, object? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteReaderAsDataTableAsync(Connection, new ExecutionCommand(sql, Parameter.ExtractDatabaseParameter(parameters), commandType, transaction, false));
        }
        /// <summary>
        /// Execute SELECT SQL query and return DataTable in an asynchronous manner
        /// </summary>
        /// <param name="executionCommand">The execution command.</param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteReaderAsDataTableAsync(this DbConnection Connection, ExecutionCommand executionCommand)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();

            using var command = Connection.CreateCommand();
            command.CommandText = executionCommand.CommandText;
            command.Transaction = executionCommand.Transaction;
            command.CommandType = executionCommand.CommandType;
            if (executionCommand.Parameters != null)
            {
                foreach (var parameter in executionCommand.Parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }

            var cursor = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var dataTable = new DataTable();
            dataTable.Load(cursor);
            return dataTable;
        }
        private static IEnumerable<dynamic> DataReaderDynamicBuilder(IDataReader reader)
        {
            using (reader)
            {
                while (reader.Read())
                {
                    yield return DataReader.RowBuilder(reader);
                }
            }

        }
        private static IEnumerable<T> DataReaderBuilder<T>(IDataReader reader) where T : class, new()
        {
            using (reader)
            {
                IDataMapper<T> converter = new Converter<T>(reader);
                while (reader.Read())
                {
                    yield return converter.GenerateObject();
                }
            }
        }
    }
    public static partial class Mapper
    {
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static IEnumerable<T1> ExecuteReader<T1, T2>(this DbConnection Connection, string sql, Func<T1, T2, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = command.ExecuteReader();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static IEnumerable<T1> ExecuteReader<T1, T2, T3>(this DbConnection Connection, string sql, Func<T1, T2, T3, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = command.ExecuteReader();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static IEnumerable<T1> ExecuteReader<T1, T2, T3, T4>(this DbConnection Connection, string sql, Func<T1, T2, T3, T4, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = command.ExecuteReader();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static async Task<IEnumerable<T1>> ExecuteReaderAsync<T1, T2>(this DbConnection Connection, string sql, Func<T1, T2, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = await command.ExecuteReaderAsync();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static async Task<IEnumerable<T1>> ExecuteReaderAsync<T1, T2, T3>(this DbConnection Connection, string sql, Func<T1, T2, T3, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = await command.ExecuteReaderAsync();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        /// <summary>
        /// Execute SELECT SQL query and return IEnumerable of specified POCO that is matching with the query columns, this overload is suitable when you perform SELECT-JOIN query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Any SELECT SQL that you want to perform with/without parameterized parameters (Do not directly put sql parameter in this parameter).</param>
        /// <param name="map">Map function.</param>
        /// <param name="parameters">SQL parameters according to the sql parameter.</param>
        /// <param name="commandType">Type of SQL Command.</param>
        /// <param name="buffered">Whether to buffered result in memory.</param>
        /// <param name="transaction">Transaction for current execution.</param>
        /// <returns>IEnumerable of POCO</returns>
        public static async Task<IEnumerable<T1>> ExecuteReaderAsync<T1, T2, T3, T4>(this DbConnection Connection, string sql, Func<T1, T2, T3, T4, T1> map, IEnumerable<DatabaseParameter>? parameters = null, DbTransaction? transaction = null, CommandType commandType = CommandType.Text, bool buffered = true)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var compatibleParameter = command.CreateParameter();
                    compatibleParameter.ParameterName = parameter.ParameterName;
                    compatibleParameter.Value = parameter.Value;
                    compatibleParameter.Direction = parameter.Direction;
                    parameter.SetBindingRedirection(compatibleParameter);
                    command.Parameters.Add(compatibleParameter);
                }
            }
            
            var cursor = await command.ExecuteReaderAsync();
            
            var deferred = Convert(cursor, map);
            if (buffered) deferred = deferred.AsList();
            return deferred;
        }
        internal static IEnumerable<T1> Convert<T1, T2>(IDataReader reader, Func<T1, T2, T1> map)
where T1 : class, new()
where T2 : class, new()
        {
            using (reader)
            {
                IDataMapper<T1> converter = new Converter<T1>(reader);
                IDataMapper<T2> childConverter = new Converter<T2>(reader);
                while (reader.Read())
                {
                    yield return map(converter.GenerateObject(), childConverter.GenerateObject());
                }
            }
        }
        internal static IEnumerable<T1> Convert<T1, T2, T3>(IDataReader reader, Func<T1, T2, T3, T1> map)
    where T1 : class, new()
    where T2 : class, new()
            where T3 : class, new()
        {
            using (reader)
            {
                IDataMapper<T1> converter = new Converter<T1>(reader);
                IDataMapper<T2> converter2 = new Converter<T2>(reader);
                IDataMapper<T3> converter3 = new Converter<T3>(reader);

                while (reader.Read())
                {
                    yield return map(converter.GenerateObject(), converter2.GenerateObject(), converter3.GenerateObject());
                }
            }
        }
        internal static IEnumerable<T1> Convert<T1, T2, T3, T4>(IDataReader reader, Func<T1, T2, T3, T4, T1> map)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            using (reader)
            {
                IDataMapper<T1> converter = new Converter<T1>(reader);
                IDataMapper<T2> converter2 = new Converter<T2>(reader);
                IDataMapper<T3> converter3 = new Converter<T3>(reader);
                IDataMapper<T4> converter4 = new Converter<T4>(reader);
                while (reader.Read())
                {
                    yield return map(converter.GenerateObject(), converter2.GenerateObject(), converter3.GenerateObject(), converter4.GenerateObject());
                }
            }
        }

    }
}
