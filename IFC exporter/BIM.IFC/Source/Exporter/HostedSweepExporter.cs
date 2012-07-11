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
    /// Provides methods to export hosted sweeps.
    /// </summary>
    class HostedSweepExporter
    {
        /// <summary>
        /// Exports a hosted weep.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="hostedSweep">The hosted sweep element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void Export(ExporterIFC exporterIFC, HostedSweep hostedSweep, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            ElementId catId = CategoryUtil.GetSafeCategoryId(hostedSweep);
            if (catId == new ElementId(BuiltInCategory.OST_Gutter))
                ExportGutter(exporterIFC, hostedSweep, geometryElement, productWrapper);
            else
                ProxyElementExporter.Export(exporterIFC, hostedSweep, geometryElement, productWrapper);
        }

        /// <summary>
        /// Exports a gutter element.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportGutter(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element))
                {
                    using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                    {
                        ecData.SetLocalPlacement(setter.GetPlacement());

                        ElementId categoryId = CategoryUtil.GetSafeCategoryId(element);

                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions();
                        IFCAnyHandle bodyRep = BodyExporter.ExportBody(element.Document.Application, exporterIFC, element, categoryId,
                            geometryElement, bodyExporterOptions, ecData).RepresentationHnd;
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                        {
                            if (ecData != null)
                                ecData.ClearOpenings();
                            return;
                        }

                        IFCAnyHandle origin = ExporterUtil.CreateAxis2Placement3D(file);
                        IFCAnyHandle repMap3dHnd = IFCInstanceExporter.CreateRepresentationMap(file, origin, bodyRep);
                        List<IFCAnyHandle> repMapList = new List<IFCAnyHandle>();
                        repMapList.Add(repMap3dHnd);
                        string elementTypeName = NamingUtil.CreateIFCObjectName(exporterIFC, element);
                        IFCAnyHandle style = IFCInstanceExporter.CreatePipeSegmentType(file, ExporterIFCUtils.CreateGUID(element), exporterIFC.GetOwnerHistoryHandle(),
                            elementTypeName, null, null, null, repMapList, NamingUtil.CreateIFCElementId(element), elementTypeName, IFCPipeSegmentType.Gutter);


                        List<IFCAnyHandle> representationMaps = GeometryUtil.GetRepresentationMaps(style);
                        IFCAnyHandle mappedItem = ExporterUtil.CreateDefaultMappedItem(file, representationMaps[0]);

                        IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                        representations.Add(mappedItem);

                        IFCAnyHandle bodyMappedItemRep = RepresentationUtil.CreateBodyMappedItemRep(exporterIFC,
                            element, categoryId, exporterIFC.Get3DContextHandle("Body"), representations);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyMappedItemRep))
                            return;

                        List<IFCAnyHandle> shapeReps = new List<IFCAnyHandle>();
                        shapeReps.Add(bodyMappedItemRep);

                        IFCAnyHandle prodRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapeReps);
                        IFCAnyHandle localPlacementToUse;
                        ElementId roomId = setter.UpdateRoomRelativeCoordinates(element, out localPlacementToUse);
                        if (roomId == ElementId.InvalidElementId)
                            localPlacementToUse = ecData.GetLocalPlacement();
                        string name = NamingUtil.GetNameOverride(element, NamingUtil.CreateIFCName(exporterIFC, -1));
                        string description = NamingUtil.GetDescriptionOverride(element, null);
                        string objectType = NamingUtil.GetObjectTypeOverride(element, elementTypeName);
                        IFCAnyHandle elemHnd = IFCInstanceExporter.CreateFlowSegment(file, ExporterIFCUtils.CreateGUID(element),
                            exporterIFC.GetOwnerHistoryHandle(), name, description, objectType, localPlacementToUse, prodRep,
                            NamingUtil.CreateIFCElementId(element));

                        if (roomId == ElementId.InvalidElementId)
                        {
                            productWrapper.AddElement(elemHnd, setter.GetLevelInfo(), ecData, true);
                        }
                        else
                        {
                            exporterIFC.RelateSpatialElement(roomId, elemHnd);
                            productWrapper.AddElement(elemHnd, setter.GetLevelInfo(), ecData, false);
                        }

                        OpeningUtil.CreateOpeningsIfNecessary(elemHnd, element, ecData, exporterIFC,
                           localPlacementToUse, setter, productWrapper);
                    }

                    tr.Commit();
                }
            }
        }
    }
}
