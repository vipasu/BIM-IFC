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
    /// Provides methods to export stairs
    /// </summary>
    class StairsExporter
    {
        /// <summary>
        /// The IfcMemberType shared by all stringers to keep their type.  This is a placeholder IfcMemberType.
        /// </summary>
        public static IFCAnyHandle GetMemberTypeHandle(ExporterIFC exporterIFC, Element stringer)
        {
            Element stringerType = stringer.Document.GetElement(stringer.GetTypeId());
            IFCAnyHandle memberType = ExporterCacheManager.ElementToHandleCache.Find(stringerType.Id);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(memberType))
            {
                IFCFile file = exporterIFC.GetFile();
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

                string stringerTypeGUID = ExporterIFCUtils.CreateGUID(stringerType);
                string origInstanceName = stringerType.Name;
                string stringerTypeName = NamingUtil.GetNameOverride(stringerType, origInstanceName);
                string stringerTypeDescription = NamingUtil.GetDescriptionOverride(stringerType, null);
                string stringerTypeElemId = NamingUtil.CreateIFCElementId(stringerType);

                memberType = IFCInstanceExporter.CreateMemberType(file, ExporterIFCUtils.CreateGUID(),
                    ownerHistory, stringerTypeName, stringerTypeDescription, null, null, null, stringerTypeElemId,
                    null, IFCMemberType.Stringer);
                ExporterCacheManager.ElementToHandleCache.Register(stringerType.Id, memberType);
            }
            return memberType;
        }


        /// <summary>
        /// Determines if an element is a legacy (created in R2012 or before) Stairs element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// Returns true if the element is a legacy (created in R2012 or before) Stairs element, false otherwise.
        /// </returns>
        static public bool IsLegacyStairs(Element element)
        {
            if (CategoryUtil.GetSafeCategoryId(element) != new ElementId(BuiltInCategory.OST_Stairs))
                return false;

            return !(element is Stairs) && !(element is FamilyInstance);
        }

        static private double GetDefaultHeightForLegacyStair(double scale)
        {
            // The default height for legacy stairs are either 12' or 3.5m.  Figure it out based on the scale of the export, and convert to feet.
            return
                (MathUtil.IsAlmostEqual(scale, 1.0) || MathUtil.IsAlmostEqual(scale, 1.0 / 12.0)) ? (12.0 * scale) : (3.5 * (100 / (12 * 2.54)) * scale);
        }

        /// <summary>
        /// Gets the stairs height for a legacy (R2012 or before) stairs.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="defaultHeight">
        /// The default height of the stair.
        /// </param>
        /// <returns>
        /// The unscaled height.
        /// </returns>
        static public double GetStairsHeightForLegacyStair(ExporterIFC exporterIFC, Element element, double defaultHeight)
        {
            Parameter bottomLevelParam = element.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM);
            if (bottomLevelParam == null || (bottomLevelParam.AsElementId() == ElementId.InvalidElementId))
                return 0.0;
            Level bottomLevel = element.Document.GetElement(bottomLevelParam.AsElementId()) as Level;
            if (bottomLevel == null)
                return 0.0;
            double bottomLevelElev = bottomLevel.Elevation;

            Level topLevel = null;
            Parameter topLevelParam = element.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM);
            if (topLevelParam != null && (topLevelParam.AsElementId() != ElementId.InvalidElementId))
                topLevel = element.Document.GetElement(topLevelParam.AsElementId()) as Level;

            Parameter bottomLevelOffsetParam = element.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET);
            double bottomLevelOffset = (bottomLevelOffsetParam == null) ? 0.0 : bottomLevelOffsetParam.AsDouble();

            Parameter topLevelOffsetParam = element.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET);
            double topLevelOffset = (topLevelOffsetParam == null) ? 0.0 : topLevelOffsetParam.AsDouble();

            double minHeight = bottomLevelElev + bottomLevelOffset;
            double maxHeight = (topLevel != null) ? topLevel.Elevation + topLevelOffset : minHeight + defaultHeight;

            double stairsHeight = maxHeight - minHeight;
            return stairsHeight;
        }

        /// <summary>
        /// Gets the number of flights of a multi-story staircase for a legacy (R2012 or before) stairs.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="defaultHeight">
        /// The default height.
        /// </param>
        /// <returns>
        /// The number of flights (at least 1.)
        /// </returns>
        static public int GetNumFlightsForLegacyStair(ExporterIFC exporterIFC, Element element, double defaultHeight)
        {
            Parameter multiStoryTopLevelParam = element.get_Parameter(BuiltInParameter.STAIRS_MULTISTORY_TOP_LEVEL_PARAM);
            if (multiStoryTopLevelParam == null || (multiStoryTopLevelParam.AsElementId() == ElementId.InvalidElementId))
                return 1;

            Parameter bottomLevelParam = element.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM);
            if (bottomLevelParam == null || (bottomLevelParam.AsElementId() == ElementId.InvalidElementId))
                return 1;
            Level bottomLevel = element.Document.GetElement(bottomLevelParam.AsElementId()) as Level;
            if (bottomLevel == null)
                return 1;
            double bottomLevelElev = bottomLevel.Elevation;

            ElementId multistoryTopLevelId = multiStoryTopLevelParam.AsElementId();
            Level multistoryTopLevel = element.Document.GetElement(multistoryTopLevelId) as Level;
            double multistoryLevelElev = multistoryTopLevel.Elevation;

            Level topLevel = null;
            Parameter topLevelParam = element.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM);
            if (topLevelParam != null && (topLevelParam.AsElementId() != ElementId.InvalidElementId))
                topLevel = element.Document.GetElement(topLevelParam.AsElementId()) as Level;

            Parameter bottomLevelOffsetParam = element.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET);
            double bottomLevelOffset = (bottomLevelOffsetParam == null) ? 0.0 : bottomLevelParam.AsDouble();

            Parameter topLevelOffsetParam = element.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET);
            double topLevelOffset = (topLevelOffsetParam == null) ? 0.0 : topLevelParam.AsDouble();

            double scale = exporterIFC.LinearScale;

            double minHeight = bottomLevelElev + bottomLevelOffset;
            double maxHeight = (topLevel != null) ? topLevel.Elevation + topLevelOffset : minHeight + defaultHeight;
            double unconnectedHeight = maxHeight;

            double stairsHeight = GetStairsHeightForLegacyStair(exporterIFC, element, defaultHeight);

            double topElev = (topLevel != null) ? topLevel.Elevation : unconnectedHeight;

            if ((topElev + MathUtil.Eps() > multistoryLevelElev) || (bottomLevelElev + MathUtil.Eps() > multistoryLevelElev))
                return 1;

            double multistoryHeight = multistoryLevelElev - bottomLevelElev;
            double oneStairHeight = stairsHeight;
            double currentHeight = oneStairHeight;

            if (oneStairHeight < MathUtil.Eps())
                return 1;

            int flightNumber = 0;
            for (; currentHeight < multistoryHeight + MathUtil.Eps() * flightNumber;
                currentHeight += oneStairHeight, flightNumber++)
            {
                // Fail if we reach some arbitrarily huge number.
                if (flightNumber > 100000)
                    return 1;
            }

            return (flightNumber > 0) ? flightNumber : 1;
        }

        static private double GetStairsHeight(ExporterIFC exporterIFC, Element stair)
        {
            if (IsLegacyStairs(stair))
            {
                // The default height for legacy stairs are either 12' or 3.5m.  Figure it out based on the scale of the export, and convert to feet.
                double defaultHeight = GetDefaultHeightForLegacyStair(exporterIFC.LinearScale);
                return GetStairsHeightForLegacyStair(exporterIFC, stair, defaultHeight);
            }

            if (stair is Stairs)
            {
                return (stair as Stairs).Height;
            }

            return 0.0;
        }

        /// <summary>
        /// Gets IFCStairType from stair type name.
        /// </summary>
        /// <param name="stairTypeName">The stair type name.</param>
        /// <returns>The IFCStairType.</returns>
        public static IFCStairType GetIFCStairType(string stairTypeName)
        {
            string typeName = stairTypeName.Replace(" ", "").Replace("_", "");

            if (String.Compare(typeName, "StraightRun", true) == 0 ||
                String.Compare(typeName, "StraightRunStair", true) == 0)
                return Toolkit.IFCStairType.Straight_Run_Stair;
            if (String.Compare(typeName, "QuarterWinding", true) == 0 ||
                String.Compare(typeName, "QuarterWindingStair", true) == 0)
                return Toolkit.IFCStairType.Quarter_Winding_Stair;
            if (String.Compare(typeName, "QuarterTurn", true) == 0 ||
                String.Compare(typeName, "QuarterTurnStair", true) == 0)
                return Toolkit.IFCStairType.Quarter_Turn_Stair;
            if (String.Compare(typeName, "HalfWinding", true) == 0 ||
                String.Compare(typeName, "HalfWindingStair", true) == 0)
                return Toolkit.IFCStairType.Half_Winding_Stair;
            if (String.Compare(typeName, "HalfTurn", true) == 0 ||
                String.Compare(typeName, "HalfTurnStair", true) == 0)
                return Toolkit.IFCStairType.Half_Turn_Stair;
            if (String.Compare(typeName, "TwoQuarterWinding", true) == 0 ||
                String.Compare(typeName, "TwoQuarterWindingStair", true) == 0)
                return Toolkit.IFCStairType.Two_Quarter_Winding_Stair;
            if (String.Compare(typeName, "TwoStraightRun", true) == 0 ||
                String.Compare(typeName, "TwoStraightRunStair", true) == 0)
                return Toolkit.IFCStairType.Two_Straight_Run_Stair;
            if (String.Compare(typeName, "TwoQuarterTurn", true) == 0 ||
                String.Compare(typeName, "TwoQuarterTurnStair", true) == 0)
                return Toolkit.IFCStairType.Two_Quarter_Turn_Stair;
            if (String.Compare(typeName, "ThreeQuarterWinding", true) == 0 ||
                String.Compare(typeName, "ThreeQuarterWindingStair", true) == 0)
                return Toolkit.IFCStairType.Three_Quarter_Winding_Stair;
            if (String.Compare(typeName, "ThreeQuarterTurn", true) == 0 ||
                String.Compare(typeName, "ThreeQuarterTurnStair", true) == 0)
                return Toolkit.IFCStairType.Three_Quarter_Turn_Stair;
            if (String.Compare(typeName, "Spiral", true) == 0 ||
                String.Compare(typeName, "SpiralStair", true) == 0)
                return Toolkit.IFCStairType.Spiral_Stair;
            if (String.Compare(typeName, "DoubleReturn", true) == 0 ||
                String.Compare(typeName, "DoubleReturnStair", true) == 0)
                return Toolkit.IFCStairType.Double_Return_Stair;
            if (String.Compare(typeName, "CurvedRun", true) == 0 ||
                String.Compare(typeName, "CurvedRunStair", true) == 0)
                return Toolkit.IFCStairType.Curved_Run_Stair;
            if (String.Compare(typeName, "TwoCurvedRun", true) == 0 ||
                String.Compare(typeName, "TwoCurvedRunStair", true) == 0)
                return Toolkit.IFCStairType.Two_Curved_Run_Stair;
            if (String.Compare(typeName, "UserDefined", true) == 0)
                return Toolkit.IFCStairType.UserDefined;

            return Toolkit.IFCStairType.NotDefined;
        }

        /// <summary>
        /// Exports the top stories of a multistory staircase.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="stair">The stairs element.</param>
        /// <param name="numFlights">The number of flights for a multistory staircase.</param>
        /// <param name="stairHnd">The stairs container handle.</param>
        /// <param name="components">The components handles.</param>
        /// <param name="ecData">The extrusion creation data.</param>
        /// <param name="componentECData">The extrusion creation data for the components.</param>
        /// <param name="materialIds">The materialIds.</param>
        /// <param name="placementSetter">The placement setter.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportMultistoryStair(ExporterIFC exporterIFC, Element stair, int numFlights,
            IFCAnyHandle stairHnd, IList<IFCAnyHandle> components, IList<IFCExtrusionCreationData> componentECData,
            HashSet<ElementId> materialIds, IFCPlacementSetter placementSetter, IFCProductWrapper productWrapper)
        {
            if (numFlights < 2)
                return;

            double heightNonScaled = GetStairsHeight(exporterIFC, stair);
            if (heightNonScaled < MathUtil.Eps())
                return;

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(stairHnd))
                return;

            IFCAnyHandle localPlacement = IFCAnyHandleUtil.GetInstanceAttribute(stairHnd, "ObjectPlacement");
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(localPlacement))
                return;

            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            IFCFile file = exporterIFC.GetFile();

            IFCAnyHandle relPlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);
            IFCAnyHandle ptHnd = IFCAnyHandleUtil.GetLocation(relPlacement);
            IList<double> origCoords = IFCAnyHandleUtil.GetCoordinates(ptHnd);

            double scale = exporterIFC.LinearScale;

            ICollection<ElementId> runIds = null;
            ICollection<ElementId> landingIds = null;
            ICollection<ElementId> supportIds = null;

            if (stair is Stairs)
            {
                Stairs stairAsStairs = stair as Stairs;
                runIds = stairAsStairs.GetStairsRuns();
                landingIds = stairAsStairs.GetStairsLandings();
                supportIds = stairAsStairs.GetStairsSupports();
            }

            IList<IFCAnyHandle> stairLocalPlacementHnds = new List<IFCAnyHandle>();
            IList<IFCLevelInfo> levelInfos = new List<IFCLevelInfo>();
            for (int ii = 0; ii < numFlights - 1; ii++)
            {
                double newOffsetScaled = 0.0;
                IFCAnyHandle newLevelHnd = null;
                levelInfos.Add(
                    placementSetter.GetOffsetLevelInfoAndHandle(heightNonScaled * (ii + 1), scale, out newLevelHnd, out newOffsetScaled));
                if (levelInfos[ii] == null)
                    levelInfos[ii] = placementSetter.GetLevelInfo();

                XYZ orig;
                if (ptHnd.HasValue)
                {
                    orig = new XYZ(origCoords[0], origCoords[1], newOffsetScaled);
                }
                else
                {
                    orig = new XYZ(0.0, 0.0, newOffsetScaled);
                }
                IFCAnyHandle relativePlacementHnd = ExporterUtil.CreateAxis2Placement3D(file, orig);
                stairLocalPlacementHnds.Add(IFCInstanceExporter.CreateLocalPlacement(file, newLevelHnd, relativePlacementHnd));
            }

            IList<List<IFCAnyHandle>> newComponents = new List<List<IFCAnyHandle>>();
            for (int ii = 0; ii < numFlights - 1; ii++)
                newComponents.Add(new List<IFCAnyHandle>());

            int compIdx = 0;
            IEnumerator<ElementId> runIter = null;
            if (runIds != null)
            {
                runIter = runIds.GetEnumerator();
                runIter.MoveNext();
            }
            IEnumerator<ElementId> landingIter = null;
            if (landingIds != null)
            {
                landingIter = landingIds.GetEnumerator();
                landingIter.MoveNext();
            }
            IEnumerator<ElementId> supportIter = null;
            if (supportIds != null)
            {
                supportIter = supportIds.GetEnumerator();
                supportIter.MoveNext();
            }

            foreach (IFCAnyHandle component in components)
            {
                string componentName = IFCAnyHandleUtil.GetStringAttribute(component, "Name");
                string componentDescription = IFCAnyHandleUtil.GetStringAttribute(component, "Description");
                string componentObjectType = IFCAnyHandleUtil.GetStringAttribute(component, "ObjectType");
                string componentElementTag = IFCAnyHandleUtil.GetStringAttribute(component, "Tag");
                IFCAnyHandle componentProdRep = IFCAnyHandleUtil.GetInstanceAttribute(component, "Representation");

                IList<string> localComponentNames = new List<string>();
                IList<IFCAnyHandle> componentPlacementHnds = new List<IFCAnyHandle>();

                IFCAnyHandle localLocalPlacement = IFCAnyHandleUtil.GetInstanceAttribute(component, "ObjectPlacement");
                IFCAnyHandle localRelativePlacement =
                    (localLocalPlacement == null) ? null : IFCAnyHandleUtil.GetInstanceAttribute(localLocalPlacement, "RelativePlacement");

                bool isSubStair = component.IsSubTypeOf(IFCEntityType.IfcStair.ToString());
                for (int ii = 0; ii < numFlights - 1; ii++)
                {
                    localComponentNames.Add((componentName == null) ? (ii + 2).ToString() : (componentName + ":" + (ii + 2)));
                    if (isSubStair)
                        componentPlacementHnds.Add(ExporterUtil.CopyLocalPlacement(file, stairLocalPlacementHnds[ii]));
                    else
                        componentPlacementHnds.Add(IFCInstanceExporter.CreateLocalPlacement(file, stairLocalPlacementHnds[ii], localRelativePlacement));
                }

                IList<IFCAnyHandle> localComponentHnds = new List<IFCAnyHandle>();
                if (isSubStair)
                {
                    string componentType = IFCAnyHandleUtil.GetEnumerationAttribute(component, "ShapeType");
                    IFCStairType localStairType = GetIFCStairType(componentType);

                    ElementId catId = CategoryUtil.GetSafeCategoryId(stair);

                    for (int ii = 0; ii < numFlights - 1; ii++)
                    {
                        IFCAnyHandle representationCopy =
                            ExporterUtil.CopyProductDefinitionShape(exporterIFC, stair, catId, componentProdRep);

                        localComponentHnds.Add(IFCInstanceExporter.CreateStair(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                            localComponentNames[ii], componentDescription, componentObjectType, componentPlacementHnds[ii], representationCopy,
                            componentElementTag, localStairType));
                    }
                }
                else if (IFCAnyHandleUtil.IsSubTypeOf(component, IFCEntityType.IfcStairFlight))
                {
                    Element runElem = (runIter == null) ? stair : stair.Document.GetElement(runIter.Current);
                    Element runElemToUse = (runElem == null) ? stair : runElem;
                    ElementId catId = CategoryUtil.GetSafeCategoryId(runElemToUse);

                    int? numberOfRiser = IFCAnyHandleUtil.GetIntAttribute(component, "NumberOfRiser");
                    int? numberOfTreads = IFCAnyHandleUtil.GetIntAttribute(component, "NumberOfTreads");
                    double? riserHeight = IFCAnyHandleUtil.GetDoubleAttribute(component, "RiserHeight");
                    double? treadLength = IFCAnyHandleUtil.GetDoubleAttribute(component, "TreadLength");

                    for (int ii = 0; ii < numFlights - 1; ii++)
                    {
                        IFCAnyHandle representationCopy =
                            ExporterUtil.CopyProductDefinitionShape(exporterIFC, runElemToUse, catId, componentProdRep);

                        localComponentHnds.Add(IFCInstanceExporter.CreateStairFlight(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                            localComponentNames[ii], componentDescription, componentObjectType, componentPlacementHnds[ii], representationCopy,
                            componentElementTag, numberOfRiser, numberOfTreads, riserHeight, treadLength));
                    }
                    runIter.MoveNext();
                }
                else if (IFCAnyHandleUtil.IsSubTypeOf(component, IFCEntityType.IfcSlab))
                {
                    string componentType = IFCAnyHandleUtil.GetEnumerationAttribute(component, "PredefinedType");
                    IFCSlabType localLandingType = FloorExporter.GetIFCSlabType(componentType);

                    Element landingElem = (landingIter == null) ? stair : stair.Document.GetElement(landingIter.Current);
                    Element landingElemToUse = (landingElem == null) ? stair : landingElem;
                    ElementId catId = CategoryUtil.GetSafeCategoryId(landingElemToUse);

                    for (int ii = 0; ii < numFlights - 1; ii++)
                    {
                        IFCAnyHandle representationCopy =
                            ExporterUtil.CopyProductDefinitionShape(exporterIFC, landingElemToUse, catId, componentProdRep);

                        localComponentHnds.Add(IFCInstanceExporter.CreateSlab(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                            localComponentNames[ii], componentDescription, componentObjectType, componentPlacementHnds[ii], representationCopy,
                            componentElementTag, localLandingType));
                    }

                    landingIter.MoveNext();
                }
                else if (IFCAnyHandleUtil.IsSubTypeOf(component, IFCEntityType.IfcMember))
                {
                    Element supportElem = (supportIter == null) ? stair : stair.Document.GetElement(supportIter.Current);
                    Element supportElemToUse = (supportElem == null) ? stair : supportElem;
                    ElementId catId = CategoryUtil.GetSafeCategoryId(supportElemToUse);

                    IFCAnyHandle memberType = (supportElemToUse != stair) ? GetMemberTypeHandle(exporterIFC, supportElemToUse) : null;

                    for (int ii = 0; ii < numFlights - 1; ii++)
                    {
                        IFCAnyHandle representationCopy =
                        ExporterUtil.CopyProductDefinitionShape(exporterIFC, supportElemToUse, catId, componentProdRep);

                        localComponentHnds.Add(IFCInstanceExporter.CreateMember(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                            localComponentNames[ii], componentDescription, componentObjectType, componentPlacementHnds[ii], representationCopy,
                            componentElementTag));

                        if (memberType != null)
                            ExporterCacheManager.TypeRelationsCache.Add(memberType, localComponentHnds[ii]);
                    }

                    supportIter.MoveNext();
                }

                for (int ii = 0; ii < numFlights - 1; ii++)
                {
                    if (localComponentHnds[ii] != null)
                    {
                        newComponents[ii].Add(localComponentHnds[ii]);
                        productWrapper.AddElement(localComponentHnds[ii], levelInfos[ii], componentECData[compIdx], false);
                    }
                }
                compIdx++;
            }

            // finally add a copy of the container.
            IList<IFCAnyHandle> stairCopyHnds = new List<IFCAnyHandle>();
            for (int ii = 0; ii < numFlights - 1; ii++)
            {
                string stairName = IFCAnyHandleUtil.GetStringAttribute(stairHnd, "Name");
                string stairObjectType = IFCAnyHandleUtil.GetStringAttribute(stairHnd, "ObjectType");
                string stairDescription = IFCAnyHandleUtil.GetStringAttribute(stairHnd, "Description");
                string stairElementTag = IFCAnyHandleUtil.GetStringAttribute(stairHnd, "Tag");
                string stairTypeAsString = IFCAnyHandleUtil.GetEnumerationAttribute(stairHnd, "ShapeType");
                IFCStairType stairType = GetIFCStairType(stairTypeAsString);

                string containerStairName = stairName + ":" + (ii + 2);
                stairCopyHnds.Add(IFCInstanceExporter.CreateStair(file, ExporterIFCUtils.CreateGUID(), ownerHistory,
                    containerStairName, stairDescription, stairObjectType, stairLocalPlacementHnds[ii], null, stairElementTag, stairType));

                productWrapper.AddElement(stairCopyHnds[ii], levelInfos[ii], null, LevelUtil.AssociateElementToLevel(stair));
            }

            for (int ii = 0; ii < numFlights - 1; ii++)
            {
                StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(stairCopyHnds[ii], newComponents[ii],
                    stairLocalPlacementHnds[ii]);
                ExporterCacheManager.StairRampContainerInfoCache.AppendStairRampContainerInfo(stair.Id, stairRampInfo);
                CategoryUtil.CreateMaterialAssociations(stair.Document, exporterIFC, stairCopyHnds[ii], materialIds);
            }
        }

        /// <summary>
        /// Exports a staircase to IfcStair, without decomposing into separate runs and landings.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ifcEnumType">The stairs type.</param>
        /// <param name="stair">The stairs element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="numFlights">The number of flights for a multistory staircase.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportStairAsSingleGeometry(ExporterIFC exporterIFC, string ifcEnumType, Element stair, GeometryElement geometryElement,
            int numFlights, IFCProductWrapper productWrapper)
        {
            if (stair == null || geometryElement == null)
                return;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, stair))
                {
                    using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                    {
                        ecData.SetLocalPlacement(placementSetter.GetPlacement());
                        ecData.ReuseLocalPlacement = false;

                        GeometryElement stairsGeom = GeometryUtil.GetOneLevelGeometryElement(geometryElement);

                        BodyData bodyData;
                        ElementId categoryId = CategoryUtil.GetSafeCategoryId(stair);

                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions();
                        IFCAnyHandle representation = RepresentationUtil.CreateBRepProductDefinitionShape(stair.Document.Application, exporterIFC,
                            stair, categoryId, stairsGeom, bodyExporterOptions, null, ecData, out bodyData);

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(representation))
                        {
                            ecData.ClearOpenings();
                            return;
                        }

                        string containedStairGuid = ExporterIFCUtils.CreateSubElementGUID(stair, (int)IFCStairSubElements.ContainedStair);
                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        string origStairName = exporterIFC.GetName();
                        string stairName = NamingUtil.GetNameOverride(stair, origStairName);
                        string stairDescription = NamingUtil.GetDescriptionOverride(stair, null);
                        string stairObjectType = NamingUtil.GetObjectTypeOverride(stair, NamingUtil.CreateIFCObjectName(exporterIFC, stair));
                        IFCAnyHandle containedStairLocalPlacement = ecData.GetLocalPlacement();
                        string elementTag = NamingUtil.CreateIFCElementId(stair);
                        IFCStairType stairType = GetIFCStairType(ifcEnumType);

                        List<IFCAnyHandle> components = new List<IFCAnyHandle>();
                        IList<IFCExtrusionCreationData> componentExtrusionData = new List<IFCExtrusionCreationData>();
                        IFCAnyHandle containedStairHnd = IFCInstanceExporter.CreateStair(file, containedStairGuid, ownerHistory, stairName,
                            stairDescription, stairObjectType, containedStairLocalPlacement, representation, elementTag, stairType);
                        components.Add(containedStairHnd);
                        componentExtrusionData.Add(ecData);
                        //productWrapper.AddElement(containedStairHnd, placementSetter.GetLevelInfo(), ecData, false);

                        string guid = ExporterIFCUtils.CreateGUID(stair);
                        IFCAnyHandle localPlacement = ecData.GetLocalPlacement();

                        IFCAnyHandle stairHnd = IFCInstanceExporter.CreateStair(file, guid, ownerHistory, stairName,
                            stairDescription, stairObjectType, localPlacement, null, elementTag, stairType);

                        productWrapper.AddElement(stairHnd, placementSetter.GetLevelInfo(), ecData, LevelUtil.AssociateElementToLevel(stair));

                        IFCAnyHandle emptyPlacement = null;
                        StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(stairHnd, components, emptyPlacement);
                        ExporterCacheManager.StairRampContainerInfoCache.AddStairRampContainerInfo(stair.Id, stairRampInfo);

                        ExportMultistoryStair(exporterIFC, stair, numFlights, stairHnd, components, componentExtrusionData,
                            bodyData.MaterialIds, placementSetter, productWrapper);
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary>
        /// Exports a staircase to IfcStair, composing into separate runs and landings.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ifcEnumType">The stairs type.</param>
        /// <param name="stair">The stairs element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="numFlights">The number of flights for a multistory staircase.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportStairsAsContainer(ExporterIFC exporterIFC, string ifcEnumType, Stairs stair, GeometryElement geometryElement,
            int numFlights, IFCProductWrapper productWrapper)
        {
            if (stair == null || geometryElement == null)
                return;

            Document doc = stair.Document;
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            IFCFile file = exporterIFC.GetFile();
            Options geomOptions = GeometryUtil.GetIFCExportGeometryOptions();
            ElementId categoryId = CategoryUtil.GetSafeCategoryId(stair);

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, stair))
                {
                    HashSet<ElementId> materialIds = new HashSet<ElementId>();

                    List<IFCAnyHandle> componentHandles = new List<IFCAnyHandle>();
                    IList<IFCExtrusionCreationData> componentExtrusionData = new List<IFCExtrusionCreationData>();

                    IFCAnyHandle contextOfItemsFootPrint = exporterIFC.Get3DContextHandle("FootPrint");

                    Transform trf = ExporterIFCUtils.GetUnscaledTransform(exporterIFC, placementSetter.GetPlacement());
                    Plane boundaryPlane = new Plane(trf.BasisX, trf.BasisY, trf.Origin);
                    XYZ boundaryProjDir = trf.BasisZ;

                    IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                    string stairGUID = ExporterIFCUtils.CreateGUID(stair);
                    string origStairName = exporterIFC.GetName();
                    string stairName = NamingUtil.GetNameOverride(stair, origStairName);
                    string stairDescription = NamingUtil.GetDescriptionOverride(stair, null);
                    string stairObjectType = NamingUtil.GetObjectTypeOverride(stair, NamingUtil.CreateIFCObjectName(exporterIFC, stair));
                    IFCAnyHandle stairLocalPlacement = placementSetter.GetPlacement();
                    string stairElementTag = NamingUtil.CreateIFCElementId(stair);
                    IFCStairType stairType = GetIFCStairType(ifcEnumType);

                    IFCAnyHandle stairContainerHnd = IFCInstanceExporter.CreateStair(file, stairGUID, ownerHistory, stairName,
                        stairDescription, stairObjectType, stairLocalPlacement, null, stairElementTag, stairType);
                    productWrapper.AddElement(stairContainerHnd, placementSetter.GetLevelInfo(), null, LevelUtil.AssociateElementToLevel(stair));

                    // Get List of runs to export their geometry.
                    ICollection<ElementId> runIds = stair.GetStairsRuns();
                    int index = 0;
                    foreach (ElementId runId in runIds)
                    {
                        index++;
                        StairsRun run = doc.GetElement(runId) as StairsRun;

                        using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                        {
                            ecData.AllowVerticalOffsetOfBReps = false;
                            ecData.SetLocalPlacement(placementSetter.GetPlacement());
                            ecData.ReuseLocalPlacement = false;

                            GeometryElement runGeometryElement = run.get_Geometry(geomOptions);

                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            BodyData bodyData = BodyExporter.ExportBody(app, exporterIFC, run, categoryId, runGeometryElement,
                                bodyExporterOptions, ecData);

                            IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                            {
                                ecData.ClearOpenings();
                                continue;
                            }

                            foreach (ElementId materialId in bodyData.MaterialIds)
                                materialIds.Add(materialId);

                            IList<IFCAnyHandle> reps = new List<IFCAnyHandle>();
                            reps.Add(bodyRep);

                            Transform runBoundaryTrf = trf.Multiply(bodyData.BrepOffsetTransform);
                            Plane runBoundaryPlane = new Plane(runBoundaryTrf.BasisX, runBoundaryTrf.BasisY, runBoundaryTrf.Origin);
                            XYZ runBoundaryProjDir = runBoundaryTrf.BasisZ;

                            CurveLoop boundary = run.GetFootprintBoundary();
                            IFCAnyHandle boundaryHnd = ExporterIFCUtils.CreateCurveFromCurveLoop(exporterIFC, boundary,
                                runBoundaryPlane, runBoundaryProjDir);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boundaryHnd))
                            {
                                HashSet<IFCAnyHandle> geomSelectSet = new HashSet<IFCAnyHandle>();
                                geomSelectSet.Add(boundaryHnd);

                                HashSet<IFCAnyHandle> boundaryItems = new HashSet<IFCAnyHandle>();
                                boundaryItems.Add(IFCInstanceExporter.CreateGeometricSet(file, geomSelectSet));

                                IFCAnyHandle boundaryRep = RepresentationUtil.CreateGeometricSetRep(exporterIFC, run, categoryId, "FootPrint",
                                    contextOfItemsFootPrint, boundaryItems);
                                reps.Add(boundaryRep);
                            }

                            CurveLoop walkingLine = run.GetStairsPath();
                            IFCAnyHandle walkingLineHnd = ExporterIFCUtils.CreateCurveFromCurveLoop(exporterIFC, walkingLine,
                                runBoundaryPlane, runBoundaryProjDir);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(walkingLineHnd))
                            {
                                HashSet<IFCAnyHandle> geomSelectSet = new HashSet<IFCAnyHandle>();
                                geomSelectSet.Add(walkingLineHnd);

                                HashSet<IFCAnyHandle> walkingLineItems = new HashSet<IFCAnyHandle>();
                                walkingLineItems.Add(IFCInstanceExporter.CreateGeometricSet(file, geomSelectSet));

                                IFCAnyHandle walkingLineRep = RepresentationUtil.CreateGeometricSetRep(exporterIFC, run, categoryId, "Axis",
                                    contextOfItemsFootPrint, walkingLineItems);
                                reps.Add(walkingLineRep);
                            }

                            IFCAnyHandle representation = IFCInstanceExporter.CreateProductDefinitionShape(exporterIFC.GetFile(), null, null, reps);

                            string runGUID = ExporterIFCUtils.CreateGUID(run);
                            string origRunName = origStairName + " Run " + index;
                            string runName = NamingUtil.GetNameOverride(run, origRunName);
                            string runDescription = NamingUtil.GetDescriptionOverride(run, stairDescription);
                            string runObjectType = NamingUtil.GetObjectTypeOverride(run, stairObjectType);
                            IFCAnyHandle runLocalPlacement = ecData.GetLocalPlacement();
                            string runElementTag = NamingUtil.CreateIFCElementId(run);

                            IFCAnyHandle stairFlightHnd = IFCInstanceExporter.CreateStairFlight(file, runGUID, ownerHistory,
                                runName, runDescription, runObjectType, runLocalPlacement, representation, runElementTag,
                                run.ActualRisersNumber, run.ActualTreadsNumber, stair.ActualRiserHeight, stair.ActualTreadDepth);

                            componentHandles.Add(stairFlightHnd);
                            componentExtrusionData.Add(ecData);

                            productWrapper.AddElement(stairFlightHnd, placementSetter.GetLevelInfo(), ecData, false);

                            ExporterCacheManager.HandleToElementCache.Register(stairFlightHnd, run.Id);
                        }
                    }

                    // Get List of landings to export their geometry.
                    ICollection<ElementId> landingIds = stair.GetStairsLandings();
                    index = 0;
                    foreach (ElementId landingId in landingIds)
                    {
                        index++;
                        StairsLanding landing = doc.GetElement(landingId) as StairsLanding;

                        using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                        {
                            ecData.AllowVerticalOffsetOfBReps = false;
                            ecData.SetLocalPlacement(placementSetter.GetPlacement());
                            ecData.ReuseLocalPlacement = false;

                            GeometryElement landingGeometryElement = landing.get_Geometry(geomOptions);

                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            BodyData bodyData = BodyExporter.ExportBody(app, exporterIFC, landing, categoryId, landingGeometryElement,
                                bodyExporterOptions, ecData);

                            IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                            {
                                ecData.ClearOpenings();
                                continue;
                            }

                            foreach (ElementId materialId in bodyData.MaterialIds)
                                materialIds.Add(materialId);

                            // create Boundary rep.
                            IList<IFCAnyHandle> reps = new List<IFCAnyHandle>();
                            reps.Add(bodyRep);

                            Transform landingBoundaryTrf = trf.Multiply(bodyData.BrepOffsetTransform);
                            Plane landingBoundaryPlane = new Plane(landingBoundaryTrf.BasisX, landingBoundaryTrf.BasisY, landingBoundaryTrf.Origin);
                            XYZ landingBoundaryProjDir = landingBoundaryTrf.BasisZ;

                            CurveLoop boundary = landing.GetFootprintBoundary();
                            IFCAnyHandle boundaryHnd = ExporterIFCUtils.CreateCurveFromCurveLoop(exporterIFC, boundary,
                                landingBoundaryPlane, landingBoundaryProjDir);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boundaryHnd))
                            {
                                HashSet<IFCAnyHandle> geomSelectSet = new HashSet<IFCAnyHandle>();
                                geomSelectSet.Add(boundaryHnd);

                                HashSet<IFCAnyHandle> boundaryItems = new HashSet<IFCAnyHandle>();
                                boundaryItems.Add(IFCInstanceExporter.CreateGeometricSet(file, geomSelectSet));

                                IFCAnyHandle boundaryRep = RepresentationUtil.CreateGeometricSetRep(exporterIFC, landing, categoryId, "FootPrint",
                                    contextOfItemsFootPrint, boundaryItems);
                                reps.Add(boundaryRep);
                            }

                            CurveLoop walkingLine = landing.GetStairsPath();
                            IFCAnyHandle walkingLineHnd = ExporterIFCUtils.CreateCurveFromCurveLoop(exporterIFC, walkingLine,
                                landingBoundaryPlane, landingBoundaryProjDir);
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(walkingLineHnd))
                            {
                                HashSet<IFCAnyHandle> geomSelectSet = new HashSet<IFCAnyHandle>();
                                geomSelectSet.Add(walkingLineHnd);

                                HashSet<IFCAnyHandle> walkingLineItems = new HashSet<IFCAnyHandle>();
                                walkingLineItems.Add(IFCInstanceExporter.CreateGeometricSet(file, geomSelectSet));

                                IFCAnyHandle walkingLineRep = RepresentationUtil.CreateGeometricSetRep(exporterIFC, landing, categoryId, "Axis",
                                    contextOfItemsFootPrint, walkingLineItems);
                                reps.Add(walkingLineRep);
                            }

                            string landingGUID = ExporterIFCUtils.CreateGUID(landing);
                            string origLandingName = origStairName + " Landing " + index;
                            string landingName = NamingUtil.GetNameOverride(landing, origLandingName);
                            string landingDescription = NamingUtil.GetDescriptionOverride(landing, stairDescription);
                            string landingObjectType = NamingUtil.GetObjectTypeOverride(landing, stairObjectType);
                            IFCAnyHandle landingLocalPlacement = ecData.GetLocalPlacement();
                            string landingElementTag = NamingUtil.CreateIFCElementId(landing);

                            IFCAnyHandle representation = IFCInstanceExporter.CreateProductDefinitionShape(exporterIFC.GetFile(), null, null, reps);

                            IFCAnyHandle landingHnd = IFCInstanceExporter.CreateSlab(file, landingGUID, ownerHistory,
                                landingName, landingDescription, landingObjectType, landingLocalPlacement, representation, landingElementTag,
                                IFCSlabType.Landing);

                            componentHandles.Add(landingHnd);
                            componentExtrusionData.Add(ecData);

                            productWrapper.AddElement(landingHnd, placementSetter.GetLevelInfo(), ecData, false);
                            ExporterCacheManager.HandleToElementCache.Register(landingHnd, landing.Id);
                        }
                    }

                    // Get List of supports to export their geometry.  Supports are not exposed to API, so export as generic Element.
                    ICollection<ElementId> supportIds = stair.GetStairsSupports();
                    index = 0;
                    foreach (ElementId supportId in supportIds)
                    {
                        index++;
                        Element support = doc.GetElement(supportId);

                        using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                        {
                            ecData.SetLocalPlacement(placementSetter.GetPlacement());
                            ecData.ReuseLocalPlacement = false;

                            GeometryElement supportGeometryElement = support.get_Geometry(geomOptions);
                            BodyData bodyData;
                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            IFCAnyHandle representation = RepresentationUtil.CreateBRepProductDefinitionShape(app, exporterIFC,
                                support, categoryId, supportGeometryElement, bodyExporterOptions, null, ecData, out bodyData);

                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(representation))
                            {
                                ecData.ClearOpenings();
                                continue;
                            }

                            foreach (ElementId materialId in bodyData.MaterialIds)
                                materialIds.Add(materialId);

                            string supportGUID = ExporterIFCUtils.CreateGUID(support);
                            string origSupportName = origStairName + " Stringer " + index;
                            string supportName = NamingUtil.GetNameOverride(support, origSupportName);
                            string supportDescription = NamingUtil.GetDescriptionOverride(support, stairDescription);
                            string supportObjectType = NamingUtil.GetObjectTypeOverride(support, stairObjectType);
                            IFCAnyHandle supportLocalPlacement = ecData.GetLocalPlacement();
                            string supportElementTag = NamingUtil.CreateIFCElementId(support);

                            IFCAnyHandle type = GetMemberTypeHandle(exporterIFC, support);

                            IFCAnyHandle supportHnd = IFCInstanceExporter.CreateMember(file, supportGUID, ownerHistory,
                                supportName, supportDescription, supportObjectType, supportLocalPlacement, representation, supportElementTag);

                            componentHandles.Add(supportHnd);
                            componentExtrusionData.Add(ecData);

                            productWrapper.AddElement(supportHnd, placementSetter.GetLevelInfo(), ecData, false);

                            ExporterCacheManager.TypeRelationsCache.Add(type, supportHnd);
                        }
                    }

                    StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(stairContainerHnd, componentHandles, null);
                    ExporterCacheManager.StairRampContainerInfoCache.AddStairRampContainerInfo(stair.Id, stairRampInfo);

                    CategoryUtil.CreateMaterialAssociations(stair.Document, exporterIFC, stairContainerHnd, materialIds);

                    ExportMultistoryStair(exporterIFC, stair, numFlights, stairContainerHnd, componentHandles, componentExtrusionData,
                         materialIds, placementSetter, productWrapper);
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Exports a legacy staircase or ramp to IfcStair or IfcRamp, composing into separate runs and landings.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="ifcEnumType">>The ifc type.</param>
        /// <param name="legacyStair">The legacy stairs or ramp element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="useCoarseTessellation">Export using a coarse tessellation if true.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void ExportLegacyStairOrRampAsContainer(ExporterIFC exporterIFC, string ifcEnumType, Element legacyStair, GeometryElement geometryElement,
            bool useCoarseTessellation, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            Autodesk.Revit.ApplicationServices.Application app = legacyStair.Document.Application;
            ElementId categoryId = CategoryUtil.GetSafeCategoryId(legacyStair);

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, legacyStair))
                {
                    IFCLegacyStairOrRamp legacyStairOrRamp = ExporterIFCUtils.GetLegacyStairOrRampComponents(exporterIFC, legacyStair);
                    if (legacyStairOrRamp == null)
                        return;

                    bool isRamp = legacyStairOrRamp.IsRamp;

                    using (IFCExtrusionCreationData ifcECData = new IFCExtrusionCreationData())
                    {
                        ifcECData.SetLocalPlacement(placementSetter.GetPlacement());

                        string stairDescription = NamingUtil.GetDescriptionOverride(legacyStair, null);
                        string stairObjectType = NamingUtil.GetObjectTypeOverride(legacyStair, NamingUtil.CreateIFCObjectName(exporterIFC, legacyStair));
                        string stairElementTag = NamingUtil.CreateIFCElementId(legacyStair);

                        double defaultHeight = GetDefaultHeightForLegacyStair(exporterIFC.LinearScale);
                        double stairHeight = GetStairsHeightForLegacyStair(exporterIFC, legacyStair, defaultHeight);
                        int numFlights = GetNumFlightsForLegacyStair(exporterIFC, legacyStair, defaultHeight);

                        List<IFCLevelInfo> localLevelInfoForFlights = new List<IFCLevelInfo>();
                        List<IFCAnyHandle> localPlacementForFlights = new List<IFCAnyHandle>();
                        List<List<IFCAnyHandle>> components = new List<List<IFCAnyHandle>>();

                        components.Add(new List<IFCAnyHandle>());
                        if (numFlights > 1)
                        {
                            XYZ zDir = new XYZ(0.0, 0.0, 1.0);
                            XYZ xDir = new XYZ(1.0, 0.0, 0.0);
                            for (int ii = 1; ii < numFlights; ii++)
                            {
                                components.Add(new List<IFCAnyHandle>());
                                double newOffsetScaled = 0.0;
                                IFCAnyHandle newLevelHnd = null;
                                localLevelInfoForFlights.Add(
                                    placementSetter.GetOffsetLevelInfoAndHandle(stairHeight * ii, exporterIFC.LinearScale, out newLevelHnd, out newOffsetScaled));

                                XYZ orig = new XYZ(0.0, 0.0, newOffsetScaled);
                                IFCAnyHandle relativePlacement = ExporterUtil.CreateAxis2Placement3D(file, orig, zDir, xDir);
                                localPlacementForFlights.Add(IFCInstanceExporter.CreateLocalPlacement(file, newLevelHnd, relativePlacement));
                            }
                        }

                        IList<IFCAnyHandle> walkingLineReps = legacyStairOrRamp.GetWalkingLineRepresentations();
                        IList<IFCAnyHandle> boundaryReps = legacyStairOrRamp.GetBoundaryRepresentations();
                        IList<IList<GeometryObject>> geometriesOfRuns = legacyStairOrRamp.GetRunGeometries();
                        IList<int> numRisers = legacyStairOrRamp.GetNumberOfRisers();
                        IList<int> numTreads = legacyStairOrRamp.GetNumberOfTreads();
                        IList<double> treadsLength = legacyStairOrRamp.GetTreadsLength();
                        double riserHeight = legacyStairOrRamp.RiserHeight;

                        int runCount = geometriesOfRuns.Count;
                        for (int ii = 0; ii < runCount; ii++)
                        {
                            BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                            if (useCoarseTessellation)
                                bodyExporterOptions.TessellationLevel = BodyExporterOptions.BodyTessellationLevel.Coarse;

                            IList<GeometryObject> geometriesOfARun = geometriesOfRuns[ii];
                            BodyData bodyData = BodyExporter.ExportBody(app, exporterIFC, legacyStair, categoryId, geometriesOfARun,
                                bodyExporterOptions, null);

                            IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                            if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                            {
                                continue;
                            }


                            HashSet<IFCAnyHandle> flightHnds = new HashSet<IFCAnyHandle>();
                            List<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(walkingLineReps[ii]))
                            {
                                representations.Add(walkingLineReps[ii]);
                            }
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boundaryReps[ii]))
                            {
                                representations.Add(boundaryReps[ii]);
                            }
                            representations.Add(bodyRep);

                            IFCAnyHandle flightRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);
                            IFCAnyHandle flightLocalPlacement = ExporterUtil.CopyLocalPlacement(file, placementSetter.GetPlacement());

                            IFCAnyHandle flightHnd;
                            string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, ii + 1));
                            if (isRamp)
                            {
                                flightHnd = IFCInstanceExporter.CreateRampFlight(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                    stairName, stairDescription, stairObjectType, flightLocalPlacement, flightRep, stairElementTag);
                                flightHnds.Add(flightHnd);
                                productWrapper.AddElement(flightHnd, placementSetter.GetLevelInfo(), null, false);
                            }
                            else
                            {
                                flightHnd = IFCInstanceExporter.CreateStairFlight(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                    stairName, stairDescription, stairObjectType, flightLocalPlacement, flightRep, stairElementTag, numRisers[ii], numTreads[ii],
                                    riserHeight, treadsLength[ii]);
                                flightHnds.Add(flightHnd);
                                productWrapper.AddElement(flightHnd, placementSetter.GetLevelInfo(), null, false);
                            }

                            components[0].Add(flightHnd);
                            for (int compIdx = 1; compIdx < numFlights; compIdx++)
                            {
                                if (isRamp)
                                {
                                    IFCAnyHandle newLocalPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacementForFlights[compIdx - 1]);
                                    IFCAnyHandle newProdRep = ExporterUtil.CopyProductDefinitionShape(exporterIFC, legacyStair, categoryId, IFCAnyHandleUtil.GetRepresentation(flightHnd));
                                    flightHnd = IFCInstanceExporter.CreateRampFlight(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, newLocalPlacement, newProdRep, stairElementTag);
                                    components[compIdx].Add(flightHnd);
                                }
                                else
                                {
                                    IFCAnyHandle newLocalPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacementForFlights[compIdx - 1]);
                                    IFCAnyHandle newProdRep = ExporterUtil.CopyProductDefinitionShape(exporterIFC, legacyStair, categoryId, IFCAnyHandleUtil.GetRepresentation(flightHnd));

                                    flightHnd = IFCInstanceExporter.CreateStairFlight(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, newLocalPlacement, newProdRep, stairElementTag,
                                        numRisers[ii], numTreads[ii], riserHeight, treadsLength[ii]);
                                    components[compIdx].Add(flightHnd);
                                }
                                productWrapper.AddElement(flightHnd, placementSetter.GetLevelInfo(), null, false);
                                flightHnds.Add(flightHnd);
                            }
                        }

                        IList<IList<GeometryObject>> geometriesOfLandings = legacyStairOrRamp.GetLandingGeometries();
                        for (int ii = 0; ii < geometriesOfLandings.Count; ii++)
                        {
                            using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                            {
                                BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                                bodyExporterOptions.TessellationLevel = BodyExporterOptions.BodyTessellationLevel.Coarse;
                                IList<GeometryObject> geometriesOfALanding = geometriesOfLandings[ii];
                                BodyData bodyData = BodyExporter.ExportBody(app, exporterIFC, legacyStair, categoryId, geometriesOfALanding,
                                    bodyExporterOptions, ecData);

                                IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                                if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                                {
                                    ecData.ClearOpenings();
                                    continue;
                                }

                                List<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(walkingLineReps[ii + runCount]))
                                {
                                    representations.Add(walkingLineReps[ii + runCount]);
                                }
                                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boundaryReps[ii + runCount]))
                                {
                                    representations.Add(boundaryReps[ii + runCount]);
                                }
                                representations.Add(bodyRep);

                                IFCAnyHandle shapeHnd = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);
                                IFCAnyHandle landingLocalPlacement = ExporterUtil.CopyLocalPlacement(file, placementSetter.GetPlacement());
                                string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, ii + 1));

                                IFCAnyHandle slabHnd = IFCInstanceExporter.CreateSlab(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                    stairName, stairDescription, stairObjectType, landingLocalPlacement, shapeHnd, stairElementTag, IFCSlabType.Landing);
                                productWrapper.AddElement(slabHnd, placementSetter.GetLevelInfo(), ecData, false);

                                components[0].Add(slabHnd);
                                for (int compIdx = 1; compIdx < numFlights; compIdx++)
                                {
                                    IFCAnyHandle newLocalPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacementForFlights[compIdx - 1]);
                                    IFCAnyHandle newProdRep = ExporterUtil.CopyProductDefinitionShape(exporterIFC, legacyStair, categoryId, IFCAnyHandleUtil.GetRepresentation(slabHnd));

                                    IFCAnyHandle newSlabHnd = IFCInstanceExporter.CreateSlab(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, newLocalPlacement, newProdRep, stairElementTag, IFCSlabType.Landing);
                                    components[compIdx].Add(newSlabHnd);
                                    productWrapper.AddElement(newSlabHnd, placementSetter.GetLevelInfo(), ecData, false);
                                }
                            }
                        }

                        IList<GeometryObject> geometriesOfStringer = legacyStairOrRamp.GetStringerGeometries();
                        for (int ii = 0; ii < geometriesOfStringer.Count; ii++)
                        {
                            using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                            {
                                BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                                bodyExporterOptions.TessellationLevel = BodyExporterOptions.BodyTessellationLevel.Coarse;
                                GeometryObject geometryOfStringer = geometriesOfStringer[ii];
                                BodyData bodyData = BodyExporter.ExportBody(app, exporterIFC, legacyStair, categoryId, geometryOfStringer,
                                    bodyExporterOptions, ecData);

                                IFCAnyHandle bodyRep = bodyData.RepresentationHnd;
                                if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRep))
                                {
                                    ecData.ClearOpenings();
                                    continue;
                                }

                                List<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                                representations.Add(bodyRep);

                                IFCAnyHandle stringerRepHnd = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);
                                IFCAnyHandle stringerLocalPlacement = ExporterUtil.CopyLocalPlacement(file, placementSetter.GetPlacement());
                                string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, ii + 1));

                                IFCAnyHandle memberHnd = IFCInstanceExporter.CreateMember(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                    stairName, stairDescription, stairObjectType, stringerLocalPlacement, stringerRepHnd, stairElementTag);
                                productWrapper.AddElement(memberHnd, placementSetter.GetLevelInfo(), ecData, false);
                                PropertyUtil.CreateBeamColumnMemberBaseQuantities(exporterIFC, memberHnd, null, ecData);

                                components[0].Add(memberHnd);
                                for (int compIdx = 1; compIdx < numFlights; compIdx++)
                                {
                                    IFCAnyHandle newLocalPlacement = ExporterUtil.CopyLocalPlacement(file, localPlacementForFlights[compIdx - 1]);
                                    IFCAnyHandle newProdRep = ExporterUtil.CopyProductDefinitionShape(exporterIFC, legacyStair, categoryId, IFCAnyHandleUtil.GetRepresentation(memberHnd));

                                    IFCAnyHandle newMemberHnd = IFCInstanceExporter.CreateMember(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, newLocalPlacement, newProdRep, stairElementTag);
                                    components[compIdx].Add(newMemberHnd);
                                    productWrapper.AddElement(newMemberHnd, placementSetter.GetLevelInfo(), ecData, false);
                                }
                            }
                        }

                        List<IFCAnyHandle> createdStairs = new List<IFCAnyHandle>();
                        if (isRamp)
                        {
                            IFCRampType rampType = RampExporter.GetIFCRampType(ifcEnumType);
                            string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, 0));
                            IFCAnyHandle containedRampHnd = IFCInstanceExporter.CreateRamp(file, ExporterIFCUtils.CreateGUID(legacyStair), exporterIFC.GetOwnerHistoryHandle(),
                                stairName, stairDescription, stairObjectType, placementSetter.GetPlacement(), null, stairElementTag, rampType);
                            productWrapper.AddElement(containedRampHnd, placementSetter.GetLevelInfo(), ifcECData, true);
                            createdStairs.Add(containedRampHnd);
                        }
                        else
                        {
                            IFCStairType stairType = GetIFCStairType(ifcEnumType);
                            string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, 0));
                            IFCAnyHandle containedStairHnd = IFCInstanceExporter.CreateStair(file, ExporterIFCUtils.CreateGUID(legacyStair), exporterIFC.GetOwnerHistoryHandle(),
                                stairName, stairDescription, stairObjectType, placementSetter.GetPlacement(), null, stairElementTag, stairType);
                            productWrapper.AddElement(containedStairHnd, placementSetter.GetLevelInfo(), ifcECData, true);
                            createdStairs.Add(containedStairHnd);
                        }

                        // multi-story stairs.
                        if (numFlights > 1)
                        {
                            IFCAnyHandle localPlacement = placementSetter.GetPlacement();
                            IFCAnyHandle relPlacement = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);
                            IFCAnyHandle ptHnd = IFCAnyHandleUtil.GetLocation(relPlacement);
                            IList<double> origCoords = null;
                            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(ptHnd))
                                origCoords = IFCAnyHandleUtil.GetCoordinates(ptHnd);

                            for (int ii = 1; ii < numFlights; ii++)
                            {
                                IFCLevelInfo levelInfo = localLevelInfoForFlights[ii - 1];
                                if (levelInfo == null)
                                    levelInfo = placementSetter.GetLevelInfo();

                                localPlacement = localPlacementForFlights[ii - 1];

                                // relate to bottom stair or closest level?  For code checking, we need closest level, and
                                // that seems good enough for the general case.
                                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(ptHnd))
                                {
                                    IFCAnyHandle relPlacement2 = GeometryUtil.GetRelativePlacementFromLocalPlacement(localPlacement);
                                    IFCAnyHandle newPt = IFCAnyHandleUtil.GetLocation(relPlacement2);

                                    List<double> newCoords = new List<double>();
                                    newCoords.Add(origCoords[0]);
                                    newCoords.Add(origCoords[1]);
                                    newCoords.Add(origCoords[2]);
                                    if (!IFCAnyHandleUtil.IsNullOrHasNoValue(newPt))
                                    {
                                        IList<double> addToCoords;
                                        addToCoords = IFCAnyHandleUtil.GetCoordinates(newPt);
                                        newCoords[0] += addToCoords[0];
                                        newCoords[1] += addToCoords[1];
                                        newCoords[2] = addToCoords[2];
                                    }

                                    IFCAnyHandle locPt = ExporterUtil.CreateCartesianPoint(file, newCoords);
                                    IFCAnyHandleUtil.SetAttribute(relPlacement2, "Location", locPt);
                                }

                                if (isRamp)
                                {
                                    IFCRampType rampType = RampExporter.GetIFCRampType(ifcEnumType);
                                    string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, 0));
                                    IFCAnyHandle containedRampHnd = IFCInstanceExporter.CreateRamp(file, ExporterIFCUtils.CreateGUID(legacyStair), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, localPlacement, null, stairElementTag, rampType);
                                    productWrapper.AddElement(containedRampHnd, levelInfo, ifcECData, true);
                                    //createdStairs.Add(containedRampHnd) ???????????????????????
                                }
                                else
                                {
                                    IFCStairType stairType = GetIFCStairType(ifcEnumType);
                                    string stairName = NamingUtil.GetNameOverride(legacyStair, NamingUtil.CreateIFCName(exporterIFC, 0));
                                    IFCAnyHandle containedStairHnd = IFCInstanceExporter.CreateStair(file, ExporterIFCUtils.CreateGUID(legacyStair), exporterIFC.GetOwnerHistoryHandle(),
                                        stairName, stairDescription, stairObjectType, localPlacement, null, stairElementTag, stairType);
                                    productWrapper.AddElement(containedStairHnd, levelInfo, ifcECData, true);
                                    createdStairs.Add(containedStairHnd);
                                }
                            }
                        }

                        localPlacementForFlights.Insert(0, null);

                        StairRampContainerInfo stairRampInfo = new StairRampContainerInfo(createdStairs, components, localPlacementForFlights);
                        ExporterCacheManager.StairRampContainerInfoCache.AddStairRampContainerInfo(legacyStair.Id, stairRampInfo);
                    }
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Exports a staircase to IfcStair.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The stairs element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        public static void Export(ExporterIFC exporterIFC, Element element, GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            string ifcEnumType = CategoryUtil.GetIFCEnumTypeName(exporterIFC, element);
            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                if (element is Stairs)
                {
                    Stairs stair = element as Stairs;
                    int numFlights = stair.NumberOfStories;
                    if (numFlights > 0)
                    {
                        ExportStairsAsContainer(exporterIFC, ifcEnumType, stair, geometryElement, numFlights, productWrapper);
                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(productWrapper.GetAnElement()))
                            ExportStairAsSingleGeometry(exporterIFC, ifcEnumType, element, geometryElement, numFlights, productWrapper);
                    }
                }
                else if (IsLegacyStairs(element))
                {
                    ExportLegacyStairOrRampAsContainer(exporterIFC, ifcEnumType, element, geometryElement, true, productWrapper);
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(productWrapper.GetAnElement()))
                    {
                        double defaultHeight = GetDefaultHeightForLegacyStair(exporterIFC.LinearScale);
                        int numFlights = GetNumFlightsForLegacyStair(exporterIFC, element, defaultHeight);
                        if (numFlights > 0)
                            ExportStairAsSingleGeometry(exporterIFC, ifcEnumType, element, geometryElement, numFlights, productWrapper);
                    }
                }
                else
                {
                    ExportStairAsSingleGeometry(exporterIFC, ifcEnumType, element, geometryElement, 1, productWrapper);
                }

                PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                tr.Commit();
            }
        }
    }
}
