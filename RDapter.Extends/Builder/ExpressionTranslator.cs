using RDapter.Entities;
using RDapter.Extends.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDapter.Extends.Builder
{
    /// <summary>
    /// Provide expression tree translation service using visitor pattern.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public class ExpressionTranslator<TObject> : ExpressionVisitor
        where TObject : class
    {
        private StringBuilder sb;
        private string _orderBy = string.Empty;
        private int? _skip = null;
        private int? _take = null;
        private string _whereClause = string.Empty;
        private string _previousVisitField;
        private readonly Func<SqlFunction, string> _compatibleFunctionMap;
        private readonly Dictionary<string, string> _fieldsConfiguration;
        private readonly List<DatabaseParameter> _sqlParameters;

        public int? Skip
        {
            get
            {
                return _skip;
            }
        }

        public int? Take
        {
            get
            {
                return _take;
            }
        }

        public string OrderBy
        {
            get
            {
                return _orderBy;
            }
        }

        public string WhereClause
        {
            get
            {
                return _whereClause;
            }
        }

        public ExpressionTranslator()
        {
            _compatibleFunctionMap = Global.DefaultSqlFunctionMap;
            _fieldsConfiguration = new Dictionary<string, string>();
            _sqlParameters = new List<DatabaseParameter>();
            var constraint = RDapter.Global.GetSchemaConstraint<TObject>();
            var properties = typeof(TObject).GetProperties(constraint.ResolveBindingFlags);
            foreach (var property in properties)
            {
                var field = constraint.GetField(property.Name);
                var key = field.Name;//RDapter.Global.GetDefaultTypeMap<TObject>(property.Name);
                var value = field.SqlName;
                _fieldsConfiguration.Add(key, value);
            }
        }

        public (string Expression, IEnumerable<DatabaseParameter> Parameters) Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            _whereClause = this.sb.ToString();
            return (_whereClause, _sqlParameters);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            else if (m.Method.Name == "Take")
            {
                if (this.ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (this.ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (this.ParseOrderByExpression(m, "ASC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (this.ParseOrderByExpression(m, "DESC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Contains")
            {
                if (m.Method.DeclaringType == typeof(System.Linq.Enumerable))
                {
                    var fieldName = (m.Arguments[1] as MemberExpression).Member.Name;
                    var field = _fieldsConfiguration[fieldName];
                    var values = CompileExpression(m.Arguments[0]) as dynamic;
                    var paramArray = new string[(int)values.Length];
                    for (var idx = 0; idx < values.Length; idx++)
                    {
                        var paramName = $@"@cepr{idx}";
                        paramArray[idx] = paramName;
                        _sqlParameters.Add(new DatabaseParameter(paramName, values[idx]));
                    }
                    sb.Append($@"({field} IN ({string.Join(",", paramArray)}))");
                    return m;
                }
                else if (m.Method.DeclaringType.Name == "List`1")
                {
                    var fieldName = (m.Arguments[0] as MemberExpression).Member.Name;
                    var field = _fieldsConfiguration[fieldName];
                    var values = CompileExpression(m.Object) as dynamic;
                    var paramArray = new string[(int)values.Count];
                    for (var idx = 0; idx < values.Count; idx++)
                    {
                        var paramName = $@"@cepr{idx}";
                        paramArray[idx] = paramName;
                        _sqlParameters.Add(new DatabaseParameter(paramName, values[idx]));
                    }
                    sb.Append($@"({field} IN ({string.Join(",", paramArray)}))");
                    return m;
                }
                else if (m.Method.DeclaringType == typeof(string))
                {
                    var field = _fieldsConfiguration[((MemberExpression)m.Object).Member.Name];
                    var expression = CompileExpression(m.Arguments[0]);//m.Arguments[0].ToString().Replace("\"", "");
                    _sqlParameters.Add(new DatabaseParameter(field, expression));
                    sb.Append($@"({field} LIKE '%' + @{field} + '%')");
                    return m;
                }
            }
            else if (m.Method.Name == "StartsWith")
            {
                var field = _fieldsConfiguration[((MemberExpression)m.Object).Member.Name];
                var expression = CompileExpression(m.Arguments[0]);//.ToString().Replace("\"", "");
                _sqlParameters.Add(new DatabaseParameter(field, expression));
                sb.Append($@"({field} LIKE @{field} + '%')");
                return m;
            }
            else if (m.Method.Name == "EndsWith")
            {
                var field = _fieldsConfiguration[((MemberExpression)m.Object).Member.Name];
                var expression = CompileExpression(m.Arguments[0]);//.ToString().Replace("\"", "");
                _sqlParameters.Add(new DatabaseParameter(field, expression));
                sb.Append($@"({field} LIKE '%' + @{field})");
                return m;
            }
            else if (m.Method.Name == "IsNullOrEmpty" || m.Method.Name == "IsNullOrWhiteSpace")
            {
                var node = ((MemberExpression)m.Arguments[0]).Member.Name;
                var field = _fieldsConfiguration[node];
                sb.Append($"({field} IS NULL OR {field} = '')");
                return m;
            }
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;

                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                case ExpressionType.Add:
                    sb.Append(" + ");
                    break;

                case ExpressionType.Subtract:
                    sb.Append(" - ");
                    break;

                case ExpressionType.Divide:
                    sb.Append(" / ");
                    break;

                case ExpressionType.Multiply:
                    sb.Append(" * ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.DateTime:
                    case TypeCode.String:
                        _sqlParameters.Add(new DatabaseParameter(_previousVisitField, c.Value));
                        sb.Append($"@{_previousVisitField}");
                        //sb.Append("'");
                        //sb.Append(c.Value);
                        //sb.Append("'");
                        break;

                    //case TypeCode.DateTime:
                    //    sb.Append("'");
                    //    sb.Append(c.Value);
                    //    sb.Append("'");
                    //    break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null)
            {
                switch (m.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        _previousVisitField = _fieldsConfiguration[m.Member.Name];
                        sb.Append(_previousVisitField);
                        return m;

                    case ExpressionType.Constant:
                        var constantInvokedValue = CompileExpression(m);
                        if (_previousVisitField == null) _previousVisitField = m.Member.Name;
                        _sqlParameters.Add(new DatabaseParameter(_previousVisitField, constantInvokedValue));
                        sb.Append($"@{_previousVisitField}");
                        return m;
                    //need more research on this
                    case ExpressionType.MemberAccess:
                        var accessingProperty = m.Member.Name.ToLower();
                        switch (accessingProperty)
                        {
                            case "length":
                                var member = (m.Expression as MemberExpression).Member.Name;
                                var lengthFunction = _compatibleFunctionMap(SqlFunction.Length);
                                sb.Append($"{lengthFunction}({member})");
                                break;

                            default:
                                var value = CompileExpression(m);
                                _sqlParameters.Add(new DatabaseParameter(_previousVisitField, value));
                                sb.Append($"@{_previousVisitField}");
                                break;
                        }
                        return m;
                }
            }
            throw new NotSupportedException($"Expression contains unsupported statement ({m}).");
        }

        private bool IsQuoteNeeded(Type propertyType)
        {
            return
               propertyType == typeof(string) ||
               propertyType == typeof(char) ||
               propertyType == typeof(char?) ||
               propertyType == typeof(DateTime) ||
               propertyType == typeof(DateTime?) ||
               propertyType == typeof(Guid) ||
               propertyType == typeof(Guid?);
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
            LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;

            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            MemberExpression body = lambdaExpression.Body as MemberExpression;
            if (body != null)
            {
                if (string.IsNullOrEmpty(_orderBy))
                {
                    _orderBy = string.Format("{0} {1}", body.Member.Name, order);
                }
                else
                {
                    _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
                }

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _skip = size;
                return true;
            }

            return false;
        }

        private object CompileExpression(Expression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
    }
}
