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
    /// Provides methods to export floor elements.
    /// </summary>
    class FloorExporter
    {
        /// <summary>
        /// Exports a floor to IFC slab.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="floor">
        /// The floor element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void Export(ExporterIFC exporterIFC, HostObject floor, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, floor);

            // export parts or not
            bool exportParts = PartExporter.CanExportParts(floor);
            if (exportParts && !PartExporter.CanExportElementInPartExport(floor, floor.Level.Id, false))
                return;

            ExportFloor(exporterIFC, floor, geometryElement, ifcEnumType, productWrapper, exportParts);
        }

        /// <summary>
        /// Exports a floor to IFC slab.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="floor">
        /// The floor element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="ifcEnumType">
        /// The string value represents the IFC type.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        /// <param name="exportParts">
        /// Whether to export parts or not.
        /// </param>
        /// <returns>
        /// True if the floor is exported successfully, false otherwise.
        /// </returns>
        public static void ExportFloor(ExporterIFC exporterIFC, Element floorElement, GeometryElement geometryElement, string ifcEnumType, 
            IFCProductWrapper productWrapper, bool exportParts)
        {
            if (geometryElement == null)
                return;

            IFCFile file = exporterIFC.GetFile();
            IList<IFCAnyHandle> slabHnds = new List<IFCAnyHandle>();
                        
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCTransformSetter transformSetter = IFCTransformSetter.Create())
                {
                    using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, floorElement))
                    {
                        IFCAnyHandle localPlacement = placementSetter.GetPlacement();
                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        bool exportedAsInternalExtrusion = false;

                        double scale = exporterIFC.LinearScale;
                        ElementId catId = CategoryUtil.GetSafeCategoryId(floorElement);

                        IList<IFCAnyHandle> reps = new List<IFCAnyHandle>();
                        IList<IList<CurveLoop>> extrusionLoops = new List<IList<CurveLoop>>();
                        IList<IFCExtrusionCreationData> loopExtraParams = new List<IFCExtrusionCreationData>();
                        Plane floorPlane = GeometryUtil.CreateDefaultPlane();

                        IList<IFCAnyHandle> localPlacements = new List<IFCAnyHandle>();
                        
                        if (!exportParts)
                        {
                            // First, try to use the ExtrusionAnalyzer for the limited cases it handles - 1 solid, no openings, end clippings only.
                            // Also limited to cases with line and arc boundaries.
                            //
                            if (floorElement is Floor)
                            {
                                Floor floor = floorElement as Floor;
                                SolidMeshGeometryInfo solidMeshInfo = GeometryUtil.GetSolidMeshGeometry(geometryElement, Transform.Identity);
                                IList<Solid> solids = solidMeshInfo.GetSolids();
                                IList<Mesh> meshes = solidMeshInfo.GetMeshes();

                                if (solids.Count == 1 && meshes.Count == 0)
                                {
                                    IList<Solid> splitVolumes = SolidUtils.SplitVolumes(solids[0]);
                                    if (splitVolumes.Count == 1)
                                    {
                                        bool completelyClipped;
                                        XYZ floorExtrusionDirection = new XYZ(0, 0, -1);
                                        XYZ modelOrigin = XYZ.Zero;

                                        XYZ floorOrigin = floor.GetVerticalProjectionPoint(modelOrigin, FloorFace.Top);
                                        if (floorOrigin == null)
                                        {
                                            // GetVerticalProjectionPoint may return null if FloorFace.Top is an edited face that doesn't 
                                            // go thruough te Revit model orgigin.  We'll try the midpoint of the bounding box instead.
                                            BoundingBoxXYZ boundingBox = floor.get_BoundingBox(null);
                                            modelOrigin = (boundingBox.Min + boundingBox.Max) / 2.0;
                                            floorOrigin = floor.GetVerticalProjectionPoint(modelOrigin, FloorFace.Top);
                                        }

                                        if (floorOrigin != null)
                                        {
                                            XYZ floorDir = floor.GetNormalAtVerticalProjectionPoint(floorOrigin, FloorFace.Top);
                                            Plane extrusionAnalyzerFloorPlane = new Plane(floorDir, floorOrigin);

                                            HandleAndData floorAndProperties =
                                                ExtrusionExporter.CreateExtrusionWithClippingAndProperties(exporterIFC, floor,
                                                catId, solids[0], extrusionAnalyzerFloorPlane, floorExtrusionDirection, null, out completelyClipped);
                                            if (completelyClipped)
                                                return;
                                            if (floorAndProperties.Handle != null)
                                            {
                                                IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                                                representations.Add(floorAndProperties.Handle);
                                                IFCAnyHandle prodRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);
                                                reps.Add(prodRep);

                                                if (floorAndProperties.Data != null)
                                                    loopExtraParams.Add(floorAndProperties.Data);
                                            }
                                        }
                                    }
                                }
                            }
                        

                            // Use internal routine as backup that handles openings.
                            if (reps.Count == 0)
                            {
                                exportedAsInternalExtrusion = ExporterIFCUtils.ExportSlabAsExtrusion(exporterIFC, floorElement, 
                                    geometryElement, transformSetter, localPlacement, out localPlacements, out reps, 
                                    out extrusionLoops, out loopExtraParams, floorPlane);
                            }

                            if (reps.Count == 0)
                            {
                                using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                                {
                                    BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                                    bodyExporterOptions.TessellationLevel = BodyExporterOptions.BodyTessellationLevel.Coarse;
                                    IFCAnyHandle prodDefHnd = RepresentationUtil.CreateBRepProductDefinitionShape(floorElement.Document.Application, exporterIFC,
                                        floorElement, catId, geometryElement, bodyExporterOptions, null, ecData);
                                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(prodDefHnd))
                                    {
                                        ecData.ClearOpenings();
                                        return;
                                    }

                                    reps.Add(prodDefHnd);
                                }
                            }
                        }

                        // Create the slab from either the extrusion or the BRep information.
                        string ifcGUID = ExporterIFCUtils.CreateGUID(floorElement);

                        int numReps = exportParts ? 1 : reps.Count;
                        for (int ii = 0; ii < numReps; ii++)
                        {
                            string ifcName = NamingUtil.GetNameOverride(floorElement, NamingUtil.CreateIFCName(exporterIFC, ii == 0 ? -1 : ii + 1));
                            string ifcDescription = NamingUtil.GetDescriptionOverride(floorElement, null);
                            string ifcObjectType = NamingUtil.GetObjectTypeOverride(floorElement, exporterIFC.GetFamilyName());
                            string ifcElemId = NamingUtil.CreateIFCElementId(floorElement);

                            string currentGUID = (ii == 0) ? ifcGUID : ExporterIFCUtils.CreateGUID();
                            IFCAnyHandle localPlacementHnd = exportedAsInternalExtrusion ? localPlacements[ii] : localPlacement;
                            IFCSlabType slabType = GetIFCSlabType(ifcEnumType);

                            IFCAnyHandle slabHnd = IFCInstanceExporter.CreateSlab(file, currentGUID, ownerHistory, ifcName,
                               ifcDescription, ifcObjectType, localPlacementHnd, exportParts ? null : reps[ii], ifcElemId, slabType);

                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(slabHnd))
                                return;

                            if (exportParts)
                            {
                                PartExporter.ExportHostPart(exporterIFC, floorElement, slabHnd, productWrapper, placementSetter, localPlacementHnd, null);
                            }

                            slabHnds.Add(slabHnd);
                        }

                        bool associateElementToLevel = LevelUtil.AssociateElementToLevel(floorElement);
                        for (int ii = 0; ii < numReps; ii++)
                        {
                            IFCExtrusionCreationData loopExtraParam = ii < loopExtraParams.Count ? loopExtraParams[ii] : null;
                            productWrapper.AddElement(slabHnds[ii], placementSetter, loopExtraParam, associateElementToLevel);
                        }

                        if (exportedAsInternalExtrusion)
                            ExporterIFCUtils.ExportExtrudedSlabOpenings(exporterIFC, floorElement, placementSetter,
                               localPlacements[0], slabHnds, extrusionLoops, floorPlane, productWrapper);

                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, floorElement, productWrapper);
                    }

                    if (!exportParts)
                    {
                        HostObjectExporter.ExportHostObjectMaterials(exporterIFC, floorElement as HostObject, slabHnds, 
                            geometryElement, productWrapper, ElementId.InvalidElementId, Toolkit.IFCLayerSetDirection.Axis3);
                    }
                }
                tr.Commit();

                return;
            }
        }

        /// <summary>
        /// Gets IFCSlabType from slab type name.
        /// </summary>
        /// <param name="ifcEnumType">The slab type name.</param>
        /// <returns>The IFCSlabType.</returns>
        public static IFCSlabType GetIFCSlabType(string ifcEnumType)
        {
            if (String.IsNullOrEmpty(ifcEnumType))
                return IFCSlabType.Floor;

            string ifcEnumTypeWithoutSpaces = ifcEnumType.Replace(" ", "").Replace("_", "");

            if (String.Compare(ifcEnumTypeWithoutSpaces, "USERDEFINED", true) == 0)
                return IFCSlabType.UserDefined;
            if (String.Compare(ifcEnumTypeWithoutSpaces, "FLOOR", true) == 0)
                return IFCSlabType.Floor;
            if (String.Compare(ifcEnumTypeWithoutSpaces, "ROOF", true) == 0)
                return IFCSlabType.Roof;
            if (String.Compare(ifcEnumTypeWithoutSpaces, "LANDING", true) == 0)
                return IFCSlabType.Landing;
            if (String.Compare(ifcEnumTypeWithoutSpaces, "BASESLAB", true) == 0)
                return IFCSlabType.BaseSlab;

            return IFCSlabType.Floor;
        }
    }
}
