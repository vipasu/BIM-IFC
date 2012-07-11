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
    /// Provides methods to export sites.
    /// </summary>
    class SiteExporter
    {
        /// <summary>
        /// Exports topography surface as IFC site object.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="topSurface">
        /// The TopographySurface object.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportTopographySurface(ExporterIFC exporterIFC, TopographySurface topSurface, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            ExportSiteBase(exporterIFC, null, topSurface, geometryElement, productWrapper);
            PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, topSurface, productWrapper);
        }

        /// <summary>
        /// Exports IFC site object if having latitude and longitude.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="document">
        /// The Revit document.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportDefaultSite(ExporterIFC exporterIFC, Document document, IFCProductWrapper productWrapper)
        {
            ExportSiteBase(exporterIFC, document, null, null, productWrapper);
        }

        /// <summary>
        /// Base implementation to export IFC site object.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="document">
        /// The Revit document.
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
        private static void ExportSiteBase(ExporterIFC exporterIFC, Document document, Element element, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            IFCAnyHandle siteHandle = exporterIFC.GetSite();

            int numSiteElements = (!IFCAnyHandleUtil.IsNullOrHasNoValue(siteHandle) ? 1 : 0);
            if (element == null && (numSiteElements != 0))
                return;

            Document doc = document;
            if (doc == null)
            {
                if (element != null)
                    doc = element.Document;
                else
                    throw new ArgumentException("Both document and element are null.");
            }

            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                IFCAnyHandle siteRepresentation = null;
                if (element != null)
                {
                    // It would be possible that they actually represent several different sites with different buildings, 
                    // but until we have a concept of a building in Revit, we have to assume 0-1 sites, 1 building.
                    bool appendedToSite = false;
                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(siteHandle))
                    {
                        IList<IFCAnyHandle> representations = IFCAnyHandleUtil.GetProductRepresentations(siteHandle);
                        if (representations.Count > 0)
                        {
                            IFCAnyHandle bodyRep = representations[0];
                            IFCAnyHandle boundaryRep = null;
                            if (representations.Count > 1)
                                boundaryRep = representations[1];

                            siteRepresentation = RepresentationUtil.CreateSurfaceProductDefinitionShape(exporterIFC, element, geometryElement, true, true, ref bodyRep, ref boundaryRep);
                            appendedToSite = true;
                        }
                    }

                    if (!appendedToSite)
                    {
                        siteRepresentation = RepresentationUtil.CreateSurfaceProductDefinitionShape(exporterIFC, element, geometryElement, true, true);
                    }
                }

                List<int> latitude = new List<int>();
                List<int> longitude = new List<int>();
                ProjectLocation projLocation = doc.ActiveProjectLocation;

                IFCAnyHandle relativePlacement = null;
                double elevation = 0.0;
                if (projLocation != null)
                {
                    double latitudeInDeg = projLocation.SiteLocation.Latitude * 180 / Math.PI;
                    double longitudeInDeg = projLocation.SiteLocation.Longitude * 180 / Math.PI;


                    int latDeg = ((int)latitudeInDeg); latitudeInDeg -= latDeg; latitudeInDeg *= 60;
                    int latMin = ((int)latitudeInDeg); latitudeInDeg -= latMin; latitudeInDeg *= 60;
                    int latSec = ((int)latitudeInDeg); latitudeInDeg -= latSec; latitudeInDeg *= 1000000;
                    int latFracSec = ((int)latitudeInDeg);
                    latitude.Add(latDeg);
                    latitude.Add(latMin);
                    latitude.Add(latSec);
                    if (!exporterIFC.ExportAs2x2)
                        latitude.Add(latFracSec);

                    int longDeg = ((int)longitudeInDeg); longitudeInDeg -= longDeg; longitudeInDeg *= 60;
                    int longMin = ((int)longitudeInDeg); longitudeInDeg -= longMin; longitudeInDeg *= 60;
                    int longSec = ((int)longitudeInDeg); longitudeInDeg -= longSec; longitudeInDeg *= 1000000;
                    int longFracSec = ((int)longitudeInDeg);
                    longitude.Add(longDeg);
                    longitude.Add(longMin);
                    longitude.Add(longSec);
                    if (!exporterIFC.ExportAs2x2)
                        longitude.Add(longFracSec);

                    Transform siteSharedCoordinatesTrf = projLocation.GetTransform().Inverse;
                    if (!siteSharedCoordinatesTrf.IsIdentity)
                    {
                        XYZ orig = siteSharedCoordinatesTrf.Origin;
                        orig = orig.Multiply(exporterIFC.LinearScale);
                        relativePlacement = ExporterUtil.CreateAxis2Placement3D(file, orig, siteSharedCoordinatesTrf.BasisZ, siteSharedCoordinatesTrf.BasisX);
                    }
                }

                // Get elevation for site.
                elevation = -LevelUtil.GetReferenceHeightForRelativeElevation(doc) * exporterIFC.LinearScale;

                if (IFCAnyHandleUtil.IsNullOrHasNoValue(relativePlacement))
                    relativePlacement = ExporterUtil.CreateAxis2Placement3D(file);

                IFCAnyHandle localPlacement = IFCInstanceExporter.CreateLocalPlacement(file, null, relativePlacement);
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                string objectType = NamingUtil.CreateIFCObjectName(exporterIFC, element);

                if (element != null)
                {
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(siteHandle))
                    {
                        string instanceGUID = ExporterIFCUtils.CreateGUID(element);
                        string origInstanceName = exporterIFC.GetName();
                        string instanceName = NamingUtil.GetNameOverride(element, origInstanceName);
                        string instanceDescription = NamingUtil.GetDescriptionOverride(element, null);
                        string instanceObjectType = NamingUtil.GetObjectTypeOverride(element, objectType);
                        string instanceElemId = NamingUtil.CreateIFCElementId(element);

                        siteHandle = IFCInstanceExporter.CreateSite(file, instanceGUID, ownerHistory, instanceName, instanceDescription, instanceObjectType, localPlacement,
                           siteRepresentation, null, Toolkit.IFCElementComposition.Element, latitude, longitude, elevation, null, null);
                    }
                }
                else
                {
                    // don't bother if we have nothing in the site whatsoever.
                    if ((latitude.Count == 0 || longitude.Count == 0) && IFCAnyHandleUtil.IsNullOrHasNoValue(relativePlacement))
                        return;

                    string defaultSiteName = "Default";
                    siteHandle = IFCInstanceExporter.CreateSite(file, ExporterIFCUtils.CreateProjectLevelGUID(doc, IFCProjectLevelGUIDType.Site), ownerHistory, defaultSiteName, null, objectType, localPlacement,
                       null, null, Toolkit.IFCElementComposition.Element, latitude, longitude, elevation, null, null);
                }

                productWrapper.AddSite(siteHandle);
                exporterIFC.SetSite(siteHandle);

                tr.Commit();
            }
        }
    }
}
