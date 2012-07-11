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
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export curtain systems.
    /// </summary>
    class CurtainSystemExporter
    {
        /// <summary>
        /// Exports curtain object as container.
        /// </summary>
        /// <param name="allSubElements">
        /// Collection of elements contained in the host curtain element.
        /// </param>
        /// <param name="wallElement">
        /// The curtain wall element.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportCurtainObjectCommonAsContainer(ICollection<ElementId> allSubElements, Element wallElement,
           ExporterIFC exporterIFC, IFCProductWrapper origWrapper)
        {
            if (wallElement == null)
                return;

            HashSet<ElementId> alreadyVisited = new HashSet<ElementId>();  // just in case.
            Options geomOptions = GeometryUtil.GetIFCExportGeometryOptions();
            {
                foreach (ElementId subElemId in allSubElements)
                {
                    using (IFCProductWrapper productWrapper = IFCProductWrapper.Create(origWrapper))
                    {
                        Element subElem = wallElement.Document.GetElement(subElemId);
                        if (subElem == null)
                            continue;
                        GeometryElement geomElem = subElem.get_Geometry(geomOptions);
                        if (geomElem == null)
                            continue;

                        if (alreadyVisited.Contains(subElem.Id))
                            continue;
                        alreadyVisited.Add(subElem.Id);

                        try
                        {
                            if (subElem is FamilyInstance)
                            {
                                if (subElem is Mullion)
                                {
                                    if (exporterIFC.ExportAs2x2)
                                        ProxyElementExporter.Export(exporterIFC, subElem, geomElem, productWrapper);
                                    else
                                    {
                                        using (IFCPlacementSetter currSetter = IFCPlacementSetter.Create(exporterIFC, wallElement))
                                        {
                                            IFCAnyHandle currLocalPlacement = currSetter.GetPlacement();
                                            using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
                                            {
                                                MullionExporter.Export(exporterIFC, subElem as Mullion, geomElem, currLocalPlacement, extraParams, currSetter,
                                                    productWrapper);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FamilyInstance subFamInst = subElem as FamilyInstance;

                                    string ifcEnumType;
                                    ElementId catId = CategoryUtil.GetSafeCategoryId(subElem);
                                    IFCExportType exportType = ElementFilteringUtil.GetExportTypeFromCategoryId(catId, out ifcEnumType);

                                    if (exporterIFC.ExportAs2x2)
                                    {
                                        if ((exportType == IFCExportType.DontExport) || (exportType == IFCExportType.ExportPlateType) ||
                                           (exportType == IFCExportType.ExportMemberType))
                                            exportType = IFCExportType.ExportBuildingElementProxy;
                                    }
                                    else
                                    {
                                        if (exportType == IFCExportType.DontExport)
                                        {
                                            ifcEnumType = "CURTAIN_PANEL";
                                            exportType = IFCExportType.ExportPlateType;
                                        }
                                    }
                                    FamilyInstanceExporter.ExportFamilyInstanceAsMappedItem(exporterIFC, subFamInst, exportType, ifcEnumType, productWrapper,
                                       ElementId.InvalidElementId, null);
                                }
                            }
                            else if (subElem is CurtainGridLine)
                            {
                                ProxyElementExporter.Export(exporterIFC, subElem, geomElem, productWrapper);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ExporterUtil.IsFatalException(wallElement.Document, ex))
                                throw ex;
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exports curtain object as one Brep.
        /// </summary>
        /// <param name="allSubElements">
        /// Collection of elements contained in the host curtain element.
        /// </param>
        /// <param name="wallElement">
        /// The curtain wall element.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="setter">
        /// The IFCPlacementSetter object.
        /// </param>
        /// <param name="localPlacement">
        /// The local placement handle.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle ExportCurtainObjectCommonAsOneBRep(ICollection<ElementId> allSubElements, Element wallElement,
           ExporterIFC exporterIFC, IFCPlacementSetter setter, IFCAnyHandle localPlacement)
        {
            IFCAnyHandle prodDefRep = null;
            Document document = wallElement.Document;
            double eps = document.Application.VertexTolerance * exporterIFC.LinearScale;

            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle contextOfItems = exporterIFC.Get3DContextHandle("Body");

            IFCGeometryInfo info = IFCGeometryInfo.CreateFaceGeometryInfo(eps);

            IList<IFCAnyHandle> bodyItems = new List<IFCAnyHandle>();

            // Want to make sure we don't accidentally add a mullion or curtain line more than once.
            HashSet<ElementId> alreadyVisited = new HashSet<ElementId>();

            Options geomOptions = GeometryUtil.GetIFCExportGeometryOptions();
            foreach (ElementId subElemId in allSubElements)
            {
                Element subElem = wallElement.Document.GetElement(subElemId);
                GeometryElement geomElem = subElem.get_Geometry(geomOptions);
                if (geomElem == null)
                    continue;

                if (alreadyVisited.Contains(subElem.Id))
                    continue;
                alreadyVisited.Add(subElem.Id);

                ExporterIFCUtils.CollectGeometryInfo(exporterIFC, info, geomElem, XYZ.Zero, false);
                HashSet<IFCAnyHandle> faces = new HashSet<IFCAnyHandle>(info.GetSurfaces());
                IFCAnyHandle outer = IFCInstanceExporter.CreateClosedShell(file, faces);

                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(outer))
                    bodyItems.Add(RepresentationUtil.CreateFacetedBRep(exporterIFC, document, outer, ElementId.InvalidElementId));
            }

            if (bodyItems.Count == 0)
                return prodDefRep;

            ElementId catId = CategoryUtil.GetSafeCategoryId(wallElement);
            IFCAnyHandle shapeRep = RepresentationUtil.CreateBRepRep(exporterIFC, wallElement, catId, contextOfItems, bodyItems);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(shapeRep))
                return prodDefRep;

            IList<IFCAnyHandle> shapeReps = new List<IFCAnyHandle>();
            shapeReps.Add(shapeRep);
            prodDefRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapeReps);
            return prodDefRep;
        }

        /// <summary>
        /// Checks if the curtain element can be exported as container.
        /// </summary>
        /// <remarks>
        /// It checks if all sub elements to be exported have geometries.
        /// </remarks>
        /// <param name="allSubElements">
        /// Collection of elements contained in the host curtain element.
        /// </param>
        /// <param name="document">
        /// The Revit document.
        /// </param>
        /// <returns>
        /// True if it can be exported as container, false otherwise.
        /// </returns>
        private static bool CanExportCurtainWallAsContainer(ICollection<ElementId> allSubElements, Document document)
        {
            Options geomOptions = GeometryUtil.GetIFCExportGeometryOptions();

            FilteredElementCollector collector = new FilteredElementCollector(document, allSubElements);

            List<Type> curtainWallSubElementTypes = new List<Type>();
            curtainWallSubElementTypes.Add(typeof(FamilyInstance));
            curtainWallSubElementTypes.Add(typeof(CurtainGridLine));
            
            ElementMulticlassFilter multiclassFilter = new ElementMulticlassFilter(curtainWallSubElementTypes, true);
            collector.WherePasses(multiclassFilter);
            ICollection<ElementId> filteredSubElemments = collector.ToElementIds();
            foreach (ElementId subElemId in filteredSubElemments)
            {
                Element subElem = document.GetElement(subElemId);
                GeometryElement geomElem = subElem.get_Geometry(geomOptions);
                if (geomElem == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Export Curtain Walls and Roofs.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="allSubElements">
        /// Collection of elements contained in the host curtain element.
        /// </param>
        /// <param name="element">
        /// The element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportBase(ExporterIFC exporterIFC, ICollection<ElementId> allSubElements,
           Element element, IFCProductWrapper wrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            IFCPlacementSetter setter = null;

            using (IFCProductWrapper curtainWallSubWrapper = IFCProductWrapper.Create(wrapper, false))
            {
                try
                {
                    IFCAnyHandle localPlacement = null;
                    bool canExportCurtainWallAsContainer = CanExportCurtainWallAsContainer(allSubElements, element.Document);
                    IFCAnyHandle rep = null;
                    if (!canExportCurtainWallAsContainer)
                    {
                        setter = IFCPlacementSetter.Create(exporterIFC, element);
                        localPlacement = setter.GetPlacement();
                        rep = ExportCurtainObjectCommonAsOneBRep(allSubElements, element, exporterIFC, setter, localPlacement);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(rep))
                            return;
                    }
                    else
                    {
                        ExportCurtainObjectCommonAsContainer(allSubElements, element, exporterIFC, curtainWallSubWrapper);
                        // This has to go LAST.  Why?  Because otherwise we apply the level transform twice -- once in the familyTrf, once here.
                        // This will be used just to put the CurtainWall on the right level.
                        setter = IFCPlacementSetter.Create(exporterIFC, element);
                        localPlacement = setter.GetPlacement();
                    }

                    string objectType = NamingUtil.CreateIFCObjectName(exporterIFC, element);

                    {
                        IFCAnyHandle prodRepHnd = null;
                        IFCAnyHandle elemHnd = null;
                        string elemGUID = ExporterIFCUtils.CreateGUID(element);
                        string elemName = NamingUtil.GetNameOverride(element, exporterIFC.GetName());
                        string elemDesc = NamingUtil.GetDescriptionOverride(element, null);
                        string elemType = NamingUtil.GetObjectTypeOverride(element, objectType);
                        string elemId = NamingUtil.CreateIFCElementId(element);
                        if (element is Wall || element is CurtainSystem || IsLegacyCurtainElement(element))
                        {
                            elemHnd = IFCInstanceExporter.CreateCurtainWall(file, elemGUID, ownerHistory, elemName, elemDesc, elemType, localPlacement, prodRepHnd, elemId);
                        }
                        else if (element is RoofBase)
                        {
                            //need to convert the string to enum
                            string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, element);
                            elemHnd = IFCInstanceExporter.CreateRoof(file, elemGUID, ownerHistory, elemName, elemDesc, elemType, localPlacement,
                                prodRepHnd, elemId, RoofExporter.GetIFCRoofType(ifcEnumType));
                        }
                        else
                        {
                            return;
                        }

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(elemHnd))
                            return;
                        
                        wrapper.AddElement(elemHnd, setter, null, true);

                        ExporterIFCUtils.CreateCurtainWallPropertySet(exporterIFC, element, wrapper);
                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, wrapper);
                        ICollection<IFCAnyHandle> relatedElementIds = curtainWallSubWrapper.GetAllObjects();
                        if (relatedElementIds.Count > 0)
                        {
                            string guid = ExporterIFCUtils.CreateSubElementGUID(element, (int)IFCCurtainWallSubElements.RelAggregates);
                            HashSet<IFCAnyHandle> relatedElementIdSet = new HashSet<IFCAnyHandle>(relatedElementIds);
                            IFCInstanceExporter.CreateRelAggregates(file, guid, ownerHistory, null, null, elemHnd, relatedElementIdSet);
                        }
                        exporterIFC.RegisterSpaceBoundingElementHandle(elemHnd, element.Id, ElementId.InvalidElementId);
                    }
                }
                finally
                {
                    if (setter != null)
                        setter.Dispose();
                }
            }
        }

        /// <summary>
        /// Exports a curtain wall to IFC curtain wall.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostElement">
        /// The host object element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportWall(ExporterIFC exporterIFC, Wall hostElement, IFCProductWrapper productWrapper)
        {
            // Don't export the Curtain Wall itself, which has no useful geometry; instead export all of the GReps of the
            // mullions and panels.
            CurtainGrid grid = hostElement.CurtainGrid;
            if (grid == null)
                return;

            ICollection<ElementId> allSubElements = grid.GetPanelIds();
            foreach (ElementId subElem in grid.GetMullionIds())
                allSubElements.Add(subElem);
            ExportBase(exporterIFC, allSubElements, hostElement, productWrapper);
        }

        /// <summary>
        /// Exports a curtain roof to IFC curtain wall.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostElement">
        /// The host object element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportExtrusionRoof(ExporterIFC exporterIFC, ExtrusionRoof hostElement, IFCProductWrapper productWrapper)
        {
            // Don't export the Curtain Wall itself, which has no useful geometry; instead export all of the GReps of the
            // mullions and panels.
            CurtainGridSet grids = hostElement.CurtainGrids;
            if (grids == null || grids.Size == 0)
                return;

            ICollection<ElementId> allSubElements = new HashSet<ElementId>();
            foreach (CurtainGrid grid in grids)
            {
                foreach (ElementId panelId in grid.GetPanelIds())
                    allSubElements.Add(panelId);
                foreach (ElementId subElem in grid.GetMullionIds())
                    allSubElements.Add(subElem);
            }
            ExportBase(exporterIFC, allSubElements, hostElement, productWrapper);
        }

        /// <summary>
        /// Exports a curtain roof to IFC curtain wall.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostElement">
        /// The host object element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportFootPrintRoof(ExporterIFC exporterIFC, FootPrintRoof hostElement, IFCProductWrapper productWrapper)
        {
            // Don't export the Curtain Wall itself, which has no useful geometry; instead export all of the GReps of the
            // mullions and panels.
            CurtainGridSet grids = hostElement.CurtainGrids;
            if (grids == null || grids.Size == 0)
                return;

            ICollection<ElementId> allSubElements = new HashSet<ElementId>();
            foreach (CurtainGrid grid in grids)
            {
                foreach (ElementId panelId in grid.GetPanelIds())
                    allSubElements.Add(panelId);
                foreach (ElementId subElem in grid.GetMullionIds())
                    allSubElements.Add(subElem);
            }
            ExportBase(exporterIFC, allSubElements, hostElement, productWrapper);
        }
		
        /// <summary>
        /// Exports a curtain system to IFC curtain system.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="hostElement">
        /// The curtain system element to be exported.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportCurtainSystem(ExporterIFC exporterIFC, CurtainSystem curtainSystem, IFCProductWrapper productWrapper)
        {
            CurtainGridSet grids = curtainSystem.CurtainGrids;
            if (grids == null || grids.Size == 0)
                return;

            ICollection<ElementId> allSubElements = new HashSet<ElementId>();
            foreach (CurtainGrid grid in grids)
            {
                foreach (ElementId panelId in grid.GetPanelIds())
                    allSubElements.Add(panelId);
                foreach (ElementId subElem in grid.GetMullionIds())
                    allSubElements.Add(subElem);
            }

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                ExportBase(exporterIFC, allSubElements, curtainSystem, productWrapper);
                transaction.Commit();
            }
        }
		
		/// <summary>
        /// Exports a legacy curtain element to IFC curtain wall.
        /// </summary>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="curtainElement">The curtain element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportLegacyCurtainElement(ExporterIFC exporterIFC, Element curtainElement, IFCProductWrapper productWrapper)
        {
            ICollection<ElementId> allSubElements = ExporterIFCUtils.GetLegacyCurtainSubElements(curtainElement);

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                ExportBase(exporterIFC, allSubElements, curtainElement, productWrapper);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Checks if the element is legacy curtain element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if it is legacy curtain element.</returns>
        public static bool IsLegacyCurtainElement(Element element)
        {
            //for now, it is sufficient to check its category.
            return (CategoryUtil.GetSafeCategoryId(element) == new ElementId(BuiltInCategory.OST_Curtain_Systems));
        }
    }
}
