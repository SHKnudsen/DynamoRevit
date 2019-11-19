using System.IO;

using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using RevitServices.Materials;
using System.Collections.Generic;

namespace RevitSystemTests
{
    [TestFixture]
    class ElementTests : RevitSystemTestBase
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
        public void CanDeleteElement()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Element\deleteWallFromDocument.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            // query count node to verify 1 item deleted as a result of the wall deletion. 
            Assert.AreEqual(1, GetPreviewValue("ccd8a5ba37fd4b1297def564392ccf54"));
         }

        /// <summary>
        /// Checks if Elements hosted elements can be retrived from Dynamo
        /// </summary>
        [Test]
        [TestModel(@".\Element\hostedElements.rvt")]
        public void CanGetHostedElements()
        {
            // Arrange
            // set-up to run dynamo script
            string samplePath = Path.Combine(workingDirectory, @".\Element\canSuccessfullyGetHostedElements.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            // Act
            // get output of Element.Names in dynamo script
            List<string> hostedElementNames = GetPreviewValue("d9a3fb06d30a4c088c582ae81ca4245f") as List<string>;
            List<string> expectedValues = new List<string>() { "600 x 3100", "600 x 3100", "600 x 3100", "Rectangular Straight Wall Opening" };

            // Assert
            // check if outcome is the same as the expected collection
            CollectionAssert.AreEqual(expectedValues, hostedElementNames);
        }

        [Test]
        [TestModel(@".\Element\elementJoin.rvt")]
        public void CanCheckIfTwoElementsAreJoined()
        {
            #region Arange
            string samplePath = Path.Combine(workingDirectory, @".\Element\canCheckIfTwoElementsAreJoined");
            string testPath = Path.GetFullPath(samplePath);
            #endregion

            #region Act
            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            // Get values of the two IsJoined nodes
            // first one should return false as it is checking two elements that are not joined
            // second one should return true as it test two joined elements
            bool? isJoinedFalse = GetPreviewValue("d18424a424aa476588f6f466675b7123") as bool?;
            bool? isJoinedTrue = GetPreviewValue("f93b0fb9baca4a6fa4d9818b4dffd713") as bool?;
            
            #endregion

            #region Assert

            Assert.AreEqual(true, isJoinedTrue);
            Assert.AreEqual(false, isJoinedFalse);
            #endregion
        }
    }
}
