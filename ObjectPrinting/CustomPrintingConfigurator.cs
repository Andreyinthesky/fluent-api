using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class CustomPrintingConfigurator<TOwner>
    {
        private readonly Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> printingPropertiesConfigs 
            = new Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>>();
        private readonly Dictionary<Type, IPropertyPrintingConfig<TOwner>> printingTypesConfigs
            = new Dictionary<Type, IPropertyPrintingConfig<TOwner>>();

        private static Expression<Func<object, string>> defaultPrintingMethod = obj => obj.ToString();

        private readonly Type[] numberTypes = new[]
        {
            typeof(long), typeof(int), typeof(double), typeof(float)
        };

        public IPropertyPrintingConfig<TOwner> AddConfigFor<TPropType>(PrintingConfig<TOwner> printingConfig)
        {
            var type = typeof(TPropType);

            if (!ContainsConfigFor(type))
            {
                printingTypesConfigs.Add(type, CreateNewConfigFor<TPropType>(printingConfig));
            }

            return printingTypesConfigs[type];
        }

        public IPropertyPrintingConfig<TOwner> AddConfigFor<TPropType>(PropertyInfo propertyInfo, PrintingConfig<TOwner> printingConfig)
        {
            if (!ContainsConfigFor(propertyInfo))
            {
                printingPropertiesConfigs.Add(propertyInfo, CreateNewConfigFor<TPropType>(printingConfig));
            }

            return printingPropertiesConfigs[propertyInfo];
        }

        private IPropertyPrintingConfig<TOwner> CreateNewConfigFor<TPropType>(PrintingConfig<TOwner> printingConfig)
        {
            PropertyPrintingConfig<TOwner, TPropType> propConf;
            var type = typeof(TPropType);

            if (type == typeof(string))
            {
                propConf = new StringPropertyConfig<TOwner, TPropType>(printingConfig);
            }
            else if (numberTypes.Contains(type))
            {
                propConf = new NumberPropertyConfig<TOwner, TPropType>(printingConfig);
            }
            else
            {
                propConf = new PropertyPrintingConfig<TOwner,TPropType>(printingConfig);
            }

            return propConf;
        }

        public LambdaExpression GetPrintingMethod(PropertyInfo property)
        {
            LambdaExpression printingMethod = null;
            if (printingTypesConfigs.ContainsKey(property.PropertyType))
            {
                printingMethod = printingTypesConfigs[property.PropertyType].PrintingMethod;
            }

            if (printingPropertiesConfigs.ContainsKey(property))
            {
                printingMethod = printingPropertiesConfigs[property].PrintingMethod ?? printingMethod;
            }

            return printingMethod ?? defaultPrintingMethod;
        }

        public CultureInfo GetCultureInfo(PropertyInfo property)
        {
            var cultureInfo = printingTypesConfigs.ContainsKey(property.PropertyType) 
                ? ((INumberPrintingConfig)printingTypesConfigs[property.PropertyType]).CultureInfo
                : null;
            cultureInfo = printingPropertiesConfigs.ContainsKey(property)
                ? ((INumberPrintingConfig) printingPropertiesConfigs[property]).CultureInfo ?? cultureInfo
                : cultureInfo;
            return cultureInfo;
        }

        public int? GetTrimLength(PropertyInfo property)
        {
            var trimLength = printingTypesConfigs.ContainsKey(property.PropertyType)
                ? ((StringPropertyConfig<TOwner, string>)printingTypesConfigs[property.PropertyType]).TrimLength
                : null;
            trimLength = printingPropertiesConfigs.ContainsKey(property)
                ? ((StringPropertyConfig<TOwner, string>) printingPropertiesConfigs[property]).TrimLength ?? trimLength
                : trimLength;

            return trimLength;
        }

        public bool ContainsConfigFor<TPropType>()
        {
            return ContainsConfigFor(typeof(TPropType));
        }

        public bool ContainsConfigFor(Type type)
        {
            return printingTypesConfigs.ContainsKey(type);
        }

        public bool ContainsConfigFor(PropertyInfo propertyInfo)
        {
            return printingPropertiesConfigs.ContainsKey(propertyInfo);
        }
    }
}