//
// BIM IFC library: this library works with Autodesk(R) Revit(R) to export IFC files containing model geometry.
// Copyright (C) 2012  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using BIM.IFC.Exporter.PropertySet;
using BIM.IFC.Utility;
using System.Reflection;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// This class implements the method of interface IExporterIFC to perform an export to IFC. 
    /// It also implements the methods of interface IExternalDBApplication to register the IFC export client to Autodesk Revit.
    /// </summary>
    class Exporter : IExporterIFC, IExternalDBApplication
    {
        // Used for debugging tool "WriteIFCExportedElements"
        private StreamWriter m_Writer;

        private IFCFile m_IfcFile;

        #region IExternalDBApplication Members

        /// <summary>
        /// The method called when Autodesk Revit exits.
        /// </summary>
        /// <param name="application">Controlled application to be shutdown.</param>
        /// <returns>Return the status of the external application.</returns>
        public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        /// <summary>
        /// The method called when Autodesk Revit starts.
        /// </summary>
        /// <param name="application">Controlled application to be loaded to Autodesk Revit process.</param>
        /// <returns>Return the status of the external application.</returns>
        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication application)
        {
            ExporterIFCRegistry.RegisterIFCExporter(this);
            return ExternalDBApplicationResult.Succeeded;
        }

        #endregion

        #region IExporterIFC Members

        /// <summary>
        /// Implements the method that Autodesk Revit will invoke to perform an export to IFC.
        /// </summary>
        /// <param name="document">The document to export.</param>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="filterView">The view whose filter visibility settings govern the export.</param>
        public void ExportIFC(Autodesk.Revit.DB.Document document, ExporterIFC exporterIFC, Autodesk.Revit.DB.View filterView)
        {
            try
            {
                // cache options
                ExportOptionsCache exportOptionsCache = ExportOptionsCache.Create(exporterIFC, filterView);
                ExporterCacheManager.ExportOptionsCache = exportOptionsCache;

                ElementFilteringUtil.InitCategoryVisibilityCache();
                
                //begin export
                BeginExport(exporterIFC, document);

                FilteredElementCollector spatialElementCollector;
                FilteredElementCollector otherElementCollector;
                ICollection<ElementId> idsToExport = exportOptionsCache.ElementsForExport;
                if (idsToExport.Count > 0)
                {
                    spatialElementCollector = new FilteredElementCollector(document, idsToExport);
                    otherElementCollector = new FilteredElementCollector(document, idsToExport);
                }
                else
                {
                    spatialElementCollector = (filterView == null) ?
                        new FilteredElementCollector(document) : new FilteredElementCollector(document, filterView.Id);
                    otherElementCollector = (filterView == null) ?
                        new FilteredElementCollector(document) : new FilteredElementCollector(document, filterView.Id);
                }

                bool spaceExported = true;
                if (exportOptionsCache.SpaceBoundaryLevel == 2)
                {
                    SpatialElementExporter.ExportSpatialElement2ndLevel(this, exporterIFC, document, filterView, ref spaceExported);
                }

                //export spatial element - none or 1st level room boundaries
                //  or create IFC Space only if 2nd level room boundaries export failed
                if (exportOptionsCache.SpaceBoundaryLevel == 0 || exportOptionsCache.SpaceBoundaryLevel == 1 || !spaceExported)
                {
                    SpatialElementExporter.InitializeSpatialElementGeometryCalculator(document, exporterIFC);
                    ElementFilter spatialElementFilter = ElementFilteringUtil.GetSpatialElementFilter(document, exporterIFC, filterView);
                    spatialElementCollector.WherePasses(spatialElementFilter);
                    foreach (Element element in spatialElementCollector)
                    {
                        ExportElement(exporterIFC, filterView, element);
                    }
                }

                //export other elements
                ElementFilter nonSpatialElementFilter = ElementFilteringUtil.GetNonSpatialElementFilter(document, exporterIFC, filterView);
                otherElementCollector.WherePasses(nonSpatialElementFilter);
                foreach (Element element in otherElementCollector)
                {
                    ExportElement(exporterIFC, filterView, element);
                }

                // Export railings cached above.  Railings are exported last as their containment is not known until all stairs have been exported.
                // This is a very simple sorting, and further containment issues could require a more robust solution in the future.
                foreach (ElementId elementId in ExporterCacheManager.RailingCache)
                {
                    Element element = document.GetElement(elementId);
                    ExportElement(exporterIFC, filterView, element);
                }

                ConnectorExporter.Export(exporterIFC);

                // end export
                EndExport(exporterIFC, document);
            }
            finally
            {
                ExporterCacheManager.Clear();

                if (m_Writer != null)
                    m_Writer.Close();

                if (m_IfcFile != null)
                {
                    m_IfcFile.Close();
                    m_IfcFile = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Performs the export of elements, including spatial and non-spatial elements.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="filterView">The view whose filter visibility settings govern the export.</param>
        /// <param name="element ">The element to export.</param>
        internal void ExportElement(ExporterIFC exporterIFC, Autodesk.Revit.DB.View filterView, Autodesk.Revit.DB.Element element)
        {
            if (!ElementFilteringUtil.ShouldCategoryBeExported(exporterIFC, element))
                return;

            // if we allow exporting parts as independent building elements, then prevent also exporting the host elements containing the parts.
            if (ExporterCacheManager.ExportOptionsCache.ExportPartsAsBuildingElements && PartExporter.CanExportParts(element))
                return;

            //WriteIFCExportedElements
            if (m_Writer != null)
            {
                Category category = element.Category;
                m_Writer.WriteLine(String.Format("{0},{1},{2}", element.Id, category == null ? "null" : category.Name, element.GetType().Name));
            }

            try
            {
                using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(exporterIFC, true))
                {
                    ExportElementImpl(exporterIFC, element, filterView, productWrapper);

                    ExportElementProperties(exporterIFC, element, productWrapper);
                    ExportElementQuantities(exporterIFC, element, productWrapper);
                    if (ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE)
                        ExportElementClassifications(exporterIFC, element, productWrapper);
                }
            }
            catch (System.Exception ex)
            {
                HandleUnexpectedException(ex, exporterIFC, element);
            }
        }

        /// <summary>
        /// Handles the unexpected Exception.
        /// </summary>
        /// <param name="ex">The unexpected exception.</param>
        /// <param name="element ">The element got the exception.</param>
        internal void HandleUnexpectedException(Exception exception, ExporterIFC exporterIFC, Element element)
        {
            Document document = element.Document;
            string errMsg = String.Format("IFC error: Exporting element \"{0}\",{1} - {2}", element.Name, element.Id, exception.ToString());
            element.Document.Application.WriteJournalComment(errMsg, true);

            if (!ExporterUtil.IsFatalException(document, exception))
            {
                FailureMessage fm = new FailureMessage(BuiltInFailures.ExportFailures.IFCGenericExportWarning);
                fm.SetFailingElement(element.Id);
                document.PostFailure(fm);
            }
            else
            {
                // This exception should be rethrown back to the main Revit application.
                throw exception;
            }
        }

        /// <summary>
        /// Checks if the element is MEP type.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element">The element to check.</param>
        /// <returns>True for MEP type of elements.</returns>
        private bool IsMEPType(ExporterIFC exporterIFC, Element element, IFCExportType exportType)
        {
            return ElementFilteringUtil.IsMEPType(exportType);
        }

        /// <summary>
        /// Checks if exporting an element as building elment proxy.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True for exporting as proxy element.</returns>
        private bool ExportAsProxy(Element element, IFCExportType exportType)
        {
            // FaceWall should be exported as IfcWall.
            return ((element is FaceWall) || (element is ModelText) || (exportType == IFCExportType.ExportBuildingElementProxy));
        }

        /// <summary>
        /// Checks if exporting an element of Stairs category.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if element is of category OST_Stairs.</returns>
        private bool IsStairs(Element element)
        {
            // FaceWall should be exported as IfcWall.
            return (CategoryUtil.GetSafeCategoryId(element) == new ElementId(BuiltInCategory.OST_Stairs));
        }

        /// <summary>
        /// Implements the export of element.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element">The element to export.</param>
        /// <param name="filterView">The view to export, if it exists.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        private void ExportElementImpl(ExporterIFC exporterIFC, Element element, Autodesk.Revit.DB.View filterView, 
            IFCProductWrapper productWrapper)
        {
            Options options;
            View ownerView = element.Document.GetElement(element.OwnerViewId) as View;
            if (ownerView == null)
            {
                options = GeometryUtil.GetIFCExportGeometryOptions();
            }
            else
            {
                options = new Options();
                options.View = ownerView;
            }
            GeometryElement geomElem = element.get_Geometry(options);

            try
            {
                exporterIFC.PushExportState(element, geomElem);

                using (SubTransaction st = new SubTransaction(element.Document))
                {
                    st.Start();

                    if (element is AssemblyInstance)
                    {
                        AssemblyInstance assemblyInstance = element as AssemblyInstance;
                        AssemblyInstanceExporter.ExportAssemblyInstanceElement(exporterIFC, assemblyInstance, productWrapper);
                    }
                    else if (element is Ceiling)
                    {
                        Ceiling ceiling = element as Ceiling;
                        CeilingExporter.ExportCeilingElement(exporterIFC, ceiling, geomElem, productWrapper);
                    }
                    else if (element is CeilingAndFloor || element is Floor)
                    {
                        // This covers both Floors and Building Pads.
                        HostObject hostObject = element as HostObject;
                        FloorExporter.Export(exporterIFC, hostObject, geomElem, productWrapper);
                    }
                    else if (element is ContFooting)
                    {
                        ContFooting footing = element as ContFooting;
                        FootingExporter.ExportFootingElement(exporterIFC, footing, geomElem, productWrapper);
                    }
                    else if (element is CurveElement)
                    {
                        CurveElement curveElem = element as CurveElement;
                        CurveElementExporter.ExportCurveElement(exporterIFC, curveElem, geomElem, productWrapper);
                    }
                    else if (element is DuctInsulation)
                    {
                        DuctInsulation ductInsulation = element as DuctInsulation;
                        DuctInsulationExporter.ExportDuctInsulation(exporterIFC, ductInsulation, geomElem, productWrapper);
                    }
                    else if (element is DuctLining)
                    {
                        DuctLining ductLining = element as DuctLining;
                        DuctLiningExporter.ExportDuctLining(exporterIFC, ductLining, geomElem, productWrapper);
                    }
                    else if (element is FamilyInstance)
                    {
                        FamilyInstance familyInstanceElem = element as FamilyInstance;
                        FamilyInstanceExporter.ExportFamilyInstanceElement(exporterIFC, familyInstanceElem, geomElem, productWrapper);
                    }
                    else if (element is FilledRegion)
                    {
                        FilledRegion filledRegion = element as FilledRegion;
                        FilledRegionExporter.Export(exporterIFC, filledRegion, geomElem, productWrapper);
                    }
                    else if (element is HostedSweep)
                    {
                        HostedSweep hostedSweep = element as HostedSweep;
                        HostedSweepExporter.Export(exporterIFC, hostedSweep, geomElem, productWrapper);
                    }
                    else if (element is Part)
                    {
                        Part part = element as Part;
                        if (ExporterCacheManager.ExportOptionsCache.ExportPartsAsBuildingElements)
                            PartExporter.ExportPartAsBuildingElement(exporterIFC, part, geomElem, productWrapper);
                        else
                            PartExporter.ExportStandalonePart(exporterIFC, part, geomElem, productWrapper);
                    }
                    else if (element is Railing)
                    {
                        if (ExporterCacheManager.RailingCache.Contains(element.Id))
                            RailingExporter.ExportRailingElement(exporterIFC, element as Railing, productWrapper);
                        else
                        {
                            ExporterCacheManager.RailingCache.Add(element.Id);
                            RailingExporter.AddSubElementsToCache(element as Railing);
                        }
                    }
                    else if (RampExporter.IsRamp(element))
                    {
                        RampExporter.Export(exporterIFC, element, geomElem, productWrapper);
                    }
                    else if (element is Rebar)
                    {
                        Rebar spatialElem = element as Rebar;
                        RebarExporter.ExportRebar(exporterIFC, spatialElem, filterView, productWrapper);
                    }
                    else if (element is SpatialElement)
                    {
                        SpatialElement spatialElem = element as SpatialElement;
                        SpatialElementExporter.ExportSpatialElement(exporterIFC, spatialElem, productWrapper);
                    }
                    else if (IsStairs(element))
                    {
                        StairsExporter.Export(exporterIFC, element, geomElem, productWrapper);
                    }
                    else if (element is TextNote)
                    {
                        TextNote textNote = element as TextNote;
                        TextNoteExporter.Export(exporterIFC, textNote, productWrapper);
                    }
                    else if (element is TopographySurface)
                    {
                        TopographySurface topSurface = element as TopographySurface;
                        SiteExporter.ExportTopographySurface(exporterIFC, topSurface, geomElem, productWrapper);
                    }
                    else if (element is Wall)
                    {
                        Wall wallElem = element as Wall;
                        WallExporter.Export(exporterIFC, wallElem, geomElem, productWrapper);
                    }
                    else if (element is WallSweep)
                    {
                        WallSweep wallSweep = element as WallSweep;
                        WallSweepExporter.Export(exporterIFC, wallSweep, geomElem, productWrapper);
                    }
                    else if (element is RoofBase)
                    {
                        RoofBase roofElement = element as RoofBase;
                        RoofExporter.Export(exporterIFC, roofElement, geomElem, productWrapper);
                    }
                    else if (element is CurtainSystem)
                    {
                        CurtainSystem curtainSystem = element as CurtainSystem;
                        CurtainSystemExporter.ExportCurtainSystem(exporterIFC, curtainSystem, productWrapper);
                    }
                    else if (CurtainSystemExporter.IsLegacyCurtainElement(element))
                    {
                        CurtainSystemExporter.ExportLegacyCurtainElement(exporterIFC, element, productWrapper);
                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                    }
                    else
                    {
                        string ifcEnumType;
                        IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, element, out ifcEnumType);

                        if (IsMEPType(exporterIFC, element, exportType))
                        {
                            GenericMEPExporter.Export(exporterIFC, element, geomElem, productWrapper);
                        }
                        else if (ExportAsProxy(element, exportType))
                        {
                            ProxyElementExporter.Export(exporterIFC, element, geomElem, productWrapper);
                        }
                    }
                    
                    if (element.AssemblyInstanceId != ElementId.InvalidElementId)
                        ExporterCacheManager.AssemblyInstanceCache.RegisterElements(element.AssemblyInstanceId, productWrapper);

                    st.RollBack();
                }
            }
            finally
            {
                exporterIFC.PopExportState();
            }
        }

        /// <summary>
        /// Initializes the common properties at the beginning of the export process.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="document">The document to export.</param>
        private void BeginExport(ExporterIFC exporterIFC, Document document)
        {
            ExporterCacheManager.Document = document;
            String writeIFCExportedElementsVar = Environment.GetEnvironmentVariable("WriteIFCExportedElements");
            if (writeIFCExportedElementsVar != null && writeIFCExportedElementsVar.Length > 0)
            {
                m_Writer = new StreamWriter(@"c:\ifc-output-filters.txt");
            }

            IFCFileModelOptions modelOptions = new IFCFileModelOptions();
            if (exporterIFC.ExportAs2x2)
            {
                modelOptions.SchemaFile = Path.Combine(ExporterUtil.RevitProgramPath, "EDM\\IFC2X2_ADD1.exp");
                modelOptions.SchemaName = "IFC2x2_FINAL";
            }
            else
            {
                modelOptions.SchemaFile = Path.Combine(ExporterUtil.RevitProgramPath, "EDM\\IFC2X3_TC1.exp");
                modelOptions.SchemaName = "IFC2x3";
            }

            m_IfcFile = IFCFile.Create(modelOptions);
            exporterIFC.SetFile(m_IfcFile);

            //init common properties
            ExporterInitializer.InitPropertySets(ExporterCacheManager.ExportOptionsCache.FileVersion);
            ExporterInitializer.InitQuantities(ExporterCacheManager.ExportOptionsCache.FileVersion, ExporterCacheManager.ExportOptionsCache.ExportBaseQuantities);

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                // create building
                IFCAnyHandle applicationHandle = CreateApplicationInformation(file, document.Application);

                CreateGlobalCartesianOrigin(exporterIFC);
                CreateGlobalDirection(exporterIFC);
                CreateGlobalDirection2D(exporterIFC);

                IFCAnyHandle units = CreateDefaultUnits(exporterIFC, document);

                // Start out relative to nothing, but replace with site later.
                IFCAnyHandle relativePlacement = ExporterUtil.CreateAxis2Placement3D(file);
                IFCAnyHandle buildingPlacement = IFCInstanceExporter.CreateLocalPlacement(file, null, relativePlacement);

                HashSet<IFCAnyHandle> repContexts = CreateContextInformation(exporterIFC, document);
                IFCAnyHandle ownerHistory = CreateGenericOwnerHistory(exporterIFC, document, applicationHandle);
                exporterIFC.SetOwnerHistoryHandle(ownerHistory);

                IFCAnyHandle projectHandle = IFCInstanceExporter.CreateProject(file,
                    ExporterIFCUtils.CreateProjectLevelGUID(document, IFCProjectLevelGUIDType.Project), ownerHistory,
                    null, null, null, null, null, repContexts, units);
                exporterIFC.SetProject(projectHandle);

                ProjectInfo projInfo = document.ProjectInformation;
                string projectAddress = projInfo != null ? projInfo.Address : String.Empty;
                SiteLocation siteLoc = document.ActiveProjectLocation.SiteLocation;
                string location = siteLoc != null ? siteLoc.PlaceName : String.Empty;

                if (projectAddress == null)
                    projectAddress = String.Empty;
                if (location == null)
                    location = String.Empty;

                IFCAnyHandle buildingAddress = CreateIFCAddress(file, projectAddress, location);

                string buildingName = String.Empty;
                if (projInfo != null)
                {
                    try
                    {
                        buildingName = projInfo.BuildingName;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                    }
                }

                IFCAnyHandle buildingHandle = IFCInstanceExporter.CreateBuilding(file,
                    ExporterIFCUtils.CreateProjectLevelGUID(document, IFCProjectLevelGUIDType.Building),
                    ownerHistory, buildingName, null, null, buildingPlacement, null, buildingName,
                    Toolkit.IFCElementComposition.Element, null, null, buildingAddress);
                exporterIFC.SetBuilding(buildingHandle);

                // create levels
                List<Level> levels = LevelUtil.FindAllLevels(document);

                bool exportAllLevels = true;
                for (int ii = 0; ii < levels.Count && exportAllLevels; ii++)
                {
                    Level level = levels[ii];
                    Parameter isBuildingStorey = level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY);
                    if (isBuildingStorey == null || (isBuildingStorey.AsInteger() != 0))
                    {
                        exportAllLevels = false;
                        break;
                    }
                }

                IList<Element> unassignedBaseLevels = new List<Element>();

                ExporterCacheManager.ExportOptionsCache.ExportAllLevels = exportAllLevels;
                double scaleFactor = exporterIFC.LinearScale;
                    
                IFCAnyHandle prevBuildingStorey = null;
                IFCAnyHandle prevPlacement = null;
                double prevHeight = 0.0;
                double prevElev = 0.0;

                for (int ii = 0; ii < levels.Count; ii++)
                {
                    Level level = levels[ii];
                    if (level == null)
                        continue;

                    IFCLevelInfo levelInfo = null;

                    if (!LevelUtil.IsBuildingStory(level))
                    {
                        if (prevBuildingStorey == null)
                            unassignedBaseLevels.Add(level);
                        else
                        {
                            levelInfo = IFCLevelInfo.Create(prevBuildingStorey, prevPlacement, prevHeight, prevElev, scaleFactor, true);
                            ExporterCacheManager.LevelInfoCache.AddLevelInfo(exporterIFC, level.Id, levelInfo);
                        }
                        continue;
                    }

                    // When exporting to IFC 2x3, we have a limited capability to export some Revit view-specific
                    // elements, specifically Filled Regions and Text.  However, we do not have the
                    // capability to choose which views to export.  As such, we will choose (up to) one DBView per
                    // exported level.
                    // TODO: Let user choose which view(s) to export.  Ensure that the user know that only one view
                    // per level is supported.
                    View view = LevelUtil.FindViewByLevel(document, ViewType.FloorPlan, level);
                    if (view != null)
                    {
                        exporterIFC.AddViewIdToExport(view.Id, level.Id);
                    }

                    double elev = level.ProjectElevation;
                    double height = 0.0;
                    List<ElementId> coincidentLevels = new List<ElementId>();
                    for (int jj = ii+1; jj < levels.Count; jj++)
                    {
                        Level nextLevel = levels[jj];
                        if (!LevelUtil.IsBuildingStory(nextLevel))
                            continue;

                        double nextElev = nextLevel.ProjectElevation;
                        if (!MathUtil.IsAlmostEqual(nextElev, elev))
                        {
                            height = nextElev - elev;
                            break;
                        }
                        else if (ExporterCacheManager.ExportOptionsCache.WallAndColumnSplitting)
                            coincidentLevels.Add(nextLevel.Id);
                    }

                    double elevation = elev * scaleFactor;
                    XYZ orig = new XYZ(0.0, 0.0, elevation);

                    IFCAnyHandle axis2Placement3D = ExporterUtil.CreateAxis2Placement3D(file, orig);
                    IFCAnyHandle placement = IFCInstanceExporter.CreateLocalPlacement(file, buildingPlacement, axis2Placement3D);
                    string levelName = level.Name;
                    string levelGUID = LevelUtil.GetLevelGUID(level);
                    IFCAnyHandle buildingStorey = IFCInstanceExporter.CreateBuildingStorey(file,
                        levelGUID, exporterIFC.GetOwnerHistoryHandle(),
                        levelName, null, null, placement,
                        null, levelName, Toolkit.IFCElementComposition.Element, elevation);

                    // If we are using the R2009 Level GUIDs, write it to a shared paramter in the file to ensure that it is preserved.
                    if (ExporterCacheManager.ExportOptionsCache.Use2009BuildingStoreyGUIDs)
                    {
                        string oldLevelGUID;
                        ParameterUtil.GetStringValueFromElement(level, BuiltInParameter.IFC_GUID, out oldLevelGUID);
                        if (String.IsNullOrEmpty(oldLevelGUID))
                        {
                            ParameterUtil.SetStringParameter(level, BuiltInParameter.IFC_GUID, levelGUID);
                        }
                    }

                    if (prevBuildingStorey == null)
                    {
                        foreach (Level baseLevel in unassignedBaseLevels)
                        {
                            levelInfo = IFCLevelInfo.Create(buildingStorey, placement, height, elev, scaleFactor, true);
                            ExporterCacheManager.LevelInfoCache.AddLevelInfo(exporterIFC, baseLevel.Id, levelInfo);
                        }
                    }
                    prevBuildingStorey = buildingStorey;
                    prevPlacement = placement;
                    prevHeight = height;
                    prevElev = elev;

                    levelInfo = IFCLevelInfo.Create(buildingStorey, placement, height, elev, scaleFactor, true);
                    ExporterCacheManager.LevelInfoCache.AddLevelInfo(exporterIFC, level.Id, levelInfo);

                    // if we have coincident levels, add buildingstoreys for them but use the old handle.
                    for (int jj = 0; jj < coincidentLevels.Count; jj++)
                    {
                        level = levels[ii + jj + 1];
                        levelInfo = IFCLevelInfo.Create(buildingStorey, placement, height, elev, scaleFactor, true);
                        ExporterCacheManager.LevelInfoCache.AddLevelInfo(exporterIFC, level.Id, levelInfo);
                    }

                    ii += coincidentLevels.Count;
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Completes the export process by writing information stored incrementally during export to the file.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="document">The document to export.</param>
        private void EndExport(ExporterIFC exporterIFC, Document document)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                foreach (KeyValuePair<ElementId, StairRampContainerInfo> stairRamp in ExporterCacheManager.StairRampContainerInfoCache)
                {
                    StairRampContainerInfo stairRampInfo = stairRamp.Value;

                    IList<IFCAnyHandle> hnds = stairRampInfo.StairOrRampHandles;
                    for (int ii = 0; ii < hnds.Count; ii++)
                    {
                        IFCAnyHandle hnd = hnds[ii];
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(hnd))
                            continue;

                        IList<IFCAnyHandle> comps = stairRampInfo.Components[ii];
                        if (comps.Count == 0)
                            continue;

                        Element elem = document.GetElement(stairRamp.Key);
                        string guid = ExporterIFCUtils.CreateSubElementGUID(elem, (int) IFCStairSubElements.ContainmentRelation);
                        ExporterUtil.RelateObjects(exporterIFC, guid, hnd, comps);
                    }
                }
                
                ProjectInfo projectInfo = document.ProjectInformation;
                    
                // relate assembly elements to assemblies
                foreach (KeyValuePair<ElementId, AssemblyInstanceInfo> assemblyInfoEntry in ExporterCacheManager.AssemblyInstanceCache)
                {
                    AssemblyInstanceInfo assemblyInfo = assemblyInfoEntry.Value;
                    if (assemblyInfo == null)
                        continue;

                    if (assemblyInfo.AssemblyInstanceHandle != null && assemblyInfo.ElementHandles != null &&
                        assemblyInfo.ElementHandles.Count != 0)
                    {
                        Element assemblyInstance = document.GetElement(assemblyInfoEntry.Key);
                        string guid = ExporterIFCUtils.CreateSubElementGUID(assemblyInstance, (int)IFCAssemblyInstanceSubElements.RelContainedInSpatialStructure);
                        ExporterUtil.RelateObjects(exporterIFC, guid, assemblyInfo.AssemblyInstanceHandle, assemblyInfo.ElementHandles);
                    }
                }

                // create spatial structure holder
                ICollection<IFCAnyHandle> relatedElements = exporterIFC.GetRelatedElements();
                if (relatedElements.Count > 0)
                {
                    HashSet<IFCAnyHandle> relatedElementSet = new HashSet<IFCAnyHandle>(relatedElements);
                    IFCInstanceExporter.CreateRelContainedInSpatialStructure(file,
                        ExporterIFCUtils.CreateSubElementGUID(projectInfo, (int)IFCBuildingSubElements.RelContainedInSpatialStructure),
                        exporterIFC.GetOwnerHistoryHandle(), null, null, relatedElementSet, exporterIFC.GetBuilding());
                }

                ICollection<IFCAnyHandle> relatedProducts = exporterIFC.GetRelatedProducts();
                if (relatedProducts.Count > 0)
                {
                    string guid = ExporterIFCUtils.CreateSubElementGUID(projectInfo, (int)IFCBuildingSubElements.RelAggregatesProducts);
                    ExporterUtil.RelateObjects(exporterIFC, guid, exporterIFC.GetBuilding(), relatedProducts);
                }

                // create a default site if we have latitude and longitude information.
                if (IFCAnyHandleUtil.IsNullOrHasNoValue(exporterIFC.GetSite()))
                {
                    using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(exporterIFC, true))
                    {
                        SiteExporter.ExportDefaultSite(exporterIFC, document, productWrapper);
                    }
                }

                IFCAnyHandle siteHandle = exporterIFC.GetSite();
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(siteHandle))
                {
                    ExporterUtil.RelateObject(exporterIFC, exporterIFC.GetProject(), siteHandle);

                    // assoc. site to the building.
                    ExporterUtil.RelateObject(exporterIFC, siteHandle, exporterIFC.GetBuilding());

                    ExporterIFCUtils.UpdateBuildingPlacement(exporterIFC);
                }
                else
                {
                    // relate building and project if no site
                    ExporterUtil.RelateObject(exporterIFC, exporterIFC.GetProject(), exporterIFC.GetBuilding());
                }

                // relate levels and products.
                RelateLevels(exporterIFC, document);

                // These elements are created internally, but we allow custom property sets for them.  Create them here.
                using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(exporterIFC, true))
                {
                    IFCAnyHandle buildingHnd = exporterIFC.GetBuilding();
                    productWrapper.AddBuilding(buildingHnd);
                    ExportElementProperties(exporterIFC, document.ProjectInformation, productWrapper);
                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, document.ProjectInformation, productWrapper);
                }

                // create material layer associations
                foreach (IFCAnyHandle materialSetLayerUsageHnd in ExporterCacheManager.MaterialLayerRelationsCache.Keys)
                {
                    IFCInstanceExporter.CreateRelAssociatesMaterial(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        null, null, ExporterCacheManager.MaterialLayerRelationsCache[materialSetLayerUsageHnd],
                        materialSetLayerUsageHnd);
                }

                // create material associations
                foreach (IFCAnyHandle materialHnd in ExporterCacheManager.MaterialRelationsCache.Keys)
                {
                    IFCInstanceExporter.CreateRelAssociatesMaterial(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        null, null, ExporterCacheManager.MaterialRelationsCache[materialHnd], materialHnd);
                }

                // create type relations
                foreach (IFCAnyHandle typeObj in ExporterCacheManager.TypeRelationsCache.Keys)
                {
                    IFCInstanceExporter.CreateRelDefinesByType(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        null, null,  ExporterCacheManager.TypeRelationsCache[typeObj], typeObj);
                }

                // create type property relations
                foreach (TypePropertyInfo typePropertyInfo in ExporterCacheManager.TypePropertyInfoCache.Values)
                {
                    HashSet<IFCAnyHandle> propertySets = typePropertyInfo.PropertySets;
                    HashSet<IFCAnyHandle> elements = typePropertyInfo.Elements;

                    foreach (IFCAnyHandle propertySet in propertySets)
                    {
                        IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                            null, null, elements, propertySet);
                    }
                }

                // create space boundaries
                foreach (SpaceBoundary boundary in ExporterCacheManager.SpaceBoundaryCache)
                {
                    SpatialElementExporter.ProcessIFCSpaceBoundary(exporterIFC, boundary, file);
                }

                // create wall/wall connectivity objects
                if (ExporterCacheManager.WallConnectionDataCache.Count > 0)
                {
                    IList<IDictionary<ElementId, IFCAnyHandle>> hostObjects = exporterIFC.GetHostObjects();
                    List<int> relatingPriorities = new List<int>();
                    List<int> relatedPriorities = new List<int>();

                    foreach (WallConnectionData wallConnectionData in ExporterCacheManager.WallConnectionDataCache)
                    {
                        foreach (IDictionary<ElementId, IFCAnyHandle> mapForLevel in hostObjects)
                        {
                            IFCAnyHandle wallElementHandle, otherElementHandle;
                            if (!mapForLevel.TryGetValue(wallConnectionData.FirstId, out wallElementHandle))
                                continue;
                            if (!mapForLevel.TryGetValue(wallConnectionData.SecondId, out otherElementHandle))
                                continue;

                            // NOTE: Definition of RelConnectsPathElements has the connection information reversed
                            // with respect to the order of the paths.
                            IFCInstanceExporter.CreateRelConnectsPathElements(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                null, null, wallConnectionData.ConnectionGeometry, wallElementHandle, otherElementHandle, relatingPriorities,
                                relatedPriorities, wallConnectionData.SecondConnectionType, wallConnectionData.FirstConnectionType);
                        }
                    }
                }

                // create Zones
                {
                    string relAssignsToGroupName = "Spatial Zone Assignment";
                    foreach (string zoneName in ExporterCacheManager.ZoneInfoCache.Keys)
                    {
                        ZoneInfo zoneInfo = ExporterCacheManager.ZoneInfoCache.Find(zoneName);
                        if (zoneInfo != null)
                        {
                            IFCAnyHandle zoneHandle = IFCInstanceExporter.CreateZone(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                zoneName, zoneInfo.ObjectType, zoneInfo.Description);
                            IFCInstanceExporter.CreateRelAssignsToGroup(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                relAssignsToGroupName, null, zoneInfo.RoomHandles, null, zoneHandle);

                            HashSet<IFCAnyHandle> zoneHnds = new HashSet<IFCAnyHandle>();
                            zoneHnds.Add(zoneHandle);
                                
                            foreach (KeyValuePair<string, IFCAnyHandle> classificationReference in zoneInfo.ClassificationReferences)
                            {
                                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                                    exporterIFC.GetOwnerHistoryHandle(), classificationReference.Key, "", zoneHnds, classificationReference.Value);
                            }

                            if (zoneInfo.EnergyAnalysisProperySetHandle != null && zoneInfo.EnergyAnalysisProperySetHandle.HasValue)
                            {
                                IFCAnyHandle relHnd = IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(),
                                    exporterIFC.GetOwnerHistoryHandle(), null, null, zoneHnds, zoneInfo.EnergyAnalysisProperySetHandle);
                            }

                        }
                    }
                }

                // create Space Occupants
                {
                    foreach (string spaceOccupantName in ExporterCacheManager.SpaceOccupantInfoCache.Keys)
                    {
                        SpaceOccupantInfo spaceOccupantInfo = ExporterCacheManager.SpaceOccupantInfoCache.Find(spaceOccupantName);
                        if (spaceOccupantInfo != null)
                        {
                            IFCAnyHandle person = IFCInstanceExporter.CreatePerson(file, null, spaceOccupantName, null, null, null, null, null, null);
                            IFCAnyHandle spaceOccupantHandle = IFCInstanceExporter.CreateOccupant(file, ExporterIFCUtils.CreateGUID(), 
                                exporterIFC.GetOwnerHistoryHandle(), null, null, spaceOccupantName, person, IFCOccupantType.NotDefined);
                            IFCInstanceExporter.CreateRelOccupiesSpaces(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                null, null, spaceOccupantInfo.RoomHandles, null, spaceOccupantHandle, null);

                            HashSet<IFCAnyHandle> spaceOccupantHandles = new HashSet<IFCAnyHandle>();
                            spaceOccupantHandles.Add(spaceOccupantHandle);

                            foreach (KeyValuePair<string, IFCAnyHandle> classificationReference in spaceOccupantInfo.ClassificationReferences)
                            {
                                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                                    exporterIFC.GetOwnerHistoryHandle(), classificationReference.Key, "", spaceOccupantHandles, classificationReference.Value);
                            }

                            if (spaceOccupantInfo.SpaceOccupantProperySetHandle != null && spaceOccupantInfo.SpaceOccupantProperySetHandle.HasValue)
                            {
                                IFCAnyHandle relHnd = IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(),
                                    exporterIFC.GetOwnerHistoryHandle(), null, null, spaceOccupantHandles, spaceOccupantInfo.SpaceOccupantProperySetHandle);
                            }
                        }
                    }
                }

                ExporterIFCUtils.EndExportInternal(exporterIFC);

                //create header

                ExportOptionsCache exportOptionsCache = ExporterCacheManager.ExportOptionsCache;

                string coordinationView = null;
                if (exportOptionsCache.ExportAs2x3CoordinationView2)
                    coordinationView = "CoordinationView_V2.0";
                else
                    coordinationView = "CoordinationView";

                List<string> descriptions = new List<string>();
                if (exportOptionsCache.ExportAs2x2 || ExporterUtil.DoCodeChecking(exportOptionsCache))
                {
                    descriptions.Add("IFC2X_PLATFORM");
                }
                else
                {
                    string currentLine;
                    currentLine = string.Format("ViewDefinition [{0}{1}]",
                       coordinationView,
                       exportOptionsCache.ExportBaseQuantities ? ", QuantityTakeOffAddOnView" : "");

                    descriptions.Add(currentLine);
                }

                string projectNumber = projectInfo.Number;
                string projectName = projectInfo.Name;
                string projectStatus = projectInfo.Status;

                if (projectNumber == null)
                    projectNumber = string.Empty;
                if (projectName == null)
                    projectName = exportOptionsCache.FileName;
                if (projectStatus == null)
                    projectStatus = string.Empty;

                IFCAnyHandle project = exporterIFC.GetProject();
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(project))
                    IFCAnyHandleUtil.UpdateProject(project, projectNumber, projectName, projectStatus);

                List<string> author = new List<string>();
                author.Add(string.Empty);
                List<string> orginization = new List<string>();
                orginization.Add(string.Empty);
                IFCInstanceExporter.CreateFileSchema(file);
                IFCInstanceExporter.CreateFileDescription(file, descriptions);
                IFCInstanceExporter.CreateFileName(file, projectNumber, author, orginization, document.Application.VersionName,
                    document.Application.VersionBuild, string.Empty);

                transaction.Commit();

                IFCFileWriteOptions writeOptions = new IFCFileWriteOptions();
                writeOptions.FileName = exportOptionsCache.FileName;
                writeOptions.FileFormat = exportOptionsCache.IFCFileFormat;
                if (writeOptions.FileFormat == IFCFileFormat.IfcXML || writeOptions.FileFormat == IFCFileFormat.IfcXMLZIP)
                {
                    writeOptions.XMLConfigFileName = Path.Combine(ExporterUtil.RevitProgramPath, "EDM\\ifcXMLconfiguration.xml");
                }
                file.Write(writeOptions);
            }
        }

        /// <summary>
        /// Some elements may not have the right structure to support stable GUIDs for some property sets.  Ignore the index for these cases.
        /// </summary>
        private static int CheckElementTypeValidityForSubIndex(PropertySetDescription currDesc, IFCAnyHandle handle, Element element)
        {
            int originalIndex = currDesc.SubElementIndex;
            if (originalIndex > 0)
            {
                if (IFCAnyHandleUtil.IsSubTypeOf(handle, IFCEntityType.IfcSlab) || IFCAnyHandleUtil.IsSubTypeOf(handle, IFCEntityType.IfcStairFlight))
                {
                    if (StairsExporter.IsLegacyStairs(element))
                    {
                        return 0;
                    }
                }
            }
            return originalIndex;
        }

        /// <summary>
        /// Exports the element properties.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element ">The element whose properties are exported.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        internal void ExportElementProperties(ExporterIFC exporterIFC, Element element, IFCProductWrapper productWrapper)
        {
            if (productWrapper.Count == 0)
                return;

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                Document doc = element.Document;

                ElementType elemType = doc.GetElement(element.GetTypeId()) as ElementType;

                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                ICollection<IFCAnyHandle> productSet = productWrapper.GetAllObjects();
                IList<IList<PropertySetDescription>> psetsToCreate = ExporterCacheManager.ParameterCache.PropertySets;

                foreach (IFCAnyHandle prodHnd in productSet)
                {
                    IFCEntityType prodHndType = IFCAnyHandleUtil.GetEntityType(prodHnd);
                    IList<PropertySetDescription> currPsetsToCreate = null;

                    if (!ExporterCacheManager.PropertySetsForTypeCache.TryGetValue(prodHndType, out currPsetsToCreate))
                    {
                        currPsetsToCreate = new List<PropertySetDescription>();

                        IList<PropertySetDescription> unconditionalPsetsToCreate = new List<PropertySetDescription>();
                        IList<PropertySetDescription> conditionalPsetsToCreate = new List<PropertySetDescription>();

                        foreach (IList<PropertySetDescription> currStandard in psetsToCreate)
                        {
                            foreach (PropertySetDescription currDesc in currStandard)
                            {
                                if (currDesc.IsAppropriateEntityType(prodHnd))
                                {
                                    if (currDesc.IsAppropriateObjectType(prodHnd))
                                        currPsetsToCreate.Add(currDesc);

                                    if (currDesc.ObjectType == "")
                                        unconditionalPsetsToCreate.Add(currDesc);
                                    else
                                        conditionalPsetsToCreate.Add(currDesc);
                                }
                            }
                        }
                        ExporterCacheManager.PropertySetsForTypeCache[prodHndType] = unconditionalPsetsToCreate;
                        ExporterCacheManager.ConditionalPropertySetsForTypeCache[prodHndType] = conditionalPsetsToCreate;
                    }
                    else
                    {
                        IList<PropertySetDescription> conditionalPsetsToCreate = 
                            ExporterCacheManager.ConditionalPropertySetsForTypeCache[prodHndType];
                        foreach (PropertySetDescription currDesc in conditionalPsetsToCreate)
                        {
                            if (currDesc.IsAppropriateObjectType(prodHnd))
                                currPsetsToCreate.Add(currDesc);
                        }
                    }

                    if (currPsetsToCreate.Count == 0)
                        continue;

                    ElementId overrideElementId = ExporterCacheManager.HandleToElementCache.Find(prodHnd);
                    Element elementToUse = (overrideElementId == ElementId.InvalidElementId) ? element : doc.GetElement(overrideElementId);
                    ElementType elemTypeToUse = (overrideElementId == ElementId.InvalidElementId) ? elemType : doc.GetElement(elementToUse.GetTypeId()) as ElementType;
                    if (elemTypeToUse == null)
                        elemTypeToUse = elemType;

                    IFCExtrusionCreationData ifcParams = productWrapper.FindExtrusionCreationParameters(prodHnd);

                    foreach (PropertySetDescription currDesc in currPsetsToCreate)
                    {
                        HashSet<IFCAnyHandle> props = new HashSet<IFCAnyHandle>();
                        IList<PropertySetEntry> entries = currDesc.Entries;
                        foreach (PropertySetEntry entry in entries)
                        {
                            IFCAnyHandle propHnd = entry.ProcessEntry(file, exporterIFC, ifcParams, elementToUse, elemTypeToUse);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                                props.Add(propHnd);
                        }

                        if (props.Count > 0)
                        {
                            int subElementIndex = CheckElementTypeValidityForSubIndex(currDesc, prodHnd, element);

                            string guid = null;
                            if (subElementIndex > 0)
                            {
                                guid = ExporterIFCUtils.CreateSubElementGUID(elementToUse, subElementIndex);
                            }
                            else
                            {
                                guid = ExporterIFCUtils.CreateGUID();
                            }

                            string paramSetName = currDesc.Name;
                            IFCAnyHandle propertySet = IFCInstanceExporter.CreatePropertySet(file, guid, ownerHistory, paramSetName, null, props);
                            IFCAnyHandle prodHndToUse = prodHnd;
                            DescriptionCalculator ifcRDC = currDesc.DescriptionCalculator;
                            if (ifcRDC != null)
                            {
                                IFCAnyHandle overrideHnd = ifcRDC.RedirectDescription(exporterIFC, elementToUse);
                                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(overrideHnd))
                                    prodHndToUse = overrideHnd;
                            }
                            HashSet<IFCAnyHandle> relatedObjects = new HashSet<IFCAnyHandle>();
                            relatedObjects.Add(prodHndToUse);
                            IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(), ownerHistory, null, null, relatedObjects, propertySet);
                        }
                    }
                }
                transaction.Commit();
            }

            if (exporterIFC.ExportAs2x2)
                ExportPsetDraughtingFor2x2(exporterIFC, element, productWrapper);
        }

        /// <summary>
        /// Exports Pset_Draughting for IFC 2x2 standard.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element ">The element whose properties are exported.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        void ExportPsetDraughtingFor2x2(ExporterIFC exporterIFC, Element element, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                string catName = CategoryUtil.GetCategoryName(element);
                Color color = CategoryUtil.GetElementColor(element);


                HashSet<IFCAnyHandle> nameAndColorProps = new HashSet<IFCAnyHandle>();

                nameAndColorProps.Add(PropertyUtil.CreateLabelPropertyFromCache(file, "Layername", catName, PropertyValueType.SingleValue, true));

                //color
                {
                    HashSet<IFCAnyHandle> colorProps = new HashSet<IFCAnyHandle>();
                    colorProps.Add(PropertyUtil.CreateIntegerPropertyFromCache(file, "Red", color.Red, PropertyValueType.SingleValue));
                    colorProps.Add(PropertyUtil.CreateIntegerPropertyFromCache(file, "Green", color.Green, PropertyValueType.SingleValue));
                    colorProps.Add(PropertyUtil.CreateIntegerPropertyFromCache(file, "Blue", color.Blue, PropertyValueType.SingleValue));

                    string propertyName = "Color";
                    nameAndColorProps.Add(IFCInstanceExporter.CreateComplexProperty(file, propertyName, null, propertyName, colorProps));
                }

                string name = "Pset_Draughting";   // IFC 2x2 standard
                IFCAnyHandle propertySet2 = IFCInstanceExporter.CreatePropertySet(file, ExporterIFCUtils.CreateGUID(), ownerHistory, name, null, nameAndColorProps);

                HashSet<IFCAnyHandle> relatedObjects = new HashSet<IFCAnyHandle>(productWrapper.GetAllObjects());
                IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(), ownerHistory, null, null, relatedObjects, propertySet2);

                transaction.Commit();
            }
        }

        /// <summary>
        /// Exports the IFC element quantities.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element ">The element whose quantities are exported.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        internal void ExportElementQuantities(ExporterIFC exporterIFC, Element element, IFCProductWrapper productWrapper)
        {
            if (productWrapper.Count == 0)
                return;

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                Document doc = element.Document;

                ElementType elemType = doc.GetElement(element.GetTypeId()) as ElementType;

                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                ICollection<IFCAnyHandle> productSet = productWrapper.GetAllObjects();
                IList<IList<QuantityDescription>> quantitiesToCreate = ExporterCacheManager.ParameterCache.Quantities;

                foreach (IList<QuantityDescription> currStandard in quantitiesToCreate)
                {
                    foreach (QuantityDescription currDesc in currStandard)
                    {
                        foreach (IFCAnyHandle prodHnd in productSet)
                        {
                            if (currDesc.IsAppropriateType(prodHnd))
                            {
                                IFCExtrusionCreationData ifcParams = productWrapper.FindExtrusionCreationParameters(prodHnd);

                                HashSet<IFCAnyHandle> quantities = new HashSet<IFCAnyHandle>();
                                IList<QuantityEntry> entries = currDesc.Entries;
                                foreach (QuantityEntry entry in entries)
                                {
                                    IFCAnyHandle quanHnd = entry.ProcessEntry(file, exporterIFC, ifcParams, element, elemType);
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(quanHnd))
                                        quantities.Add(quanHnd);
                                }

                                string paramSetName = currDesc.Name;

                                string methodName = currDesc.MethodOfMeasurement;

                                if (quantities.Count > 0)
                                {
                                    IFCAnyHandle propertySet = IFCInstanceExporter.CreateElementQuantity(file, ExporterIFCUtils.CreateGUID(), ownerHistory, paramSetName, methodName, null, quantities);
                                    IFCAnyHandle prodHndToUse = prodHnd;
                                    DescriptionCalculator ifcRDC = currDesc.DescriptionCalculator;
                                    if (ifcRDC != null)
                                    {
                                        IFCAnyHandle overrideHnd = ifcRDC.RedirectDescription(exporterIFC, element);
                                        if (!IFCAnyHandleUtil.IsNullOrHasNoValue(overrideHnd))
                                            prodHndToUse = overrideHnd;
                                    }
                                    HashSet<IFCAnyHandle> relatedObjects = new HashSet<IFCAnyHandle>();
                                    relatedObjects.Add(prodHndToUse);
                                    IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(), ownerHistory, null, null, relatedObjects, propertySet);
                                }
                            }
                        }
                    }
                }
                transaction.Commit();
            }
        }

        /// <summary>Exports the element classification(s)./// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="element">The element whose classifications are exported.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        internal void ExportElementClassifications(ExporterIFC exporterIFC, Element element, IFCProductWrapper productWrapper)
        {
            if (productWrapper.Count == 0)
                return;

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                ICollection<IFCAnyHandle> productSet = productWrapper.GetAllObjects();
                foreach (IFCAnyHandle prodHnd in productSet)
                {
                    if (IFCAnyHandleUtil.IsSubTypeOf(prodHnd, IFCEntityType.IfcElement))
                        ClassificationUtil.CreateUniformatClassification(exporterIFC, file, element, prodHnd);
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Creates the application information.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="app">The application object.</param>
        /// <returns>The handle of IFC file.</returns>
        private IFCAnyHandle CreateApplicationInformation(IFCFile file, Application app)
        {
            string productFullName = app.VersionName;
            string productVersion = app.VersionNumber;
            string productIdentifier = "Revit";

            IFCAnyHandle developer = IFCInstanceExporter.CreateOrganization(file, null, productFullName, null, null, null);
            IFCAnyHandle application = IFCInstanceExporter.CreateApplication(file, developer, productVersion, productFullName, productIdentifier);
            return application;
        }

        /// <summary>
        /// Creates the 3D and 2D contexts information.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="doc">The document provides the ProjectLocation.</param>
        /// <returns>The collection contains the 3D/2D context (not sub-context) handles of IFC file.</returns>
        private HashSet<IFCAnyHandle> CreateContextInformation(ExporterIFC exporterIFC, Document doc)
        {
            HashSet<IFCAnyHandle> repContexts = new HashSet<IFCAnyHandle>();
            double scale = exporterIFC.LinearScale;
            double precision = MathUtil.Eps();
            if (scale > 1.0 + precision)
            {
                int exponent = ((int)(Math.Log10(scale) - 0.01)) + 1;
                precision *= Math.Pow(10.0, exponent);
            }
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle origin = ExporterIFCUtils.GetGlobal3DOriginHandle();
            IFCAnyHandle wcs = IFCInstanceExporter.CreateAxis2Placement3D(file, origin, null, null);

            ProjectLocation projLoc = doc.ActiveProjectLocation;
            double trueNorthAngleInRadians = 0.0;
            try
            {
                ProjectPosition projPos = projLoc.get_ProjectPosition(XYZ.Zero);
                trueNorthAngleInRadians = projPos.Angle;
            }
            catch (InternalException)
            {
                //fail to get true north, ignore
            }

            // CoordinationView2.0 requires that we always export true north, even if it is the same as project north.
            IFCAnyHandle trueNorth = null;
            {
                double trueNorthAngleConverted = -trueNorthAngleInRadians + Math.PI / 2.0;
                List<double> dirRatios = new List<double>();
                dirRatios.Add(2);
                dirRatios.Add(Math.Cos(trueNorthAngleConverted));
                dirRatios.Add(Math.Sin(trueNorthAngleConverted));
                trueNorth = IFCInstanceExporter.CreateDirection(file, dirRatios);
            }

            int dimCount = 3;
            IFCAnyHandle context3D = IFCInstanceExporter.CreateGeometricRepresentationContext(file, null,
                "Model", dimCount, precision, wcs, trueNorth);
            // CoordinationView2.0 requires sub-contexts of "Axis", "Body", and "Box".  We will use these for regular export also.
            {
                IFCAnyHandle context3DAxis = IFCInstanceExporter.CreateGeometricRepresentationSubContext(file,
                    "Axis", "Model", context3D, null, Toolkit.IFCGeometricProjection.Graph_View, null);
                IFCAnyHandle context3DBody = IFCInstanceExporter.CreateGeometricRepresentationSubContext(file,
                    "Body", "Model", context3D, null, Toolkit.IFCGeometricProjection.Model_View, null);
                IFCAnyHandle context3DBox = IFCInstanceExporter.CreateGeometricRepresentationSubContext(file,
                    "Box", "Model", context3D, null, Toolkit.IFCGeometricProjection.Model_View, null);
                IFCAnyHandle context3DFootPrint = IFCInstanceExporter.CreateGeometricRepresentationSubContext(file,
                    "FootPrint", "Model", context3D, null, Toolkit.IFCGeometricProjection.Model_View, null);

                exporterIFC.Set3DContextHandle(context3DAxis, "Axis");
                exporterIFC.Set3DContextHandle(context3DBody, "Body");
                exporterIFC.Set3DContextHandle(context3DBox, "Box");
                exporterIFC.Set3DContextHandle(context3DFootPrint, "FootPrint");
            }

            exporterIFC.Set3DContextHandle(context3D, "");
            repContexts.Add(context3D); // Only Contexts in list, not sub-contexts.

            if (ExporterCacheManager.ExportOptionsCache.ExportAnnotations)
            {
                string context2DType = "Annotation";
                IFCAnyHandle context2DHandle = IFCInstanceExporter.CreateGeometricRepresentationContext(file,
                    null, context2DType, dimCount, precision, wcs, trueNorth);

                IFCAnyHandle context2D = IFCInstanceExporter.CreateGeometricRepresentationSubContext(file,
                    null, context2DType, context2DHandle, 0.01, Toolkit.IFCGeometricProjection.Plan_View, null);

                exporterIFC.Set2DContextHandle(context2D);
                repContexts.Add(context2DHandle); // Only Contexts in list, not sub-contexts.
            }

            return repContexts;
        }

        /// <summary>
        /// Creates the IfcOwnerHistory.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="doc">The document provides the owner information.</param>
        /// <param name="application">The handle of IFC file to create the owner history.</param>
        /// <returns>The handle.</returns>
        private IFCAnyHandle CreateGenericOwnerHistory(ExporterIFC exporterIFC, Document doc, IFCAnyHandle application)
        {
            string familyName;
            string givenName;
            List<string> middleNames;
            List<string> prefixTitles;
            List<string> suffixTitles;

            string author = String.Empty;
            ProjectInfo projInfo = doc.ProjectInformation;
            if (projInfo != null)
            {
                try
                {
                    author = projInfo.Author;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    //if failed to get author from project info, try to get the username from application later.
                }
            }

            if (String.IsNullOrEmpty(author))
            {
                author = doc.Application.Username;
            }

            NamingUtil.ParseName(author, out familyName, out givenName, out middleNames, out prefixTitles, out suffixTitles);
            
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle person = IFCInstanceExporter.CreatePerson(file, null, familyName, givenName, middleNames,
                prefixTitles, suffixTitles, null, null);

            string organizationName = String.Empty;
            string organizationDescription = String.Empty;
            if (projInfo != null)
            {
                try
                {
                    organizationName = projInfo.OrganizationName;
                    organizationDescription = projInfo.OrganizationDescription;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                }
            }

            IFCAnyHandle organization = IFCInstanceExporter.CreateOrganization(file, null, organizationName, organizationDescription,
                null, null);

            IFCAnyHandle owningUser = IFCInstanceExporter.CreatePersonAndOrganization(file, person, organization, null);
            IFCAnyHandle ownerHistory = IFCInstanceExporter.CreateOwnerHistory(file, owningUser, application, null,
                Toolkit.IFCChangeAction.NoChange, null, null, null, 0);

            return ownerHistory;
        }

        /// <summary>
        /// Creates the IfcPostalAddress, and assigns it to the file.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="address">The address string.</param>
        /// <param name="town">The town string.</param>
        /// <returns>The handle of IFC file.</returns>
        private IFCAnyHandle CreateIFCAddress(IFCFile file, string address, string town)
        {
            List<string> parsedAddress = new List<string>();
            string city = String.Empty;
            string state = String.Empty;
            string postCode = String.Empty;
            string country = String.Empty;

            string parsedTown = town;
            int commaLoc = -1;
            do
            {
                commaLoc = parsedTown.IndexOf(',');
                if (commaLoc >= 0)
                {
                    if (commaLoc > 0)
                        parsedAddress.Add(parsedTown.Substring(0, commaLoc));
                    parsedTown = parsedTown.Substring(commaLoc + 1).TrimStart(' ');
                }
                else if (!String.IsNullOrEmpty(parsedTown))
                    parsedAddress.Add(parsedTown);
            } while (commaLoc >= 0);

            int numLines = parsedAddress.Count;
            if (numLines > 0)
            {
                country = parsedAddress[numLines - 1];
                numLines--;
            }

            if (numLines > 0)
            {
                int spaceLoc = parsedAddress[numLines - 1].IndexOf(' ');
                if (spaceLoc > 0)
                {
                    state = parsedAddress[numLines - 1].Substring(0, spaceLoc);
                    postCode = parsedAddress[numLines - 1].Substring(spaceLoc + 1);
                }
                else
                    state = parsedAddress[numLines - 1];
                numLines--;
            }

            if (numLines > 0)
            {
                city = parsedAddress[numLines - 1];
                numLines--;
            }

            List<string> addressLines = new List<string>();
            if (!String.IsNullOrEmpty(address))
                addressLines.Add(address);

            for (int ii = 0; ii < numLines; ii++)
            {
                addressLines.Add(parsedAddress[ii]);
            }


            IFCAnyHandle postalAddress = IFCInstanceExporter.CreatePostalAddress(file, null, null, null,
               null, addressLines, null, city, state, postCode, country);

            return postalAddress;
        }

        /// <summary>
        /// Creates the IfcUnitAssignment.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="doc">The document provides ProjectUnit and DisplayUnitSystem.</param>
        /// <returns>The IFC handle.</returns>
        private IFCAnyHandle CreateDefaultUnits(ExporterIFC exporterIFC, Document doc)
        {
            HashSet<IFCAnyHandle> unitSet = new HashSet<IFCAnyHandle>();
            IFCFile file = exporterIFC.GetFile();
            bool exportToCOBIE = ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE;
            {
                bool conversionBased = false;

                IFCUnit lenUnitType = IFCUnit.LengthUnit;
                IFCUnit areaUnitType = IFCUnit.AreaUnit;
                IFCUnit volUnitType = IFCUnit.VolumeUnit;

                IFCSIPrefix? prefix = null;
                IFCSIUnitName lenUnitName = IFCSIUnitName.Metre;
                IFCSIUnitName areaUnitName = IFCSIUnitName.Square_Metre;
                IFCSIUnitName volUnitName = IFCSIUnitName.Cubic_Metre;

                string lenConvName = null;
                string areaConvName = null;
                string volConvName = null;

                double factor = 1.0;
                double partialScaleFactor = 1.0;

                FormatOptions formatOptions = doc.ProjectUnit.get_FormatOptions(UnitType.UT_Length);

                switch (formatOptions.Units)
                {
                    case DisplayUnitType.DUT_METERS:
                    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                        break;
                    case DisplayUnitType.DUT_CENTIMETERS:
                        prefix = IFCSIPrefix.Centi;
                        partialScaleFactor = 100.0;
                        break;
                    case DisplayUnitType.DUT_MILLIMETERS:
                        prefix = IFCSIPrefix.Milli;
                        partialScaleFactor = 1000.0;
                        break;
                    case DisplayUnitType.DUT_DECIMAL_FEET:
                    case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                        {
                            if (exportToCOBIE)
                            {
                                lenConvName = "foot";
                                areaConvName = "foot";
                                volConvName = "foot";
                            }
                            else
                            {
                                lenConvName = "FOOT";
                                areaConvName = "SQUARE FOOT";
                                volConvName = "CUBIC FOOT";
                            }
                            factor = 0.3048;
                            conversionBased = true;
                        }
                        break;
                    case DisplayUnitType.DUT_DECIMAL_INCHES:
                        {
                            if (exportToCOBIE)
                            {
                                lenConvName = "inch";
                                areaConvName = "inch";
                                volConvName = "inch";
                            }
                            else
                            {
                                lenConvName = "INCH";
                                areaConvName = "SQUARE INCH";
                                volConvName = "CUBIC INCH";
                            }
                        }
                        factor = 0.0254;
                        partialScaleFactor = 12.0;
                        conversionBased = true;
                        break;
                    default:
                        {
                            //Couldn't find display unit type conversion -- assuming foot
                            if (exportToCOBIE)
                            {
                                lenConvName = "foot";
                                areaConvName = "foot";
                                volConvName = "foot";
                            }
                            else
                            {
                                lenConvName = "FOOT";
                                areaConvName = "SQUARE FOOT";
                                volConvName = "CUBIC FOOT";
                            }
                            factor = 0.3048;
                            conversionBased = true;
                        }
                        break;
                }

                double scaleFactor = 0.0;
                switch (doc.DisplayUnitSystem)
                {
                    case DisplayUnit.METRIC:
                        scaleFactor = partialScaleFactor * ExporterIFCUtils.ConvertUnits(doc, 1.0, DisplayUnitType.DUT_METERS);
                        break;
                    case DisplayUnit.IMPERIAL:
                        scaleFactor = partialScaleFactor * ExporterIFCUtils.ConvertUnits(doc, 1.0, DisplayUnitType.DUT_DECIMAL_FEET);
                        break;
                    default:
                        //Invalid display unit system -- assuming imperial
                        scaleFactor = ExporterIFCUtils.ConvertUnits(doc, 1.0, DisplayUnitType.DUT_DECIMAL_FEET);
                        break;
                }
                exporterIFC.LinearScale = scaleFactor;

                IFCAnyHandle lenSiUnit = IFCInstanceExporter.CreateSIUnit(file, lenUnitType, prefix, lenUnitName);
                IFCAnyHandle areaSiUnit = IFCInstanceExporter.CreateSIUnit(file, areaUnitType, prefix, areaUnitName);
                IFCAnyHandle volSiUnit = IFCInstanceExporter.CreateSIUnit(file, volUnitType, prefix, volUnitName);

                if (conversionBased)
                {
                    IFCAnyHandle lenDims = IFCInstanceExporter.CreateDimensionalExponents(file, 1, 0, 0, 0, 0, 0, 0); // length
                    IFCAnyHandle lenConvFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor), lenSiUnit);
                    lenSiUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, lenDims, lenUnitType, lenConvName, lenConvFactor);

                    IFCAnyHandle areaDims = IFCInstanceExporter.CreateDimensionalExponents(file, 2, 0, 0, 0, 0, 0, 0); // area
                    IFCAnyHandle areaConvFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor * factor), areaSiUnit);
                    areaSiUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, areaDims, areaUnitType, areaConvName, areaConvFactor);

                    IFCAnyHandle volDims = IFCInstanceExporter.CreateDimensionalExponents(file, 3, 0, 0, 0, 0, 0, 0); // volume
                    IFCAnyHandle volConvFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor * factor * factor), volSiUnit);
                    volSiUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, volDims, volUnitType, volConvName, volConvFactor);
                }

                unitSet.Add(lenSiUnit);      // created above, so unique.
                unitSet.Add(areaSiUnit);      // created above, so unique.
                unitSet.Add(volSiUnit);      // created above, so unique.
            }

            // Plane angle unit -- support degrees only.
            {
                IFCUnit unitType = IFCUnit.PlaneAngleUnit;
                IFCSIUnitName unitName = IFCSIUnitName.Radian;

                IFCAnyHandle planeAngleSIUnit = IFCInstanceExporter.CreateSIUnit(file, unitType, null, unitName);

                IFCAnyHandle dims = IFCInstanceExporter.CreateDimensionalExponents(file, 0, 0, 0, 0, 0, 0, 0);
                double factor = Math.PI / 180; // --> degrees to radians
                string convName = "DEGREE";

                IFCAnyHandle convFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor), planeAngleSIUnit);
                IFCAnyHandle planeAngleUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, dims, unitType, convName, convFactor);
                unitSet.Add(planeAngleUnit);      // created above, so unique.
            }

            // Time -- support seconds only.
            IFCAnyHandle timeSIUnit = null;
            {
                IFCUnit unitType = IFCUnit.TimeUnit;
                IFCSIUnitName unitName = IFCSIUnitName.Second;

                timeSIUnit = IFCInstanceExporter.CreateSIUnit(file, unitType, null, unitName);
                unitSet.Add(timeSIUnit);      // created above, so unique.
            }


            // GSA only units.
            if (exportToCOBIE)
            {
                // Mass
                {
                    IFCUnit unitType = IFCUnit.MassUnit;
                    IFCSIPrefix prefix = IFCSIPrefix.Kilo;
                    IFCSIUnitName unitName = IFCSIUnitName.Gram;

                    IFCAnyHandle massSIUnit = IFCInstanceExporter.CreateSIUnit(file, unitType, prefix, unitName);

                    IFCAnyHandle dims = IFCInstanceExporter.CreateDimensionalExponents(file, 0, 1, 0, 0, 0, 0, 0);
                    double factor = 0.45359237; // --> pound to kilogram
                    string convName = "pound";

                    IFCAnyHandle convFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor), massSIUnit);
                    IFCAnyHandle massUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, dims, unitType, convName, convFactor);
                    unitSet.Add(massUnit);      // created above, so unique.
                }

                // Illuminance
                {
                    IFCUnit unitType = IFCUnit.IlluminanceUnit;
                    IFCSIUnitName unitName = IFCSIUnitName.Lux;

                    IFCAnyHandle luxSIUnit = IFCInstanceExporter.CreateSIUnit(file, unitType, null, unitName);
                    unitSet.Add(luxSIUnit);      // created above, so unique.
                    ExporterCacheManager.UnitsCache["LUX"] = luxSIUnit;
                }

                // Air Changes per Hour
                {
                    IFCUnit unitType = IFCUnit.TimeUnit;
                    IFCAnyHandle dims = IFCInstanceExporter.CreateDimensionalExponents(file, 0, 0, -1, 0, 0, 0, 0);
                    double factor = 1.0 / 3600.0; // --> seconds to hours
                    string convName = "ACH";

                    IFCAnyHandle convFactor = IFCInstanceExporter.CreateMeasureWithUnit(file, Toolkit.IFCDataUtil.CreateAsRatioMeasure(factor), timeSIUnit);
                    IFCAnyHandle achUnit = IFCInstanceExporter.CreateConversionBasedUnit(file, dims, unitType, convName, convFactor);
                    unitSet.Add(achUnit);      // created above, so unique.
                    ExporterCacheManager.UnitsCache["ACH"] = achUnit;
                }
            }

            return IFCInstanceExporter.CreateUnitAssignment(file, unitSet);
        }

        /// <summary>
        /// Creates the global direction and sets the cardinal directions in 3D.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        private void CreateGlobalDirection(ExporterIFC exporterIFC)
        {
            IFCAnyHandle xDirPos = null;
            IFCAnyHandle xDirNeg = null;
            IFCAnyHandle yDirPos = null;
            IFCAnyHandle yDirNeg = null;
            IFCAnyHandle zDirPos = null;
            IFCAnyHandle zDirNeg = null;

            IFCFile file = exporterIFC.GetFile();
            IList<double> xxp = new List<double>();
            xxp.Add(1.0); xxp.Add(0.0); xxp.Add(0.0);
            xDirPos = IFCInstanceExporter.CreateDirection(file, xxp);

            IList<double> xxn = new List<double>();
            xxn.Add(-1.0); xxn.Add(0.0); xxn.Add(0.0);
            xDirNeg = IFCInstanceExporter.CreateDirection(file, xxn);

            IList<double> yyp = new List<double>();
            yyp.Add(0.0); yyp.Add(1.0); yyp.Add(0.0);
            yDirPos = IFCInstanceExporter.CreateDirection(file, yyp);

            IList<double> yyn = new List<double>();
            yyn.Add(0.0); yyn.Add(-1.0); yyn.Add(0.0);
            yDirNeg = IFCInstanceExporter.CreateDirection(file, yyn);

            IList<double> zzp = new List<double>();
            zzp.Add(0.0); zzp.Add(0.0); zzp.Add(1.0);
            zDirPos = IFCInstanceExporter.CreateDirection(file, zzp);

            IList<double> zzn = new List<double>();
            zzn.Add(0.0); zzn.Add(0.0); zzn.Add(-1.0);
            zDirNeg = IFCInstanceExporter.CreateDirection(file, zzn);

            ExporterIFCUtils.SetGlobal3DDirectionHandles(true, xDirPos, yDirPos, zDirPos);
            ExporterIFCUtils.SetGlobal3DDirectionHandles(false, xDirNeg, yDirNeg, zDirNeg);
        }

        /// <summary>
        /// Creates the global direction and sets the cardinal directions in 2D.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        private void CreateGlobalDirection2D(ExporterIFC exporterIFC)
        {
            IFCAnyHandle xDirPos2D = null;
            IFCAnyHandle xDirNeg2D = null;
            IFCAnyHandle yDirPos2D = null;
            IFCAnyHandle yDirNeg2D = null;
            IFCFile file = exporterIFC.GetFile();

            IList<double> xxp = new List<double>();
            xxp.Add(1.0); xxp.Add(0.0);
            xDirPos2D = IFCInstanceExporter.CreateDirection(file, xxp);

            IList<double> xxn = new List<double>();
            xxn.Add(-1.0); xxn.Add(0.0);
            xDirNeg2D = IFCInstanceExporter.CreateDirection(file, xxn);

            IList<double> yyp = new List<double>();
            yyp.Add(0.0); yyp.Add(1.0);
            yDirPos2D = IFCInstanceExporter.CreateDirection(file, yyp);

            IList<double> yyn = new List<double>();
            yyn.Add(0.0); yyn.Add(-1.0);
            yDirNeg2D = IFCInstanceExporter.CreateDirection(file, yyn);
            ExporterIFCUtils.SetGlobal2DDirectionHandles(true, xDirPos2D, yDirPos2D);
            ExporterIFCUtils.SetGlobal2DDirectionHandles(false, xDirNeg2D, yDirNeg2D);
        }

        /// <summary>
        /// Creates the global cartesian origin then sets the 3D and 2D origins.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        private void CreateGlobalCartesianOrigin(ExporterIFC exporterIFC)
        {

            IFCAnyHandle origin2d = null;
            IFCAnyHandle origin = null;

            IFCFile file = exporterIFC.GetFile();
            IList<double> measure = new List<double>();
            measure.Add(0.0); measure.Add(0.0); measure.Add(0.0);
            origin = IFCInstanceExporter.CreateCartesianPoint(file, measure);

            IList<double> measure2d = new List<double>();
            measure2d.Add(0.0); measure2d.Add(0.0);
            origin2d = IFCInstanceExporter.CreateCartesianPoint(file, measure2d);
            ExporterIFCUtils.SetGlobal3DOriginHandle(origin);
            ExporterIFCUtils.SetGlobal2DOriginHandle(origin2d);
        }

        /// <summary>
        /// Relate levels and products.
        /// </summary>
        /// <param name="exporterIFC">The IFC exporter object.</param>
        /// <param name="document">The document to relate the levels.</param>
        private void RelateLevels(ExporterIFC exporterIFC, Document document)
        {
            HashSet<IFCAnyHandle> buildingStoreys = new HashSet<IFCAnyHandle>();
            List<ElementId> levelIds = ExporterCacheManager.LevelInfoCache.LevelsByElevation;
            for (int ii = 0; ii < levelIds.Count; ii++)
            {
                ElementId levelId = levelIds[ii];
                IFCLevelInfo levelInfo = ExporterCacheManager.LevelInfoCache.GetLevelInfo(exporterIFC, levelId);
                if (levelInfo == null)
                    continue;
                
                // remove products that are aggregated (e.g., railings in stairs).
                Element level = document.GetElement(levelId);

                ICollection<IFCAnyHandle> relatedProductsToCheck = levelInfo.GetRelatedProducts();
                ICollection<IFCAnyHandle> relatedElementsToCheck = levelInfo.GetRelatedElements();

                // get coincident levels, if any.
                double currentElevation = levelInfo.Elevation;
                int nextLevelIdx = ii + 1;
                for (int jj = ii + 1; jj < levelIds.Count; jj++, nextLevelIdx++)
                {
                    ElementId nextLevelId = levelIds[jj];
                    IFCLevelInfo levelInfo2 = ExporterCacheManager.LevelInfoCache.GetLevelInfo(exporterIFC, nextLevelId);
                    if (levelInfo2 == null)
                        continue;

                    if (MathUtil.IsAlmostEqual(currentElevation, levelInfo2.Elevation))
                    {
                        foreach (IFCAnyHandle relatedProduct in levelInfo2.GetRelatedProducts())
                        {
                            relatedProductsToCheck.Add(relatedProduct);
                        }
                        foreach (IFCAnyHandle relatedElement in levelInfo2.GetRelatedElements())
                        {
                            relatedElementsToCheck.Add(relatedElement);
                        }
                    }
                    else
                        break;
                }

                // We may get stale handles in here; protect against this.
                HashSet<IFCAnyHandle> relatedProducts = new HashSet<IFCAnyHandle>();
                foreach (IFCAnyHandle relatedProduct in relatedProductsToCheck)
                {
                    try
                    {
                        if (!IFCAnyHandleUtil.HasRelDecomposes(relatedProduct))
                            relatedProducts.Add(relatedProduct);
                    }
                    catch
                    {
                    }
                }

                HashSet<IFCAnyHandle> relatedElements = new HashSet<IFCAnyHandle>();
                foreach (IFCAnyHandle relatedElement in relatedElementsToCheck)
                {
                    try
                    {
                        if (!IFCAnyHandleUtil.HasRelDecomposes(relatedElement))
                            relatedElements.Add(relatedElement);
                    }
                    catch
                    {
                    }
                }

                // skip coincident levels, if any.
                for (int jj = ii + 1; jj < nextLevelIdx; jj++)
                {
                    ElementId nextLevelId = levelIds[jj];
                    IFCLevelInfo levelInfo2 = ExporterCacheManager.LevelInfoCache.GetLevelInfo(exporterIFC, nextLevelId);
                    if (levelInfo2 == null)
                        continue;
                    
                    if (!levelInfo.GetBuildingStorey().Equals(levelInfo2.GetBuildingStorey()))
                        levelInfo2.GetBuildingStorey().Delete();
                }
                ii = nextLevelIdx - 1;

                if (relatedProducts.Count == 0 && relatedElements.Count == 0)
                {
                    levelInfo.GetBuildingStorey().Delete();
                }
                else
                {
                    bool added = buildingStoreys.Add(levelInfo.GetBuildingStorey());
                    if (added)
                    {
                        using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(exporterIFC, false))
                        {
                            productWrapper.AddElement(levelInfo.GetBuildingStorey(), levelInfo, null, false);
                            Element element = document.GetElement(levelId);
                            ExportElementProperties(exporterIFC, element, productWrapper);
                            PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                        }
                    }
                }

                if (relatedProducts.Count > 0)
                {
                    string guid = ExporterIFCUtils.CreateSubElementGUID(level, (int)IFCBuildingStoreySubElements.RelAggregates);
                    ExporterUtil.RelateObjects(exporterIFC, guid, levelInfo.GetBuildingStorey(), relatedProducts);
                }
                if (relatedElements.Count > 0)
                {
                    string guid = ExporterIFCUtils.CreateSubElementGUID(level, (int)IFCBuildingStoreySubElements.RelContainedInSpatialStructure);
                    IFCInstanceExporter.CreateRelContainedInSpatialStructure(exporterIFC.GetFile(), guid, exporterIFC.GetOwnerHistoryHandle(), null, null, relatedElements, levelInfo.GetBuildingStorey());
                }
            }

            if (buildingStoreys.Count > 0)
            {
                ProjectInfo projectInfo = document.ProjectInformation;
                string guid = ExporterIFCUtils.CreateSubElementGUID(projectInfo, (int)IFCBuildingSubElements.RelAggregatesBuildingStoreys);
                ExporterUtil.RelateObjects(exporterIFC, guid, exporterIFC.GetBuilding(), buildingStoreys);
            }
        }
    }
}
