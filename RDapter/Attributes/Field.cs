using System;

namespace RDapter.Attributes
{
    /// <summary>
    /// Attribute for field name customization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FieldAttribute : Attribute
    {
        internal string FieldName { get; }

        public FieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}