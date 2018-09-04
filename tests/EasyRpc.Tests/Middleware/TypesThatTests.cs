using System;
using System.Reflection;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Classes;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class TypesThatTests
    {
        #region Attribute

        public class SomeAttribute : Attribute { }

        [Some]
        public class AttributedClass { }

        [Fact]
        public void TypesThatHaveAttribute()
        {
            var test = (Func<Type, bool>)TypesThat.HaveAttribute(typeof(SomeAttribute));

            Assert.True(test(typeof(AttributedClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatHaveAttributeGeneric()
        {
            var test = (Func<Type, bool>)TypesThat.HaveAttribute<SomeAttribute>();

            Assert.True(test(typeof(AttributedClass)));
            Assert.False(test(typeof(IntMathService)));
        }


        [Fact]
        public void TypesThatHaveAttributeFilter()
        {
            var test = (Func<Type, bool>)TypesThat.HaveAttribute(t => t == typeof(SomeAttribute));

            Assert.True(test(typeof(AttributedClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        #endregion

        #region Property

        public class PropertyClass
        {
            public int IntValue { get; set; }
        }

        [Fact]
        public void TypesThatHaveProperty()
        {
            var test = (Func<Type, bool>) TypesThat.HaveProperty("IntValue");

            Assert.True(test(typeof(PropertyClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        public class OtherPropertyClass
        {
            public int OtherValue { get; set; }
        }
        
        [Fact]
        public void TypesThatHavePropertyTyped()
        {
            var test = (Func<Type, bool>)TypesThat.HaveProperty(typeof(int));

            Assert.True(test(typeof(PropertyClass)));
            Assert.True(test(typeof(OtherPropertyClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatHavePropertyTypedFiltered()
        {
            var test = (Func<Type, bool>)TypesThat.HaveProperty(typeof(int), "IntValue");

            Assert.True(test(typeof(PropertyClass)));
            Assert.False(test(typeof(OtherPropertyClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatHavePropertyGenericTyped()
        {
            var test = (Func<Type, bool>)TypesThat.HaveProperty<int>();

            Assert.True(test(typeof(PropertyClass)));
            Assert.True(test(typeof(OtherPropertyClass)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatHavePropertyTypedGenericFiltered()
        {
            var test = (Func<Type, bool>)TypesThat.HaveProperty<int>("IntValue");

            Assert.True(test(typeof(PropertyClass)));
            Assert.False(test(typeof(OtherPropertyClass)));
            Assert.False(test(typeof(IntMathService)));
        }
        #endregion

        #region Name

        [Fact]
        public void TypesThatStartWith()
        {
            var test = (Func<Type, bool>)TypesThat.StartWith("Types");

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatEndWith()
        {
            var test = (Func<Type, bool>)TypesThat.EndWith("Tests");

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatContain()
        {
            var test = (Func<Type, bool>)TypesThat.Contains("That");

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }

        #endregion

        #region Namespace

        [Fact]
        public void TypesThatAreInTheSameNamespace()
        {
            var test = (Func<Type, bool>)TypesThat.AreInTheSameNamespace("EasyRpc.Tests.Middleware");

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatAreInTheSameNamespaceAs()
        {
            var test = (Func<Type, bool>)TypesThat.AreInTheSameNamespaceAs(typeof(TypesThatTests));

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }

        [Fact]
        public void TypesThatAreInTheSameNamespaceAsGeneric()
        {
            var test = (Func<Type, bool>)TypesThat.AreInTheSameNamespaceAs<TypesThatTests>();

            Assert.True(test(typeof(TypesThatTests)));
            Assert.False(test(typeof(IntMathService)));
        }
        #endregion

        #region Based On Methods

        public class BaseClass { }

        public class InheritClass : BaseClass { }

        [Fact]
        public void TypesThatAreBasedOn()
        {
            var test = (Func<Type, bool>)TypesThat.AreBasedOn(typeof(BaseClass));

            Assert.True(test(typeof(InheritClass)));
            Assert.False(test(typeof(TypesThatTests)));
        }

        [Fact]
        public void TypesThatAreBasedOnGeneric()
        {
            var test = (Func<Type, bool>)TypesThat.AreBasedOn<BaseClass>();

            Assert.True(test(typeof(InheritClass)));
            Assert.False(test(typeof(TypesThatTests)));
        }

        [Fact]
        public void TypesThatAreBasedOnFilter()
        {
            var test = (Func<Type, bool>)TypesThat.AreBasedOn(type => type.GetTypeInfo().BaseType == typeof(BaseClass));

            Assert.True(test(typeof(InheritClass)));
            Assert.False(test(typeof(TypesThatTests)));
        }

        #endregion

        #region Match Method
        [Fact]
        public void TypesThatMatch()
        {
            var test = (Func<Type, bool>)TypesThat.Match(t => t.Name.Contains("That"));

            Assert.False(test(typeof(PrivateClass)));
            Assert.True(test(typeof(TypesThatTests)));
        }
        #endregion

        #region Public Methods

        private class PrivateClass
        {

        }

        [Fact]
        public void TypesThatArePublic()
        {
            var test = (Func<Type, bool>)TypesThat.ArePublic();

            Assert.False(test(typeof(PrivateClass)));
            Assert.True(test(typeof(TypesThatTests)));

        }

        [Fact]
        public void TypesThatAreNotPublic()
        {
            var test = (Func<Type, bool>)TypesThat.AreNotPublic();

            Assert.False(test(typeof(TypesThatTests)));
            Assert.True(test(typeof(PrivateClass)));

        }

        #endregion

        #region Generic Methods

        public class OpenGeneric<T>
        {
            public OpenGeneric(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }

        [Fact]
        public void TypesThatAreConstructedGeneric()
        {
            var test = (Func<Type, bool>)TypesThat.AreConstructedGeneric();

            Assert.False(test(typeof(OpenGeneric<>)));
            Assert.True(test(typeof(OpenGeneric<int>)));
        }


        [Fact]
        public void TypesThatAreOpenGeneric()
        {
            var test = (Func<Type, bool>)TypesThat.AreOpenGeneric();

            Assert.True(test(typeof(OpenGeneric<>)));
            Assert.False(test(typeof(OpenGeneric<int>)));
        }

        #endregion
    }
}
