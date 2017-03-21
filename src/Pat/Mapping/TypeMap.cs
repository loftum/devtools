using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pat.Reflection;

namespace Pat.Mapping
{
    public class TypeMap
    {
        public Type SourceType { get; }
        public Type TargetType { get; }
        public List<PropertyMap> PropertyMaps { get; } = new List<PropertyMap>();

        public TypeMap(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
            var sourceProperties = sourceType.GetProperties().ToDictionary(p => p.Name, p => p);
            foreach (var targetProperty in targetType.GetProperties())
            {
                PropertyInfo sourceProperty;
                if (sourceProperties.TryGetValue(targetProperty.Name, out sourceProperty))
                {
                    var propertyMap = PropertyMap.For(sourceProperty, targetProperty);
                    if (propertyMap != null)
                    {
                        PropertyMaps.Add(propertyMap);
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{SourceType.GetFriendlyName()} -> {TargetType.GetFriendlyName()}";
        }
    }
}