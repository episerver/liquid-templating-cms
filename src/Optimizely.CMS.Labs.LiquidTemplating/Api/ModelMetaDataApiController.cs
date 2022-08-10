using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Optimizely.CMS.Labs.LiquidTemplating.Api
{
    [Route("api/liquid/[controller]")]
    [ApiController]
    [HideRouteFromProduction]
    public class ModelMetaDataApiController : Controller
    {
        [HttpGet]
        [Route("Class")]
        public IActionResult ForClass(string name)
        {
            if (string.IsNullOrEmpty(name))
                return NotFound();
            
            var allTypes = GetFilteredAssemblies();

            var type = allTypes.FirstOrDefault(t => t.FullName == name);
            if (type == null)
                return NotFound();

            var modelInfo = new ModelInfo();
            modelInfo.Name = name;

            foreach (var pt in type.GetProperties())
            {
                modelInfo.Properties.Add(new PropertyInformation(pt, !PropertyInformation.IsPrimitive(pt.PropertyType.Name)));
            }

            // If class is a typed generic then we need to resolve both parts manually
            if (name.Contains("<"))
            {
                var parentName = name.Split('<')[0];
                var parent = allTypes.FirstOrDefault(x => x.FullName.StartsWith(parentName));
                var genericName = name.Split('<')[1].TrimEnd('>');
                var generic = allTypes.FirstOrDefault(x => x.FullName == genericName);

                if (parent != null && generic != null)
                {
                    modelInfo.Name = parent.Name;
                    foreach (var p in parent.GetProperties())
                    {
                        if (p.PropertyType.Name == "T")
                        {
                            var pi = new PropertyInformation() { Name = p.Name, TypeFullName = genericName, Type = genericName.Split('.').Last(), Properties = new List<PropertyInformation>() };
                            foreach (var gp in generic.GetProperties())
                            {
                                if (gp.PropertyType.IsPublic)
                                {
                                    pi.Properties.Add(new PropertyInformation(gp, !PropertyInformation.IsPrimitive(gp.PropertyType.Name)));
                                }
                            }
                            modelInfo.Properties.Add(pi);
                        }
                        else
                        {
                            modelInfo.Properties.Add(new PropertyInformation(p));
                        }
                    }
                }

                return Json(modelInfo);
            }

            return Json(modelInfo);
        }
        private static List<Type> GetFilteredAssemblies()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();

            var allTypes = new List<Type>();

            foreach (Assembly assembly in assems)
            {
                allTypes.AddRange(assembly.GetTypes());
            }

            return allTypes;
        }
    }

    public class ModelInfo
    {
        public ModelInfo()
        {
            Properties = new List<PropertyInformation>();
        }

        public string Name { get; set; }
        public IList<PropertyInformation> Properties { get; set; }
    }

    public class PropertyInformation
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeFullName { get; set; }
        public string CustomAttributes { get; set; }
        public IList<PropertyInformation> Properties { get; set; }
        public PropertyInformation() { }
        public PropertyInformation(PropertyInfo info, bool resolveChildren = true)
        {
            Name = info.Name;

            // Special case for nullable types
            if (info.PropertyType.IsGenericType && info.PropertyType.Name == "Nullable`1")
            {
                Type = info.PropertyType.GetGenericArguments().First().Name + "?";
                TypeFullName = "Nullable<" + info.PropertyType.GetGenericArguments().First().FullName + ">";
            }
            else if (info.PropertyType.IsGenericType && info.PropertyType.Name == "IList`1")
            {
                Type = "IList<" + info.PropertyType.GetGenericArguments().First().Name + ">";
                TypeFullName = "IList<" + info.PropertyType.GetGenericArguments().First().FullName + ">";
            }
            else
            {
                Type = info.PropertyType.Name;
                TypeFullName = info.PropertyType.FullName;
            }

            if (info.CustomAttributes != null)
            {
                foreach (var ca in info.CustomAttributes)
                {
                    CustomAttributes += ca.AttributeType.FullName + ", ";
                }
                if (CustomAttributes != null)
                {
                    CustomAttributes = CustomAttributes.TrimEnd(',', ' ');
                }
            }

            if (resolveChildren)
            {
                Properties = new List<PropertyInformation>();
                var childProps = info.PropertyType.GetProperties();
                foreach (var prop in childProps)
                {
                    if (IsPrimitive(prop.PropertyType.Name))
                    {
                        resolveChildren = false;
                    }

                    //Properties.Add(new PropertyInformation(prop, resolveChildren));
                }
            }
        }

        public static bool IsPrimitive(string typeName)
        {
            return "Object,String,Int32,Boolean,Decimal,PageReference,ContentReference,XhtmlString,PropertyDataCollection"
                    .Split(',').Contains(typeName);
        }

    }

}