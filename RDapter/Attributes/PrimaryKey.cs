using System;

namespace RDapter.Attributes
{
    /// <summary>
    /// Attribute which specified which property is a primary key
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Whether this primary key is auto-increment or not.
        /// </summary>
        public readonly bool AutoIncrement;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="autoIncrement"></param>
        public PrimaryKeyAttribute(bool autoIncrement = false)
        {
            AutoIncrement = autoIncrement;
        }
    }
}