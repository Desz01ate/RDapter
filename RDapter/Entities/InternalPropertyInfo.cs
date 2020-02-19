using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using RDapter.DataBuilder.Helper;

namespace RDapter.Entities
{
    /// <summary>
    /// Proxy for PropertyInfo for customizable property for internal use only.
    /// </summary>
    internal class InternalPropertyInfo : PropertyInfo
    {
        private string _innerName = null;
        internal readonly PropertyInfo _basePropertyInfo;

        public InternalPropertyInfo(PropertyInfo property)
        {
            _basePropertyInfo = property;
        }

        public override PropertyAttributes Attributes => _basePropertyInfo.Attributes;

        public override bool CanRead => _basePropertyInfo.CanRead;

        public override bool CanWrite => _basePropertyInfo.CanWrite;

        public override Type PropertyType => _basePropertyInfo.PropertyType;

        public override Type DeclaringType => _basePropertyInfo.DeclaringType;
        public string ForeignKeyName { get; set; }

        public override string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_innerName))
                {
                    var constraint = Global.GetSchemaConstraint(DeclaringType);
                    _innerName = constraint.GetField(_basePropertyInfo.Name).SqlName;//_basePropertyInfo.FieldNameAttributeValidate();
                }
                return _innerName;
            }
        }

        public string OriginalName => _basePropertyInfo.Name;
        public override Type ReflectedType => _basePropertyInfo.ReflectedType;

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return _basePropertyInfo.GetAccessors(nonPublic);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _basePropertyInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _basePropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _basePropertyInfo.GetGetMethod(nonPublic);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return _basePropertyInfo.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _basePropertyInfo.GetSetMethod(nonPublic);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return _basePropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _basePropertyInfo.IsDefined(attributeType, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            _basePropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
        }
    }
}
