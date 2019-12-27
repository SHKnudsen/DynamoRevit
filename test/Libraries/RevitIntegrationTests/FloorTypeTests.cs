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
    public class FloorTypeTests : RevitSystemTestBase
    {
        [Test]
        [TestModel(@".\FloorType\FloorType.rvt")]
        public void CanGetFloorTypeThermalProperties()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\FloorType\canGetFloorTypeThermalProperties.dyn");
            string testPath = Path.GetFullPath(samplePath);

            double expectedWallTypeAbsorptance = 0.7;
            double expectedWallTypeHeatTransferCoefficient = 0.117437;
            double expectedWallTypeRoughness = 3;
            double expectedWallTypeThermalMass = 365549.43;
            double expectedWallTypeThermalResistance = 8.515159;

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            var resultWallTypeAbsorptance = GetPreviewValue("56cb939de2cc4ca4a21731ba78f4299b");
            var resultWallTypeHeatTransferCoefficient = GetPreviewValue("6d63251353044032b5167428b7749f62");
            var resultWallTypeRoughness = GetPreviewValue("9cd0f885729641dc928f93250bae096a");
            var resultWallTypeThermalMass = GetPreviewValue("e60b5201ae23449196beb090d0d825d1");
            var resultWallTypeThermalResistance = GetPreviewValue("1ca68872f565419b838cdbb8306057d3");

            // Assert
            Assert.AreEqual(expectedWallTypeAbsorptance, (double)resultWallTypeAbsorptance, 0.001);
            Assert.AreEqual(expectedWallTypeHeatTransferCoefficient, (double)resultWallTypeHeatTransferCoefficient, 0.001);
            Assert.AreEqual(expectedWallTypeRoughness, (double)resultWallTypeRoughness, 0.001);
            Assert.AreEqual(expectedWallTypeThermalMass, (double)resultWallTypeThermalMass, 0.001);
            Assert.AreEqual(expectedWallTypeThermalResistance, (double)resultWallTypeThermalResistance, 0.001);
        }

        [Test]
        [TestModel(@".\FloorType\FloorType.rvt")]
        public void CanGetFloorTypeStructuralMaterial()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\FloorType\canGetFloorTypeStructuralMaterial.dyn");
            string testPath = Path.GetFullPath(samplePath);

            int expectedMaterialIdOnFloor = 45453;

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            var structuralMaterailId = GetPreviewValue("170186573b144adc88acbeba69b47f9c");

            // Assert
            Assert.AreEqual(expectedMaterialIdOnFloor, structuralMaterailId);
        }

        [Test]
        [TestModel(@".\FloorType\FloorType.rvt")]
        public void CanCheckIfFloorTypeIsFoundationSlab()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\FloorType\CanCheckIfFloorTypeIsFoundationSlab.dyn");
            string testPath = Path.GetFullPath(samplePath);

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            var isFoundationSlab = GetPreviewValue("4b5c27789bab4c43a6864e33786a2847");

            // Assert
            Assert.AreEqual(false, isFoundationSlab);
        }

    }
}
