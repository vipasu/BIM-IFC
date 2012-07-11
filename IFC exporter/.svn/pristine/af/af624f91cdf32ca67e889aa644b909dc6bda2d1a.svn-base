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


namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export curve elements.
    /// </summary>
    class CurveElementExporter
    {
        /// <summary>
        /// Checks if the curve element should be exported.
        /// </summary>
        /// <param name="curveElement">
        /// The curve element.
        /// </param>
        /// <returns>
        /// True if the curve element should be exported, false otherwise.
        /// </returns>
        private static bool ShouldCurveElementBeExported(CurveElement curveElement)
        {
            CurveElementType curveElementType = curveElement.CurveElementType;
            bool exported = false;
            if (curveElementType == CurveElementType.ModelCurve || curveElementType == CurveElementType.CurveByPoints)
                exported = true;

            if (exported)
            {
                // Confirm curve is not used by another element
                exported = !ExporterIFCUtils.IsCurveFromOtherElementSketch(curveElement);

                // Confirm the geometry curve is valid.
                Curve curve = curveElement.GeometryCurve;

                if (curve == null)
                    exported = false;
                else if (curve is Line)
                {
                    if (!curve.IsBound)
                        exported = false;
                    else
                    {
                        XYZ end1 = curve.get_EndPoint(0);
                        XYZ end2 = curve.get_EndPoint(1);
                        if (end1.IsAlmostEqualTo(end2))
                            exported = false;
                    }
                }
            }

            return exported;
        }

        /// <summary>
        /// Exports a curve element to IFC curve annotation.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="curveElement">
        /// The curve element to be exported.
        /// </param>
        /// <param name="geometryElement">
        /// The geometry element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void ExportCurveElement(ExporterIFC exporterIFC, CurveElement curveElement, GeometryElement geometryElement,
                                               IFCProductWrapper productWrapper)
        {
            if (geometryElement == null || !ShouldCurveElementBeExported(curveElement))
                return;

            SketchPlane sketchPlane = curveElement.SketchPlane;
            if (sketchPlane == null)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction transaction = new IFCTransaction(file))
            {
                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, curveElement))
                {
                    IFCAnyHandle localPlacement = setter.GetPlacement();
                    IFCAnyHandle axisPlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);

                    Plane planeSK = sketchPlane.Plane;
                    XYZ projDir = planeSK.Normal;
                    XYZ origin = planeSK.Origin;
                    bool useOffsetTrf = false;
                    if (projDir.IsAlmostEqualTo(XYZ.BasisZ))
                    {
                        XYZ offset = XYZ.BasisZ * setter.Offset;
                        origin -= offset;
                    }
                    else
                        useOffsetTrf = true;

                    Plane plane = new Plane(planeSK.XVec, planeSK.YVec, origin);
                    IFCGeometryInfo info = IFCGeometryInfo.CreateCurveGeometryInfo(exporterIFC, plane, projDir, false);

                    if (useOffsetTrf)
                    {
                        XYZ offsetOrig = -XYZ.BasisZ * setter.Offset;
                        Transform trf = Transform.get_Translation(offsetOrig);
                        ExporterIFCUtils.CollectGeometryInfo(exporterIFC, info, geometryElement, XYZ.Zero, false, trf);
                    }
                    else
                    {
                        ExporterIFCUtils.CollectGeometryInfo(exporterIFC, info, geometryElement, XYZ.Zero, false);
                    }

                    IList<IFCAnyHandle> curves = info.GetCurves();

                    if (curves.Count != 1)
                    {
                        throw new Exception("IFC: expected 1 curve when export curve element.");
                    }

                    HashSet<IFCAnyHandle> curveSet = new HashSet<IFCAnyHandle>(curves);
                    IFCAnyHandle repItemHnd = IFCInstanceExporter.CreateGeometricCurveSet(file, curveSet);

                    IFCAnyHandle curveStyle = file.CreateStyle(exporterIFC, repItemHnd);

                    CurveAnnotationCache annotationCache = ExporterCacheManager.CurveAnnotationCache;
                    IFCAnyHandle curveAnno = annotationCache.GetAnnotation(sketchPlane.Id, curveStyle);
                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(curveAnno))
                    {
                        AddCurvesToAnnotation(curveAnno, curves);
                    }
                    else
                    {
                        curveAnno = CreateCurveAnnotation(exporterIFC, curveElement, curveElement.Category.Id, sketchPlane.Id, plane, curveStyle, setter, localPlacement, repItemHnd);
                        productWrapper.AddAnnotation(curveAnno, setter.GetLevelInfo(), true);

                        annotationCache.AddAnnotation(sketchPlane.Id, curveStyle, curveAnno);
                    }
                }
                transaction.Commit();
            }
        }

        /// <summary>
        ///  Creates a new IfcAnnotation object.
        /// </summary>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="curveElement">The curve element.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="sketchPlaneId">The sketch plane id.</param>
        /// <param name="refPlane">The reference plane.</param>
        /// <param name="curveStyle">The curve style.</param>
        /// <param name="placementSetter">The placemenet setter.</param>
        /// <param name="localPlacement">The local placement.</param>
        /// <param name="repItemHnd">The representation item.</param>
        /// <returns>The handle.</returns>
        static IFCAnyHandle CreateCurveAnnotation(ExporterIFC exporterIFC, Element curveElement, ElementId categoryId, ElementId sketchPlaneId,
            Plane refPlane, IFCAnyHandle curveStyle, IFCPlacementSetter placementSetter, IFCAnyHandle localPlacement, IFCAnyHandle repItemHnd)
        {
            HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
            bodyItems.Add(repItemHnd);
            IFCAnyHandle bodyRepHnd = RepresentationUtil.CreateAnnotationSetRep(exporterIFC, curveElement, categoryId, exporterIFC.Get2DContextHandle(), bodyItems);

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRepHnd))
                throw new Exception("Failed to create shape representation.");

            List<IFCAnyHandle> shapes = new List<IFCAnyHandle>();
            shapes.Add(bodyRepHnd);

            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle prodShapeHnd = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapes);

            XYZ xDir = refPlane.XVec; XYZ zDir = refPlane.Normal; XYZ origin = refPlane.Origin;

            // subtract out level origin if we didn't already before.
            IFCLevelInfo levelInfo = placementSetter.GetLevelInfo();
            if (levelInfo != null && !MathUtil.IsAlmostEqual(zDir.Z, 1.0))
            {
                zDir -= new XYZ(0, 0, levelInfo.Elevation);
            }

            origin = origin * exporterIFC.LinearScale;
            IFCAnyHandle relativePlacement = ExporterUtil.CreateAxis(file, origin, zDir, xDir);
            GeometryUtil.SetRelativePlacement(localPlacement, relativePlacement);

            IFCAnyHandle annotation = IFCInstanceExporter.CreateAnnotation(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                null, null, null, localPlacement, prodShapeHnd);

            return annotation;
        }
        
        /// <summary>
        ///  Adds IfcCurve handles to the IfcAnnotation handle.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="curves">The curves.</param>
        static void AddCurvesToAnnotation(IFCAnyHandle annotation, IList<IFCAnyHandle> curves)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(annotation, false, IFCEntityType.IfcAnnotation);

            IFCAnyHandle prodShapeHnd = IFCAnyHandleUtil.GetRepresentation(annotation);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(prodShapeHnd))
                throw new InvalidOperationException("Couldn't find IfcAnnotation.");

            List<IFCAnyHandle> repList = IFCAnyHandleUtil.GetRepresentations(prodShapeHnd);
            if (repList.Count != 1)
                throw new InvalidOperationException("Invalid repList for IfcAnnotation.");

            HashSet<IFCAnyHandle> repItemSet = IFCAnyHandleUtil.GetAggregateInstanceAttribute<HashSet<IFCAnyHandle>>(repList[0], "Items");
            if (repItemSet.Count != 1)
                throw new InvalidOperationException("Invalid repItemSet for IfcAnnotation.");

            IFCAnyHandle repItemHnd = repItemSet.ElementAt(0);
            if (!IFCAnyHandleUtil.IsSubTypeOf(repItemHnd, IFCEntityType.IfcGeometricSet))
                throw new InvalidOperationException("Expected GeometricSet for IfcAnnotation.");

            HashSet<IFCAnyHandle> newElements = IFCAnyHandleUtil.GetAggregateInstanceAttribute<HashSet<IFCAnyHandle>>(repItemHnd, "Elements");
            newElements.Add(curves[0]);
            IFCAnyHandleUtil.SetAttribute(repItemHnd, "Elements", newElements);
        }
    }
}
