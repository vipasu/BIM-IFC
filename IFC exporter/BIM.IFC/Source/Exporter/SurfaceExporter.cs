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
    /// Provides methods to export geometry to surface representation.
    /// </summary>
    class SurfaceExporter
    {
        /// <summary>
        /// Exports a geometry element to boundary representation.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="exportBoundaryRep">True if to export boundary representation.</param>
        /// <param name="exportAsFacetation">True if to export the geometry as facetation.</param>
        /// <param name="bodyRep">Body representation.</param>
        /// <param name="boundaryRep">Boundary representation.</param>
        /// <returns>True if success, false if fail.</returns>
        public static bool ExportSurface(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement,
           bool exportBoundaryRep, bool exportAsFacetation, ref IFCAnyHandle bodyRep, ref IFCAnyHandle boundaryRep)
        {
            if (geometryElement == null)
                return false;

            IFCGeometryInfo ifcGeomInfo = null;
            Plane plane = GeometryUtil.CreateDefaultPlane();
            XYZ projDir = new XYZ(0, 0, 1);
            double eps = element.Document.Application.VertexTolerance * exporterIFC.LinearScale;

            ifcGeomInfo = IFCGeometryInfo.CreateFaceGeometryInfo(exporterIFC, plane, projDir, eps, exportBoundaryRep);

            ExporterIFCUtils.CollectGeometryInfo(exporterIFC, ifcGeomInfo, geometryElement, XYZ.Zero, true);

            IFCFile file = exporterIFC.GetFile();
            HashSet<IFCAnyHandle> faceSets = new HashSet<IFCAnyHandle>();
            IList<ICollection<IFCAnyHandle>> faceList = ifcGeomInfo.GetFaces();
            foreach (ICollection<IFCAnyHandle> faces in faceList)
            {
                // no faces, don't complain.
                if (faces.Count == 0)
                    continue;
                HashSet<IFCAnyHandle> faceSet = new HashSet<IFCAnyHandle>(faces);
                faceSets.Add(IFCInstanceExporter.CreateConnectedFaceSet(file, faceSet));
            }

            if (faceSets.Count == 0)
                return false;

            IFCAnyHandle surface = IFCInstanceExporter.CreateFaceBasedSurfaceModel(file, faceSets);

            IList<IFCAnyHandle> surfaceItems = new List<IFCAnyHandle>();
            surfaceItems.Add(surface);

            ElementId catId = CategoryUtil.GetSafeCategoryId(element);

            bodyRep = RepresentationUtil.CreateSurfaceRep(exporterIFC, element, catId, exporterIFC.Get3DContextHandle("Body"), surfaceItems, 
                exportAsFacetation, bodyRep);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                return false;

            ICollection<IFCAnyHandle> boudaryRepresentations = ifcGeomInfo.GetRepresentations();
            if (exportBoundaryRep && boudaryRepresentations.Count > 1)
            {
                boundaryRep = RepresentationUtil.CreateBoundaryRep(exporterIFC, element, catId, exporterIFC.Get3DContextHandle("Body"), boudaryRepresentations, 
                    boundaryRep);
            }

            return true;
        }
    }
}
