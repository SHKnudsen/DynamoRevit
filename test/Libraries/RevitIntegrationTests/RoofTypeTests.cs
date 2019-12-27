using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Autodesk.DesignScript.Geometry;
using CoreNodeModels.Input;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitTestServices;
using RTF.Framework;
using Revit.Elements;

namespace RevitSystemTests
{
    [TestFixture]
    class RoofTypeTests : RevitSystemTestBase
    {
        [Test]
        [TestModel(@".\RoofType\RoofType.rvt")]
        public void CanGetRoofTypeThermalProperties()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\RoofType\canGetRoofTypeThermalProperties.dyn");
            string testPath = Path.GetFullPath(samplePath);

            double expectedRoofTypeAbsorptance = 0.7;
            double expectedRoofTypeHeatTransferCoefficient = 0.15892777;
            double expectedRoofTypeRoughness = 3;
            double expectedRoofTypeThermalMass = 1056333.2;
            double expectedRoofTypeThermalResistance = 6.29216624;

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            var resultRoofTypeAbsorptance = GetPreviewValue("56cb939de2cc4ca4a21731ba78f4299b");
            var resultRoofTypeHeatTransferCoefficient = GetPreviewValue("6d63251353044032b5167428b7749f62");
            var resultRoofTypeRoughness = GetPreviewValue("9cd0f885729641dc928f93250bae096a");
            var resultRoofTypeThermalMass = GetPreviewValue("e60b5201ae23449196beb090d0d825d1");
            var resultRoofTypeThermalResistance = GetPreviewValue("1ca68872f565419b838cdbb8306057d3");

            // Assert
            Assert.AreEqual(expectedRoofTypeAbsorptance, (double)resultRoofTypeAbsorptance, 0.001);
            Assert.AreEqual(expectedRoofTypeHeatTransferCoefficient, (double)resultRoofTypeHeatTransferCoefficient, 0.001);
            Assert.AreEqual(expectedRoofTypeRoughness, (double)resultRoofTypeRoughness, 0.001);
            Assert.AreEqual(expectedRoofTypeThermalMass, (double)resultRoofTypeThermalMass, 0.001);
            Assert.AreEqual(expectedRoofTypeThermalResistance, (double)resultRoofTypeThermalResistance, 0.001);
        }
    }
}
