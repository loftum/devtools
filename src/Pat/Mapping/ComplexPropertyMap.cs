using System.Collections.Generic;
using System.Reflection;

namespace Pat.Mapping
{
    public class ComplexPropertyMap : PropertyMap
    {
        private readonly IList<PropertyMap> _propertyMaps;
        public ComplexPropertyMap(PropertyInfo sourceProperty, PropertyInfo targetProperty) : base(sourceProperty, targetProperty)
        {
            _propertyMaps = new TypeMap(sourceProperty.PropertyType, targetProperty.PropertyType).PropertyMaps;
        }

        public override void Map(object source, object target)
        {
            if (source == null || target == null)
            {
                return;
            }
            var sourcePropertyValue = SourceProperty.GetValue(source);
            var targetPropertyValue = TargetProperty.GetValue(target);

            foreach (var propertyMap in _propertyMaps)
            {
                propertyMap.Map(sourcePropertyValue, targetPropertyValue);
            }
        }
    }
}