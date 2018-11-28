using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void DemoPerson()
        {
            var person = new Person { Name = null, Age = 19, Height = 128.3};
            var str = "word";

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(num => "I am string")
                //3. Для числовых типов указать культуру (int long double float)
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(age => str + age.ToString() + age.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(10)
                //6. Исключить из сериализации конкретное свойство
                .Excluding(p => p.Age);
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            //8. ...с конфигурированием
            string s3 = person.PrintToString(p => p.Excluding<int>());
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void DemoList()
        {
            var list = new List<int>() {1, 2, 3, 4};
            var str = list.PrintToString();
            Console.WriteLine(str);
        }
    }
}