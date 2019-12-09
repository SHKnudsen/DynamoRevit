using System.IO;

using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using RevitServices.Materials;
namespace RevitSystemTests
{
    [TestFixture]
    class ElementTypeTests : RevitSystemTestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            //setup the material manager just for tests
            var mgr = MaterialsManager.Instance;
            mgr.InitializeForActiveDocumentOnIdle();
        }
        
        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetElementTypeByName()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\Element\canGetElementTypeByName.dyn");
            string testPath = Path.GetFullPath(samplePath);
            int expectedId = 60411;

            // Act
            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            // Assert
            Assert.AreEqual(expectedId, GetPreviewValue("880295ffe9b24d21b20e1ac1a63c2bb8"));
        }
    }
}
