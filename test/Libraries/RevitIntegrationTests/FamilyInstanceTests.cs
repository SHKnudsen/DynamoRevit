﻿using System;
using System.IO;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.ViewModels;
using NUnit.Framework;
using RevitServices.Elements;
using RevitTestServices;

using RTF.Framework;
using FamilyInstance = Revit.Elements.FamilyInstance;
using Dynamo.Applications.Models;
using RevitServices.Persistence;
using System.Linq;


namespace RevitSystemTests
{
    [TestFixture]
    class FamilyInstanceTests : RevitSystemTestBase
    {
        private const double Tolerance = 0.001;

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void CanPlaceFamilyInstanceByTypeAndCoordinateSystem()
        {
            // Arrange
            string samplePath = Path.Combine(workingDirectory, @".\FamilyInstance\CanPlaceFamilyInstanceByTypeAndCoordinateSystem.dyn");
            string testPath = Path.GetFullPath(samplePath);

            var expectedFacingOrientation = Vector.ByCoordinates(-0.707, 0.707, 0.000);

            // Act
            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();

            var facingOrientationX = GetPreviewValue("8012a71e912a419fbfe73ee867c57d6f");
            var facingOrientationY = GetPreviewValue("d401f82f33644f459627e07dbf92cb83");
            var facingOrientationZ = GetPreviewValue("c4b05df9238d4ff59e01695aabf49c2e");

            // Assert
            Assert.AreEqual(expectedFacingOrientation.X, (double)facingOrientationX, Tolerance);
            Assert.AreEqual(expectedFacingOrientation.Y, (double)facingOrientationY, Tolerance);
            Assert.AreEqual(expectedFacingOrientation.Z, (double)facingOrientationZ, Tolerance);
        }
    }
}
