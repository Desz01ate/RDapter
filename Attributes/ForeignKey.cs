using RDapter.DataBuilder.Helper;
using RDapter.Entities;
using System;

namespace RDapter.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ForeignKeyAttribute : Attribute
    {
        internal InternalPropertyInfo ReferenceKeyProperty { get; }

        public ForeignKeyAttribute(Type referenceTable)
        {
            var primaryKeyOfReferenceTable = AttributeValidator.PrimaryKeyAttributeValidate(referenceTable);
            ReferenceKeyProperty = primaryKeyOfReferenceTable;
        }
    }
}