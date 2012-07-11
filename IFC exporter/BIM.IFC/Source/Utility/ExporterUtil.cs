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
using BIM.IFC.Toolkit;

namespace BIM.IFC.Utility
{
    /// <summary>
    /// Provides general utility methods for IFC export.
    /// </summary>
    class ExporterUtil
    {
        /// <summary>
        /// Determines if the Exception is local to the element, or if export should be aborted.
        /// </summary>
        /// <param name="ex">The unexpected exception.</param>
        public static bool IsFatalException(Document document, Exception exception)
        {
            string msg = exception.ToString();
            if (msg.Contains("Error in allocating memory"))
            {
                FailureMessage fm = new FailureMessage(BuiltInFailures.ExportFailures.IFCFatalToolkitExportError);
                document.PostFailure(fm); 
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the Revit program path.
        /// </summary>
        public static string RevitProgramPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(typeof(Autodesk.Revit.ApplicationServices.Application).Assembly.Location);
            }
        }

        /// <summary>
        /// Checks if using IFCBCA - Building Code Authority code checking.
        /// </summary>
        /// <param name="exportOptionsCache">The export options cache.</param>
        /// <returns>True if it is, false otherwise.</returns>
        public static bool DoCodeChecking(ExportOptionsCache exportOptionsCache)
        {
            switch (exportOptionsCache.FileVersion)
            {
                case IFCVersion.IFC2x2:
                    {
                        return exportOptionsCache.WallAndColumnSplitting;
                    }
                case IFCVersion.IFCBCA:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Relates one object to another. 
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="relatingObject">
        /// The relating object.
        /// </param>
        /// <param name="relatedObject">
        /// The related object.
        /// </param>
        public static void RelateObject(ExporterIFC exporterIFC, IFCAnyHandle relatingObject, IFCAnyHandle relatedObject)
        {
            HashSet<IFCAnyHandle> relatedObjects = new HashSet<IFCAnyHandle>();
            relatedObjects.Add(relatedObject);
            RelateObjects(exporterIFC, null, relatingObject, relatedObjects);
        }

        /// <summary>
        /// Relates one object to a collection of others. 
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="optionalGUID">
        /// A GUID value, or null to generate a random GUID.
        /// </param>
        /// <param name="relatingObject">
        /// The relating object.
        /// </param>
        /// <param name="relatedObjects">
        /// The related objects.
        /// </param>
        public static void RelateObjects(ExporterIFC exporterIFC, string optionalGUID, IFCAnyHandle relatingObject, ICollection<IFCAnyHandle> relatedObjects)
        {
            string guid = (optionalGUID != null) ? optionalGUID : ExporterIFCUtils.CreateGUID();
            IFCInstanceExporter.CreateRelAggregates(exporterIFC.GetFile(), guid, exporterIFC.GetOwnerHistoryHandle(), null, null, relatingObject, new HashSet<IFCAnyHandle>(relatedObjects));
        }

        /// <summary>
        /// Creates IfcAxis2Placement3D object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="origin">
        /// The origin.
        /// </param>
        /// <param name="zDirection">
        /// The Z direction.
        /// </param>
        /// <param name="xDirection">
        /// The X direction.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateAxis(IFCFile file, XYZ origin, XYZ zDirection, XYZ xDirection)
        {
            IFCAnyHandle direction = null;
            IFCAnyHandle refDirection = null;
            IFCAnyHandle location = null;

            if (origin != null)
            {
                IList<double> measure = new List<double>();
                measure.Add(origin.X); measure.Add(origin.Y); measure.Add(origin.Z);
                location = CreateCartesianPoint(file, measure);
            }
            else
            {
                location = ExporterIFCUtils.GetGlobal3DOriginHandle();
            }

            bool exportzDirectionAndxDirection = (zDirection != null && xDirection != null && (!MathUtil.IsAlmostEqual(zDirection[2], 1.0) || !MathUtil.IsAlmostEqual(xDirection[0], 1.0)));

            if (exportzDirectionAndxDirection)
            {
                IList<double> axisPts = new List<double>();
                axisPts.Add(zDirection.X); axisPts.Add(zDirection.Y); axisPts.Add(zDirection.Z);
                direction = CreateDirection(file, axisPts);
            }

            if (exportzDirectionAndxDirection)
            {
                IList<double> axisPts = new List<double>();
                axisPts.Add(xDirection.X); axisPts.Add(xDirection.Y); axisPts.Add(xDirection.Z);
                refDirection = CreateDirection(file, axisPts);
            }

            return IFCInstanceExporter.CreateAxis2Placement3D(file, location, direction, refDirection);
        }

        /// <summary>
        /// Creates IfcDirection object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="realList">
        /// The list of doubles to create the direction.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateDirection(IFCFile file, IList<double> realList)
        {
            IList<double> cleanList = new List<double>();

            foreach (double measure in realList)
            {
                double ceilMeasure = Math.Ceiling(measure);
                double floorMeasure = Math.Floor(measure);

                if (MathUtil.IsAlmostEqual(measure, ceilMeasure))
                    cleanList.Add(ceilMeasure);
                else if (MathUtil.IsAlmostEqual(measure, floorMeasure))
                    cleanList.Add(floorMeasure);
                else
                    cleanList.Add(measure);
            }

            int sz = realList.Count;

            if (sz == 3)
            {
                for (int ii = 0; ii < 3; ii++)
                {
                    if (MathUtil.IsAlmostEqual(cleanList[ii], 1.0))
                    {
                        if (!MathUtil.IsAlmostZero(cleanList[(ii + 1) % 3]) || !MathUtil.IsAlmostZero(cleanList[(ii + 2) % 3]))
                            break;
                        return ExporterIFCUtils.GetGlobal3DDirectionHandles(true)[ii];
                    }
                    else if (MathUtil.IsAlmostEqual(cleanList[ii], -1.0))
                    {
                        if (!MathUtil.IsAlmostZero(cleanList[(ii + 1) % 3]) || !MathUtil.IsAlmostZero(cleanList[(ii + 2) % 3]))
                            break;
                        return ExporterIFCUtils.GetGlobal3DDirectionHandles(false)[ii];
                    }
                }
            }
            else if (sz == 2)
            {
                for (int ii = 0; ii < 2; ii++)
                {
                    if (MathUtil.IsAlmostEqual(cleanList[ii], 1.0))
                    {
                        if (!MathUtil.IsAlmostZero(cleanList[1 - ii]))
                            break;
                        return ExporterIFCUtils.GetGlobal2DDirectionHandles(true)[ii];
                    }
                    else if (MathUtil.IsAlmostEqual(cleanList[ii], -1.0))
                    {
                        if (!MathUtil.IsAlmostZero(cleanList[1 - ii]))
                            break;
                        return ExporterIFCUtils.GetGlobal2DDirectionHandles(false)[ii];
                    }
                }
            }

            IFCAnyHandle directionHandle = IFCInstanceExporter.CreateDirection(file, cleanList);
            return directionHandle;
        }

        /// <summary>
        /// Creates IfcDirection object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="direction">
        /// The direction.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateDirection(IFCFile file, XYZ direction)
        {
            IList<double> measure = new List<double>();
            measure.Add(direction.X);
            measure.Add(direction.Y);
            measure.Add(direction.Z);
            return CreateDirection(file, measure);
        }

        /// <summary>
        /// Creates IfcCartesianPoint object from a 2D point.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="point">The point</param>
        /// <returns>The IfcCartesianPoint handle.</returns>
        public static IFCAnyHandle CreateCartesianPoint(IFCFile file, UV point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            List<double> points = new List<double>();
            points.Add(point.U);
            points.Add(point.V);

            return CreateCartesianPoint(file, points);
        }

        /// <summary>
        /// Creates IfcCartesianPoint object from a 3D point.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="point">The point</param>
        /// <returns>The IfcCartesianPoint handle.</returns>
        public static IFCAnyHandle CreateCartesianPoint(IFCFile file, XYZ point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            List<double> points = new List<double>();
            points.Add(point.X);
            points.Add(point.Y);
            points.Add(point.Z);

            return CreateCartesianPoint(file, points);
        }

        /// <summary>
        /// Creates IfcCartesianPoint object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="measure">
        /// The list of doubles to create the Cartesian point.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateCartesianPoint(IFCFile file, IList<double> measure)
        {
            IList<double> cleanMeasure = new List<double>();
            foreach (double value in measure)
            {
                double ceilMeasure = Math.Ceiling(value);
                double floorMeasure = Math.Floor(value);

                if (MathUtil.IsAlmostEqual(value, ceilMeasure))
                    cleanMeasure.Add(ceilMeasure);
                else if (MathUtil.IsAlmostEqual(value, floorMeasure))
                    cleanMeasure.Add(floorMeasure);
                else
                    cleanMeasure.Add(value);
            }

            if (MathUtil.IsAlmostZero(cleanMeasure[0]) && MathUtil.IsAlmostZero(cleanMeasure[1]))
            {
                if (measure.Count == 2)
                {
                    return ExporterIFCUtils.GetGlobal2DOriginHandle();
                }
                if (measure.Count == 3 && MathUtil.IsAlmostZero(cleanMeasure[2]))
                {
                    return ExporterIFCUtils.GetGlobal3DOriginHandle();
                }

            }

            IFCAnyHandle pointHandle = IFCInstanceExporter.CreateCartesianPoint(file, cleanMeasure);

            return pointHandle;
        }

        /// <summary>
        /// Creates an IfcAxis2Placement3D object.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="location">The origin. If null, it will use the global origin handle.</param>
        /// <param name="axis">The Z direction.</param>
        /// <param name="refDirection">The X direction.</param>
        /// <returns>the handle.</returns>
        public static IFCAnyHandle CreateAxis2Placement3D(IFCFile file, XYZ location, XYZ axis, XYZ refDirection)
        {
            IFCAnyHandle locationHandle = null;
            if (location != null)
            {
                List<double> measure = new List<double>();
                measure.Add(location.X);
                measure.Add(location.Y);
                measure.Add(location.Z);
                locationHandle = CreateCartesianPoint(file, measure);
            }
            else
            {
                locationHandle = ExporterIFCUtils.GetGlobal3DOriginHandle();
            }


            bool exportDirAndRef = (axis != null && refDirection != null &&
                (!MathUtil.IsAlmostEqual(axis[2], 1.0) || !MathUtil.IsAlmostEqual(refDirection[0], 1.0)));

            if ((axis != null) ^ (refDirection != null))
            {
                exportDirAndRef = false;
            }

            IFCAnyHandle axisHandle = null;
            if (exportDirAndRef)
            {
                List<double> measure = new List<double>();
                measure.Add(axis.X);
                measure.Add(axis.Y);
                measure.Add(axis.Z);
                axisHandle = CreateDirection(file, measure);
            }

            IFCAnyHandle refDirectionHandle = null;
            if (exportDirAndRef)
            {
                List<double> measure = new List<double>();
                measure.Add(refDirection.X);
                measure.Add(refDirection.Y);
                measure.Add(refDirection.Z);
                refDirectionHandle = CreateDirection(file, measure);
            }

            return IFCInstanceExporter.CreateAxis2Placement3D(file, locationHandle, axisHandle, refDirectionHandle);
        }

        /// <summary>
        /// Creates an IfcAxis2Placement3D object.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="location">The origin.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAxis2Placement3D(IFCFile file, XYZ location)
        {
            return CreateAxis2Placement3D(file, location, null, null);
        }

        /// <summary>
        /// Creates a default IfcAxis2Placement3D object.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAxis2Placement3D(IFCFile file)
        {
            return CreateAxis2Placement3D(file, null);
        }

        /// <summary>
        /// Creates IfcMappedItem object from an origin.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="repMap">
        /// The handle to be mapped.
        /// </param>
        /// <param name="orig">
        /// The orig for mapping transformation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateDefaultMappedItem(IFCFile file, IFCAnyHandle repMap, XYZ orig)
        {
            if (MathUtil.IsAlmostZero(orig.X) && MathUtil.IsAlmostZero(orig.Y) && MathUtil.IsAlmostZero(orig.Z))
                return CreateDefaultMappedItem(file, repMap);

            IFCAnyHandle origin = CreateCartesianPoint(file, orig);
            double scale = 1.0;
            IFCAnyHandle mappingTarget =
               IFCInstanceExporter.CreateCartesianTransformationOperator3D(file, null, null, origin, scale, null);
            return IFCInstanceExporter.CreateMappedItem(file, repMap, mappingTarget);
        }

        /// <summary>
        /// Creates IfcMappedItem object at (0,0,0).
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="repMap">
        /// The handle to be mapped.
        /// </param>
        /// <param name="orig">
        /// The orig for mapping transformation.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateDefaultMappedItem(IFCFile file, IFCAnyHandle repMap)
        {
            IFCAnyHandle transformHnd = ExporterCacheManager.GetDefaultCartesianTransformationOperator3D(file);
            return IFCInstanceExporter.CreateMappedItem(file, repMap, transformHnd);
        }

        /// <summary>
        /// Creates IfcMappedItem object from a transform
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="repMap">
        /// The handle to be mapped.
        /// </param>
        /// <param name="transform">
        /// The transform.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CreateMappedItemFromTransform(IFCFile file, IFCAnyHandle repMap, Transform transform)
        {
            IFCAnyHandle axis1 = CreateDirection(file, transform.BasisX);
            IFCAnyHandle axis2 = CreateDirection(file, transform.BasisY);
            IFCAnyHandle axis3 = CreateDirection(file, transform.BasisZ);
            IFCAnyHandle origin = CreateCartesianPoint(file, transform.Origin);
            double scale = 1.0;
            IFCAnyHandle mappingTarget =
               IFCInstanceExporter.CreateCartesianTransformationOperator3D(file, axis1, axis2, origin, scale, axis3);
            return IFCInstanceExporter.CreateMappedItem(file, repMap, mappingTarget);
        }

        /// <summary>
        /// Creates an IfcPolyLine from a list of UV points.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="polylinePts">This list of UV values.</param>
        /// <returns>An IfcPolyline handle.</returns>
        public static IFCAnyHandle CreatePolyline(IFCFile file, IList<UV> polylinePts)
        {
            int numPoints = polylinePts.Count;
            if (numPoints < 2)
                return null;

            bool closed = MathUtil.IsAlmostEqual(polylinePts[0], polylinePts[numPoints - 1]);
            if (closed)
            {
                if (numPoints == 2)
                    return null;
                numPoints--;
            }

            IList<IFCAnyHandle> points = new List<IFCAnyHandle>();
            for (int ii = 0; ii < numPoints; ii++)
            {
                points.Add(CreateCartesianPoint(file, polylinePts[ii]));
            }
            if (closed)
                points.Add(points[0]);

            return IFCInstanceExporter.CreatePolyline(file, points);
        }

        /// <summary>
        /// Creates a copy of local placement object.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="originalPlacement">
        /// The original placement object to be copied.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CopyLocalPlacement(IFCFile file, IFCAnyHandle originalPlacement)
        {
            IFCAnyHandle placementRelToOpt = GeometryUtil.GetPlacementRelToFromLocalPlacement(originalPlacement);
            IFCAnyHandle relativePlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(originalPlacement);
            return IFCInstanceExporter.CreateLocalPlacement(file, placementRelToOpt, relativePlacement);
        }

        public static IList<IFCAnyHandle> CopyRepresentations(ExporterIFC exporterIFC, Element element, ElementId catId, IFCAnyHandle origProductRepresentation)
        {
            IList<IFCAnyHandle> origReps = IFCAnyHandleUtil.GetRepresentations(origProductRepresentation);
            IList<IFCAnyHandle> newReps = new List<IFCAnyHandle>();
            IFCFile file = exporterIFC.GetFile();

            int num = origReps.Count;
            for (int ii = 0; ii < num; ii++)
            {
                IFCAnyHandle repHnd = origReps[ii];
                if (IFCAnyHandleUtil.IsTypeOf(repHnd, IFCEntityType.IfcShapeRepresentation))
                {
                    IFCAnyHandle newRepHnd = RepresentationUtil.CreateShapeRepresentation(exporterIFC, element, catId, 
                        IFCAnyHandleUtil.GetContextOfItems(repHnd),
                        IFCAnyHandleUtil.GetRepresentationIdentifier(repHnd), IFCAnyHandleUtil.GetRepresentationType(repHnd), 
                        IFCAnyHandleUtil.GetItems(repHnd));
                    newReps.Add(newRepHnd);
                }
                else
                {
                    // May want to throw exception here.
                    newReps.Add(repHnd);
                }
            }

            return newReps;
        }

        /// <summary>
        /// Creates a copy of a product definition shape.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="origProductDefinitionShape">
        /// The original product definition shape to be copied.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public static IFCAnyHandle CopyProductDefinitionShape(ExporterIFC exporterIFC, 
            Element elem,
            ElementId catId,
            IFCAnyHandle origProductDefinitionShape)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(origProductDefinitionShape))
                return null;

            IList<IFCAnyHandle> representations = CopyRepresentations(exporterIFC, elem, catId, origProductDefinitionShape);

            IFCFile file = exporterIFC.GetFile();
            return IFCInstanceExporter.CreateProductDefinitionShape(file, IFCAnyHandleUtil.GetProductDefinitionShapeName(origProductDefinitionShape),
                IFCAnyHandleUtil.GetProductDefinitionShapeDescription(origProductDefinitionShape), representations);
        }


        /// <summary>
        /// Gets export type for an element.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="enumTypeValue">
        /// The output string value represents the enum type.
        /// </param>
        /// <returns>
        /// The IFCExportType.
        /// </returns>
        public static IFCExportType GetExportType(ExporterIFC exporterIFC, Element element,
           out string enumTypeValue)
        {
            enumTypeValue = "";
            IFCExportType exportType = IFCExportType.DontExport;

            // Get potential override value first.
            {
                string symbolClassName;

                string exportAsEntity = "IFCExportAs";
                string exportAsType = "IFCExportType";

                ParameterUtil.GetStringValueFromElementOrSymbol(element, exportAsEntity, out symbolClassName);
                ParameterUtil.GetStringValueFromElementOrSymbol(element, exportAsType, out enumTypeValue);

                if (!String.IsNullOrEmpty(symbolClassName))
                {
                    exportType = ElementFilteringUtil.GetExportTypeFromClassName(symbolClassName);
                    if (exportType != IFCExportType.DontExport)
                        return exportType;
                }
            }

            Category category = element.Category;
            if (category == null)
                return IFCExportType.DontExport;

            ElementId categoryId = category.Id;

            string ifcClassName = ExporterIFCUtils.GetIFCClassName(element, exporterIFC);
            if (ifcClassName != "")
            {
                enumTypeValue = ExporterIFCUtils.GetIFCType(element, exporterIFC);
                // if using name, override category id if match is found.
                if (!ifcClassName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    exportType = ElementFilteringUtil.GetExportTypeFromClassName(ifcClassName);
            }

            // if not set, fall back on category id.
            if (exportType == IFCExportType.DontExport)
            {
                //bool exportSeparately = true;
                exportType = ElementFilteringUtil.GetExportTypeFromCategoryId(categoryId, out enumTypeValue /*, out bool exportSeparately*/);
            }

            // if not set, fall back on symbol functions.
            // allow override of IfcBuildingElementProxy.
            if ((exportType == IFCExportType.DontExport) || (exportType == IFCExportType.ExportBuildingElementProxy))
            {
                // TODO: add isColumn.
                //if (familySymbol.IsColumn())
                //exportType = IFCExportType.ExportColumnType;
                //else 
                FamilyInstance familyInstance = element as FamilyInstance;
                if (familyInstance != null)
                {
                    switch (familyInstance.StructuralType)
                    {
                        case Autodesk.Revit.DB.Structure.StructuralType.Beam:
                            exportType = IFCExportType.ExportBeam;
                            break;
                        case Autodesk.Revit.DB.Structure.StructuralType.Footing:
                            exportType = IFCExportType.ExportFooting;
                            break;
                    }
                }
            }

            return exportType;
        }
    }
}
