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
using BIM.IFC.Utility;

namespace BIM.IFC.Exporter.PropertySet.Calculators
{
    /// <summary>
    /// A calculation class to calculate riser and tread parameters.
    /// </summary>
    class StairRiserTreadsCalculator : PropertyCalculator
    {
        /// <summary>
        /// An int variable to keep the calculated NumberOfRisers value.
        /// </summary>
        private int m_NumberOfRisers = 0;

        /// <summary>
        /// An int variable to keep the calculated NumberOfTreads value.
        /// </summary>
        private int m_NumberOfTreads = 0;

        /// <summary>
        /// An int variable to keep the calculated RiserHeight value.
        /// </summary>
        private double m_RiserHeight = 0.0;

        /// <summary>
        /// An int variable to keep the calculated TreadLength value.
        /// </summary>
        private double m_TreadLength = 0.0;

        /// <summary>
        /// An int variable to keep the calculated m_TreadLengthAtOffset value.
        /// </summary>
        private double m_TreadLengthAtOffset = 0.0;

        /// <summary>
        /// An int variable to keep the calculated TreadLength value.
        /// </summary>
        private double m_TreadLengthAtInnerSide = 0.0;

        /// <summary>
        /// An int variable to keep the calculated NosingLength value.
        /// </summary>
        private double m_NosingLength = 0.0;

        /// <summary>
        /// An int variable to keep the calculated WalkingLineOffset value.
        /// </summary>
        private double m_WalkingLineOffset = 0.0;
        
        /// <summary>
        /// An int variable to keep the calculated WaistThickness value.
        /// </summary>
        private double m_WaistThickness = 0.0;
        
        /// <summary>
        /// Determine if the multiple variables need to be calculated or not for the current element.
        /// </summary>
        private Element m_CurrentElement = null;

        /// <summary>
        /// A static instance of this class.
        /// </summary>
        static StairRiserTreadsCalculator s_Instance = new StairRiserTreadsCalculator();

        /// <summary>
        /// The StairNumberOfRisersCalculator instance.
        /// </summary>
        public static StairRiserTreadsCalculator Instance
        {
            get { return s_Instance; }
        }

        /// <summary>
        /// Determines if the calculator calculates only one parameter, or multiple.
        /// </summary>
        /// <returns>
        /// True for multiple parameters, false for one.
        /// </returns>
        public override bool CalculatesMultipleParameters
        {
            get { return true; }
        }

        /// <summary>
        /// Calculates number of risers for a stair.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="extrusionCreationData">
        /// The IFCExtrusionCreationData.
        /// </param>
        /// <param name="element">
        /// The element to calculate the value.
        /// </param>
        /// <param name="elementType">
        /// The element type.
        /// </param>
        /// <returns>
        /// True if the operation succeed, false otherwise.
        /// </returns>
        public override bool Calculate(ExporterIFC exporterIFC, IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            bool valid = true;
            if (m_CurrentElement != element)
            {
                double scale = exporterIFC.LinearScale;

                m_CurrentElement = element;
                if (StairsExporter.IsLegacyStairs(element))
                {
                    ExporterIFCUtils.GetLegacyStairsProperties(exporterIFC, element, 
                        out m_NumberOfRisers, out m_NumberOfTreads,
                        out m_RiserHeight, out m_TreadLength, out m_TreadLengthAtInnerSide,
                        out m_NosingLength, out m_WaistThickness);
                    m_TreadLengthAtOffset = m_TreadLength;
                    m_WalkingLineOffset = m_WaistThickness / 2.0;
                }
                else if (element is Stairs)
                {
                    Stairs stairs = element as Stairs;
                    m_NumberOfRisers = stairs.ActualRisersNumber;
                    m_NumberOfTreads = stairs.ActualTreadsNumber;
                    m_RiserHeight = stairs.ActualRiserHeight * scale;
                    m_TreadLength = stairs.ActualTreadDepth * scale;
                }
                else if (element is StairsRun)
                {
                    StairsRun stairsRun = element as StairsRun;
                    StairsRunType stairsRunType = stairsRun.Document.GetElement(stairsRun.GetTypeId()) as StairsRunType;
                    Stairs stairs = stairsRun.GetStairs();
                    StairsType stairsType = stairs.Document.GetElement(stairs.GetTypeId()) as StairsType;

                    m_NumberOfRisers = stairs.ActualRisersNumber;
                    m_NumberOfTreads = stairs.ActualTreadsNumber;
                    m_RiserHeight = stairs.ActualRiserHeight * scale;
                    m_TreadLength = stairs.ActualTreadDepth * scale;
                    m_TreadLengthAtOffset = m_TreadLength;
                    m_NosingLength = stairsRunType.NosingLength * scale;
                    m_WaistThickness = stairsRun.ActualRunWidth * scale;
                    m_WalkingLineOffset = m_WaistThickness / 2.0;

                    Parameter treadLengthAtInnerSideParam = stairsType.get_Parameter(BuiltInParameter.STAIRSTYPE_MINIMUM_TREAD_WIDTH_INSIDE_BOUNDARY);
                    m_TreadLengthAtInnerSide = (treadLengthAtInnerSideParam != null) ? (treadLengthAtInnerSideParam.AsDouble() * scale) : 0.0;
                }
                else
                {
                    valid = false;
                }
            }
            return valid;
        }

        /// <summary>
        /// Gets the calculated int value.
        /// </summary>
        /// <returns>
        /// The int value.
        /// </returns>
        public override int GetIntValue(string paramName)
        {
            if (String.Compare(paramName, "NumberOfRiser", true) == 0)
                return m_NumberOfRisers;
            if (String.Compare(paramName, "NumberOfTreads", true) == 0)
                return m_NumberOfTreads;
            return 0;
        }
        
        /// <summary>
        /// Gets the calculated double value.
        /// </summary>
        /// <returns>
        /// The double value.
        /// </returns>
        public override double GetDoubleValue(string paramName)
        {
            if (String.Compare(paramName, "NosingLength", true) == 0)
                return m_NosingLength;
            if (String.Compare(paramName, "RiserHeight", true) == 0)
                return m_RiserHeight;
            if (String.Compare(paramName, "TreadLength", true) == 0)
                return m_TreadLength;
            if (String.Compare(paramName, "TreadLengthAtInnerSide", true) == 0)
                return m_TreadLengthAtInnerSide;
            if (String.Compare(paramName, "TreadLengthAtOffset", true) == 0)
                return m_TreadLengthAtOffset;
            if (String.Compare(paramName, "WalkingLineOffset", true) == 0)
                return m_WalkingLineOffset;
            if (String.Compare(paramName, "WaistThickness", true) == 0)
                return m_WaistThickness;
            return 0.0;
        }
    }
}
