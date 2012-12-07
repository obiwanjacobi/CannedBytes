using System.ComponentModel.Composition;

namespace CannedBytes.Media.IO.UnitTests.Stubs
{
    [Export]
    class ComposableTestClass : ITestInterface
    {
        public int TestProperty { get; set; }

        public void TestMe()
        {
            throw new System.NotImplementedException();
        }
    }
}