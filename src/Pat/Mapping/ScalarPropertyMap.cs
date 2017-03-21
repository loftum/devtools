using System.Reflection;

namespace Pat.Mapping
{
    public class ScalarPropertyMap : PropertyMap
    {
        public ScalarPropertyMap(PropertyInfo sourceProperty, PropertyInfo targetProperty) : base(sourceProperty, targetProperty)
        {
        }

        public override void Map(object source, object target)
        {
            if (source == null || target == null)
            {
                return;
            }
            var value = SourceProperty.GetValue(source);
            TargetProperty.SetValue(target, value);
        }
    }
}