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
using BIM.IFC.Utility;

namespace BIM.IFC.Exporter.PropertySet.Calculators
{
    /// <summary>
    /// A calculation class to calculate gross volume for a slab.
    /// </summary>
    class SlabGrossVolumeCalculator : PropertyCalculator
    {
        /// <summary>
        /// A double variable to keep the calculated value.
        /// </summary>
        private double m_Volumn = 0;

        /// <summary>
        /// A static instance of this class.
        /// </summary>
        static SlabGrossVolumeCalculator s_Instance = new SlabGrossVolumeCalculator();

        /// <summary>
        /// The SlabGrossVolumeCalculator instance.
        /// </summary>
        public static SlabGrossVolumeCalculator Instance
        {
            get { return s_Instance; }
        }

        /// <summary>
        /// Calculates gross volume for a slab.
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
            if (extrusionCreationData == null)
                return false;
            double area = extrusionCreationData.ScaledArea;
            double length = extrusionCreationData.ScaledLength;
            if (area < MathUtil.Eps() * MathUtil.Eps() || length < MathUtil.Eps())
                return false;
            else
            {
                m_Volumn = area * length;
                return true;
            }
        }

        /// <summary>
        /// Gets the calculated double value.
        /// </summary>
        /// <returns>
        /// The double value.
        /// </returns>
        public override double GetDoubleValue()
        {
            return m_Volumn;
        }
    }
}
