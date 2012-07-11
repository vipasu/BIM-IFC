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
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;


namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export spatial elements.
    /// </summary>
    class SpatialElementExporter
    {
        /// <summary>
        /// The SpatialElementGeometryCalculator object that contains some useful results from previous calculator.
        /// </summary>
        private static SpatialElementGeometryCalculator s_SpatialElementGeometryCalculator = null;

        /// <summary>
        /// Initializes SpatialElementGeometryCalculator object.
        /// </summary>
        /// <param name="document">
        /// The Revit document.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        public static void InitializeSpatialElementGeometryCalculator(Document document, ExporterIFC exporterIFC)
        {
            SpatialElementBoundaryOptions options = ExporterIFCUtils.GetSpatialElementBoundaryOptions(exporterIFC, null);
            s_SpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(document, options);
        }

        /// <summary>
        /// Exports spatial elements, including rooms, areas and spaces. 1st level space boundaries.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="spatialElement">
        /// The spatial element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportSpatialElement(ExporterIFC exporterIFC, SpatialElement spatialElement, IFCProductWrapper productWrapper)
        {
            //quick reject
            bool isArea = spatialElement is Area;
            if (isArea)
            {
                if (!IsAreaGrossInterior(exporterIFC, spatialElement))
                    return;
            }

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                ElementId levelId = spatialElement.Level != null ? spatialElement.Level.Id : ElementId.InvalidElementId;
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, spatialElement, null, null, levelId))
                {
                    if (!CreateIFCSpace(exporterIFC, spatialElement, productWrapper, setter))
                        return;

                    // Do not create boundary information, or extra property sets.
                    if (spatialElement is Area)
                    {
                        transaction.Commit();
                        return;
                    }

                    if (ExporterCacheManager.ExportOptionsCache.SpaceBoundaryLevel == 1)
                    {
                        Document document = spatialElement.Document;
                        IFCLevelInfo levelInfo = exporterIFC.GetLevelInfo(levelId);
                        double baseHeightNonScaled = levelInfo.Elevation;

                        SpatialElementGeometryResults spatialElemGeomResult = s_SpatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

                        Solid spatialElemGeomSolid = spatialElemGeomResult.GetGeometry();
                        FaceArray faces = spatialElemGeomSolid.Faces;
                        foreach (Face face in faces)
                        {
                            IList<SpatialElementBoundarySubface> spatialElemBoundarySubfaces = spatialElemGeomResult.GetBoundaryFaceInfo(face);
                            foreach (SpatialElementBoundarySubface spatialElemBSubface in spatialElemBoundarySubfaces)
                            {
                                if (spatialElemBSubface.SubfaceType == SubfaceType.Side)
                                    continue;

                                if (spatialElemBSubface.GetSubface() == null)
                                    continue;

                                ElementId elemId = spatialElemBSubface.SpatialBoundaryElement.LinkInstanceId;
                                if (elemId == ElementId.InvalidElementId)
                                {
                                    elemId = spatialElemBSubface.SpatialBoundaryElement.HostElementId;
                                }

                                Element boundingElement = document.GetElement(elemId);
                                if (boundingElement == null)
                                    continue;

                                bool isObjectExt = CategoryUtil.IsElementExternal(boundingElement);

                                IFCGeometryInfo info = IFCGeometryInfo.CreateSurfaceGeometryInfo(spatialElement.Document.Application.VertexTolerance);

                                Face subFace = spatialElemBSubface.GetSubface();
                                ExporterIFCUtils.CollectGeometryInfo(exporterIFC, info, subFace, XYZ.Zero, false);

                                foreach (IFCAnyHandle surfaceHnd in info.GetSurfaces())
                                {
                                    IFCAnyHandle connectionGeometry = IFCInstanceExporter.CreateConnectionSurfaceGeometry(file, surfaceHnd, null);

                                    SpaceBoundary spaceBoundary = new SpaceBoundary(spatialElement.Id, boundingElement.Id, setter.LevelId, connectionGeometry, IFCPhysicalOrVirtual.Physical,
                                        isObjectExt ? IFCInternalOrExternal.External : IFCInternalOrExternal.Internal);

                                    if (!ProcessIFCSpaceBoundary(exporterIFC, spaceBoundary, file))
                                        ExporterCacheManager.SpaceBoundaryCache.Add(spaceBoundary);
                                }
                            }
                        }

                        IList<IList<BoundarySegment>> roomBoundaries = spatialElement.GetBoundarySegments(ExporterIFCUtils.GetSpatialElementBoundaryOptions(exporterIFC, spatialElement));
                        double roomHeight = GetHeight(spatialElement, exporterIFC.LinearScale, levelId, levelInfo);
                        XYZ zDir = new XYZ(0, 0, 1);

                        foreach (IList<BoundarySegment> roomBoundaryList in roomBoundaries)
                        {
                            foreach (BoundarySegment roomBoundary in roomBoundaryList)
                            {
                                Element boundingElement = roomBoundary.Element;

                                if (boundingElement == null)
                                    continue;

                                ElementId buildingElemId = boundingElement.Id;
                                Curve trimmedCurve = roomBoundary.Curve;

                                if (trimmedCurve == null)
                                    continue;

                                //trimmedCurve.Visibility = Visibility.Visible; readonly
                                IFCAnyHandle connectionGeometry = ExporterIFCUtils.CreateExtrudedSurfaceFromCurve(
                                   exporterIFC, trimmedCurve, zDir, roomHeight, baseHeightNonScaled);

                                IFCPhysicalOrVirtual physOrVirt = IFCPhysicalOrVirtual.Physical;
                                if (boundingElement is CurveElement)
                                    physOrVirt = IFCPhysicalOrVirtual.Virtual;
                                else if (boundingElement is Autodesk.Revit.DB.Architecture.Room)
                                    physOrVirt = IFCPhysicalOrVirtual.NotDefined;

                                bool isObjectExt = CategoryUtil.IsElementExternal(boundingElement);
                                bool isObjectPhys = (physOrVirt == IFCPhysicalOrVirtual.Physical);

                                ElementId actualBuildingElemId = isObjectPhys ? buildingElemId : ElementId.InvalidElementId;

                                SpaceBoundary spaceBoundary = new SpaceBoundary(spatialElement.Id, actualBuildingElemId, setter.LevelId, !IFCAnyHandleUtil.IsNullOrHasNoValue(connectionGeometry) ? connectionGeometry : null,
                                    physOrVirt, isObjectExt ? IFCInternalOrExternal.External : IFCInternalOrExternal.Internal);

                                if (!ProcessIFCSpaceBoundary(exporterIFC, spaceBoundary, file))
                                    ExporterCacheManager.SpaceBoundaryCache.Add(spaceBoundary);

                                // try to add doors and windows for host objects if appropriate.
                                if (isObjectPhys && boundingElement is HostObject)
                                {
                                    HostObject hostObj = boundingElement as HostObject;
                                    IList<ElementId> elemIds = hostObj.FindInserts(false, false, false, false);
                                    foreach (ElementId elemId in elemIds)
                                    {
                                        // we are going to do a simple bbox export, not complicated geometry.
                                        Element instElem = document.GetElement(elemId);
                                        if (instElem == null)
                                            continue;

                                        BoundingBoxXYZ instBBox = instElem.get_BoundingBox(null);

                                        // make copy of original trimmed curve.
                                        Curve instCurve = trimmedCurve.Clone();
                                        XYZ instOrig = instCurve.get_EndPoint(0);

                                        // make sure that the insert is on this level.
                                        if (instBBox.Max.Z < instOrig.Z)
                                            continue;
                                        if (instBBox.Min.Z > instOrig.Z + roomHeight)
                                            continue;

                                        double insHeight = Math.Min(instBBox.Max.Z, instOrig.Z + roomHeight) - Math.Max(instOrig.Z, instBBox.Min.Z);
                                        if (insHeight < (1.0 / (12.0 * 16.0)))
                                            continue;

                                        // move base curve to bottom of bbox.
                                        XYZ moveDir = new XYZ(0.0, 0.0, instBBox.Min.Z - instOrig.Z);
                                        Transform moveTrf = Transform.get_Translation(moveDir);
                                        instCurve = instCurve.get_Transformed(moveTrf);

                                        bool isHorizOrVert = false;
                                        if (instCurve is Line)
                                        {
                                            Line instLine = instCurve as Line;
                                            XYZ lineDir = instLine.Direction;
                                            if (MathUtil.IsAlmostEqual(Math.Abs(lineDir.X), 1.0) || (MathUtil.IsAlmostEqual(Math.Abs(lineDir.Y), 1.0)))
                                                isHorizOrVert = true;
                                        }

                                        double[] parameters = new double[2];
                                        double[] origEndParams = new double[2];
                                        if (isHorizOrVert)
                                        {
                                            parameters[0] = instCurve.Project(instBBox.Min).Parameter;
                                            parameters[1] = instCurve.Project(instBBox.Max).Parameter;
                                        }
                                        else
                                        {
                                            FamilyInstance famInst = instElem as FamilyInstance;
                                            if (famInst == null)
                                                continue;

                                            ElementType elementType = document.GetElement(famInst.GetTypeId()) as ElementType;
                                            if (elementType == null)
                                                continue;

                                            BoundingBoxXYZ symBBox = elementType.get_BoundingBox(null);
                                            Curve symCurve = trimmedCurve.Clone();
                                            Transform trf = famInst.GetTransform();
                                            Transform invTrf = trf.Inverse;
                                            Curve trfCurve = symCurve.get_Transformed(invTrf);
                                            parameters[0] = trfCurve.Project(symBBox.Min).Parameter;
                                            parameters[1] = trfCurve.Project(symBBox.Max).Parameter;
                                        }

                                        // ignore if less than 1/16".
                                        if (Math.Abs(parameters[1] - parameters[0]) < 1.0 / (12.0 * 16.0))
                                            continue;
                                        if (parameters[0] > parameters[1])
                                        {
                                            //swap
                                            double tempParam = parameters[0];
                                            parameters[0] = parameters[1];
                                            parameters[1] = tempParam;
                                        }

                                        origEndParams[0] = instCurve.get_EndParameter(0);
                                        origEndParams[1] = instCurve.get_EndParameter(1);

                                        if (origEndParams[0] > parameters[1] - (1.0 / (12.0 * 16.0)))
                                            continue;
                                        if (origEndParams[1] < parameters[0] + (1.0 / (12.0 * 16.0)))
                                            continue;

                                        if (parameters[0] > origEndParams[0])
                                            instCurve.set_EndParameter(0, parameters[0]);
                                        if (parameters[1] < origEndParams[1])
                                            instCurve.set_EndParameter(1, parameters[1]);

                                        IFCAnyHandle insConnectionGeom = ExporterIFCUtils.CreateExtrudedSurfaceFromCurve(exporterIFC, instCurve, zDir,
                                           insHeight, baseHeightNonScaled);

                                        SpaceBoundary instBoundary = new SpaceBoundary(spatialElement.Id, elemId, setter.LevelId, !IFCAnyHandleUtil.IsNullOrHasNoValue(insConnectionGeom) ? insConnectionGeom : null, physOrVirt,
                                            isObjectExt ? IFCInternalOrExternal.External : IFCInternalOrExternal.Internal);
                                        if (!ProcessIFCSpaceBoundary(exporterIFC, instBoundary, file))
                                            ExporterCacheManager.SpaceBoundaryCache.Add(instBoundary);
                                    }
                                }
                            }
                        }
                    }
                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, spatialElement, productWrapper);
                    CreateZoneInfos(exporterIFC, file, spatialElement, productWrapper);
                    CreateSpaceOccupantInfo(exporterIFC, file, spatialElement, productWrapper);
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Exports spatial elements, including rooms, areas and spaces. 2nd level space boundaries.
        /// </summary>
        /// <param name="ifcExporter">
        /// The Exporter object.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="document">
        /// The Revit document.
        /// </param>
        /// <param name="filterView">
        /// The view not exported.
        /// </param>
        /// <param name="spaceExported">
        /// The output boolean value indicates if exported successfully.
        /// </param>
        public static void ExportSpatialElement2ndLevel(BIM.IFC.Exporter.Exporter ifcExporter, ExporterIFC exporterIFC, Document document, View filterView, ref bool spaceExported)
        {
            using (SubTransaction st = new SubTransaction(document))
            {
                st.Start();

                EnergyAnalysisDetailModel model = null;
                try
                {
                    IFCFile file = exporterIFC.GetFile();
                    using (IFCTransaction transaction = new IFCTransaction(file))
                    {

                        EnergyAnalysisDetailModelOptions options = new EnergyAnalysisDetailModelOptions();
                        options.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries; //2nd level space boundaries
                        options.SimplifyCurtainSystems = true;
                        try
                        {
                            model = EnergyAnalysisDetailModel.Create(document, options);
                        }
                        catch (System.Exception)
                        {
                            spaceExported = false;
                            return;
                        }

                        IList<EnergyAnalysisSpace> spaces = model.GetAnalyticalSpaces();
                        spaceExported = true;

                        foreach (EnergyAnalysisSpace space in spaces)
                        {
                            SpatialElement spatialElement = document.GetElement(space.SpatialElementId) as SpatialElement;

                            if (spatialElement == null)
                                continue;

                            //quick reject
                            bool isArea = spatialElement is Area;
                            if (isArea)
                            {
                                if (!IsAreaGrossInterior(exporterIFC, spatialElement))
                                    continue;
                            }

                            //current view only
                            if (filterView != null && !ElementFilteringUtil.IsElementVisible(filterView, spatialElement))
                                continue;
                            //

                            if (!ElementFilteringUtil.ShouldCategoryBeExported(exporterIFC, spatialElement))
                                continue;

                            Options geomOptions = GeometryUtil.GetIFCExportGeometryOptions();
                            View ownerView = spatialElement.Document.GetElement(spatialElement.OwnerViewId) as View;
                            if (ownerView != null)
                                geomOptions.View = ownerView;
                            GeometryElement geomElem = spatialElement.get_Geometry(geomOptions);

                            try
                            {
                                exporterIFC.PushExportState(spatialElement, geomElem);

                                using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(exporterIFC, true))
                                {
                                    ElementId levelId = spatialElement.Level != null ? spatialElement.Level.Id : ElementId.InvalidElementId;
                                    using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, spatialElement, null, null, levelId))
                                    {
                                        if (!CreateIFCSpace(exporterIFC, spatialElement, productWrapper, setter))
                                            continue;

                                        XYZ offset = GetSapceBoundaryOffset(setter);

                                        //get boundary information from surfaces
                                        IList<EnergyAnalysisSurface> surfaces = space.GetAnalyticalSurfaces();
                                        foreach (EnergyAnalysisSurface surface in surfaces)
                                        {
                                            Element boundingElement = GetBoundaryElement(document, surface.OriginatingElementId);

                                            IList<EnergyAnalysisOpening> openings = surface.GetAnalyticalOpenings();
                                            IFCAnyHandle connectionGeometry = CreateConnectionSurfaceGeometry(file, surface, openings, offset);
                                            CreateIFCSpaceBoundary(file, exporterIFC, spatialElement, boundingElement, setter.LevelId, connectionGeometry);

                                            // try to add doors and windows for host objects if appropriate.
                                            if (boundingElement is HostObject)
                                            {
                                                foreach (EnergyAnalysisOpening opening in openings)
                                                {
                                                    Element openingBoundingElem = GetBoundaryElement(document, opening.OriginatingElementId);
                                                    IFCAnyHandle openingConnectionGeom = CreateConnectionSurfaceGeometry(file, opening, offset);
                                                    CreateIFCSpaceBoundary(file, exporterIFC, spatialElement, openingBoundingElem, setter.LevelId, openingConnectionGeom);
                                                }
                                            }
                                        }
                                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, spatialElement, productWrapper);
                                        CreateZoneInfos(exporterIFC, file, spatialElement, productWrapper);
                                        CreateSpaceOccupantInfo(exporterIFC, file, spatialElement, productWrapper);
                                        ifcExporter.ExportElementProperties(exporterIFC, spatialElement, productWrapper);
                                        ifcExporter.ExportElementQuantities(exporterIFC, spatialElement, productWrapper);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ifcExporter.HandleUnexpectedException(ex, exporterIFC, spatialElement);
                            }
                            finally
                            {
                                exporterIFC.PopExportState();
                            }
                        }
                        transaction.Commit();
                    }
                }
                finally
                {
                    if (model != null)
                        EnergyAnalysisDetailModel.Destroy(model);
                }

                st.RollBack();
            }
        }

        /// <summary>
        /// Creates SpaceBoundary from a bounding element.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="spatialElement">
        /// The spatial element.
        /// </param>
        /// <param name="boundingElement">
        /// The bounding element.
        /// </param>
        /// <param name="levelId">
        /// The level id.
        /// </param>
        /// <param name="connectionGeometry">
        /// The connection geometry handle.
        /// </param>
        static void CreateIFCSpaceBoundary(IFCFile file, ExporterIFC exporterIFC, SpatialElement spatialElement, Element boundingElement, ElementId levelId, IFCAnyHandle connectionGeometry)
        {
            IFCPhysicalOrVirtual physOrVirt = IFCPhysicalOrVirtual.Physical;
            if (boundingElement == null || boundingElement is CurveElement)
                physOrVirt = IFCPhysicalOrVirtual.Virtual;
            else if (boundingElement is Autodesk.Revit.DB.Architecture.Room)
                physOrVirt = IFCPhysicalOrVirtual.NotDefined;

            bool isObjectExt = CategoryUtil.IsElementExternal(boundingElement);

            SpaceBoundary spaceBoundary = new SpaceBoundary(spatialElement.Id, boundingElement != null ? boundingElement.Id : ElementId.InvalidElementId,
                levelId, connectionGeometry, physOrVirt, isObjectExt ? IFCInternalOrExternal.External : IFCInternalOrExternal.Internal);

            if (!ProcessIFCSpaceBoundary(exporterIFC, spaceBoundary, file))
                ExporterCacheManager.SpaceBoundaryCache.Add(spaceBoundary);
        }

        /// <summary>
        /// Gets element from LinkElementId.
        /// </summary>
        /// <param name="document">
        /// The Revit document.
        /// </param>
        /// <param name="linkElementId">
        /// The link element id.
        /// </param>
        /// <returns>
        /// The element.
        /// </returns>
        static Element GetBoundaryElement(Document document, LinkElementId linkElementId)
        {
            ElementId elemId = linkElementId.LinkInstanceId;
            if (elemId == ElementId.InvalidElementId)
            {
                elemId = linkElementId.HostElementId;
            }
            return document.GetElement(elemId);
        }

        /// <summary>
        /// Creates IFC connection surface geometry from a surface object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="surface">
        /// The EnergyAnalysisSurface.
        /// </param>
        /// <param name="openings">
        /// List of EnergyAnalysisOpenings.
        /// </param>
        /// <param name="offset">
        /// The offset of the geometry.
        /// </param>
        /// <returns>
        /// The connection geometry handle.
        /// </returns>
        static IFCAnyHandle CreateConnectionSurfaceGeometry(IFCFile file, EnergyAnalysisSurface surface, IList<EnergyAnalysisOpening> openings, XYZ offset)
        {
            Polyloop outerLoop = surface.GetPolyloop();
            IList<XYZ> outerLoopPoints = outerLoop.GetPoints();

            List<XYZ> newOuterLoopPoints = new List<XYZ>();
            foreach (XYZ point in outerLoopPoints)
            {
                newOuterLoopPoints.Add(point.Subtract(offset));
            }

            IList<IList<XYZ>> innerLoopPoints = new List<IList<XYZ>>();
            foreach (EnergyAnalysisOpening opening in openings)
            {
                IList<XYZ> openingPoints = opening.GetPolyloop().GetPoints();
                List<XYZ> newOpeningPoints = new List<XYZ>();
                foreach (XYZ openingPoint in openingPoints)
                {
                    newOpeningPoints.Add(openingPoint.Subtract(offset));
                }
                innerLoopPoints.Add(newOpeningPoints);
            }

            IFCAnyHandle hnd = file.CreateCurveBoundedPlane(newOuterLoopPoints, innerLoopPoints);

            return IFCInstanceExporter.CreateConnectionSurfaceGeometry(file, hnd, null);
        }

        /// <summary>
        /// Creates IFC connection surface geometry from an opening object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="opening">
        /// The EnergyAnalysisOpening.
        /// </param>
        /// <param name="offset">
        /// The offset of opening.
        /// </param>
        /// <returns>
        /// The connection surface geometry handle.
        /// </returns>
        static IFCAnyHandle CreateConnectionSurfaceGeometry(IFCFile file, EnergyAnalysisOpening opening, XYZ offset)
        {
            Polyloop outerLoop = opening.GetPolyloop();
            IList<XYZ> outerLoopPoints = outerLoop.GetPoints();

            List<XYZ> newOuterLoopPoints = new List<XYZ>();
            foreach (XYZ point in outerLoopPoints)
            {
                newOuterLoopPoints.Add(point.Subtract(offset));
            }

            IList<IList<XYZ>> innerLoopPoints = new List<IList<XYZ>>();

            IFCAnyHandle hnd = file.CreateCurveBoundedPlane(newOuterLoopPoints, innerLoopPoints);

            return IFCInstanceExporter.CreateConnectionSurfaceGeometry(file, hnd, null);
        }

        /// <summary>
        /// Checks if the spatial element is gross interior.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="spatialElement">
        /// The spatial element.
        /// </param>
        /// <returns>
        /// True if the area is gross interior.
        /// </returns>
        static bool IsAreaGrossInterior(ExporterIFC exporterIFC, SpatialElement spatialElement)
        {
            Area area = spatialElement as Area;
            if (area != null)
            {
                double scale = exporterIFC.LinearScale;

                double dArea = 0.0;
                Parameter paramRoomArea = area.get_Parameter(BuiltInParameter.ROOM_AREA);
                if (paramRoomArea != null && paramRoomArea.StorageType == StorageType.Double)
                {
                    dArea = paramRoomArea.AsDouble();
                    dArea *= (scale * scale);
                }  // convert scale to export scale; area is scale squared.

                if (!MathUtil.IsAlmostZero(dArea) && area.IsGrossInterior)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the height of a spatial element.
        /// </summary>
        /// <param name="spatialElement">
        /// The spatial element.
        /// </param>
        /// <param name="scale">
        /// The scale value.
        /// </param>
        /// <param name="levelInfo">
        /// The level info.
        /// </param>
        /// <returns>
        /// The height.
        /// </returns>
        static double GetHeight(SpatialElement spatialElement, double scale, ElementId levelId, IFCLevelInfo levelInfo)
        {
            Document document = spatialElement.Document;

            double roomHeight = 0.0;
            double bottomOffset = 0.0;

            Parameter paramTopLevelId = spatialElement.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
            ElementId topLevelId = paramTopLevelId != null ? paramTopLevelId.AsElementId() : ElementId.InvalidElementId;

            Parameter paramTopOffset = spatialElement.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
            double topOffset = paramTopOffset != null ? paramTopOffset.AsDouble() : 0.0;

            Parameter paramBottomOffset = spatialElement.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET);
            bottomOffset = paramBottomOffset != null ? paramBottomOffset.AsDouble() : 0.0;

            Level bottomLevel = document.GetElement(levelId) as Level;
            Level topLevel =
               (levelId == topLevelId) ? bottomLevel : document.GetElement(topLevelId) as Level;

            if (bottomLevel != null && topLevel != null)
            {
                roomHeight = (topLevel.Elevation - bottomLevel.Elevation) + (topOffset - bottomOffset);
                roomHeight *= scale;
            }

            if (MathUtil.IsAlmostZero(roomHeight))
            {
                double levelHeight = ExporterCacheManager.LevelInfoCache.FindHeight(levelId);
                if (levelHeight < 0.0)
                    levelHeight = LevelUtil.calculateDistanceToNextLevel(document, levelId, levelInfo);

                roomHeight = levelHeight * scale;
            }

            // For area spaces, we assign a dummy height (1'), as we are not allowed to export IfcSpaces without a volumetric representation.
            if (MathUtil.IsAlmostZero(roomHeight) && spatialElement is Area)
            {
                roomHeight = 1.0;
            }

            return roomHeight;
        }

        /// <summary>
        /// Gets the offset of the space boundary.
        /// </summary>
        /// <param name="setter">The placement settter.</param>
        /// <returns>The offset.</returns>
        static XYZ GetSapceBoundaryOffset(IFCPlacementSetter setter)
        {
            IFCAnyHandle localPlacement = setter.GetPlacement();
            double zOffset = setter.Offset;

            IFCAnyHandle relPlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(relPlacement))
            {
                IFCAnyHandle ptHnd = IFCAnyHandleUtil.GetLocation(relPlacement);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(ptHnd))
                {
                    IList<double> addToCoords = IFCAnyHandleUtil.GetCoordinates(ptHnd);
                    return new XYZ(addToCoords[0], addToCoords[1], addToCoords[2] + zOffset);
                }
            }

            return new XYZ(0, 0, zOffset);
        }

        /// <summary>
        /// Creates space boundary.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="boundary">
        /// The space boundary object.
        /// </param>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <returns>
        /// True if processed successfully, false otherwise.
        /// </returns>
        public static bool ProcessIFCSpaceBoundary(ExporterIFC exporterIFC, SpaceBoundary boundary, IFCFile file)
        {
            string spaceBoundaryName = String.Empty;
            if (ExporterCacheManager.ExportOptionsCache.SpaceBoundaryLevel == 1)
                spaceBoundaryName = "1stLevel";
            else if (ExporterCacheManager.ExportOptionsCache.SpaceBoundaryLevel == 2)
                spaceBoundaryName = "2ndLevel";

            IFCAnyHandle spatialElemHnd = ExporterCacheManager.SpatialElementHandleCache.Find(boundary.SpatialElementId);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(spatialElemHnd))
                return false;

            IFCPhysicalOrVirtual boundaryType = boundary.SpaceBoundaryType;
            IFCAnyHandle buildingElemHnd = null;
            if (boundaryType == IFCPhysicalOrVirtual.Physical)
            {
                buildingElemHnd = exporterIFC.FindSpaceBoundingElementHandle(boundary.BuildingElementId, boundary.LevelId);
                if (IFCAnyHandleUtil.IsNullOrHasNoValue(buildingElemHnd))
                    return false;
            }

            IFCInstanceExporter.CreateRelSpaceBoundary(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(), spaceBoundaryName, null,
               spatialElemHnd, buildingElemHnd, boundary.ConnectGeometryHandle, boundaryType, boundary.InternalOrExternal);

            return true;
        }

        /// <summary>
        /// Creates COBIESpaceClassifications.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="file">The file.</param>
        /// <param name="spaceHnd">The space handle.</param>
        /// <param name="projectInfo">The project info.</param>
        /// <param name="spatialElement">The spatial element.</param>
        private static void CreateCOBIESpaceClassifications(ExporterIFC exporterIFC, IFCFile file, IFCAnyHandle spaceHnd, 
            ProjectInfo projectInfo, SpatialElement spatialElement)
        {
            HashSet<IFCAnyHandle> spaceHnds = new HashSet<IFCAnyHandle>();
            spaceHnds.Add(spaceHnd);

            string bimStandardsLocation;
            ParameterUtil.GetStringValueFromElement(projectInfo, "BIM Standards URL", out bimStandardsLocation);

            // OCCS - Space by Function.
            string itemReference = "";
            if (ParameterUtil.GetStringValueFromElement(spatialElement, "OmniClass Number", out itemReference))
            {
                string itemName;
                ParameterUtil.GetStringValueFromElement(spatialElement, "OmniClass Title", out itemName);

                IFCAnyHandle classification;
                if (!ExporterCacheManager.ClassificationCache.TryGetValue("OmniClass", out classification))
                {
                    classification = IFCInstanceExporter.CreateClassification(file, "http://www.omniclass.org", "v 1.0", null, "OmniClass");
                    ExporterCacheManager.ClassificationCache.Add("OmniClass", classification);
                }

                IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                  "http://www.omniclass.org/tables/OmniClass_13_2006-03-28.pdf", itemReference, itemName, classification);
                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                   exporterIFC.GetOwnerHistoryHandle(), "OmniClass", null, spaceHnds, classificationReference);
            }

            // Space Type (Owner)
            itemReference = "";
            if (ParameterUtil.GetStringValueFromElement(spatialElement, "Space Type (Owner) Reference", out itemReference))
            {
                string itemName;
                ParameterUtil.GetStringValueFromElement(spatialElement, "Space Type (Owner) Name", out itemName);

                IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                  bimStandardsLocation, itemReference, itemName, null);
                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                   exporterIFC.GetOwnerHistoryHandle(), "Space Type (Owner)", null, spaceHnds, classificationReference);
            }

            // Space Category (Owner)
            itemReference = "";
            if (ParameterUtil.GetStringValueFromElement(spatialElement, "Space Category (Owner) Reference", out itemReference))
            {
                string itemName;
                ParameterUtil.GetStringValueFromElement(spatialElement, "Space Category (Owner) Name", out itemName);

                IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                  bimStandardsLocation, itemReference, itemName, null);
                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                   exporterIFC.GetOwnerHistoryHandle(), "Space Category (Owner)", null, spaceHnds, classificationReference);
            }

            // Space Category (BOMA)
            itemReference = "";
            if (ParameterUtil.GetStringValueFromElement(spatialElement, "Space Category (BOMA) Reference", out itemReference))
            {
                string itemName;
                ParameterUtil.GetStringValueFromElement(spatialElement, "Space Category (BOMA) Name", out itemName);

                IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                  "http://www.BOMA.org", itemReference, itemName, null);
                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                   exporterIFC.GetOwnerHistoryHandle(), "Space Category (BOMA)", "", spaceHnds, classificationReference);
            }
        }

        /// <summary>
        /// Creates IFC room/space/area item, not include boundaries. 
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="spatialElement">
        /// The spatial element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="setter">
        /// The IFCPlacementSetter.
        /// </param>
        /// <returns>
        /// True if created successfully, false otherwise.
        /// </returns>
        static bool CreateIFCSpace(ExporterIFC exporterIFC, SpatialElement spatialElement, IFCProductWrapper productWrapper, IFCPlacementSetter setter)
        {
            IList<CurveLoop> curveLoops = null;
            try
            {
                SpatialElementBoundaryOptions options = ExporterIFCUtils.GetSpatialElementBoundaryOptions(exporterIFC, spatialElement);
                curveLoops = ExporterIFCUtils.GetRoomBoundaryAsCurveLoopArray(spatialElement, options, true);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                //Some spatial elements are not placed that have no boundary loops. Don't export them.
                return false;
            }

            Autodesk.Revit.DB.Document document = spatialElement.Document;
            ElementId levelId = spatialElement.Level != null ? spatialElement.Level.Id : ElementId.InvalidElementId;
            double scale = exporterIFC.LinearScale;

            ElementId catId = spatialElement.Category != null ? spatialElement.Category.Id : ElementId.InvalidElementId;

            double dArea = 0.0;

            Parameter param = spatialElement.get_Parameter(BuiltInParameter.ROOM_AREA);
            if (param != null)
            {
                dArea = param.AsDouble();
                dArea *= (scale * scale);
            }

            IFCLevelInfo levelInfo = exporterIFC.GetLevelInfo(levelId);

            string strSpaceNumber = null;
            string strSpaceName = null;
            string strSpaceDesc = null;

            bool isArea = spatialElement is Area;
            if (!isArea)
            {
                Parameter paramRoomNum = spatialElement.get_Parameter(BuiltInParameter.ROOM_NUMBER);
                if (paramRoomNum != null)
                {
                    strSpaceNumber = paramRoomNum.AsString();
                }

                Parameter paramRoomName = spatialElement.get_Parameter(BuiltInParameter.ROOM_NAME);
                if (paramRoomName != null)
                {
                    strSpaceName = paramRoomName.AsString();
                }

                Parameter paramRoomComm = spatialElement.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                if (paramRoomComm != null)
                {
                    strSpaceDesc = paramRoomComm.AsString();
                }
            }
            else
            {
                Element level = document.GetElement(levelId);
                if (level != null)
                {
                    strSpaceNumber = level.Name + " GSA Design Gross Area";
                }
            }

            string name = strSpaceNumber;
            string longName = strSpaceName;
            string desc = strSpaceDesc;

            IFCFile file = exporterIFC.GetFile();

            IFCAnyHandle localPlacement = setter.GetPlacement();
            ElementType elemType = document.GetElement(spatialElement.GetTypeId()) as ElementType;
            IFCInternalOrExternal internalOrExternal = CategoryUtil.IsElementExternal(spatialElement) ? IFCInternalOrExternal.External : IFCInternalOrExternal.Internal;

            double roomHeight = 0.0;

            roomHeight = GetHeight(spatialElement, scale, levelId, levelInfo);

            double bottomOffset = 0.0;
            Parameter paramBottomOffset = spatialElement.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET);
            bottomOffset = paramBottomOffset != null ? paramBottomOffset.AsDouble() : 0.0;

            XYZ zDir = new XYZ(0, 0, 1);
            XYZ orig = new XYZ(0, 0, levelInfo.Elevation + bottomOffset);

            Plane plane = new Plane(zDir, orig); // room calculated as level offset.

            GeometryElement geomElem = null;
            if (spatialElement is Autodesk.Revit.DB.Architecture.Room)
            {
                Autodesk.Revit.DB.Architecture.Room room = spatialElement as Autodesk.Revit.DB.Architecture.Room;
                geomElem = room.ClosedShell;
            }
            else if (spatialElement is Autodesk.Revit.DB.Mechanical.Space)
            {
                Autodesk.Revit.DB.Mechanical.Space space = spatialElement as Autodesk.Revit.DB.Mechanical.Space;
                geomElem = space.ClosedShell;
            }

            IFCAnyHandle spaceHnd = null;
            using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
            {
                extraParams.SetLocalPlacement(localPlacement);
                extraParams.PossibleExtrusionAxes = IFCExtrusionAxes.TryZ;

                using (IFCTransaction transaction2 = new IFCTransaction(file))
                {
                    IFCAnyHandle repHnd = null;
                    if (!ExporterCacheManager.ExportOptionsCache.Use2DRoomBoundaryForRoomVolumeCreation && geomElem != null)
                    {
                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                        bodyExporterOptions.TessellationLevel = BodyExporterOptions.BodyTessellationLevel.Coarse;
                        repHnd = RepresentationUtil.CreateBRepProductDefinitionShape(spatialElement.Document.Application, exporterIFC, spatialElement,
                            catId, geomElem, bodyExporterOptions, null, extraParams);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(repHnd))
                            extraParams.ClearOpenings();
                    }
                    else
                    {
                        IFCAnyHandle shapeRep = ExtrusionExporter.CreateExtrudedSolidFromCurveLoop(exporterIFC, null, curveLoops, plane, zDir, roomHeight);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(shapeRep))
                            return false;
                        IFCAnyHandle styledItemHnd = BodyExporter.CreateSurfaceStyleForRepItem(exporterIFC, document,
                            shapeRep, ElementId.InvalidElementId);

                        HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
                        bodyItems.Add(shapeRep);
                        shapeRep = RepresentationUtil.CreateSweptSolidRep(exporterIFC, spatialElement, catId, exporterIFC.Get3DContextHandle("Body"), bodyItems, null);
                        IList<IFCAnyHandle> shapeReps = new List<IFCAnyHandle>();
                        shapeReps.Add(shapeRep);
                        repHnd = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapeReps);
                    }

                    extraParams.ScaledHeight = roomHeight;
                    extraParams.ScaledArea = dArea;

                    spaceHnd = IFCInstanceExporter.CreateSpace(file, ExporterIFCUtils.CreateGUID(spatialElement),
                                                      exporterIFC.GetOwnerHistoryHandle(),
                                                      NamingUtil.GetNameOverride(spatialElement, name),
                                                      NamingUtil.GetDescriptionOverride(spatialElement, desc),
                                                      NamingUtil.GetObjectTypeOverride(spatialElement, null),
                                                      extraParams.GetLocalPlacement(), repHnd, longName, Toolkit.IFCElementComposition.Element
                                                      , internalOrExternal, null);
                    transaction2.Commit();
                }

                productWrapper.AddSpace(spaceHnd, levelInfo, extraParams, true);
            }

            // Save room handle for later use/relationships
            ExporterCacheManager.SpatialElementHandleCache.Register(spatialElement.Id, spaceHnd);
            exporterIFC.RegisterSpatialElementHandle(spatialElement.Id, spaceHnd);

            if (!MathUtil.IsAlmostZero(dArea) && !(ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE))
            {
                ExporterIFCUtils.CreatePreCOBIEGSAQuantities(exporterIFC, spaceHnd, "GSA Space Areas", (isArea ? "GSA Design Gross Area" : "GSA BIM Area"), dArea);
            }

            // Export BaseQuantities for SpatialElement
            if (ExporterCacheManager.ExportOptionsCache.ExportBaseQuantities && !(ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE))
            {
                ExporterIFCUtils.CreateNonCOBIERoomQuantities(exporterIFC, spaceHnd, spatialElement, dArea, roomHeight);
            }

            // Export Classifications for SpatialElement for GSA/COBIE.
            if (ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE)
            {
                CreateCOBIESpaceClassifications(exporterIFC, file, spaceHnd, document.ProjectInformation, spatialElement);
            }

            return true;
        }

        /// <summary>
        /// Creates spatial zone energy analysis property set.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="file">The file.</param>
        /// <param name="element">The element.</param>
        /// <returns>The handle.</returns>
        static private IFCAnyHandle CreateSpatialZoneEnergyAnalysisPSet(ExporterIFC exporterIFC, IFCFile file, Element element)
        {
            // Property Sets.  We don't use the generic Property Set mechanism because Zones aren't "real" elements.
            HashSet<IFCAnyHandle> properties = new HashSet<IFCAnyHandle>();

            string paramValue = "";
            if (ParameterUtil.GetStringValueFromElement(element, "Spatial Zone Conditioning Requirement", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "SpatialZoneConditioningRequirement", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            if (ParameterUtil.GetStringValueFromElement(element, "HVAC System Type", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "HVACSystemType", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            if (ParameterUtil.GetStringValueFromElement(element, "User Defined HVAC System Type", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "UserDefinedHVACSystemType", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            double infiltrationRate = 0.0;
            if (ParameterUtil.GetDoubleValueFromElement(element, "Infiltration Rate", out infiltrationRate))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsReal(infiltrationRate);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "InfiltrationRate", null, paramVal,
                    ExporterCacheManager.UnitsCache["ACH"]);
                properties.Add(propSingleValue);
            }

            int isDaylitZone = 0;
            if (ParameterUtil.GetIntValueFromElement(element, "Is Daylit Zone", out isDaylitZone))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsBoolean(isDaylitZone != 0);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "IsDaylitZone", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            int numberOfDaylightSensors = 0;
            if (ParameterUtil.GetIntValueFromElement(element, "Number of Daylight Sensors", out numberOfDaylightSensors))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsInteger(numberOfDaylightSensors);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "NumberOfDaylightSensors", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            double designIlluminance = 0.0;
            if (ParameterUtil.GetDoubleValueFromElement(element, "Design Illuminance", out designIlluminance))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsReal(designIlluminance);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "DesignIlluminance", null, paramVal, 
                    ExporterCacheManager.UnitsCache["LUX"]);
                properties.Add(propSingleValue);
            }

            if (ParameterUtil.GetStringValueFromElement(element, "Lighting Controls Type", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "LightingControlsType", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            if (properties.Count > 0)
            {
                return IFCInstanceExporter.CreatePropertySet(file,
                    ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(), "ePSet_SpatialZoneEnergyAnalysis",
                    null, properties);
            }

            return null;
        }

        /// <summary>
        /// Creates the ePSet_SpaceOccupant.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static IFCAnyHandle CreatePSetSpaceOccupant(ExporterIFC exporterIFC, IFCFile file, Element element)
        {
            HashSet<IFCAnyHandle> properties = new HashSet<IFCAnyHandle>();

            string paramValue = "";
            if (ParameterUtil.GetStringValueFromElement(element, "Space Occupant Organization Abbreviation", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "SpaceOccupantOrganizationAbbreviation", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            if (ParameterUtil.GetStringValueFromElement(element, "Space Occupant Organization Name", out paramValue))
            {
                IFCData paramVal = BIM.IFC.Toolkit.IFCDataUtil.CreateAsLabel(paramValue);
                IFCAnyHandle propSingleValue = IFCInstanceExporter.CreatePropertySingleValue(file, "SpaceOccupantOrganizationName", null, paramVal, null);
                properties.Add(propSingleValue);
            }

            if (properties.Count > 0)
            {
                return IFCInstanceExporter.CreatePropertySet(file,
                    ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(), "ePset_SpaceOccupant", null, properties);
            }

            return null;
        }

        /// <summary>
        /// Collect information to create space occupants and cache them to create when end export.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporterIFC object.
        /// </param>
        /// <param name="file">
        /// The IFCFile object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        static void CreateSpaceOccupantInfo(ExporterIFC exporterIFC, IFCFile file, Element element, IFCProductWrapper productWrapper)
        {
            IFCAnyHandle roomHandle = null;
            ICollection<IFCAnyHandle> products = productWrapper.GetProducts();

            if (products.Count == 0)
                return;
            else
                roomHandle = products.ElementAt(0);

            bool exportToCOBIE = ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE;

            string name;
            if (ParameterUtil.GetStringValueFromElement(element, "Occupant", out name))
            {
                Dictionary<string, IFCAnyHandle> classificationHandles = new Dictionary<string, IFCAnyHandle>();

                // Classifications.
                if (exportToCOBIE)
                {
                    Document doc = element.Document;
                    ProjectInfo projectInfo = doc.ProjectInformation;

                    string location;
                    ParameterUtil.GetStringValueFromElement(projectInfo, "BIM Standards URL", out location);

                    string itemReference;
                    if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Organization ID Reference", out itemReference))
                    {
                        string itemName;
                        ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Organization ID Name", out itemName);

                        IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                          location, itemReference, itemName, null);
                        classificationHandles["Space Occupant Organization ID"] = classificationReference;
                    }

                    if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Sub-Organization ID Reference", out itemReference))
                    {
                        string itemName;
                        ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Sub-Organization ID Name", out itemName);

                        IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                          location, itemReference, itemName, null);
                        classificationHandles["Space Occupant Sub-Organization ID"] = classificationReference;
                    }

                    if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Sub-Organization ID Reference", out itemReference))
                    {
                        string itemName;
                        ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Sub-Organization ID Name", out itemName);

                        IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                          location, itemReference, itemName, null);
                        classificationHandles["Space Occupant Sub-Organization ID"] = classificationReference;
                    }

                    if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Organization Billing ID Reference", out itemReference))
                    {
                        string itemName;
                        ParameterUtil.GetStringValueFromElementOrSymbol(element, "Space Occupant Organization Billing ID Name", out itemName);

                        IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                          location, itemReference, itemName, null);
                        classificationHandles["Space Occupant Organization Billing ID"] = classificationReference;
                    }
                }

                // Look for Parameter Set definition.  We don't use the general approach as Space Occupants are not "real" elements.
                IFCAnyHandle spaceOccupantPSetHnd = CreatePSetSpaceOccupant(exporterIFC, file, element);

                SpaceOccupantInfo spaceOccupantInfo = ExporterCacheManager.SpaceOccupantInfoCache.Find(name);
                if (spaceOccupantInfo == null)
                {
                    spaceOccupantInfo = new SpaceOccupantInfo(roomHandle, classificationHandles, spaceOccupantPSetHnd);
                    ExporterCacheManager.SpaceOccupantInfoCache.Register(name, spaceOccupantInfo);
                }
                else
                {
                    spaceOccupantInfo.RoomHandles.Add(roomHandle);
                    foreach (KeyValuePair<string, IFCAnyHandle> classificationReference in classificationHandles)
                    {
                        if (!spaceOccupantInfo.ClassificationReferences[classificationReference.Key].HasValue)
                            spaceOccupantInfo.ClassificationReferences[classificationReference.Key] = classificationReference.Value;
                        else
                        {
                            // Delete redundant IfcClassificationReference from file.
                            classificationReference.Value.Delete();
                        }
                    }

                    if (spaceOccupantInfo.SpaceOccupantProperySetHandle == null || !spaceOccupantInfo.SpaceOccupantProperySetHandle.HasValue)
                        spaceOccupantInfo.SpaceOccupantProperySetHandle = spaceOccupantPSetHnd;
                    else if (spaceOccupantPSetHnd.HasValue)
                        spaceOccupantPSetHnd.Delete();
                }
            }
        }
        
        /// <summary>
        /// Collect information to create zones and cache them to create when end export.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporterIFC object.
        /// </param>
        /// <param name="file">
        /// The IFCFile object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        static void CreateZoneInfos(ExporterIFC exporterIFC, IFCFile file, Element element, IFCProductWrapper productWrapper)
        {
            bool exportToCOBIE = ExporterCacheManager.ExportOptionsCache.FileVersion == IFCVersion.IFCCOBIE;

            // Zone Information - For BCA / GSA
            int val = 0;
            string basePropZoneName = "ZoneName";
            string basePropZoneObjectType = "ZoneObjectType";
            string basePropZoneDescription = "ZoneDescription";

            // While a room may contain multiple zones, only one can have the extra GSA parameters.  We will allow the first zone encountered
            // to be defined by them. If we require defining multiple zones in one room, then the code below should be modified to modify the 
            // names of the shared parameters to include the index of the appropriate room.
            bool exportedExtraZoneInformation = false;

            while (++val < 1000)   // prevent infinite loop.
            {
                string propZoneName, propZoneObjectType, propZoneDescription;
                if (val == 1)
                {
                    propZoneName = basePropZoneName;
                    propZoneObjectType = basePropZoneObjectType;
                    propZoneDescription = basePropZoneDescription;
                }
                else
                {
                    propZoneName = basePropZoneName + " " + val;
                    propZoneObjectType = basePropZoneObjectType + " " + val;
                    propZoneDescription = basePropZoneDescription + " " + val;
                }

                {
                    string zoneName;
                    string zoneObjectType;
                    string zoneDescription;

                    if (ParameterUtil.GetStringValueFromElementOrSymbol(element, propZoneName, out zoneName) && !String.IsNullOrEmpty(zoneName))
                    {
                        ParameterUtil.GetStringValueFromElementOrSymbol(element, propZoneObjectType, out zoneObjectType);

                        ParameterUtil.GetStringValueFromElementOrSymbol(element, propZoneDescription, out zoneDescription);

                        IFCAnyHandle roomHandle = null;
                        ICollection<IFCAnyHandle> products = productWrapper.GetProducts();

                        if (products.Count == 0)
                            return;
                        else
                            roomHandle = products.ElementAt(0);

                        Dictionary<string, IFCAnyHandle> classificationHandles = new Dictionary<string, IFCAnyHandle>();
                        IFCAnyHandle energyAnalysisPSetHnd = null;

                        if (exportToCOBIE && !exportedExtraZoneInformation)
                        {
                            if (NamingUtil.IsEqualIgnoringCaseAndSpaces(zoneObjectType, "SpatialZone"))
                            {
                                // Classifications.
                                Document doc = element.Document;
                                ProjectInfo projectInfo = doc.ProjectInformation;

                                string location;
                                ParameterUtil.GetStringValueFromElement(projectInfo, "BIM Standards URL", out location);

                                string itemReference;
                                string itemName;
                            
                                // Spatial Zone Type (Owner)
                                if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Spatial Zone Type (Owner) Reference", out itemReference))
                                {
                                    ParameterUtil.GetStringValueFromElementOrSymbol(element, "Spatial Zone Type (Owner) Name", out itemName);
                                    
                                    IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                                      location, itemReference, itemName, null);
                                    classificationHandles["Spatial Zone Type (Owner)"] = classificationReference;
                                }

                                // Spatial Zone Security Level (Owner)
                                if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "Spatial Zone Security Level (Owner) Reference", out itemReference))
                                {
                                    itemName = "";
                                    ParameterUtil.GetStringValueFromElementOrSymbol(element, "Spatial Zone Security Level (Owner) Name", out itemName);

                                    IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                                      location, itemReference, itemName, null);
                                    classificationHandles["Spatial Zone Security Level (Owner)"] = classificationReference;
                                }

                                // Spatial Zone Type (Energy Analysis)
                                if (ParameterUtil.GetStringValueFromElementOrSymbol(element, "ASHRAE Zone Type", out itemName))
                                {
                                    IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                                      "ASHRAE 90.1", "Common Space Type", itemName, null);
                                    classificationHandles["ASHRAE Zone Type"] = classificationReference;
                                }

                                // Property Sets.  We don't use the generic Property Set mechanism because Zones aren't "real" elements.
                                energyAnalysisPSetHnd = CreateSpatialZoneEnergyAnalysisPSet(exporterIFC, file, element);

                                if (classificationHandles.Count > 0 || energyAnalysisPSetHnd != null)
                                    exportedExtraZoneInformation = true;
                            }
                        }

                        ZoneInfo zoneInfo = ExporterCacheManager.ZoneInfoCache.Find(zoneName);
                        if (zoneInfo == null)
                        {
                            zoneInfo = new ZoneInfo(zoneObjectType, zoneDescription, roomHandle, classificationHandles, energyAnalysisPSetHnd);
                            ExporterCacheManager.ZoneInfoCache.Register(zoneName, zoneInfo);
                        }
                        else
                        {
                            // if description and object type were empty, overwrite.
                            if (!String.IsNullOrEmpty(zoneObjectType) && String.IsNullOrEmpty(zoneInfo.ObjectType))
                                zoneInfo.ObjectType = zoneObjectType;
                            if (!String.IsNullOrEmpty(zoneDescription) && String.IsNullOrEmpty(zoneInfo.Description))
                                zoneInfo.Description = zoneDescription;

                            zoneInfo.RoomHandles.Add(roomHandle);
                            foreach (KeyValuePair<string, IFCAnyHandle> classificationReference in classificationHandles)
                            {
                                if (!zoneInfo.ClassificationReferences[classificationReference.Key].HasValue)
                                    zoneInfo.ClassificationReferences[classificationReference.Key] = classificationReference.Value;
                                else
                                {
                                    // Delete redundant IfcClassificationReference from file.
                                    classificationReference.Value.Delete();
                                }
                            }

                            if (zoneInfo.EnergyAnalysisProperySetHandle == null || !zoneInfo.EnergyAnalysisProperySetHandle.HasValue)
                                zoneInfo.EnergyAnalysisProperySetHandle = energyAnalysisPSetHnd;
                            else if (energyAnalysisPSetHnd.HasValue)
                                energyAnalysisPSetHnd.Delete();
                        }
                    }
                    else
                        break;
                }
            }
        }
    }
}
