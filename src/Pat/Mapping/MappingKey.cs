using System;

namespace Pat.Mapping
{
    public class MappingKey
    {
        public Type SourceType { get; }
        public Type TargetType { get; }

        public MappingKey(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        protected bool Equals(MappingKey other)
        {
            return SourceType == other.SourceType && TargetType == other.TargetType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MappingKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceType != null ? SourceType.GetHashCode() : 0)*397) ^ (TargetType != null ? TargetType.GetHashCode() : 0);
            }
        }
    }
}