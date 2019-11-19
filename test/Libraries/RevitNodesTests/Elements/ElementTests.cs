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
    class ElementTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\materials.rvt")]
        public void SetParameterByName_Element_CanSuccessfullySetMaterialByElement()
        {

            var mat = Revit.Elements.Material.ByName("Glass");

            var ele = ElementSelector.ByType<Autodesk.Revit.DB.FamilyInstance>(true).First();

            var paramName = "Body Material";
            var elemId0 = ele.GetParameterValueByName(paramName);

            Assert.AreNotEqual( mat.Id, elemId0 );

            ele.SetParameterByName(paramName, mat);

            DocumentManager.Regenerate();

            var elemId1 = ele.GetParameterValueByName(paramName) as Element;

            Assert.AreEqual(mat.InternalElement.Id, elemId1.InternalElement.Id);

        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSuccessfullyDeleteElement()
        {
            // Id of wall
            const int wallId = 184176;

            // Get element from document
            using (Element wall = ElementSelector.ByElementId(wallId, true))
            {
                Assert.IsNotNull(wall);

                // Delete Element
                int[] deleted = Element.Delete(wall);

                // Confirm list of elements represent the wall requested to delete. 
                Assert.AreEqual(1, deleted.Length);
                Assert.AreEqual(wallId, deleted[0]);
            }
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSuccessfullySetAndGetElement()
        {
            var wall = ElementSelector.ByElementId(184176, true);
            var famSym = FamilyType.ByName("18\" x 18\"");

            var name = "Column";
            wall.SetParameterByName(name, famSym);
            var sym = wall.GetParameterValueByName(name) as Element;
            Assert.NotNull(sym);
            Assert.AreEqual(sym.Name, "18\" x 18\"");
        }

        [Test] 
        [TestModel(@".\element.rvt")]
        public void CanSuccessfullySetElementParamWithUnitType()
        {
            var wall = ElementSelector.ByElementId(184176, true);

            SetExpectedWallHeight(wall, 45.5);
            GetExpectedWallHeight(wall, 45.5);

            // Change project to meters
            var units = DocumentManager.Instance.CurrentDBDocument.GetUnits();
            units.SetFormatOptions(UnitType.UT_Length, new FormatOptions(DisplayUnitType.DUT_METERS));
            DocumentManager.Instance.CurrentDBDocument.SetUnits(units);

            SetExpectedWallHeight(wall, 45.5);
            GetExpectedWallHeight(wall, 45.5);
        }

        private static void SetExpectedWallHeight(Element wall, double value)
        {
            var name = "Unconnected Height";
            wall.SetParameterByName(name, value);
        }

        private static void GetExpectedWallHeight(Element wall, double value)
        {
            var name = "Unconnected Height";
            var height = (double)(wall.GetParameterValueByName(name));

            Assert.NotNull(height);
            height.ShouldBeApproximately(value);
        }

        [Test]
        [TestModel(@".\element.rvt")]
        public void CanSuccessfullyGetElementParamWithUnitType()
        {
            var wall = ElementSelector.ByElementId(184176, true);
            GetExpectedWallHeight(wall, 20);
        }

        #region Face/Solid Extraction

        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Solids_ExtractsSolidAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var solids = ele.Solids;

            Assert.AreEqual(1, solids.Length);

            var bbox = BoundingBox.ByGeometry(solids);

            bbox.MaxPoint.ShouldBeApproximately(-210.846457, -26.243438, 199.124016, 1e-2);
            bbox.MinPoint.ShouldBeApproximately(-304.160105, -126.243438, 0, 1e-2);
        }

        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Faces_ExtractsFacesAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var faces = ele.Faces;

            Assert.AreEqual(6, faces.Length);

            var bbox = BoundingBox.ByGeometry(faces);

            bbox.MaxPoint.ShouldBeApproximately(-210.846457, -26.243438, 199.124016, 1e-2);
            bbox.MinPoint.ShouldBeApproximately(-304.160105, -126.243438, 0, 1e-2);

            var refs = faces.Select(x => ElementFaceReference.TryGetFaceReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(46874, refer.InternalReference.ElementId.IntegerValue);
            }
        }
        
        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Geometry_ExtractsSolidAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var objects = ele.Geometry();
            Assert.AreEqual(1, objects.Length);

            var solids = objects.OfType<Autodesk.DesignScript.Geometry.Solid>();
            Assert.AreEqual(1, solids.Count());

            var bbox = BoundingBox.ByGeometry(solids);

            bbox.MaxPoint.ShouldBeApproximately(-210.846457, -26.243438, 199.124016, 1e-2);
            bbox.MinPoint.ShouldBeApproximately(-304.160105, -126.243438, 0, 1e-2);
        }
        
        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void ElementFaceReferences_ExtractsExpectedReferences()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var refs = ele.ElementFaceReferences;

            Assert.AreEqual(6, refs.Length);

            foreach (var refer in refs)
            {
                Assert.AreEqual(46874, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        #endregion

        #region Curve extraction

        [Test]
        [TestModel(@".\projectWithNestedNonSharedAdaptiveComponent.rvt")]
        public void Curves_ExtractsCurvesFromNestedNonSharedFamilyInstanceAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(186006, true);
            var crvs = ele.Curves;

            Assert.AreEqual(4, crvs.Length);
            Assert.AreEqual(4, crvs.OfType<Autodesk.DesignScript.Geometry.Line>().Count());

            var bbox = BoundingBox.ByGeometry(crvs);

            bbox.MinPoint.ShouldBeApproximately(-103.697, -88.156, 0, 1e-2);
            bbox.MaxPoint.ShouldBeApproximately(83.445, 108.963, 0, 1e-2);

            var refs = crvs.Select(x => ElementCurveReference.TryGetCurveReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(186006, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        [Test]
        [TestModel(@".\GetCurvesFromFamilyInstance.rfa")]
        public void Curves_ExtractsCurvesAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(32107, true);
            var crvs = ele.Curves;

            Assert.AreEqual(4, crvs.Length);
            Assert.AreEqual(4, crvs.OfType<Autodesk.DesignScript.Geometry.Line>().Count());

            var bbox = BoundingBox.ByGeometry(crvs);

            bbox.MaxPoint.ShouldBeApproximately(50.0, 0, 0, 1e-3);
            bbox.MinPoint.ShouldBeApproximately(0, -100.0, 0, 1e-3);

            var refs = crvs.Select(x => ElementCurveReference.TryGetCurveReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(32107, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        [Test]
        [TestModel(@".\GetCurvesFromFamilyInstance.rfa")]
        public void ElementCurveReferences_ExtractsExpectedReferences()
        {
            var ele = ElementSelector.ByElementId(32107, true);
            var refs = ele.ElementCurveReferences;

            Assert.AreEqual(4, refs.Length);

            foreach (var refer in refs)
            {
                Assert.AreEqual(32107, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        #endregion


        [Test]
        [TestModel(@".\Element\hostedElements.rvt")]
        public void CanSuccessfullyGetHostedElements()
        {
            // Arrange - Select the wall element in revit by it Id
            var elem = ElementSelector.ByElementId(261723, true);
            // Model element names
            string windowFamName = "600 x 3100";
            string curtainWallFamName = "Curtain Wall";
            string wallOpeningFamName = "Rectangular Straight Wall Opening";
            // Assert lists
            var nameCombo1 = new[] { windowFamName, windowFamName, windowFamName };
            var nameCombo2 = new[] { curtainWallFamName, windowFamName, windowFamName, windowFamName, wallOpeningFamName };
            var nameCombo3 = new[] { windowFamName, windowFamName, windowFamName, wallOpeningFamName };
            var nameCombo4 = new[] { curtainWallFamName, windowFamName, windowFamName, windowFamName };

            // Act - Invoke GetHostedElements with all possible cobinations
            var hostedElementsIncludeNothing = elem.GetHostedElements();
            var hostedElementsIncludeEverything = elem.GetHostedElements(true, true, true, true);

            var hostedElementsIncludeOpenings = elem.GetHostedElements(true, false, false, false);
            var hostedElementsIncludeOpeningsAndShadows = elem.GetHostedElements(true, true, false, false);
            var hostedElementsIncludeOpeningsAndShadowsAndEmbeddedWalls = elem.GetHostedElements(true, true, true, false);

            var hostedElementsIncludeShadows = elem.GetHostedElements(false, true, false, false);
            var hostedElementsIncludeShadowsAndEmbeddedWalls = elem.GetHostedElements(false, true, true, false);
            var hostedElementsIncludeShadowsAndEmbeddedWallsAndEmbeddedInserts = elem.GetHostedElements(false, true, true, true);

            var hostedElementsIncludeEmbeddedWalls = elem.GetHostedElements(false, false, true, false);
            var hostedElementsIncludeEmbeddedWallsAndEmbeddedInserts = elem.GetHostedElements(false, false, true, true);
            var hostedElementsIncludeEmbeddedWallsAndEmbeddedInsertsAndOpenings = elem.GetHostedElements(true, false, true, true);

            var hostedElementsIncludeEmbeddedInserts = elem.GetHostedElements(false, false, false, true);
            var hostedElementsIncludeSEmbeddedInsertsAndOpenings = elem.GetHostedElements(true, false, false, true);
            var hostedElementsIncludeEmbeddedInsertsAndOpeningsAndShadows = elem.GetHostedElements(true, true, false, true);

            //Assert all combinations has the right amount of output elements
            Assert.AreEqual(3, hostedElementsIncludeNothing.Count);
            Assert.AreEqual(5, hostedElementsIncludeEverything.Count);

            Assert.AreEqual(4, hostedElementsIncludeOpenings.Count);
            Assert.AreEqual(4, hostedElementsIncludeOpeningsAndShadows.Count);
            Assert.AreEqual(5, hostedElementsIncludeOpeningsAndShadowsAndEmbeddedWalls.Count);

            Assert.AreEqual(3, hostedElementsIncludeShadows.Count);
            Assert.AreEqual(4, hostedElementsIncludeShadowsAndEmbeddedWalls.Count);
            Assert.AreEqual(4, hostedElementsIncludeShadowsAndEmbeddedWallsAndEmbeddedInserts.Count);

            Assert.AreEqual(4, hostedElementsIncludeEmbeddedWalls.Count);
            Assert.AreEqual(4, hostedElementsIncludeEmbeddedWallsAndEmbeddedInserts.Count);
            Assert.AreEqual(5, hostedElementsIncludeEmbeddedWallsAndEmbeddedInsertsAndOpenings.Count);

            Assert.AreEqual(3, hostedElementsIncludeEmbeddedInserts.Count);
            Assert.AreEqual(4, hostedElementsIncludeSEmbeddedInsertsAndOpenings.Count);
            Assert.AreEqual(4, hostedElementsIncludeEmbeddedInsertsAndOpeningsAndShadows.Count);

            //Assert all combinations has the right elements as output
            CollectionAssert.AreEqual(nameCombo1, hostedElementsIncludeNothing.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo2, hostedElementsIncludeEverything.Select(x => x.Name).ToArray());

            CollectionAssert.AreEqual(nameCombo3, hostedElementsIncludeOpenings.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo3, hostedElementsIncludeOpeningsAndShadows.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo2, hostedElementsIncludeOpeningsAndShadowsAndEmbeddedWalls.Select(x => x.Name).ToArray());

            CollectionAssert.AreEqual(nameCombo1, hostedElementsIncludeShadows.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo4, hostedElementsIncludeShadowsAndEmbeddedWalls.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo4, hostedElementsIncludeShadowsAndEmbeddedWallsAndEmbeddedInserts.Select(x => x.Name).ToArray());

            CollectionAssert.AreEqual(nameCombo4, hostedElementsIncludeEmbeddedWalls.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo4, hostedElementsIncludeEmbeddedWallsAndEmbeddedInserts.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo2, hostedElementsIncludeEmbeddedWallsAndEmbeddedInsertsAndOpenings.Select(x => x.Name).ToArray());

            CollectionAssert.AreEqual(nameCombo1, hostedElementsIncludeEmbeddedInserts.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo3, hostedElementsIncludeSEmbeddedInsertsAndOpenings.Select(x => x.Name).ToArray());
            CollectionAssert.AreEqual(nameCombo3, hostedElementsIncludeEmbeddedInsertsAndOpeningsAndShadows.Select(x => x.Name).ToArray());
        }
    }
}
