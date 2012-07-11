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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Exporter.PropertySet;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export ramps
    /// </summary>
    class RampExporter
    {
        /// <summary>
        /// Checks if exporting an element of Ramp category.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if element is of category OST_Ramps.</returns>
        static public bool IsRamp(Element element)
        {
            // FaceWall should be exported as IfcWall.
            return (CategoryUtil.GetSafeCategoryId(element) == new ElementId(BuiltInCategory.OST_Ramps));
        }

        static private double GetDefaultHeightForRamp(double scale)
        {
            // The default height for ramps is 3'.
            return (3.0 * scale);
        }

        /// <summary>
        /// Gets the ramp height for a ramp.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The unscaled height.
        /// </returns>
        static public double GetRampHeight(ExporterIFC exporterIFC, Element element)
        {
            // Re-use the code for stairs height for legacy stairs.
            return StairsExporter.GetStairsHeightForLegacyStair(exporterIFC, element, GetDefaultHeightForRamp(exporterIFC.LinearScale));
        }

        /// <summary>
        /// Gets the number of flights of a multi-story ramp.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The number of flights (at least 1.)
        /// </returns>
        static public int GetNumFlightsForRamp(ExporterIFC exporterIFC, Element element)
        {
            return StairsExporter.GetNumFlightsForLegacyStair(exporterIFC, element, GetDefaultHeightForRamp(exporterIFC.LinearScale));
        }

        /// <summary>
        /// Gets IFCRampType from ramp type name.
        /// </summary>
        /// <param name="rampTypeName">The ramp type name.</param>
        /// <returns>The IFCRampType.</returns>
        public static IFCRampType GetIFCRampType(string rampTypeName)
        {
            string typeName = rampTypeName.Replace(" ", "").Replace("_", "");

            if (String.Compare(typeName, "StraightRun", true) == 0 ||
                String.Compare(typeName, "StraightRunRamp", true) == 0)
                return Toolkit.IFCRampType.Straight_Run_Ramp;
            if (String.Compare(typeName, "TwoStraightRun", true) == 0 ||
                String.Compare(typeName, "TwoStraightRunRamp", true) == 0)
                return Toolkit.IFCRampType.Two_Straight_Run_Ramp;
            if (String.Compare(typeName, "QuarterTurn", true) == 0 ||
                String.Compare(typeName, "QuarterTurnRamp", true) == 0)
                return Toolkit.IFCRampType.Quarter_Turn_Ramp;
            if (String.Compare(typeName, "TwoQuarterTurn", true) == 0 ||
                String.Compare(typeName, "TwoQuarterTurnRamp", true) == 0)
                return Toolkit.IFCRampType.Two_Quarter_Turn_Ramp;
            if (String.Compare(typeName, "HalfTurn", true) == 0 ||
                String.Compare(typeName, "HalfTurnRamp", true) == 0)
                return Toolkit.IFCRampType.Half_Turn_Ramp;
            if (String.Compare(typeName, "Spiral", true) == 0 ||
                String.Compare(typeName, "SpiralRamp", true) == 0)
                return Toolkit.IFCRampType.Spiral_Ramp;
            if (String.Compare(typeName, "UserDefined", true) == 0)
                return Toolkit.IFCRampType.UserDefined;

            return Toolkit.IFCRampType.NotDefined;
        }

        /// <summary>
        /// Exports the top stories of a multistory ramp.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ramp">The ramp element.</param>
        /// <param name="numFlights">The number of flights for a multistory ramp.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="localPlacement">The local placement of the IfcRamp container.</param>
        /// <param name="containedRampLocalPlacement">The local placement of the IfcRamp containing the ramp geometry.</param>
        /// <param name="representation">The ramp geometry representation.</param>
        /// <param name="rampName">The ramp name.</param>
        /// <param name="rampObjectType">The ramp object type.</param>
        /// <param name="rampDescription">The ramp description.</param>
        /// <param name="elementTag">The ramp element tag.</param>
        /// <param name="rampType">The ramp type.</param>
        /// <param name="ecData">The extrusion creation data.</param>
        /// <param name="placementSetter">The placement setter.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportMultistoryRamp(ExporterIFC exporterIFC, Element ramp, int numFlights,
            IFCAnyHandle ownerHistory, IFCAnyHandle localPlacement, IFCAnyHandle containedRampLocalPlacement, IFCAnyHandle representation,
            string rampName, string rampObjectType, string rampDescription, string elementTag, IFCRampType rampType,
            IFCExtrusionCreationData ecData, IFCPlacementSetter placementSetter, IFCProductWrapper productWrapper)
        {
            if (numFlights < 2)
                return;

            IFCFile file = exporterIFC.GetFile();

            IFCAnyHandle relPlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);
            IFCAnyHandle ptHnd = IFCAnyHandleUtil.GetLocation(relPlacement);
            IList<double> origCoords = IFCAnyHandleUtil.GetCoordinates(ptHnd);

            double heightNonScaled = GetRampHeight(exporterIFC, ramp);
            double scale = exporterIFC.LinearScale;

            for (int ii = 1; ii < numFlights; ii++)
            {
                double newOffsetScaled = 0.0;
                IFCAnyHandle newLevelHnd = null;
                IFCLevelInfo currLevelInfo =
                    placementSetter.GetOffsetLevelInfoAndHandle(heightNonScaled * ii, scale, out newLevelHnd, out newOffsetScaled);
                if (currLevelInfo == null)
                    currLevelInfo = placementSetter.GetLevelInfo();

                XYZ orig = new XYZ(0.0, 0.0, newOffsetScaled);
                IFCAnyHandle relativePlacementHnd = ExporterUtil.CreateAxis2Placement3D(file, orig);
                IFCAnyHandle containedLocalPlacementCopy = IFCInstanceExporter.CreateLocalPlacement(file, newLevelHnd, relativePlacementHnd);

                if (ptHnd.HasValue)
                {
                    IFCAnyHandle relPlacementCopy = GeometryUtil.GetRelativePlacementFromLocalPlacement(containedLocalPlacementCopy);
                    IFCAnyHandle newPt = IFCAnyHandleUtil.GetLocation(relPlacement);

                    IList<double> newCoords = new List<double>();
                    newCoords.Add(origCoords[0]);
                    newCoords.Add(origCoords[1]);
                    newCoords.Add(origCoords[2]);
                    if (newPt.HasValue)
                    {
                        IList<double> addToCoords = IFCAnyHandleUtil.GetCoordinates(newPt);
                        newCoords[0] += addToCoords[0];
                        newCoords[1] += addToCoords[1];
                        newCoords[2] = addToCoords[2];
                    }

                    IFCAnyHandle locPt = IFCInstanceExporter.CreateCartesianPoint(file, newCoords);
                    IFCAnyHandleUtil.SetAttribute(relPlacementCopy, "Location", locPt);
                }

                ElementId catId = CategoryUtil.GetSafeCategoryId(ramp);
                IFCAnyHandle representationCopy = ExporterUtil.CopyProductDefinitionShape(exporterIFC, ramp, catId, representation);

                string localRampName = rampName + ":" + (ii + 1);

                List<IFCAnyHandle> components = new List<IFCAnyHandle>();
                IFCAnyHandle containedRampCopyHnd = IFCInstanceExporter.CreateRamp(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                    localRampName, rampDescription, rampObjectType, containedLocalPlacementCopy, representationCopy, elementTag, rampType);
                components.Add(containedRampCopyHnd);

                productWrapper.AddElement(containedRampCopyHnd, currLevelInfo, ecData, false);

                IFCAnyHandle rampLocalPlacementCopy = ExporterUtil.CopyLocalPlacement(file, containedRampLocalPlacement);

                IFCAnyHandle rampCopyHnd = IFCInstanceExporter.CreateRamp(file, ExporterIFCUtils.CreateGUID(), ownerHistory, localRampName,
                    rampDescription, rampObjectType, rampLocalPlacementCopy, null, elementTag, rampType);

                productWrapper.AddElement(rampCopyHnd, currLevelInfo, ecData, LevelUtil.AssociateElementToLevel(ramp));

                StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(rampCopyHnd, components, rampLocalPlacementCopy);
                ExporterCacheManager.StairRampContainerInfoCache.AppendStairRampContainerInfo(ramp.Id, stairRampInfo);
            }
        }

        /// <summary>
        /// Exports a ramp to IfcRamp, without decomposing into separate runs and landings.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ifcEnumType">The ramp type.</param>
        /// <param name="ramp">The ramp element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="numFlights">The number of flights for a multistory ramp.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportRamp(ExporterIFC exporterIFC, string ifcEnumType, Element ramp, GeometryElement geometryElement,
            int numFlights, IFCProductWrapper productWrapper)
        {
            if (ramp == null || geometryElement == null)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, ramp))
                {
                    using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                    {
                        ecData.SetLocalPlacement(placementSetter.GetPlacement());

                        ElementId categoryId = CategoryUtil.GetSafeCategoryId(ramp);
                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions();
                        IFCAnyHandle representation = RepresentationUtil.CreateBRepProductDefinitionShape(ramp.Document.Application,
                            exporterIFC, ramp, categoryId, geometryElement, bodyExporterOptions, null, ecData);

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(representation))
                        {
                            ecData.ClearOpenings();
                            return;
                        }

                        string containedRampGuid = ExporterIFCUtils.CreateSubElementGUID(ramp, (int)IFCRampSubElements.ContainedRamp);
                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        string origRampName = exporterIFC.GetName();
                        string rampName = NamingUtil.GetNameOverride(ramp, origRampName);
                        string rampDescription = NamingUtil.GetDescriptionOverride(ramp, null);
                        string rampObjectType = NamingUtil.GetObjectTypeOverride(ramp, NamingUtil.CreateIFCObjectName(exporterIFC, ramp));
                        IFCAnyHandle containedRampLocalPlacement = ExporterUtil.CopyLocalPlacement(file, ecData.GetLocalPlacement());
                        string elementTag = NamingUtil.CreateIFCElementId(ramp);
                        IFCRampType rampType = GetIFCRampType(ifcEnumType);

                        List<IFCAnyHandle> components = new List<IFCAnyHandle>();
                        IFCAnyHandle containedRampHnd = IFCInstanceExporter.CreateRamp(file, containedRampGuid, ownerHistory, rampName,
                            rampDescription, rampObjectType, containedRampLocalPlacement, representation, elementTag, rampType);
                        components.Add(containedRampHnd);
                        productWrapper.AddElement(containedRampHnd, placementSetter.GetLevelInfo(), ecData, false);

                        string guid = ExporterIFCUtils.CreateGUID(ramp);
                        IFCAnyHandle localPlacement = ecData.GetLocalPlacement();

                        IFCAnyHandle rampHnd = IFCInstanceExporter.CreateRamp(file, guid, ownerHistory, rampName,
                            rampDescription, rampObjectType, localPlacement, null, elementTag, rampType);

                        productWrapper.AddElement(rampHnd, placementSetter.GetLevelInfo(), ecData, LevelUtil.AssociateElementToLevel(ramp));

                        StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(rampHnd, components, null);
                        ExporterCacheManager.StairRampContainerInfoCache.AddStairRampContainerInfo(ramp.Id, stairRampInfo);

                        ExportMultistoryRamp(exporterIFC, ramp, numFlights,
                            ownerHistory, localPlacement, containedRampLocalPlacement, representation,
                            rampName, rampObjectType, rampDescription, elementTag, rampType,
                            ecData, placementSetter, productWrapper);
                    }
                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, ramp, productWrapper);
                    tr.Commit();
                }
            }
        }
        
        /// <summary>
        /// Exports a ramp to IfcRamp.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The ramp element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void Export(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, element);
            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                if (!(element is FamilyInstance))
                {
                    StairsExporter.ExportLegacyStairOrRampAsContainer(exporterIFC, ifcEnumType, element, geometryElement, true, productWrapper);
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(productWrapper.GetAnElement()))
                    {
                        int numFlights = GetNumFlightsForRamp(exporterIFC, element);
                        if (numFlights > 0)
                            ExportRamp(exporterIFC, ifcEnumType, element, geometryElement, numFlights, productWrapper);
                    }
                    else
                        PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                }
                else
                {
                    ExportRamp(exporterIFC, ifcEnumType, element, geometryElement, 1, productWrapper);
                }

                tr.Commit();
            }
        }
    }
}
