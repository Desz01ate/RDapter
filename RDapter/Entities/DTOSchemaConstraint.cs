using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RDapter.Entities
{
    public class DTOSchemaConstraint
    {
        private string _tableName;
        public string TableName
        {
            get
            {
                return CheckAppliedState(_tableName);
            }
            private set
            {
                _tableName = value;
            }
        }
        public FieldConstraint PrimaryKey
        {
            get
            {
                return CheckAppliedState(_fields.SingleOrDefault(x => x.IsPrimaryKey));
            }
        }
        private List<FieldConstraint> _fields = new List<FieldConstraint>();
        private BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        public BindingFlags ResolveBindingFlags
        {
            get
            {
                return CheckAppliedState(_bindingFlags);
            }
            private set
            {
                _bindingFlags = value;
            }
        }
        private T CheckAppliedState<T>(T obj)
        {
            if (!applied) throw new Exception("DTOSchema is not applied correctly.");
            return obj;
        }
        private bool applied { get; set; }
        public DTOSchemaConstraint SetTableName(string name)
        {
            //if (!string.IsNullOrWhiteSpace(_tableName)) throw new InvalidOperationException();
            this.TableName = name;
            return this;
        }
        public DTOSchemaConstraint SetPrimaryKey(string name, string sqlName = null, bool autoIncrement = false)
        {
            if (_fields.Any(x => x.Name == name)) throw new InvalidOperationException();
            if (sqlName == null) sqlName = name;
            SetField(name, sqlName, false, false, true, autoIncrement, true);
            return this;
        }
        public DTOSchemaConstraint SetPrimaryKey<TType>(Expression<Func<TType, object>> expression, string sqlName = null, bool autoIncrement = false) where TType : class
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Helpers.Expression.TryGetMemberName(expression, out var name))
            {
                return SetPrimaryKey(name, sqlName, autoIncrement);
            }
            return this;
        }
        private DTOSchemaConstraint SetField(string name, string sqlName, bool ignoreInsert = false, bool ignoreUpdate = false, bool isPrimaryKey = false, bool isAutoIncrement = false, bool isNotNull = false)
        {
            if (_fields.Any(x => x.Name == name)) throw new InvalidOperationException();
            _fields.Add(new FieldConstraint(name, sqlName, ignoreInsert, ignoreUpdate, isPrimaryKey, isAutoIncrement, isNotNull));
            return this;
        }
        public DTOSchemaConstraint SetField(string name, string sqlName, bool ignoreInsert = false, bool ignoreUpdate = false, bool isNotNull = false)
        {
            if (_fields.Any(x => x.Name == name)) throw new InvalidOperationException();
            _fields.Add(new FieldConstraint(name, sqlName, ignoreInsert, ignoreUpdate, isNotNull: isNotNull));
            return this;
        }
        public DTOSchemaConstraint SetField<TType>(Expression<Func<TType, object>> expression, string sqlName, bool ignoreInsert = false, bool ignoreUpdate = false, bool isNotNull = false)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Helpers.Expression.TryGetMemberName(expression, out var name))
            {
                return SetField(name, sqlName, ignoreInsert, ignoreUpdate, isNotNull);
            }
            return this;
        }
        public FieldConstraint GetField(string name)
        {
            var field = _fields.SingleOrDefault(x => x.Name == name);
            if (field != null) return field;
            SetField(name, name, false, false, false);
            return GetField(name);
        }
        public DTOSchemaConstraint SetBindingFlags(BindingFlags bindingFlags)
        {
            ResolveBindingFlags = bindingFlags;
            return this;
        }
        internal void Apply()
        {
            if (string.IsNullOrWhiteSpace(_tableName)) throw new ArgumentNullException(TableName);
            applied = true;
        }

    }

    public class FieldConstraint
    {
        public readonly string Name;
        public readonly string SqlName;
        public readonly bool IgnoreInsert;
        public readonly bool IgnoreUpdate;
        public readonly bool IsPrimaryKey;
        public readonly bool AutoIncrement;
        public readonly bool NotNull;

        internal FieldConstraint(string name, string sqlName, bool insert, bool update, bool isPrimaryKey = false, bool isAutoIncrement = false, bool isNotNull = false)
        {
            Name = name;
            SqlName = sqlName;
            IgnoreInsert = insert;
            IgnoreUpdate = update;
            this.IsPrimaryKey = isPrimaryKey;
            this.AutoIncrement = isAutoIncrement;
            this.NotNull = isNotNull;
        }
        public bool IsFieldIgnored(string name)
        {
            return (this.Name == name) && (this.IgnoreInsert || this.IgnoreUpdate);
        }
    }
}
