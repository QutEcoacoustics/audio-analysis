using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Production
{
    using System.IO;
    using System.Reflection;

    using PowerArgs;

    internal class CustomUsageHook : UsageHook
    {
        public static Dictionary<Type, string> FriendlyTypes = new Dictionary<Type, string>()
                                                               {
                                                                   { typeof(int?), "int" },
                                                                   {
                                                                       typeof(double?),
                                                                       "double"
                                                                   },
                                                                   {
                                                                       typeof(DirectoryInfo),
                                                                       "directory"
                                                                   },
                                                                   {
                                                                       typeof(FileInfo),
                                                                       "file"
                                                                   },
                                                               };


        public override void BeforeGenerateUsage(ArgumentUsageInfo info)
        {
            var propertyType = info.Property.PropertyType;
            if (FriendlyTypes.ContainsKey(propertyType))
            {
                info.Type = FriendlyTypes[propertyType];
            }
        }
    }

    public class CustomDescription : ArgDescription
    {
        public override string GetDescription(object propertyInfoOrClassType)
        {
            if (propertyInfoOrClassType == null)
            {
                return base.Description;
            }

            MethodInfo method = null;

            if (propertyInfoOrClassType is PropertyInfo)
            {
                method = ((PropertyInfo)propertyInfoOrClassType).PropertyType.GetMethod("Description", BindingFlags.Public | BindingFlags.Static);   
            }

            if (propertyInfoOrClassType is Type)
            {
                method = ((Type)propertyInfoOrClassType).GetMethod("Description", BindingFlags.Public | BindingFlags.Static);
            }

            if (method == null || method.ReturnType != (typeof(string)) || method.GetParameters().Length > 0)
            {
                string name;
                if (method == null)
                {
                    name = propertyInfoOrClassType as PropertyInfo != null
                        ? ((PropertyInfo)propertyInfoOrClassType).DeclaringType.FullName
                        : ((Type)propertyInfoOrClassType).FullName;
                }
                else
                {
                    name = method.DeclaringType.FullName;
                }
                throw new ArgException(name + ": You cannot apply the CustomDetailedDescriptionAttribute to a class that does not have a public, callable, static, 'Description' method! This method must take no parameters and return a string!");
            }

            // now dynamically invoke - this is madness
            string result = (string)method.Invoke(null, null);

            return result;
        }
    }

    public class CustomDetailedDescription : ArgDetailedDescription
    {
        public override string GetDetailedDescription(Type propertyType)
        {
            if (propertyType == null)
            {
                return base.DetailedDescription;
            }

            var method = propertyType.GetMethod("AdditionalNotes", BindingFlags.Public | BindingFlags.Static);

            if (method == null || method.ReturnType != (typeof(string)) || method.GetParameters().Length > 0)
            {
                throw new ArgException(propertyType.FullName + ": You cannot apply the CustomDetailedDescriptionAttribute to a class that does not have a public, callable, static, 'AdditionalNotes' method! This method must take no parameters and return a string!");
            }

            // now dynamically invoke - this is madness
            string result = (string)method.Invoke(null, null);

            return result;
        }
    }
}
