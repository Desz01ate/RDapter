using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDapter.Helpers
{
    internal static class Expression
    {
        internal static bool TryGetMemberName<TType>(Expression<Func<TType, object>> expression, out string? memberName)
        {
            //**explicit cast to get exception throw!**
            if (expression.Body is UnaryExpression unaryExpression)
            {
                if (GetMemberExpression((MemberExpression)unaryExpression.Operand, out var v))
                {
                    memberName = v;
                    return true;
                }
            }
            if (GetMemberExpression((MemberExpression)expression.Body, out var v2))
            {
                memberName = v2;
                return true;
            }
            memberName = null;
            return false;
        }
        private static bool GetMemberExpression(MemberExpression expression, out string? memberName)
        {
            try
            {
                memberName = expression.Member.Name;
                return true;
            }
            catch
            {
                memberName = null;
                return false;
            }
        }
    }
}
