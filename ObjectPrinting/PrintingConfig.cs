using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ExcludingConfigurator excludingConfigurator = new ExcludingConfigurator();

        private readonly CustomPrintingConfigurator<TOwner> customPrintingConfigurator
            = new CustomPrintingConfigurator<TOwner>();

        private readonly Type[] numberTypes = new[]
        {
            typeof(long), typeof(int), typeof(double), typeof(float)
        };

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private HashSet<object> visitedObjects = new HashSet<object>();

        private static Func<IEnumerable, string> listPrintingMethod = (e) =>
        {
            var builder = new StringBuilder("[");
            var enumerator = e.GetEnumerator();

            if (enumerator.MoveNext())
            {
                builder.Append(enumerator.Current);
            }

            while (enumerator.MoveNext())
            {
                builder
                    .Append(", ")
                    .Append(enumerator.Current);
            }

            return builder
                .Append("]")
                .ToString();
        };

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingConfigurator.ExcludeType<TPropType>();
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression) propSelector.Body).Member;
            excludingConfigurator.ExcludeProperty(propertyInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return 
                (PropertyPrintingConfig<TOwner, TPropType>)customPrintingConfigurator
                    .AddConfigFor<TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression) propSelector.Body).Member;

            return (PropertyPrintingConfig<TOwner, TPropType>)
                customPrintingConfigurator
                    .AddConfigFor<TPropType>(propertyInfo, this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return PrintLine("null");

            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            if (visitedObjects.Contains(obj))
                return sb.ToString();
            visitedObjects.Add(obj);

            if (obj is IEnumerable enumerable)
                return listPrintingMethod(enumerable);

            foreach (var propertyInfo in excludingConfigurator.Filter(type.GetProperties()))
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          (
                              finalTypes.Contains(propertyInfo.PropertyType)
                                  ? PrintLine(PrintProperty(obj, propertyInfo))
                                  : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)
                          ));
            }
            return sb.ToString();
        }

        private string PrintProperty(object obj, PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetValue(obj) == null)
                return "null";

            var printingMethod = customPrintingConfigurator.GetPrintingMethod(propertyInfo);
            var printingResult = printingMethod.Compile().DynamicInvoke(propertyInfo.GetValue(obj));

            if (propertyInfo.PropertyType == typeof(string))
            {
                var resultString = printingResult.ToString();
                var trimLength = customPrintingConfigurator.GetTrimLength(propertyInfo);
                return resultString.Substring(0, Math.Min(resultString.Length, trimLength ?? int.MaxValue));
            }

            if (numberTypes.Contains(propertyInfo.PropertyType))
            {
                var cultureInfo = customPrintingConfigurator.GetCultureInfo(propertyInfo);
                return cultureInfo != null ? printingResult.ToString().ToString(cultureInfo) : printingResult.ToString();
            }

            return printingResult.ToString();
        }

        private string PrintLine(string value)
        {
            return value + Environment.NewLine;
        }
    }
}