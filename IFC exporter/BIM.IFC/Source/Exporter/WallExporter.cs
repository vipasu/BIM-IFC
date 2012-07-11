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
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export walls.
    /// </summary>
    class WallExporter
    {
        private static IFCAnyHandle FallbackTryToCreateAsExtrusion(ExporterIFC exporterIFC, Wall wallElement, SolidMeshGeometryInfo smCapsule,
            ElementId catId, Curve curve, Plane plane, double depth, IFCRange zSpan, IFCRange range, IFCPlacementSetter setter, 
            out IList<IFCExtrusionData> cutPairOpenings, out bool isCompletelyClipped)
        {
            cutPairOpenings = new List<IFCExtrusionData>();

            IFCAnyHandle bodyRep;
            isCompletelyClipped = false;

            XYZ localOrig = plane.Origin;

            bool hasExtrusion = HasElevationProfile(wallElement);
            if (hasExtrusion)
            {
                IList<CurveLoop> loops = GetElevationProfile(wallElement);
                if (loops.Count == 0)
                    hasExtrusion = false;
                else
                {
                    IList<IList<CurveLoop>> sortedLoops = ExporterIFCUtils.SortCurveLoops(loops);
                    if (sortedLoops.Count == 0)
                        return null;

                    // Current limitation: can't handle wall split into multiple disjointed pieces.
                    int numSortedLoops = sortedLoops.Count;
                    if (numSortedLoops > 1)
                        return null;

                    bool ignoreExtrusion = true;
                    bool cantHandle = false;
                    bool hasGeometry = false;
                    for (int ii = 0; (ii < numSortedLoops) && !cantHandle; ii++)
                    {
                        int sortedLoopSize = sortedLoops[ii].Count;
                        if (sortedLoopSize == 0)
                            continue;
                        if (!ExporterIFCUtils.IsCurveLoopConvexWithOpenings(sortedLoops[ii][0], wallElement, range, out ignoreExtrusion))
                        {
                            if (ignoreExtrusion)
                            {
                                // we need more information.  Is there something to export?  If so, we'll
                                // ignore the extrusion.  Otherwise, we will fail.

                                if (smCapsule.SolidsCount() == 0 && smCapsule.MeshesCount() == 0)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                cantHandle = true;
                            }
                            hasGeometry = true;
                        }
                        else
                        {
                            hasGeometry = true;
                        }
                    }

                    if (!hasGeometry)
                    {
                        isCompletelyClipped = true;
                        return null;
                    }

                    if (cantHandle)
                        return null;
                }
            }

            if (!CanExportWallGeometryAsExtrusion(wallElement, range))
                return null;

            // extrusion direction.
            XYZ extrusionDir = GetWallHeightDirection(wallElement);

            // create extrusion boundary.
            IList<CurveLoop> boundaryLoops = new List<CurveLoop>();

            bool alwaysThickenCurve = IsWallBaseRectangular(wallElement, curve);

            if (!alwaysThickenCurve)
            {
                boundaryLoops = GetLoopsFromTopBottomFace(wallElement, exporterIFC);
                if (boundaryLoops.Count == 0)
                    return null;
            }
            else
            {
                CurveLoop newLoop = CurveLoop.CreateViaThicken(curve, wallElement.Width, XYZ.BasisZ);
                if (newLoop == null)
                    return null;

                if (!newLoop.IsCounterclockwise(XYZ.BasisZ))
                    newLoop = GeometryUtil.ReverseOrientation(newLoop);
                boundaryLoops.Add(newLoop);
            }

            // origin gets scaled later.
            XYZ setterOffset = new XYZ(0, 0, setter.Offset + (localOrig[2] - setter.BaseOffset));

            IFCAnyHandle baseBodyItemHnd = ExtrusionExporter.CreateExtrudedSolidFromCurveLoop(exporterIFC, null, boundaryLoops, plane,
                extrusionDir, depth);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(baseBodyItemHnd))
                return null;

            IFCAnyHandle bodyItemHnd = AddClippingsToBaseExtrusion(exporterIFC, wallElement,
               setterOffset, range, zSpan, baseBodyItemHnd, out cutPairOpenings);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyItemHnd))
                return null;

            IFCAnyHandle styledItemHnd = BodyExporter.CreateSurfaceStyleForRepItem(exporterIFC, wallElement.Document,
                baseBodyItemHnd, ElementId.InvalidElementId);

            HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
            bodyItems.Add(bodyItemHnd);

            IFCAnyHandle contextOfItemsBody = exporterIFC.Get3DContextHandle("Body");
            if (baseBodyItemHnd.Id == bodyItemHnd.Id)
            {
                bodyRep = RepresentationUtil.CreateSweptSolidRep(exporterIFC, wallElement, catId, contextOfItemsBody, bodyItems, null);
            }
            else
            {
                bodyRep = RepresentationUtil.CreateClippingRep(exporterIFC, wallElement, catId, contextOfItemsBody, bodyItems);
            }

            return bodyRep;
        }

        /// <summary>
        /// Main implementation to export walls.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="origWrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="overrideLevelId">
        /// The level id.
        /// </param>
        /// <param name="range">
        /// The range to be exported for the element.
        /// </param>
        /// <returns>
        /// The exported wall handle.
        /// </returns>
        public static IFCAnyHandle ExportWallBase(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement,
           IFCProductWrapper origWrapper, ElementId overrideLevelId, IFCRange range)
        {
            using (IFCProductWrapper localWrapper = IFCProductWrapper.Create(origWrapper))
            {
                ElementId catId = CategoryUtil.GetSafeCategoryId(element);

                Wall wallElement = element as Wall;
                FamilyInstance famInstWallElem = element as FamilyInstance;

                if (wallElement == null && famInstWallElem == null)
                    return null;

                if (wallElement != null && IsWallCompletelyClipped(wallElement, exporterIFC, range))
                    return null;

                // get global values.
                Document doc = element.Document;
                double scale = exporterIFC.LinearScale;

                IFCFile file = exporterIFC.GetFile();
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                IFCAnyHandle contextOfItemsAxis = exporterIFC.Get3DContextHandle("Axis");
                IFCAnyHandle contextOfItemsBody = exporterIFC.Get3DContextHandle("Body");
              
                IFCRange zSpan = new IFCRange();
                double depth = 0.0;
                bool validRange = (range != null && !MathUtil.IsAlmostZero(range.Start - range.End));

                bool exportParts = PartExporter.CanExportParts(wallElement);
                if (exportParts && !PartExporter.CanExportElementInPartExport(wallElement, validRange? overrideLevelId : wallElement.Level.Id, validRange))
                    return null;

                // get bounding box height so that we can subtract out pieces properly.
                // only for Wall, not FamilyInstance.
                if (wallElement != null && geometryElement != null)
                {
                    BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                    if (boundingBox == null)
                        return null;
                    zSpan = new IFCRange(boundingBox.Min.Z, boundingBox.Max.Z);

                    // if we have a top clipping plane, modify depth accordingly.
                    double bottomHeight = validRange ? Math.Max(zSpan.Start, range.Start) : zSpan.Start;
                    double topHeight = validRange ? Math.Min(zSpan.End, range.End) : zSpan.End;
                    depth = topHeight - bottomHeight;
                    if (MathUtil.IsAlmostZero(depth))
                        return null;
                    depth *= scale;
                }

                IFCAnyHandle axisRep = null;
                IFCAnyHandle bodyRep = null;

                bool exportingAxis = false;
                Curve curve = null;

                bool exportedAsWallWithAxis = false;
                bool exportedBodyDirectly = false;
                bool exportingInplaceOpenings = false;

                Curve centerCurve = GetWallAxis(wallElement);

                XYZ localXDir = new XYZ(1, 0, 0);
                XYZ localYDir = new XYZ(0, 1, 0);
                XYZ localZDir = new XYZ(0, 0, 1);
                XYZ localOrig = new XYZ(0, 0, 0);
                double eps = MathUtil.Eps();

                if (centerCurve != null)
                {
                    Curve baseCurve = GetWallAxisAtBaseHeight(wallElement);
                    curve = GetWallTrimmedCurve(wallElement, baseCurve);

                    IFCRange curveBounds;
                    XYZ oldOrig;
                    GeometryUtil.GetAxisAndRangeFromCurve(curve, out curveBounds, out localXDir, out oldOrig);

                    localOrig = oldOrig;
                    if (baseCurve != null)
                    {
                        if (!validRange || (MathUtil.IsAlmostEqual(range.Start, zSpan.Start)))
                        {
                            XYZ newOrig = baseCurve.Evaluate(curveBounds.Start, false);
                            if (!validRange && (zSpan.Start < newOrig[2] - eps))
                                localOrig = new XYZ(localOrig.X, localOrig.Y, zSpan.Start);
                            else
                                localOrig = new XYZ(localOrig.X, localOrig.Y, newOrig[2]);
                        }
                        else
                        {
                            localOrig = new XYZ(localOrig.X, localOrig.Y, range.Start);
                        }
                    }

                    double dist = localOrig[2] - oldOrig[2];
                    if (!MathUtil.IsAlmostZero(dist))
                    {
                        XYZ moveVec = new XYZ(0, 0, dist);
                        curve = GeometryUtil.MoveCurve(curve, moveVec);
                    }
                    localYDir = localZDir.CrossProduct(localXDir);

                    // ensure that X and Z axes are orthogonal.
                    double xzDot = localZDir.DotProduct(localXDir);
                    if (!MathUtil.IsAlmostZero(xzDot))
                        localXDir = localYDir.CrossProduct(localZDir);
                }

                Transform orientationTrf = Transform.Identity;
                orientationTrf.BasisX = localXDir;
                orientationTrf.BasisY = localYDir;
                orientationTrf.BasisZ = localZDir;
                orientationTrf.Origin = localOrig;

                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element, null, orientationTrf, overrideLevelId))
                {
                    IFCAnyHandle localPlacement = setter.GetPlacement();

                    Plane plane = new Plane(localXDir, localYDir, localOrig);  // project curve to XY plane.
                    XYZ projDir = XYZ.BasisZ;

                    // two representations: axis, body.         
                    {
                        if ((centerCurve != null) && (GeometryUtil.CurveIsLineOrArc(centerCurve)))
                        {
                            exportingAxis = true;

                            string identifierOpt = "Axis";	// IFC2x2 convention
                            string representationTypeOpt = "Curve2D";  // IFC2x2 convention

                            IFCGeometryInfo info = IFCGeometryInfo.CreateCurveGeometryInfo(exporterIFC, plane, projDir, false);
                            ExporterIFCUtils.CollectGeometryInfo(exporterIFC, info, curve, XYZ.Zero, true);
                            IList<IFCAnyHandle> axisItems = info.GetCurves();

                            if (axisItems.Count == 0)
                            {
                                exportingAxis = false;
                            }
                            else
                            {
                                HashSet<IFCAnyHandle> axisItemSet = new HashSet<IFCAnyHandle>();
                                foreach (IFCAnyHandle axisItem in axisItems)
                                    axisItemSet.Add(axisItem);

                                axisRep = RepresentationUtil.CreateShapeRepresentation(exporterIFC, element, catId, contextOfItemsAxis,
                                   identifierOpt, representationTypeOpt, axisItemSet);
                            }
                        }
                    }

                    IList<IFCExtrusionData> cutPairOpenings = new List<IFCExtrusionData>();
                    Document document = element.Document;

                    if (wallElement != null && exportingAxis && curve != null)
                    {
                        SolidMeshGeometryInfo solidMeshInfo = 
                            (range == null) ? GeometryUtil.GetSolidMeshGeometry(geometryElement, Transform.Identity) :
                                GeometryUtil.GetClippedSolidMeshGeometry(geometryElement, range);
                                
                        IList<Solid> solids = solidMeshInfo.GetSolids();
                        IList<Mesh> meshes = solidMeshInfo.GetMeshes();
                        if (solids.Count == 0 && meshes.Count == 0)
                            return null;

                        bool useNewCode = false;
                        if (useNewCode && solids.Count == 1 && meshes.Count == 0)
                        {
                            bool completelyClipped;
                            bodyRep = ExtrusionExporter.CreateExtrusionWithClipping(exporterIFC, wallElement, catId, solids[0], 
                                plane, projDir, range, out completelyClipped);

                            if (completelyClipped)
                                return null;

                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                            {
                                exportedAsWallWithAxis = true;
                                exportedBodyDirectly = true;
                            }
                            else
                            {
                                exportedAsWallWithAxis = false;
                                exportedBodyDirectly = false;
                            }
                        }
                            
                        if (!exportedAsWallWithAxis)
                        {
                            // Fallback - use native routines to try to export wall.
                            bool isCompletelyClipped;
                            bodyRep = FallbackTryToCreateAsExtrusion(exporterIFC, wallElement, solidMeshInfo,
                                catId, curve, plane, depth, zSpan, range, setter, 
                                out cutPairOpenings, out isCompletelyClipped);
                            if (isCompletelyClipped)
                                return null;
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                                exportedAsWallWithAxis = true;
                        }
                    }
                
                    using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
                    {
                        ElementId matId = ElementId.InvalidElementId;

                        if (!exportedAsWallWithAxis)
                        {
                            SolidMeshGeometryInfo solidMeshCapsule = null;

                            if (wallElement != null)
                            {
                                if (validRange)
                                {
                                    solidMeshCapsule = GeometryUtil.GetClippedSolidMeshGeometry(geometryElement, range);
                                }
                                else
                                {
                                    solidMeshCapsule = GeometryUtil.GetSplitSolidMeshGeometry(geometryElement);
                                }
                                if (solidMeshCapsule.SolidsCount() == 0 && solidMeshCapsule.MeshesCount() == 0)
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                GeometryElement geomElemToUse = GetGeometryFromInplaceWall(famInstWallElem);
                                if (geomElemToUse != null)
                                {
                                    exportingInplaceOpenings = true;
                                }
                                else
                                {
                                    exportingInplaceOpenings = false;
                                    geomElemToUse = geometryElement;
                                }
                                Transform trf = Transform.Identity;
                                if (geomElemToUse != geometryElement)
                                    trf = famInstWallElem.GetTransform();
                                solidMeshCapsule = GeometryUtil.GetSolidMeshGeometry(geomElemToUse, trf);
                            }

                            IList<Solid> solids = solidMeshCapsule.GetSolids();
                            IList<Mesh> meshes = solidMeshCapsule.GetMeshes();

                            extraParams.PossibleExtrusionAxes = IFCExtrusionAxes.TryZ;   // only allow vertical extrusions!
                            extraParams.AreInnerRegionsOpenings = true;

                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            if ((solids.Count > 0) || (meshes.Count > 0))
                            {
                                matId = BodyExporter.GetBestMaterialIdForGeometry(solids, meshes);
                                bodyRep = BodyExporter.ExportBody(element.Document.Application, exporterIFC, element, catId,
                                    solids, meshes, bodyExporterOptions, extraParams).RepresentationHnd;
                            }
                            else
                            {
                                IList<GeometryObject> geomElemList = new List<GeometryObject>();
                                geomElemList.Add(geometryElement);
                                BodyData bodyData = BodyExporter.ExportBody(element.Document.Application, exporterIFC, element, catId,
                                    geomElemList, bodyExporterOptions, extraParams);
                                bodyRep = bodyData.RepresentationHnd;
                            }

                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                            {
                                extraParams.ClearOpenings();
                                return null;
                            }

                            // We will be able to export as a IfcWallStandardCase as long as we have an axis curve.
                            XYZ extrDirUsed = XYZ.Zero;
                            if (extraParams.HasExtrusionDirection)
                            {
                                extrDirUsed = extraParams.ExtrusionDirection;
                                if (MathUtil.IsAlmostEqual(Math.Abs(extrDirUsed[2]), 1.0))
                                {
                                    if ((solids.Count == 1) && (meshes.Count == 0))
                                        exportedAsWallWithAxis = exportingAxis;
                                    exportedBodyDirectly = true;
                                }
                            }
                        }

                        IFCAnyHandle prodRep = null;
                        IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                        if (exportingAxis)
                            representations.Add(axisRep);

                        representations.Add(bodyRep);
                        prodRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);

                        string objectType = NamingUtil.CreateIFCObjectName(exporterIFC, element);
                        IFCAnyHandle wallHnd = null;

                        string elemGUID = (validRange) ? ExporterIFCUtils.CreateGUID() : ExporterIFCUtils.CreateGUID(element);
                        string elemName = NamingUtil.GetNameOverride(element, exporterIFC.GetName());
                        string elemDesc = NamingUtil.GetDescriptionOverride(element, null);
                        string elemObjectType = NamingUtil.GetObjectTypeOverride(element, objectType);
                        string elemId = NamingUtil.CreateIFCElementId(element);

                        if (exportedAsWallWithAxis)
                        {
                            wallHnd = IFCInstanceExporter.CreateWallStandardCase(file, elemGUID, ownerHistory, elemName, elemDesc, elemObjectType,
                                localPlacement, exportParts ? null : prodRep, elemId);

                            if (exportParts)
                                PartExporter.ExportHostPart(exporterIFC, wallElement, wallHnd, localWrapper, setter, localPlacement, overrideLevelId);

                            localWrapper.AddElement(wallHnd, setter, extraParams, true);

                            OpeningUtil.CreateOpeningsIfNecessary(wallHnd, element, cutPairOpenings, exporterIFC, localPlacement, setter, localWrapper);
                            if (exportedBodyDirectly)
                            {
                                OpeningUtil.CreateOpeningsIfNecessary(wallHnd, element, extraParams, exporterIFC, localPlacement, setter, localWrapper);
                            }
                            else
                            {
                                double scaledWidth = wallElement.Width * scale;
                                ExporterIFCUtils.AddOpeningsToElement(exporterIFC, wallHnd, wallElement, scaledWidth, range, setter, localPlacement, localWrapper);
                            }

                            // export Base Quantities
                            if (ExporterCacheManager.ExportOptionsCache.ExportBaseQuantities)
                            {
                                CreateWallBaseQuantities(exporterIFC, wallElement, wallHnd, depth);
                            }
                        }
                        else
                        {
                            wallHnd = IFCInstanceExporter.CreateWall(file, elemGUID, ownerHistory, elemName, elemDesc, elemObjectType,
                                localPlacement, exportParts ? null : prodRep, elemId);

                            if (exportParts)
                                PartExporter.ExportHostPart(exporterIFC, wallElement, wallHnd, localWrapper, setter, localPlacement, overrideLevelId);

                            localWrapper.AddElement(wallHnd, setter, extraParams, true);

                            // Only export one material for 2x2; for future versions, export the whole list.
                            if (exporterIFC.ExportAs2x2 && (matId != ElementId.InvalidElementId) && !exportParts)
                            {
                                CategoryUtil.CreateMaterialAssociation(doc, exporterIFC, wallHnd, matId);
                            }

                            if (exportingInplaceOpenings)
                            {
                                ExporterIFCUtils.AddOpeningsToElement(exporterIFC, wallHnd, famInstWallElem, 0.0, range, setter, localPlacement, localWrapper);
                            }

                            if (exportedBodyDirectly)
                                OpeningUtil.CreateOpeningsIfNecessary(wallHnd, element, extraParams, exporterIFC, localPlacement, setter, localWrapper);
                        }

                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, localWrapper);

                        ElementId wallLevelId = (validRange) ? setter.LevelId : ElementId.InvalidElementId;

                        if (wallElement != null && !exportParts)
                        {
                            if (!exporterIFC.ExportAs2x2 || exportedAsWallWithAxis) //will move this check into ExportHostObject
                                HostObjectExporter.ExportHostObjectMaterials(exporterIFC, wallElement, localWrapper.GetAnElement(),
                                    geometryElement, localWrapper, wallLevelId, Toolkit.IFCLayerSetDirection.Axis2);
                        }

                        exporterIFC.RegisterSpaceBoundingElementHandle(wallHnd, element.Id, wallLevelId);
                        return wallHnd;
                    }
                }
            }
        }

        /// <summary>
        /// Exports element as Wall.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportWall(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement,
           IFCProductWrapper productWrapper)
        {
            IList<IFCAnyHandle> createdWalls = new List<IFCAnyHandle>();

            // We will not split walls and columns if the assemblyId is set, as we would like to keep the original wall
            // associated with the assembly, on the level of the assembly.
            bool splitWall = ExporterCacheManager.ExportOptionsCache.WallAndColumnSplitting && (element.AssemblyInstanceId == ElementId.InvalidElementId);
            if (splitWall)
            {
                Wall wallElement = element as Wall;
                IList<ElementId> levels = new List<ElementId>();
                IList<IFCRange> ranges = new List<IFCRange>();
                if (wallElement != null && geometryElement != null)
                {
                    LevelUtil.CreateSplitLevelRangesForElement(exporterIFC, IFCExportType.ExportWall, element, out levels, out ranges);
                }

                int numPartsToExport = ranges.Count;
                if (numPartsToExport == 0)
                {
                    IFCAnyHandle wallElemHnd = ExportWallBase(exporterIFC, element, geometryElement, productWrapper, ElementId.InvalidElementId, null);
                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(wallElemHnd))
                        createdWalls.Add(wallElemHnd);
                }
                else
                {
                    IFCAnyHandle wallElemHnd = ExportWallBase(exporterIFC, element, geometryElement, productWrapper, levels[0], ranges[0]);
                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(wallElemHnd))
                        createdWalls.Add(wallElemHnd);
                    for (int ii = 1; ii < numPartsToExport; ii++)
                    {
                        wallElemHnd = ExportWallBase(exporterIFC, element, geometryElement, productWrapper, levels[ii], ranges[ii]);
                        if (!IFCAnyHandleUtil.IsNullOrHasNoValue(wallElemHnd))
                            createdWalls.Add(wallElemHnd);
                    }
                }

                if (ExporterCacheManager.DummyHostCache.HasRegistered(element.Id))
                {
                    List<KeyValuePair<ElementId, IFCRange>> levelRangeList = ExporterCacheManager.DummyHostCache.Find(element.Id);
                    foreach (KeyValuePair<ElementId, IFCRange> levelRange in levelRangeList)
                    {
                        IFCAnyHandle wallElemHnd = ExportDummyWall(exporterIFC, element, geometryElement, productWrapper, levelRange.Key, levelRange.Value);
                        if (!IFCAnyHandleUtil.IsNullOrHasNoValue(wallElemHnd))
                            createdWalls.Add(wallElemHnd);
                    }
                }
            }

            if (createdWalls.Count == 0)
                ExportWallBase(exporterIFC, element, geometryElement, productWrapper, ElementId.InvalidElementId, null);
        }

        /// <summary>
        /// Exports Walls.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void Export(ExporterIFC exporterIFC, Wall wallElement, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                //stacked wall is not supported yet.
                if (wallElement.WallType.Kind == WallKind.Stacked)
                    return;

                bool exportAsCurtainWall = wallElement.CurtainGrid != null;
                bool isOldCurtainWall = IsLegacyCurtainWall(wallElement); ;

                if (exportAsCurtainWall)
                {
                    if (!isOldCurtainWall)
                        CurtainSystemExporter.ExportWall(exporterIFC, wallElement, productWrapper);
                    else
                        CurtainSystemExporter.ExportLegacyCurtainElement(exporterIFC, wallElement, productWrapper);
                }
                else
                    ExportWall(exporterIFC, wallElement, geometryElement, productWrapper);

                // create join information.
                ElementId id = wallElement.Id;

                IList<IList<IFCConnectedWallData>> connectedWalls = new List<IList<IFCConnectedWallData>>();
                connectedWalls.Add(ExporterIFCUtils.GetConnectedWalls(wallElement, IFCConnectedWallDataLocation.Start));
                connectedWalls.Add(ExporterIFCUtils.GetConnectedWalls(wallElement, IFCConnectedWallDataLocation.End));
                for (int ii = 0; ii < 2; ii++)
                {
                    int count = connectedWalls[ii].Count;
                    IFCConnectedWallDataLocation currConnection = (ii == 0) ? IFCConnectedWallDataLocation.Start : IFCConnectedWallDataLocation.End;
                    for (int jj = 0; jj < count; jj++)
                    {
                        ElementId otherId = connectedWalls[ii][jj].ElementId;
                        IFCConnectedWallDataLocation joinedEnd = connectedWalls[ii][jj].Location;

                        if ((otherId == id) && (joinedEnd == currConnection))  //self-reference
                            continue;

                        ExporterCacheManager.WallConnectionDataCache.Add(new WallConnectionData(id, otherId, GetIFCConnectionTypeFromLocation(currConnection),
                            GetIFCConnectionTypeFromLocation(joinedEnd), null));
                    }
                }

                // look for connected columns.  Note that this is only for columns that interrupt the wall path.
                IList<FamilyInstance> attachedColumns = ExporterIFCUtils.GetAttachedColumns(wallElement);
                int numAttachedColumns = attachedColumns.Count;
                for (int ii = 0; ii < numAttachedColumns; ii++)
                {
                    ElementId otherId = attachedColumns[ii].Id;

                    IFCConnectionType connect1 = IFCConnectionType.NotDefined;   // can't determine at the moment.
                    IFCConnectionType connect2 = IFCConnectionType.NotDefined;   // meaningless for column

                    ExporterCacheManager.WallConnectionDataCache.Add(new WallConnectionData(id, otherId, connect1, connect2, null));
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Export the dummy wall to host an orphan part. It usually happens in the cases of associated parts are higher than split sub-wall.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The wall element.</param>
        /// <param name="geometryElement">The geometry of wall.</param>
        /// <param name="origWrapper">The IFCProductWrapper.</param>
        /// <param name="overrideLevelId">The ElementId that will crate the dummy wall.</param>
        /// <param name="range">The IFCRange corresponding to the dummy wall.</param>
        /// <returns>The handle of dummy wall.</returns>
        public static IFCAnyHandle ExportDummyWall(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement,
           IFCProductWrapper origWrapper, ElementId overrideLevelId, IFCRange range)
        {
            using (IFCProductWrapper localWrapper = IFCProductWrapper.Create(origWrapper))
            {
                ElementId catId = CategoryUtil.GetSafeCategoryId(element);

                Wall wallElement = element as Wall;
                if (wallElement == null)
                    return null;

                if (wallElement != null && IsWallCompletelyClipped(wallElement, exporterIFC, range))
                    return null;

                // get global values.
                Document doc = element.Document;
                double scale = exporterIFC.LinearScale;

                IFCFile file = exporterIFC.GetFile();
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                bool validRange = (range != null && !MathUtil.IsAlmostZero(range.Start - range.End));

                bool exportParts = PartExporter.CanExportParts(wallElement);
                if (exportParts && !PartExporter.CanExportElementInPartExport(wallElement, validRange ? overrideLevelId : wallElement.Level.Id, validRange))
                    return null;

                string objectType = NamingUtil.CreateIFCObjectName(exporterIFC, element);
                IFCAnyHandle wallHnd = null;

                string elemGUID = (validRange) ? ExporterIFCUtils.CreateGUID() : ExporterIFCUtils.CreateGUID(element);
                string elemName = NamingUtil.GetNameOverride(element, exporterIFC.GetName());
                string elemDesc = NamingUtil.GetDescriptionOverride(element, null);
                string elemObjectType = NamingUtil.GetObjectTypeOverride(element, objectType);
                string elemId = NamingUtil.CreateIFCElementId(element);

                Transform orientationTrf = Transform.Identity;

                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element, null, orientationTrf, overrideLevelId))
                {
                    IFCAnyHandle localPlacement = setter.GetPlacement();
                    wallHnd = IFCInstanceExporter.CreateWall(file, elemGUID, ownerHistory, elemName, elemDesc, elemObjectType,
                                    localPlacement, null, elemId);

                    if (exportParts)
                        PartExporter.ExportHostPart(exporterIFC, wallElement, wallHnd, localWrapper, setter, localPlacement, overrideLevelId);

                    IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData();
                    extraParams.PossibleExtrusionAxes = IFCExtrusionAxes.TryZ;   // only allow vertical extrusions!
                    extraParams.AreInnerRegionsOpenings = true;
                    localWrapper.AddElement(wallHnd, setter, extraParams, true);

                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, localWrapper);

                    ElementId wallLevelId = (validRange) ? setter.LevelId : ElementId.InvalidElementId;
                    exporterIFC.RegisterSpaceBoundingElementHandle(wallHnd, element.Id, wallLevelId);
                }
                
                return wallHnd;
            }
        }

        /// <summary>
        /// Checks if the wall is clipped completely.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <param name="range">
        /// The range of which may clip the wall.
        /// </param>
        /// <returns>
        /// True if the wall is clipped completely, false otherwise.
        /// </returns>
        static bool IsWallCompletelyClipped(Wall wallElement, ExporterIFC exporterIFC, IFCRange range)
        {
            return ExporterIFCUtils.IsWallCompletelyClipped(wallElement, exporterIFC, range);
        }

        /// <summary>
        /// Gets wall axis.
        /// </summary>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <returns>
        /// The curve.
        /// </returns>
        static Curve GetWallAxis(Wall wallElement)
        {
            if (wallElement == null)
                return null;
            LocationCurve locationCurve = wallElement.Location as LocationCurve;
            return locationCurve.Curve;
        }

        /// <summary>
        /// Gets wall axis at base height.
        /// </summary>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <returns>
        /// The curve.
        /// </returns>
        static Curve GetWallAxisAtBaseHeight(Wall wallElement)
        {
            LocationCurve locationCurve = wallElement.Location as LocationCurve;
            Curve nonBaseCurve = locationCurve.Curve;

            double baseOffset = ExporterIFCUtils.GetWallBaseOffset(wallElement);

            Transform trf = Transform.get_Translation(new XYZ(0, 0, baseOffset));

            return nonBaseCurve.get_Transformed(trf);
        }

        /// <summary>
        /// Gets wall trimmed curve.
        /// </summary>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <param name="baseCurve">
        /// The base curve.
        /// </param>
        /// <returns>
        /// The curve.
        /// </returns>
        static Curve GetWallTrimmedCurve(Wall wallElement, Curve baseCurve)
        {
            Curve result = ExporterIFCUtils.GetWallTrimmedCurve(wallElement);
            if (result == null)
                return baseCurve;

            return result;
        }

        /// <summary>
        /// Identifies if the wall has a sketched elevation profile.
        /// </summary>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <returns>
        /// True if the wall has a sketch elevation profile, false otherwise.
        /// </returns>
        static bool HasElevationProfile(Wall wallElement)
        {
            return ExporterIFCUtils.HasElevationProfile(wallElement);
        }

        /// <summary>
        /// Obtains the curve loops which bound the wall's elevation profile.
        /// </summary>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <returns>
        /// The collection of curve loops.
        /// </returns>
        static IList<CurveLoop> GetElevationProfile(Wall wallElement)
        {
            return ExporterIFCUtils.GetElevationProfile(wallElement);
        }

        /// <summary>
        /// Identifies if the base geometry of the wall can be represented as an extrusion.
        /// </summary>
        /// <param name="element">
        /// The wall element.
        /// </param>
        /// <param name="range">
        /// The range. This consists of two double values representing the height in Z at the start and the end
        /// of the range.  If the values are identical the entire wall is used.
        /// </param>
        /// <returns>
        /// True if the wall export can be made in the form of an extrusion, false if the
        /// geometry cannot be assigned to an extrusion.
        /// </returns>
        static bool CanExportWallGeometryAsExtrusion(Element element, IFCRange range)
        {
            return ExporterIFCUtils.CanExportWallGeometryAsExtrusion(element, range);
        }

        /// <summary>
        /// Obtains a special snapshot of the geometry of an in-place wall element suitable for export.
        /// </summary>
        /// <param name="famInstWallElem">
        /// The in-place wall instance.
        /// </param>
        /// <returns>
        /// The in-place wall geometry.
        /// </returns>
        static GeometryElement GetGeometryFromInplaceWall(FamilyInstance famInstWallElem)
        {
            return ExporterIFCUtils.GetGeometryFromInplaceWall(famInstWallElem);
        }

        /// <summary>
        /// Obtains a special snapshot of the geometry of an in-place wall element suitable for export.
        /// </summary>
        /// <param name="wallElement">
        /// The wall.
        /// </param>
        /// <returns>
        /// The direction of the wall.
        /// </returns>
        static XYZ GetWallHeightDirection(Wall wallElement)
        {
            return ExporterIFCUtils.GetWallHeightDirection(wallElement);
        }

        /// <summary>
        /// Identifies if the wall's base can be represented by a direct thickening of the wall's base curve.
        /// </summary>
        /// <param name="wallElement">
        /// The wall.
        /// </param>
        /// <param name="curve">
        /// The wall's base curve.
        /// </param>
        /// <returns>
        /// True if the wall's base can be represented by a direct thickening of the wall's base curve.
        /// False is the wall's base shape is affected by other geometry, and thus cannot be represented
        /// by a direct thickening of the wall's base cure.
        /// </returns>
        static bool IsWallBaseRectangular(Wall wallElement, Curve curve)
        {
            return ExporterIFCUtils.IsWallBaseRectangular(wallElement, curve);
        }

        /// <summary>
        /// Gets the curve loop(s) that represent the bottom or top face of the wall.
        /// </summary>
        /// <param name="wallElement">
        /// The wall.
        /// </param>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <returns>
        /// The curve loops.
        /// </returns>
        static IList<CurveLoop> GetLoopsFromTopBottomFace(Wall wallElement, ExporterIFC exporterIFC)
        {
            return ExporterIFCUtils.GetLoopsFromTopBottomFace(exporterIFC, wallElement);
        }

        /// <summary>
        /// Processes the geometry of the wall to create an extruded area solid representing the geometry of the wall (including
        /// any clippings imposed by neighboring elements).
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="wallElement">
        /// The wall.
        /// </param>
        /// <param name="setterOffset">
        /// The offset from the placement setter.
        /// </param>
        /// <param name="range">
        /// The range.  This consists of two double values representing the height in Z at the start and the end
        /// of the range.  If the values are identical the entire wall is used.
        /// </param>
        /// <param name="zSpan">
        /// The overall span in Z of the wall.
        /// </param>
        /// <param name="baseBodyItemHnd">
        /// The IfcExtrudedAreaSolid handle generated initially for the wall.
        /// </param>
        /// <param name="cutPairOpenings">
        /// A collection of extruded openings that can be derived from the wall geometry.
        /// </param>
        /// <returns>
        /// IfcEtxtrudedAreaSolid handle.  This may be the same handle as was input, or a modified handle derived from the clipping
        /// geometry.  If the function fails this handle will have no value.
        /// </returns>
        static IFCAnyHandle AddClippingsToBaseExtrusion(ExporterIFC exporterIFC, Wall wallElement,
           XYZ setterOffset, IFCRange range, IFCRange zSpan, IFCAnyHandle baseBodyItemHnd, out IList<IFCExtrusionData> cutPairOpenings)
        {
            return ExporterIFCUtils.AddClippingsToBaseExtrusion(exporterIFC, wallElement, setterOffset, range, zSpan, baseBodyItemHnd, out cutPairOpenings);
        }

        /// <summary>
        /// Creates the wall base quantities and adds them to the export.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="wallHnd">
        /// The wall element handle.
        /// </param>
        /// <param name="wallElement">
        /// The wall element.
        /// </param>
        /// <param name="depth">
        /// The depth of the wall.
        /// </param>
        static void CreateWallBaseQuantities(ExporterIFC exporterIFC, Wall wallElement, IFCAnyHandle wallHnd, double depth)
        {
            ExporterIFCUtils.CreateWallBaseQuantities(exporterIFC, wallHnd, wallElement, depth);
        }

        /// <summary>
        /// Gets IFCConnectionType from IFCConnectedWallDataLocation.
        /// </summary>
        /// <param name="location">The IFCConnectedWallDataLocation.</param>
        /// <returns>The IFCConnectionType.</returns>
        static IFCConnectionType GetIFCConnectionTypeFromLocation(IFCConnectedWallDataLocation location)
        {
            switch(location)
            {
                case IFCConnectedWallDataLocation.Start:
                    return IFCConnectionType.AtStart;
                case IFCConnectedWallDataLocation.End:
                    return IFCConnectionType.AtEnd;
                case IFCConnectedWallDataLocation.Path:
                    return IFCConnectionType.AtPath;
                case IFCConnectedWallDataLocation.NotDefined:
                    return IFCConnectionType.NotDefined;
                default:
                    throw new ArgumentException("Invalid IFCConnectedWallDataLocation", "location");
            }
        }

        /// <summary>
        /// Checks if the wall is legacy curtain wall.
        /// </summary>
        /// <param name="wall">The wall.</param>
        /// <returns>True if it is legacy curtain wall, false otherwise.</returns>
        static bool IsLegacyCurtainWall(Wall wall)
        {
            try
            {
                CurtainGrid curtainGrid = wall.CurtainGrid;
                if (curtainGrid != null)
                {
                    curtainGrid.GetPanelIds();
                }
                else
                    return false;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
            	 if (ex.Message == "The host object is obsolete.")
                     return true;
                 else
                     throw ex;
            }

            return false;
        }
    }
}
