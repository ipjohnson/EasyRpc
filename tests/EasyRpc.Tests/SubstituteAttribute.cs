using System;
using System.Reflection;
using SimpleFixture;
using SimpleFixture.Attributes;
using SimpleFixture.NSubstitute;

namespace EasyRpc.Tests
{
    public class SubstituteAttribute : FixtureInitializationAttribute
    {
        private readonly Type _type;

        public SubstituteAttribute(Type type)
        {
            _type = type;
        }

        public bool Singleton { get; set; }

        /// <summary>Initialize fixture</summary>
        /// <param name="fixture">fixture</param>
        public override void Initialize(Fixture fixture)
        {
            var closedMethod = _substituteType.MakeGenericMethod(_type);

            closedMethod.Invoke(this, new object[] { fixture });

            foreach (var implementedInterface in _type.GetInterfaces())
            {
                closedMethod = _substituteInterface.MakeGenericMethod(_type, implementedInterface);

                closedMethod.Invoke(this, new object[] { fixture });
            }
        }

        private void SubstituteType<T>(Fixture fixture) where T : class
        {
            fixture.Substitute<T>(singleton: Singleton);
        }

        private void SubstituteInterface<T, TInterface>(Fixture fixture) where T : class, TInterface
        {
            fixture.ExportAs<T, TInterface>();
        }

        private static readonly MethodInfo _substituteType = typeof(SubstituteAttribute).GetMethod("SubstituteType", BindingFlags.NonPublic);
        private static readonly MethodInfo _substituteInterface = typeof(SubstituteAttribute).GetMethod("SubstituteInterface", BindingFlags.NonPublic);
    }
}
