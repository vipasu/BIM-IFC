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

namespace BIM.IFC.Toolkit
{
    /// <summary>
    /// Defines the basic configuration of the window type in terms of the number of window panels and the subdivision of the total window.
    /// </summary>
    enum IFCWindowStyleOperation
    {
        Single_Panel,
        Double_Panel_Vertical,
        Double_Panel_Horizontal,
        Triple_Panel_Vertical,
        Triple_Panel_Bottom,
        Triple_Panel_Top,
        Triple_Panel_Left,
        Triple_Panel_Right,
        Triple_Panel_Horizontal,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the basic types of construction of windows.
    /// </summary>
    enum IFCWindowStyleConstruction
    {
        Aluminium,
        High_Grade_Steel,
        Steel,
        Wood,
        Aluminium_Wood,
        Plastic,
        Other_Construction,
        NotDefined
    }

    /// <summary>
    /// Defines the basic configuration of the window type in terms of the location of window panels.
    /// </summary>
    enum IFCWindowPanelPosition
    {
        Left,
        Middle,
        Right,
        Bottom,
        Top,
        NotDefined
    }

    /// <summary>
    /// Defines the basic ways to describe how window panels operate. 
    /// </summary>
    enum IFCWindowPanelOperation
    {
        SideHungRightHand,
        SideHungLeftHand,
        TiltAndTurnRightHand,
        TiltAndTurnLeftHand,
        TopHung,
        BottomHung,
        PivotHorizontal,
        PivotVertical,
        SlidingHorizontal,
        SlidingVertical,
        RemovableCasement,
        FixedCasement,
        OtherOperation,
        NotDefined
    }

    /// <summary>
    /// Determines the direction of the text characters in respect to each other.
    /// </summary>
    enum IFCTextPath
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Defines a list of commonly shared property set definitions of a slab and an optional set of product representations.
    /// </summary>
    enum IFCSlabType
    {
        Floor,
        Roof,
        Landing,
        BaseSlab,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the different types of spaces or space boundaries in terms of either being inside the building or outside the building.
    /// </summary>
    public enum IFCInternalOrExternal
    {
        Internal,
        External,
        NotDefined
    }

    /// <summary>
    /// Defines the different types of space boundaries in terms of its physical manifestation.
    /// </summary>
    public enum IFCPhysicalOrVirtual
    {
        Physical,
        Virtual,
        NotDefined
    }

    /// <summary>
    /// Enumeration denoting whether sense of direction is positive or negative along the given axis.
    /// </summary>
    enum IFCDirectionSense
    {
        Positive,
        Negative
    }

    /// <summary>
    /// Identification of the axis of element geometry, denoting the layer set thickness direction, or direction of layer offsets.
    /// </summary>
    enum IFCLayerSetDirection
    {
        Axis1,
        Axis2,
        Axis3
    }

    /// <summary>
    /// Defines the various representation types that can be semantically distinguished.
    /// </summary>
    enum IFCGeometricProjection
    {
        Graph_View,
        Sketch_View,
        Model_View,
        Plan_View,
        Reflected_Plan_View,
        Section_View,
        Elevation_View,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the generic footing type.
    /// </summary>
    enum IFCFootingType
    {
        Footing_Beam,
        Pad_Footing,
        Pile_Cap,
        Strip_Footing,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the basic types of construction of doors.
    /// </summary>
    enum IFCDoorStyleConstruction
    {
        Aluminium,
        High_Grade_Steel,
        Steel,
        Wood,
        Aluminium_Wood,
        Aluminium_Plastic,
        Plastic,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the basic ways to describe how doors operate. 
    /// </summary>
    enum IFCDoorStyleOperation
    {
        Single_Swing_Left,
        Single_Swing_Right,
        Double_Door_Single_Swing,
        Double_Door_Single_Swing_Opposite_Left,
        Double_Door_Single_Swing_Opposite_Right,
        Double_Swing_Left,
        Double_Swing_Right,
        Double_Door_Double_Swing,
        Sliding_To_Left,
        Sliding_To_Right,
        Double_Door_Sliding,
        Folding_To_Left,
        Folding_To_Right,
        Double_Door_Folding,
        Revolving,
        RollingUp,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the basic ways to describe the location of a door panel within a door lining.
    /// </summary>
    enum IFCDoorPanelPosition
    {
        Left,
        Middle,
        Right,
        NotDefined
    }

    /// <summary>
    /// Defines the basic ways how individual door panels operate. 
    /// </summary>
    enum IFCDoorPanelOperation
    {
        Swinging,
        Double_Acting,
        Sliding,
        Folding,
        Revolving,
        RollingUp,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the flow direction at a connection point as either a Source, Sink, or both SourceAndSink.
    /// </summary>
    enum IFCFlowDirection
    {
        Source,
        Sink,
        SourceAndSink,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining where the assembly is intended to take place, either in a factory or on the building site.
    /// </summary>
    enum IFCAssemblyPlace
    {
        Site,
        Factory,
        NotDefined
    }

    /// <summary>
    /// Defines different types of standard assemblies.
    /// </summary>
    enum IFCElementAssemblyType
    {
        Accessory_Assembly,
        Arch,
        Beam_Grid,
        Braced_Frame,
        Girder,
        Reinforcement_Unit,
        Rigid_Frame,
        Slab_Field,
        Truss,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of waste terminal that can be specified.
    /// </summary>
    enum IFCWasteTerminalType
    {
        FloorTrap,
        FloorWaste,
        GullySump,
        GullyTrap,
        GreaseInterceptor,
        OilInterceptor,
        PetrolInterceptor,
        RoofDrain,
        WasteDisposalUnit,
        WasteTrap,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of valve that can be specified.
    /// </summary>
    enum IFCValveType
    {
        AirRelease,
        AntiVacuum,
        ChangeOver,
        Check,
        Commissioning,
        Diverting,
        DrawOffCock,
        DoubleCheck,
        DoubleRegulating,
        Faucet,
        Flushing,
        GasCock,
        GasTap,
        Isolating,
        Mixing,
        PressureReducing,
        PressureRelief,
        Regulating,
        SafetyCutoff,
        SteamTrap,
        StopCock,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the functional type of unitary equipment.
    /// </summary>
    enum IFCUnitaryEquipmentType
    {
        AirHandler,
        AirConditioningUnit,
        SplitSystem,
        RoofTopUnit,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of tube bundles.
    /// </summary>
    enum IFCTubeBundleType
    {
        Finned,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies primary transport element types.
    /// </summary>
    enum IFCTransportElementType
    {
        Elevator,
        Escalator,
        MovingWalkWay,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// 
    /// </summary>
    enum IFCTransformerType
    {
        Current,
        Frequency,
        Voltage,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of transformer that can be specified.
    /// </summary>
    enum IFCTankType
    {
        Preformed,
        Sectional,
        Expansion,
        PressureVessel,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of switch that can be specified.
    /// </summary>
    enum IFCSwitchingDeviceType
    {
        Contactor,
        EmergencyStop,
        Starter,
        SwitchDisconnector,
        ToggleSwitch,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of stack terminal that can be specified for use at the top of a vertical stack subsystem.
    /// </summary>
    enum IFCStackTerminalType
    {
        BirdCage,
        Cowl,
        RainwaterHopper,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the functional type of space heater.
    /// </summary>
    enum IFCSpaceHeaterType
    {
        SectionalRadiator,
        PanelRadiator,
        TubularRadiator,
        Convector,
        BaseBoardHeater,
        FinnedTubeUnit,
        UnitHeater,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of sensor that can be specified.
    /// </summary>
    enum IFCSensorType
    {
        Co2Sensor,
        FireSensor,
        FlowSensor,
        GasSensor,
        HeatSensor,
        HumiditySensor,
        LightSensor,
        MoistureSensor,
        MovementSensor,
        PressureSensor,
        SmokeSensor,
        SoundSensor,
        TemperatureSensor,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of sanitary terminal that can be specified.
    /// </summary>
    enum IFCSanitaryTerminalType
    {
        Bath,
        Bidet,
        Cistern,
        Shower,
        Sink,
        SanitaryFountain,
        ToiletPan,
        Urinal,
        WashhandBasin,
        WCSeat,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines general types of pumps.
    /// </summary>
    enum IFCPumpType
    {
        Circulator,
        EndSuction,
        SplitCase,
        VerticalInline,
        VerticalTurbine,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different breaker unit types that can be used in conjunction with protective device.
    /// </summary>
    enum IFCProtectiveDeviceType
    {
        FuseDisconnector,
        CircuitBreaker,
        EarthFailureDevice,
        ResidualCurrentCircuitBreaker,
        ResidualCurrentSwitch,
        Varistor,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies the primary purpose of a pipe segment.
    /// </summary>
    enum IFCPipeSegmentType
    {
        FlexibleSegment,
        RigidSegment,
        Gutter,
        Spool,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies the primary purpose of a pipe fitting.
    /// </summary>
    enum IFCPipeFittingType
    {
        Bend,
        Connector,
        Entry,
        Exit,
        Junction,
        Obstruction,
        Transition,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the different types of planar elements.
    /// </summary>
    enum IFCPlateType
    {
        Curtain_Panel,
        Sheet,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of outlet that can be specified.
    /// </summary>
    enum IFCOutletType
    {
        AudiovisualOutlet,
        CommunicationsOutlet,
        PowerOutlet,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of motor connection that can be specified.
    /// </summary>
    enum IFCMotorConnectionType
    {
        BeltDrive,
        Coupling,
        DirectDrive,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the different types of linear elements an IfcMemberType object can fulfill.
    /// </summary>
    enum IFCMemberType
    {
        Brace,
        Chord,
        Collar,
        Member,
        Mullion,
        Plate,
        Post,
        Purlin,
        Rafter,
        Stringer,
        Strut,
        Stud,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of light fixture available.
    /// </summary>
    enum IFCLightFixtureType
    {
        PointSource,
        DirectionSource,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of lamp available.
    /// </summary>
    enum IFCLampType
    {
        CompactFluorescent,
        Fluorescent,
        HighPressureMercury,
        HighPressureSodium,
        MetalHalide,
        TungstenFilament,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of types of junction boxes available.
    /// </summary>
    enum IFCJunctionBoxType
    {
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of humidifiers.
    /// </summary>
    enum IFCHumidifierType
    {
        SteamInjection,
        AdiabaticAirWasher,
        AdiabaticPan,
        AdiabaticWettedElement,
        AdiabaticAtomizing,
        AdiabaticUltraSonic,
        AdiabaticRigidMedia,
        AdiabaticCompressedAirNozzle,
        AssistedElectric,
        AssistedNaturalGas,
        AssistedPropane,
        AssistedButane,
        AssistedSteam,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of heat exchangers.
    /// </summary>
    enum IFCHeatExchangerType
    {
        Plate,
        ShellAndTube,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the functional type of gas terminal.
    /// </summary>
    enum IFCGasTerminalType
    {
        GasAppliance,
        GasBooster,
        GasBurner,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines various types of flow meter.
    /// </summary>
    enum IFCFlowMeterType
    {
        ElectricMeter,
        EnergyMeter,
        FlowMeter,
        GasMeter,
        OilMeter,
        WaterMeter,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of flow instrument that can be specified.
    /// </summary>
    enum IFCFlowInstrumentType
    {
        PressureGauge,
        Thermometer,
        Ammeter,
        FrequencyMeter,
        PowerFactorMeter,
        PhaseAngleMeter,
        VoltMeter_Peak,
        VoltMeter_Rms,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of fire suppression terminal that can be specified.
    /// </summary>
    enum IFCFireSuppressionTerminalType
    {
        BreechingInlet,
        FireHydrant,
        HoseReel,
        Sprinkler,
        SprinklerDeflector,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the various types of filter typically used within building services distribution systems.
    /// </summary>
    enum IFCFilterType
    {
        AirParticleFilter,
        OdorFilter,
        OilFilter,
        Strainer,
        WaterFilter,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of fans.
    /// </summary>
    enum IFCFanType
    {
        CentrifugalForwardCurved,
        CentrifugalRadial,
        CentrifugalBackwardInclinedCurved,
        CentrifugalAirfoil,
        TubeAxial,
        VaneAxial,
        PropellorAxial,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of evaporators.
    /// </summary>
    enum IFCEvaporatorType
    {
        DirectExpansionShellAndTube,
        DirectExpansionTubeInTube,
        DirectExpansionBrazedPlate,
        FloodedShellAndTube,
        ShellAndCoil,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of evaporative coolers. 
    /// </summary>
    enum IFCEvaporativeCoolerType
    {
        DirectEvaporativeRandomMediaAirCooler,
        DirectEvaporativeRigidMediaAirCooler,
        DirectEvaporativeSlingersPackagedAirCooler,
        DirectEvaporativePackagedRotaryAirCooler,
        DirectEvaporativeAirWasher,
        IndirectEvaporativePackageAirCooler,
        IndirectEvaporativeWetCoil,
        IndirectEvaporativeCoolingTowerOrCoilCooler,
        IndirectDirectCombination,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of types of electrical time control available.
    /// </summary>
    enum IFCElectricTimeControlType
    {
        TimeClock,
        TimeDelay,
        Relay,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of electric motor that can be specified.
    /// </summary>
    enum IFCElectricMotorType
    {
        DC,
        Induction,
        Polyphase,
        ReluctanceSynchronous,
        Synchronous,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of types of electric heater available.
    /// </summary>
    enum IFCElectricHeaterType
    {
        ElectricPointHeater,
        ElectricCableHeater,
        ElectricMatHeater,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of types of electric generators available.
    /// </summary>
    enum IFCElectricGeneratorType
    {
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of electrical flow storage device available.
    /// </summary>
    enum IFCElectricFlowStorageDeviceType
    {
        Battery,
        CapacitorBank,
        HarmonicFilter,
        InductorBank,
        Ups,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of electrical appliance that can be specified.
    /// </summary>
    enum IFCElectricApplianceType
    {
        Computer,
        DirectWaterHeater,
        DishWasher,
        ElectricCooker,
        ElectricHeater,
        Facsimile,
        FreeStandingFan,
        Freezer,
        Fridge_Freezer,
        HandDryer,
        IndirectWaterHeater,
        Microwave,
        PhotoCopier,
        Printer,
        Refrigerator,
        RadianTheater,
        Scanner,
        Telephone,
        TumbleDryer,
        TV,
        VendingMachine,
        WashingMachine,
        WaterHeater,
        WaterCooler,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of duct silencers.
    /// </summary>
    enum IFCDuctSilencerType
    {
        FlatOval,
        Rectangular,
        Round,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies the primary purpose of a duct segment. 
    /// </summary>
    enum IFCDuctSegmentType
    {
        RigidSegment,
        FlexibleSegment,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies the primary purpose of a duct fitting.
    /// </summary>
    enum IFCDuctFittingType
    {
        Bend,
        Connector,
        Entry,
        Exit,
        Junction,
        Obstruction,
        Transition,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies different types of distribution chambers.
    /// </summary>
    enum IFCDistributionChamberElementType
    {
        FormedDuct,
        InspectionChamber,
        InspectionPit,
        Manhole,
        MeterChamber,
        Sump,
        Trench,
        ValveChamber,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the various types of damper.
    /// </summary>
    enum IFCDamperType
    {
        ControlDamper,
        FireDamper,
        SmokeDamper,
        FireSmokeDamper,
        BackDraftDamper,
        ReliefDamper,
        BlastDamper,
        GravityDamper,
        GravityReliefDamper,
        BalancingDamper,
        FumeHoodExhaust,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of cooling towers.
    /// </summary>
    enum IFCCoolingTowerType
    {
        NaturalDraft,
        MechanicalInducedDraft,
        MechanicalForcedDraft,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of cooled beams.
    /// </summary>
    enum IFCCooledBeamType
    {
        Active,
        Passive,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of controller that can be specified.
    /// </summary>
    enum IFCControllerType
    {
        Floating,
        Proportional,
        ProportionalIntegral,
        ProportionalIntegralDerivative,
        TimedTwoPosition,
        TwoPosition,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of condensers.
    /// </summary>
    enum IFCCondenserType
    {
        WaterCooledShellTube,
        WaterCooledShellCoil,
        WaterCooledTubeInTube,
        WaterCooledBrazedPlate,
        AirCooled,
        EvaporativeCooled,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Types of compressors.
    /// </summary>
    enum IFCCompressorType
    {
        Dynamic,
        Reciprocating,
        Rotary,
        Scroll,
        Trochoidal,
        SingleStage,
        Booster,
        OpenType,
        Hermetic,
        SemiHermetic,
        WeldedShellHermetic,
        RollingPiston,
        RotaryVane,
        SingleScrew,
        TwinScrew,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of coils.
    /// </summary>
    enum IFCCoilType
    {
        DXCoolingCoil,
        WaterCoolingCoil,
        SteamHeatingCoil,
        WaterHeatingCoil,
        ElectricHeatingCoil,
        GasHeatingCoil,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of Chillers classified by their method of heat rejection.
    /// </summary>
    enum IFCChillerType
    {
        AirCooled,
        WaterCooled,
        HeatRecovery,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of cable segment that can be specified.
    /// </summary>
    enum IFCCableSegmentType
    {
        CableSegment,
        ConductorSegment,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of cable carrier segment that can be specified.
    /// </summary>
    enum IFCCableCarrierSegmentType
    {
        CableLadderSEGMENT,
        CableTraySegment,
        CableTrunkingSegment,
        ConduitSegment,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of cable carrier fitting that can be specified.
    /// </summary>
    enum IFCCableCarrierFittingType
    {
        Bend,
        Cross,
        Reducer,
        Tee,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the typical types of boilers.
    /// </summary>
    enum IFCBoilerType
    {
        Water,
        Steam,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of alarm that can be specified.
    /// </summary>
    enum IFCAlarmType
    {
        Bell,
        BreakGlassButton,
        Light,
        ManualPullBox,
        Siren,
        Whistle,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines general types of pumps.
    /// </summary>
    enum IFCAirToAirHeatRecoveryType
    {
        FixedPlateCounterFlowExchanger,
        FixedPlateCrossFlowExchanger,
        FixedPlateParallelFlowExchanger,
        RotaryWheel,
        RunaroundCoilloop,
        HeatPipe,
        TwinTowerEnthalpyRecoveryLoops,
        ThermosiphonSealedTubeHeatExchangers,
        ThermosiphonCoilTypeHeatExchangers,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Enumeration defining the functional types of air terminals.
    /// </summary>
    enum IFCAirTerminalType
    {
        Grille,
        Register,
        Diffuser,
        EyeBall,
        Iris,
        LinearGrille,
        LinearDiffuser,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Identifies different types of air terminal boxes. 
    /// </summary>
    enum IFCAirTerminalBoxType
    {
        ConstantFlow,
        VariableFlowPressureDependant,
        VariableFlowPressureIndependant,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of actuator that can be specified.
    /// </summary>
    enum IFCActuatorType
    {
        ElectricActuator,
        HandOperatedActuator,
        HydraulicActuator,
        PneumaticActuator,
        ThermostaticActuator,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the different types of linear elements an IfcColumnType object can fulfill.
    /// </summary>
    enum IFCColumnType
    {
        Column,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// The name of an SI unit.
    /// </summary>
    enum IFCSIUnitName
    {
        Ampere,
        Becquerel,
        Candela,
        Coulomb,
        Cubic_Metre,
        Degree_Celsius,
        Farad,
        Gram,
        Gray,
        Henry,
        Hertz,
        Joule,
        Kelvin,
        Lumen,
        Lux,
        Metre,
        Mole,
        Newton,
        Ohm,
        Pascal,
        Radian,
        Second,
        Siemens,
        Sievert,
        Square_Metre,
        Steradian,
        Tesla,
        Volt,
        Watt,
        Weber
    }

    /// <summary>
    /// The name of a prefix that may be associated with an SI unit.
    /// </summary>
    enum IFCSIPrefix
    {
        Exa,
        Peta,
        Tera,
        Giga,
        Mega,
        Kilo,
        Hecto,
        Deca,
        Deci,
        Centi,
        Milli,
        Micro,
        Nano,
        Pico,
        Femto,
        Atto
    }

    /// <summary>
    /// Allowed unit types of IfcNamedUnit. 
    /// </summary>
    enum IFCUnit
    {
        AbsorbedDoseUnit,
        AmountOfSubstanceUnit,
        AreaUnit,
        DoseEquivalentUnit,
        ElectricCapacitanceUnit,
        ElectricChargeUnit,
        ElectricConductanceUnit,
        ElectricCurrentUnit,
        ElectricResistanceUnit,
        ElectricVoltageUnit,
        EnergyUnit,
        ForceUnit,
        FrequencyUnit,
        IlluminanceUnit,
        InductanceUnit,
        LengthUnit,
        LuminousFluxUnit,
        LuminousIntensityUnit,
        MagneticFluxDensityUnit,
        MagneticFluxUnit,
        MassUnit,
        PlaneAngleUnit,
        PowerUnit,
        PressureUnit,
        RadioActivityUnit,
        SolidAngleUnit,
        ThermoDynamicTemperatureUnit,
        TimeUnit,
        VolumeUnit,
        UserDefined
    }

    /// <summary>
    /// Identifies the logical location of the address.
    /// </summary>
    enum IFCAddressType
    {
        Office,
        Site,
        Home,
        DistributionPoint,
        UserDefined
    }

    /// <summary>
    /// Enumeration identifying the type of change that might have occurred to the object during the last session.
    /// </summary>
    enum IFCChangeAction
    {
        NoChange,
        Modified,
        Added,
        Deleted,
        ModifiedAdded,
        ModifiedDeleted
    }

    /// <summary>
    /// Enumeration identifying the state or accessibility of the object.
    /// </summary>
    enum IFCState
    {
        ReadWrite,
        ReadOnly,
        Locked,
        ReadWriteLocked,
        ReadOnlyLocked
    }

    /// <summary>
    /// Indicates the element composition type.
    /// </summary>
    enum IFCElementComposition
    {
        Complex,
        Element,
        Partial
    }

    /// <summary>
    /// Defines the applicable object categories.
    /// </summary>
    enum IFCObjectType
    {
        Product,
        Process,
        Control,
        Resource,
        Actor,
        Group,
        Project,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of covering that can further specify an IfcCovering or an IfcCoveringType. 
    /// </summary>
    enum IFCCoveringType
    {
        Ceiling,
        Flooring,
        Cladding,
        Roofing,
        Insulation,
        Membrane,
        Sleeving,
        Wrapping,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the range of different types of covering that can further specify an IfcRailing
    /// </summary>
    enum IFCRailingType
    {
        HandRail,
        GuardRail,
        Balustrade,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the types of IfcReinforcingBar roles
    /// </summary>
    enum IFCReinforcingBarRole
    {
        Main,
        Shear,
        Ligature,
        Stud,
        Punching,
        Edge,
        Ring,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines reflectance methods for IfcSurfaceStyleRendering
    /// </summary>
    enum IFCReflectanceMethod
    {
        Blinn,
        Flat,
        Glass,
        Matt,
        Metal,
        Mirror,
        Phong,
        Plastic,
        Strauss,
        NotDefined
    }

    /// <summary>
    /// Defines the types of IfcReinforcingBar surfaces
    /// </summary>
    enum IFCReinforcingBarSurface
    {
        Plain,
        Textured
    }

    /// <summary>
    /// Defines the basic configuration of the roof in terms of the different roof shapes. 
    /// </summary>
    enum IFCRoofType
    {
        Flat_Roof,
        Shed_Roof,
        Gable_Roof,
        Hip_Roof,
        Hipped_Gable_Roof,
        Gambrel_Roof,
        Mansard_Roof,
        Barrel_Roof,
        Rainbow_Roof,
        Butterfly_Roof,
        Pavilion_Roof,
        Dome_Roof,
        FreeForm,
        NotDefined
    }

    /// <summary>
    /// Defines the basic configuration of the ramps in terms of the different ramp shapes. 
    /// </summary>
    enum IFCRampType
    {
        Straight_Run_Ramp,
        Two_Straight_Run_Ramp,
        Quarter_Turn_Ramp,
        Two_Quarter_Turn_Ramp,
        Half_Turn_Ramp,
        Spiral_Ramp,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines the basic configuration of the stairs in terms of the different stair shapes. 
    /// </summary>
    enum IFCStairType
    {
        Straight_Run_Stair,
        Two_Straight_Run_Stair,
        Quarter_Winding_Stair,
        Quarter_Turn_Stair,
        Half_Winding_Stair,
        Half_Turn_Stair,
        Two_Quarter_Winding_Stair,
        Two_Quarter_Turn_Stair,
        Three_Quarter_Winding_Stair,
        Three_Quarter_Turn_Stair,
        Spiral_Stair,
        Double_Return_Stair,
        Curved_Run_Stair,
        Two_Curved_Run_Stair,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Defines suface sides for IfcSurfaceStyle
    /// </summary>
    enum IFCSurfaceSide
    {
        Positive,
        Negative,
        Both
    }

    /// <summary>
    /// Defines the different ways how path based elements can connect.
    /// </summary>
    enum IFCConnectionType
    {
        AtPath,
        AtStart,
        AtEnd,
        NotDefined
    }

    /// <summary>
    /// Defines the types of occupant from which the type required can be selected.
    /// </summary>
    enum IFCOccupantType
    {
        Assignee,
        Assignor,
        Lessee,
        Lessor,
        LettingAgent,
        Owner,
        Tenant,
        UserDefined,
        NotDefined
    }

    /// <summary>
    /// Roles which may be played by an actor.
    /// </summary>
    enum IFCRoleEnum
    {
        Supplier,
        Manufacturer,
        Contractor,
        Subcontractor,
        Architect,
        StructuralEngineer,
        CostEngineer,
        Client,
        BuildingOwner,
        BuildingOperator,
        MechanicalEngineer,
        ElectricalEngineer,
        ProjectManager,
        FacilitiesManager,
        CivilEngineer,
        CommissioningEngineer,
        Engineer,
        Owner,
        Consultant,
        ConstructionManager,
        FieldConstructionManager,
        Reseller,
        UserDefined
    }

    /// <summary>
    /// Defines the range of different types of profiles.
    /// </summary>
    enum IFCProfileType
    {
        Curve,
        Area
    }

    /// <summary>
    /// Defines the boolean operators used in clipping.
    /// </summary>
    enum IFCBooleanOperator
    {
        Union,
        Intersection,
        Difference
    }

    /// <summary>
    /// Defines the transition type used by compositive curve segments.
    /// </summary>
    enum IFCTransitionCode
    {
        Discontinuous,
        Continuous,
        ContSameGradient,
        ContSameGradientSameCurvature
    }

    /// <summary>
    /// Defines the trimming preference used by bounded curves.
    /// </summary>
    enum IFCTrimmingPreference
    {
        Cartesian,
        Parameter,
        Unspecified
    }

    /// <summary>
    /// IFC entity types.
    /// </summary>
    enum IFCEntityType
    {
        Ifc2DCompositeCurve,
        IfcActionRequest,
        IfcActor,
        IfcActorRole,
        IfcActuatorType,
        IfcAddress,
        IfcAirTerminalBoxType,
        IfcAirTerminalType,
        IfcAirToAirHeatRecoveryType,
        IfcAlarmType,
        IfcAngularDimension,
        IfcAnnotation,
        IfcAnnotationCurveOccurrence,
        IfcAnnotationFillArea,
        IfcAnnotationFillAreaOccurrence,
        IfcAnnotationOccurrence,
        IfcAnnotationSurface,
        IfcAnnotationSurfaceOccurrence,
        IfcAnnotationSymbolOccurrence,
        IfcAnnotationTextOccurrence,
        IfcApplication,
        IfcAppliedValue,
        IfcAppliedValueRelationship,
        IfcApproval,
        IfcApprovalActorRelationship,
        IfcApprovalPropertyRelationship,
        IfcApprovalRelationship,
        IfcArbitraryClosedProfileDef,
        IfcArbitraryOpenProfileDef,
        IfcArbitraryProfileDefWithVoids,
        IfcAsset,
        IfcAsymmetricIShapeProfileDef,
        IfcAxis1Placement,
        IfcAxis2Placement2D,
        IfcAxis2Placement3D,
        IfcBSplineCurve,
        IfcBeam,
        IfcBeamType,
        IfcBezierCurve,
        IfcBlobTexture,
        IfcBlock,
        IfcBoilerType,
        IfcBooleanClippingResult,
        IfcBooleanResult,
        IfcBoundaryCondition,
        IfcBoundaryEdgeCondition,
        IfcBoundaryFaceCondition,
        IfcBoundaryNodeCondition,
        IfcBoundaryNodeConditionWarping,
        IfcBoundedCurve,
        IfcBoundedSurface,
        IfcBoundingBox,
        IfcBoxedHalfSpace,
        IfcBuilding,
        IfcBuildingElement,
        IfcBuildingElementComponent,
        IfcBuildingElementPart,
        IfcBuildingElementProxy,
        IfcBuildingElementProxyType,
        IfcBuildingElementType,
        IfcBuildingStorey,
        IfcCShapeProfileDef,
        IfcCableCarrierFittingType,
        IfcCableCarrierSegmentType,
        IfcCableSegmentType,
        IfcCalendarDate,
        IfcCartesianPoint,
        IfcCartesianTransformationOperator,
        IfcCartesianTransformationOperator2D,
        IfcCartesianTransformationOperator2DnonUniform,
        IfcCartesianTransformationOperator3D,
        IfcCartesianTransformationOperator3DnonUniform,
        IfcCenterLineProfileDef,
        IfcChamferEdgeFeature,
        IfcChillerType,
        IfcCircle,
        IfcCircleHollowProfileDef,
        IfcCircleProfileDef,
        IfcClassification,
        IfcClassificationItem,
        IfcClassificationItemRelationship,
        IfcClassificationNotation,
        IfcClassificationNotationFacet,
        IfcClassificationReference,
        IfcClosedShell,
        IfcCoilType,
        IfcColourRgb,
        IfcColourSpecification,
        IfcColumn,
        IfcColumnType,
        IfcComplexProperty,
        IfcCompositeCurve,
        IfcCompositeCurveSegment,
        IfcCompositeProfileDef,
        IfcCompressorType,
        IfcCondenserType,
        IfcCondition,
        IfcConditionCriterion,
        IfcConic,
        IfcConnectedFaceSet,
        IfcConnectionCurveGeometry,
        IfcConnectionGeometry,
        IfcConnectionPointEccentricity,
        IfcConnectionPointGeometry,
        IfcConnectionPortGeometry,
        IfcConnectionSurfaceGeometry,
        IfcConstraint,
        IfcConstraintAggregationRelationship,
        IfcConstraintClassificationRelationship,
        IfcConstraintRelationship,
        IfcConstructionEquipmentResource,
        IfcConstructionMaterialResource,
        IfcConstructionProductResource,
        IfcConstructionResource,
        IfcContextDependentUnit,
        IfcControl,
        IfcControllerType,
        IfcConversionBasedUnit,
        IfcCooledBeamType,
        IfcCoolingTowerType,
        IfcCoordinatedUniversalTimeOffset,
        IfcCostItem,
        IfcCostSchedule,
        IfcCostValue,
        IfcCovering,
        IfcCoveringType,
        IfcCraneRailAShapeProfileDef,
        IfcCraneRailFShapeProfileDef,
        IfcCrewResource,
        IfcCsgPrimitive3D,
        IfcCsgSolid,
        IfcCurrencyRelationship,
        IfcCurtainWall,
        IfcCurtainWallType,
        IfcCurve,
        IfcCurveBoundedPlane,
        IfcCurveStyle,
        IfcCurveStyleFont,
        IfcCurveStyleFontAndScaling,
        IfcCurveStyleFontPattern,
        IfcDamperType,
        IfcDateAndTime,
        IfcDefinedSymbol,
        IfcDerivedProfileDef,
        IfcDerivedUnit,
        IfcDerivedUnitElement,
        IfcDiameterDimension,
        IfcDimensionCalloutRelationship,
        IfcDimensionCurve,
        IfcDimensionCurveDirectedCallout,
        IfcDimensionCurveTerminator,
        IfcDimensionPair,
        IfcDimensionalExponents,
        IfcDirection,
        IfcDiscreteAccessory,
        IfcDiscreteAccessoryType,
        IfcDistributionChamberElement,
        IfcDistributionChamberElementType,
        IfcDistributionControlElement,
        IfcDistributionControlElementType,
        IfcDistributionElement,
        IfcDistributionElementType,
        IfcDistributionFlowElement,
        IfcDistributionFlowElementType,
        IfcDistributionPort,
        IfcDocumentElectronicFormat,
        IfcDocumentInformation,
        IfcDocumentInformationRelationship,
        IfcDocumentReference,
        IfcDoor,
        IfcDoorLiningProperties,
        IfcDoorPanelProperties,
        IfcDoorStyle,
        IfcDraughtingCallout,
        IfcDraughtingCalloutRelationship,
        IfcDraughtingPreDefinedColour,
        IfcDraughtingPreDefinedCurveFont,
        IfcDraughtingPreDefinedTextFont,
        IfcDuctFittingType,
        IfcDuctSegmentType,
        IfcDuctSilencerType,
        IfcEdge,
        IfcEdgeCurve,
        IfcEdgeFeature,
        IfcEdgeLoop,
        IfcElectricApplianceType,
        IfcElectricDistributionPoint,
        IfcElectricFlowStorageDeviceType,
        IfcElectricGeneratorType,
        IfcElectricHeaterType,
        IfcElectricMotorType,
        IfcElectricTimeControlType,
        IfcElectricalBaseProperties,
        IfcElectricalCircuit,
        IfcElectricalElement,
        IfcElement,
        IfcElementAssembly,
        IfcElementComponent,
        IfcElementComponentType,
        IfcElementQuantity,
        IfcElementType,
        IfcElementarySurface,
        IfcEllipse,
        IfcEllipseProfileDef,
        IfcEnergyConversionDevice,
        IfcEnergyConversionDeviceType,
        IfcEnergyProperties,
        IfcEnvironmentalImpactValue,
        IfcEquipmentElement,
        IfcEquipmentStandard,
        IfcEvaporativeCoolerType,
        IfcEvaporatorType,
        IfcExtendedMaterialProperties,
        IfcExternalReference,
        IfcExternallyDefinedHatchStyle,
        IfcExternallyDefinedSurfaceStyle,
        IfcExternallyDefinedSymbol,
        IfcExternallyDefinedTextFont,
        IfcExtrudedAreaSolid,
        IfcFace,
        IfcFaceBasedSurfaceModel,
        IfcFaceBound,
        IfcFaceOuterBound,
        IfcFaceSurface,
        IfcFacetedBrep,
        IfcFacetedBrepWithVoids,
        IfcFailureConnectionCondition,
        IfcFanType,
        IfcFastener,
        IfcFastenerType,
        IfcFeatureElement,
        IfcFeatureElementAddition,
        IfcFeatureElementSubtraction,
        IfcFillAreaStyle,
        IfcFillAreaStyleHatching,
        IfcFillAreaStyleTileSymbolWithStyle,
        IfcFillAreaStyleTiles,
        IfcFilterType,
        IfcFireSuppressionTerminalType,
        IfcFlowController,
        IfcFlowControllerType,
        IfcFlowFitting,
        IfcFlowFittingType,
        IfcFlowInstrumentType,
        IfcFlowMeterType,
        IfcFlowMovingDevice,
        IfcFlowMovingDeviceType,
        IfcFlowSegment,
        IfcFlowSegmentType,
        IfcFlowStorageDevice,
        IfcFlowStorageDeviceType,
        IfcFlowTerminal,
        IfcFlowTerminalType,
        IfcFlowTreatmentDevice,
        IfcFlowTreatmentDeviceType,
        IfcFluidFlowProperties,
        IfcFooting,
        IfcFuelProperties,
        IfcFurnishingElement,
        IfcFurnishingElementType,
        IfcFurnitureStandard,
        IfcFurnitureType,
        IfcGasTerminalType,
        IfcGeneralMaterialProperties,
        IfcGeneralProfileProperties,
        IfcGeometricCurveSet,
        IfcGeometricRepresentationContext,
        IfcGeometricRepresentationItem,
        IfcGeometricRepresentationSubContext,
        IfcGeometricSet,
        IfcGrid,
        IfcGridAxis,
        IfcGridPlacement,
        IfcGroup,
        IfcHalfSpaceSolid,
        IfcHeatExchangerType,
        IfcHumidifierType,
        IfcHygroscopicMaterialProperties,
        IfcIShapeProfileDef,
        IfcImageTexture,
        IfcInventory,
        IfcIrregularTimeSeries,
        IfcIrregularTimeSeriesValue,
        IfcJunctionBoxType,
        IfcLShapeProfileDef,
        IfcLaborResource,
        IfcLampType,
        IfcLibraryInformation,
        IfcLibraryReference,
        IfcLightDistributionData,
        IfcLightFixtureType,
        IfcLightIntensityDistribution,
        IfcLightSource,
        IfcLightSourceAmbient,
        IfcLightSourceDirectional,
        IfcLightSourceGoniometric,
        IfcLightSourcePositional,
        IfcLightSourceSpot,
        IfcLine,
        IfcLinearDimension,
        IfcLocalPlacement,
        IfcLocalTime,
        IfcLoop,
        IfcManifoldSolidBrep,
        IfcMappedItem,
        IfcMaterial,
        IfcMaterialClassificationRelationship,
        IfcMaterialDefinitionRepresentation,
        IfcMaterialLayer,
        IfcMaterialLayerSet,
        IfcMaterialLayerSetUsage,
        IfcMaterialList,
        IfcMaterialProperties,
        IfcMeasureWithUnit,
        IfcMechanicalConcreteMaterialProperties,
        IfcMechanicalFastener,
        IfcMechanicalFastenerType,
        IfcMechanicalMaterialProperties,
        IfcMechanicalSteelMaterialProperties,
        IfcMember,
        IfcMemberType,
        IfcMetric,
        IfcMonetaryUnit,
        IfcMotorConnectionType,
        IfcMove,
        IfcNamedUnit,
        IfcObject,
        IfcObjectDefinition,
        IfcObjectPlacement,
        IfcObjective,
        IfcOccupant,
        IfcOffsetCurve2D,
        IfcOffsetCurve3D,
        IfcOneDirectionRepeatFactor,
        IfcOpenShell,
        IfcOpeningElement,
        IfcOpticalMaterialProperties,
        IfcOrderAction,
        IfcOrganization,
        IfcOrganizationRelationship,
        IfcOrientedEdge,
        IfcOutletType,
        IfcOwnerHistory,
        IfcParameterizedProfileDef,
        IfcPath,
        IfcPerformanceHistory,
        IfcPermeableCoveringProperties,
        IfcPermit,
        IfcPerson,
        IfcPersonAndOrganization,
        IfcPhysicalComplexQuantity,
        IfcPhysicalQuantity,
        IfcPhysicalSimpleQuantity,
        IfcPile,
        IfcPipeFittingType,
        IfcPipeSegmentType,
        IfcPixelTexture,
        IfcPlacement,
        IfcPlanarBox,
        IfcPlanarExtent,
        IfcPlane,
        IfcPlate,
        IfcPlateType,
        IfcPoint,
        IfcPointOnCurve,
        IfcPointOnSurface,
        IfcPolyLoop,
        IfcPolygonalBoundedHalfSpace,
        IfcPolyline,
        IfcPort,
        IfcPostalAddress,
        IfcPreDefinedColour,
        IfcPreDefinedCurveFont,
        IfcPreDefinedDimensionSymbol,
        IfcPreDefinedItem,
        IfcPreDefinedPointMarkerSymbol,
        IfcPreDefinedSymbol,
        IfcPreDefinedTerminatorSymbol,
        IfcPreDefinedTextFont,
        IfcPresentationLayerAssignment,
        IfcPresentationLayerWithStyle,
        IfcPresentationStyle,
        IfcPresentationStyleAssignment,
        IfcProcedure,
        IfcProcess,
        IfcProduct,
        IfcProductDefinitionShape,
        IfcProductRepresentation,
        IfcProductsOfCombustionProperties,
        IfcProfileDef,
        IfcProfileProperties,
        IfcProject,
        IfcProjectOrder,
        IfcProjectOrderRecord,
        IfcProjectionCurve,
        IfcProjectionElement,
        IfcProperty,
        IfcPropertyBoundedValue,
        IfcPropertyConstraintRelationship,
        IfcPropertyDefinition,
        IfcPropertyDependencyRelationship,
        IfcPropertyEnumeratedValue,
        IfcPropertyEnumeration,
        IfcPropertyListValue,
        IfcPropertyReferenceValue,
        IfcPropertySet,
        IfcPropertySetDefinition,
        IfcPropertySingleValue,
        IfcPropertyTableValue,
        IfcProtectiveDeviceType,
        IfcProxy,
        IfcPumpType,
        IfcQuantityArea,
        IfcQuantityCount,
        IfcQuantityLength,
        IfcQuantityTime,
        IfcQuantityVolume,
        IfcQuantityWeight,
        IfcRadiusDimension,
        IfcRailing,
        IfcRailingType,
        IfcRamp,
        IfcRampFlight,
        IfcRampFlightType,
        IfcRationalBezierCurve,
        IfcRectangleHollowProfileDef,
        IfcRectangleProfileDef,
        IfcRectangularPyramid,
        IfcRectangularTrimmedSurface,
        IfcReferencesValueDocument,
        IfcRegularTimeSeries,
        IfcReinforcementBarProperties,
        IfcReinforcementDefinitionProperties,
        IfcReinforcingBar,
        IfcReinforcingElement,
        IfcReinforcingMesh,
        IfcRelAggregates,
        IfcRelAssigns,
        IfcRelAssignsTasks,
        IfcRelAssignsToActor,
        IfcRelAssignsToControl,
        IfcRelAssignsToGroup,
        IfcRelAssignsToProcess,
        IfcRelAssignsToProduct,
        IfcRelAssignsToProjectOrder,
        IfcRelAssignsToResource,
        IfcRelAssociates,
        IfcRelAssociatesAppliedValue,
        IfcRelAssociatesApproval,
        IfcRelAssociatesClassification,
        IfcRelAssociatesConstraint,
        IfcRelAssociatesDocument,
        IfcRelAssociatesLibrary,
        IfcRelAssociatesMaterial,
        IfcRelAssociatesProfileProperties,
        IfcRelConnects,
        IfcRelConnectsElements,
        IfcRelConnectsPathElements,
        IfcRelConnectsPortToElement,
        IfcRelConnectsPorts,
        IfcRelConnectsStructuralActivity,
        IfcRelConnectsStructuralElement,
        IfcRelConnectsStructuralMember,
        IfcRelConnectsWithEccentricity,
        IfcRelConnectsWithRealizingElements,
        IfcRelContainedInSpatialStructure,
        IfcRelCoversBldgElements,
        IfcRelCoversSpaces,
        IfcRelDecomposes,
        IfcRelDefines,
        IfcRelDefinesByProperties,
        IfcRelDefinesByType,
        IfcRelFillsElement,
        IfcRelFlowControlElements,
        IfcRelInteractionRequirements,
        IfcRelNests,
        IfcRelOccupiesSpaces,
        IfcRelOverridesProperties,
        IfcRelProjectsElement,
        IfcRelReferencedInSpatialStructure,
        IfcRelSchedulesCostItems,
        IfcRelSequence,
        IfcRelServicesBuildings,
        IfcRelSpaceBoundary,
        IfcRelVoidsElement,
        IfcRelationship,
        IfcRelaxation,
        IfcRepresentation,
        IfcRepresentationContext,
        IfcRepresentationItem,
        IfcRepresentationMap,
        IfcResource,
        IfcRevolvedAreaSolid,
        IfcRibPlateProfileProperties,
        IfcRightCircularCone,
        IfcRightCircularCylinder,
        IfcRoof,
        IfcRoot,
        IfcRoundedEdgeFeature,
        IfcRoundedRectangleProfileDef,
        IfcSIUnit,
        IfcSanitaryTerminalType,
        IfcScheduleTimeControl,
        IfcSectionProperties,
        IfcSectionReinforcementProperties,
        IfcSectionedSpine,
        IfcSensorType,
        IfcServiceLife,
        IfcServiceLifeFactor,
        IfcShapeAspect,
        IfcShapeModel,
        IfcShapeRepresentation,
        IfcShellBasedSurfaceModel,
        IfcSimpleProperty,
        IfcSite,
        IfcSlab,
        IfcSlabType,
        IfcSlippageConnectionCondition,
        IfcSolidModel,
        IfcSoundProperties,
        IfcSoundValue,
        IfcSpace,
        IfcSpaceHeaterType,
        IfcSpaceProgram,
        IfcSpaceThermalLoadProperties,
        IfcSpaceType,
        IfcSpatialStructureElement,
        IfcSpatialStructureElementType,
        IfcSphere,
        IfcStackTerminalType,
        IfcStair,
        IfcStairFlight,
        IfcStairFlightType,
        IfcStructuralAction,
        IfcStructuralActivity,
        IfcStructuralAnalysisModel,
        IfcStructuralConnection,
        IfcStructuralConnectionCondition,
        IfcStructuralCurveConnection,
        IfcStructuralCurveMember,
        IfcStructuralCurveMemberVarying,
        IfcStructuralItem,
        IfcStructuralLinearAction,
        IfcStructuralLinearActionVarying,
        IfcStructuralLoad,
        IfcStructuralLoadGroup,
        IfcStructuralLoadLinearForce,
        IfcStructuralLoadPlanarForce,
        IfcStructuralLoadSingleDisplacement,
        IfcStructuralLoadSingleDisplacementDistortion,
        IfcStructuralLoadSingleForce,
        IfcStructuralLoadSingleForceWarping,
        IfcStructuralLoadStatic,
        IfcStructuralLoadTemperature,
        IfcStructuralMember,
        IfcStructuralPlanarAction,
        IfcStructuralPlanarActionVarying,
        IfcStructuralPointAction,
        IfcStructuralPointConnection,
        IfcStructuralPointReaction,
        IfcStructuralProfileProperties,
        IfcStructuralReaction,
        IfcStructuralResultGroup,
        IfcStructuralSteelProfileProperties,
        IfcStructuralSurfaceConnection,
        IfcStructuralSurfaceMember,
        IfcStructuralSurfaceMemberVarying,
        IfcStructuredDimensionCallout,
        IfcStyleModel,
        IfcStyledItem,
        IfcStyledRepresentation,
        IfcSubContractResource,
        IfcSubedge,
        IfcSurface,
        IfcSurfaceCurveSweptAreaSolid,
        IfcSurfaceOfLinearExtrusion,
        IfcSurfaceOfRevolution,
        IfcSurfaceStyle,
        IfcSurfaceStyleLighting,
        IfcSurfaceStyleRefraction,
        IfcSurfaceStyleRendering,
        IfcSurfaceStyleShading,
        IfcSurfaceStyleWithTextures,
        IfcSurfaceTexture,
        IfcSweptAreaSolid,
        IfcSweptDiskSolid,
        IfcSweptSurface,
        IfcSwitchingDeviceType,
        IfcSymbolStyle,
        IfcSystem,
        IfcSystemFurnitureElementType,
        IfcTShapeProfileDef,
        IfcTable,
        IfcTableRow,
        IfcTankType,
        IfcTask,
        IfcTelecomAddress,
        IfcTendon,
        IfcTendonAnchor,
        IfcTerminatorSymbol,
        IfcTextLiteral,
        IfcTextLiteralWithExtent,
        IfcTextStyle,
        IfcTextStyleFontModel,
        IfcTextStyleForDefinedFont,
        IfcTextStyleTextModel,
        IfcTextStyleWithBoxCharacteristics,
        IfcTextureCoordinate,
        IfcTextureCoordinateGenerator,
        IfcTextureMap,
        IfcTextureVertex,
        IfcThermalMaterialProperties,
        IfcTimeSeries,
        IfcTimeSeriesReferenceRelationship,
        IfcTimeSeriesSchedule,
        IfcTimeSeriesValue,
        IfcTopologicalRepresentationItem,
        IfcTopologyRepresentation,
        IfcTransformerType,
        IfcTransportElement,
        IfcTransportElementType,
        IfcTrapeziumProfileDef,
        IfcTrimmedCurve,
        IfcTubeBundleType,
        IfcTwoDirectionRepeatFactor,
        IfcTypeObject,
        IfcTypeProduct,
        IfcUShapeProfileDef,
        IfcUnitAssignment,
        IfcUnitaryEquipmentType,
        IfcValveType,
        IfcVector,
        IfcVertex,
        IfcVertexBasedTextureMap,
        IfcVertexLoop,
        IfcVertexPoint,
        IfcVibrationIsolatorType,
        IfcVirtualElement,
        IfcVirtualGridIntersection,
        IfcWall,
        IfcWallStandardCase,
        IfcWallType,
        IfcWasteTerminalType,
        IfcWaterProperties,
        IfcWindow,
        IfcWindowLiningProperties,
        IfcWindowPanelProperties,
        IfcWindowStyle,
        IfcWorkControl,
        IfcWorkPlan,
        IfcWorkSchedule,
        IfcZShapeProfileDef,
        IfcZone,
        UnKnown
    }
}
