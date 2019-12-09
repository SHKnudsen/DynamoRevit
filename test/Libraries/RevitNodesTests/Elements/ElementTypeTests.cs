using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryReferences;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

using Element = Revit.Elements.Element;
using FamilyType = Revit.Elements.FamilyType;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    class ElementTypeTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetElementTypeProperties()
        {
            // Arrange
            var wall = ElementSelector.ByElementId(184176, true);
            var column = ElementSelector.ByElementId(184324, true);

            //var expectedWallTypeName = ;
            //var expectedWallTypeFamilyName = ;
            //var expectedWallTypeCanBeDeleted = ;
            //var expectedWallTypeCanBeCopied = ;
            //var expectedWallTypeCanBeRenamed = ;

            //var expectedColumnTypeName = ;
            //var expectedColumnTypeFamilyName = ;
            //var expectedColumnTypeCanBeDeleted = ;
            //var expectedColumnTypeCanBeCopied = ;
            //var expectedColumnTypeCanBeRenamed = ;

            // Act
            var wallElementType = wall.ElementType;
            var columnElementType = column.ElementType;

            // Assert
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetElementTypeByName()
        {
            // Arrange - get element from model
            string elementTypeName = "24\" x 24\"";
            string emptyTypeName = "";
            string notFoundTypeName = "wallTypeTest";
            int expectedId = 60411;

            // Act
            Element typeElement = Revit.Elements.ElementType.ByName(elementTypeName);
            Assert.Throws<System.ArgumentNullException>(() => Revit.Elements.ElementType.ByName(emptyTypeName));
            Assert.Throws<KeyNotFoundException>(() => Revit.Elements.ElementType.ByName(notFoundTypeName));
            int typeId = typeElement.InternalElement.Id.IntegerValue;

            // Assert
            Assert.AreEqual(expectedId, typeId);
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanDuplicateElementType()
        {
            // Arrange


            // Act


            // Assert

        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanGetElementTypePreviewImage()
        {
            // Arrange


            // Act


            // Assert
        }
    }
}
