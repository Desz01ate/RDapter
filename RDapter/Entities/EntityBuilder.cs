using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDapter.Entities
{
    public abstract class EntityBuilder
    {
        internal abstract string EntityName { get; }

        internal abstract IReadOnlyList<EntityMemberBuilder> Members { get; }
    }
    public class EntityBuilder<T> : EntityBuilder where T : new()
    {
        private string _entityName;

        internal override string EntityName => _entityName;

        private List<EntityMemberBuilder> _members { get; } = new List<EntityMemberBuilder>();

        internal override IReadOnlyList<EntityMemberBuilder> Members => _members;

        internal EntityMemberBuilder PrimaryKey { get; private set; }

        public EntityBuilder<T> HasEntityName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this._entityName = name;
            return this;
        }

        public EntityBuilder<T> HasKey(Expression<Func<T, object>> propertyBuilder)
        {
            if (Helpers.Expression.TryGetMemberName(propertyBuilder, out var name))
            {
                var member = _members.Single(x => x.MemberName == name);
                member.IsRequired();
                PrimaryKey = member;
            }
            return this;
        }
        public EntityMemberBuilder? Property(Expression<Func<T, object>> propertyBuilder)
        {
            EntityMemberBuilder? entityMember = default;
            if (Helpers.Expression.TryGetMemberName(propertyBuilder, out var name))
            {
                entityMember = new EntityMemberBuilder(name);
                _members.Add(entityMember);
            }
            return entityMember;
        }
    }
}
