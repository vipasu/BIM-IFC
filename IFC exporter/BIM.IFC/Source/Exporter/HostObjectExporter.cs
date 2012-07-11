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
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export host objects.
    /// </summary>
    class HostObjectExporter
    {
        /// <summary>
        /// Exports materials for host object.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostObject">
        /// The host object.
        /// </param>
        /// <param name="elemHnds">
        /// The host IFC handles.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="levelId">
        /// The level id.
        /// </param>
        /// <param name="direction">
        /// The IFCLayerSetDirection.
        /// </param>
        /// <returns>
        /// True if exported successfully, false otherwise.
        /// </returns>
        public static bool ExportHostObjectMaterials(ExporterIFC exporterIFC, HostObject hostObject,
            IList<IFCAnyHandle> elemHnds, GeometryElement geometryElement, IFCProductWrapper productWrapper,
            ElementId levelId, Toolkit.IFCLayerSetDirection direction)
        {
            if (hostObject == null)
                return true; //nothing to do

            if (elemHnds == null || (elemHnds.Count == 0))
                return true; //nothing to do

            IFCFile file = exporterIFC.GetFile();
            
            // Roofs with no components are only allowed one material.  We will arbitrarily choose the thickest material.
            IFCAnyHandle primaryMaterialHnd = null;

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                if (productWrapper != null)
                    productWrapper.ClearFinishMaterials();

                double scale = exporterIFC.LinearScale;

                double scaledOffset = 0.0, scaledWallWidth = 0.0, wallHeight = 0.0;
                Wall wall = hostObject as Wall;
                if (wall != null)
                {
                    scaledWallWidth = wall.Width * scale;
                    scaledOffset = -scaledWallWidth / 2.0;
                    BoundingBoxXYZ boundingBox = wall.get_BoundingBox(null);
                    if (boundingBox != null)
                        wallHeight = boundingBox.Max.Z - boundingBox.Min.Z;
                }

                ElementId typeElemId = hostObject.GetTypeId();
                IFCAnyHandle materialLayerSet = ExporterCacheManager.MaterialLayerSetCache.Find(typeElemId);
                if (IFCAnyHandleUtil.IsNullOrHasNoValue(materialLayerSet))
                {
                    HostObjAttributes hostObjAttr = hostObject.Document.GetElement(typeElemId) as HostObjAttributes;
                    if (hostObjAttr == null)
                        return true; //nothing to do

                    List<ElementId> matIds = new List<ElementId>();
                    List<double> widths = new List<double>();
                    List<MaterialFunctionAssignment> functions = new List<MaterialFunctionAssignment>();
                    ElementId baseMatId = CategoryUtil.GetBaseMaterialIdForElement(hostObject);
                    CompoundStructure cs = hostObjAttr.GetCompoundStructure();
                    if (cs != null)
                    {
                        //TODO: Vertically compound structures are not yet supported by export.
                        if (!cs.IsVerticallyHomogeneous() && !MathUtil.IsAlmostZero(wallHeight))
                            cs = cs.GetSimpleCompoundStructure(wallHeight, wallHeight / 2.0);

                        for (int i = 0; i < cs.LayerCount; ++i)
                        {
                            ElementId matId = cs.GetMaterialId(i);
                            if (matId != ElementId.InvalidElementId)
                            {
                                matIds.Add(matId);
                            }
                            else
                            {
                                matIds.Add(baseMatId);
                            }
                            widths.Add(cs.GetLayerWidth(i));
                            // save layer function into IFCProductWrapper, 
                            // it's used while exporting "Function" of Pset_CoveringCommon
                            functions.Add(cs.GetLayerFunction(i));
                        }
                    }

                    if (matIds.Count == 0)
                    {
                        matIds.Add(baseMatId);
                        widths.Add(cs != null ? cs.GetWidth() : 0);
                        functions.Add(MaterialFunctionAssignment.None);
                    }

                    List<IFCAnyHandle> layers = new List<IFCAnyHandle>();
                    double thickestLayer = 0.0;
                    for (int i = 0; i < matIds.Count; ++i)
                    {
                        if (widths[i] < MathUtil.Eps())
                            continue;

                        IFCAnyHandle materialHnd = CategoryUtil.GetOrCreateMaterialHandle(hostObjAttr.Document, exporterIFC, matIds[i]);
                        if (primaryMaterialHnd == null || (widths[i] > thickestLayer))
                        {
                            primaryMaterialHnd = materialHnd;
                            thickestLayer = widths[i];
                        }

                        double scaledWidth = widths[i] * scale;
                        IFCAnyHandle materialLayer = IFCInstanceExporter.CreateMaterialLayer(file, materialHnd, scaledWidth, null);
                        layers.Add(materialLayer);
                        if ((productWrapper != null) && (functions[i] == MaterialFunctionAssignment.Finish1 || functions[i] == MaterialFunctionAssignment.Finish2))
                        {
                            productWrapper.AddFinishMaterial(materialHnd);
                        }
                    }

                    if (layers.Count == 0)
                        return false;

                    string layerSetName = NamingUtil.CreateIFCFamilyName(exporterIFC, -1);
                    materialLayerSet = IFCInstanceExporter.CreateMaterialLayerSet(file, layers, layerSetName);

                    ExporterCacheManager.MaterialLayerSetCache.Register(typeElemId, materialLayerSet);
                }

                // IfcMaterialLayerSetUsage is not supported for IfcWall, only IfcWallStandardCase.
                IFCAnyHandle layerSetUsage = null;
                for (int ii = 0; ii < elemHnds.Count; ii++)
                {
                    IFCAnyHandle elemHnd = elemHnds[ii];
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(elemHnd))
                        continue;

                    HashSet<IFCAnyHandle> relDecomposesSet = IFCAnyHandleUtil.GetRelDecomposes(elemHnd);
                    
                    IList<IFCAnyHandle> subElemHnds = new List<IFCAnyHandle>();
                    if (relDecomposesSet != null && relDecomposesSet.Count == 1)
                    {
                        IFCAnyHandle relAggregates = relDecomposesSet.First();
                        if (IFCAnyHandleUtil.IsTypeOf(relAggregates, IFCEntityType.IfcRelAggregates))
                        {
                            IFCData ifcData = relAggregates.GetAttribute("RelatedObjects");
                            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
                            {
                                IFCAggregate aggregate = ifcData.AsAggregate();
                                if (aggregate != null && aggregate.Count > 0)
                                {
                                    foreach (IFCData val in aggregate)
                                    {
                                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                                        {
                                            subElemHnds.Add(val.AsInstance());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool hasSubElems = !(subElemHnds.Count == 0);
                    bool isRoof = IFCAnyHandleUtil.IsTypeOf(elemHnd, IFCEntityType.IfcRoof);
                    if (!hasSubElems && !isRoof && !IFCAnyHandleUtil.IsTypeOf(elemHnd, IFCEntityType.IfcWall))
                    {
                        if (layerSetUsage == null)
                        {
                            bool flipDirSense = true;
                            if (wall != null)
                            {
                                // if we have flipped the center curve on export, we need to take that into account here.
                                // We flip the center curve on export if it is an arc and it has a negative Z direction.
                                LocationCurve locCurve = wall.Location as LocationCurve;
                                if (locCurve != null)
                                {
                                    Curve curve = locCurve.Curve;
                                    Plane defPlane = new Plane(XYZ.BasisX, XYZ.BasisY, XYZ.Zero);
                                    bool curveFlipped = GeometryUtil.MustFlipCurve(defPlane, curve);
                                    flipDirSense = !(wall.Flipped ^ curveFlipped);
                                }
                            }
                            else if (hostObject is Floor)
                            {
                                flipDirSense = false;
                            }

                            double offsetFromReferenceLine = flipDirSense ? -scaledOffset : scaledOffset;
                            IFCDirectionSense sense = flipDirSense ? IFCDirectionSense.Negative : IFCDirectionSense.Positive;

                            layerSetUsage = IFCInstanceExporter.CreateMaterialLayerSetUsage(file, materialLayerSet, direction, sense, offsetFromReferenceLine);
                        }
                        ExporterCacheManager.MaterialLayerRelationsCache.Add(layerSetUsage, elemHnd);
                    }
                    else
                    {
                        if (hasSubElems)
                        {
                            foreach (IFCAnyHandle subElemHnd in subElemHnds)
                            {
                                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(subElemHnd))
                                    ExporterCacheManager.MaterialLayerRelationsCache.Add(materialLayerSet, subElemHnd);
                            }
                        }
                        else if (!isRoof)
                        {
                            ExporterCacheManager.MaterialLayerRelationsCache.Add(materialLayerSet, elemHnd);
                        }   
                        else if (primaryMaterialHnd != null)
                        {
                            ExporterCacheManager.MaterialLayerRelationsCache.Add(primaryMaterialHnd, elemHnd);
                        }
                    }

                    exporterIFC.RegisterSpaceBoundingElementHandle(elemHnd, hostObject.Id, levelId);
                }

                tr.Commit();
                return true;
            }
        }

                /// <summary>
        /// Exports materials for host object.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostObject">
        /// The host object.
        /// </param>
        /// <param name="elemHnd">
        /// The host IFC handle.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="levelId">
        /// The level id.
        /// </param>
        /// <param name="direction">
        /// The IFCLayerSetDirection.
        /// </param>
        /// <returns>
        /// True if exported successfully, false otherwise.
        /// </returns>
        public static bool ExportHostObjectMaterials(ExporterIFC exporterIFC, HostObject hostObject,
            IFCAnyHandle elemHnd, GeometryElement geometryElement, IFCProductWrapper productWrapper,
            ElementId levelId, Toolkit.IFCLayerSetDirection direction)
        {
            IList<IFCAnyHandle> elemHnds = new List<IFCAnyHandle>();
            elemHnds.Add(elemHnd);
            return ExportHostObjectMaterials(exporterIFC, hostObject, elemHnds, geometryElement, productWrapper, levelId, direction);
        }
    }
}
