﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class DocumentTests : RevitSystemTestBase
    {
        [Test]
        [TestModel(@"./empty.rfa")]
        public void InitialUIDocumentIsNotNull()
        {
            Assert.IsNotNull(DocumentManager.Instance.CurrentUIDocument);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void OpeningNewDocumentDoesNotSwitchUIDocument()
        {
            // a reference to the initial document
            var initialDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;

            var newDoc = OpenAndActivateNewModel(emptyModelPath1);

            // Assert that the active UI document is
            // still the initial document
            Assert.AreEqual(DocumentManager.Instance.CurrentUIDocument.Document.PathName, initialDoc.Document.PathName);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void SwitchingViewAwayFromCachedDocumentDisablesRun()
        {
            // empty.rfa will be open at test start
            // swap documents and ensure that 
            var initialDoc = DocumentManager.Instance.CurrentUIDocument;
            var newDoc = OpenAndActivateNewModel(emptyModelPath1);

            Assert.False(ViewModel.HomeSpace.RunSettings.RunEnabled);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void RunIsDisabledWhenOpeningADocumentinPerspectiveView()
        {
            // Swap to a document that only has one open perspective view
            SwapCurrentModel(Path.Combine(workingDirectory, "model_with_box.rvt"));

            Assert.False(ViewModel.HomeSpace.RunSettings.RunEnabled);

            // Then you need to swap back because the journal's ID_FLUSH_UNDO
            // is disabled in perspective as well

            SwapCurrentModel(emptyModelPath);
        }

        [Test, Ignore]
        [TestModel(@"./empty.rfa")]
        public void AttachesToNewDocumentWhenAllDocsWereClosed()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }

        [Test, Ignore]
        [TestModel(@"./empty.rfa")]
        public void WhenActiveDocumentResetIsRequiredVisualizationsAreCleared()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }

        [Test]
        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSaveFamiliesInCurrentDocument()
        {
            // Arange
            string samplePath = Path.Combine(workingDirectory, @".\Document\canSaveFamiliesInCurrentDocument.dyn");
            string testPath = Path.GetFullPath(samplePath);

            int expectedElementId = 137650;
            string expectedSavedFamilyFileName = "Rectangular Column.rfa";
            int expectedSavedFileCount = 1;

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();
            
            var resultElementId = GetPreviewValue("6c63a2a458db4e1da7eba8bcc2b05ae4");
            string savedFamilyDirectoryPath = GetPreviewValue("0af5cc47acab47369c479f3a4b198306").ToString();

            List<string> files = Directory.GetFiles(savedFamilyDirectoryPath).Select(x => Path.GetFileName(x)).ToList();
            int resultSavedFileCount = files.Count;
            string savedFamilyName = files.FirstOrDefault();

            // Assert
            Assert.AreEqual(expectedElementId, resultElementId);
            Assert.AreEqual(expectedSavedFileCount, resultSavedFileCount);
            Assert.AreEqual(savedFamilyName, expectedSavedFamilyFileName);

            // Clean up
            Directory.Delete(savedFamilyDirectoryPath, true);
        }

        [Test]
        [TestModel(@".\Document\BIM360\4481adfb-0f03-4e58-9f49-8bd37dde9e0e.rvt")]
        public void CanGetWorksharingModelPathOnCloudModel()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\Document\canGetWorksharingModelPath.dyn");
            string testPath = Path.GetFullPath(samplePath);
            string expectedWorksharingFilePath = @"BIM 360://Node test/BIM360_model.rvt";
            bool expectedIsCloudPathResult = true;

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();
            string resultWorksharingPath = GetPreviewValue("3f5e9a8cb7344c52a3c4937455ee68b1") as string;
            var resultIsCloudPath = GetPreviewValue("1b62b04935b84f58a31bf45efe48955d");

            // Assert
            Assert.IsTrue(resultWorksharingPath.Contains(expectedWorksharingFilePath));
            Assert.AreEqual(expectedIsCloudPathResult, resultIsCloudPath);
        }
    }
}
