using System.Collections;
using System.Reflection;

namespace Pat.Mapping
{
    public class CollectionPropertyMap : PropertyMap
    {
        public CollectionPropertyMap(PropertyInfo sourceProperty, PropertyInfo targetProperty) : base(sourceProperty, targetProperty)
        {
        }

        public override void Map(object source, object target)
        {
            if (source == null || target == null)
            {
                return;
            }
            var sourceCollection = SourceProperty.GetValue(source) as IList;
            var targetCollection = TargetProperty.GetValue(target) as IList;
            if (sourceCollection == null || targetCollection == null)
            {
                return;
            }
            targetCollection.Clear();
            foreach (var item in sourceCollection)
            {
                targetCollection.Add(item);
            }
        }
    }
}