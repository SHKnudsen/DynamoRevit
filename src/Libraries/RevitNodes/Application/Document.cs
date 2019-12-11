
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.Elements;
using Revit.GeometryConversion;

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
        /// Purge unused Elements from the model. This node is not able to purge materials and material assets
        /// </summary>
        /// <returns></returns>
        public string PurgeUnused()
        {
            //The internal GUID of the Performance Adviser Rule 
            const string purgeGuid = "e8c63650-70b7-435a-9010-ec97660c1bda";

            var performanceAdviserRuleId = new List<PerformanceAdviserRuleId>();
            var performanceAdviserRules = PerformanceAdviser.GetPerformanceAdviser().GetAllRuleIds();

            //Iterating through all PerformanceAdviser rules looking to find that which matches PURGE_GUID
            for (int i = 0; i < performanceAdviserRules.Count; i++)
            {
                if (performanceAdviserRules[i].Guid.ToString() == purgeGuid)
                {
                    performanceAdviserRuleId.Add(performanceAdviserRules[i]);
                    break;
                }
            }

            //Attempting to recover all purgeable elements and delete them from the document
            List<ElementId> purgeableElementIds = getPurgeableElementIds(this.InternalDocument, performanceAdviserRuleId);
            if (purgeableElementIds == null) 
            {
                return Properties.Resources.NoElementsToPurge;
            }
            TransactionManager.Instance.EnsureInTransaction(this.InternalDocument);
            this.InternalDocument.Delete(purgeableElementIds);
            TransactionManager.Instance.TransactionTaskDone();
            return string.Format(Properties.Resources.PurgedElements, purgeableElementIds.Count);
        }

        private static List<Autodesk.Revit.DB.ElementId> getPurgeableElementIds(Autodesk.Revit.DB.Document document, List<PerformanceAdviserRuleId> performanceAdviserRuleId)
        {
            List<Autodesk.Revit.DB.FailureMessage> failureMessages = PerformanceAdviser.GetPerformanceAdviser().ExecuteRules(document,
                                                                                                                             performanceAdviserRuleId).ToList();
            if (failureMessages.Count > 0)
            {
                List<ElementId> purgeableElementIds = failureMessages[0].GetFailingElements().ToList();
                return purgeableElementIds;
            }
            return null;
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
