using System;

namespace RDapter.Entities
{
    public sealed class EntityMemberBuilder
    {
        internal string MemberName { get; private set; }

        internal bool Nullable { get; private set; }

        internal bool IgnoreInsert { get; private set; }

        internal bool IgnoreUpdate { get; private set; }

        internal EntityMemberBuilder(string memberName)
        {
            this.MemberName = memberName;
        }

        public EntityMemberBuilder IsRequired(bool required = true)
        {
            this.Nullable = required;
            return this;
        }

        public EntityMemberBuilder IgnoreOnInsert(bool ignore = true)
        {
            this.IgnoreInsert = ignore;
            return this;
        }

        public EntityMemberBuilder IgnoreOnUpdate(bool ignore = true)
        {
            this.IgnoreUpdate = ignore;
            return this;
        }

        public EntityMemberBuilder HasName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.MemberName = name;
            return this;
        }
    }
}