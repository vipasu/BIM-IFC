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
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export family instances.
    /// </summary>
    class FamilyInstanceExporter
    {
        /// <summary>
        /// Exports a family instance to corresponding IFC object.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance to be exported.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportFamilyInstanceElement(ExporterIFC exporterIFC,
           FamilyInstance familyInstance, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            // Don't export family if it is invisible, or has a null geometry.
            if (familyInstance.Invisible || geometryElement == null)
                return;

            // Don't export mullions and panels if they have a host and their host is not a mass
            // as they will be exported with the host (curtain wall/roof).
            if (familyInstance.Category.Id == new ElementId(BuiltInCategory.OST_CurtainWallMullions) ||
                familyInstance.Category.Id == new ElementId(BuiltInCategory.OST_CurtainWallPanels))
            {
                Element host = familyInstance.Host;
                if (host != null && host.Category.Id != new ElementId(BuiltInCategory.OST_Mass))
                    return;
            }

            FamilySymbol familySymbol = familyInstance.Symbol;
            Family family = familySymbol.Family;
            if (family == null)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                string ifcEnumType;
                IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, familyInstance, out ifcEnumType);

                if (exportType == IFCExportType.DontExport)
                    return;

                if (ExportFamilyInstanceAsStandardElement(exporterIFC, familyInstance, geometryElement, exportType, ifcEnumType, productWrapper))
                {
                    tr.Commit();
                    return;
                }

                // If we are exporting a column, we may need to split it into parts by level.  Create a list of ranges.
                IList<ElementId> levels = new List<ElementId>();
                IList<IFCRange> ranges = new List<IFCRange>();

                // We will not split walls and columns if the assemblyId is set, as we would like to keep the original wall
                // associated with the assembly, on the level of the assembly.
                bool splitColumn = (exportType == IFCExportType.ExportColumnType) && (ExporterCacheManager.ExportOptionsCache.WallAndColumnSplitting) &&
                    (familyInstance.AssemblyInstanceId == ElementId.InvalidElementId);
                if (splitColumn)
                {
                    LevelUtil.CreateSplitLevelRangesForElement(exporterIFC, exportType, familyInstance, out levels, out ranges);
                }

                int numPartsToExport = ranges.Count;
                if (numPartsToExport == 0)
                {
                    ExportFamilyInstanceAsMappedItem(exporterIFC, familyInstance, exportType, ifcEnumType, productWrapper,
                       ElementId.InvalidElementId, null);
                }
                else
                {
                    for (int ii = 0; ii < numPartsToExport; ii++)
                    {
                        ExportFamilyInstanceAsMappedItem(exporterIFC, familyInstance, exportType, ifcEnumType, productWrapper,
                          levels[ii], ranges[ii]);
                    }

                    if (ExporterCacheManager.DummyHostCache.HasRegistered(familyInstance.Id))
                    {
                        List<KeyValuePair<ElementId, IFCRange>> levelRangeList = ExporterCacheManager.DummyHostCache.Find(familyInstance.Id);
                        foreach (KeyValuePair<ElementId, IFCRange> levelRange in levelRangeList)
                        {
                            ExportFamilyInstanceAsMappedItem(exporterIFC, familyInstance, exportType, ifcEnumType, productWrapper, levelRange.Key, levelRange.Value);
                        }
                    }
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Exports a family instance as a mapped item.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance to be exported.
        /// </param>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <param name="ifcEnumType">
        /// The string value represents the IFC type.
        /// </param>
        /// <param name="wrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="overrideLevelId">
        /// The level id.
        /// </param>
        /// <param name="range">
        /// The range of this family instance to be exported.
        /// </param>
        public static void ExportFamilyInstanceAsMappedItem(ExporterIFC exporterIFC,
           FamilyInstance familyInstance, IFCExportType exportType, string ifcEnumType,
           IFCProductWrapper wrapper, ElementId overrideLevelId, IFCRange range)
        {
            bool exportParts = PartExporter.CanExportParts(familyInstance);
            bool isSplit = range != null;
            if (exportParts && !PartExporter.CanExportElementInPartExport(familyInstance, isSplit ? overrideLevelId : familyInstance.Level.Id, isSplit))
                return;

            Document doc = familyInstance.Document;
            IFCFile file = exporterIFC.GetFile();

            FamilySymbol familySymbol = ExporterIFCUtils.GetOriginalSymbol(familyInstance);
            if (familySymbol == null)
                return;

            IFCProductWrapper familyProductWrapper = IFCProductWrapper.Create(wrapper);
            double scale = exporterIFC.LinearScale;

            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            HostObject hostElement = familyInstance.Host as HostObject; //hostElement could be null
            ElementId categoryId = CategoryUtil.GetSafeCategoryId(familySymbol);

            //string emptyString = "";
            string familyName = familySymbol.Name;
            string objectType = familyName;

            // A Family Instance can have its own copy of geometry, or use the symbol's copy with a transform.
            // The routine below tells us whether to use the Instance's copy or the Symbol's copy.
            bool useInstanceGeometry = ExporterIFCUtils.UsesInstanceGeometry(familyInstance);

            IList<IFCExtrusionData> cutPairOpeningsForColumns = new List<IFCExtrusionData>();
            using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
            {
                Transform trf = familyInstance.GetTransform();

                // Extra information if we are exporting a door or a window.
                IFCDoorWindowInfo doorWindowInfo = null;
                if (exportType == IFCExportType.ExportDoorType)
                    doorWindowInfo = IFCDoorWindowInfo.CreateDoorInfo(exporterIFC, familyInstance, familySymbol, hostElement, overrideLevelId, trf);
                else if (exportType == IFCExportType.ExportWindowType)
                    doorWindowInfo = IFCDoorWindowInfo.CreateWindowInfo(exporterIFC, familyInstance, familySymbol, hostElement, overrideLevelId, trf);

                FamilyTypeInfo typeInfo = new FamilyTypeInfo();
                XYZ extraOffset = XYZ.Zero;

                bool flipped = doorWindowInfo != null ? doorWindowInfo.IsSymbolFlipped : false;
                FamilyTypeInfo currentTypeInfo = ExporterCacheManager.TypeObjectsCache.Find(familySymbol.Id, flipped);
                bool found = currentTypeInfo.IsValid();

                Family family = familySymbol.Family;

                // TODO: this code to be removed by ExtrusionAnalyzer code.
                bool trySpecialColumnCreation = ((exportType == IFCExportType.ExportColumnType) && (!family.IsInPlace));

                // We will create a new mapped type if:
                // 1.  We are exporting part of a column or in-place wall (range != null), OR
                // 2.  We are using the instance's copy of the geometry (that it, it has unique geometry), OR
                // 3.  We haven't already created the type.
                bool creatingType = ((range != null) || useInstanceGeometry || !found);
                if (creatingType)
                {
                    IFCAnyHandle bodyRepresentation = null;
                    IFCAnyHandle planRepresentation = null;

                    // If we are using the instance geometry, ignore the transformation.
                    if (useInstanceGeometry)
                        trf = Transform.Identity;

                    // TODO: this code to be removed by ExtrusionAnalyzer code.
                    if (trySpecialColumnCreation)
                    {
                        XYZ rangeOffset = trf.Origin;
                        IFCFamilyInstanceExtrusionExportResults results;

                        results = ExporterIFCUtils.ExportFamilyInstanceAsExtrusion(exporterIFC, familyInstance, useInstanceGeometry, range, overrideLevelId, extraParams);

                        bodyRepresentation = results.GetExtrusionHandle();
                        extraOffset = results.ExtraOffset;
                        cutPairOpeningsForColumns = results.GetCutPairOpenings();

                        if (!IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRepresentation))
                        {
                            typeInfo.MaterialIds.Add(results.MaterialId);
                            // add in level for real columns, not in-place ones.
                            Element actualLevel =
                               (overrideLevelId == ElementId.InvalidElementId) ? familyInstance.Level : doc.GetElement(overrideLevelId);
                            if (actualLevel != null)
                            {
                                IFCLevelInfo levelInfo = exporterIFC.GetLevelInfo(actualLevel.Id);
                                double nonStoryLevelOffset = LevelUtil.GetNonStoryLevelOffsetIfAny(exporterIFC, actualLevel as Level);
                                if (range != null)
                                {
                                    rangeOffset = new XYZ(rangeOffset.X, rangeOffset.Y, levelInfo.Elevation + nonStoryLevelOffset);
                                }
                            }
                        }

                        rangeOffset += extraOffset;
                        trf.Origin = rangeOffset;
                    }

                    Transform doorWindowTrf = Transform.Identity;
                    IFCAnyHandle dummyPlacement = null;
                    if (doorWindowInfo != null)
                    {
                        doorWindowTrf = ExporterIFCUtils.GetTransformForDoorOrWindow(familyInstance, familySymbol, doorWindowInfo);
                    }
                    else
                    {
                        dummyPlacement = IFCInstanceExporter.CreateLocalPlacement(file, null, ExporterUtil.CreateAxis2Placement3D(file));
                        extraParams.SetLocalPlacement(dummyPlacement);
                    }

                    bool needToCreate2d = ExporterCacheManager.ExportOptionsCache.ExportAnnotations;
                    bool needToCreate3d = IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRepresentation);

                    if (needToCreate2d || needToCreate3d)
                    {
                        using (IFCTransformSetter trfSetter = IFCTransformSetter.Create())
                        {
                            if (doorWindowInfo != null)
                            {
                                trfSetter.Initialize(exporterIFC, doorWindowTrf);
                            }

                            Options options = GeometryUtil.GetIFCExportGeometryOptions();

                            GeometryElement exportGeometry =
                               useInstanceGeometry ? familyInstance.get_Geometry(options) : familySymbol.get_Geometry(options);
                            if (exportGeometry == null)
                                return;

                            Transform brepOffsetTransform = null;
                            if (needToCreate3d)
                            {
                                SolidMeshGeometryInfo solidMeshCapsule = null;

                                if (range == null)
                                {
                                    solidMeshCapsule = GeometryUtil.GetSolidMeshGeometry(exportGeometry, Transform.Identity);
                                }
                                else
                                {
                                    solidMeshCapsule = GeometryUtil.GetClippedSolidMeshGeometry(exportGeometry, range);
                                }

                                IList<Solid> solids = solidMeshCapsule.GetSolids();
                                IList<Mesh> polyMeshes = solidMeshCapsule.GetMeshes();

                                IList<GeometryObject> geomObjects = FamilyExporterUtil.RemoveSolidsAndMeshesSetToDontExport(doc, exporterIFC, solids, polyMeshes);
                                if (geomObjects.Count == 0 && (solids.Count > 0 || polyMeshes.Count > 0))
                                    return;

                                bool tryToExportAsExtrusion = (!exporterIFC.ExportAs2x2 || (exportType == IFCExportType.ExportColumnType));

                                if (exportType == IFCExportType.ExportColumnType)
                                {
                                    extraParams.PossibleExtrusionAxes = IFCExtrusionAxes.TryZ;
                                }
                                else
                                {
                                    extraParams.PossibleExtrusionAxes = IFCExtrusionAxes.TryXYZ;
                                }

                                BodyData bodyData = null;
                                BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(tryToExportAsExtrusion);
                                if (geomObjects.Count > 0)
                                {
                                    bodyData = BodyExporter.ExportBody(familyInstance.Document.Application, exporterIFC, familyInstance, categoryId,
                                        geomObjects, bodyExporterOptions, extraParams);
                                    typeInfo.MaterialIds = bodyData.MaterialIds;
                                }
                                else
                                {
                                    IList<GeometryObject> exportedGeometries = new List<GeometryObject>();
                                    exportedGeometries.Add(exportGeometry);
                                    bodyData = BodyExporter.ExportBody(familyInstance.Document.Application, exporterIFC, familyInstance, categoryId,
                                        exportedGeometries, bodyExporterOptions, extraParams);
                                }

                                bodyRepresentation = bodyData.RepresentationHnd;
                                if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRepresentation))
                                {
                                    extraParams.ClearOpenings();
                                    return;
                                }

                                brepOffsetTransform = bodyData.BrepOffsetTransform;
                            }

                            // By default: if exporting IFC2x3 or later, export 2D plan rep of family, if it exists, unless we are exporting Coordination View V2.
                            // This default can be overridden in the export options.
                            if (needToCreate2d)
                            {
                                XYZ curveOffset = new XYZ(0, 0, 0);
                                if (brepOffsetTransform != null)
                                    curveOffset = -brepOffsetTransform.Origin / scale;

                                HashSet<IFCAnyHandle> curveSet = new HashSet<IFCAnyHandle>();
                                {
                                    Transform planeTrf = doorWindowTrf.Inverse;
                                    Plane plane = new Plane(planeTrf.get_Basis(0), planeTrf.get_Basis(1), planeTrf.Origin);
                                    XYZ projDir = new XYZ(0, 0, 1);

                                    IFCGeometryInfo IFCGeometryInfo = IFCGeometryInfo.CreateCurveGeometryInfo(exporterIFC, plane, projDir, true);
                                    ExporterIFCUtils.CollectGeometryInfo(exporterIFC, IFCGeometryInfo, exportGeometry, curveOffset, false);

                                    IList<IFCAnyHandle> curves = IFCGeometryInfo.GetCurves();
                                    foreach (IFCAnyHandle curve in curves)
                                        curveSet.Add(curve);

                                    if (curveSet.Count > 0)
                                    {
                                        IFCAnyHandle contextOfItems2d = exporterIFC.Get2DContextHandle();
                                        IFCAnyHandle curveRepresentationItem = IFCInstanceExporter.CreateGeometricSet(file, curveSet);
                                        HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
                                        bodyItems.Add(curveRepresentationItem);
                                        planRepresentation = RepresentationUtil.CreateGeometricSetRep(exporterIFC, familyInstance, categoryId, "Annotation",
                                           contextOfItems2d, bodyItems);
                                    }
                                }
                            }
                        }
                    }

                    if (doorWindowInfo != null)
                    {
                        typeInfo.StyleTransform = doorWindowTrf.Inverse;
                    }
                    else
                    {
                        if (!MathUtil.IsAlmostZero(extraOffset.DotProduct(extraOffset)))
                        {
                            Transform newTransform = typeInfo.StyleTransform;
                            XYZ newOrig = newTransform.Origin + extraOffset;
                            newTransform.Origin = newOrig;
                            typeInfo.StyleTransform = newTransform;
                        }
                        typeInfo.StyleTransform = ExporterIFCUtils.GetUnscaledTransform(exporterIFC, extraParams.GetLocalPlacement());
                    }

                    IFCAnyHandle origin = ExporterUtil.CreateAxis2Placement3D(file);
                    IFCAnyHandle repMap2dHnd = null;
                    IFCAnyHandle repMap3dHnd = IFCInstanceExporter.CreateRepresentationMap(file, origin, bodyRepresentation);

                    IList<IFCAnyHandle> repMapList = new List<IFCAnyHandle>();
                    repMapList.Add(repMap3dHnd);
                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(planRepresentation))
                    {
                        repMap2dHnd = IFCInstanceExporter.CreateRepresentationMap(file, origin, planRepresentation);
                        repMapList.Add(repMap2dHnd);
                    }

                    // for Door, Window
                    bool paramTakesPrecedence = false; // For Revit, this is currently always false.
                    bool sizeable = false;

                    // for many
                    HashSet<IFCAnyHandle> propertySets = new HashSet<IFCAnyHandle>();

                    string guid = ExporterIFCUtils.CreateGUID(familySymbol);
                    string symId = NamingUtil.CreateIFCElementId(familySymbol);

                    // This covers many generic types.  If we can't find it in the list here, do custom exports.
                    IFCAnyHandle typeStyle = FamilyExporterUtil.ExportGenericType(file, exportType, ifcEnumType, guid,
                       ownerHistory, objectType, null, null, propertySets, repMapList, symId, objectType,
                       familyInstance, familySymbol);

                    // Cover special cases not covered above.
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(typeStyle))
                    {
                        switch (exportType)
                        {
                            case IFCExportType.ExportColumnType:
                                {
                                    // If we are using the instance GRep, then we have to create a generic GUID for the
                                    // column type, as they share the same ElementId.
                                    string colGUID = null;
                                    string colElemId = null;

                                    if (useInstanceGeometry)
                                    {
                                        colGUID = ExporterIFCUtils.CreateGUID();
                                        colElemId = NamingUtil.CreateIFCElementId(familyInstance);
                                    }
                                    else
                                    {
                                        colGUID = guid;
                                        colElemId = NamingUtil.CreateIFCElementId(familySymbol);
                                    }

                                    string columnType = "Column";
                                    typeStyle = IFCInstanceExporter.CreateColumnType(file, colGUID, ownerHistory, objectType,
                                       null, null, propertySets, repMapList, colElemId,
                                       objectType, GetColumnType(familyInstance, columnType));

                                    break;
                                }
                            case IFCExportType.ExportDoorType:
                                {
                                    string constructionType = string.Empty;
                                    ParameterUtil.GetStringValueFromElementOrSymbol(familyInstance, "Construction", out constructionType);

                                    IFCAnyHandle doorLining = DoorWindowUtil.CreateDoorLiningProperties(exporterIFC, familyInstance);
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(doorLining))
                                        propertySets.Add(doorLining);

                                    IList<IFCAnyHandle> doorPanels = DoorWindowUtil.CreateDoorPanelProperties(exporterIFC, doorWindowInfo,
                                       familyInstance);
                                    propertySets.UnionWith(doorPanels);

                                    string doorStyleGUID = ExporterIFCUtils.CreateGUID();
                                    string doorStyleElemId = NamingUtil.CreateIFCElementId(familyInstance);
                                    typeStyle = IFCInstanceExporter.CreateDoorStyle(file, doorStyleGUID, ownerHistory, objectType,
                                       null, null, propertySets, repMapList, doorStyleElemId,
                                       GetDoorStyleOperation(doorWindowInfo.DoorOperationType), GetDoorStyleConstruction(familyInstance, constructionType),
                                       paramTakesPrecedence, sizeable);
                                    break;
                                }
                            case IFCExportType.ExportSystemFurnitureElementType:
                                {
                                    string furnitureId = NamingUtil.CreateIFCElementId(familyInstance);
                                    typeStyle = IFCInstanceExporter.CreateSystemFurnitureElementType(file, guid, ownerHistory, objectType,
                                       null, null, propertySets, repMapList, furnitureId,
                                       objectType);
                                    break;
                                }
                            case IFCExportType.ExportWindowType:
                                {
                                    Toolkit.IFCWindowStyleOperation operationType = DoorWindowUtil.GetIFCWindowStyleOperation(familySymbol);
                                    IFCWindowStyleConstruction constructionType = DoorWindowUtil.GetIFCWindowStyleConstruction(familyInstance, "");

                                    IFCAnyHandle windowLining = DoorWindowUtil.CreateWindowLiningProperties(exporterIFC, familyInstance, null);
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(windowLining))
                                        propertySets.Add(windowLining);

                                    IList<IFCAnyHandle> windowPanels =
                                       DoorWindowUtil.CreateWindowPanelProperties(exporterIFC, familyInstance, null);
                                    propertySets.UnionWith(windowPanels);

                                    string windowStyleGUID = ExporterIFCUtils.CreateGUID();
                                    string windowStyleElemId = NamingUtil.CreateIFCElementId(familyInstance);
                                    typeStyle = IFCInstanceExporter.CreateWindowStyle(file, windowStyleGUID, ownerHistory, objectType,
                                       null, null, propertySets, repMapList, windowStyleElemId,
                                       constructionType, operationType, paramTakesPrecedence, sizeable);
                                    break;
                                }
                            case IFCExportType.ExportBuildingElementProxy:
                            default:
                                {
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(repMap2dHnd))
                                        typeInfo.Map2DHandle = repMap2dHnd;
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(repMap3dHnd))
                                        typeInfo.Map3DHandle = repMap3dHnd;
                                    break;
                                }
                        }
                    }

                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(typeStyle))
                    {
                        typeInfo.Style = typeStyle;

                        if (((exportType == IFCExportType.ExportColumnType) && trySpecialColumnCreation) ||
                           (exportType == IFCExportType.ExportMemberType))
                        {
                            typeInfo.ScaledArea = extraParams.ScaledArea;
                            typeInfo.ScaledDepth = extraParams.ScaledLength;
                            typeInfo.ScaledInnerPerimeter = extraParams.ScaledInnerPerimeter;
                            typeInfo.ScaledOuterPerimeter = extraParams.ScaledOuterPerimeter;
                        }

                        ClassificationUtil.CreateUniformatClassification(exporterIFC, file, familySymbol, typeStyle);
                    }
                }
                else if (!creatingType && (trySpecialColumnCreation))
                {
                    // still need to modify instance trf for columns.
                    trf.Origin += GetLevelOffsetForExtrudedColumns(exporterIFC, familyInstance, overrideLevelId, extraParams);
                }

                if (found && !typeInfo.IsValid())
                {
                    typeInfo = currentTypeInfo;
                }

                // we'll pretend we succeeded, but we'll do nothing.
                if (!typeInfo.IsValid())
                    return;

                // add to the map, as long as we are not using range, not using instance geometry, and don't have extra openings.
                if ((range == null) && !useInstanceGeometry && (extraParams.GetOpenings().Count == 0))
                    ExporterCacheManager.TypeObjectsCache.Register(familySymbol.Id, flipped, typeInfo);

                Transform oldTrf = new Transform(trf);
                XYZ scaledMapOrigin = XYZ.Zero;

                trf = trf.Multiply(typeInfo.StyleTransform);

                // create instance.  
                IList<IFCAnyHandle> shapeReps = new List<IFCAnyHandle>();
                {
                    IFCAnyHandle contextOfItems2d = exporterIFC.Get2DContextHandle();
                    IFCAnyHandle contextOfItems3d = exporterIFC.Get3DContextHandle("Body");

                    // for proxies, we store the IfcRepresentationMap directly since there is no style.
                    IFCAnyHandle style = typeInfo.Style;
                    IList<IFCAnyHandle> repMapList = !IFCAnyHandleUtil.IsNullOrHasNoValue(style) ?
                        GeometryUtil.GetRepresentationMaps(style) : null;
                    int numReps = repMapList != null ? repMapList.Count : 0;

                    IFCAnyHandle repMap2dHnd = typeInfo.Map2DHandle;
                    IFCAnyHandle repMap3dHnd = typeInfo.Map3DHandle;
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(repMap3dHnd) && (numReps > 0))
                        repMap3dHnd = repMapList[0];
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(repMap2dHnd) && (numReps > 1))
                        repMap2dHnd = repMapList[1];

                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(repMap3dHnd))
                    {
                        IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                        representations.Add(ExporterUtil.CreateDefaultMappedItem(file, repMap3dHnd, scaledMapOrigin));
                        IFCAnyHandle shapeRep = RepresentationUtil.CreateBodyMappedItemRep(exporterIFC, familyInstance, categoryId, contextOfItems3d, 
                            representations);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(shapeRep))
                            return;
                        shapeReps.Add(shapeRep);
                    }

                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(repMap2dHnd))
                    {
                        HashSet<IFCAnyHandle> representations = new HashSet<IFCAnyHandle>();
                        representations.Add(ExporterUtil.CreateDefaultMappedItem(file, repMap2dHnd, scaledMapOrigin));
                        IFCAnyHandle shapeRep = RepresentationUtil.CreatePlanMappedItemRep(exporterIFC, familyInstance, categoryId, contextOfItems2d, 
                            representations);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(shapeRep))
                            return;
                        shapeReps.Add(shapeRep);
                    }
                }

                IFCAnyHandle rep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapeReps);

                IFCAnyHandle instanceHandle = null;
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, familyInstance, trf, null, overrideLevelId))
                {
                    string instanceGUID = ExporterIFCUtils.CreateGUID(familyInstance);
                    string origInstanceName = exporterIFC.GetName();
                    string instanceName = NamingUtil.GetNameOverride(familyInstance, origInstanceName);
                    string instanceDescription = NamingUtil.GetDescriptionOverride(familyInstance, null);
                    string instanceObjectType = NamingUtil.GetObjectTypeOverride(familyInstance, objectType);
                    string instanceElemId = NamingUtil.CreateIFCElementId(familyInstance);

                    IFCAnyHandle localPlacement = setter.GetPlacement();
                    instanceHandle = FamilyExporterUtil.ExportGenericInstance(exportType, exporterIFC, familyInstance,
                       wrapper, setter, extraParams, instanceGUID, ownerHistory, instanceName, instanceDescription, instanceObjectType,
                       exportParts? null : rep, instanceElemId);

                    if (exportParts)
                    {
                        PartExporter.ExportHostPart(exporterIFC, familyInstance, instanceHandle, familyProductWrapper, setter, setter.GetPlacement(), overrideLevelId);
                    }

                    if (ElementFilteringUtil.IsMEPType(exportType))
                        ExporterCacheManager.MEPCache.Register(familyInstance, instanceHandle);

                    switch (exportType)
                    {
                        case IFCExportType.ExportColumnType:
                            {
                                IFCAnyHandle placementToUse = localPlacement;
                                if (!useInstanceGeometry)
                                {
                                    bool needToCreateOpenings =
                                        (cutPairOpeningsForColumns.Count == 0) || OpeningUtil.NeedToCreateOpenings(instanceHandle, extraParams);
                                    if (needToCreateOpenings)
                                    {
                                        Transform openingTrf = new Transform(oldTrf);
                                        Transform extraRot = new Transform(oldTrf);
                                        extraRot.Origin = XYZ.Zero;
                                        openingTrf = openingTrf.Multiply(extraRot);
                                        openingTrf = openingTrf.Multiply(typeInfo.StyleTransform);

                                        IFCAnyHandle openingRelativePlacement = ExporterUtil.CreateAxis2Placement3D(file, openingTrf.Origin * scale,
                                           openingTrf.get_Basis(2), openingTrf.get_Basis(0));
                                        IFCAnyHandle openingPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacement);
                                        GeometryUtil.SetRelativePlacement(openingPlacement, openingRelativePlacement);
                                        placementToUse = openingPlacement;
                                    }
                                }

                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, cutPairOpeningsForColumns,
                                   exporterIFC, placementToUse, setter, wrapper);
                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, extraParams, exporterIFC,
                                   placementToUse, setter, wrapper);

                                //export Base Quantities.
                                PropertyUtil.CreateBeamColumnBaseQuantities(exporterIFC, instanceHandle, familyInstance, typeInfo);

                                ExporterIFCUtils.CreateColumnPropertySet(exporterIFC, familyInstance, extraParams, wrapper);
                                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                break;
                            }
                        case IFCExportType.ExportDoorType:
                        case IFCExportType.ExportWindowType:
                            {
                                double doorHeight = doorWindowInfo.OpeningHeight;
                                if (doorHeight < MathUtil.Eps())
                                    doorHeight = GetMinSymbolHeight(familySymbol);
                                double doorWidth = doorWindowInfo.OpeningWidth;
                                if (doorWidth < MathUtil.Eps())
                                    doorWidth = GetMinSymbolWidth(familySymbol);

                                double height = doorHeight * scale;
                                double width = doorWidth * scale;

                                if (IFCAnyHandleUtil.IsNullOrHasNoValue(doorWindowInfo.GetLocalPlacement()))
                                    doorWindowInfo.SetLocalPlacement(localPlacement);

                                IFCAnyHandle doorWindowOrigLocalPlacement = doorWindowInfo.GetLocalPlacement();
                                Transform relTrf = ExporterIFCUtils.GetRelativeLocalPlacementOffsetTransform(localPlacement, doorWindowOrigLocalPlacement);

                                IFCAnyHandle doorWindowRelativePlacement = ExporterUtil.CreateAxis2Placement3D(file, relTrf.Origin, relTrf.BasisZ, relTrf.BasisX);
                                IFCAnyHandle doorWindowLocalPlacement =
                                   IFCInstanceExporter.CreateLocalPlacement(file, doorWindowOrigLocalPlacement, doorWindowRelativePlacement);
                                if (exportType == IFCExportType.ExportDoorType)
                                    instanceHandle = IFCInstanceExporter.CreateDoor(file, instanceGUID, ownerHistory,
                                       instanceName, instanceDescription, instanceObjectType, doorWindowLocalPlacement,
                                       rep, instanceElemId, height, width);
                                else if (exportType == IFCExportType.ExportWindowType)
                                    instanceHandle = IFCInstanceExporter.CreateWindow(file, instanceGUID, ownerHistory,
                                       instanceName, instanceDescription, instanceObjectType, doorWindowLocalPlacement,
                                       rep, instanceElemId, height, width);
                                wrapper.AddElement(instanceHandle, setter, extraParams, true);

                                exporterIFC.RegisterSpaceBoundingElementHandle(instanceHandle, familyInstance.Id, setter.LevelId);

                                // only necessary when exporting as possible breps.
                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, extraParams, exporterIFC,
                                   doorWindowLocalPlacement, setter, wrapper);
                                if (ExporterCacheManager.ExportOptionsCache.ExportBaseQuantities)
                                    ExporterIFCUtils.CreateDoorWindowBaseQuantities(exporterIFC, instanceHandle, (doorHeight * scale), (doorWidth * scale));

                                if (exportType == IFCExportType.ExportDoorType)
                                {
                                    ExporterIFCUtils.CreateDoorPropertySet(exporterIFC, familyInstance, wrapper);
                                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                }
                                else
                                {
                                    ExporterIFCUtils.CreateWindowPropertySet(exporterIFC, familyInstance, wrapper);
                                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                }
                                break;
                            }
                        case IFCExportType.ExportMemberType:
                            {
                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, extraParams, exporterIFC,
                                   localPlacement, setter, wrapper);

                                //export Base Quantities.
                                PropertyUtil.CreateBeamColumnBaseQuantities(exporterIFC, instanceHandle, familyInstance, typeInfo);
                                // TODO: create PropertySet!
                                //createMemberPropertySet(exporter, pFamInst, pWrapper, extraParams);
                                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                break;
                            }
                        case IFCExportType.ExportPlateType:
                            {
                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, extraParams, exporterIFC,
                                   localPlacement, setter, wrapper);

                                // TODO: create PropertySet!
                                //createPlatePropertySet(exporter, pFamInst, pWrapper, extraParams);
                                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                break;
                            }
                        case IFCExportType.ExportTransportElementType:
                            {
                                IFCAnyHandle localPlacementToUse;
                                ElementId roomId = setter.UpdateRoomRelativeCoordinates(familyInstance, out localPlacementToUse);

                                instanceHandle = IFCInstanceExporter.CreateTransportElement(file, instanceGUID, ownerHistory,
                                   instanceName, instanceDescription, instanceObjectType,
                                   localPlacementToUse, rep, instanceElemId, null, null, null);

                                if (roomId == ElementId.InvalidElementId)
                                {
                                    wrapper.AddElement(instanceHandle, setter, extraParams, true);
                                }
                                else
                                {
                                    exporterIFC.RelateSpatialElement(roomId, instanceHandle);
                                    wrapper.AddElement(instanceHandle, setter, extraParams, false);
                                }

                                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                break;
                            }
                        case IFCExportType.ExportBuildingElementProxy:
                        default:
                            {
                                bool isBuildingElementProxy = (exportType == IFCExportType.ExportBuildingElementProxy);

                                IFCAnyHandle localPlacementToUse;
                                ElementId roomId = setter.UpdateRoomRelativeCoordinates(familyInstance, out localPlacementToUse);

                                if (!isBuildingElementProxy)
                                {
                                    if (FamilyExporterUtil.IsDistributionControlElementSubType(exportType))
                                    {
                                        instanceHandle = IFCInstanceExporter.CreateDistributionControlElement(file, instanceGUID,
                                           ownerHistory, instanceName, instanceDescription, instanceObjectType,
                                           localPlacementToUse, rep, instanceElemId, null);

                                        if (roomId == ElementId.InvalidElementId)
                                        {
                                            wrapper.AddElement(instanceHandle, setter, extraParams, true);
                                        }
                                        else
                                        {
                                            exporterIFC.RelateSpatialElement(roomId, instanceHandle);
                                            wrapper.AddElement(instanceHandle, setter, extraParams, false);
                                        }
                                    }
                                    else if (IFCAnyHandleUtil.IsNullOrHasNoValue(instanceHandle))
                                    {
                                        isBuildingElementProxy = true;
                                    }
                                }

                                if (isBuildingElementProxy)
                                {
                                    Toolkit.IFCElementComposition proxyType = Toolkit.IFCElementComposition.Element;

                                    instanceHandle = IFCInstanceExporter.CreateBuildingElementProxy(file, instanceGUID,
                                       ownerHistory, instanceName, instanceDescription, instanceObjectType,
                                       localPlacementToUse, rep, instanceElemId, proxyType);

                                    if (roomId == ElementId.InvalidElementId)
                                    {
                                        wrapper.AddElement(instanceHandle, setter, extraParams, true);
                                    }
                                    else
                                    {
                                        exporterIFC.RelateSpatialElement(roomId, instanceHandle);
                                        wrapper.AddElement(instanceHandle, setter, extraParams, false);
                                    }
                                }

                                IFCAnyHandle placementToUse = localPlacement;
                                if (!useInstanceGeometry)
                                {
                                    bool needToCreateOpenings = OpeningUtil.NeedToCreateOpenings(instanceHandle, extraParams);
                                    if (needToCreateOpenings)
                                    {
                                        Transform openingTrf = new Transform(oldTrf);
                                        Transform extraRot = new Transform(oldTrf);
                                        extraRot.Origin = XYZ.Zero;
                                        openingTrf = openingTrf.Multiply(extraRot);
                                        openingTrf = openingTrf.Multiply(typeInfo.StyleTransform);

                                        IFCAnyHandle openingRelativePlacement = ExporterUtil.CreateAxis2Placement3D(file, openingTrf.Origin * scale,
                                           openingTrf.get_Basis(2), openingTrf.get_Basis(0));
                                        IFCAnyHandle openingPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacement);
                                        GeometryUtil.SetRelativePlacement(openingPlacement, openingRelativePlacement);
                                        placementToUse = openingPlacement;
                                    }
                                }

                                OpeningUtil.CreateOpeningsIfNecessary(instanceHandle, familyInstance, extraParams, exporterIFC,
                                   placementToUse, setter, wrapper);
                                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, familyInstance, wrapper);
                                break;
                            }
                    }

                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(instanceHandle))
                    {
                        if (doorWindowInfo != null)
                        {
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(doorWindowInfo.GetOpening()))
                            {
                                string relGUID = ExporterIFCUtils.CreateGUID();
                                IFCInstanceExporter.CreateRelFillsElement(file, relGUID, ownerHistory, null, null, doorWindowInfo.GetOpening(), instanceHandle);
                            }
                            else if (doorWindowInfo.NeedsOpening)
                            {
                                bool added = doorWindowInfo.SetDelayedFamilyInstance(instanceHandle, localPlacement, doorWindowInfo.AssignedLevelId);
                                if (added)
                                    exporterIFC.RegisterDoorWindowForOpeningUpdate(doorWindowInfo);
                                else
                                {
                                    // we need to fill a later opening.
                                    exporterIFC.RegisterDoorWindowForUncreatedOpening(familyInstance.Id, instanceHandle);
                                }
                            }
                        }

                        if (!exportParts)
                            CategoryUtil.CreateMaterialAssociations(doc, exporterIFC, instanceHandle, typeInfo.MaterialIds);
                        
                        if (!IFCAnyHandleUtil.IsNullOrHasNoValue(typeInfo.Style))
                            ExporterCacheManager.TypeRelationsCache.Add(typeInfo.Style, instanceHandle);
                    }
                }
            }
        }

        /// <summary>
        /// Exports a family instance as standard element.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element to be exported.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="familyType">
        /// The export type.
        /// </param>
        /// <param name="ifcEnumTypeString">
        /// The string value represents the IFC type.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        static bool ExportFamilyInstanceAsStandardElement(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, IFCExportType familyType, string ifcEnumTypeString, IFCProductWrapper productWrapper)
        {
            switch (familyType)
            {
                // These entities don't get exported as a mapped instance.  As such, we export them using
                // the standard methods.

                // standard building elements
                case IFCExportType.ExportBeam:
                    BeamExporter.ExportBeam(exporterIFC, element, geometryElement, productWrapper);
                    return true;
                case IFCExportType.ExportFooting:
                    FootingExporter.ExportFooting(exporterIFC, element, geometryElement, ifcEnumTypeString, productWrapper);
                    return true;
                case IFCExportType.ExportCovering:
                    CeilingExporter.ExportCovering(exporterIFC, element, geometryElement, ifcEnumTypeString, productWrapper);
                    return true;
                case IFCExportType.ExportRamp:
                    RampExporter.ExportRamp(exporterIFC, ifcEnumTypeString, element, geometryElement, 1, productWrapper);
                    return true;
                case IFCExportType.ExportRailing:
                    if (ExporterCacheManager.RailingCache.Contains(element.Id))
                    {
                        // Don't export this object if it is part of a parent railing.
                        if (!ExporterCacheManager.RailingSubElementCache.Contains(element.Id))
                            RailingExporter.ExportRailing(exporterIFC, element, geometryElement, ifcEnumTypeString, productWrapper);
                    }
                    else
                    {
                        ExporterCacheManager.RailingCache.Add(element.Id);
                    }
                    return true;
                case IFCExportType.ExportRoof:
                    RoofExporter.ExportRoof(exporterIFC, ifcEnumTypeString, element, geometryElement, productWrapper);
                    return true;
                case IFCExportType.ExportSlab:
                    FloorExporter.ExportFloor(exporterIFC, element, geometryElement, ifcEnumTypeString, productWrapper, false);
                    return true;
                case IFCExportType.ExportStair:
                    StairsExporter.ExportStairAsSingleGeometry(exporterIFC, ifcEnumTypeString, element, geometryElement, 1, productWrapper);
                    return true;
                case IFCExportType.ExportWall:
                    WallExporter.ExportWall(exporterIFC, element, geometryElement, productWrapper);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets level offset for extruded columns.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance.
        /// </param>
        /// <param name="overrideLevelId">
        /// The level id.
        /// </param>
        /// <param name="extraParams">
        /// The extrusion creation data.
        /// </param>
        static XYZ GetLevelOffsetForExtrudedColumns(ExporterIFC exporterIFC,
           FamilyInstance familyInstance, ElementId overrideLevelId, IFCExtrusionCreationData extraParams)
        {
            
            IFCFamilyInstanceExtrusionExportResults results = ExporterIFCUtils.ExportFamilyInstanceAsExtrusion(exporterIFC, familyInstance, false, overrideLevelId, extraParams);
            IFCAnyHandle extrusionHandle = results.GetExtrusionHandle();
            XYZ levelOffset = (!IFCAnyHandleUtil.IsNullOrHasNoValue(extrusionHandle)) ? results.ExtraOffset : XYZ.Zero;

            extrusionHandle.Delete();

            return levelOffset;
        }

        /// <summary>
        /// Gets minimum height of a family symbol.
        /// </summary>
        /// <param name="symbol">
        /// The family symbol.
        /// </param>
        static double GetMinSymbolHeight(FamilySymbol symbol)
        {
            return ExporterIFCUtils.GetMinSymbolHeight(symbol);
        }

        /// <summary>
        /// Gets minimum width of a family symbol.
        /// </summary>
        /// <param name="symbol">
        /// The family symbol.
        /// </param>
        static double GetMinSymbolWidth(FamilySymbol symbol)
        {
            return ExporterIFCUtils.GetMinSymbolWidth(symbol);
        }

        /// <summary>
        /// Gets IFCColumnType from column type name.
        /// </summary>
        /// <param name="element">The column element.</param>
        /// <param name="columnType">The column type name.</param>
        /// <returns>The IFCColumnType.</returns>
        static IFCColumnType GetColumnType(Element element, string columnType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = columnType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCColumnType.Column;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCColumnType.UserDefined;

            return IFCColumnType.Column;
        }

        /// <summary>
        /// Gets IFCDoorStyleOperation from Revit IFCDoorStyleOperation.
        /// </summary>
        /// <param name="operation">The Revit IFCDoorStyleOperation.</param>
        /// <returns>The IFCDoorStyleOperation.</returns>
        static Toolkit.IFCDoorStyleOperation GetDoorStyleOperation(Autodesk.Revit.DB.IFC.IFCDoorStyleOperation operation)
        {
            switch(operation)
            {
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorDoubleSwing:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Double_Swing;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorFolding:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Folding;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorSingleSwing:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Single_Swing;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorSingleSwingOppositeLeft:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Single_Swing_Opposite_Left;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorSingleSwingOppositeRight:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Single_Swing_Opposite_Right;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleDoorSliding:
                    return Toolkit.IFCDoorStyleOperation.Double_Door_Sliding;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleSwingLeft:
                    return Toolkit.IFCDoorStyleOperation.Double_Swing_Left;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.DoubleSwingRight:
                    return Toolkit.IFCDoorStyleOperation.Double_Swing_Right;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.FoldingToLeft:
                    return Toolkit.IFCDoorStyleOperation.Folding_To_Left;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.FoldingToRight:
                    return Toolkit.IFCDoorStyleOperation.Folding_To_Right;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.Revolving:
                    return Toolkit.IFCDoorStyleOperation.Revolving;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.RollingUp:
                    return Toolkit.IFCDoorStyleOperation.RollingUp;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.SingleSwingLeft:
                    return Toolkit.IFCDoorStyleOperation.Single_Swing_Left;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.SingleSwingRight:
                    return Toolkit.IFCDoorStyleOperation.Single_Swing_Right;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.SlidingToLeft:
                    return Toolkit.IFCDoorStyleOperation.Sliding_To_Left;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.SlidingToRight:
                    return Toolkit.IFCDoorStyleOperation.Sliding_To_Right;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.UserDefined:
                    return Toolkit.IFCDoorStyleOperation.UserDefined;
                case Autodesk.Revit.DB.IFC.IFCDoorStyleOperation.NotDefined:
                    return Toolkit.IFCDoorStyleOperation.NotDefined;
                default:
                    throw new ArgumentException("No corresponding type.", "operation");
            }
        }

        /// <summary>
        /// Gets IFCDoorStyleConstruction from construction type name.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="constructionType">The construction type name.</param>
        /// <returns>The IFCDoorStyleConstruction.</returns>
        static IFCDoorStyleConstruction GetDoorStyleConstruction(Element element, string constructionType)
        {            
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "Construction", out value))
            {
                value = constructionType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDoorStyleConstruction.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCDoorStyleConstruction.UserDefined;
            if (String.Compare(newValue, "ALUMINIUM", true) == 0)
                return IFCDoorStyleConstruction.Aluminium;
            if (String.Compare(newValue, "HIGH_GRADE_STEEL", true) == 0)
                return IFCDoorStyleConstruction.High_Grade_Steel;
            if (String.Compare(newValue, "STEEL", true) == 0)
                return IFCDoorStyleConstruction.Steel;
            if (String.Compare(newValue, "WOOD", true) == 0)
                return IFCDoorStyleConstruction.Wood;
            if (String.Compare(newValue, "ALUMINIUM_WOOD", true) == 0)
                return IFCDoorStyleConstruction.Aluminium_Wood;
            if (String.Compare(newValue, "ALUMINIUM_PLASTIC", true) == 0)
                return IFCDoorStyleConstruction.Aluminium_Plastic;
            if (String.Compare(newValue, "PLASTIC", true) == 0)
                return IFCDoorStyleConstruction.Plastic;

            return IFCDoorStyleConstruction.UserDefined;
        }
    }
}
