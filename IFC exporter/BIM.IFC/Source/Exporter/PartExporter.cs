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
using System.Linq;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export Part element.
    /// </summary>
    class PartExporter
    {
        /// <summary>
        /// Export all the parts of the host element.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="hostElement">The host element having parts to export.</param>
        /// <param name="hostHandle">The host element handle.</param>
        /// <param name="originalWrapper">The IFCProductWrapper object.</param>
        public static void ExportHostPart(ExporterIFC exporterIFC, Element hostElement, IFCAnyHandle hostHandle,
            IFCProductWrapper originalWrapper, IFCPlacementSetter placementSetter, IFCAnyHandle originalPlacement, ElementId overrideLevelId)
        {
            using (IFCProductWrapper subWrapper = IFCProductWrapper.Create(exporterIFC, true))
            {
                List<ElementId> associatedPartsList = PartUtils.GetAssociatedParts(hostElement.Document, hostElement.Id, false, true).ToList();
                if (associatedPartsList.Count == 0)
                    return;

                bool isWallOrColumn = IsHostWallOrColumn(exporterIFC, hostElement);
                bool hasOverrideLevel = overrideLevelId != null && overrideLevelId != ElementId.InvalidElementId;

                IFCExtrusionAxes ifcExtrusionAxes = GetDefaultExtrusionAxesForHost(exporterIFC, hostElement);

                // Split parts if wall or column is split by level, and then export; otherwise, export parts normally.
                if (isWallOrColumn && hasOverrideLevel && ExporterCacheManager.ExportOptionsCache.WallAndColumnSplitting)
                {
                    if (!ExporterCacheManager.HostPartsCache.HasRegistered(hostElement.Id))                
                        SplitParts(exporterIFC, hostElement, associatedPartsList); // Split parts and associate them with host.                   

                    // Find and export the parts that are split by specific level.
                    List<KeyValuePair<Part, IFCRange>> splitPartRangeList = new List<KeyValuePair<Part, IFCRange>>();
                    splitPartRangeList = ExporterCacheManager.HostPartsCache.Find(hostElement.Id, overrideLevelId);

                    foreach (KeyValuePair<Part, IFCRange> partRange in splitPartRangeList)
                    {
                        PartExporter.ExportPart(exporterIFC, partRange.Key, subWrapper, placementSetter, originalPlacement, partRange.Value, ifcExtrusionAxes, hostElement, overrideLevelId, false);
                    }
                }
                else
                {
                    foreach (ElementId partId in associatedPartsList)
                    {
                        Part part = hostElement.Document.GetElement(partId) as Part;
                        PartExporter.ExportPart(exporterIFC, part, subWrapper, placementSetter, originalPlacement, null, ifcExtrusionAxes, hostElement, overrideLevelId, false);
                    }
                }

                // Create the relationship of Host and Parts.
                ICollection<IFCAnyHandle> relatedElementIds = subWrapper.GetAllObjects();
                if (relatedElementIds.Count > 0)
                {
                    string guid = ExporterIFCUtils.CreateGUID();
                    HashSet<IFCAnyHandle> relatedElementIdSet = new HashSet<IFCAnyHandle>(relatedElementIds);
                    IFCInstanceExporter.CreateRelAggregates(exporterIFC.GetFile(), guid, exporterIFC.GetOwnerHistoryHandle(), null, null, hostHandle, relatedElementIdSet);
                }
            }
        }

        /// <summary>
        /// Export the standalone parts:
        ///     - The parts made from originals in Links 
        ///     - The Orphan parts: the linked file where the original host element comes from is unloaded.
        ///     - The Zombie parts: the original host element is deleted from the linked file.
        /// </summary>
        /// <remarks>
        /// This is a temporary workaround to export the parts made from linked elements. It should be refined when linked are supported (LinkedInstance at least.)
        /// There are some limitations:
        /// The linked element will not export as host, including the relative elements: e.g. windows, doors, openings.
        /// The host part cannot export if visibility is set by linked view and has 'Show Original'.
        /// The standalone part will skip export if Base Level is set 'Non Associated'.
        /// The linked part export cannot be split even if its category is wall or column and 'Split wall or column by story' is checked.
        /// </remarks>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="partElement">The standalone part to export.</param>
        /// <param name="geometryElement">The goemetry of the part.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        public static void ExportStandalonePart(ExporterIFC exporterIFC, Element partElement, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            Part part = partElement as Part;
            if (!ExporterCacheManager.ExportOptionsCache.ExportParts || part == null || geometryElement == null)
                return;
            
            foreach (LinkElementId linkElementId in part.GetSourceElementIds())
            {
                if (linkElementId.HostElementId != ElementId.InvalidElementId )
                {
                    // Has host element, so should export with host element.
                    return;
                }
                if (linkElementId.LinkedElementId != ElementId.InvalidElementId)
                {
                    if (part.Level == null || part.Level.Id == ElementId.InvalidElementId)
                    {
                        // skip the parts have NO Base Level.
                        continue;
                    }

                    IFCExtrusionAxes ifcExtrusionAxes = GetDefaultExtrusionAxesForPart(part);
                    PartExporter.ExportPart(exporterIFC, partElement, productWrapper, null, null, null, ifcExtrusionAxes, null, null, false);
                }
            }
        }

        /// <summary>
        /// Export the parts as independent building elements. 
        /// </summary>
        /// <remarks>
        /// The function works with AlternateIFCUI and it requires two conditions:
        /// 1. Allows export parts: 'current view only' is checked and 'show parts' is selected.
        /// 2. Allows export parts independent: 'Export parts as building elements' is checked in alternate UI dialog.
        /// </remarks>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="partElement">The standalone part to export.</param>
        /// <param name="geometryElement">The goemetry of the part.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        public static void ExportPartAsBuildingElement(ExporterIFC exporterIFC, Element partElement, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            Part part = partElement as Part;
            if (!ExporterCacheManager.ExportOptionsCache.ExportParts || part == null || geometryElement == null)
                return;

            bool isWall = part.OriginalCategoryId == new ElementId(BuiltInCategory.OST_Walls);
            bool isColumn = part.OriginalCategoryId == new ElementId(BuiltInCategory.OST_Columns);
            bool isWallOrColumn = isWall || isColumn;
            IFCExtrusionAxes ifcExtrusionAxes = GetDefaultExtrusionAxesForPart(part);
            
            Element hostElement = null;
            ElementId overrideLevelId = null;

            // Find the host element of the part.
            hostElement = FindRootParent(part, part.OriginalCategoryId);

            // If part's level is not associated, try to get the host's level with the same category.
            if (hostElement != null && part.Level == null)
            {
                overrideLevelId = hostElement.Level.Id;
            }

            // Split parts with original category is wall or column and the option wall or column is split by level is checked, and then export; 
            // otherwise, export separate parts normally.
            if (isWallOrColumn && ExporterCacheManager.ExportOptionsCache.WallAndColumnSplitting)
            {
                IList<ElementId> levels = new List<ElementId>();
                IList<IFCRange> ranges = new List<IFCRange>();
                IFCExportType exportType = isWall ? IFCExportType.ExportWall : IFCExportType.ExportColumnType;
                LevelUtil.CreateSplitLevelRangesForElement(exporterIFC, exportType, part, out levels, out ranges);
                if (ranges.Count == 0)
                {
                    PartExporter.ExportPart(exporterIFC, partElement, productWrapper, null, null, null, ifcExtrusionAxes, hostElement, overrideLevelId, true);
                }
                else
                {
                    for (int ii = 0; ii < ranges.Count; ii++)
                    {
                        PartExporter.ExportPart(exporterIFC, partElement, productWrapper, null, null, ranges[ii], ifcExtrusionAxes, hostElement, levels[ii], true);
                    }
                }
            }
            else
                PartExporter.ExportPart(exporterIFC, partElement, productWrapper, null, null, null, ifcExtrusionAxes, hostElement, overrideLevelId, true);
        }

        /// <summary>
        /// Export the individual part (IfcBuildingElementPart).
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="partElement">The part element to export.</param>
        /// <param name="geometryElement">The geometry of part.</param>
        /// <param name="productWrapper">The IFCProductWrapper object.</param>
        public static void ExportPart(ExporterIFC exporterIFC, Element partElement, IFCProductWrapper productWrapper,
            IFCPlacementSetter placementSetter, IFCAnyHandle originalPlacement, IFCRange range, IFCExtrusionAxes ifcExtrusionAxes,
            Element hostElement, ElementId overrideLevelId, bool asBuildingElement)
        {
            if (!ElementFilteringUtil.IsElementVisible(ExporterCacheManager.ExportOptionsCache.FilterViewForExport, partElement))
                return;

            Part part = partElement as Part;
            if (part == null)
                return;

            IFCPlacementSetter standalonePlacementSetter = null;
            bool standaloneExport = hostElement == null && !asBuildingElement;

            ElementId partExportLevel = null;
            if (standaloneExport || asBuildingElement)
            {
                if (partElement.Level != null)
                    partExportLevel = partElement.Level.Id;
            }
            else
            {
                if (part.OriginalCategoryId != hostElement.Category.Id)
                    return;
                partExportLevel = hostElement.Level.Id;
            }
            if (overrideLevelId != null)
                partExportLevel = overrideLevelId;

            if (ExporterCacheManager.PartExportedCache.HasExported(partElement.Id, partExportLevel))
                return;

            Options options = GeometryUtil.GetIFCExportGeometryOptions();
            View ownerView = partElement.Document.GetElement(partElement.OwnerViewId) as View;
            if (ownerView != null)
                options.View = ownerView;

            GeometryElement geometryElement = partElement.get_Geometry(options);
            if (geometryElement == null)
                return;

            try
            {
                IFCFile file = exporterIFC.GetFile();
                using (IFCTransaction transaction = new IFCTransaction(file))
                {
                    IFCAnyHandle partPlacement = null;
                    if (standaloneExport || asBuildingElement)
                    {
                        Transform orientationTrf = Transform.Identity;
                        standalonePlacementSetter = IFCPlacementSetter.Create(exporterIFC, partElement, null, orientationTrf, partExportLevel);
                        partPlacement = standalonePlacementSetter.GetPlacement();
                    }
                    else
                        partPlacement = ExporterUtil.CopyLocalPlacement(file, originalPlacement);

                    bool validRange = (range != null && !MathUtil.IsAlmostZero(range.Start - range.End));

                    SolidMeshGeometryInfo solidMeshInfo;
                    if (validRange)
                    {
                        solidMeshInfo = GeometryUtil.GetClippedSolidMeshGeometry(geometryElement, range);
                        if (solidMeshInfo.GetSolids().Count == 0 && solidMeshInfo.GetMeshes().Count == 0)
                            return;
                    }
                    else
                    {
                        solidMeshInfo = GeometryUtil.GetSolidMeshGeometry(geometryElement, Transform.Identity);
                    }

                    using (IFCExtrusionCreationData extrusionCreationData = new IFCExtrusionCreationData())
                    {
                        extrusionCreationData.SetLocalPlacement(partPlacement);
                        extrusionCreationData.ReuseLocalPlacement = false;
                        extrusionCreationData.PossibleExtrusionAxes = ifcExtrusionAxes;

                        IList<Solid> solids = solidMeshInfo.GetSolids();
                        IList<Mesh> meshes = solidMeshInfo.GetMeshes();

                        ElementId catId = CategoryUtil.GetSafeCategoryId(partElement);

                        BodyData bodyData = null;
                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                        if (solids.Count > 0 || meshes.Count > 0)
                        {
                            bodyData = BodyExporter.ExportBody(partElement.Document.Application, exporterIFC, partElement, catId, solids, meshes,
                                bodyExporterOptions, extrusionCreationData);
                        }
                        else
                        {
                            IList<GeometryObject> geomlist = new List<GeometryObject>();
                            geomlist.Add(geometryElement);
                            bodyData = BodyExporter.ExportBody(partElement.Document.Application, exporterIFC, partElement, catId, geomlist,
                                bodyExporterOptions, extrusionCreationData);
                        }

                        IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                        {
                            extrusionCreationData.ClearOpenings();
                            return;
                        }

                        IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                        representations.Add(bodyRep);

                        IFCAnyHandle prodRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);

                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                        string partGUID = ExporterIFCUtils.CreateGUID(partElement);
                        string origPartName = NamingUtil.CreateIFCName(exporterIFC, -1);
                        string partName = NamingUtil.GetNameOverride(partElement, origPartName);
                        string partDescription = NamingUtil.GetDescriptionOverride(partElement, null);
                        string partObjectType = NamingUtil.GetObjectTypeOverride(partElement, NamingUtil.CreateIFCObjectName(exporterIFC, partElement));
                        string partElemId = NamingUtil.CreateIFCElementId(partElement);

                        IFCAnyHandle ifcPart = null;
                        if (!asBuildingElement)
                        {
                            ifcPart = IFCInstanceExporter.CreateBuildingElementPart(file, partGUID, ownerHistory, partName, partDescription, 
                                partObjectType, extrusionCreationData.GetLocalPlacement(), prodRep, partElemId);
                        }
                        else
                        {
                            string ifcEnumType;
                            IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, hostElement, out ifcEnumType);
                            switch (exportType)
                            {
                                case IFCExportType.ExportColumnType:
                                    ifcPart = IFCInstanceExporter.CreateColumn(file, partGUID, ownerHistory, partName, partDescription, partObjectType, 
                                        extrusionCreationData.GetLocalPlacement(), prodRep, partElemId);
                                    break;
                                case IFCExportType.ExportCovering:
                                    IFCCoveringType coveringType = CeilingExporter.GetIFCCoveringType(hostElement, ifcEnumType);
                                    ifcPart = IFCInstanceExporter.CreateCovering(file, partGUID, ownerHistory, partName, partDescription, partObjectType, 
                                        extrusionCreationData.GetLocalPlacement(), prodRep, partElemId, coveringType);
                                    break;
                                case IFCExportType.ExportFooting:
                                    IFCFootingType footingType = FootingExporter.GetIFCFootingType(hostElement, ifcEnumType);
                                    ifcPart = IFCInstanceExporter.CreateFooting(file, partGUID, ownerHistory, partName, partDescription, partObjectType, 
                                        extrusionCreationData.GetLocalPlacement(), prodRep, partElemId, footingType);
                                    break;
                                case IFCExportType.ExportRoof:
                                    IFCRoofType roofType = RoofExporter.GetIFCRoofType(ifcEnumType);
                                    ifcPart = IFCInstanceExporter.CreateRoof(file, partGUID, ownerHistory, partName, partDescription, partObjectType, 
                                        extrusionCreationData.GetLocalPlacement(), prodRep, partElemId, roofType);
                                    break;
                                case IFCExportType.ExportSlab:
                                    IFCSlabType slabType = FloorExporter.GetIFCSlabType(ifcEnumType);
                                    ifcPart = IFCInstanceExporter.CreateSlab(file, partGUID, ownerHistory, partName, partDescription, partObjectType, 
                                        extrusionCreationData.GetLocalPlacement(), prodRep, partElemId, slabType);
                                    break;
                                case IFCExportType.ExportWall:
                                    ifcPart = IFCInstanceExporter.CreateWallStandardCase(file, partGUID, ownerHistory, partName, partDescription, 
                                        partObjectType, extrusionCreationData.GetLocalPlacement(), prodRep, partElemId);
                                    break;
                                default:
                                    ifcPart = IFCInstanceExporter.CreateBuildingElementProxy(file, partGUID, ownerHistory, partName, partDescription, 
                                        partObjectType, extrusionCreationData.GetLocalPlacement(), prodRep, partElemId, IFCElementComposition.Element);
                                    break;
                            }
                        }

                        productWrapper.AddElement(ifcPart, standaloneExport || asBuildingElement ? standalonePlacementSetter : placementSetter, extrusionCreationData, standaloneExport || asBuildingElement);

                        //Add the exported part to exported cache.
                        TraceExportedParts(partElement, partExportLevel, standaloneExport || asBuildingElement ? ElementId.InvalidElementId : hostElement.Id);

                        CategoryUtil.CreateMaterialAssociations(partElement.Document, exporterIFC, ifcPart, bodyData.MaterialIds);

                        transaction.Commit();
                    }
                }
            }
            finally
            {
                if (standalonePlacementSetter != null)
                    standalonePlacementSetter.Dispose();
            }
        }

        /// <summary>
        /// Add the exported part to cache.
        /// </summary>
        /// <param name="partElement">The exported part.</param>
        /// <param name="partExportLevel">The level to which the part has exported.</param>
        /// <param name="hostElement">The host element of part exported.</param>
        private static void TraceExportedParts(Element partElement, ElementId partExportLevel, ElementId hostElementId)
        {
            if (!ExporterCacheManager.PartExportedCache.HasRegistered(partElement.Id))
            {
                Dictionary<ElementId, ElementId> hostOverideLevels = new Dictionary<ElementId, ElementId>();

                if (!hostOverideLevels.ContainsKey(partExportLevel))
                    hostOverideLevels.Add(partExportLevel, hostElementId);
                ExporterCacheManager.PartExportedCache.Register(partElement.Id, hostOverideLevels);
            }
            else
            {
                ExporterCacheManager.PartExportedCache.Add(partElement.Id, partExportLevel, hostElementId);
            }   
        }

        /// <summary>
        /// Identifies if the host element can export the associated parts.
        /// </summary>
        /// <param name="hostElement">The host element.</param>
        /// <returns>True if host element can export the parts and have any associated parts, false otherwise.</returns>
        public static bool CanExportParts(Element hostElement)
        {
            if (ExporterCacheManager.ExportOptionsCache.ExportParts)
            {
                return PartUtils.HasAssociatedParts(hostElement.Document, hostElement.Id);
            }
            return false;
        }

        /// <summary>
        /// Identifies if the host element can export when exporting parts.
        /// 1. If host element has non merged parts (>0), it can be export no matter if it has merged parts or not, and return true.
        /// 2. If host element has merged parts
        ///    - If the merged part is the right category and not export yet, return true.
        ///    - If the merged part is the right category but has been exported by other host, return false.
        ///    - If the merged part is not the right category, should not export and return false.
        /// </summary>
        /// <param name="hostElement">The host element having parts.</param>
        /// <param name="levelId">The level the part would export.</param>
        /// <Param name="IsSplit">The bool flag identifies if the host element is split by story.</Param>
        /// <returns>True if the element can export, false otherwise.</returns>
        public static bool CanExportElementInPartExport(Element hostElement, ElementId levelId, bool IsSplit)
        {
            List<ElementId> associatedPartsList = PartUtils.GetAssociatedParts(hostElement.Document, hostElement.Id, false, true).ToList();

            foreach (ElementId partId in associatedPartsList)
            {
                Part part = hostElement.Document.GetElement(partId) as Part;
                if (PartUtils.IsMergedPart(part))
                {
                    if (part.OriginalCategoryId == hostElement.Category.Id)
                    {
                        if (IsSplit)
                        {
                            if (!ExporterCacheManager.PartExportedCache.HasExported(partId, levelId))
                            {
                                // has merged split part and not export yet.
                                return true;
                            }
                        }
                        else if (!ExporterCacheManager.PartExportedCache.HasRegistered(partId))
                        {
                            // has merged part and not export yet.
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

            // has no merged parts or other parts or merged parts have been exported.
            return false;
        }

        /// <summary>
        ///  Identifies if host element is a Wall or a Column
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="hostElement">The host element having associated parts.</param>
        /// <returns>True if Wall or Column, false otherwise.</returns>
        private static bool IsHostWallOrColumn(ExporterIFC exporterIFC, Element hostElement)
        {
            string ifcEnumType;
            IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, hostElement, out ifcEnumType);
            return (exportType == IFCExportType.ExportWall) || (exportType == IFCExportType.ExportColumnType);
        }

        /// <summary>
        /// Get the Default IFCExtrusionAxes for part. 
        /// Simply having roof/floor/wall/column as Z and everything else as XY.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns>TryZ for wall/column/floor/roof category and TryXY for other category.</returns>
        private static IFCExtrusionAxes GetDefaultExtrusionAxesForPart(Part part)
        {
            switch ((BuiltInCategory)part.OriginalCategoryId.IntegerValue)
            {
                case BuiltInCategory.OST_Walls:
                case BuiltInCategory.OST_Columns:
                case BuiltInCategory.OST_Floors:
                case BuiltInCategory.OST_Roofs:
                    return IFCExtrusionAxes.TryZ;
                default:
                    return IFCExtrusionAxes.TryXY;
            }
        }

        /// <summary>
        /// Get the Default IFCExtrusionAxes for host element. 
        /// Simply having roof/floor/wall/column as Z and everything else as XY.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="hostElement">The host element to get the IFCExtrusionAxes.</param>
        /// <returns>TryZ for wall/column/floor/roof elements and TryXY for other elements.</returns>
        private static IFCExtrusionAxes GetDefaultExtrusionAxesForHost(ExporterIFC exporterIFC, Element hostElement)
        {
            string ifcEnumType;
            IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, hostElement, out ifcEnumType);

            switch (exportType)
            {
                case IFCExportType.ExportWall:
                case IFCExportType.ExportColumnType:
                case IFCExportType.ExportSlab:
                case IFCExportType.ExportRoof:
                    return IFCExtrusionAxes.TryZ;
                default:
                    return IFCExtrusionAxes.TryXY;
            }
        }

        /// <summary>
        /// Split associated parts when host element is split by level.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="hostElement">The host element havign associtaed parts.</param>
        /// <param name="associatedPartsList">The list of associtated parts.</param>
        private static void SplitParts(ExporterIFC exporterIFC, Element hostElement, List<ElementId> associatedPartsList)
        {
            string ifcEnumType;
            IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, hostElement, out ifcEnumType);

            // Split the host to find the orphan parts.
            IList<ElementId> orphanLevels = new List<ElementId>();
            IList<ElementId> hostLevels = new List<ElementId>();
            IList<IFCRange> hostRanges = new List<IFCRange>();
            LevelUtil.CreateSplitLevelRangesForElement(exporterIFC, exportType, hostElement, out hostLevels, out hostRanges);
            orphanLevels = hostLevels;

            // Split each Parts
            IList<ElementId> levels = new List<ElementId>();
            IList<IFCRange> ranges = new List<IFCRange>();
            // Dictionary to storage the level and its parts.
            Dictionary<ElementId, List<KeyValuePair<Part, IFCRange>>> levelParts = new Dictionary<ElementId, List<KeyValuePair<Part, IFCRange>>>();

            foreach (ElementId partId in associatedPartsList)
            {
                Part part = hostElement.Document.GetElement(partId) as Part;
                LevelUtil.CreateSplitLevelRangesForElement(exporterIFC, exportType, part, out levels, out ranges);

                // if the parts are above top level, associate them with nearest bottom level.
                if (ranges.Count == 0)
                {
                    ElementId bottomLevelId = FindPartSplitLevel(exporterIFC, part);

                    if (bottomLevelId == ElementId.InvalidElementId && part.Level != null)
                        bottomLevelId = part.Level.Id;

                    if (!levelParts.ContainsKey(bottomLevelId))
                        levelParts.Add(bottomLevelId, new List<KeyValuePair<Part, IFCRange>>());

                    KeyValuePair<Part, IFCRange> splitPartRange = new KeyValuePair<Part, IFCRange>(part, null);
                    levelParts[bottomLevelId].Add(splitPartRange);

                    continue;
                }

                // The parts split by levels are stored in dictionary.
                for (int ii = 0; ii < ranges.Count; ii++)
                {
                    if (!levelParts.ContainsKey(levels[ii]))
                        levelParts.Add(levels[ii], new List<KeyValuePair<Part, IFCRange>>());

                    KeyValuePair<Part, IFCRange> splitPartRange = new KeyValuePair<Part, IFCRange>(part, ranges[ii]);
                    levelParts[levels[ii]].Add(splitPartRange);
                }
             
                if (levels.Count > hostLevels.Count)
                {
                    orphanLevels = orphanLevels.Union<ElementId>(levels).ToList();
                }
            }      

            ExporterCacheManager.HostPartsCache.Register(hostElement.Id, levelParts);

            // The levels of orphan part.
            orphanLevels = orphanLevels.Where(number => !hostLevels.Contains(number)).ToList();
            List<KeyValuePair<ElementId, IFCRange>> levelRangePairList = new List<KeyValuePair<ElementId, IFCRange>>();
            foreach (ElementId orphanLevelId in orphanLevels)
            {
                IFCLevelInfo levelInfo = ExporterCacheManager.LevelInfoCache.GetLevelInfo(exporterIFC, orphanLevelId);
                if (levelInfo == null)
                    continue;
                double levelHeight = ExporterCacheManager.LevelInfoCache.FindHeight(orphanLevelId);
                IFCRange levelRange = new IFCRange(levelInfo.Elevation, levelInfo.Elevation + levelHeight);

                List<KeyValuePair<Part, IFCRange>> splitPartRangeList = new List<KeyValuePair<Part, IFCRange>>();
                splitPartRangeList = ExporterCacheManager.HostPartsCache.Find(hostElement.Id, orphanLevelId);
                IFCRange highestRange = levelRange;
                foreach (KeyValuePair<Part, IFCRange> partRange in splitPartRangeList)
                {
                    if (partRange.Value.End > highestRange.End)
                    {
                        highestRange = partRange.Value;
                    }
                }
                levelRangePairList.Add(new KeyValuePair<ElementId, IFCRange>(orphanLevelId, highestRange));
            }
            if (levelRangePairList.Count > 0)
            {
                ExporterCacheManager.DummyHostCache.Register(hostElement.Id, levelRangePairList);
            }
        }

        /// <summary>
        /// Find the nearest bottom level for parts that are above top level.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="part">The part above top level.</param>
        /// <returns>The ElementId of nearest bottom level.</returns>
        private static ElementId FindPartSplitLevel(ExporterIFC exporterIFC, Part part)
        {
            double extension = LevelUtil.GetLevelExtension();
            ElementId theSplitLevelId = ElementId.InvalidElementId;
            BoundingBoxXYZ boundingBox = part.get_BoundingBox(null);
            // The levels should have been sorted.
            List<ElementId> levelIds = ExporterCacheManager.LevelInfoCache.LevelsByElevation;
            // Find the nearest bottom level.
            foreach (ElementId levelId in levelIds)
            {
                IFCLevelInfo levelInfo = ExporterCacheManager.LevelInfoCache.GetLevelInfo(exporterIFC, levelId);
                if (levelInfo == null)
                    continue;
                if (levelInfo.Elevation < boundingBox.Min.Z + extension)
                {
                    theSplitLevelId = levelId;
                }
            }

            return theSplitLevelId;
        }

        /// <summary>
        /// Find the root element for a part with its original category. 
        /// </summary>
        /// <param name="part">The part element.</param>
        /// <param name="originalCategoryId">The category id to find the root element.</param>
        /// <returns>The root element that makes the part; returns null if fail to find the root parent.</returns>
        private static Element FindRootParent(Part part, ElementId originalCategoryId)
        {
            Element hostElement = null;

            foreach (LinkElementId linkElementId in part.GetSourceElementIds())
            {
                if (linkElementId.HostElementId == ElementId.InvalidElementId)
                    continue;

                Element parentElement = part.Document.GetElement(linkElementId.HostElementId);
                // If the direct parent is a part, find its parent.
                if (parentElement is Part)
                {
                    Part parentPart = parentElement as Part;
                    hostElement = FindRootParent(parentPart, originalCategoryId);
                    if (hostElement != null)
                        return hostElement;
                }
                else if (originalCategoryId == parentElement.Category.Id)
                {
                    hostElement = parentElement;
                    return hostElement;
                }
            }

            return hostElement;
        }
    }
}
