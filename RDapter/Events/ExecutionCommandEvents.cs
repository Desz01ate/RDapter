using RDapter.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDapter.Events
{
    /// <summary>
    /// Contains event delegate signature for ExecutionCommand.
    /// </summary>
    public static class ExecutionCommandEvents
    {
        /// <summary>
        /// The event that will trigger when query is ready to execute.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public delegate void OnQueryReadyEventHandler(string sql, IEnumerable<DatabaseParameter> parameters);
        /// <summary>
        /// The event that will trigger when query is executed.
        /// </summary>
        /// <param name="affectedRows"></param>
        public delegate void OnQueryExecutedEventHandler(int affectedRows);
    }
}
