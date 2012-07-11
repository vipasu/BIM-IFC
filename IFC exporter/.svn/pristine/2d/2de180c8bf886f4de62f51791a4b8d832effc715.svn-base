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
using BIM.IFC.Toolkit;

namespace BIM.IFC.Utility
{
    using IFCDoorStyleOperation = Autodesk.Revit.DB.IFC.IFCDoorStyleOperation;
    using IFCWindowStyleOperation = Autodesk.Revit.DB.IFC.IFCWindowStyleOperation;

    /// <summary>
    /// Provides static methods for door and window related manipulations.
    /// </summary>
    class DoorWindowUtil
    {
        /// <summary>
        /// Gets the panel operation from door style operation.
        /// </summary>
        /// <param name="ifcDoorStyleOperationType">
        /// The IFCDoorStyleOperation.
        /// </param>
        /// <returns>
        /// The string represents the door panel operation.
        /// </returns>
        public static IFCDoorPanelOperation GetPanelOperationFromDoorStyleOperation(IFCDoorStyleOperation ifcDoorStyleOperationType)
        {
            switch (ifcDoorStyleOperationType)
            {
                case IFCDoorStyleOperation.SingleSwingLeft:
                case IFCDoorStyleOperation.SingleSwingRight:
                case IFCDoorStyleOperation.DoubleDoorSingleSwing:
                case IFCDoorStyleOperation.DoubleDoorSingleSwingOppositeLeft:
                case IFCDoorStyleOperation.DoubleDoorSingleSwingOppositeRight:
                    return IFCDoorPanelOperation.Swinging;
                case IFCDoorStyleOperation.DoubleSwingLeft:
                case IFCDoorStyleOperation.DoubleSwingRight:
                case IFCDoorStyleOperation.DoubleDoorDoubleSwing:
                    return IFCDoorPanelOperation.Double_Acting;
                case IFCDoorStyleOperation.SlidingToLeft:
                case IFCDoorStyleOperation.SlidingToRight:
                case IFCDoorStyleOperation.DoubleDoorSliding:
                    return IFCDoorPanelOperation.Sliding;
                case IFCDoorStyleOperation.FoldingToLeft:
                case IFCDoorStyleOperation.FoldingToRight:
                case IFCDoorStyleOperation.DoubleDoorFolding:
                    return IFCDoorPanelOperation.Folding;
                case IFCDoorStyleOperation.Revolving:
                    return IFCDoorPanelOperation.Revolving;
                case IFCDoorStyleOperation.RollingUp:
                    return IFCDoorPanelOperation.RollingUp;
                case IFCDoorStyleOperation.UserDefined:
                    return IFCDoorPanelOperation.UserDefined;
                default:
                    return IFCDoorPanelOperation.NotDefined;
            }
        }

        /// <summary>
        /// Creates door panel properties.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="doorWindowInfo">
        /// The IFCDoorWindowInfo object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance of a door.
        /// </param>
        /// <returns>
        /// The list of handles created.
        /// </returns>
        public static IList<IFCAnyHandle> CreateDoorPanelProperties(ExporterIFC exporterIFC,
           IFCDoorWindowInfo doorWindowInfo, Element familyInstance)
        {
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            IList<IFCAnyHandle> doorPanels = new List<IFCAnyHandle>();

            IList<double?> panelDepthList = new List<double?>();
            IList<double?> panelWidthList = new List<double?>();

            IList<IFCDoorPanelOperation> panelOperationList = new List<IFCDoorPanelOperation>();
            IList<IFCDoorPanelPosition> panelPositionList = new List<IFCDoorPanelPosition>();

            int panelNumber = 1;
            const int maxPanels = 64;  // arbitrary large number to prevent infinite loops.
            for (; panelNumber < maxPanels; panelNumber++)
            {
                string panelDepthCurrString = "PanelDepth" + panelNumber.ToString();
                string panelWidthCurrString = "PanelWidth" + panelNumber.ToString();

                // We will always have at least one panel definition as long as the panelOperation is not
                // NotDefined.

                panelOperationList.Add(GetPanelOperationFromDoorStyleOperation(doorWindowInfo.DoorOperationType));

                // If the panel operation is defined we'll allow no panel position for the 1st panel.
                IFCDoorPanelPosition? panelPosition = GetIFCDoorPanelPosition("", familyInstance, panelNumber);
                if (panelPosition == null)
                {
                    if (panelNumber == 1)
                        panelPosition = GetIFCDoorPanelPosition("", familyInstance, -1);
                    if ((panelPosition == null) && (panelNumber > 1))
                    {
                        panelPositionList.Add(IFCDoorPanelPosition.NotDefined);
                        break;
                    }
                }

                if (doorWindowInfo.IsFlippedInX ^ doorWindowInfo.IsFlippedInY)
                    panelPosition = ReverseDoorPanelPosition(panelPosition);

                panelPositionList.Add(panelPosition != null ? (IFCDoorPanelPosition)panelPosition : IFCDoorPanelPosition.NotDefined);

                double value1 = 0.0, value2 = 0.0;
                bool foundDepth = ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, panelDepthCurrString, out value1);
                if (!foundDepth && (panelNumber == 1))
                    foundDepth = ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "PanelDepth", out value1);

                bool foundWidth = ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, panelWidthCurrString, out value2);
                if (!foundWidth && (panelNumber == 1))
                    foundWidth = ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "PanelWidth", out value2);

                if (foundDepth && foundWidth)
                {
                    panelDepthList.Add(value1);
                    panelWidthList.Add(value2);
                }
                else
                {
                    panelDepthList.Add(null);
                    panelWidthList.Add(null);
                }
            }

            // calculate panelWidths
            double totalPanelWidth = 0.0;
            for (int panelIndex = 0; (panelIndex < panelNumber - 1); panelIndex++)
            {
                if (panelDepthList[panelIndex] == null || MathUtil.IsAlmostZero((double)panelDepthList[panelIndex]) ||
                    panelWidthList[panelIndex] == null || MathUtil.IsAlmostZero((double)panelWidthList[panelIndex]))
                {
                    totalPanelWidth = 0.0;
                    break;
                }
                totalPanelWidth += (double)panelWidthList[panelIndex];
            }

            if (!MathUtil.IsAlmostZero(totalPanelWidth))
            {
                for (int panelIndex = 0; (panelIndex < panelNumber - 1); panelIndex++)
                {
                    double? currentPanelWidth = null;
                    if (panelWidthList[panelIndex].HasValue)
                        currentPanelWidth = (double)panelWidthList[panelIndex] / totalPanelWidth;

                    string doorPanelGUID = ExporterIFCUtils.CreateGUID();
                    string doorPanelName = exporterIFC.GetName();
                    IFCAnyHandle doorPanel = IFCInstanceExporter.CreateDoorPanelProperties(file, doorPanelGUID, ownerHistory,
                       doorPanelName, null, panelDepthList[panelIndex], panelOperationList[panelIndex],
                       currentPanelWidth, panelPositionList[panelIndex], null);
                    doorPanels.Add(doorPanel);
                }
            }

            return doorPanels;
        }

        /// <summary>
        /// Creates door lining properties.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance of a door.
        /// </param>
        /// <returns>
        /// The handle created.
        /// </returns>
        public static IFCAnyHandle CreateDoorLiningProperties(ExporterIFC exporterIFC, Element familyInstance)
        {
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            double? liningDepthOpt = null;
            double? liningThicknessOpt = null;
            double? thresholdDepthOpt = null;
            double? thresholdThicknessOpt = null;
            double? transomThicknessOpt = null;
            double? transomOffsetOpt = null;
            double? liningOffsetOpt = null;
            double? thresholdOffsetOpt = null;
            double? casingThicknessOpt = null;
            double? casingDepthOpt = null;

            double value1, value2;

            // both of these must be defined, or not defined - if only one is defined, we ignore the values.
            if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "LiningDepth", out value1))
            {
                if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "LiningThickness", out value2))
                {
                    liningDepthOpt = value1;
                    liningThicknessOpt = value2;
                }
            }

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "LiningOffset", out value1))
                liningOffsetOpt = value1;

            // both of these must be defined, or not defined - if only one is defined, we ignore the values.
            if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "ThresholdDepth", out value1))
            {
                if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "ThresholdThickness", out value2))
                {
                    thresholdDepthOpt = value1;
                    thresholdThicknessOpt = value2;
                }
            }

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "ThreshholdOffset", out value1))
                liningOffsetOpt = value1;

            // both of these must be defined, or not defined - if only one is defined, we ignore the values.
            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "TransomOffset", out value1))
            {
                if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "TransomThickness", out value2))
                {
                    transomOffsetOpt = value1;
                    transomThicknessOpt = value2;
                }
            }

            // both of these must be defined, or not defined - if only one is defined, we ignore the values.
            if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "CasingDepth", out value1))
            {
                if (ParameterUtil.GetPositiveDoubleValueFromElementOrSymbol(familyInstance, "CasingThickness", out value2))
                {
                    casingDepthOpt = value1;
                    casingThicknessOpt = value2;
                }
            }

            string doorLiningGUID = ExporterIFCUtils.CreateSubElementGUID(familyInstance, (int) IFCDoorSubElements.DoorLining);
            string doorLiningName = exporterIFC.GetName();
            return IFCInstanceExporter.CreateDoorLiningProperties(file, doorLiningGUID, ownerHistory,
               doorLiningName, null, liningDepthOpt, liningThicknessOpt, thresholdDepthOpt, thresholdThicknessOpt,
               transomThicknessOpt, transomOffsetOpt, liningOffsetOpt, thresholdOffsetOpt, casingThicknessOpt,
               casingDepthOpt, null);
        }

        /// <summary>
        /// Gets door panel position.
        /// </summary>
        /// <param name="typeName">
        /// The type name of the door.
        /// </param>
        /// <param name="element">
        /// The door element.
        /// </param>
        /// <param name="number">
        /// The number of panel position.
        /// </param>
        /// <returns>
        /// The string represents the door panel position.
        /// </returns>
        public static IFCDoorPanelPosition? GetIFCDoorPanelPosition(string typeName, Element element, int number)
        {
            string currPanelName;
            if (number == -1)
                currPanelName = "PanelPosition";
            else
                currPanelName = "PanelPosition" + number.ToString();

            string value = "";
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, currPanelName, out value))
                value = typeName;

            if (value == "")
                return null;
            else if (String.Compare(value, "left", true) == 0)
                return IFCDoorPanelPosition.Left;
            else if (String.Compare(value, "middle", true) == 0)
                return IFCDoorPanelPosition.Middle;
            else if (String.Compare(value, "right", true) == 0)
                return IFCDoorPanelPosition.Right;
            else
                return IFCDoorPanelPosition.NotDefined;
        }

        /// <summary>
        /// Reverses door panel position.
        /// </summary>
        /// <param name="originalPosition">
        /// The original position.
        /// </param>
        /// <returns>
        /// The string represents the reversed door panel position.
        /// </returns>
        public static IFCDoorPanelPosition ReverseDoorPanelPosition(IFCDoorPanelPosition? originalPosition)
        {
            if (originalPosition == null)
                return IFCDoorPanelPosition.NotDefined;
            else if (originalPosition == IFCDoorPanelPosition.Left)
                return IFCDoorPanelPosition.Right;
            else if (originalPosition == IFCDoorPanelPosition.Right)
                return IFCDoorPanelPosition.Left;
            return (IFCDoorPanelPosition)originalPosition;
        }

        /// <summary>
        /// Gets window style operation.
        /// </summary>
        /// <param name="familySymbol">
        /// The element type of window.
        /// </param>
        /// <returns>
        /// The IFCWindowStyleOperation.
        /// </returns>
        public static Toolkit.IFCWindowStyleOperation GetIFCWindowStyleOperation(ElementType familySymbol)
        {
            Parameter parameter = familySymbol.get_Parameter(BuiltInParameter.WINDOW_OPERATION_TYPE);
            string value = parameter.AsValueString();

            if (String.IsNullOrEmpty(value))
                return Toolkit.IFCWindowStyleOperation.NotDefined;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "UserDefined"))
                return Toolkit.IFCWindowStyleOperation.UserDefined;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "SinglePanel"))
                return Toolkit.IFCWindowStyleOperation.Single_Panel;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "DoublePanelVertical"))
                return Toolkit.IFCWindowStyleOperation.Double_Panel_Vertical;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "DoublePanelHorizontal"))
                return Toolkit.IFCWindowStyleOperation.Double_Panel_Horizontal;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelVertical"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Vertical;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelBottom"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Bottom;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelTop"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Top;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelLeft"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Left;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelRight"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Right;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TriplePanelHorizontal"))
                return Toolkit.IFCWindowStyleOperation.Triple_Panel_Horizontal;

            return Toolkit.IFCWindowStyleOperation.NotDefined;
        }

        /// <summary>
        /// Gets window style construction.
        /// </summary>
        /// <param name="element">
        /// The window element.
        /// </param>
        /// <param name="initialValue">
        /// The initial value.
        /// </param>
        /// <returns>
        /// The string represents the window style construction.
        /// </returns>
        public static IFCWindowStyleConstruction GetIFCWindowStyleConstruction(Element element, string initialValue)
        {
            string value;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, "Construction", out value))
                value = initialValue;

            if (value == "")
                return IFCWindowStyleConstruction.NotDefined;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Aluminum"))
                return IFCWindowStyleConstruction.Aluminium;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "HighGradeSteel"))
                return IFCWindowStyleConstruction.High_Grade_Steel;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Steel"))
                return IFCWindowStyleConstruction.Steel;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Wood"))
                return IFCWindowStyleConstruction.Wood;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "AluminumWood"))
                return IFCWindowStyleConstruction.Aluminium_Wood;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Plastic"))
                return IFCWindowStyleConstruction.Plastic;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "OtherConstruction"))
                return IFCWindowStyleConstruction.Other_Construction;

            return IFCWindowStyleConstruction.NotDefined;
        }

        /// <summary>
        /// Gets window panel operation.
        /// </summary>
        /// <param name="initialValue">
        /// The initial value.
        /// </param>
        /// <param name="element">
        /// The window element.
        /// </param>
        /// <param name="number">
        /// The number of panel operation.
        /// </param>
        /// <returns>
        /// The string represents the window panel operation.
        /// </returns>
        public static IFCWindowPanelOperation GetIFCWindowPanelOperation(string initialValue, Element element, int number)
        {
            string currPanelName = "PanelOperation" + number.ToString();

            string value;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, currPanelName, out value))
                value = initialValue;

            if (value == "")
                return IFCWindowPanelOperation.NotDefined;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "SideHungRightHand"))
                return IFCWindowPanelOperation.SideHungRightHand;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "SideHungLeftHand"))
                return IFCWindowPanelOperation.SideHungLeftHand;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TiltAndTurnRightHand"))
                return IFCWindowPanelOperation.TiltAndTurnRightHand;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TiltAndTurnLeftHand"))
                return IFCWindowPanelOperation.TiltAndTurnLeftHand;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "TopHung"))
                return IFCWindowPanelOperation.TopHung;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "BottomHung"))
                return IFCWindowPanelOperation.BottomHung;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "PivotHorizontal"))
                return IFCWindowPanelOperation.PivotHorizontal;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "PivotVertical"))
                return IFCWindowPanelOperation.PivotVertical;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "SlidingHorizontal"))
                return IFCWindowPanelOperation.SlidingHorizontal;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "SlidingVertical"))
                return IFCWindowPanelOperation.SlidingVertical;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "RemovableCasement"))
                return IFCWindowPanelOperation.RemovableCasement;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "FixedCasement"))
                return IFCWindowPanelOperation.FixedCasement;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "OtherOperation"))
                return IFCWindowPanelOperation.OtherOperation;

            return IFCWindowPanelOperation.NotDefined;
        }

        /// <summary>
        /// Gets window panel position.
        /// </summary>
        /// <param name="initialValue">
        /// The initial value.
        /// </param>
        /// <param name="element">
        /// The window element.
        /// </param>
        /// <param name="number">
        /// The number of panel position.
        /// </param>
        /// <returns>
        /// The string represents the window panel position.
        /// </returns>
        public static IFCWindowPanelPosition GetIFCWindowPanelPosition(string initialValue, Element element, int number)
        {
            string currPanelName = "PanelPosition" + number.ToString();

            string value;
            if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, currPanelName, out value))
                value = initialValue;

            if (value == "")
                return IFCWindowPanelPosition.NotDefined;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Left"))
                return IFCWindowPanelPosition.Left;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Middle"))
                return IFCWindowPanelPosition.Middle;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Right"))
                return IFCWindowPanelPosition.Right;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Bottom"))
                return IFCWindowPanelPosition.Bottom;
            else if (NamingUtil.IsEqualIgnoringCaseSpacesAndUnderscores(value, "Top"))
                return IFCWindowPanelPosition.Top;

            return IFCWindowPanelPosition.NotDefined;
        }

        /// <summary>
        /// Creates window panel position.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance of a window.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <returns>
        /// The handle created.
        /// </returns>
        public static IFCAnyHandle CreateWindowLiningProperties(ExporterIFC exporterIFC,
           Element familyInstance, string description)
        {
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            double? liningDepthOpt = null;
            double? liningThicknessOpt = null;
            double? transomThicknessOpt = null;
            double? mullionThicknessOpt = null;
            double? firstTransomOffsetOpt = null;
            double? secondTransomOffsetOpt = null;
            double? firstMullionOffsetOpt = null;
            double? secondMullionOffsetOpt = null;

            double value1 = 0.0;
            double value2 = 0.0;

            // both of these must be defined (or not defined)
            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "LiningDepth", out value1) &&
               ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "LiningThickness", out value2))
            {
                liningDepthOpt = value1;
                liningThicknessOpt = value2;
            }

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "TransomThickness", out value1))
                transomThicknessOpt = value1;

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "FirstTransomOffset", out value1))
                firstTransomOffsetOpt = value1;

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "SecondTransomOffset", out value1))
                secondTransomOffsetOpt = value1;

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "MullionThickness", out value1))
                mullionThicknessOpt = value1;

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "FirstMullionOffset", out value1))
                firstMullionOffsetOpt = value1;

            if (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "SecondMullionOffset", out value1))
                secondMullionOffsetOpt = value1;

            string windowLiningGUID = ExporterIFCUtils.CreateGUID();
            string windowLiningName = exporterIFC.GetName();
            return IFCInstanceExporter.CreateWindowLiningProperties(file, windowLiningGUID, ownerHistory,
               windowLiningName, description, liningDepthOpt, liningThicknessOpt, transomThicknessOpt, mullionThicknessOpt,
               firstTransomOffsetOpt, secondTransomOffsetOpt, firstMullionOffsetOpt, secondMullionOffsetOpt, null);
        }

        /// <summary>
        /// Creates window panel properties.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="doorWindowInfo">
        /// The IFCDoorWindowInfo object.
        /// </param>
        /// <param name="familyInstance">
        /// The family instance of a window.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <returns>
        /// The list of handles created.
        /// </returns>
        public static IList<IFCAnyHandle> CreateWindowPanelProperties(ExporterIFC exporterIFC,
           Element familyInstance, string description)
        {
            IList<IFCAnyHandle> panels = new List<IFCAnyHandle>();
            IFCFile file = exporterIFC.GetFile();
            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();

            const int maxPanels = 1000;  // arbitrary large number to prevent infinite loops.
            for (int panelNumber = 1; panelNumber < maxPanels; panelNumber++)
            {
                string frameDepthCurrString = "FrameDepth" + panelNumber.ToString();
                string frameThicknessCurrString = "FrameThickness" + panelNumber.ToString();

                IFCWindowPanelOperation panelOperation = GetIFCWindowPanelOperation("", familyInstance, panelNumber);
                IFCWindowPanelPosition panelPosition = GetIFCWindowPanelPosition("", familyInstance, panelNumber);
                if (panelOperation == IFCWindowPanelOperation.NotDefined && panelPosition == IFCWindowPanelPosition.NotDefined)
                    break;

                double? frameDepth = null;
                double? frameThickness = null;

                double value1, value2;
                if ((ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, frameDepthCurrString, out value1) ||
                    ((panelNumber == 1) && (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "FrameDepth", out value1)))) &&
                   (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, frameThicknessCurrString, out value2) ||
                    ((panelNumber == 1) && (ParameterUtil.GetDoubleValueFromElementOrSymbol(familyInstance, "FrameThickness", out value2)))))
                {
                    frameDepth = value1;
                    frameThickness = value2;
                }

                string panelGUID = ExporterIFCUtils.CreateGUID();
                string panelName = NamingUtil.CreateIFCName(exporterIFC, panelNumber);
                panels.Add(IFCInstanceExporter.CreateWindowPanelProperties(file, panelGUID, ownerHistory,
                   panelName, description, panelOperation, panelPosition, frameDepth, frameThickness, null));
            }
            return panels;
        }
    }
}