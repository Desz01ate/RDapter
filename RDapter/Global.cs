using RDapter.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RDapter
{
    /// <summary>
    /// Global configuration for schema mapping.
    /// </summary>
    public static partial class Global
    {
        private static Dictionary<Type, EntityBuilder> defaultMapConstraint = new Dictionary<Type, EntityBuilder>();
        public static void SetBuilderFor<T>(Action<EntityBuilder<T>> action) where T : new()
        {
            var entityBuilder = new EntityBuilder<T>();
            action(entityBuilder);
            entityBuilder.Validate();
            defaultMapConstraint[typeof(T)] = entityBuilder;
        }
        public static EntityBuilder GetSchemaConstraint<T>()
        {
            var type = typeof(T);
            return GetSchemaConstraint(type);
        }
        public static EntityBuilder GetSchemaConstraint(Type type)
        {
            if (defaultMapConstraint.TryGetValue(type, out var v))
            {
                return v;
            }
            throw new NotImplementedException();
            //SetSchemaConstraint(type, (constraint) =>
            // {
            //     constraint.SetBindingFlags(BindingFlags.Public | BindingFlags.Instance);
            //     foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //     {
            //         constraint.SetField(property.Name, property.Name, false, false);
            //     }
            // });
            //return GetSchemaConstraint(type);
        }
    }
}
