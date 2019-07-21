using NUnit.Framework;

namespace ReflectSettingsTests.Factory
{
    [TestFixture]
    internal class EditableConfigFactoryTests : EditableConfigFactoryTestBase
    {
#if !DEBUG
        [Test]
        public void CyclicTypesResultInDummyEditable()
        {
            var result = Produce<ClassWithCyclicDefinition>();
            var editable = result.First();

            Assert.True(editable is EditableDummy);
        }
#endif
    }
}