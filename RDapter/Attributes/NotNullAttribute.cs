using System;

namespace RDapter.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotNullAttribute : Attribute
    {
    }
}