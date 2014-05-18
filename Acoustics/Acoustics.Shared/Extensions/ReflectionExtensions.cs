namespace System
{
    using System;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        internal static bool HasAttr<T>(this MemberInfo info)
        {
            return info.GetCustomAttributes(typeof(T), true).Length > 0;
        }


        internal static T Attr<T>(this MemberInfo info)
        {
            if (info.HasAttr<T>())
            {
                return (T)info.GetCustomAttributes(typeof(T), true)[0];
            }
            else
            {
                return default(T);
            }
        }

        public static bool IsInstanceOfGenericType(this Type genericType, object instance, out Type matchedType)
        {
            Type type = instance.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                {
                    matchedType = type;
                    return true;
                }
                type = type.BaseType;
            }

            matchedType = null;
            return false;
        }
    }
}
