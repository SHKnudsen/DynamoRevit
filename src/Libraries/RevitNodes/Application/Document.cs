
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.Elements;
using Revit.GeometryConversion;
using System;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using View = Revit.Elements.Views.View;

namespace Revit.Application
{
    /// <summary>
    /// A Revit Document
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Internal reference to the Document
        /// </summary>
        internal Autodesk.Revit.DB.Document InternalDocument { get; private set; }

        internal Document(Autodesk.Revit.DB.Document currentDBDocument)
        {
            InternalDocument = currentDBDocument;  
        }

        /// <summary>
        /// Get the active view for the document
        /// </summary>
        public View ActiveView
        {
            get
            {
                return (View)InternalDocument.ActiveView.ToDSType(true);
            }
        }

        /// <summary>
        /// Is the Document a Family?
        /// </summary>
        public bool IsFamilyDocument
        {
            get
            {
                return InternalDocument.IsFamilyDocument;
            }
        }

        /// <summary>
        /// The full path of the Document.
        /// </summary>
        public string FilePath
        {
            get { return InternalDocument.PathName ?? string.Empty; }
        }

        /// <summary>
        /// Get the current document
        /// </summary>
        /// <returns></returns>
        public static Document Current
        {
            get { return new Document(DocumentManager.Instance.CurrentDBDocument); }
        }

        /// <summary>
        /// Gets the worksharing path of the current document
        /// </summary>
        public string WorksharingPath
        {
            get
            {
                ModelPath modelPath = WorksharingModelPath;
                if (modelPath == null)
                    throw new NullReferenceException(Properties.Resources.DocumentNotWorkshared);
                return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath); 
            }
        }

        /// <summary>
        /// Whether the Worksharing path represents a path on an Autodesk server such as BIM360.
        /// </summary>
        public bool IsCloudPath
        {
            get{ return WorksharingModelPath.CloudPath; }
        }

        internal ModelPath WorksharingModelPath
        {
            get { return this.InternalDocument.GetWorksharingCentralModelPath(); }
        }

        /// <summary>
        /// Purge unused Elements from the model.
        /// </summary>
        /// <param name="fullPurge">Similar to clicking the purge button multiple times in Revit, the node will repeatedly purge until there is nothing left to remove.</param>
        /// <returns>Purged element ids</returns>
        public List<int> PurgeUnused(bool fullPurge = false)
        {
            List<ElementId> purgedElementIds = new List<ElementId>();
            TransactionManager.Instance.EnsureInTransaction(this.InternalDocument);

            purgedElementIds.AddRange(PurgeElements(fullPurge));
            purgedElementIds.AddRange(PurgeMaterials());

            TransactionManager.Instance.TransactionTaskDone();

            if (purgedElementIds.Count == 0)
                throw new InvalidOperationException(Properties.Resources.NoElementsToPurge);

            return purgedElementIds.Select(x => x.IntegerValue).ToList();
        }

        private List<ElementId> PurgeMaterials()
        {
            List<Autodesk.Revit.DB.ElementId> materials = new FilteredElementCollector(this.InternalDocument).OfClass(typeof(Autodesk.Revit.DB.Material))
                                                                                                           .ToElementIds()
                                                                                                           .ToList();
            if (materials == null)
                return new List<ElementId>();

            List<Autodesk.Revit.DB.Element> elementTypes = new FilteredElementCollector(this.InternalDocument).WhereElementIsElementType()
                                                                                                              .ToElements()
                                                                                                              .ToList();
            if (elementTypes == null)
                return new List<ElementId>();

            var nonPurgedElements = new List<Autodesk.Revit.DB.ElementId>();

            for (int i = 0; i < elementTypes.Count; i++)
            {
                HostObjAttributes hostObj = elementTypes[i] as HostObjAttributes;
                if (hostObj != null)
                {
                    var compundStructure = hostObj.GetCompoundStructure();
                    if (compundStructure == null)
                        continue;

                    int layerCount = compundStructure.LayerCount;
                    for (int j = 0; j < layerCount; j++)
                    {
                        ElementId elementId = compundStructure.GetMaterialId(j);
                        if (materials.Contains(elementId))
                        {
                            nonPurgedElements.Add(elementId);
                            materials.Remove(elementId);
                        }
                        
                    }
                    continue;
                }

                List<ElementId> elementMaterialIds = elementTypes[i].GetMaterialIds(false).ToList();
                if (elementMaterialIds.Any())
                {
                    materials.RemoveAll(purgedId => elementMaterialIds.Exists(elemId => purgedId == elemId));
                    nonPurgedElements.AddRange(elementMaterialIds.Where(elmeMatId => !nonPurgedElements.Any(nonPurgedElemeMatId => nonPurgedElemeMatId == elmeMatId)));
                }

                if (!materials.Any())
                    break;
            }
            if (materials.Count > 0)
                this.InternalDocument.Delete(materials);
            List<ElementId> purgedMaterials = new List<ElementId>();
            purgedMaterials.AddRange(PurgeMaterialAssets(nonPurgedElements));
            purgedMaterials.AddRange(materials);

            return purgedMaterials;
        }

        private List<Autodesk.Revit.DB.ElementId> PurgeMaterialAssets(List<Autodesk.Revit.DB.ElementId> elementIds)
        {
            List<ElementId> appearanceAssetIds = new FilteredElementCollector(this.InternalDocument).OfClass(typeof(AppearanceAssetElement))
                                                                                                    .ToElementIds()
                                                                                                    .ToList();

            List<ElementId> propertySet = new FilteredElementCollector(this.InternalDocument).OfClass(typeof(PropertySetElement))
                                                                                             .ToElementIds()
                                                                                             .ToList();
            int elementIdsCount = elementIds.Count();
            for (int i = 0; i < elementIdsCount; i++)
            {
                var material = this.InternalDocument.GetElement(elementIds[i]) as Autodesk.Revit.DB.Material;
                propertySet.Remove(material.ThermalAssetId);
                propertySet.Remove(material.StructuralAssetId);
                appearanceAssetIds.Remove(material.AppearanceAssetId);
            }

            if (appearanceAssetIds.Count > 0)
                this.InternalDocument.Delete(appearanceAssetIds);
            if (propertySet.Count > 0)
                this.InternalDocument.Delete(propertySet);

            List<ElementId> purgedElementIds = new List<ElementId>(appearanceAssetIds.Concat(propertySet));
            return purgedElementIds;
        }

        private List<ElementId> PurgeElements(bool runRecursively)
        {
            List<ElementId> purgedElementIds = new List<ElementId>();
            List<ElementId> purgeableElementIds = GetPurgeableElementIds();
            
            if (!runRecursively)
            {
                if (purgeableElementIds == null)
                    return purgedElementIds;
                purgedElementIds.AddRange(purgeableElementIds);
                this.InternalDocument.Delete(purgeableElementIds);
                return purgedElementIds;
            }
            if (purgeableElementIds == null)
                return purgedElementIds;

            purgedElementIds.AddRange(purgeableElementIds);
            this.InternalDocument.Delete(purgeableElementIds);
            purgedElementIds.AddRange(PurgeElements(runRecursively));

            return purgedElementIds;
        }

        private List<Autodesk.Revit.DB.ElementId> GetPurgeableElementIds()
        {
            List<Autodesk.Revit.DB.FailureMessage> failureMessages = PerformanceAdviser.GetPerformanceAdviser()
                                                                                       .ExecuteRules(this.InternalDocument, GetPerfromanceAdviserRuleId())
                                                                                       .ToList();
            if (failureMessages.Count > 0)
            {
                List<ElementId> purgeableElementIds = failureMessages[0].GetFailingElements().ToList();
                return purgeableElementIds;
            }
            return null;
        }

        private static List<PerformanceAdviserRuleId> GetPerfromanceAdviserRuleId()
        {
            //The internal GUID of the purge Performance Adviser Rule 
            const string purgePerformanceAdviserRuleGuid = "e8c63650-70b7-435a-9010-ec97660c1bda";

            IList<PerformanceAdviserRuleId> performanceAdviserRules = PerformanceAdviser.GetPerformanceAdviser().GetAllRuleIds();
            List<PerformanceAdviserRuleId> performanceAdviserRuleId = performanceAdviserRules.Where(x => x.Guid.ToString() == purgePerformanceAdviserRuleGuid).ToList();
            return performanceAdviserRuleId;
        }

        /// <summary>
        /// Extracts Latitude and Longitude from Revit
        /// </summary>
        /// 
        /// <returns name="Lat">Latitude</returns>
        /// <returns name="Long">Longitude</returns>
        /// <search>Latitude, Longitude</search>

        public DynamoUnits.Location Location
        {
            get
            {
                var loc = InternalDocument.SiteLocation;
                return DynamoUnits.Location.ByLatitudeAndLongitude(
                    loc.Latitude.ToDegrees(),
                    loc.Longitude.ToDegrees());
            }
        }

        /// <summary>
        /// Saves all of the input families at a given location.
        /// </summary>
        /// <param name="family">The Revit family to save</param>
        /// <param name="directoryPath">Directory to save the family to. If directory does not exist, it will be created.</param>
        /// <returns>File path of saved families</returns>
        public string SaveFamilyToFolder(Revit.Elements.Family family, string directoryPath)
        {
            var dir = new DirectoryInfo(directoryPath);
            if (!dir.Exists)
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Close all transactions
            var trans = TransactionManager.Instance;
            trans.ForceCloseTransaction();

            var fam = family.InternalFamily;
            var familyDocument = this.InternalDocument.EditFamily(fam);
            var name = fam.Name + ".rfa";
            string filePath = Path.Combine(directoryPath, name);
            familyDocument.SaveAs(filePath);
            familyDocument.Close(false);
            return filePath;
        }
    }

}
