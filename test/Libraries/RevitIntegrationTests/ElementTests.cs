﻿using System.IO;

using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using RevitServices.Materials;
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

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetPinnedStatus()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Element\canGetPinnedStatus.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();
            
            // query AllFalse node to check that the output of IsPinned is false for both elements in test model
            Assert.AreEqual(true, GetPreviewValue("d9811fadc1964cd0b58c440e227ce9ba"));
        }

        /// <summary>
        /// Checks that an Element's pinned status can be set from Dynamo.
        /// </summary>
        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSetPinnedStatus()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Element\setElementsPinStatus.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();
            //check Select Model Element
            var selectElement = "f49d6941-4497-43c3-9a52-fe4e5424e4e7-0002cf70;";
            var selectElementValue = GetPreviewValue(selectElement) as Element;
            Assert.IsNotNull(selectElementValue);

            bool originalPinnedStatus = selectElementValue.IsPinned;
            Assert.IsNotNull(originalPinnedStatus);
            Assert.AreEqual(false, originalPinnedStatus);

            //now flip the switch for setting the pinned status to true
            var boolNode = ViewModel.Model.CurrentWorkspace.Nodes.Where(x => x is CoreNodeModels.Input.BoolSelector).First();
            Assert.IsNotNull(boolNode);
            bool boolNodeValue = true;
            ((CoreNodeModels.Input.BasicInteractive<bool>)boolNode).Value = boolNodeValue;

            RunCurrentModel();
            bool newPinnedStatus = selectElementValue.IsPinned;
            Assert.AreNotEqual(originalPinnedStatus, newPinnedStatus);
            Assert.IsNotNull(newPinnedStatus);
            Assert.AreEqual(boolNodeValue, newPinnedStatus);
            
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
