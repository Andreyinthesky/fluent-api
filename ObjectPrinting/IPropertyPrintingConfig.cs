using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        PropertyInfo PropertyInfo { get; }
        Type PropertyType { get; }
        LambdaExpression PrintingMethod { get; }
    }
}