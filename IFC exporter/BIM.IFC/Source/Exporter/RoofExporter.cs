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
    /// Provides methods to export Roof elements.
    /// </summary>
    class RoofExporter
    {
        /// <summary>
        /// Exports a roof to IfcRoof.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ifcEnumType">The roof type.</param>
        /// <param name="roof">The roof element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportRoof(ExporterIFC exporterIFC, string ifcEnumType, Element roof, GeometryElement geometryElement, 
            IFCProductWrapper productWrapper)
        {
            if (roof == null || geometryElement == null)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, roof))
                {
                    using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                    {
                        ecData.PossibleExtrusionAxes = IFCExtrusionAxes.TryZ;
                        ecData.AreInnerRegionsOpenings = true;
                        ecData.SetLocalPlacement(placementSetter.GetPlacement());

                        ElementId categoryId = CategoryUtil.GetSafeCategoryId(roof);

                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                        IFCAnyHandle representation = RepresentationUtil.CreateBRepProductDefinitionShape(roof.Document.Application, exporterIFC, roof,
                            categoryId, geometryElement, bodyExporterOptions, null, ecData);

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(representation))
                        {
                            ecData.ClearOpenings();
                            return;
                        }

                        bool exportSlab = ecData.ScaledLength > MathUtil.Eps();

                        string guid = ExporterIFCUtils.CreateGUID(roof);
                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        string origRoofName = exporterIFC.GetName();
                        string roofName = NamingUtil.GetNameOverride(roof, origRoofName);
                        string roofDescription = NamingUtil.GetDescriptionOverride(roof, null);
                        string roofObjectType = NamingUtil.GetObjectTypeOverride(roof, NamingUtil.CreateIFCObjectName(exporterIFC, roof));
                        IFCAnyHandle localPlacement = ecData.GetLocalPlacement();
                        string elementTag = NamingUtil.CreateIFCElementId(roof);
                        IFCRoofType roofType = GetIFCRoofType(ifcEnumType);

                        IFCAnyHandle roofHnd = IFCInstanceExporter.CreateRoof(file, guid, ownerHistory, roofName, roofDescription,
                            roofObjectType, localPlacement, exportSlab ? null : representation, elementTag, roofType);

                        productWrapper.AddElement(roofHnd, placementSetter.GetLevelInfo(), ecData, LevelUtil.AssociateElementToLevel(roof));

                        if (exportSlab)
                        {
                            string slabGUID = ExporterIFCUtils.CreateSubElementGUID(roof, (int)IFCRoofSubElements.RoofSlabStart);
                            string slabName = roofName + ":1";
                            IFCAnyHandle slabLocalPlacementHnd = ExporterUtil.CopyLocalPlacement(file, localPlacement);

                            IFCAnyHandle slabHnd = IFCInstanceExporter.CreateSlab(file, slabGUID, ownerHistory, slabName,
                               roofDescription, roofObjectType, slabLocalPlacementHnd, representation, elementTag, IFCSlabType.Roof);
                            OpeningUtil.CreateOpeningsIfNecessary(slabHnd, roof, ecData, exporterIFC, slabLocalPlacementHnd,
                                placementSetter, productWrapper);

                            ExporterUtil.RelateObject(exporterIFC, roofHnd, slabHnd);

                            productWrapper.AddElement(slabHnd, placementSetter.GetLevelInfo(), ecData, false);
                        }
                    }
                    tr.Commit();
                }
            }
        }
        
        /// <summary>
        /// Exports a roof to IfcRoof.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="roof">The roof element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void Export(ExporterIFC exporterIFC, RoofBase roof, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                // export parts or not
                bool exportParts = PartExporter.CanExportParts(roof);
                bool exportAsExtrusionCurtainRoof = false;
                bool exportAsFootprintCurtainRoof = false;
                if (roof is ExtrusionRoof)
                {
                    ExtrusionRoof extrusionRoof = roof as ExtrusionRoof;
                    exportAsExtrusionCurtainRoof = ((extrusionRoof.CurtainGrids != null) && (extrusionRoof.CurtainGrids.Size != 0));
                }
                else if (roof is FootPrintRoof)
                {
                    FootPrintRoof footprintRoof = roof as FootPrintRoof;
                    exportAsFootprintCurtainRoof = ((footprintRoof.CurtainGrids != null) && (footprintRoof.CurtainGrids.Size != 0));
                }

                if (exportParts)
                {
                    if (!PartExporter.CanExportElementInPartExport(roof, roof.Level.Id, false))
                        return;
                    ExportRoofAsParts(exporterIFC, roof, geometryElement, productWrapper); // Right now, only flat roof could have parts.
                }
                else if (exportAsExtrusionCurtainRoof)
                {
                    CurtainSystemExporter.ExportExtrusionRoof(exporterIFC, roof as ExtrusionRoof, productWrapper);
                }
                else if (exportAsFootprintCurtainRoof)
                {
                    CurtainSystemExporter.ExportFootPrintRoof(exporterIFC, roof as FootPrintRoof, productWrapper);
                }
                else
                {
                    string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, roof);
                    
                    IFCAnyHandle roofHnd = ExporterIFCUtils.ExportRoofAsContainer(exporterIFC, ifcEnumType, roof, 
                        geometryElement, productWrapper);
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(roofHnd))
                        ExportRoof(exporterIFC, ifcEnumType, roof, geometryElement, productWrapper);

                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, roof, productWrapper);

                    // call for host objects; curtain roofs excused from call (no material information)
                    HostObjectExporter.ExportHostObjectMaterials(exporterIFC, roof, productWrapper.GetAnElement(),
                        geometryElement, productWrapper, ElementId.InvalidElementId, IFCLayerSetDirection.Axis3);
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Export the roof to IfcRoof containing its parts.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The roof element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportRoofAsParts(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, element))
                {
                    IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                    IFCAnyHandle localPlacement = setter.GetPlacement();

                    using (IFCExtrusionCreationData extrusionCreationData = new IFCExtrusionCreationData())
                    {
                        extrusionCreationData.SetLocalPlacement(setter.GetPlacement());
                        extrusionCreationData.PossibleExtrusionAxes = IFCExtrusionAxes.TryXY;

                        IFCAnyHandle prodRepHnd = null;

                        string elementGUID = ExporterIFCUtils.CreateGUID(element);
                        string origElementName = exporterIFC.GetName();
                        string elementName = NamingUtil.GetNameOverride(element, origElementName);
                        string elementDescription = NamingUtil.GetDescriptionOverride(element, null);
                        string elementObjectType = NamingUtil.GetObjectTypeOverride(element, exporterIFC.GetFamilyName());
                        string elementId = NamingUtil.CreateIFCElementId(element);

                        //need to convert the string to enum
                        string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, element);
                        IFCAnyHandle roofHandle = IFCInstanceExporter.CreateRoof(file, elementGUID, ownerHistory, elementName, elementDescription, elementObjectType, localPlacement, prodRepHnd, elementId, GetIFCRoofType(ifcEnumType));

                        // Export the parts
                        PartExporter.ExportHostPart(exporterIFC, element, roofHandle, productWrapper, setter, localPlacement, null);

                        productWrapper.AddElement(roofHandle, setter, extrusionCreationData, LevelUtil.AssociateElementToLevel(element));

                        OpeningUtil.CreateOpeningsIfNecessary(roofHandle, element, extrusionCreationData, exporterIFC, extrusionCreationData.GetLocalPlacement(), setter, productWrapper);

                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                    }
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Gets IFCRoofType from roof type name.
        /// </summary>
        /// <param name="roofTypeName">The roof type name.</param>
        /// <returns>The IFCRoofType.</returns>
        public static IFCRoofType GetIFCRoofType(string roofTypeName)
        {
            string typeName = roofTypeName.Replace(" ", "").Replace("_", "");

            if (String.Compare(typeName, "ROOFTYPEENUM", true) == 0 ||
                String.Compare(typeName, "ROOFTYPEENUMFREEFORM", true) == 0)
                return Toolkit.IFCRoofType.FreeForm;
            if (String.Compare(typeName, "FLAT", true) == 0 ||
                String.Compare(typeName, "FLATROOF", true) == 0)
                return Toolkit.IFCRoofType.Flat_Roof;
            if (String.Compare(typeName, "SHED", true) == 0 ||
                String.Compare(typeName, "SHEDROOF", true) == 0)
                return Toolkit.IFCRoofType.Shed_Roof;
            if (String.Compare(typeName, "GABLE", true) == 0 ||
                String.Compare(typeName, "GABLEROOF", true) == 0)
                return Toolkit.IFCRoofType.Gable_Roof;
            if (String.Compare(typeName, "HIP", true) == 0 ||
                String.Compare(typeName, "HIPROOF", true) == 0)
                return Toolkit.IFCRoofType.Hip_Roof;
            if (String.Compare(typeName, "HIPPED_GABLE", true) == 0 ||
                String.Compare(typeName, "HIPPED_GABLEROOF", true) == 0)
                return Toolkit.IFCRoofType.Hipped_Gable_Roof;
            if (String.Compare(typeName, "MANSARD", true) == 0 ||
                String.Compare(typeName, "MANSARDROOF", true) == 0)
                return Toolkit.IFCRoofType.Mansard_Roof;
            if (String.Compare(typeName, "BARREL", true) == 0 ||
                String.Compare(typeName, "BARRELROOF", true) == 0)
                return Toolkit.IFCRoofType.Barrel_Roof;
            if (String.Compare(typeName, "BUTTERFLY", true) == 0 ||
                String.Compare(typeName, "BUTTERFLYROOF", true) == 0)
                return Toolkit.IFCRoofType.Butterfly_Roof;
            if (String.Compare(typeName, "PAVILION", true) == 0 ||
                String.Compare(typeName, "PAVILIONROOF", true) == 0)
                return Toolkit.IFCRoofType.Pavilion_Roof;
            if (String.Compare(typeName, "DOME", true) == 0 ||
                String.Compare(typeName, "DOMEROOF", true) == 0)
                return Toolkit.IFCRoofType.Dome_Roof;

            return Toolkit.IFCRoofType.NotDefined;
        }
    }
}
