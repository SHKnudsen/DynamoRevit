using NUnit.Framework;
using Revit.Application;
using Revit.Elements;
using RevitTestServices;
using RTF.Framework;
using System;
using System.IO;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class DocumentTests : RevitNodeTestBase
    {

        [Test]
        public void Current()
        {
            var doc = Document.Current;
            Assert.NotNull(doc);
            Assert.NotNull(doc.ActiveView);
            Assert.IsTrue(doc.IsFamilyDocument);
        }

        [Test]
        [TestModel(@".\Document\LocalModel\Project1_LocalFile.rvt")]
        public void CanGetWorksharingModelPath()
        {
            // Arrange
            var doc = Document.Current;
            string expectedWorksharingFilePath = @"DynamoRevit\test\System\Document\CentralModel";
            bool expectedIsCloudPathResult = false;

            // Act
            string resultWorksharingPath = doc.WorksharingPath;
            bool resultIsCloudPath = doc.IsCloudPath;

            // Assert
            Assert.IsTrue(resultWorksharingPath.Contains(expectedWorksharingFilePath));
            Assert.AreEqual(expectedIsCloudPathResult, resultIsCloudPath);
        }

        [Test]
        [TestModel(@".\Document\BIM360\4481adfb-0f03-4e58-9f49-8bd37dde9e0e.rvt")]
        public void CanGetWorksharingModelPathOnCloudModel()
        {
            // Arrange
            var doc = Document.Current;
            string expectedWorksharingFilePath = @"BIM 360://Node test/BIM360_model.rvt";
            bool expectedIsCloudPathResult = true;

            // Act
            string resultWorksharingPath = doc.WorksharingPath;
            bool resultIsCloudPath = doc.IsCloudPath;

            // Assert
            Assert.IsTrue(resultWorksharingPath.Contains(expectedWorksharingFilePath));
            Assert.AreEqual(expectedIsCloudPathResult, resultIsCloudPath);
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetWorksharingModelPathOnNotWorksharedModel()
        {
            // Arrange
            var doc = Document.Current;
            string expectedNullReferenceExceptionMessage = Revit.Properties.Resources.DocumentNotWorkshared;
            bool expectedIsCloudPathResult = false;

            // Act
            var resultWorksharingPath = Assert.Throws<System.NullReferenceException>(() => GetWorksharingPathThrowsNullReferenceInNonSharedDocument());
            bool resultIsCloudPath = doc.IsCloudPath;

            // Assert
            Assert.AreEqual(resultWorksharingPath.Message, expectedNullReferenceExceptionMessage);
            Assert.AreEqual(expectedIsCloudPathResult, resultIsCloudPath);
        }

        private void GetWorksharingPathThrowsNullReferenceInNonSharedDocument()
        {
            string worksharePath = Document.Current.WorksharingPath;
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSaveFamiliesInCurrentDocument()
        {
            // Arrange
            var saveableFamily = ElementSelector.ByElementId(110049, true);
            var noneditableFamily = ElementSelector.ByElementId(20915, true);

            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempFolder);
            
            int expectedSavedFamilyId = 110049;

            // Act
            var doc = Document.Current;
            var resultSavedFamily = doc.SaveFamilyLibrary((Family)saveableFamily, tempFolder);
            int resultElementId = resultSavedFamily.Id;
            var resultnoneditableFamily = Assert.Throws<Autodesk.Revit.Exceptions.ArgumentException>(() => doc.SaveFamilyLibrary((Family)noneditableFamily, tempFolder));
            var fileExist = File.Exists(Path.Combine(tempFolder, saveableFamily.Name + ".rfa"));

            // Assert
            Assert.AreEqual(expectedSavedFamilyId, resultElementId);
            Assert.IsTrue(fileExist);

            // Clean up
            Directory.Delete(tempFolder, true);
        }
    }
}
