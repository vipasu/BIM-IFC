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
using System.Collections.ObjectModel;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Exporter.PropertySet;
using BIM.IFC.Exporter.PropertySet.Calculators;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Initializes user defined parameters and quantities.
    /// </summary>
    class ExporterInitializer
    {
        /// <summary>
        /// Initializes property sets.
        /// </summary>
        /// <param name="fileVersion">The IFC file version.</param>
        public static void InitPropertySets(IFCVersion fileVersion)
        {
            ParameterCache cache = ExporterCacheManager.ParameterCache;

            InitCommonPropertySets(cache.PropertySets, fileVersion);

            if (fileVersion == IFCVersion.IFCCOBIE)
            {
                InitCOBIEPropertySets(cache.PropertySets);
            }
        }

        /// <summary>
        /// Initializes quantities.
        /// </summary>
        /// <param name="fileVersion">The IFC file version.</param>
        /// <param name="exportBaseQuantities">True if export base quantities.</param>
        public static void InitQuantities(IFCVersion fileVersion, bool exportBaseQuantities)
        {
            ParameterCache cache = ExporterCacheManager.ParameterCache;

            if (exportBaseQuantities)
            {
                InitBaseQuantities(cache.Quantities);
            }

            if (fileVersion == IFCVersion.IFCCOBIE)
            {
                InitCOBIEQuantities(cache.Quantities);
            }
        }

        // Properties
        /// <summary>
        /// Initializes common property sets.
        /// </summary>
        /// <param name="propertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitCommonPropertySets(IList<IList<PropertySetDescription>> propertySets, IFCVersion fileVersion)
        {
            IList<PropertySetDescription> commonPropertySets = new List<PropertySetDescription>();

            // Manufacturer type information
            InitPropertySetManufacturerTypeInformation(commonPropertySets);

            // Architectural property sets.
            InitPropertySetBeamCommon(commonPropertySets);
            InitPropertySetRailingCommon(commonPropertySets);
            InitPropertySetRampCommon(commonPropertySets);
            InitPropertySetRampFlightCommon(commonPropertySets);
            InitPropertySetRoofCommon(commonPropertySets, fileVersion);
            InitPropertySetSlabCommon(commonPropertySets);
            InitPropertySetStairCommon(commonPropertySets);
            InitPropertySetStairFlightCommon(commonPropertySets);
            InitPropertySetWallCommon(commonPropertySets);

            // Building property sets.
            InitPropertySetBuildingCommon(commonPropertySets, fileVersion);
            InitPropertySetBuildingWaterStorage(commonPropertySets);

            // Proxy property sets.
            InitPropertySetElementShading(commonPropertySets);

            // Level property sets.
            InitPropertySetLevelCommon(commonPropertySets, fileVersion);

            // Site property sets.
            InitPropertySetSiteCommon(commonPropertySets);

            // Building Element Proxy
            InitPropertySetBuildingElementProxyCommon(commonPropertySets);

            // Space
            InitPropertySetSpaceCommon(commonPropertySets, fileVersion);
            InitPropertySetSpaceFireSafetyRequirements(commonPropertySets);
            InitPropertySetSpaceLightingRequirements(commonPropertySets);
            InitPropertySetSpaceThermalRequirements(commonPropertySets, fileVersion);
            InitPropertySetGSASpaceCategories(commonPropertySets);
            InitPropertySetSpaceOccupant(commonPropertySets);
            InitPropertySetSpaceZones(commonPropertySets, fileVersion);

            propertySets.Add(commonPropertySets);
        }

        /// <summary>
        /// Initializes manufacturer type information property sets for all IfcElements.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetManufacturerTypeInformation(IList<PropertySetDescription> commonPropertySets)
        {
            //property set Manufacturer Information
            PropertySetDescription propertySetManufacturer = new PropertySetDescription();
            propertySetManufacturer.Name = "Pset_ManufacturerTypeInformation";

            // sub type of IfcElement
            propertySetManufacturer.EntityTypes.Add(IFCEntityType.IfcElement);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("ArticleNumber");
            propertySetManufacturer.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ModelReference");
            propertySetManufacturer.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ModelLabel");
            propertySetManufacturer.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Manufacturer");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.ALL_MODEL_MANUFACTURER;
            propertySetManufacturer.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("ProductionYear");
            propertySetManufacturer.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetManufacturer);
        }

        /// <summary>
        /// Initializes common wall property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetWallCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property set wall common
            PropertySetDescription propertySetWallCommon = new PropertySetDescription();
            propertySetWallCommon.Name = "Pset_WallCommon";
            propertySetWallCommon.SubElementIndex = (int)IFCWallSubElements.PSetWallCommon;

            propertySetWallCommon.EntityTypes.Add(IFCEntityType.IfcWall);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetWallCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("LoadBearing");
            ifcPSE.PropertyCalculator = LoadBearingCalculator.Instance;
            propertySetWallCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("ExtendToStructure");
            ifcPSE.PropertyCalculator = ExtendToStructureCalculator.Instance;
            propertySetWallCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetWallCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetWallCommon.Entries.Add(ifcPSE);

            propertySetWallCommon.Entries.Add(PropertySetEntry.CreateLabel("AcousticRating"));
            propertySetWallCommon.Entries.Add(PropertySetEntry.CreateLabel("SurfaceSpreadOfFlame"));
            propertySetWallCommon.Entries.Add(PropertySetEntry.CreateBoolean("Combustible"));
            propertySetWallCommon.Entries.Add(PropertySetEntry.CreateBoolean("Compartmentation"));
            propertySetWallCommon.Entries.Add(PropertySetEntry.CreateReal("ThermalTransmittance"));

            commonPropertySets.Add(propertySetWallCommon);
        }

        /// <summary>
        /// Initializes common beam property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBeamCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property beam common
            PropertySetDescription propertySetBeamCommon = new PropertySetDescription();
            propertySetBeamCommon.Name = "Pset_BeamCommon";
            propertySetBeamCommon.SubElementIndex = (int)IFCBeamSubElements.PSetBeamCommon;

            propertySetBeamCommon.EntityTypes.Add(IFCEntityType.IfcBeam);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("LoadBearing");
            ifcPSE.PropertyCalculator = BeamLoadBearingCalculator.Instance;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("Span");
            ifcPSE.PropertyCalculator = BeamSpanCalculator.Instance;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = BeamSlopeCalculator.Instance;
            propertySetBeamCommon.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetBeamCommon);
        }

        /// <summary>
        /// Initializes common roof property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRoofCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            // PSet_RoofCommon
            PropertySetDescription propertySetRoofCommon = new PropertySetDescription();
            propertySetRoofCommon.Name = "Pset_RoofCommon";
            propertySetRoofCommon.SubElementIndex = (int)IFCRoofSubElements.PSetRoofCommon;

            propertySetRoofCommon.EntityTypes.Add(IFCEntityType.IfcRoof);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetRoofCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetRoofCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetRoofCommon.Entries.Add(ifcPSE);

            if (fileVersion != IFCVersion.IFC2x2)
            {
                ifcPSE = PropertySetEntry.CreateArea("TotalArea");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
                propertySetRoofCommon.Entries.Add(ifcPSE);

                ifcPSE = PropertySetEntry.CreateArea("ProjectedArea");
                ifcPSE.PropertyCalculator = RoofProjectedAreaCalculator.Instance;
                propertySetRoofCommon.Entries.Add(ifcPSE);
            }

            commonPropertySets.Add(propertySetRoofCommon);
        }

        /// <summary>
        /// Initializes common slab property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSlabCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_SlabCommon
            PropertySetDescription propertySetSlabCommon = new PropertySetDescription();
            propertySetSlabCommon.Name = "Pset_SlabCommon";
            propertySetSlabCommon.SubElementIndex = (int)IFCSlabSubElements.PSetSlabCommon;
            
            propertySetSlabCommon.EntityTypes.Add(IFCEntityType.IfcSlab);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("LoadBearing");
            ifcPSE.PropertyCalculator = SlabLoadBearingCalculator.Instance; // always true
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("AcousticRating");
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("SurfaceSpreadOfFlame");
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("Combustible");
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("Compartmentation");
            propertySetSlabCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("ThermalTransmittance");
            propertySetSlabCommon.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetSlabCommon);
        }

        /// <summary>
        /// Initializes common railing property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRailingCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_RailingCommon
            PropertySetDescription propertySetRailingCommon = new PropertySetDescription();
            propertySetRailingCommon.Name = "Pset_RailingCommon";

            propertySetRailingCommon.EntityTypes.Add(IFCEntityType.IfcRailing);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetRailingCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetRailingCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Height");
            ifcPSE.PropertyCalculator = RailingHeightCalculator.Instance;
            propertySetRailingCommon.Entries.Add(ifcPSE);

            // Railing diameter not supported.

            commonPropertySets.Add(propertySetRailingCommon);
        }

        /// <summary>
        /// Initializes common ramp property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRampCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_RampCommon
            PropertySetDescription propertySetRampCommon = new PropertySetDescription();
            propertySetRampCommon.Name = "Pset_RampCommon";
            propertySetRampCommon.SubElementIndex = (int)IFCRampSubElements.PSetRampCommon;

            propertySetRampCommon.EntityTypes.Add(IFCEntityType.IfcRamp);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetRampCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetRampCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetRampCommon.Entries.Add(ifcPSE);

            propertySetRampCommon.Entries.Add(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetRampCommon.Entries.Add(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetRampCommon.Entries.Add(PropertySetEntry.CreateBoolean("HasNonSkidSurface"));
            propertySetRampCommon.Entries.Add(PropertySetEntry.CreateReal("RequiredHeadroom"));
            propertySetRampCommon.Entries.Add(PropertySetEntry.CreateReal("RequiredSlope"));

            commonPropertySets.Add(propertySetRampCommon);
        }

        /// <summary>
        /// Initializes common stair flight property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetStairFlightCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_StairFlightCommon
            PropertySetDescription propertySetStairFlightCommon = new PropertySetDescription();
            propertySetStairFlightCommon.Name = "Pset_StairFlightCommon";
            // Add Calculator for SubElementIndex.

            propertySetStairFlightCommon.EntityTypes.Add(IFCEntityType.IfcStairFlight);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            PropertyCalculator stairRiserAndTreadsCalculator = StairRiserTreadsCalculator.Instance;
            ifcPSE = PropertySetEntry.CreateInteger("NumberOfRiser");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateInteger("NumberOfTreads");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("RiserHeight");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLengthAtOffset");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLengthAtInnerSide");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("NosingLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("WalkingLineOffset");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("WaistThickness");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairFlightCommon.Entries.Add(ifcPSE);

            propertySetStairFlightCommon.Entries.Add(PropertySetEntry.CreatePositiveLength("Headroom"));

            commonPropertySets.Add(propertySetStairFlightCommon);
        }

        /// <summary>
        /// Initializes common ramp flight property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetRampFlightCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // Pset_RampFlightCommon
            PropertySetDescription propertySetRampFlightCommon = new PropertySetDescription();
            propertySetRampFlightCommon.Name = "Pset_RampFlightCommon";

            propertySetRampFlightCommon.EntityTypes.Add(IFCEntityType.IfcRampFlight);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetRampFlightCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePlaneAngle("Slope");
            ifcPSE.PropertyCalculator = RampFlightSlopeCalculator.Instance;
            propertySetRampFlightCommon.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetRampFlightCommon);
        }

        /// <summary>
        /// Initializes common stair property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetStairCommon(IList<PropertySetDescription> commonPropertySets)
        {
            // PSet_StairCommon
            PropertySetDescription propertySetStairCommon = new PropertySetDescription();
            propertySetStairCommon.Name = "Pset_StairCommon";
            propertySetStairCommon.SubElementIndex = (int)IFCStairSubElements.PSetStairCommon;

            propertySetStairCommon.EntityTypes.Add(IFCEntityType.IfcStair);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetStairCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("IsExternal");
            ifcPSE.PropertyCalculator = ExternalCalculator.Instance;
            propertySetStairCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("FireRating");
            ifcPSE.RevitBuiltInParameter = BuiltInParameter.DOOR_FIRE_RATING;
            propertySetStairCommon.Entries.Add(ifcPSE);

            PropertyCalculator stairRiserAndTreadsCalculator = StairRiserTreadsCalculator.Instance;
            ifcPSE = PropertySetEntry.CreateInteger("NumberOfRiser");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateInteger("NumberOfTreads");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("RiserHeight");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreatePositiveLength("TreadLength");
            ifcPSE.PropertyCalculator = stairRiserAndTreadsCalculator;
            propertySetStairCommon.Entries.Add(ifcPSE);

            propertySetStairCommon.Entries.Add(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetStairCommon.Entries.Add(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetStairCommon.Entries.Add(PropertySetEntry.CreateBoolean("HasNonSkidSurface"));
            propertySetStairCommon.Entries.Add(PropertySetEntry.CreateBoolean("RequiredHeadroom"));

            commonPropertySets.Add(propertySetStairCommon);
        }

        /// <summary>
        /// Initializes common building property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitPropertySetBuildingCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            // PSet_BuildingCommon
            PropertySetDescription propertySetBuildingCommon = new PropertySetDescription();
            propertySetBuildingCommon.Name = "Pset_BuildingCommon";
            propertySetBuildingCommon.EntityTypes.Add(IFCEntityType.IfcBuilding);

            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateIdentifier("BuildingID"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateBoolean("IsPermanentID"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateLabel("MainFireUse"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateLabel("AncillaryFireUse"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateBoolean("SprinklerProtection"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateLabel("OccupancyType"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateArea("GrossPlannedArea"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateLabel("YearOfConstruction"));
            propertySetBuildingCommon.Entries.Add(PropertySetEntry.CreateBoolean("IsLandmarked"));

            if (fileVersion != IFCVersion.IFC2x2)
            {
                PropertySetEntry ifcPSE = PropertySetEntry.CreateInteger("NumberOfStoreys");
                ifcPSE.PropertyCalculator = NumberOfStoreysCalculator.Instance;
                propertySetBuildingCommon.Entries.Add(ifcPSE);
            }

            commonPropertySets.Add(propertySetBuildingCommon);
        }

        /// <summary>
        /// Initializes common level property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        /// <param name="fileVersion">The IFC file version.</param>
        private static void InitPropertySetLevelCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property level common
            PropertySetDescription propertySetLevelCommon = new PropertySetDescription();
            propertySetLevelCommon.Name = "Pset_BuildingStoreyCommon";
            propertySetLevelCommon.EntityTypes.Add(IFCEntityType.IfcBuildingStorey);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateBoolean("EntranceLevel");
            propertySetLevelCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("AboveGround");
            propertySetLevelCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("SprinklerProtection");
            propertySetLevelCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic");
            propertySetLevelCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("GrossAreaPlanned");
            propertySetLevelCommon.Entries.Add(ifcPSE);

            if (fileVersion != IFCVersion.IFC2x2)
            {
                ifcPSE = PropertySetEntry.CreateReal("NetAreaPlanned");
                propertySetLevelCommon.Entries.Add(ifcPSE);
            }

            commonPropertySets.Add(propertySetLevelCommon);
        }

        /// <summary>
        /// Initializes common site property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSiteCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property site common
            PropertySetDescription propertySetSiteCommon = new PropertySetDescription();
            propertySetSiteCommon.Name = "Pset_SiteCommon";
            propertySetSiteCommon.EntityTypes.Add(IFCEntityType.IfcSite);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("BuildableArea");
            propertySetSiteCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("BuildingHeightLimit");
            propertySetSiteCommon.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("GrossAreaPlanned");
            propertySetSiteCommon.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetSiteCommon);
        }

        /// <summary>
        /// Initializes common building element proxy property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBuildingElementProxyCommon(IList<PropertySetDescription> commonPropertySets)
        {
            //property building element proxy common
            PropertySetDescription propertySetBuildingElementProxyCommon = new PropertySetDescription();
            propertySetBuildingElementProxyCommon.Name = "Pset_BuildingElementProxyCommon";
            propertySetBuildingElementProxyCommon.EntityTypes.Add(IFCEntityType.IfcBuildingElementProxy);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetBuildingElementProxyCommon.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetBuildingElementProxyCommon);
        }

        /// <summary>
        /// Initializes common space property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceCommon(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            //property set space common
            PropertySetDescription propertySetSpaceCommon = new PropertySetDescription();
            propertySetSpaceCommon.Name = "Pset_SpaceCommon";

            propertySetSpaceCommon.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateIdentifier("Reference");
            ifcPSE.PropertyCalculator = ReferenceCalculator.Instance;
            propertySetSpaceCommon.Entries.Add(ifcPSE);

            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateLabel("OccupancyType"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateReal("OccupancyNumber"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateBoolean("PubliclyAccessible"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateBoolean("HandicapAccessible"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateBoolean("NaturalVentilation"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateReal("NaturalVentilationRate"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateReal("MechnicalVentilationRate"));
            propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateReal("GrossAreaPlanned"));

            if (fileVersion == IFCVersion.IFC2x2)
            {
                ifcPSE = PropertySetEntry.CreateBoolean("Concealed");
                ifcPSE.PropertyCalculator = SpaceConcealCalculator.Instance;
                propertySetSpaceCommon.Entries.Add(ifcPSE);
            }
            else
            {
                ifcPSE = PropertySetEntry.CreateLabel("CeilingCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_CEILING;
                propertySetSpaceCommon.Entries.Add(ifcPSE);

                ifcPSE = PropertySetEntry.CreateLabel("WallCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_WALL;
                propertySetSpaceCommon.Entries.Add(ifcPSE);

                ifcPSE = PropertySetEntry.CreateLabel("FloorCovering");
                ifcPSE.RevitBuiltInParameter = BuiltInParameter.ROOM_FINISH_FLOOR;
                propertySetSpaceCommon.Entries.Add(ifcPSE);

                propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateLabel("SkirtingBoard"));
                propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateBoolean("ConcealedCeiling"));
                propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateBoolean("ConcealedFlooring"));
                propertySetSpaceCommon.Entries.Add(PropertySetEntry.CreateReal("NetAreaPlanned"));
            }

            commonPropertySets.Add(propertySetSpaceCommon);
        }

        /// <summary>
        /// Initializes SpaceFireSafetyRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceFireSafetyRequirements(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceFireSafetyRequirements = new PropertySetDescription();
            propertySetSpaceFireSafetyRequirements.Name = "Pset_SpaceFireSafetyRequirements";

            propertySetSpaceFireSafetyRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateLabel("MainFireUse"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateLabel("AncillaryFireUse"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateLabel("FireRiskFactor"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateLabel("FireHazardFactor"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateBoolean("FlammableStorage"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateBoolean("FireExit"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateBoolean("SprinklerProtection"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateBoolean("SprinklerProtectionAutomatic"));
            propertySetSpaceFireSafetyRequirements.Entries.Add(PropertySetEntry.CreateBoolean("AirPressurization"));

            commonPropertySets.Add(propertySetSpaceFireSafetyRequirements);
        }

        /// <summary>
        /// Initializes SpaceLightingRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceLightingRequirements(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceLightingRequirements = new PropertySetDescription();
            propertySetSpaceLightingRequirements.Name = "Pset_SpaceLightingRequirements";

            propertySetSpaceLightingRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceLightingRequirements.Entries.Add(PropertySetEntry.CreateBoolean("ArtificialLighting"));
            propertySetSpaceLightingRequirements.Entries.Add(PropertySetEntry.CreateReal("Illuminance"));

            commonPropertySets.Add(propertySetSpaceLightingRequirements);
        }

        /// <summary>
        /// Initializes SpaceThermalRequirements property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceThermalRequirements(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            PropertySetDescription propertySetSpaceThermalRequirements = new PropertySetDescription();
            propertySetSpaceThermalRequirements.Name = "Pset_SpaceThermalRequirements";

            propertySetSpaceThermalRequirements.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureMax"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureMin"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceHumidity"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceHumiditySummer"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceHumidityWinter"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateBoolean("DiscontinuedHeating"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateBoolean("AirConditioning"));
            propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateBoolean("AirConditioningCentral"));

            if (fileVersion == IFCVersion.IFC2x2)
            {
                PropertySetEntry ifcPSE = PropertySetEntry.CreateBoolean("SpaceTemperatureSummer");
                ifcPSE.PropertyCalculator = new SpaceTemperatureCalculator("SpaceTemperatureSummer");
                propertySetSpaceThermalRequirements.Entries.Add(ifcPSE);

                ifcPSE = PropertySetEntry.CreateBoolean("SpaceTemperatureWinter");
                ifcPSE.PropertyCalculator = new SpaceTemperatureCalculator("SpaceTemperatureWinter");
                propertySetSpaceThermalRequirements.Entries.Add(ifcPSE);
            }
            else
            {
                propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureSummerMax"));
                propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureSummerMin"));
                propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureWinterMax"));
                propertySetSpaceThermalRequirements.Entries.Add(PropertySetEntry.CreateReal("SpaceTemperatureWinterMin"));
            }

            commonPropertySets.Add(propertySetSpaceThermalRequirements);
        }

        /// <summary>
        /// Initializes GSA Space Categories property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetGSASpaceCategories(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetGSASpaceCategories = new PropertySetDescription();
            propertySetGSASpaceCategories.Name = "GSA Space Categories";

            propertySetGSASpaceCategories.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetGSASpaceCategories.Entries.Add(PropertySetEntry.CreateLabel("GSA STAR Space Type"));
            propertySetGSASpaceCategories.Entries.Add(PropertySetEntry.CreateLabel("GSA STAR Space Category"));
            propertySetGSASpaceCategories.Entries.Add(PropertySetEntry.CreateLabel("ANSI/BOMA Space Category"));

            commonPropertySets.Add(propertySetGSASpaceCategories);
        }

        /// <summary>
        /// Initializes Space Occupant Properties sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceOccupant(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetSpaceOccupant = new PropertySetDescription();
            propertySetSpaceOccupant.Name = "Space Occupant Properties";

            propertySetSpaceOccupant.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceOccupant.Entries.Add(PropertySetEntry.CreateLabel("Occupant Organization Code"));
            propertySetSpaceOccupant.Entries.Add(PropertySetEntry.CreateLabel("Occupant Organization Abbreviation"));
            propertySetSpaceOccupant.Entries.Add(PropertySetEntry.CreateLabel("Occupant Organization Name"));
            propertySetSpaceOccupant.Entries.Add(PropertySetEntry.CreateLabel("Occupant Sub-Organization Code"));
            propertySetSpaceOccupant.Entries.Add(PropertySetEntry.CreateLabel("Occupant Billing ID"));

            commonPropertySets.Add(propertySetSpaceOccupant);
        }

        /// <summary>
        /// Initializes Space Zones property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetSpaceZones(IList<PropertySetDescription> commonPropertySets, IFCVersion fileVersion)
        {
            PropertySetDescription propertySetSpaceZones = new PropertySetDescription();
            propertySetSpaceZones.Name = "Space Zones";

            propertySetSpaceZones.EntityTypes.Add(IFCEntityType.IfcSpace);

            propertySetSpaceZones.Entries.Add(PropertySetEntry.CreateLabel("Security Zone"));
            propertySetSpaceZones.Entries.Add(PropertySetEntry.CreateLabel("Preservation Zone"));
            propertySetSpaceZones.Entries.Add(PropertySetEntry.CreateLabel("Privacy Zone"));
            if (fileVersion != IFCVersion.IFC2x2)
            {
                propertySetSpaceZones.Entries.Add(PropertySetEntry.CreateLabel("Zone GrossAreaPlanned"));
                propertySetSpaceZones.Entries.Add(PropertySetEntry.CreateLabel("Zone NetAreaPlanned"));
            }

            PropertySetEntry ifcPSE = PropertySetEntry.CreateListValue("Project Specific Zone", PropertyType.Label);
            ifcPSE.PropertyCalculator = SpecificZoneCalculator.Instance;
            ifcPSE.UseCalculatorOnly = true;
            propertySetSpaceZones.Entries.Add(ifcPSE);

            commonPropertySets.Add(propertySetSpaceZones);
        }

        /// <summary>
        /// Initializes building water storage property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetBuildingWaterStorage(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetBuildingWaterStorage = new PropertySetDescription();
            propertySetBuildingWaterStorage.Name = "Pset_BuildingWaterStorage";
            propertySetBuildingWaterStorage.EntityTypes.Add(IFCEntityType.IfcBuilding);

            propertySetBuildingWaterStorage.Entries.Add(PropertySetEntry.CreateReal("OneDayPotableWater"));
            propertySetBuildingWaterStorage.Entries.Add(PropertySetEntry.CreateReal("OneDayProcessOrProductionWater"));
            propertySetBuildingWaterStorage.Entries.Add(PropertySetEntry.CreateReal("OneDayCoolingTowerMakeupWater"));

            commonPropertySets.Add(propertySetBuildingWaterStorage);
        }

        /// <summary>
        /// Initializes element shading property sets.
        /// </summary>
        /// <param name="commonPropertySets">List to store property sets.</param>
        private static void InitPropertySetElementShading(IList<PropertySetDescription> commonPropertySets)
        {
            PropertySetDescription propertySetElementShading = new PropertySetDescription();
            propertySetElementShading.Name = "Pset_ElementShading";
            propertySetElementShading.EntityTypes.Add(IFCEntityType.IfcBuildingElementProxy);
            propertySetElementShading.ObjectType = "Solar Shade";

            propertySetElementShading.Entries.Add(PropertySetEntry.CreateEnumeratedValue("ShadingDeviceType", PropertyType.Label));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePlaneAngle("Azimuth"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePlaneAngle("Inclination"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePlaneAngle("TiltRange"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePositiveRatio("AverageSolarTransmittance"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePositiveRatio("AverageVisibleTransmittance"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePositiveRatio("Reflectance"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreatePositiveLength("Roughness"));
            propertySetElementShading.Entries.Add(PropertySetEntry.CreateLabel("Color"));

            commonPropertySets.Add(propertySetElementShading);
        }

        /// <summary>
        /// Initializes COBIE property sets.
        /// </summary>
        /// <param name="propertySets">List to store property sets.</param>
        private static void InitCOBIEPropertySets(IList<IList<PropertySetDescription>> propertySets)
        {
            IList<PropertySetDescription> cobiePSets = new List<PropertySetDescription>();
            InitCOBIEPSetSpaceThermalSimulationProperties(cobiePSets);
            InitCOBIEPSetSpaceThermalDesign(cobiePSets);
            InitCOBIEPSetSpaceVentilationCriteria(cobiePSets);
            InitCOBIEPSetBuildingEnergyTarget(cobiePSets);
            InitCOBIEPSetGlazingPropertiesEnergyAnalysis(cobiePSets);
            InitCOBIEPSetPhotovoltaicArray(cobiePSets);
            propertySets.Add(cobiePSets);
        }

        /// <summary>
        /// Initializes COBIE space thermal simulation property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceThermalSimulationProperties(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceThermalSimulationProperties = new PropertySetDescription();
            propertySetSpaceThermalSimulationProperties.Name = "ePset_SpaceThermalSimulationProperties";
            propertySetSpaceThermalSimulationProperties.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Space Thermal Simulation Type");
            ifcPSE.PropertyName = "SpaceThermalSimulationType";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Space Conditioning Requirement");
            ifcPSE.PropertyName = "SpaceConditioningRequirement";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Density");
            ifcPSE.PropertyName = "SpaceOccupantDensity";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Heat Rate");
            ifcPSE.PropertyName = "SpaceOccupantHeatRate";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Occupant Load");
            ifcPSE.PropertyName = "SpaceOccupantLoad";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Equipment Load");
            ifcPSE.PropertyName = "SpaceEquipmentLoad";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Space Lighting Load");
            ifcPSE.PropertyName = "SpaceLightingLoad";
            propertySetSpaceThermalSimulationProperties.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceThermalSimulationProperties);
        }

        /// <summary>
        /// Initializes COBIE space thermal design property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceThermalDesign(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceThermalDesign = new PropertySetDescription();
            propertySetSpaceThermalDesign.Name = "Pset_SpaceThermalDesign";
            propertySetSpaceThermalDesign.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateReal("Inside Dry Bulb Temperature - Heating");
            ifcPSE.PropertyName = "HeatingDryBulb";
            propertySetSpaceThermalDesign.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Relative Humidity - Heating");
            ifcPSE.PropertyName = "HeatingRelativeHumidity";
            propertySetSpaceThermalDesign.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Dry Bulb Temperature - Cooling");
            ifcPSE.PropertyName = "CoolingDryBulb";
            propertySetSpaceThermalDesign.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Relative Humidity - Cooling");
            ifcPSE.PropertyName = "CoolingRelativeHumidity";
            propertySetSpaceThermalDesign.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Inside Return Air Plenum");
            ifcPSE.PropertyName = "InsideReturnAirPlenum";
            propertySetSpaceThermalDesign.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceThermalDesign);
        }

        /// <summary>
        /// Initializes COBIE space ventilation criteria property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetSpaceVentilationCriteria(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetSpaceVentilationCriteria = new PropertySetDescription();
            propertySetSpaceVentilationCriteria.Name = "ePset_SpaceVentilationCriteria";
            propertySetSpaceVentilationCriteria.EntityTypes.Add(IFCEntityType.IfcSpace);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Ventilation Type");
            ifcPSE.PropertyName = "VentilationType";
            propertySetSpaceVentilationCriteria.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Outside Air Per Person");
            ifcPSE.PropertyName = "OutsideAirPerPerson";
            propertySetSpaceVentilationCriteria.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetSpaceVentilationCriteria);
        }

        /// <summary>
        /// Initializes COBIE building energy target property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetBuildingEnergyTarget(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetBuildingEnergyTarget = new PropertySetDescription();
            propertySetBuildingEnergyTarget.Name = "ePset_BuildingEnergyTarget";
            propertySetBuildingEnergyTarget.EntityTypes.Add(IFCEntityType.IfcBuilding);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateReal("Building Energy Target Value");
            ifcPSE.PropertyName = "BuildingEnergyTargetValue";
            propertySetBuildingEnergyTarget.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Building Energy Target Units");
            ifcPSE.PropertyName = "BuildingEnergyTargetUnits";
            propertySetBuildingEnergyTarget.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetBuildingEnergyTarget);
        }

        /// <summary>
        /// Initializes COBIE glazing properties energy analysis property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetGlazingPropertiesEnergyAnalysis(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetGlazingPropertiesEnergyAnalysis = new PropertySetDescription();
            propertySetGlazingPropertiesEnergyAnalysis.Name = "ePset_GlazingPropertiesEnergyAnalysis";
            propertySetGlazingPropertiesEnergyAnalysis.EntityTypes.Add(IFCEntityType.IfcCurtainWall);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateLabel("Windows 6 Glazing System Name");
            ifcPSE.PropertyName = "Windows6GlazingSystemName";
            propertySetGlazingPropertiesEnergyAnalysis.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetGlazingPropertiesEnergyAnalysis);
        }

        /// <summary>
        /// Initializes COBIE photo voltaic array property sets.
        /// </summary>
        /// <param name="cobiePropertySets">List to store property sets.</param>
        private static void InitCOBIEPSetPhotovoltaicArray(IList<PropertySetDescription> cobiePropertySets)
        {
            PropertySetDescription propertySetPhotovoltaicArray = new PropertySetDescription();
            propertySetPhotovoltaicArray.Name = "ePset_PhotovoltaicArray";
            propertySetPhotovoltaicArray.EntityTypes.Add(IFCEntityType.IfcRoof);
            propertySetPhotovoltaicArray.EntityTypes.Add(IFCEntityType.IfcWall);

            PropertySetEntry ifcPSE = PropertySetEntry.CreateBoolean("Hosts Photovoltaic Array");
            ifcPSE.PropertyName = "HostsPhotovoltaicArray";
            propertySetPhotovoltaicArray.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Active Area Ratio");
            ifcPSE.PropertyName = "ActiveAreaRatio";
            propertySetPhotovoltaicArray.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("DC to AC Conversion Efficiency");
            ifcPSE.PropertyName = "DCtoACConversionEfficiency";
            propertySetPhotovoltaicArray.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateLabel("Photovoltaic Surface Integration");
            ifcPSE.PropertyName = "PhotovoltaicSurfaceIntegration";
            propertySetPhotovoltaicArray.Entries.Add(ifcPSE);

            ifcPSE = PropertySetEntry.CreateReal("Photovoltaic Cell Efficiency");
            ifcPSE.PropertyName = "PhotovoltaicCellEfficiency";
            propertySetPhotovoltaicArray.Entries.Add(ifcPSE);

            cobiePropertySets.Add(propertySetPhotovoltaicArray);
        }

        // Quantities

        /// <summary>
        /// Initializes ceiling base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitCeilingBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcCeilingQuantity = new QuantityDescription();
            ifcCeilingQuantity.Name = "BaseQuantities";
            ifcCeilingQuantity.EntityTypes.Add(IFCEntityType.IfcCovering);

            QuantityEntry ifcQE = new QuantityEntry("GrossCeilingArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
            ifcCeilingQuantity.Entries.Add(ifcQE);

            baseQuantities.Add(ifcCeilingQuantity);
        }

        /// <summary>
        /// Initializes railing base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitRailingBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcRailingQuantity = new QuantityDescription();
            ifcRailingQuantity.Name = "BaseQuantities";
            ifcRailingQuantity.EntityTypes.Add(IFCEntityType.IfcRailing);

            QuantityEntry ifcQE = new QuantityEntry("Length");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;
            ifcRailingQuantity.Entries.Add(ifcQE);

            baseQuantities.Add(ifcRailingQuantity);
        }

        /// <summary>
        /// Initializes slab base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitSlabBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcSlabQuantity = new QuantityDescription();
            ifcSlabQuantity.Name = "BaseQuantities";
            ifcSlabQuantity.EntityTypes.Add(IFCEntityType.IfcSlab);

            QuantityEntry ifcQE = new QuantityEntry("GrossArea");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SlabGrossAreaCalculator.Instance;
            ifcSlabQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("GrossVolume");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SlabGrossVolumeCalculator.Instance;
            ifcSlabQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("Perimeter");
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SlabPerimeterCalculator.Instance;
            ifcSlabQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SlabWidthCalculator.Instance;
            ifcSlabQuantity.Entries.Add(ifcQE);

            baseQuantities.Add(ifcSlabQuantity);
        }

        /// <summary>
        /// Initializes ramp flight base quantities.
        /// </summary>
        /// <param name="baseQuantities">List to store quantities.</param>
        private static void InitRampFlightBaseQuantities(IList<QuantityDescription> baseQuantities)
        {
            QuantityDescription ifcBaseQuantity = new QuantityDescription();
            ifcBaseQuantity.Name = "BaseQuantities";
            ifcBaseQuantity.EntityTypes.Add(IFCEntityType.IfcRampFlight);

            QuantityEntry ifcQE = new QuantityEntry("Width");
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.RevitBuiltInParameter = BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH;
            ifcBaseQuantity.Entries.Add(ifcQE);

            baseQuantities.Add(ifcBaseQuantity);
        }
        
        /// <summary>
        /// Initializes base quantities.
        /// </summary>
        /// <param name="quantities">List to store quantities.</param>
        private static void InitBaseQuantities(IList<IList<QuantityDescription>> quantities)
        {
            IList<QuantityDescription> baseQuantities = new List<QuantityDescription>();
            InitCeilingBaseQuantities(baseQuantities);
            InitRailingBaseQuantities(baseQuantities);
            InitSlabBaseQuantities(baseQuantities);
            InitRampFlightBaseQuantities(baseQuantities);
            quantities.Add(baseQuantities);
        }

        /// <summary>
        /// Initializes COBIE quantities.
        /// </summary>
        /// <param name="quantities">List to store quantities.</param>
        private static void InitCOBIEQuantities(IList<IList<QuantityDescription>> quantities)
        {
            IList<QuantityDescription> cobieQuantities = new List<QuantityDescription>();
            InitCOBIESpaceQuantities(cobieQuantities);
            InitCOBIESpaceLevelQuantities(cobieQuantities);
            InitCOBIEPMSpaceQuantities(cobieQuantities);
            quantities.Add(cobieQuantities);
        }

        /// <summary>
        /// Initializes COBIE space quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIESpaceQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "BaseQuantities";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);

            QuantityEntry ifcQE = new QuantityEntry("Height");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpaceHeightCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("GrossPerimeter");
            ifcQE.MethodOfMeasurement = "length measured in geometry";
            ifcQE.QuantityType = QuantityType.PositiveLength;
            ifcQE.PropertyCalculator = SpacePerimeterCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("GrossFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("NetFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            ifcQE = new QuantityEntry("GrossVolume");
            ifcQE.MethodOfMeasurement = "volume measured in geometry";
            ifcQE.QuantityType = QuantityType.Volume;
            ifcQE.PropertyCalculator = SpaceVolumeCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }

        /// <summary>
        /// Initializes COBIE space level quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIESpaceLevelQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "BaseQuantities";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);
            ifcCOBIEQuantity.DescriptionCalculator = SpaceLevelDescriptionCalculator.Instance;

            QuantityEntry ifcQE = new QuantityEntry("GrossFloorArea");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceLevelAreaCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }

        /// <summary>
        /// Initializes COBIE BM space quantities.
        /// </summary>
        /// <param name="cobieQuantities">List to store quantities.</param>
        private static void InitCOBIEPMSpaceQuantities(IList<QuantityDescription> cobieQuantities)
        {
            QuantityDescription ifcCOBIEQuantity = new QuantityDescription();
            ifcCOBIEQuantity.Name = "Space Quantities (Property Management)";
            ifcCOBIEQuantity.MethodOfMeasurement = "As defined by BOMA (see www.boma.org)";
            ifcCOBIEQuantity.EntityTypes.Add(IFCEntityType.IfcSpace);

            QuantityEntry ifcQE = new QuantityEntry("NetFloorArea_BOMA");
            ifcQE.MethodOfMeasurement = "area measured in geometry";
            ifcQE.QuantityType = QuantityType.Area;
            ifcQE.PropertyCalculator = SpaceAreaCalculator.Instance;
            ifcCOBIEQuantity.Entries.Add(ifcQE);

            cobieQuantities.Add(ifcCOBIEQuantity);
        }
    }
}
