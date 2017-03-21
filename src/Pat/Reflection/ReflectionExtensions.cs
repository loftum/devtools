using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pat.Reflection
{
    public static class ReflectionExtensions
    {
        private static readonly IDictionary<Type, string> ValueNames = new Dictionary<Type, string>
        {
            {typeof(int), "int"},
            {typeof(short), "short"},
            {typeof(byte), "byte"},
            {typeof(bool), "bool"},
            {typeof(long), "long"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(string), "string"}
        };

        public static string GetFriendlyName(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (ValueNames.ContainsKey(type))
            {
                return ValueNames[type];
            }
            if (type.IsGenericType)
            {
                return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName))}>";
            }
            return type.Name;
        }

        public static bool HasSetter(this PropertyInfo property)
        {
            return property.GetGetMethod() != null;
        }

        public static bool HasGetter(this PropertyInfo property)
        {
            return property.GetSetMethod() != null;
        }

        public static string GetFullName(this PropertyInfo property)
        {
            var type = property.ReflectedType?.Name ?? property.DeclaringType?.Name ?? "(unknown)";
            return $"{property.PropertyType.GetFriendlyName()} {type}.{property.Name}";
        }

        public static TypeCategory GetCategory(this Type type)
        {
            if (type == typeof(string) || type.IsValueType)
            {
                return TypeCategory.Scalar;
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                return TypeCategory.Collection;
            }
            return TypeCategory.Complex;
        }
    }

    public enum TypeCategory
    {
        Scalar,
        Complex,
        Collection
    }
}