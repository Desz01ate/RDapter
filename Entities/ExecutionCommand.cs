using RDapter.DataBuilder.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using static RDapter.Events.ExecutionCommandEvents;

namespace RDapter.Entities
{
    /// <summary>
    /// Contains execute command definition.
    /// </summary>
    public struct ExecutionCommand
    {
        /// <summary>
        /// The command text.
        /// </summary>
        public string CommandText { get; set; }
        /// <summary>
        /// The parameters related to query parameterized parameters.
        /// </summary>
        public IEnumerable<DatabaseParameter>? Parameters { get; set; }
        /// <summary>
        /// Type of command.
        /// </summary>
        public CommandType CommandType { get; set; }
        /// <summary>
        /// Database transaction.
        /// </summary>
        public DbTransaction? Transaction { get; set; }
        /// <summary>
        /// Should the execution be buffered.
        /// </summary>
        public bool Buffered { get; set; }
        /// <summary>
        /// The event that will trigger when query is ready to execute.
        /// </summary>
        public event OnQueryReadyEventHandler? OnQueryReady;
        /// <summary>
        /// The event that will trigger when query is executed.
        /// </summary>
        public event OnQueryExecutedEventHandler? OnqueryExecuted;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="commandText"></param>
        public ExecutionCommand(string commandText)
        {
            this.CommandText = commandText;
            this.Parameters = null;
            this.CommandType = CommandType.Text;
            this.Transaction = null;
            this.Buffered = true;
            OnQueryReady = null;
            OnqueryExecuted = null;
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="commandText"></param>
        public ExecutionCommand(string commandText, IEnumerable<DatabaseParameter> parameter, CommandType commandType = CommandType.Text, DbTransaction transaction = null, bool buffered = true)
        {
            this.CommandText = commandText;
            this.Parameters = parameter;
            this.CommandType = commandType;
            this.Transaction = transaction;
            this.Buffered = buffered;
            OnQueryReady = null;
            OnqueryExecuted = null;
        }
        internal void Executed(int affectedRows)
        {
            this.OnqueryExecuted?.Invoke(affectedRows);
        }
    }
}
