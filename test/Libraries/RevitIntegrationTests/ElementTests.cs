using System.IO;

using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using RevitServices.Materials;
using System.Collections.Generic;
using Revit.Elements;

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
        [TestModel(@".\Element\elementJoin.rvt")]
        public void CanGetJoinedElementsFromElement()
        {
            // Arrange - set-up to run dynamo script
            string samplePath = Path.Combine(workingDirectory, @".\Element\canGetJoinedElementsFromElement.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            // Act - get output of Element.GetJoinedElements in dynamo script
            List<Element> joinedElements = GetPreviewValue("bae6e489b6d34519996aa4f9c9ad8e67") as List<Element>;
            Assert.IsNotNull(joinedElements);

            List<int> joinedElementIds = new List<int>();
            for (int i = 0; i < joinedElementIds.Count; i++)
            {
                joinedElementIds.Add(joinedElements[i].Id);
            }
            Assert.IsNotNull(joinedElementIds);

            List<int> expectedElementIds = new List<int>() { 184176,208422 };

            // Assert - check if outcome element ids are the same as the expected element ids
            CollectionAssert.AreEqual(expectedElementIds, joinedElementIds);
        }

        [Test]
        [TestModel(@".\Element\elementPinned.rvt")]
        public void CanGetPinnedStatus()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Element\canGetPinnedStatus.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();
            
            // query AllFalse node to check that the output of IsPinned is false for both elements in test model
            Assert.AreEqual(true, GetPreviewValue("d9811fadc1964cd0b58c440e227ce9ba"));
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
