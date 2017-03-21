using System;
using System.Collections.Generic;

namespace Pat.Mapping
{
    public class Mapper
    {
        public Dictionary<MappingKey, TypeMap> TypeMaps { get; }

        public Mapper()
        {
            TypeMaps = new Dictionary<MappingKey, TypeMap>();
        }

        public void Map(object source, object dest)
        {
            if (source == null || dest == null)
            {
                return;
            }
            var map = GetOrCreate(source.GetType(), dest.GetType());
            foreach (var propertyMap in map.PropertyMaps)
            {
                propertyMap.Map(source, dest);
            }
        }

        private TypeMap GetOrCreate(Type sourceType, Type targetType)
        {
            var key = new MappingKey(sourceType, targetType);
            if (!TypeMaps.ContainsKey(key))
            {
                TypeMaps[key] = new TypeMap(sourceType, targetType);
            }
            return TypeMaps[key];
        }
    }
}