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
using BIM.IFC.Exporter;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Utility
{
    /// <summary>
    /// Provides static methods to create varies IFC representations.
    /// </summary>
    class RepresentationUtil
    {
        /// <summary>
        /// Creates a shape representation or appends existing ones to original representation.
        /// </summary>
        /// <remarks>
        /// This function has two modes. 
        /// If originalShapeRepresentation has no value, then this function will create a new ShapeRepresentation handle. 
        /// If originalShapeRepresentation has a value, then it is expected to be an aggregation of representations, and the new representation
        /// will be appended to the end of the list.
        /// </remarks>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="identifierOpt">
        /// The identifier for the representation.
        /// </param>
        /// <param name="representationTypeOpt">
        /// The type handle for the representation.
        /// </param>
        /// <param name="items">
        /// Collection of geometric representation items that are defined for this representation.
        /// </param>
        /// <param name="originalShapeRepresentation">
        /// The original shape representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateOrAppendShapeRepresentation(ExporterIFC exporterIFC, Element element, ElementId categoryId, IFCAnyHandle contextOfItems,
           string identifierOpt, string representationTypeOpt, ICollection<IFCAnyHandle> items, IFCAnyHandle originalShapeRepresentation)
        {
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(originalShapeRepresentation))
            {
                GeometryUtil.AddItemsToShape(originalShapeRepresentation, items);
                return originalShapeRepresentation;
            }

            return CreateShapeRepresentation(exporterIFC, element, categoryId, contextOfItems, identifierOpt, representationTypeOpt, items);
        }

        /// <summary>
        /// Creates a shape representation and register it to shape representation layer.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="identifier">
        /// The identifier for the representation.
        /// </param>
        /// <param name="representationType">
        /// The type handle for the representation.
        /// </param>
        /// <param name="items">
        /// Collection of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateShapeRepresentation(ExporterIFC exporterIFC, Element element, ElementId categoryId, IFCAnyHandle contextOfItems,
           string identifier, string representationType, ICollection<IFCAnyHandle> items)
        {
            IFCFile file = exporterIFC.GetFile();
            HashSet<IFCAnyHandle> itemSet = new HashSet<IFCAnyHandle>(items);
            IFCAnyHandle newShapeRepresentation = IFCInstanceExporter.CreateShapeRepresentation(file, contextOfItems, identifier, representationType, itemSet);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(newShapeRepresentation))
                return newShapeRepresentation;

            // We are using the DWG export layer table to correctly map category to DWG layer for the 
            // IfcPresentationLayerAsssignment.
            exporterIFC.RegisterShapeForPresentationLayer(element, categoryId, newShapeRepresentation);
            return newShapeRepresentation;
        }

        /// <summary>
        /// Creates a shape representation and register it to shape representation layer.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="identifierOpt">
        /// The identifier for the representation.
        /// </param>
        /// <param name="representationTypeOpt">
        /// The type handle for the representation.
        /// </param>
        /// <param name="items">
        /// List of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateShapeRepresentation(ExporterIFC exporterIFC, Element element, ElementId categoryId, IFCAnyHandle contextOfItems,
           string identifierOpt, string representationTypeOpt, IList<IFCAnyHandle> items)
        {
            HashSet<IFCAnyHandle> itemSet = new HashSet<IFCAnyHandle>();
            foreach (IFCAnyHandle axisItem in items)
                itemSet.Add(axisItem);
            return CreateShapeRepresentation(exporterIFC, element, categoryId, contextOfItems, identifierOpt, representationTypeOpt, itemSet);
        }

        /// <summary>
        /// Creates an IfcFacetedBrep handle.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="shell">
        /// The closed shell handle.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateFacetedBRep(ExporterIFC exporterIFC, Document document, IFCAnyHandle shell, ElementId overrideMaterialId)
        {
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle brep = IFCInstanceExporter.CreateFacetedBrep(file, shell);

            // Coordination View V2 does not support styled items by default.  IFC2x2 does, even if it isn't widely used, so we can't use ExportAnnotations.
            if (!ExporterCacheManager.ExportOptionsCache.ExportAs2x3CoordinationView2)
            {
                BodyExporter.CreateSurfaceStyleForRepItem(exporterIFC, document, brep, overrideMaterialId);

            }
            return brep;
        }

        /// <summary>
        /// Creates a sweep solid representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <param name="originalShapeRepresentation">
        /// The original shape representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateSweptSolidRep(ExporterIFC exporterIFC, Element element, ElementId categoryId, IFCAnyHandle contextOfItems, 
            ICollection<IFCAnyHandle> bodyItems, IFCAnyHandle originalRepresentation)
        {
            string identifierOpt = "Body";	// this is by IFC2x2 convention, not temporary
            string repTypeOpt = "SweptSolid";  // this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation =
               CreateOrAppendShapeRepresentation(exporterIFC, element, categoryId, contextOfItems, identifierOpt, repTypeOpt,
                  bodyItems, originalRepresentation);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a clipping representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateClippingRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
           IFCAnyHandle contextOfItems, HashSet<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Body";	// this is by IFC2x2 convention, not temporary
            string repTypeOpt = "Clipping";  // this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a Brep representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBRepRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
           IFCAnyHandle contextOfItems, ICollection<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Body";	// this is by IFC2x2 convention, not temporary
            string repTypeOpt = "Brep";	// this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a Solid model representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateSolidModelRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
           IFCAnyHandle contextOfItems, ICollection<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Body";
            string repTypeOpt = "SolidModel";
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a Brep representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <param name="exportAsFacetation">
        /// If this is true, the identifier for the representation is "Facetation" as required by IfcSite.
        /// If this is false, the identifier for the representation is "Body" as required by IfcBuildingElement.
        /// </param>
        /// <param name="originalShapeRepresentation">
        /// The original shape representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateSurfaceRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IFCAnyHandle contextOfItems, ICollection<IFCAnyHandle> bodyItems, bool exportAsFacetation, IFCAnyHandle originalRepresentation)
        {
            string identifierOpt = null;
            if (exportAsFacetation)
                identifierOpt = "Facetation";
            else
                identifierOpt = "Body";	// this is by IFC2x3 convention, not temporary

            string repTypeOpt = "SurfaceModel";  // this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation = CreateOrAppendShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems, originalRepresentation);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a boundary representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Collection of geometric representation items that are defined for this representation.
        /// </param>
        /// <param name="originalShapeRepresentation">
        /// The original shape representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBoundaryRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IFCAnyHandle contextOfItems, ICollection<IFCAnyHandle> bodyItems, IFCAnyHandle originalRepresentation)
        {
            string identifierOpt = "FootPrint";	// this is by IFC2x3 convention, not temporary

            string repTypeOpt = "Curve2D";  // this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation = CreateOrAppendShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems, originalRepresentation);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a geometric set representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="type">
        /// The representation type.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateGeometricSetRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
           string type, IFCAnyHandle contextOfItems, HashSet<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = type;
            string repTypeOpt = "GeometricSet";	// this is by IFC2x2 convention, not temporary
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a body mapped item representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBodyMappedItemRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
           IFCAnyHandle contextOfItems, IList<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Body";	// this is by IFC2x2+ convention
            string repTypeOpt = "MappedRepresentation";  // this is by IFC2x2+ convention
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
               contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a plan mapped item representation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="contextOfItems">
        /// The context for which the different subtypes of representation are valid. 
        /// </param>
        /// <param name="bodyItems">
        /// Set of geometric representation items that are defined for this representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreatePlanMappedItemRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IFCAnyHandle contextOfItems, HashSet<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Annotation";	// this is by IFC2x2+ convention
            string repTypeOpt = "MappedRepresentation";  // this is by IFC2x2+ convention
            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
                contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates an annotation representation.
        /// </summary>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="element">The element.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="contextOfItems">The context for which the different subtypes of representation are valid.</param>
        /// <param name="bodyItems">Set of geometric representation items that are defined for this representation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAnnotationSetRep(ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IFCAnyHandle contextOfItems, HashSet<IFCAnyHandle> bodyItems)
        {
            string identifierOpt = "Annotation";
            string repTypeOpt = "Annotation2D";	// this is by IFC2x3 convention

            IFCAnyHandle bodyRepresentation = CreateShapeRepresentation(exporterIFC, element, categoryId,
                contextOfItems, identifierOpt, repTypeOpt, bodyItems);
            return bodyRepresentation;
        }

        /// <summary>
        /// Creates a Brep product definition shape representation.
        /// </summary>
        /// <remarks>
        /// It will try to export the geometry as an extrusion if it is not exported as IFC 2x2 version.
        /// </remarks>
        /// <param name="application">
        /// The Revit application object.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="bodyExporterOptions">
        /// The body exporter options.
        /// </param>
        /// <param name="extraReps">
        /// Extra representations (e.g. Axis, Boundary).  May be null.
        /// </param>
        /// <param name="extrusionCreationData">
        /// The extrusion creation data. 
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBRepProductDefinitionShape(Autodesk.Revit.ApplicationServices.Application application,
            ExporterIFC exporterIFC, Element element, ElementId categoryId,
            GeometryElement geometryElement, BodyExporterOptions bodyExporterOptions, IList<IFCAnyHandle> extraReps, 
            IFCExtrusionCreationData extrusionCreationData)
        {
            BodyData bodyData;
            return CreateBRepProductDefinitionShape(application, exporterIFC, element, categoryId,
            geometryElement, bodyExporterOptions, extraReps, extrusionCreationData, out bodyData);
        }

        /// <summary>
        /// Creates a Brep product definition shape representation.
        /// </summary>
        /// <remarks>
        /// It will try to export the geometry as an extrusion if it is not exported as IFC 2x2 version.
        /// </remarks>
        /// <param name="application">
        /// The Revit application object.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="bodyExporterOptions">
        /// The body exporter options.
        /// </param>
        /// <param name="extraReps">
        /// Extra representations (e.g. Axis, Boundary).  May be null.
        /// </param>
        /// <param name="extrusionCreationData">
        /// The extrusion creation data. 
        /// </param>
        /// <param name="bodyData">
        /// The body data.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBRepProductDefinitionShape(Autodesk.Revit.ApplicationServices.Application application,
            ExporterIFC exporterIFC, Element element, ElementId categoryId,
            GeometryElement geometryElement, BodyExporterOptions bodyExporterOptions, IList<IFCAnyHandle> extraReps, IFCExtrusionCreationData extrusionCreationData,
            out BodyData bodyData)
        {
            SolidMeshGeometryInfo info = null;
            IList<GeometryObject> solids = new List<GeometryObject>();

            if (!exporterIFC.ExportAs2x2)
            {
                info = GeometryUtil.GetSolidMeshGeometry(geometryElement, Transform.Identity);
                IList<Mesh> meshes = info.GetMeshes();
                if (meshes.Count == 0)
                {
                    IList<Solid> solidList = info.GetSolids();
                    foreach (Solid solid in solidList)
                    {
                        solids.Add(solid);
                    }
                }
            }

            IFCAnyHandle ret = null;
            if (solids.Count == 0)
            {
                IList<GeometryObject> geometryList = new List<GeometryObject>();
                geometryList.Add(geometryElement);
                ret = CreateBRepProductDefinitionShape(application, exporterIFC, element, categoryId,
                    geometryList, extraReps, bodyExporterOptions, extrusionCreationData, out bodyData);
            }
            else
            {
                bodyExporterOptions.TryToExportAsExtrusion = true;
                ret = CreateBRepProductDefinitionShape(application, exporterIFC, element, categoryId,
                    solids, extraReps, bodyExporterOptions, extrusionCreationData, out bodyData);
            }
            return ret;
        }

        /// <summary>
        /// Creates a Brep product definition shape representation.
        /// </summary>
        /// <param name="application">The Revit application object.</param>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="geometryObject">The geometry object.</param>
        /// <param name="extraReps">Extra representations (e.g. Axis, Footprint).  May be null.</param>
        /// <param name="options">The settings for how to export the body.</param>
        /// <param name="extrusionCreationData">The extrusion creation data.</param>
        /// <param name="bodyData">The body data.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBRepProductDefinitionShape(Autodesk.Revit.ApplicationServices.Application application,
            ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IList<GeometryObject> geometryObjectIn, IList<IFCAnyHandle> extraReps,
            BodyExporterOptions bodyExporterOptions, IFCExtrusionCreationData extrusionCreationData, out BodyData bodyData)
        {

            bodyData = BodyExporter.ExportBody(application, exporterIFC, element, categoryId, geometryObjectIn,
                bodyExporterOptions, extrusionCreationData);
            IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
            List<IFCAnyHandle> bodyReps = new List<IFCAnyHandle>();
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
            {
                if (extrusionCreationData != null)
                    extrusionCreationData.ClearOpenings();
            }
            else
                bodyReps.Add(bodyRep);

            if (extraReps != null)
            {
                foreach (IFCAnyHandle hnd in extraReps)
                    bodyReps.Add(hnd);
            }

            if (bodyReps.Count == 0)
                return null;
            return IFCInstanceExporter.CreateProductDefinitionShape(exporterIFC.GetFile(), null, null, bodyReps);
        }

        /// <summary>
        /// Creates a Brep product definition shape representation.
        /// </summary>
        /// <param name="application">
        /// The Revit application object.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="categoryId">
        /// The category id.
        /// </param>
        /// <param name="geometryObject">
        /// The geometry object.
        /// </param>
        /// <param name="extraReps">
        /// Extra representations (e.g. Axis, Footprint).  May be null.
        /// </param>
        /// <param name="options">The settings for how to export the body.</param>
        /// <param name="extrusionCreationData">
        /// The extrusion creation data. 
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateBRepProductDefinitionShape(Autodesk.Revit.ApplicationServices.Application application,
            ExporterIFC exporterIFC, Element element, ElementId categoryId,
            IList<GeometryObject> geometryObjectIn, IList<IFCAnyHandle> extraReps,
            BodyExporterOptions bodyExporterOptions, IFCExtrusionCreationData extrusionCreationData)
        {
            BodyData bodyData;
            return CreateBRepProductDefinitionShape(application, exporterIFC, element, categoryId, geometryObjectIn, extraReps, bodyExporterOptions,
                extrusionCreationData, out bodyData);
        }

        /// <summary>
        /// Creates a surface product definition shape representation.
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
        /// <param name="exportBoundaryRep">
        /// If this is true, it will export boundary representations.
        /// </param>
        /// <param name="exportAsFacetation">
        /// If this is true, it will export the geometry as facetation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateSurfaceProductDefinitionShape(ExporterIFC exporterIFC, Element element,
           GeometryElement geometryElement, bool exportBoundaryRep, bool exportAsFacetation)
        {
            IFCAnyHandle bodyRep = null;
            IFCAnyHandle boundaryRep = null;
            return CreateSurfaceProductDefinitionShape(exporterIFC, element, geometryElement, exportBoundaryRep, exportAsFacetation, ref bodyRep, ref boundaryRep);
        }

        /// <summary>
        /// Creates a surface product definition shape representation.
        /// </summary>
        /// <remarks>
        /// If a body representation is supplied, then we expect that this is already contained in a representation list, inside
        /// a product representation. As such, just modify the list and return.
        /// </remarks>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="exportBoundaryRep">
        /// If this is true, it will export boundary representations.
        /// </param>
        /// <param name="exportAsFacetation">
        /// If this is true, it will export the geometry as facetation.
        /// </param>
        /// <param name="bodyRep">
        /// Body representation.
        /// </param>
        /// <param name="boundaryRep">
        /// Boundary representation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateSurfaceProductDefinitionShape(ExporterIFC exporterIFC, Element element,
           GeometryElement geometryElement, bool exportBoundaryRep, bool exportAsFacetation, ref IFCAnyHandle bodyRep, ref IFCAnyHandle boundaryRep)
        {
            bool hasOriginalBodyRepresentation = bodyRep != null;
            bool success = SurfaceExporter.ExportSurface(exporterIFC, element, geometryElement, exportBoundaryRep, exportAsFacetation, ref bodyRep, ref boundaryRep);

            if (!success)
                return null;

            // If we supplied a body representation, then we expect that this is already contained in a representation list, inside
            // a product representation.  As such, just modify the list and return.
            if (hasOriginalBodyRepresentation)
                return null;

            List<IFCAnyHandle> representations = new List<IFCAnyHandle>();
            representations.Add(bodyRep);
            if (exportBoundaryRep && !IFCAnyHandleUtil.IsNullOrHasNoValue(boundaryRep))
                representations.Add(boundaryRep);

            return IFCInstanceExporter.CreateProductDefinitionShape(exporterIFC.GetFile(), null, null, representations);
        }
    }
}
