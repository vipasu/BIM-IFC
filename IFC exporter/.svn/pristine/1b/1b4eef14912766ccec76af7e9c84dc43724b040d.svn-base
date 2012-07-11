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
using Autodesk.Revit.DB.Structure;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export beams.
    /// </summary>
    class RebarExporter
    {
        /// <summary>
        /// Exports a Rebar to IFC ReinforcingMesh.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportRebar(ExporterIFC exporterIFC,
           Rebar element, Autodesk.Revit.DB.View filterView, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element))
                {
                    GeometryElement rebarGeometry = ExporterIFCUtils.GetRebarGeometry(element, filterView);

                    // only options are: Not Export, BuildingElementProxy, or ReinforcingBar/Mesh, depending on layout.
                    // Not Export is handled previously, and ReinforcingBar vs Mesh will be determined below.
                    string ifcEnumType;
                    IFCExportType exportType = ExporterUtil.GetExportType(exporterIFC, element, out ifcEnumType);

                    if (exportType == IFCExportType.ExportBuildingElementProxy)
                    {
                        if (rebarGeometry != null)
                        {
                            ProxyElementExporter.ExportBuildingElementProxy(exporterIFC, element, rebarGeometry, productWrapper);
                            transaction.Commit();
                        }
                        return;
                    }

                    IFCAnyHandle prodRep = null;
                    using (IFCExtrusionCreationData extrusionCreationData = new IFCExtrusionCreationData())
                    {
                        extrusionCreationData.SetLocalPlacement(setter.GetPlacement());

                        if (rebarGeometry != null)
                        {
                            ElementId categoryId = CategoryUtil.GetSafeCategoryId(element);

                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            prodRep = BodyExporter.ExportBody(element.Document.Application, exporterIFC, element, categoryId, rebarGeometry, bodyExporterOptions,
                                extrusionCreationData).RepresentationHnd;
                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(prodRep))
                                extrusionCreationData.ClearOpenings();
                        }

                        double scale = exporterIFC.LinearScale;

                        double barLength = element.TotalLength * scale;
                        if (MathUtil.IsAlmostZero(barLength))
                            return;

                        int quantity = element.Quantity;
                        if (quantity < 1)
                            return;

                        ElementId typeId = element.GetTypeId();
                        RebarBarType elementType = element.Document.GetElement(element.GetTypeId()) as RebarBarType;
                        double diameter = elementType == null ? 1.0 / 12.0 : elementType.BarDiameter;
                        double longitudinalBarNominalDiameter = diameter * scale;
                        double longitudinalBarCrossSectionArea = (element.Volume * scale * scale * scale) / barLength;

                        string steelGradeOpt = null;
                        IFCAnyHandle elemHnd = null;

                        string rebarGUID = ExporterIFCUtils.CreateGUID(element);
                        string origRebarName = exporterIFC.GetName();
                        string rebarName = NamingUtil.GetNameOverride(element, origRebarName);
                        string rebarDescription = NamingUtil.GetDescriptionOverride(element, null);
                        string rebarObjectType = NamingUtil.GetObjectTypeOverride(element, NamingUtil.CreateIFCObjectName(exporterIFC, element));
                        string rebarElemId = NamingUtil.CreateIFCElementId(element);

                        if (element.LayoutRule == RebarLayoutRule.Single || (quantity == 1))
                        {
                            IFCReinforcingBarRole role = IFCReinforcingBarRole.NotDefined;
                            elemHnd = IFCInstanceExporter.CreateReinforcingBar(file, rebarGUID, exporterIFC.GetOwnerHistoryHandle(),
                                rebarName, rebarDescription, rebarObjectType, extrusionCreationData.GetLocalPlacement(),
                                prodRep, rebarElemId, steelGradeOpt, longitudinalBarNominalDiameter, longitudinalBarCrossSectionArea,
                                barLength, role, null);
                        }
                        else
                        {
                            double meshLength;
                            double longitudinalBarSpacing;
                            double val = element.ArrayLength * scale;

                            if (element.LayoutRule == RebarLayoutRule.NumberWithSpacing)
                            {
                                longitudinalBarSpacing = val;
                                meshLength = val * (quantity - 1);
                            }
                            else
                            {
                                meshLength = val;
                                longitudinalBarSpacing = val / (quantity - 1);
                            }

                            double meshWidth = diameter * scale; // array is in one direction only.
                            double transverseBarNominalDiameter = longitudinalBarNominalDiameter;
                            double transverseBarCrossSectionArea = longitudinalBarCrossSectionArea;
                            double transverseBarSpacing = longitudinalBarSpacing;

                            elemHnd = IFCInstanceExporter.CreateReinforcingMesh(file, rebarGUID,
                                exporterIFC.GetOwnerHistoryHandle(), rebarName,
                                rebarDescription, rebarObjectType, extrusionCreationData.GetLocalPlacement(), prodRep, rebarElemId,
                                steelGradeOpt, meshLength, meshWidth, longitudinalBarNominalDiameter,
                                transverseBarNominalDiameter, longitudinalBarCrossSectionArea,
                                transverseBarCrossSectionArea, longitudinalBarSpacing,
                                transverseBarSpacing);
                        }

                        productWrapper.AddElement(elemHnd, setter.GetLevelInfo(), extrusionCreationData, true);

                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                    }
                }
                transaction.Commit();
            }
        }
    }
}
