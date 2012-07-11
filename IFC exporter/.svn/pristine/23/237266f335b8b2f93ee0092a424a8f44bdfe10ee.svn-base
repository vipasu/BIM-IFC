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

namespace BIM.IFC.Utility
{
    /// <summary>
    /// IFC export type.
    /// </summary>
    enum IFCExportType
    {
        /// <summary>
        /// Don't export.
        /// </summary>
        DontExport,
        /// <summary>
        /// Annotation.
        /// </summary>
        ExportAnnotation,
        /// <summary>
        /// Beam.
        /// </summary>
        ExportBeam,
        /// <summary>
        /// Building element part.
        /// </summary>
        ExportBuildingElementPart,
        /// <summary>
        /// Building element proxy.
        /// </summary>
        ExportBuildingElementProxy,
        /// <summary>
        /// Building storey.
        /// </summary>
        ExportBuildingStorey,
        /// <summary>
        /// Column type.
        /// </summary>
        ExportColumnType,
        /// <summary>
        /// Covering.
        /// </summary>
        ExportCovering,
        /// <summary>
        /// Curtain wall.
        /// </summary>
        ExportCurtainWall,
        /// <summary>
        /// Door type.
        /// </summary>
        ExportDoorType,
        /// <summary>
        /// Assembly.
        /// </summary>
        ExportElementAssembly,
        /// <summary>
        /// Footing.
        /// </summary>
        ExportFooting,
        /// <summary>
        /// Member type.
        /// </summary>
        ExportMemberType,
        /// <summary>
        /// Opening element.
        /// </summary>
        ExportOpeningElement,
        /// <summary>
        /// Plate type.
        /// </summary>
        ExportPlateType,
        /// <summary>
        /// Railing.
        /// </summary>
        ExportRailing,
        /// <summary>
        /// Ramp.
        /// </summary>
        ExportRamp,
        /// <summary>
        /// Roof.
        /// </summary>
        ExportRoof,
        /// <summary>
        /// Site.
        /// </summary>
        ExportSite,
        /// <summary>
        /// Slab.
        /// </summary>
        ExportSlab,
        /// <summary>
        /// Space.
        /// </summary>
        ExportSpace,
        /// <summary>
        /// Stair.
        /// </summary>
        ExportStair,
        /// <summary>
        /// Transport element type.
        /// </summary>
        ExportTransportElementType,
        /// <summary>
        /// Wall.
        /// </summary>
        ExportWall,
        /// <summary>
        /// Reinforcing bar.
        /// </summary>
        ExportReinforcingBar,
        /// <summary>
        /// Reinforcing mesh.
        /// </summary>
        ExportReinforcingMesh,
        /// <summary>
        /// Window type.
        /// </summary>
        ExportWindowType,
        /// <summary>
        /// Furnishing element and their subclasses.
        /// </summary>
        ExportFurnishingElement,
        /// <summary>
        /// Direct subclass of FurnishingElementType.
        /// </summary>
        ExportFurnitureType,
        /// <summary>
        /// System furniture element type.
        /// </summary>
        ExportSystemFurnitureElementType,
        /// <summary>
        /// Distribution elements and their subclasses.
        /// </summary>
        ExportDistributionElement,
        /// <summary>
        /// Direct subclasses of DistributionElement.
        /// </summary>
        ExportDistributionControlElement,
        /// <summary>
        /// Distribution flow element.
        /// </summary>
        ExportDistributionFlowElement,
        /// <summary>
        /// Direct subclass of DistributionFlowElement.
        /// </summary>
        ExportDistributionChamberElementType,
        /// <summary>
        /// Energy conversion device.
        /// </summary>
        ExportEnergyConversionDevice,
        /// <summary>
        /// Flow fitting.
        /// </summary>
        ExportFlowFitting,
        /// <summary>
        /// Flow moving device.
        /// </summary>
        ExportFlowMovingDevice,
        /// <summary>
        /// Flow segment.
        /// </summary>
        ExportFlowSegment,
        /// <summary>
        /// Flow storage device.
        /// </summary>
        ExportFlowStorageDevice,
        /// <summary>
        /// Flow terminal.
        /// </summary>
        ExportFlowTerminal,
        /// <summary>
        /// Flow treatment device.
        /// </summary>
        ExportFlowTreatmentDevice,
        /// <summary>
        /// Flow controller.
        /// </summary>
        ExportFlowController,
        // direct subclass of FlowController
        //not suported -- ExportElectricDistributionPointType,
        // types of DistributionControlElementType
        /// <summary>
        /// Actuator type.
        /// </summary>
        ExportActuatorType,
        /// <summary>
        /// Alarm type.
        /// </summary>
        ExportAlarmType,
        /// <summary>
        /// Controller type.
        /// </summary>
        ExportControllerType,
        /// <summary>
        /// Flow instrument type.
        /// </summary>
        ExportFlowInstrumentType,
        /// <summary>
        /// Sensor type.
        /// </summary>
        ExportSensorType,
        // types of EnergyConversionDeviceType
        /// <summary>
        /// Air to air heat recovery type.
        /// </summary>
        ExportAirToAirHeatRecoveryType,
        /// <summary>
        /// Boiler type.
        /// </summary>
        ExportBoilerType,
        /// <summary>
        /// Chiller type.
        /// </summary>
        ExportChillerType,
        /// <summary>
        /// Coil type.
        /// </summary>
        ExportCoilType,
        /// <summary>
        /// Condenser type.
        /// </summary>
        ExportCondenserType,
        /// <summary>
        /// Cooled beam type.
        /// </summary>
        ExportCooledBeamType,
        /// <summary>
        /// Cooling tower type.
        /// </summary>
        ExportCoolingTowerType,
        /// <summary>
        /// Electric generator type.
        /// </summary>
        ExportElectricGeneratorType,
        /// <summary>
        /// Electric motor type.
        /// </summary>
        ExportElectricMotorType,
        /// <summary>
        /// Evaporative cooler type.
        /// </summary>
        ExportEvaporativeCoolerType,
        /// <summary>
        /// Evaporator type.
        /// </summary>
        ExportEvaporatorType,
        /// <summary>
        /// Heat exchanger type.
        /// </summary>
        ExportHeatExchangerType,
        /// <summary>
        /// Humidifier type.
        /// </summary>
        ExportHumidifierType,
        /// <summary>
        /// Motor connection type.
        /// </summary>
        ExportMotorConnectionType,
        /// <summary>
        /// Space heater type.
        /// </summary>
        ExportSpaceHeaterType,
        /// <summary>
        /// Transformer type.
        /// </summary>
        ExportTransformerType,
        /// <summary>
        /// Tube bundle type.
        /// </summary>
        ExportTubeBundleType,
        /// <summary>
        /// Unitary equipment type.
        /// </summary>
        ExportUnitaryEquipmentType,
        // types of FlowControllerType
        /// <summary>
        /// Air terminal box type.
        /// </summary>
        ExportAirTerminalBoxType,
        /// <summary>
        /// Damper type.
        /// </summary>
        ExportDamperType,
        /// <summary>
        /// Electric time control type.
        /// </summary>
        ExportElectricTimeControlType,
        /// <summary>
        /// Flow meter type.
        /// </summary>
        ExportFlowMeterType,
        /// <summary>
        /// Protective device type.
        /// </summary>
        ExportProtectiveDeviceType,
        /// <summary>
        /// Switching device type.
        /// </summary>
        ExportSwitchingDeviceType,
        /// <summary>
        /// Valve type.
        /// </summary>
        ExportValveType,
        // types of FlowFittingType
        /// <summary>
        /// Cable carrier fitting type.
        /// </summary>
        ExportCableCarrierFittingType,
        /// <summary>
        /// Duct fitting type.
        /// </summary>
        ExportDuctFittingType,
        /// <summary>
        /// Junction box type.
        /// </summary>
        ExportJunctionBoxType,
        /// <summary>
        /// Pipe fitting type.
        /// </summary>
        ExportPipeFittingType,
        // types of FlowMovingDeviceType
        /// <summary>
        /// Compressor type.
        /// </summary>
        ExportCompressorType,
        /// <summary>
        /// Fan type.
        /// </summary>
        ExportFanType,
        /// <summary>
        /// Pump type.
        /// </summary>
        ExportPumpType,
        // types of FlowSegmentType
        /// <summary>
        /// Cable carrier segment type.
        /// </summary>
        ExportCableCarrierSegmentType,
        /// <summary>
        /// Cable segment type.
        /// </summary>
        ExportCableSegmentType,
        /// <summary>
        /// Duct segment type.
        /// </summary>
        ExportDuctSegmentType,
        /// <summary>
        /// Pipe segment type.
        /// </summary>
        ExportPipeSegmentType,
        // types of FlowStorageDeviceType
        /// <summary>
        /// Electric flow storage device type.
        /// </summary>
        ExportElectricFlowStorageDeviceType,
        /// <summary>
        /// Tank type.
        /// </summary>
        ExportTankType,
        // types of FlowTreatmentDeviceType
        /// <summary>
        /// Duct silencer type.
        /// </summary>
        ExportDuctSilencerType,
        /// <summary>
        /// Filter type.
        /// </summary>
        ExportFilterType,
        // types of FlowTerminalType
        /// <summary>
        /// Air terminal type.
        /// </summary>
        ExportAirTerminalType,
        /// <summary>
        /// Electric appliance type.
        /// </summary>
        ExportElectricApplianceType,
        /// <summary>
        /// Electric heater type.
        /// </summary>
        ExportElectricHeaterType,
        /// <summary>
        /// Fire suppression terminal type.
        /// </summary>
        ExportFireSuppressionTerminalType,
        /// <summary>
        /// Gas terminal type.
        /// </summary>
        ExportGasTerminalType,
        /// <summary>
        /// Lamp type.
        /// </summary>
        ExportLampType,
        /// <summary>
        /// Light fixture type.
        /// </summary>
        ExportLightFixtureType,
        /// <summary>
        /// Outlet type.
        /// </summary>
        ExportOutletType,
        /// <summary>
        /// Sanitary terminal type.
        /// </summary>
        ExportSanitaryTerminalType,
        /// <summary>
        /// Stack terminal type.
        /// </summary>
        ExportStackTerminalType,
        /// <summary>
        /// Waste terminal type.
        /// </summary>
        ExportWasteTerminalType,
    }


    /// <summary>
    /// Provides static methods for filtering elements.
    /// </summary>
    class ElementFilteringUtil
    {
        /// <summary>
        /// Gets spatial element filter.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="filterView">The view for current view export.</param>
        /// <returns>The element filter.</returns>
        public static ElementFilter GetSpatialElementFilter(Document document, ExporterIFC exporterIFC, Element filterView)
        {
            return GetExportFilter(document, exporterIFC, filterView, true);
        }

        /// <summary>
        /// Gets filter for non spatial elements.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="filterView">The view for current view export.</param>
        /// <returns>The Element filter.</returns>
        public static ElementFilter GetNonSpatialElementFilter(Document document, ExporterIFC exporterIFC, Element filterView)
        {
            return GetExportFilter(document, exporterIFC, filterView, false);
        }

        /// <summary>
        /// Gets element filter for export.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="filterView">The view for current view export.</param>
        /// <param name="forSpatialElements">True to get spatial element filter, false for non spatial elements filter.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetExportFilter(Document document, ExporterIFC exporterIFC, Element filterView, 
            bool forSpatialElements)
        {
            List<ElementFilter> filters = new List<ElementFilter>();

            // Class types & categories
            ElementFilter classFilter = GetClassFilter(forSpatialElements);

            // Special handling for family instances and view specific elements
            if (!forSpatialElements)
            {
                ElementFilter familyInstanceFilter = GetFamilyInstanceFilter(exporterIFC);

                List<ElementFilter> classFilters = new List<ElementFilter>();
                classFilters.Add(classFilter);
                classFilters.Add(familyInstanceFilter);

                if (ExporterCacheManager.ExportOptionsCache.ExportAnnotations)
                {
                    ElementFilter ownerViewFilter = GetViewSpecificTypesFilter(exporterIFC);
                    classFilters.Add(ownerViewFilter);
                }

                classFilter = new LogicalOrFilter(classFilters);
            }

            filters.Add(classFilter);

            // Design options
            filters.Add(GetDesignOptionFilter());

            // Phases
            filters.Add(GetPhaseStatusFilter(document, filterView));

            return new LogicalAndFilter(filters);
        }

        /// <summary>
        /// Gets element filter for family instance.
        /// </summary>
        /// <param name="exporter">The ExporterIFC object.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetFamilyInstanceFilter(ExporterIFC exporter)
        {
            List<ElementFilter> filters = new List<ElementFilter>();
            filters.Add(new ElementOwnerViewFilter(ElementId.InvalidElementId));
            filters.Add(new ElementClassFilter(typeof(FamilyInstance)));
            LogicalAndFilter andFilter = new LogicalAndFilter(filters);

            return andFilter;
        }

        /// <summary>
        /// Gets element filter meeting design option requirements.
        /// </summary>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetDesignOptionFilter()
        {
            ElementFilter designOptionFilter = new ElementDesignOptionFilter(ElementId.InvalidElementId);
            ElementFilter primaryOptionsFilter = new PrimaryDesignOptionMemberFilter();
            return new LogicalOrFilter(designOptionFilter, primaryOptionsFilter);
        }

        // Cannot be implemented until ExportLayerTable can be read.  Replacement is ShouldCategoryBeExported()
        /*private static ElementFilter GetCategoryFilter()
        {
            
        }*/

        /// <summary>
        /// Checks if element in certain category should be exported.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <returns>True if the element should be exported, false otherwise.</returns>
        public static bool ShouldCategoryBeExported(ExporterIFC exporterIFC, Element element)
        {
            IFCExportType exportType = IFCExportType.DontExport;
            String ifcClassName = ExporterIFCUtils.GetIFCClassName(element, exporterIFC);
            if (ifcClassName == "")
                return false;

            bool foundName = String.Compare(ifcClassName, "Default", true) != 0;
            if (foundName)
                exportType = GetExportTypeFromClassName(ifcClassName);
            // We don't export openings directly, only via the element they are opening.
            if (!foundName || (exportType != IFCExportType.DontExport && exportType != IFCExportType.ExportOpeningElement))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if name is equal to base or its type name.
        /// </summary>
        /// <param name="name">The object type name.</param>
        /// <param name="baseName">The IFC base name.</param>
        /// <returns>True if equal, false otherwise.</returns>
        private static bool IsEqualToTypeName(String name, String baseName)
        {
            if (String.Compare(name, baseName, true) == 0)
                return true;

            String typeName = baseName + "Type";
            return (String.Compare(name, typeName, true) == 0);
        }

        /// <summary>
        /// Gets export type from IFC class name.
        /// </summary>
        /// <param name="ifcClassName">The IFC class name.</param>
        /// <returns>The export type.</returns>
        public static IFCExportType GetExportTypeFromClassName(String ifcClassName)
        {
            if (String.Compare(ifcClassName, "IfcAnnotation", true) == 0)
            {
                // Used to mark curves, text, and filled regions for export.
                return IFCExportType.ExportAnnotation;
            }
            else if (String.Compare(ifcClassName, "IfcBeam", true) == 0)
                return IFCExportType.ExportBeam;
            else if (String.Compare(ifcClassName, "IfcBuildingElementPart", true) == 0)
                return IFCExportType.ExportBuildingElementPart;
            else if (String.Compare(ifcClassName, "IfcBuildingElementProxy", true) == 0)
                return IFCExportType.ExportBuildingElementProxy;
            else if (String.Compare(ifcClassName, "IfcBuildingStorey", true) == 0)
                return IFCExportType.ExportBuildingStorey;  // Only to be used for exporting level information!
            else if (IsEqualToTypeName(ifcClassName, ("IfcColumn")))
                return IFCExportType.ExportColumnType;
            else if (String.Compare(ifcClassName, "IfcCovering", true) == 0)
                return IFCExportType.ExportCovering;
            else if (String.Compare(ifcClassName, "IfcCurtainWall", true) == 0)
                return IFCExportType.ExportCurtainWall;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDoor")))
                return IFCExportType.ExportDoorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElementAssembly")))
                return IFCExportType.ExportElementAssembly;
            else if (String.Compare(ifcClassName, "IfcFloor", true) == 0)
            {
                // IfcFloor is not a real type, but is being kept for backwards compatibility as a typo.
                return IFCExportType.ExportSlab;
            }
            else if (IsEqualToTypeName(ifcClassName, ("IfcFooting")))
                return IFCExportType.ExportFooting;
            else if (String.Compare(ifcClassName, "IfcMember", true) == 0)
                return IFCExportType.ExportMemberType;
            else if (String.Compare(ifcClassName, "IfcOpeningElement", true) == 0)
            {
                // Used to mark reveals for export.
                return IFCExportType.ExportOpeningElement;
            }
            else if (String.Compare(ifcClassName, "IfcPlate", true) == 0)
                return IFCExportType.ExportPlateType;
            else if (String.Compare(ifcClassName, "IfcRailing", true) == 0)
                return IFCExportType.ExportRailing;
            else if (String.Compare(ifcClassName, "IfcRamp", true) == 0)
                return IFCExportType.ExportRamp;
            else if (String.Compare(ifcClassName, "IfcRoof", true) == 0)
                return IFCExportType.ExportRoof;
            else if (String.Compare(ifcClassName, "IfcSite", true) == 0)
                return IFCExportType.ExportSite;
            else if (String.Compare(ifcClassName, "IfcSlab", true) == 0)
                return IFCExportType.ExportSlab;
            else if (String.Compare(ifcClassName, "IfcSpace", true) == 0)
            {
                // Not a real type; used to mark rooms for export.
                return IFCExportType.ExportSpace;
            }
            else if (String.Compare(ifcClassName, "IfcStair", true) == 0)
                return IFCExportType.ExportStair;
            else if (IsEqualToTypeName(ifcClassName, ("IfcTransportElement")))
                return IFCExportType.ExportTransportElementType;
            else if ((String.Compare(ifcClassName, "IfcWall", true) == 0) ||
                     (String.Compare(ifcClassName, "IfcWallStandardCase", true) == 0))
                return IFCExportType.ExportWall;
            else if (IsEqualToTypeName(ifcClassName, ("IfcWindow")))
                return IFCExportType.ExportWindowType;
            // furnishing type(s) to be exported as geometry, not mapped item
            else if (IsEqualToTypeName(ifcClassName, ("IfcFurnishingElement")))
                return IFCExportType.ExportFurnishingElement;
            // furnishing types to be exported as mapped item
            else if (IsEqualToTypeName(ifcClassName, ("IfcFurniture")))
                return IFCExportType.ExportFurnitureType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcSystemFurnitureElement")))
                return IFCExportType.ExportSystemFurnitureElementType;
            // distribution types to be exported as geometry, not mapped item
            else if (IsEqualToTypeName(ifcClassName, ("IfcDistributionElement")))
                return IFCExportType.ExportDistributionElement;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDistributionControlElement")))
                return IFCExportType.ExportDistributionControlElement;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDistributionFlowElement")))
                return IFCExportType.ExportDistributionFlowElement;
            else if (IsEqualToTypeName(ifcClassName, ("IfcEnergyConversionDevice")))
                return IFCExportType.ExportEnergyConversionDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowFitting")))
                return IFCExportType.ExportFlowFitting;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowMovingDevice")))
                return IFCExportType.ExportFlowMovingDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowSegment")))
                return IFCExportType.ExportFlowSegment;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowSegmentStorageDevice")))
                return IFCExportType.ExportFlowStorageDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowTerminal")))
                return IFCExportType.ExportFlowTerminal;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowTreatmentDevice")))
                return IFCExportType.ExportFlowTreatmentDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowController")))
                return IFCExportType.ExportFlowController;
            // distribution types to be exported as mapped item
            else if (IsEqualToTypeName(ifcClassName, ("IfcActuator")))
                return IFCExportType.ExportActuatorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcAirTerminalBox")))
                return IFCExportType.ExportAirTerminalBoxType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcAirTerminal")))
                return IFCExportType.ExportAirTerminalType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcAirToAirHeatRecovery")))
                return IFCExportType.ExportAirToAirHeatRecoveryType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcAlarm")))
                return IFCExportType.ExportAlarmType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcBoiler")))
                return IFCExportType.ExportBoilerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCableCarrierFitting")))
                return IFCExportType.ExportCableCarrierFittingType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCableCarrierSegment")))
                return IFCExportType.ExportCableCarrierSegmentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCableSegment")))
                return IFCExportType.ExportCableSegmentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcChiller")))
                return IFCExportType.ExportChillerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCoil")))
                return IFCExportType.ExportCoilType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCompressor")))
                return IFCExportType.ExportCompressorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCondenser")))
                return IFCExportType.ExportCondenserType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcController")))
                return IFCExportType.ExportControllerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCooledBeam")))
                return IFCExportType.ExportCooledBeamType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcCoolingTower")))
                return IFCExportType.ExportCoolingTowerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDamper")))
                return IFCExportType.ExportDamperType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDistributionChamberElement")))
                return IFCExportType.ExportDistributionChamberElementType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDuctFitting")))
                return IFCExportType.ExportDuctFittingType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDuctSegment")))
                return IFCExportType.ExportDuctSegmentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcDuctSilencer")))
                return IFCExportType.ExportDuctSilencerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricAppliance")))
                return IFCExportType.ExportElectricApplianceType;
            //else if (IsEqualToTypeName(ifcClassName, ("IfcElectricDistributionPoint")))
            //return IFCExportType.ExportElectricDistributionPointType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricFlowStorageDevice")))
                return IFCExportType.ExportElectricFlowStorageDeviceType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricGenerator")))
                return IFCExportType.ExportElectricGeneratorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricHeater")))
                return IFCExportType.ExportElectricHeaterType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricMotor")))
                return IFCExportType.ExportElectricMotorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcElectricTimeControl")))
                return IFCExportType.ExportElectricTimeControlType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcEnergyConversionDevice")))
                return IFCExportType.ExportEnergyConversionDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcEvaporativeCooler")))
                return IFCExportType.ExportEvaporativeCoolerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcEvaporator")))
                return IFCExportType.ExportEvaporatorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFan")))
                return IFCExportType.ExportFanType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFilter")))
                return IFCExportType.ExportFilterType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFireSuppressionTerminal")))
                return IFCExportType.ExportFireSuppressionTerminalType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowController")))
                return IFCExportType.ExportFlowController;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowFitting")))
                return IFCExportType.ExportFlowFitting;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowInstrument")))
                return IFCExportType.ExportFlowInstrumentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowMeter")))
                return IFCExportType.ExportFlowMeterType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowMovingDevice")))
                return IFCExportType.ExportFlowMovingDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowSegment")))
                return IFCExportType.ExportFlowSegment;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowStorageDevice")))
                return IFCExportType.ExportFlowStorageDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowTerminal")))
                return IFCExportType.ExportFlowTerminal;
            else if (IsEqualToTypeName(ifcClassName, ("IfcFlowTreatmentDevice")))
                return IFCExportType.ExportFlowTreatmentDevice;
            else if (IsEqualToTypeName(ifcClassName, ("IfcGasTerminal")))
                return IFCExportType.ExportGasTerminalType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcHeatExchanger")))
                return IFCExportType.ExportHeatExchangerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcHumidifier")))
                return IFCExportType.ExportHumidifierType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcJunctionBox")))
                return IFCExportType.ExportJunctionBoxType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcLamp")))
                return IFCExportType.ExportLampType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcLightFixture")))
                return IFCExportType.ExportLightFixtureType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcMotorConnection")))
                return IFCExportType.ExportMotorConnectionType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcOutlet")))
                return IFCExportType.ExportOutletType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcPipeFitting")))
                return IFCExportType.ExportPipeFittingType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcPipeSegment")))
                return IFCExportType.ExportPipeSegmentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcProtectiveDevice")))
                return IFCExportType.ExportProtectiveDeviceType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcPump")))
                return IFCExportType.ExportPumpType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcReinforcingBar")))
                return IFCExportType.ExportReinforcingBar;
            else if (IsEqualToTypeName(ifcClassName, ("IfcReinforcingMesh")))
                return IFCExportType.ExportReinforcingMesh;
            else if (IsEqualToTypeName(ifcClassName, ("IfcSanitaryTerminal")))
                return IFCExportType.ExportSanitaryTerminalType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcSensor")))
                return IFCExportType.ExportSensorType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcSpaceHeater")))
                return IFCExportType.ExportSpaceHeaterType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcStackTerminal")))
                return IFCExportType.ExportStackTerminalType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcSwitchingDevice")))
                return IFCExportType.ExportSwitchingDeviceType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcTank")))
                return IFCExportType.ExportTankType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcTransformer")))
                return IFCExportType.ExportTransformerType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcTubeBundle")))
                return IFCExportType.ExportTubeBundleType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcUnitaryEquipment")))
                return IFCExportType.ExportUnitaryEquipmentType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcValve")))
                return IFCExportType.ExportValveType;
            else if (IsEqualToTypeName(ifcClassName, ("IfcWasteTerminal")))
                return IFCExportType.ExportWasteTerminalType;
            else if (ifcClassName.StartsWith("Ifc"))
            {
                // This used to throw an exception, but this could abort export if the user enters a bad IFC class name
                // in the ExportLayerOptions table.  In the future, we should log this.
                //throw new Exception("IFC: Unknown IFC type in getExportTypeFromClassName: " + ifcClassName);
                return IFCExportType.ExportBuildingElementProxy;
            }

            return IFCExportType.DontExport;
        }

        // TODO: implement  out bool exportSeparately
        /// <summary>
        /// Gets export type from category id.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <param name="ifcEnumType">The string value represents the IFC type.</param>
        /// <returns>The export type.</returns>
        public static IFCExportType GetExportTypeFromCategoryId(ElementId categoryId, out string ifcEnumType /*, out bool exportSeparately*/)
        {
            ifcEnumType = "";
            //exportSeparately = true;

            if (categoryId == new ElementId(BuiltInCategory.OST_Cornices))
                return IFCExportType.ExportBeam;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Ceilings))
                return IFCExportType.ExportCovering;
            else if (categoryId == new ElementId(BuiltInCategory.OST_CurtainWallPanels))
            {
                ifcEnumType = "CURTAIN_PANEL";
                //exportSeparately = false;
                return IFCExportType.ExportPlateType;
            }
            else if (categoryId == new ElementId(BuiltInCategory.OST_Doors))
                return IFCExportType.ExportDoorType;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Furniture))
                return IFCExportType.ExportFurnitureType;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Floors))
            {
                ifcEnumType = "FLOOR";
                return IFCExportType.ExportSlab;
            }
            else if (categoryId == new ElementId(BuiltInCategory.OST_Mass))
                return IFCExportType.ExportBuildingElementProxy;
            else if (categoryId == new ElementId(BuiltInCategory.OST_CurtainWallMullions))
            {
                ifcEnumType = "MULLION";
                //exportSeparately = false;
                return IFCExportType.ExportMemberType;
            }
            else if (categoryId == new ElementId(BuiltInCategory.OST_Railings))
                return IFCExportType.ExportRailing;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Ramps))
                return IFCExportType.ExportRamp;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Roofs))
                return IFCExportType.ExportRoof;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Site))
                return IFCExportType.ExportSite;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Stairs))
                return IFCExportType.ExportStair;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Walls))
                return IFCExportType.ExportWall;
            else if (categoryId == new ElementId(BuiltInCategory.OST_Windows))
                return IFCExportType.ExportWindowType;

            return IFCExportType.DontExport;
        }

        /// <summary>
        /// Gets element filter for specific views.
        /// </summary>
        /// <param name="exporter">The ExporterIFC object.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetViewSpecificTypesFilter(ExporterIFC exporter)
        {
            ElementFilter ownerViewFilter = GetOwnerViewFilter(exporter);

            List<Type> viewSpecificTypes = new List<Type>();
            viewSpecificTypes.Add(typeof(TextNote));
            viewSpecificTypes.Add(typeof(FilledRegion));
            ElementMulticlassFilter classFilter = new ElementMulticlassFilter(viewSpecificTypes);


            LogicalAndFilter viewSpecificTypesFilter = new LogicalAndFilter(ownerViewFilter, classFilter);
            return viewSpecificTypesFilter;
        }

        /// <summary>
        /// Gets element filter to match elements which are owned by a particular view.
        /// </summary>
        /// <param name="exporter">The exporter.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetOwnerViewFilter(ExporterIFC exporter)
        {
            List<ElementFilter> filters = new List<ElementFilter>();
            ICollection<ElementId> viewIds = exporter.GetViewIdsToExport();
            foreach (ElementId id in viewIds)
            {
                filters.Add(new ElementOwnerViewFilter(id));
            }
            filters.Add(new ElementOwnerViewFilter(ElementId.InvalidElementId));
            LogicalOrFilter viewFilters = new LogicalOrFilter(filters);

            return viewFilters;
        }

        /// <summary>
        /// Gets element filter that match certain types.
        /// </summary>
        /// <param name="forSpatialElements">True if to get filter for spatial element, false for other elements.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetClassFilter(bool forSpatialElements)
        {
            if (forSpatialElements)
            {
                return new ElementClassFilter(typeof(SpatialElement));
            }
            else
            {
                List<Type> excludedTypes = new List<Type>();

                // FamilyInstances are handled in separate filter.
                excludedTypes.Add(typeof(FamilyInstance));

                excludedTypes.Add(typeof(SpatialElement));

                if (!ExporterCacheManager.ExportOptionsCache.ExportAnnotations)
                    excludedTypes.Add(typeof(CurveElement));

                excludedTypes.Add(typeof(ElementType));
                excludedTypes.Add(typeof(Group));

                excludedTypes.Add(typeof(BaseArray));

                excludedTypes.Add(typeof(FillPatternElement));
                excludedTypes.Add(typeof(LinePatternElement));
                excludedTypes.Add(typeof(Material));
                excludedTypes.Add(typeof(GraphicsStyle));
                excludedTypes.Add(typeof(Family));
                excludedTypes.Add(typeof(SketchPlane));
                excludedTypes.Add(typeof(View));
                excludedTypes.Add(typeof(Autodesk.Revit.DB.Structure.LoadBase));
                excludedTypes.Add(typeof(BeamSystem));

                // curtain wall sub-types we are ignoring.
                excludedTypes.Add(typeof(CurtainGridLine));
                // excludedTypes.Add(typeof(Mullion));

                // this will be gotten from the element(s) it cuts.
                excludedTypes.Add(typeof(Opening));

                // 2D types we are ignoring
                excludedTypes.Add(typeof(SketchBase));
                excludedTypes.Add(typeof(FaceSplitter));

                // 2D types covered by the element owner view filter
                excludedTypes.Add(typeof(TextNote));
                excludedTypes.Add(typeof(FilledRegion));

                // exclude levels that are covered in BeginExport
                excludedTypes.Add(typeof(Level));

                // exclude analytical models
                excludedTypes.Add(typeof(Autodesk.Revit.DB.Structure.AnalyticalModel));

                ElementFilter excludedClassFilter = new ElementMulticlassFilter(excludedTypes, true);

                List<BuiltInCategory> excludedCategories = new List<BuiltInCategory>();


                // Native Revit types without match in API
                excludedCategories.Add(BuiltInCategory.OST_Property);
                excludedCategories.Add(BuiltInCategory.OST_SiteProperty);
                excludedCategories.Add(BuiltInCategory.OST_SitePropertyLineSegment);
                excludedCategories.Add(BuiltInCategory.OST_Viewports);
                excludedCategories.Add(BuiltInCategory.OST_Views);
                excludedCategories.Add(BuiltInCategory.OST_IOS_GeoLocations);
                excludedCategories.Add(BuiltInCategory.OST_RvtLinks);
                excludedCategories.Add(BuiltInCategory.OST_DecalElement);
                //excludedCategories.Add(BuiltInCategory.OST_Parts);
                excludedCategories.Add(BuiltInCategory.OST_DuctCurvesCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_DuctFittingCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_PipeCurvesCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_PipeFittingCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_ConduitCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_ConduitFittingCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_FlexDuctCurvesCenterLine);
                excludedCategories.Add(BuiltInCategory.OST_FlexPipeCurvesCenterLine);

                ElementMulticategoryFilter excludedCategoryFilter = new ElementMulticategoryFilter(excludedCategories, true);

                LogicalAndFilter exclusionFilter = new LogicalAndFilter(excludedClassFilter, excludedCategoryFilter);

                ElementOwnerViewFilter ownerViewFilter = new ElementOwnerViewFilter(ElementId.InvalidElementId);

                LogicalAndFilter returnedFilter = new LogicalAndFilter(exclusionFilter, ownerViewFilter);

                return returnedFilter;
            }
        }

        /// <summary>
        /// Gets element filter that match certain phases. 
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <returns>The element filter.</returns>
        private static ElementFilter GetPhaseStatusFilter(Document document, Element filterView)
        {
            List<ElementOnPhaseStatus> phaseStatuses = new List<ElementOnPhaseStatus>();
            phaseStatuses.Add(ElementOnPhaseStatus.None);  //include "none" because we might want to export phaseless elements.
            phaseStatuses.Add(ElementOnPhaseStatus.Existing);
            phaseStatuses.Add(ElementOnPhaseStatus.New);

            PhaseArray phaseArray = document.Phases;

            ElementId phaseId = ElementId.InvalidElementId;
            if (filterView != null)
            {
                Parameter currPhase = filterView.get_Parameter(BuiltInParameter.VIEW_PHASE);
                if (currPhase != null)
                    phaseId = currPhase.AsElementId();
            }
            if (phaseId == ElementId.InvalidElementId)
            {
                Phase lastPhase = phaseArray.get_Item(phaseArray.Size - 1);
                phaseId = lastPhase.Id;
            }

            return new ElementPhaseStatusFilter(phaseId, phaseStatuses);
        }

        private static IDictionary<ElementId, bool> m_CategoryVisibilityCache = new Dictionary<ElementId, bool>();

        public static void InitCategoryVisibilityCache()
        {
            m_CategoryVisibilityCache.Clear();
        }

        private static bool CheckIsCategoryHidden(View filterView, Element element)
        {
            bool value = false;
            Category category = element.Category;
            if (category == null)
                return value;

            if (m_CategoryVisibilityCache.TryGetValue(category.Id, out value))
                return value;

            value = (category.get_AllowsVisibilityControl(filterView) && !category.get_Visible(filterView));
            m_CategoryVisibilityCache[category.Id] = value;
            return value;
        }

        /// <summary>
        /// Checks if element is visible for certain view.
        /// </summary>
        /// <param name="filterView">The view.</param>
        /// <param name="element">The element.</param>
        /// <returns>True if the element is visible, false otherwise.</returns>
        public static bool IsElementVisible(View filterView, Element element)
        {
            bool hidden = element.IsHidden(filterView);
            if (hidden)
                return false;

            Category category = element.Category;
            hidden = CheckIsCategoryHidden(filterView, element);
            if (hidden)
                return false;

            bool temporaryVisible = filterView.IsElementVisibleInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate, element.Id);

            return temporaryVisible;
        }

        /// <summary>
        /// Checks if the IFC type is MEP type.
        /// </summary>
        /// <param name="exportType">IFC Export Type to check</param>
        /// <returns>True for MEP type of elements.</returns>
        public static bool IsMEPType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportDistributionElement && exportType <= IFCExportType.ExportWasteTerminalType);
        }
    }
}
