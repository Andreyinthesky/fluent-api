using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectTests
    {
        [Test]
        public void PrintToString_WhenNoSettings_ReturnResultUsingDefaultHandlers()
        {
            var person = new Person {Name = "Alex", Age = 19};

            person.PrintToString().Should().Be
            (
                $"{nameof(Person)}\r\n" +
                $"\t{nameof(Person.Id)} = {person.Id.GetType().Name}\r\n" +
                $"\t{nameof(Person.Name)} = {person.Name}\r\n" +
                $"\t{nameof(Person.Height)} = {person.Height}\r\n" +
                $"\t{nameof(Person.Age)} = {person.Age}\r\n"
            );
        }
   
        [TestCase(new int[]{})]
        [TestCase(new int[]{1, 2, 3})]
        public void PrintToString_WhenObjectIsIntArray(int[] obj)
        {
            obj.PrintToString().Should().Be($"[{string.Join(", ", obj)}]");
        }

        [Test]
        public void PrintToString_WhenObjectIsList()
        {
            var obj = new List<int>() {1, 2, 3};

            obj.PrintToString().Should().Be($"[{string.Join(", ", obj)}]");
        }

        [Test]
        [Timeout(1000)]
        public void PrintToString_WhenObjectHasCycleReference_DoesNotThrowStackOverflowException()
        {
            var obj1 = new Source {DoubleProp1 = 3.3};
            var obj2 = new Source {DoubleProp1 = 4.5, InnerSource = obj1};
            obj1.InnerSource = obj2;

            Action act = () => obj1.PrintToString();

            act.Should().NotThrow<StackOverflowException>();
        }
    }
}