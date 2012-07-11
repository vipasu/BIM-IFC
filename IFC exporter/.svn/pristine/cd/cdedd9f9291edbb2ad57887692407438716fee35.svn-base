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
using Autodesk.Revit.DB.Structure;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// A utility class that execute a calculation to determine the value of special IFC parameters.
    /// </summary>
    abstract class PropertyCalculator
    {
        /// <summary>
        /// Performs the calculation.
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
        abstract public bool Calculate(ExporterIFC exporterIFC, IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType);

        /// <summary>
        /// Determines if the calculator calculates only one parameter, or multiple.
        /// </summary>
        public virtual bool CalculatesMultipleParameters
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the calculator calculates only one value, or multiple.
        /// </summary>
        public virtual bool CalculatesMutipleValues
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the calculated string value.  Use if CalculatesMultipleParameters is false.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <returns>
        /// The calculated string value.
        /// </returns>
        public virtual string GetStringValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated string values.  Use if CalculatesMutipleValues is true.
        /// </summary>
        /// <returns>The list of strings.</returns>
        public virtual IList<string> GetStringValues()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated boolean value.  Use if CalculatesMultipleParameters is false.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <returns>
        /// The calculated boolean value.
        /// </returns>
        public virtual bool GetBooleanValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated double value.  Use if CalculatesMultipleParameters is false.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <returns>
        /// The calculated double value.
        /// </returns>
        public virtual double GetDoubleValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated integer value.  Use if CalculatesMultipleParameters is false.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <returns>
        /// The calculated integer value.
        /// </returns>
        public virtual int GetIntValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated double value for a given parameter.  Use if CalculatesMultipleParameters is true.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <param name="paramName">
        /// The name of the parameter.
        /// </param>
        /// <returns>
        /// The calculated double value.
        /// </returns>
        public virtual double GetDoubleValue(string paramName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the calculated integer value for a given parameter.  Use if CalculatesMultipleParameters is true.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Default method is not implemented.
        /// </exception>
        /// <param name="paramName">
        /// The name of the parameter.
        /// </param>
        /// <returns>
        /// The calculated integer value.
        /// </returns>
        public virtual int GetIntValue(string paramName)
        {
            throw new NotImplementedException();
        }
    }
}