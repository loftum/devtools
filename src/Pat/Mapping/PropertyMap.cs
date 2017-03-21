using System;
using System.Reflection;
using Pat.Reflection;

namespace Pat.Mapping
{
    public abstract class PropertyMap
    {
        public PropertyInfo SourceProperty { get; }
        public PropertyInfo TargetProperty { get; }

        protected PropertyMap(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            SourceProperty = sourceProperty;
            TargetProperty = targetProperty;
        }

        public abstract void Map(object source, object target);

        public static PropertyMap For(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            switch (targetProperty.PropertyType.GetCategory())
            {
                case TypeCategory.Scalar:
                    return sourceProperty.PropertyType == targetProperty.PropertyType ? new ScalarPropertyMap(sourceProperty, targetProperty) : null;
                case TypeCategory.Complex:
                    return sourceProperty.PropertyType == targetProperty.PropertyType ? new ComplexPropertyMap(sourceProperty, targetProperty) : null;
                case TypeCategory.Collection:
                    return new CollectionPropertyMap(sourceProperty, targetProperty);
                default:
                    throw new InvalidOperationException($"Cannot create map from {sourceProperty.GetFullName()} to {targetProperty.GetFullName()}");
            }
        }

        public override string ToString()
        {
            return $"{SourceProperty.GetFullName()} -> {TargetProperty.GetFullName()}";
        }
    }
}