using System;
using NUnit.Framework;
using Revit.Elements;
using RevitTestServices;
using RTF.Framework;

namespace RevitNodesTests.Elements
{

    [TestFixture]
    public class WallTypeTests : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var wallTypeName = "Curtain Wall 1";
            var wallType = WallType.ByName(wallTypeName);
            Assert.NotNull(wallType);
            Assert.AreEqual(wallTypeName, wallType.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => WallType.ByName(null));
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void canGetWallTypeProperties()
        {
            // Arrange
            var wall = ElementSelector.ByElementId(184176, true);
            var expectedWallTypeName = "Generic - 8\"";
            var expectedWallTypeWidth = 0.666;
            var expectedWallTypeKind = "Basic";
            var expectedWallTypeFunction = "Exterior";

            // Act
            var wallType = wall.ElementType as WallType;
            var resultWallTypeName = wallType.Name;
            var resultWallTypeWidth = wallType.Width;
            var resultWallTypeKind = wallType.Kind;
            var resultWallTypeFunction = wallType.Function;

            // Assert
            Assert.AreEqual(expectedWallTypeName, resultWallTypeName);
            Assert.AreEqual(expectedWallTypeWidth, resultWallTypeWidth, 0.001);
            Assert.AreEqual(expectedWallTypeKind, resultWallTypeKind);
            Assert.AreEqual(expectedWallTypeFunction, resultWallTypeFunction);
        }

    }

}

