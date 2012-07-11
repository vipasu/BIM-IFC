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
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// This enumerated type represents the types of quantities that can be exported.
    /// </summary>
    enum QuantityType
    {
        /// <summary>
        /// A real number quantity.
        /// </summary>
        Real,
        /// <summary>
        /// A length quantity.
        /// </summary>
        PositiveLength,
        /// <summary>
        /// An area quantity.
        /// </summary>
        Area,
        /// <summary>
        /// A volume quantity.
        /// </summary>
        Volume,
    }

    /// <summary>
    /// Represents a mapping from a Revit parameter or calculated quantity to an IFC quantity.
    /// </summary>
    class QuantityEntry : Entry
    {
        /// <summary>
        /// Defines the building code used to calculate the element quantity.
        /// </summary>
        string m_MethodOfMeasurement = String.Empty;

        /// <summary>
        /// The type of the quantity.
        /// </summary>
        QuantityType m_QuantityType = QuantityType.Real;

        /// <summary>
        /// Constructs a QuantityEntry object.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        public QuantityEntry(string revitParameterName)
            : base(revitParameterName)
        {

        }

        /// <summary>
        /// The type of the quantity.
        /// </summary>
        public QuantityType QuantityType
        {
            get
            {
                return m_QuantityType;
            }
            set
            {
                m_QuantityType = value;
            }
        }

        /// <summary>
        /// Defines the building code used to calculate the element quantity.
        /// </summary>
        public string MethodOfMeasurement
        {
            get
            {
                return m_MethodOfMeasurement;
            }
            set
            {
                m_MethodOfMeasurement = value;
            }
        }

        /// <summary>
        /// Process to create element quantity.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="extrusionCreationData">
        /// The IFCExtrusionCreationData.
        /// </param>
        /// <param name="element">
        /// The element of which this property is created for.
        /// </param>
        /// <param name="elementType">
        /// The element type of which this quantity is created for.
        /// </param>
        /// <returns>
        /// Then created quantity handle.
        /// </returns>
        public IFCAnyHandle ProcessEntry(IFCFile file, ExporterIFC exporterIFC,
           IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            bool useProperty = (!String.IsNullOrEmpty(RevitParameterName)) || (RevitBuiltInParameter != BuiltInParameter.INVALID);

            bool success = false;
            double val = 0;
            if (useProperty)
            {
                success = ParameterUtil.GetDoubleValueFromElementOrSymbol(element, RevitParameterName, out val);
                if (!success && RevitBuiltInParameter != BuiltInParameter.INVALID)
                    success = ParameterUtil.GetDoubleValueFromElementOrSymbol(element, RevitBuiltInParameter, out val);
            }

            if (PropertyCalculator != null && !success)
            {
                success = PropertyCalculator.Calculate(exporterIFC, extrusionCreationData, element, elementType);
                if (success)
                    val = PropertyCalculator.GetDoubleValue();
            }

            IFCAnyHandle quantityHnd = null;
            if (success)
            {
                switch (QuantityType)
                {
                    case QuantityType.PositiveLength:
                        quantityHnd = IFCInstanceExporter.CreateQuantityLength(file, PropertyName, MethodOfMeasurement, null, val);
                        break;
                    case QuantityType.Area:
                        quantityHnd = IFCInstanceExporter.CreateQuantityArea(file, PropertyName, MethodOfMeasurement, null, val);
                        break;
                    case QuantityType.Volume:
                        quantityHnd = IFCInstanceExporter.CreateQuantityVolume(file, PropertyName, MethodOfMeasurement, null, val);
                        break;
                    default:
                        throw new InvalidOperationException("Missing case!");
                }
            }

            return quantityHnd;
        }
    }
}
