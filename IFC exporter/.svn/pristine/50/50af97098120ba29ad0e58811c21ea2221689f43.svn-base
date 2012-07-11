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
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;


namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export generic family instances and types.
    /// </summary>
    class FamilyExporterUtil
    {
        /// <summary>
        /// Checks if export type is distribution control element.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is distribution control element, false otherwise.
        /// </returns>
        public static bool IsDistributionControlElementSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportActuatorType && exportType <= IFCExportType.ExportSensorType);
        }

        /// <summary>
        /// Checks if export type is distribution flow element.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is distribution flow element, false otherwise.
        /// </returns>
        public static bool IsDistributionFlowElementSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportDistributionChamberElementType &&
               exportType <= IFCExportType.ExportFlowController);
        }

        /// <summary>
        /// Checks if export type is conversion device.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is conversion device, false otherwise.
        /// </returns>
        public static bool IsEnergyConversionDeviceSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportAirToAirHeatRecoveryType &&
               exportType <= IFCExportType.ExportUnitaryEquipmentType);
        }

        /// <summary>
        /// Checks if export type is flow fitting.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow fitting, false otherwise.
        /// </returns>
        public static bool IsFlowFittingSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportCableCarrierFittingType &&
               exportType <= IFCExportType.ExportPipeFittingType);
        }

        /// <summary>
        /// Checks if export type is flow moving device.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow moving device, false otherwise.
        /// </returns>
        public static bool IsFlowMovingDeviceSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportCompressorType &&
               exportType <= IFCExportType.ExportPumpType);
        }

        /// <summary>
        /// Checks if export type is flow segment.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow segment, false otherwise.
        /// </returns>
        public static bool IsFlowSegmentSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportCableCarrierSegmentType &&
               exportType <= IFCExportType.ExportPipeSegmentType);
        }

        /// <summary>
        /// Checks if export type is flow storage device.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow storage device, false otherwise.
        /// </returns>
        public static bool IsFlowStorageDeviceSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportElectricFlowStorageDeviceType &&
               exportType <= IFCExportType.ExportTankType);
        }

        /// <summary>
        /// Checks if export type is flow terminal.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow terminal, false otherwise.
        /// </returns>
        public static bool IsFlowTerminalSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportAirTerminalType &&
               exportType <= IFCExportType.ExportWasteTerminalType);
        }

        /// <summary>
        /// Checks if export type is flow treatment device.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow treatment device, false otherwise.
        /// </returns>
        public static bool IsFlowTreatmentDeviceSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportDuctSilencerType &&
               exportType <= IFCExportType.ExportFilterType);
        }

        /// <summary>
        /// Checks if export type is flow controller.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is flow controller, false otherwise.
        /// </returns>
        public static bool IsFlowControllerSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportAirTerminalBoxType &&
               exportType <= IFCExportType.ExportValveType);
        }

        /// <summary>
        /// Checks if export type is furnishing element.
        /// </summary>
        /// <param name="exportType">
        /// The export type.
        /// </param>
        /// <returns>
        /// True if it is furnishing element, false otherwise.
        /// </returns>
        public static bool IsFurnishingElementSubType(IFCExportType exportType)
        {
            return (exportType >= IFCExportType.ExportFurnitureType &&
               exportType <= IFCExportType.ExportSystemFurnitureElementType);
        }

        /// <summary>
        /// Exports a generic family instance as IFC instance.
        /// </summary>
        /// <param name="type">The export type.</param>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="familyInstance">The element.</param>
        /// <param name="wrapper">The IFCProductWrapper.</param>
        /// <param name="setter">The IFCPlacementSetter.</param>
        /// <param name="extraParams">The extrusion creation data.</param>
        /// <param name="instanceGUID">The guid.</param>
        /// <param name="ownerHistory">The owner history handle.</param>
        /// <param name="instanceName">The name.</param>
        /// <param name="instanceDescription">The description.</param>
        /// <param name="instanceObjectType">The object type.</param>
        /// <param name="productRepresentation">The representation handle.</param>
        /// <param name="instanceElemId">The element id label.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle ExportGenericInstance(IFCExportType type,
           ExporterIFC exporterIFC, Element familyInstance,
           IFCProductWrapper wrapper, IFCPlacementSetter setter, IFCExtrusionCreationData extraParams,
           string instanceGUID, IFCAnyHandle ownerHistory,
           string instanceName, string instanceDescription, string instanceObjectType,
           IFCAnyHandle productRepresentation,
           string instanceElemId)
        {
            IFCFile file = exporterIFC.GetFile();
            Document doc = familyInstance.Document;

            bool isRoomRelated = IsRoomRelated(type);

            IFCAnyHandle localPlacementToUse = setter.GetPlacement();
            ElementId roomId = ElementId.InvalidElementId;
            if (isRoomRelated)
            {
                roomId = setter.UpdateRoomRelativeCoordinates(familyInstance, out localPlacementToUse);
            }

            //should remove the create method where there is no use of this handle for API methods
            //some places uses the return value of ExportGenericInstance as input parameter for API methods
            IFCAnyHandle instanceHandle = null;
            switch (type)
            {
                case IFCExportType.ExportColumnType:
                    {
                        instanceHandle = IFCInstanceExporter.CreateColumn(file, instanceGUID, ownerHistory,
                           instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        break;
                    }
                case IFCExportType.ExportMemberType:
                    {
                        instanceHandle = IFCInstanceExporter.CreateMember(file, instanceGUID, ownerHistory,
                           instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        break;
                    }
                case IFCExportType.ExportPlateType:
                    {
                        instanceHandle = IFCInstanceExporter.CreatePlate(file, instanceGUID, ownerHistory,
                           instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        break;
                    }
                case IFCExportType.ExportDistributionControlElement:
                    {
                        instanceHandle = IFCInstanceExporter.CreateDistributionControlElement(file, instanceGUID, ownerHistory,
                           instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId,
                           null);
                        break;
                    }
                case IFCExportType.ExportDistributionElement:
                    {
                        instanceHandle = IFCInstanceExporter.CreateDistributionElement(file, instanceGUID, ownerHistory,
                           instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        break;
                    }
                default:
                    {
                        if ((type == IFCExportType.ExportFurnishingElement) || IsFurnishingElementSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFurnishingElement(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportEnergyConversionDevice) || IsEnergyConversionDeviceSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateEnergyConversionDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowFitting) || IsFlowFittingSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowFitting(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowMovingDevice) || IsFlowMovingDeviceSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowMovingDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowSegment) || IsFlowSegmentSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowSegment(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowStorageDevice) || IsFlowStorageDeviceSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowStorageDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowTerminal) || IsFlowTerminalSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowTerminal(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowTreatmentDevice) || IsFlowTreatmentDeviceSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowTreatmentDevice(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportFlowController) || IsFlowControllerSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateFlowController(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        else if ((type == IFCExportType.ExportDistributionFlowElement) || IsDistributionFlowElementSubType(type))
                        {
                            instanceHandle = IFCInstanceExporter.CreateDistributionFlowElement(file, instanceGUID, ownerHistory,
                               instanceName, instanceDescription, instanceObjectType, localPlacementToUse, productRepresentation, instanceElemId);
                        }
                        break;
                    }
            }

            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(instanceHandle))
            {
                if (roomId == ElementId.InvalidElementId)
                {
                    wrapper.AddElement(instanceHandle, setter, extraParams, true);
                }
                else
                {
                    exporterIFC.RelateSpatialElement(roomId, instanceHandle);
                    wrapper.AddElement(instanceHandle, setter, extraParams, false);
                }
            }
            return instanceHandle;
        }

        /// <summary>
        /// Exports IFC type.
        /// </summary>
        /// <remarks>
        /// This method will override the default value of the elemId label for certain element types, and then pass it on
        /// to the generic routine.
        /// </remarks>
        /// <param name="file">The IFC file.</param>
        /// <param name="type">The export type.</param>
        /// <param name="ifcEnumType">The string value represents the IFC type.</param>
        /// <param name="guid">The guid.</param>
        /// <param name="ownerHistory">The owner history handle.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The optional data type of the entity.</param>
        /// <param name="propertySets">The property sets.</param>
        /// <param name="representationMapList">List of representations.</param>
        /// <param name="elemId">The element id label.</param>
        /// <param name="typeName">The IFCPlacementSetter.</param>
        /// <param name="instance">The family instance.</param>
        /// <param name="symbol">The element type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle ExportGenericType(IFCFile file,
           IFCExportType type,
           string ifcEnumType,
           string guid,
           IFCAnyHandle ownerHistory,
           string name,
           string description,
           string applicableOccurrence,
           HashSet<IFCAnyHandle> propertySets,
           IList<IFCAnyHandle> representationMapList,
           string elemId,
           string typeName,
           Element instance,
           ElementType symbol)
        {
            string elemIdToUse = elemId;
            switch (type)
            {
                case IFCExportType.ExportFurnitureType:
                case IFCExportType.ExportMemberType:
                case IFCExportType.ExportPlateType:
                    {
                        elemIdToUse = NamingUtil.CreateIFCElementId(instance);
                        break;
                    }
            }
            return ExportGenericTypeBase(file, type, ifcEnumType, guid, ownerHistory, name, description, applicableOccurrence,
               propertySets, representationMapList, elemIdToUse, typeName, instance, symbol);
        }

        /// <summary>
        /// Exports IFC type.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="type">The export type.</param>
        /// <param name="ifcEnumType">The string value represents the IFC type.</param>
        /// <param name="guid">The guid.</param>
        /// <param name="ownerHistory">The owner history handle.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The optional data type of the entity.</param>
        /// <param name="propertySets">The property sets.</param>
        /// <param name="representationMapList">List of representations.</param>
        /// <param name="elementTag">The element tag.</param>
        /// <param name="typeName">The IFCPlacementSetter.</param>
        /// <param name="instance">The family instance.</param>
        /// <param name="symbol">The element type.</param>
        /// <returns>The handle.</returns>
        private static IFCAnyHandle ExportGenericTypeBase(IFCFile file,
           IFCExportType type,
           string ifcEnumType,
           string guid,
           IFCAnyHandle ownerHistory,
           string name,
           string description,
           string applicableOccurrence,
           HashSet<IFCAnyHandle> propertySets,
           IList<IFCAnyHandle> representationMapList,
           string elementTag,
           string typeName,
           Element instance,
           ElementType symbol)
        {
            switch (type)
            {
                case IFCExportType.ExportActuatorType:
                    return IFCInstanceExporter.CreateActuatorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetActuatorType(instance, ifcEnumType));
                case IFCExportType.ExportAirTerminalBoxType:
                    return IFCInstanceExporter.CreateAirTerminalBoxType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetAirTerminalBoxType(instance, ifcEnumType));
                case IFCExportType.ExportAirTerminalType:
                    return IFCInstanceExporter.CreateAirTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetAirTerminalType(instance, ifcEnumType));
                case IFCExportType.ExportAirToAirHeatRecoveryType:
                    return IFCInstanceExporter.CreateAirToAirHeatRecoveryType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetAirToAirHeatRecoveryType(instance, ifcEnumType));
                case IFCExportType.ExportAlarmType:
                    return IFCInstanceExporter.CreateAlarmType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetAlarmType(instance, ifcEnumType));
                case IFCExportType.ExportBoilerType:
                    return IFCInstanceExporter.CreateBoilerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetBoilerType(instance, ifcEnumType));
                case IFCExportType.ExportCableCarrierFittingType:
                    return IFCInstanceExporter.CreateCableCarrierFittingType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCableCarrierFittingType(instance, ifcEnumType));
                case IFCExportType.ExportCableCarrierSegmentType:
                    return IFCInstanceExporter.CreateCableCarrierSegmentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCableCarrierSegmentType(instance, ifcEnumType));
                case IFCExportType.ExportCableSegmentType:
                    return IFCInstanceExporter.CreateCableSegmentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCableSegmentType(instance, ifcEnumType));
                case IFCExportType.ExportChillerType:
                    return IFCInstanceExporter.CreateChillerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetChillerType(instance, ifcEnumType));
                case IFCExportType.ExportCoilType:
                    return IFCInstanceExporter.CreateCoilType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCoilType(instance, ifcEnumType));
                case IFCExportType.ExportCompressorType:
                    return IFCInstanceExporter.CreateCompressorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCompressorType(instance, ifcEnumType));
                case IFCExportType.ExportCondenserType:
                    return IFCInstanceExporter.CreateCondenserType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCondenserType(instance, ifcEnumType));
                case IFCExportType.ExportControllerType:
                    return IFCInstanceExporter.CreateControllerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetControllerType(instance, ifcEnumType));
                case IFCExportType.ExportCooledBeamType:
                    return IFCInstanceExporter.CreateCooledBeamType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCooledBeamType(instance, ifcEnumType));
                case IFCExportType.ExportCoolingTowerType:
                    return IFCInstanceExporter.CreateCoolingTowerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetCoolingTowerType(instance, ifcEnumType));
                case IFCExportType.ExportDamperType:
                    return IFCInstanceExporter.CreateDamperType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetDamperType(instance, ifcEnumType));
                case IFCExportType.ExportDistributionChamberElementType:
                    return IFCInstanceExporter.CreateDistributionChamberElementType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetDistributionChamberElementType(instance, ifcEnumType));
                case IFCExportType.ExportDuctFittingType:
                    return IFCInstanceExporter.CreateDuctFittingType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetDuctFittingType(instance, ifcEnumType));
                case IFCExportType.ExportDuctSegmentType:
                    return IFCInstanceExporter.CreateDuctSegmentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetDuctSegmentType(instance, ifcEnumType));
                case IFCExportType.ExportDuctSilencerType:
                    return IFCInstanceExporter.CreateDuctSilencerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetDuctSilencerType(instance, ifcEnumType));
                case IFCExportType.ExportElectricApplianceType:
                    return IFCInstanceExporter.CreateElectricApplianceType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricApplianceType(instance, ifcEnumType));
                case IFCExportType.ExportElectricFlowStorageDeviceType:
                    return IFCInstanceExporter.CreateElectricFlowStorageDeviceType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricFlowStorageDeviceType(instance, ifcEnumType));
                case IFCExportType.ExportElectricGeneratorType:
                    return IFCInstanceExporter.CreateElectricGeneratorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricGeneratorType(instance, ifcEnumType));
                case IFCExportType.ExportElectricHeaterType:
                    return IFCInstanceExporter.CreateElectricHeaterType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricHeaterType(instance, ifcEnumType));
                case IFCExportType.ExportElectricMotorType:
                    return IFCInstanceExporter.CreateElectricMotorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricMotorType(instance, ifcEnumType));
                case IFCExportType.ExportElectricTimeControlType:
                    return IFCInstanceExporter.CreateElectricTimeControlType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetElectricTimeControlType(instance, ifcEnumType));
                case IFCExportType.ExportEvaporativeCoolerType:
                    return IFCInstanceExporter.CreateEvaporativeCoolerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetEvaporativeCoolerType(instance, ifcEnumType));
                case IFCExportType.ExportEvaporatorType:
                    return IFCInstanceExporter.CreateEvaporatorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetEvaporatorType(instance, ifcEnumType));
                case IFCExportType.ExportFanType:
                    return IFCInstanceExporter.CreateFanType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetFanType(instance, ifcEnumType));
                case IFCExportType.ExportFilterType:
                    return IFCInstanceExporter.CreateFilterType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetFilterType(instance, ifcEnumType));
                case IFCExportType.ExportFireSuppressionTerminalType:
                    return IFCInstanceExporter.CreateFireSuppressionTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetFireSuppressionTerminalType(instance, ifcEnumType));
                case IFCExportType.ExportFlowInstrumentType:
                    return IFCInstanceExporter.CreateFlowInstrumentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetFlowInstrumentType(instance, ifcEnumType));
                case IFCExportType.ExportFlowMeterType:
                    return IFCInstanceExporter.CreateFlowMeterType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetFlowMeterType(instance, ifcEnumType));
                case IFCExportType.ExportFurnitureType:
                    return IFCInstanceExporter.CreateFurnitureType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetAssemblyPlace(instance, ifcEnumType));
                case IFCExportType.ExportGasTerminalType:
                    return IFCInstanceExporter.CreateGasTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetGasTerminalType(instance, ifcEnumType));
                case IFCExportType.ExportHeatExchangerType:
                    return IFCInstanceExporter.CreateHeatExchangerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetHeatExchangerType(instance, ifcEnumType));
                case IFCExportType.ExportHumidifierType:
                    return IFCInstanceExporter.CreateHumidifierType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetHumidifierType(instance, ifcEnumType));
                case IFCExportType.ExportJunctionBoxType:
                    return IFCInstanceExporter.CreateJunctionBoxType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetJunctionBoxType(instance, ifcEnumType));
                case IFCExportType.ExportLampType:
                    return IFCInstanceExporter.CreateLampType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetLampType(instance, ifcEnumType));
                case IFCExportType.ExportLightFixtureType:
                    return IFCInstanceExporter.CreateLightFixtureType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetLightFixtureType(instance, ifcEnumType));
                case IFCExportType.ExportMemberType:
                    return IFCInstanceExporter.CreateMemberType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetMemberType(instance, ifcEnumType));
                case IFCExportType.ExportMotorConnectionType:
                    return IFCInstanceExporter.CreateMotorConnectionType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetMotorConnectionType(instance, ifcEnumType));
                case IFCExportType.ExportOutletType:
                    return IFCInstanceExporter.CreateOutletType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetOutletType(instance, ifcEnumType));
                case IFCExportType.ExportPlateType:
                    return IFCInstanceExporter.CreatePlateType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetPlateType(instance, ifcEnumType));
                case IFCExportType.ExportPipeFittingType:
                    return IFCInstanceExporter.CreatePipeFittingType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetPipeFittingType(instance, ifcEnumType));
                case IFCExportType.ExportPipeSegmentType:
                    return IFCInstanceExporter.CreatePipeSegmentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetPipeSegmentType(instance, ifcEnumType));
                case IFCExportType.ExportProtectiveDeviceType:
                    return IFCInstanceExporter.CreateProtectiveDeviceType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetProtectiveDeviceType(instance, ifcEnumType));
                case IFCExportType.ExportPumpType:
                    return IFCInstanceExporter.CreatePumpType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetPumpType(instance, ifcEnumType));
                case IFCExportType.ExportSanitaryTerminalType:
                    return IFCInstanceExporter.CreateSanitaryTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetSanitaryTerminalType(instance, ifcEnumType));
                case IFCExportType.ExportSensorType:
                    return IFCInstanceExporter.CreateSensorType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetSensorType(instance, ifcEnumType));
                case IFCExportType.ExportSpaceHeaterType:
                    return IFCInstanceExporter.CreateSpaceHeaterType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetSpaceHeaterType(instance, ifcEnumType));
                case IFCExportType.ExportStackTerminalType:
                    return IFCInstanceExporter.CreateStackTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetStackTerminalType(instance, ifcEnumType));
                case IFCExportType.ExportSwitchingDeviceType:
                    return IFCInstanceExporter.CreateSwitchingDeviceType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetSwitchingDeviceType(instance, ifcEnumType));
                case IFCExportType.ExportTankType:
                    return IFCInstanceExporter.CreateTankType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetTankType(instance, ifcEnumType));
                case IFCExportType.ExportTransformerType:
                    return IFCInstanceExporter.CreateTransformerType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetTransformerType(instance, ifcEnumType));
                case IFCExportType.ExportTransportElementType:
                    return IFCInstanceExporter.CreateTransportElementType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetTransportElementType(instance, ifcEnumType));
                case IFCExportType.ExportTubeBundleType:
                    return IFCInstanceExporter.CreateTubeBundleType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetTubeBundleType(instance, ifcEnumType));
                case IFCExportType.ExportUnitaryEquipmentType:
                    return IFCInstanceExporter.CreateUnitaryEquipmentType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetUnitaryEquipmentType(instance, ifcEnumType));
                case IFCExportType.ExportValveType:
                    return IFCInstanceExporter.CreateValveType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetValveType(instance, ifcEnumType));
                case IFCExportType.ExportWasteTerminalType:
                    return IFCInstanceExporter.CreateWasteTerminalType(file, guid, ownerHistory, name,
                       description, applicableOccurrence, propertySets, representationMapList, elementTag,
                       typeName, GetWasteTerminalType(instance, ifcEnumType));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if export type is room related.
        /// </summary>
        /// <param name="exportType">The export type.</param>
        /// <returns>True if the export type is room related, false otherwise.</returns>
        private static bool IsRoomRelated(IFCExportType exportType)
        {
            return (IsFurnishingElementSubType(exportType) ||
                IsDistributionControlElementSubType(exportType) ||
                IsDistributionFlowElementSubType(exportType) ||
                IsEnergyConversionDeviceSubType(exportType) ||
                IsFlowFittingSubType(exportType) ||
                IsFlowMovingDeviceSubType(exportType) ||
                IsFlowSegmentSubType(exportType) ||
                IsFlowStorageDeviceSubType(exportType) ||
                IsFlowTerminalSubType(exportType) ||
                IsFlowTreatmentDeviceSubType(exportType) ||
                IsFlowControllerSubType(exportType));
        }

        private static IFCActuatorType GetActuatorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCActuatorType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCActuatorType.UserDefined;
            if (String.Compare(newValue, "ELECTRICACTUATOR", true) == 0)
                return IFCActuatorType.ElectricActuator;
            if (String.Compare(newValue, "HANDOPERATEDACTUATOR", true) == 0)
                return IFCActuatorType.HandOperatedActuator;
            if (String.Compare(newValue, "HYDRAULICACTUATOR", true) == 0)
                return IFCActuatorType.HydraulicActuator;
            if (String.Compare(newValue, "PNEUMATICACTUATOR", true) == 0)
                return IFCActuatorType.PneumaticActuator;
            if (String.Compare(newValue, "THERMOSTATICACTUATOR", true) == 0)
                return IFCActuatorType.ThermostaticActuator;
            if (String.Compare(newValue, "THERMOSTATICACTUATOR", true) == 0)
                return IFCActuatorType.ThermostaticActuator;

            return IFCActuatorType.UserDefined;
        }

        private static IFCAirTerminalBoxType GetAirTerminalBoxType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCAirTerminalBoxType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCAirTerminalBoxType.UserDefined;
            if (String.Compare(newValue, "CONSTANTFLOW", true) == 0)
                return IFCAirTerminalBoxType.ConstantFlow;
            if (String.Compare(newValue, "VARIABLEFLOWPRESSUREDEPENDANT", true) == 0)
                return IFCAirTerminalBoxType.VariableFlowPressureDependant;
            if (String.Compare(newValue, "VARIABLEFLOWPRESSUREINDEPENDANT", true) == 0)
                return IFCAirTerminalBoxType.VariableFlowPressureIndependant;

            return IFCAirTerminalBoxType.UserDefined;
        }

        private static IFCAirTerminalType GetAirTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCAirTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCAirTerminalType.UserDefined;
            if (String.Compare(newValue, "GRILLE", true) == 0)
                return IFCAirTerminalType.Grille;
            if (String.Compare(newValue, "REGISTER", true) == 0)
                return IFCAirTerminalType.Register;
            if (String.Compare(newValue, "DIFFUSER", true) == 0)
                return IFCAirTerminalType.Diffuser;
            if (String.Compare(newValue, "EYEBALL", true) == 0)
                return IFCAirTerminalType.EyeBall;
            if (String.Compare(newValue, "IRIS", true) == 0)
                return IFCAirTerminalType.Iris;
            if (String.Compare(newValue, "LINEARGRILLE", true) == 0)
                return IFCAirTerminalType.LinearGrille;
            if (String.Compare(newValue, "LINEARDIFFUSER", true) == 0)
                return IFCAirTerminalType.LinearDiffuser;

            return IFCAirTerminalType.UserDefined;
        }

        private static IFCAirToAirHeatRecoveryType GetAirToAirHeatRecoveryType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCAirToAirHeatRecoveryType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCAirToAirHeatRecoveryType.UserDefined;
            if (String.Compare(newValue, "FIXEDPLATECOUNTERFLOWEXCHANGER", true) == 0)
                return IFCAirToAirHeatRecoveryType.FixedPlateCounterFlowExchanger;
            if (String.Compare(newValue, "FIXEDPLATECROSSFLOWEXCHANGER", true) == 0)
                return IFCAirToAirHeatRecoveryType.FixedPlateCrossFlowExchanger;
            if (String.Compare(newValue, "FIXEDPLATEPARALLELFLOWEXCHANGER", true) == 0)
                return IFCAirToAirHeatRecoveryType.FixedPlateParallelFlowExchanger;
            if (String.Compare(newValue, "ROTARYWHEEL", true) == 0)
                return IFCAirToAirHeatRecoveryType.RotaryWheel;
            if (String.Compare(newValue, "RUNAROUNDCOILLOOP", true) == 0)
                return IFCAirToAirHeatRecoveryType.RunaroundCoilloop;
            if (String.Compare(newValue, "HEATPIPE", true) == 0)
                return IFCAirToAirHeatRecoveryType.HeatPipe;
            if (String.Compare(newValue, "TWINTOWERENTHALPYRECOVERYLOOPS", true) == 0)
                return IFCAirToAirHeatRecoveryType.TwinTowerEnthalpyRecoveryLoops;
            if (String.Compare(newValue, "THERMOSIPHONSEALEDTUBEHEATEXCHANGERS", true) == 0)
                return IFCAirToAirHeatRecoveryType.ThermosiphonSealedTubeHeatExchangers;
            if (String.Compare(newValue, "THERMOSIPHONCOILTYPEHEATEXCHANGERS", true) == 0)
                return IFCAirToAirHeatRecoveryType.ThermosiphonCoilTypeHeatExchangers;

            return IFCAirToAirHeatRecoveryType.UserDefined;
        }

        private static IFCAlarmType GetAlarmType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCAlarmType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "USERDEFINED", true) == 0)
                return IFCAlarmType.UserDefined;
            if (String.Compare(newValue, "BELL", true) == 0)
                return IFCAlarmType.Bell;
            if (String.Compare(newValue, "BREAKGLASSBUTTON", true) == 0)
                return IFCAlarmType.BreakGlassButton;
            if (String.Compare(newValue, "LIGHT", true) == 0)
                return IFCAlarmType.Light;
            if (String.Compare(newValue, "MANUALPULLBOX", true) == 0)
                return IFCAlarmType.ManualPullBox;
            if (String.Compare(newValue, "SIREN", true) == 0)
                return IFCAlarmType.Siren;
            if (String.Compare(newValue, "WHISTLE", true) == 0)
                return IFCAlarmType.Whistle;

            return IFCAlarmType.UserDefined;
        }

        private static IFCBoilerType GetBoilerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCBoilerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "WATER", true) == 0)
                return IFCBoilerType.Water;
            if (String.Compare(newValue, "STEAM", true) == 0)
                return IFCBoilerType.Steam;

            return IFCBoilerType.UserDefined;
        }

        private static IFCCableCarrierFittingType GetCableCarrierFittingType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCableCarrierFittingType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BEND", true) == 0)
                return IFCCableCarrierFittingType.Bend;
            if (String.Compare(newValue, "CROSS", true) == 0)
                return IFCCableCarrierFittingType.Cross;
            if (String.Compare(newValue, "REDUCER", true) == 0)
                return IFCCableCarrierFittingType.Reducer;
            if (String.Compare(newValue, "TEE", true) == 0)
                return IFCCableCarrierFittingType.Tee;

            return IFCCableCarrierFittingType.UserDefined;
        }

        private static IFCCableCarrierSegmentType GetCableCarrierSegmentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCableCarrierSegmentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CABLELADDERSEGMENT", true) == 0)
                return IFCCableCarrierSegmentType.CableLadderSEGMENT;
            if (String.Compare(newValue, "CABLETRAYSEGMENT", true) == 0)
                return IFCCableCarrierSegmentType.CableTraySegment;
            if (String.Compare(newValue, "CABLETRUNKINGSEGMENT", true) == 0)
                return IFCCableCarrierSegmentType.CableTrunkingSegment;
            if (String.Compare(newValue, "CONDUITSEGMENT", true) == 0)
                return IFCCableCarrierSegmentType.ConduitSegment;

            return IFCCableCarrierSegmentType.UserDefined;
        }

        private static IFCCableSegmentType GetCableSegmentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCableSegmentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CABLESEGMENT", true) == 0)
                return IFCCableSegmentType.CableSegment;
            if (String.Compare(newValue, "CONDUCTORSEGMENT", true) == 0)
                return IFCCableSegmentType.ConductorSegment;

            return IFCCableSegmentType.UserDefined;
        }

        private static IFCChillerType GetChillerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCChillerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "AIRCOOLED", true) == 0)
                return IFCChillerType.AirCooled;
            if (String.Compare(newValue, "WATERCOOLED", true) == 0)
                return IFCChillerType.WaterCooled;
            if (String.Compare(newValue, "HEATRECOVERY", true) == 0)
                return IFCChillerType.HeatRecovery;

            return IFCChillerType.UserDefined;
        }

        private static IFCCoilType GetCoilType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCoilType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "DXCOOLINGCOIL", true) == 0)
                return IFCCoilType.DXCoolingCoil;
            if (String.Compare(newValue, "WATERCOOLINGCOIL", true) == 0)
                return IFCCoilType.WaterCoolingCoil;
            if (String.Compare(newValue, "STEAMHEATINGCOIL", true) == 0)
                return IFCCoilType.SteamHeatingCoil;
            if (String.Compare(newValue, "WATERHEATINGCOIL", true) == 0)
                return IFCCoilType.WaterHeatingCoil;
            if (String.Compare(newValue, "ELECTRICHEATINGCOIL", true) == 0)
                return IFCCoilType.ElectricHeatingCoil;
            if (String.Compare(newValue, "GASHEATINGCOIL", true) == 0)
                return IFCCoilType.GasHeatingCoil;

            return IFCCoilType.UserDefined;
        }

        private static IFCCompressorType GetCompressorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCompressorType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "DYNAMIC", true) == 0)
                return IFCCompressorType.Dynamic;
            if (String.Compare(newValue, "RECIPROCATING", true) == 0)
                return IFCCompressorType.Reciprocating;
            if (String.Compare(newValue, "ROTARY", true) == 0)
                return IFCCompressorType.Rotary;
            if (String.Compare(newValue, "SCROLL", true) == 0)
                return IFCCompressorType.Scroll;
            if (String.Compare(newValue, "TROCHOIDAL", true) == 0)
                return IFCCompressorType.Trochoidal;
            if (String.Compare(newValue, "SINGLESTAGE", true) == 0)
                return IFCCompressorType.SingleStage;
            if (String.Compare(newValue, "BOOSTER", true) == 0)
                return IFCCompressorType.Booster;
            if (String.Compare(newValue, "OPENTYPE", true) == 0)
                return IFCCompressorType.OpenType;
            if (String.Compare(newValue, "HERMETIC", true) == 0)
                return IFCCompressorType.Hermetic;
            if (String.Compare(newValue, "SEMIHERMETIC", true) == 0)
                return IFCCompressorType.SemiHermetic;
            if (String.Compare(newValue, "WELDEDSHELLHERMETIC", true) == 0)
                return IFCCompressorType.WeldedShellHermetic;
            if (String.Compare(newValue, "ROLLINGPISTON", true) == 0)
                return IFCCompressorType.RollingPiston;
            if (String.Compare(newValue, "ROTARYVANE", true) == 0)
                return IFCCompressorType.RotaryVane;
            if (String.Compare(newValue, "SINGLESCREW", true) == 0)
                return IFCCompressorType.SingleScrew;
            if (String.Compare(newValue, "TWINSCREW", true) == 0)
                return IFCCompressorType.TwinScrew;

            return IFCCompressorType.UserDefined;
        }

        private static IFCCondenserType GetCondenserType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCondenserType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "WATERCOOLEDSHELLTUBE", true) == 0)
                return IFCCondenserType.WaterCooledShellTube;
            if (String.Compare(newValue, "WATERCOOLEDSHELLCOIL", true) == 0)
                return IFCCondenserType.WaterCooledShellCoil;
            if (String.Compare(newValue, "WATERCOOLEDTUBEINTUBE", true) == 0)
                return IFCCondenserType.WaterCooledTubeInTube;
            if (String.Compare(newValue, "WATERCOOLEDBRAZEDPLATE", true) == 0)
                return IFCCondenserType.WaterCooledBrazedPlate;
            if (String.Compare(newValue, "AIRCOOLED", true) == 0)
                return IFCCondenserType.AirCooled;
            if (String.Compare(newValue, "EVAPORATIVECOOLED", true) == 0)
                return IFCCondenserType.EvaporativeCooled;

            return IFCCondenserType.UserDefined;
        }

        private static IFCControllerType GetControllerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCControllerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FLOATING", true) == 0)
                return IFCControllerType.Floating;
            if (String.Compare(newValue, "PROPORTIONAL", true) == 0)
                return IFCControllerType.Proportional;
            if (String.Compare(newValue, "PROPORTIONALINTEGRAL", true) == 0)
                return IFCControllerType.ProportionalIntegral;
            if (String.Compare(newValue, "PROPORTIONALINTEGRALDERIVATIVE", true) == 0)
                return IFCControllerType.ProportionalIntegralDerivative;
            if (String.Compare(newValue, "TIMEDTWOPOSITION", true) == 0)
                return IFCControllerType.TimedTwoPosition;
            if (String.Compare(newValue, "TWOPOSITION", true) == 0)
                return IFCControllerType.TwoPosition;

            return IFCControllerType.UserDefined;
        }

        private static IFCCooledBeamType GetCooledBeamType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCooledBeamType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "ACTIVE", true) == 0)
                return IFCCooledBeamType.Active;
            if (String.Compare(newValue, "PASSIVE", true) == 0)
                return IFCCooledBeamType.Passive;

            return IFCCooledBeamType.UserDefined;
        }

        private static IFCCoolingTowerType GetCoolingTowerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCCoolingTowerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "NATURALDRAFT", true) == 0)
                return IFCCoolingTowerType.NaturalDraft;
            if (String.Compare(newValue, "MECHANICALINDUCEDDRAFT", true) == 0)
                return IFCCoolingTowerType.MechanicalInducedDraft;
            if (String.Compare(newValue, "MECHANICALFORCEDDRAFT", true) == 0)
                return IFCCoolingTowerType.MechanicalForcedDraft;

            return IFCCoolingTowerType.UserDefined;
        }

        private static IFCDamperType GetDamperType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDamperType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CONTROLDAMPER", true) == 0)
                return IFCDamperType.ControlDamper;
            if (String.Compare(newValue, "FIREDAMPER", true) == 0)
                return IFCDamperType.FireDamper;
            if (String.Compare(newValue, "SMOKEDAMPER", true) == 0)
                return IFCDamperType.SmokeDamper;
            if (String.Compare(newValue, "FIRESMOKEDAMPER", true) == 0)
                return IFCDamperType.FireSmokeDamper;
            if (String.Compare(newValue, "BACKDRAFTDAMPER", true) == 0)
                return IFCDamperType.BackDraftDamper;
            if (String.Compare(newValue, "RELIEFDAMPER", true) == 0)
                return IFCDamperType.ReliefDamper;
            if (String.Compare(newValue, "BLASTDAMPER", true) == 0)
                return IFCDamperType.BlastDamper;
            if (String.Compare(newValue, "GRAVITYDAMPER", true) == 0)
                return IFCDamperType.GravityDamper;
            if (String.Compare(newValue, "GRAVITYRELIEFDAMPER", true) == 0)
                return IFCDamperType.GravityReliefDamper;
            if (String.Compare(newValue, "BALANCINGDAMPER", true) == 0)
                return IFCDamperType.BalancingDamper;
            if (String.Compare(newValue, "FUMEHOODEXHAUST", true) == 0)
                return IFCDamperType.FumeHoodExhaust;

            return IFCDamperType.UserDefined;
        }

        private static IFCDistributionChamberElementType GetDistributionChamberElementType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDistributionChamberElementType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FORMEDDUCT", true) == 0)
                return IFCDistributionChamberElementType.FormedDuct;
            if (String.Compare(newValue, "INSPECTIONCHAMBER", true) == 0)
                return IFCDistributionChamberElementType.InspectionChamber;
            if (String.Compare(newValue, "INSPECTIONPIT", true) == 0)
                return IFCDistributionChamberElementType.InspectionPit;
            if (String.Compare(newValue, "MANHOLE", true) == 0)
                return IFCDistributionChamberElementType.Manhole;
            if (String.Compare(newValue, "METERCHAMBER", true) == 0)
                return IFCDistributionChamberElementType.MeterChamber;
            if (String.Compare(newValue, "SUMP", true) == 0)
                return IFCDistributionChamberElementType.Sump;
            if (String.Compare(newValue, "TRENCH", true) == 0)
                return IFCDistributionChamberElementType.Trench;
            if (String.Compare(newValue, "VALVECHAMBER", true) == 0)
                return IFCDistributionChamberElementType.ValveChamber;

            return IFCDistributionChamberElementType.UserDefined;
        }

        private static IFCDuctFittingType GetDuctFittingType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDuctFittingType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BEND", true) == 0)
                return IFCDuctFittingType.Bend;
            if (String.Compare(newValue, "CONNECTOR", true) == 0)
                return IFCDuctFittingType.Connector;
            if (String.Compare(newValue, "ENTRY", true) == 0)
                return IFCDuctFittingType.Entry;
            if (String.Compare(newValue, "EXIT", true) == 0)
                return IFCDuctFittingType.Exit;
            if (String.Compare(newValue, "JUNCTION", true) == 0)
                return IFCDuctFittingType.Junction;
            if (String.Compare(newValue, "OBSTRUCTION", true) == 0)
                return IFCDuctFittingType.Obstruction;
            if (String.Compare(newValue, "TRANSITION", true) == 0)
                return IFCDuctFittingType.Transition;

            return IFCDuctFittingType.UserDefined;
        }

        private static IFCDuctSegmentType GetDuctSegmentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDuctSegmentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "RIGIDSEGMENT", true) == 0)
                return IFCDuctSegmentType.RigidSegment;
            if (String.Compare(newValue, "FLEXIBLESEGMENT", true) == 0)
                return IFCDuctSegmentType.FlexibleSegment;

            return IFCDuctSegmentType.UserDefined;
        }

        private static IFCDuctSilencerType GetDuctSilencerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCDuctSilencerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FLATOVAL", true) == 0)
                return IFCDuctSilencerType.FlatOval;
            if (String.Compare(newValue, "RECTANGULAR", true) == 0)
                return IFCDuctSilencerType.Rectangular;
            if (String.Compare(newValue, "ROUND", true) == 0)
                return IFCDuctSilencerType.Round;

            return IFCDuctSilencerType.UserDefined;
        }

        private static IFCElectricApplianceType GetElectricApplianceType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricApplianceType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "COMPUTER", true) == 0)
                return IFCElectricApplianceType.Computer;
            if (String.Compare(newValue, "DIRECTWATERHEATER", true) == 0)
                return IFCElectricApplianceType.DirectWaterHeater;
            if (String.Compare(newValue, "DISHWASHER", true) == 0)
                return IFCElectricApplianceType.DishWasher;
            if (String.Compare(newValue, "ELECTRICCOOKER", true) == 0)
                return IFCElectricApplianceType.ElectricCooker;
            if (String.Compare(newValue, "ELECTRICHEATER", true) == 0)
                return IFCElectricApplianceType.ElectricHeater;
            if (String.Compare(newValue, "FACSIMILE", true) == 0)
                return IFCElectricApplianceType.Facsimile;
            if (String.Compare(newValue, "FREESTANDINGFAN", true) == 0)
                return IFCElectricApplianceType.FreeStandingFan;
            if (String.Compare(newValue, "FREEZER", true) == 0)
                return IFCElectricApplianceType.Freezer;
            if (String.Compare(newValue, "FRIDGEFREEZER", true) == 0)
                return IFCElectricApplianceType.Fridge_Freezer;
            if (String.Compare(newValue, "HANDDRYER", true) == 0)
                return IFCElectricApplianceType.HandDryer;
            if (String.Compare(newValue, "INDIRECTWATERHEATER", true) == 0)
                return IFCElectricApplianceType.IndirectWaterHeater;
            if (String.Compare(newValue, "MICROWAVE", true) == 0)
                return IFCElectricApplianceType.Microwave;
            if (String.Compare(newValue, "PHOTOCOPIER", true) == 0)
                return IFCElectricApplianceType.PhotoCopier;
            if (String.Compare(newValue, "PRINTER", true) == 0)
                return IFCElectricApplianceType.Printer;
            if (String.Compare(newValue, "REFRIGERATOR", true) == 0)
                return IFCElectricApplianceType.Refrigerator;
            if (String.Compare(newValue, "RADIANTHEATER", true) == 0)
                return IFCElectricApplianceType.RadianTheater;
            if (String.Compare(newValue, "SCANNER", true) == 0)
                return IFCElectricApplianceType.Scanner;
            if (String.Compare(newValue, "TELEPHONE", true) == 0)
                return IFCElectricApplianceType.Telephone;
            if (String.Compare(newValue, "TUMBLEDRYER", true) == 0)
                return IFCElectricApplianceType.TumbleDryer;
            if (String.Compare(newValue, "TV", true) == 0)
                return IFCElectricApplianceType.TV;
            if (String.Compare(newValue, "VENDINGMACHINE", true) == 0)
                return IFCElectricApplianceType.VendingMachine;
            if (String.Compare(newValue, "WASHINGMACHINE", true) == 0)
                return IFCElectricApplianceType.WashingMachine;
            if (String.Compare(newValue, "WATERHEATER", true) == 0)
                return IFCElectricApplianceType.WaterHeater;
            if (String.Compare(newValue, "WATERCOOLER", true) == 0)
                return IFCElectricApplianceType.WaterCooler;

            return IFCElectricApplianceType.UserDefined;
        }

        private static IFCElectricFlowStorageDeviceType GetElectricFlowStorageDeviceType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricFlowStorageDeviceType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BATTERY", true) == 0)
                return IFCElectricFlowStorageDeviceType.Battery;
            if (String.Compare(newValue, "CAPACITORBANK", true) == 0)
                return IFCElectricFlowStorageDeviceType.CapacitorBank;
            if (String.Compare(newValue, "HARMONICFILTER", true) == 0)
                return IFCElectricFlowStorageDeviceType.HarmonicFilter;
            if (String.Compare(newValue, "INDUCTORBANK", true) == 0)
                return IFCElectricFlowStorageDeviceType.InductorBank;
            if (String.Compare(newValue, "UPS", true) == 0)
                return IFCElectricFlowStorageDeviceType.Ups;

            return IFCElectricFlowStorageDeviceType.UserDefined;
        }

        private static IFCElectricGeneratorType GetElectricGeneratorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricGeneratorType.NotDefined;

            return IFCElectricGeneratorType.UserDefined;
        }

        private static IFCElectricHeaterType GetElectricHeaterType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricHeaterType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "ELECTRICPOINTHEATER", true) == 0)
                return IFCElectricHeaterType.ElectricPointHeater;
            if (String.Compare(newValue, "ELECTRICCABLEHEATER", true) == 0)
                return IFCElectricHeaterType.ElectricCableHeater;
            if (String.Compare(newValue, "ELECTRICMATHEATER", true) == 0)
                return IFCElectricHeaterType.ElectricMatHeater;

            return IFCElectricHeaterType.UserDefined;
        }

        private static IFCElectricMotorType GetElectricMotorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricMotorType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "DC", true) == 0)
                return IFCElectricMotorType.DC;
            if (String.Compare(newValue, "INDUCTION", true) == 0)
                return IFCElectricMotorType.Induction;
            if (String.Compare(newValue, "POLYPHASE", true) == 0)
                return IFCElectricMotorType.Polyphase;
            if (String.Compare(newValue, "RELUCTANCESYNCHRONOUS", true) == 0)
                return IFCElectricMotorType.ReluctanceSynchronous;
            if (String.Compare(newValue, "SYNCHRONOUS", true) == 0)
                return IFCElectricMotorType.Synchronous;

            return IFCElectricMotorType.UserDefined;
        }

        private static IFCElectricTimeControlType GetElectricTimeControlType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCElectricTimeControlType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "TIMECLOCK", true) == 0)
                return IFCElectricTimeControlType.TimeClock;
            if (String.Compare(newValue, "TIMEDELAY", true) == 0)
                return IFCElectricTimeControlType.TimeDelay;
            if (String.Compare(newValue, "RELAY", true) == 0)
                return IFCElectricTimeControlType.Relay;

            return IFCElectricTimeControlType.UserDefined;
        }

        private static IFCEvaporativeCoolerType GetEvaporativeCoolerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCEvaporativeCoolerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "DIRECTEVAPORATIVERANDOMMEDIAAIRCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.DirectEvaporativeRandomMediaAirCooler;
            if (String.Compare(newValue, "DIRECTEVAPORATIVERIGIDMEDIAAIRCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.DirectEvaporativeRigidMediaAirCooler;
            if (String.Compare(newValue, "DIRECTEVAPORATIVESLINGERSPACKAGEDAIRCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.DirectEvaporativeSlingersPackagedAirCooler;
            if (String.Compare(newValue, "DIRECTEVAPORATIVEPACKAGEDROTARYAIRCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.DirectEvaporativePackagedRotaryAirCooler;
            if (String.Compare(newValue, "DIRECTEVAPORATIVEAIRWASHER", true) == 0)
                return IFCEvaporativeCoolerType.DirectEvaporativeAirWasher;
            if (String.Compare(newValue, "INDIRECTEVAPORATIVEPACKAGEAIRCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.IndirectEvaporativePackageAirCooler;
            if (String.Compare(newValue, "INDIRECTEVAPORATIVEWETCOIL", true) == 0)
                return IFCEvaporativeCoolerType.IndirectEvaporativeWetCoil;
            if (String.Compare(newValue, "INDIRECTEVAPORATIVECOOLINGTOWERORCOILCOOLER", true) == 0)
                return IFCEvaporativeCoolerType.IndirectEvaporativeCoolingTowerOrCoilCooler;
            if (String.Compare(newValue, "INDIRECTDIRECTCOMBINATION", true) == 0)
                return IFCEvaporativeCoolerType.IndirectDirectCombination;

            return IFCEvaporativeCoolerType.UserDefined;
        }

        private static IFCEvaporatorType GetEvaporatorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCEvaporatorType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "DIRECTEXPANSIONSHELLANDTUBE", true) == 0)
                return IFCEvaporatorType.DirectExpansionShellAndTube;
            if (String.Compare(newValue, "DIRECTEXPANSIONTUBEINTUBE", true) == 0)
                return IFCEvaporatorType.DirectExpansionTubeInTube;
            if (String.Compare(newValue, "DIRECTEXPANSIONBRAZEDPLATE", true) == 0)
                return IFCEvaporatorType.DirectExpansionBrazedPlate;
            if (String.Compare(newValue, "FLOODEDSHELLANDTUBE", true) == 0)
                return IFCEvaporatorType.FloodedShellAndTube;
            if (String.Compare(newValue, "SHELLANDCOIL", true) == 0)
                return IFCEvaporatorType.ShellAndCoil;

            return IFCEvaporatorType.UserDefined;
        }

        private static IFCFanType GetFanType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCFanType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CENTRIFUGALFORWARDCURVED", true) == 0)
                return IFCFanType.CentrifugalForwardCurved;
            if (String.Compare(newValue, "CENTRIFUGALRADIAL", true) == 0)
                return IFCFanType.CentrifugalRadial;
            if (String.Compare(newValue, "CENTRIFUGALBACKWARDINCLINEDCURVED", true) == 0)
                return IFCFanType.CentrifugalBackwardInclinedCurved;
            if (String.Compare(newValue, "CENTRIFUGALAIRFOIL", true) == 0)
                return IFCFanType.CentrifugalAirfoil;
            if (String.Compare(newValue, "TUBEAXIAL", true) == 0)
                return IFCFanType.TubeAxial;
            if (String.Compare(newValue, "VANEAXIAL", true) == 0)
                return IFCFanType.VaneAxial;
            if (String.Compare(newValue, "PROPELLORAXIAL", true) == 0)
                return IFCFanType.PropellorAxial;

            return IFCFanType.UserDefined;
        }

        private static IFCFilterType GetFilterType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCFilterType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "AIRPARTICLEFILTER", true) == 0)
                return IFCFilterType.AirParticleFilter;
            if (String.Compare(newValue, "ODORFILTER", true) == 0)
                return IFCFilterType.OdorFilter;
            if (String.Compare(newValue, "OILFILTER", true) == 0)
                return IFCFilterType.OilFilter;
            if (String.Compare(newValue, "STRAINER", true) == 0)
                return IFCFilterType.Strainer;
            if (String.Compare(newValue, "WATERFILTER", true) == 0)
                return IFCFilterType.WaterFilter;

            return IFCFilterType.UserDefined;
        }

        private static IFCFireSuppressionTerminalType GetFireSuppressionTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCFireSuppressionTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BREECHINGINLET", true) == 0)
                return IFCFireSuppressionTerminalType.BreechingInlet;
            if (String.Compare(newValue, "FIREHYDRANT", true) == 0)
                return IFCFireSuppressionTerminalType.FireHydrant;
            if (String.Compare(newValue, "HOSEREEL", true) == 0)
                return IFCFireSuppressionTerminalType.HoseReel;
            if (String.Compare(newValue, "SPRINKLER", true) == 0)
                return IFCFireSuppressionTerminalType.Sprinkler;
            if (String.Compare(newValue, "SPRINKLERDEFLECTOR", true) == 0)
                return IFCFireSuppressionTerminalType.SprinklerDeflector;

            return IFCFireSuppressionTerminalType.UserDefined;
        }

        private static IFCFlowInstrumentType GetFlowInstrumentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCFlowInstrumentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "PRESSUREGAUGE", true) == 0)
                return IFCFlowInstrumentType.PressureGauge;
            if (String.Compare(newValue, "THERMOMETER", true) == 0)
                return IFCFlowInstrumentType.Thermometer;
            if (String.Compare(newValue, "AMMETER", true) == 0)
                return IFCFlowInstrumentType.Ammeter;
            if (String.Compare(newValue, "FREQUENCYMETER", true) == 0)
                return IFCFlowInstrumentType.FrequencyMeter;
            if (String.Compare(newValue, "POWERFACTORMETER", true) == 0)
                return IFCFlowInstrumentType.PowerFactorMeter;
            if (String.Compare(newValue, "PHASEANGLEMETER", true) == 0)
                return IFCFlowInstrumentType.PhaseAngleMeter;
            if (String.Compare(newValue, "VOLTMETERPEAK", true) == 0)
                return IFCFlowInstrumentType.VoltMeter_Peak;
            if (String.Compare(newValue, "VOLTMETERRMS", true) == 0)
                return IFCFlowInstrumentType.VoltMeter_Rms;

            return IFCFlowInstrumentType.UserDefined;
        }

        private static IFCFlowMeterType GetFlowMeterType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCFlowMeterType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "ELECTRICMETER", true) == 0)
                return IFCFlowMeterType.ElectricMeter;
            if (String.Compare(newValue, "ENERGYMETER", true) == 0)
                return IFCFlowMeterType.EnergyMeter;
            if (String.Compare(newValue, "FLOWMETER", true) == 0)
                return IFCFlowMeterType.FlowMeter;
            if (String.Compare(newValue, "GASMETER", true) == 0)
                return IFCFlowMeterType.GasMeter;
            if (String.Compare(newValue, "OILMETER", true) == 0)
                return IFCFlowMeterType.OilMeter;
            if (String.Compare(newValue, "WATERMETER", true) == 0)
                return IFCFlowMeterType.WaterMeter;

            return IFCFlowMeterType.UserDefined;
        }

        private static IFCGasTerminalType GetGasTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCGasTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "GASAPPLIANCE", true) == 0)
                return IFCGasTerminalType.GasAppliance;
            if (String.Compare(newValue, "GASBOOSTER", true) == 0)
                return IFCGasTerminalType.GasBooster;
            if (String.Compare(newValue, "GASBURNER", true) == 0)
                return IFCGasTerminalType.GasBurner;

            return IFCGasTerminalType.UserDefined;
        }

        private static IFCHeatExchangerType GetHeatExchangerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCHeatExchangerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "PLATE", true) == 0)
                return IFCHeatExchangerType.Plate;
            if (String.Compare(newValue, "SHELLANDTUBE", true) == 0)
                return IFCHeatExchangerType.ShellAndTube;

            return IFCHeatExchangerType.UserDefined;
        }

        private static IFCHumidifierType GetHumidifierType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCHumidifierType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "STEAMINJECTION", true) == 0)
                return IFCHumidifierType.SteamInjection;
            if (String.Compare(newValue, "ADIABATICAIRWASHER", true) == 0)
                return IFCHumidifierType.AdiabaticAirWasher;
            if (String.Compare(newValue, "ADIABATICPAN", true) == 0)
                return IFCHumidifierType.AdiabaticPan;
            if (String.Compare(newValue, "ADIABATICWETTEDELEMENT", true) == 0)
                return IFCHumidifierType.AdiabaticWettedElement;
            if (String.Compare(newValue, "ADIABATICATOMIZING", true) == 0)
                return IFCHumidifierType.AdiabaticAtomizing;
            if (String.Compare(newValue, "ADIABATICULTRASONIC", true) == 0)
                return IFCHumidifierType.AdiabaticUltraSonic;
            if (String.Compare(newValue, "ADIABATICRIGIDMEDIA", true) == 0)
                return IFCHumidifierType.AdiabaticRigidMedia;
            if (String.Compare(newValue, "ADIABATICCOMPRESSEDAIRNOZZLE", true) == 0)
                return IFCHumidifierType.AdiabaticCompressedAirNozzle;
            if (String.Compare(newValue, "ASSISTEDELECTRIC", true) == 0)
                return IFCHumidifierType.AssistedElectric;
            if (String.Compare(newValue, "ASSISTEDNATURALGAS", true) == 0)
                return IFCHumidifierType.AssistedNaturalGas;
            if (String.Compare(newValue, "ASSISTEDPROPANE", true) == 0)
                return IFCHumidifierType.AssistedPropane;
            if (String.Compare(newValue, "ASSISTEDBUTANE", true) == 0)
                return IFCHumidifierType.AssistedButane;
            if (String.Compare(newValue, "ASSISTEDSTEAM", true) == 0)
                return IFCHumidifierType.AssistedSteam;

            return IFCHumidifierType.UserDefined;
        }

        private static IFCJunctionBoxType GetJunctionBoxType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCJunctionBoxType.NotDefined;

            return IFCJunctionBoxType.UserDefined;
        }

        private static IFCLampType GetLampType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCLampType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "COMPACTFLUORESCENT", true) == 0)
                return IFCLampType.CompactFluorescent;
            if (String.Compare(newValue, "FLUORESCENT", true) == 0)
                return IFCLampType.Fluorescent;
            if (String.Compare(newValue, "HIGHPRESSUREMERCURY", true) == 0)
                return IFCLampType.HighPressureMercury;
            if (String.Compare(newValue, "HIGHPRESSURESODIUM", true) == 0)
                return IFCLampType.HighPressureSodium;
            if (String.Compare(newValue, "METALHALIDE", true) == 0)
                return IFCLampType.MetalHalide;
            if (String.Compare(newValue, "TUNGSTENFILAMENT", true) == 0)
                return IFCLampType.TungstenFilament;

            return IFCLampType.UserDefined;
        }

        private static IFCLightFixtureType GetLightFixtureType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCLightFixtureType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "POINTSOURCE", true) == 0)
                return IFCLightFixtureType.PointSource;
            if (String.Compare(newValue, "DIRECTIONSOURCE", true) == 0)
                return IFCLightFixtureType.DirectionSource;

            return IFCLightFixtureType.UserDefined;
        }

        private static IFCMemberType GetMemberType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCMemberType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BRACE", true) == 0)
                return IFCMemberType.Brace;
            if (String.Compare(newValue, "CHORD", true) == 0)
                return IFCMemberType.Chord;
            if (String.Compare(newValue, "COLLAR", true) == 0)
                return IFCMemberType.Collar;
            if (String.Compare(newValue, "MEMBER", true) == 0)
                return IFCMemberType.Member;
            if (String.Compare(newValue, "MULLION", true) == 0)
                return IFCMemberType.Mullion;
            if (String.Compare(newValue, "PLATE", true) == 0)
                return IFCMemberType.Plate;
            if (String.Compare(newValue, "POST", true) == 0)
                return IFCMemberType.Post;
            if (String.Compare(newValue, "PURLIN", true) == 0)
                return IFCMemberType.Purlin;
            if (String.Compare(newValue, "RAFTER", true) == 0)
                return IFCMemberType.Rafter;
            if (String.Compare(newValue, "STRINGER", true) == 0)
                return IFCMemberType.Stringer;
            if (String.Compare(newValue, "STRUT", true) == 0)
                return IFCMemberType.Strut;
            if (String.Compare(newValue, "STUD", true) == 0)
                return IFCMemberType.Stud;

            return IFCMemberType.UserDefined;
        }

        private static IFCMotorConnectionType GetMotorConnectionType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCMotorConnectionType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BELTDRIVE", true) == 0)
                return IFCMotorConnectionType.BeltDrive;
            if (String.Compare(newValue, "COUPLING", true) == 0)
                return IFCMotorConnectionType.Coupling;
            if (String.Compare(newValue, "DIRECTDRIVE", true) == 0)
                return IFCMotorConnectionType.DirectDrive;

            return IFCMotorConnectionType.UserDefined;
        }

        private static IFCOutletType GetOutletType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCOutletType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "AUDIOVISUALOUTLET", true) == 0)
                return IFCOutletType.AudiovisualOutlet;
            if (String.Compare(newValue, "COMMUNICATIONSOUTLET", true) == 0)
                return IFCOutletType.CommunicationsOutlet;
            if (String.Compare(newValue, "POWEROUTLET", true) == 0)
                return IFCOutletType.PowerOutlet;

            return IFCOutletType.UserDefined;
        }

        private static IFCPlateType GetPlateType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCPlateType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CURTAINPANEL", true) == 0)
                return IFCPlateType.Curtain_Panel;
            if (String.Compare(newValue, "SHEET", true) == 0)
                return IFCPlateType.Sheet;

            return IFCPlateType.UserDefined;
        }

        private static IFCPipeFittingType GetPipeFittingType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCPipeFittingType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BEND", true) == 0)
                return IFCPipeFittingType.Bend;
            if (String.Compare(newValue, "CONNECTOR", true) == 0)
                return IFCPipeFittingType.Connector;
            if (String.Compare(newValue, "ENTRY", true) == 0)
                return IFCPipeFittingType.Entry;
            if (String.Compare(newValue, "EXIT", true) == 0)
                return IFCPipeFittingType.Exit;
            if (String.Compare(newValue, "JUNCTION", true) == 0)
                return IFCPipeFittingType.Junction;
            if (String.Compare(newValue, "OBSTRUCTION", true) == 0)
                return IFCPipeFittingType.Obstruction;
            if (String.Compare(newValue, "TRANSITION", true) == 0)
                return IFCPipeFittingType.Transition;

            return IFCPipeFittingType.UserDefined;
        }

        private static IFCPipeSegmentType GetPipeSegmentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCPipeSegmentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FLEXIBLESEGMENT", true) == 0)
                return IFCPipeSegmentType.FlexibleSegment;
            if (String.Compare(newValue, "RIGIDSEGMENT", true) == 0)
                return IFCPipeSegmentType.RigidSegment;
            if (String.Compare(newValue, "GUTTER", true) == 0)
                return IFCPipeSegmentType.Gutter;
            if (String.Compare(newValue, "SPOOL", true) == 0)
                return IFCPipeSegmentType.Spool;

            return IFCPipeSegmentType.UserDefined;
        }

        private static IFCProtectiveDeviceType GetProtectiveDeviceType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCProtectiveDeviceType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FUSEDISCONNECTOR", true) == 0)
                return IFCProtectiveDeviceType.FuseDisconnector;
            if (String.Compare(newValue, "CIRCUITBREAKER", true) == 0)
                return IFCProtectiveDeviceType.CircuitBreaker;
            if (String.Compare(newValue, "EARTHFAILUREDEVICE", true) == 0)
                return IFCProtectiveDeviceType.EarthFailureDevice;
            if (String.Compare(newValue, "RESIDUALCURRENTCIRCUITBREAKER", true) == 0)
                return IFCProtectiveDeviceType.ResidualCurrentCircuitBreaker;
            if (String.Compare(newValue, "RESIDUALCURRENTSWITCH", true) == 0)
                return IFCProtectiveDeviceType.ResidualCurrentSwitch;
            if (String.Compare(newValue, "VARISTOR", true) == 0)
                return IFCProtectiveDeviceType.Varistor;

            return IFCProtectiveDeviceType.UserDefined;
        }

        private static IFCPumpType GetPumpType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCPumpType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CIRCULATOR", true) == 0)
                return IFCPumpType.Circulator;
            if (String.Compare(newValue, "ENDSUCTION", true) == 0)
                return IFCPumpType.EndSuction;
            if (String.Compare(newValue, "SPLITCASE", true) == 0)
                return IFCPumpType.SplitCase;
            if (String.Compare(newValue, "VERTICALINLINE", true) == 0)
                return IFCPumpType.VerticalInline;
            if (String.Compare(newValue, "VERTICALTURBINE", true) == 0)
                return IFCPumpType.VerticalTurbine;

            return IFCPumpType.UserDefined;
        }

        private static IFCSanitaryTerminalType GetSanitaryTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCSanitaryTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BATH", true) == 0)
                return IFCSanitaryTerminalType.Bath;
            if (String.Compare(newValue, "BIDET", true) == 0)
                return IFCSanitaryTerminalType.Bidet;
            if (String.Compare(newValue, "CISTERN", true) == 0)
                return IFCSanitaryTerminalType.Cistern;
            if (String.Compare(newValue, "SHOWER", true) == 0)
                return IFCSanitaryTerminalType.Shower;
            if (String.Compare(newValue, "SINK", true) == 0)
                return IFCSanitaryTerminalType.Sink;
            if (String.Compare(newValue, "SANITARYFOUNTAIN", true) == 0)
                return IFCSanitaryTerminalType.SanitaryFountain;
            if (String.Compare(newValue, "TOILETPAN", true) == 0)
                return IFCSanitaryTerminalType.ToiletPan;
            if (String.Compare(newValue, "URINAL", true) == 0)
                return IFCSanitaryTerminalType.Urinal;
            if (String.Compare(newValue, "WASHHANDBASIN", true) == 0)
                return IFCSanitaryTerminalType.WashhandBasin;
            if (String.Compare(newValue, "WCSEAT", true) == 0)
                return IFCSanitaryTerminalType.WCSeat;

            return IFCSanitaryTerminalType.UserDefined;
        }

        private static IFCSensorType GetSensorType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCSensorType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CO2SENSOR", true) == 0)
                return IFCSensorType.Co2Sensor;
            if (String.Compare(newValue, "FIRESENSOR", true) == 0)
                return IFCSensorType.FireSensor;
            if (String.Compare(newValue, "FLOWSENSOR", true) == 0)
                return IFCSensorType.FlowSensor;
            if (String.Compare(newValue, "GASSENSOR", true) == 0)
                return IFCSensorType.GasSensor;
            if (String.Compare(newValue, "HEATSENSOR", true) == 0)
                return IFCSensorType.HeatSensor;
            if (String.Compare(newValue, "HUMIDITYSENSOR", true) == 0)
                return IFCSensorType.HumiditySensor;
            if (String.Compare(newValue, "LIGHTSENSOR", true) == 0)
                return IFCSensorType.LightSensor;
            if (String.Compare(newValue, "MOISTURESENSOR", true) == 0)
                return IFCSensorType.MoistureSensor;
            if (String.Compare(newValue, "MOVEMENTSENSOR", true) == 0)
                return IFCSensorType.MovementSensor;
            if (String.Compare(newValue, "PRESSURESENSOR", true) == 0)
                return IFCSensorType.PressureSensor;
            if (String.Compare(newValue, "SMOKESENSOR", true) == 0)
                return IFCSensorType.SmokeSensor;
            if (String.Compare(newValue, "SOUNDSENSOR", true) == 0)
                return IFCSensorType.SoundSensor;
            if (String.Compare(newValue, "TEMPERATURESENSOR", true) == 0)
                return IFCSensorType.TemperatureSensor;

            return IFCSensorType.UserDefined;
        }

        private static IFCSpaceHeaterType GetSpaceHeaterType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCSpaceHeaterType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "SECTIONALRADIATOR", true) == 0)
                return IFCSpaceHeaterType.SectionalRadiator;
            if (String.Compare(newValue, "PANELRADIATOR", true) == 0)
                return IFCSpaceHeaterType.PanelRadiator;
            if (String.Compare(newValue, "TUBULARRADIATOR", true) == 0)
                return IFCSpaceHeaterType.TubularRadiator;
            if (String.Compare(newValue, "CONVECTOR", true) == 0)
                return IFCSpaceHeaterType.Convector;
            if (String.Compare(newValue, "BASEBOARDHEATER", true) == 0)
                return IFCSpaceHeaterType.BaseBoardHeater;
            if (String.Compare(newValue, "FINNEDTUBEUNIT", true) == 0)
                return IFCSpaceHeaterType.FinnedTubeUnit;
            if (String.Compare(newValue, "UNITHEATER", true) == 0)
                return IFCSpaceHeaterType.UnitHeater;

            return IFCSpaceHeaterType.UserDefined;
        }

        private static IFCStackTerminalType GetStackTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCStackTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "BIRDCAGE", true) == 0)
                return IFCStackTerminalType.BirdCage;
            if (String.Compare(newValue, "COWL", true) == 0)
                return IFCStackTerminalType.Cowl;
            if (String.Compare(newValue, "RAINWATERHOPPER", true) == 0)
                return IFCStackTerminalType.RainwaterHopper;

            return IFCStackTerminalType.UserDefined;
        }

        private static IFCSwitchingDeviceType GetSwitchingDeviceType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCSwitchingDeviceType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CONTACTOR", true) == 0)
                return IFCSwitchingDeviceType.Contactor;
            if (String.Compare(newValue, "EMERGENCYSTOP", true) == 0)
                return IFCSwitchingDeviceType.EmergencyStop;
            if (String.Compare(newValue, "STARTER", true) == 0)
                return IFCSwitchingDeviceType.Starter;
            if (String.Compare(newValue, "SWITCHDISCONNECTOR", true) == 0)
                return IFCSwitchingDeviceType.SwitchDisconnector;
            if (String.Compare(newValue, "TOGGLESWITCH", true) == 0)
                return IFCSwitchingDeviceType.ToggleSwitch;

            return IFCSwitchingDeviceType.UserDefined;
        }

        private static IFCTankType GetTankType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCTankType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "PREFORMED", true) == 0)
                return IFCTankType.Preformed;
            if (String.Compare(newValue, "SECTIONAL", true) == 0)
                return IFCTankType.Sectional;
            if (String.Compare(newValue, "EXPANSION", true) == 0)
                return IFCTankType.Expansion;
            if (String.Compare(newValue, "PRESSUREVESSEL", true) == 0)
                return IFCTankType.PressureVessel;

            return IFCTankType.UserDefined;
        }

        private static IFCTransformerType GetTransformerType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCTransformerType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "CURRENT", true) == 0)
                return IFCTransformerType.Current;
            if (String.Compare(newValue, "FREQUENCY", true) == 0)
                return IFCTransformerType.Frequency;
            if (String.Compare(newValue, "VOLTAGE", true) == 0)
                return IFCTransformerType.Voltage;

            return IFCTransformerType.UserDefined;
        }

        private static IFCTransportElementType GetTransportElementType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCTransportElementType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "ELEVATOR", true) == 0)
                return IFCTransportElementType.Elevator;
            if (String.Compare(newValue, "ESCALATOR", true) == 0)
                return IFCTransportElementType.Escalator;
            if (String.Compare(newValue, "MOVINGWALKWAY", true) == 0)
                return IFCTransportElementType.MovingWalkWay;

            return IFCTransportElementType.UserDefined;
        }

        private static IFCTubeBundleType GetTubeBundleType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCTubeBundleType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FINNED", true) == 0)
                return IFCTubeBundleType.Finned;

            return IFCTubeBundleType.UserDefined;
        }

        private static IFCUnitaryEquipmentType GetUnitaryEquipmentType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCUnitaryEquipmentType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "AIRHANDLER", true) == 0)
                return IFCUnitaryEquipmentType.AirHandler;
            if (String.Compare(newValue, "AIRCONDITIONINGUNIT", true) == 0)
                return IFCUnitaryEquipmentType.AirConditioningUnit;
            if (String.Compare(newValue, "SPLITSYSTEM", true) == 0)
                return IFCUnitaryEquipmentType.SplitSystem;
            if (String.Compare(newValue, "ROOFTOPUNIT", true) == 0)
                return IFCUnitaryEquipmentType.RoofTopUnit;

            return IFCUnitaryEquipmentType.UserDefined;
        }

        private static IFCValveType GetValveType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCValveType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "AIRRELEASE", true) == 0)
                return IFCValveType.AirRelease;
            if (String.Compare(newValue, "ANTIVACUUM", true) == 0)
                return IFCValveType.AntiVacuum;
            if (String.Compare(newValue, "CHANGEOVER", true) == 0)
                return IFCValveType.ChangeOver;
            if (String.Compare(newValue, "CHECK", true) == 0)
                return IFCValveType.Check;
            if (String.Compare(newValue, "COMMISSIONING", true) == 0)
                return IFCValveType.Commissioning;
            if (String.Compare(newValue, "DIVERTING", true) == 0)
                return IFCValveType.Diverting;
            if (String.Compare(newValue, "DRAWOFFCOCK", true) == 0)
                return IFCValveType.DrawOffCock;
            if (String.Compare(newValue, "DOUBLECHECK", true) == 0)
                return IFCValveType.DoubleCheck;
            if (String.Compare(newValue, "DOUBLEREGULATING", true) == 0)
                return IFCValveType.DoubleRegulating;
            if (String.Compare(newValue, "FAUCET", true) == 0)
                return IFCValveType.Faucet;
            if (String.Compare(newValue, "FLUSHING", true) == 0)
                return IFCValveType.Flushing;
            if (String.Compare(newValue, "GASCOCK", true) == 0)
                return IFCValveType.GasCock;
            if (String.Compare(newValue, "GASTAP", true) == 0)
                return IFCValveType.GasTap;
            if (String.Compare(newValue, "ISOLATING", true) == 0)
                return IFCValveType.Isolating;
            if (String.Compare(newValue, "MIXING", true) == 0)
                return IFCValveType.Mixing;
            if (String.Compare(newValue, "PRESSUREREDUCING", true) == 0)
                return IFCValveType.PressureReducing;
            if (String.Compare(newValue, "PRESSURERELIEF", true) == 0)
                return IFCValveType.PressureRelief;
            if (String.Compare(newValue, "REGULATING", true) == 0)
                return IFCValveType.Regulating;
            if (String.Compare(newValue, "SAFETYCUTOFF", true) == 0)
                return IFCValveType.SafetyCutoff;
            if (String.Compare(newValue, "STEAMTRAP", true) == 0)
                return IFCValveType.SteamTrap;
            if (String.Compare(newValue, "STOPCOCK", true) == 0)
                return IFCValveType.StopCock;

            return IFCValveType.UserDefined;
        }

        private static IFCWasteTerminalType GetWasteTerminalType(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCWasteTerminalType.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "FLOORTRAP", true) == 0)
                return IFCWasteTerminalType.FloorTrap;
            if (String.Compare(newValue, "FLOORWASTE", true) == 0)
                return IFCWasteTerminalType.FloorWaste;
            if (String.Compare(newValue, "GULLYSUMP", true) == 0)
                return IFCWasteTerminalType.GullySump;
            if (String.Compare(newValue, "GULLYTRAP", true) == 0)
                return IFCWasteTerminalType.GullyTrap;
            if (String.Compare(newValue, "GREASEINTERCEPTOR", true) == 0)
                return IFCWasteTerminalType.GreaseInterceptor;
            if (String.Compare(newValue, "OILINTERCEPTOR", true) == 0)
                return IFCWasteTerminalType.OilInterceptor;
            if (String.Compare(newValue, "PETROLINTERCEPTOR", true) == 0)
                return IFCWasteTerminalType.PetrolInterceptor;
            if (String.Compare(newValue, "ROOFDRAIN", true) == 0)
                return IFCWasteTerminalType.RoofDrain;
            if (String.Compare(newValue, "WASTEDISPOSALUNIT", true) == 0)
                return IFCWasteTerminalType.WasteDisposalUnit;
            if (String.Compare(newValue, "WASTETRAP", true) == 0)
                return IFCWasteTerminalType.WasteTrap;

            return IFCWasteTerminalType.UserDefined;
        }

        private static IFCAssemblyPlace GetAssemblyPlace(Element element, string ifcEnumType)
        {
            string value = null;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "IfcType", out value))
            {
                value = ifcEnumType;
            }

            if (String.IsNullOrEmpty(value))
                return IFCAssemblyPlace.NotDefined;

            string newValue = value.Replace(" ", "").Replace("_", "");

            if (String.Compare(newValue, "SITE", true) == 0)
                return IFCAssemblyPlace.Site;
            if (String.Compare(newValue, "FACTORY", true) == 0)
                return IFCAssemblyPlace.Factory;

            return IFCAssemblyPlace.NotDefined;
        }

        public static List<GeometryObject> RemoveSolidsAndMeshesSetToDontExport(Document doc, ExporterIFC exporterIFC, IList<Solid> solids, IList<Mesh> polyMeshes)
        {
            List<GeometryObject> geomObjectsIn = new List<GeometryObject>();
            geomObjectsIn.AddRange(solids);
            geomObjectsIn.AddRange(polyMeshes);

            List<GeometryObject> geomObjectsOut = new List<GeometryObject>();

            foreach (GeometryObject obj in geomObjectsIn)
            {
                GraphicsStyle gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                if (gStyle != null)
                {
                    ElementId catId = gStyle.GraphicsStyleCategory.Id;
                    string ifcClassName = ExporterIFCUtils.GetIFCClassNameByCategory(catId, exporterIFC);
                    if (ifcClassName != "")
                    {
                        bool foundName = String.Compare(ifcClassName, "Default", true) != 0;
                        if (foundName)
                        {
                            IFCExportType exportType = ElementFilteringUtil.GetExportTypeFromClassName(ifcClassName);
                            if (exportType == IFCExportType.DontExport)
                                continue;
                        }
                    }
                }
                geomObjectsOut.Add(obj);
            }

            return geomObjectsOut;
        }

    }
}
