using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace RDapter.Entities
{
    /// <summary>
    /// Provide an abstract layer for IDbParameter for using in a non-generic environment.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public sealed class DatabaseParameter
    {
        private object _value;
        /// <summary>
        /// Name of parameter;
        /// </summary>
        public string ParameterName;
        /// <summary>
        /// Value of parameter;
        /// </summary>
        public object Value
        {
            get
            {
                if (_bindingRedirectionParameter != null)
                {
                    return _bindingRedirectionParameter.Value;
                }
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        /// <summary>
        /// Direction of parameter;
        /// </summary>
        public ParameterDirection Direction;
        /// <summary>
        /// Get the <seealso cref="System.Data.DbType"/> of the parameter, this property is currently not allow to set.
        /// </summary>
        public DbType? DbType => _bindingRedirectionParameter?.DbType;
        private IDbDataParameter? _bindingRedirectionParameter;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Value of parameter</param>
        public DatabaseParameter(string parameterName, object value)
        {
            this.ParameterName = parameterName;
            this._value = value ?? DBNull.Value;
            this.Direction = System.Data.ParameterDirection.Input;
            this._bindingRedirectionParameter = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Value of parameter</param>
        /// <param name="direction">Direction of parameter</param>
        public DatabaseParameter(string parameterName, object value, ParameterDirection direction)
        {
            this.ParameterName = parameterName;
            this._value = value ?? DBNull.Value;
            this.Direction = direction;
            this._bindingRedirectionParameter = null;
        }

        internal void SetBindingRedirection(IDbDataParameter compatibleParameter)
        {
            if (this.Direction == ParameterDirection.InputOutput || this.Direction == ParameterDirection.Output)
            {
                this._bindingRedirectionParameter = compatibleParameter;
            }
        }
    }
}
