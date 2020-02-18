using System;

namespace RDapter.Attributes
{
    /// <summary>
    /// Attribute for ignorance on insert or update statement
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreFieldAttribute : Attribute
    {
        internal bool IgnoreInsert { get; }
        internal bool IgnoreUpdate { get; }

        public IgnoreFieldAttribute(bool insert, bool update)
        {
            IgnoreInsert = insert;
            IgnoreUpdate = update;
        }
    }
}