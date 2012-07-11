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

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// Represents a mapping from a Revit parameter or calculated value to an IFC property or quantity.
    /// </summary>
    /// <remarks>
    /// Symbol property is true if the property comes from the symbol (vs. the element itself).  Default is TRUE.
    /// Revit parameter type defaults to RPTString.
    ///
    /// One of the following:
    /// <list type="bullet">
    /// <item>Revit parameter name</item>
    /// <item>Revit built-in parameter</item>
    /// <item>Calculator</item>
    /// </list>
    /// must be set. If more than one is valid,
    /// generally, parameter name is used first, followed by parameter id,
    /// then by function.
    /// </remarks>
    abstract class Entry
    {
        /// <summary>
        /// The parameter name to be used to get the parameter value.
        /// </summary>
        string m_RevitParameterName = String.Empty;

        /// <summary>
        /// The name for IFC property.
        /// </summary>
        string m_PropertyName = String.Empty;

        /// <summary>
        /// Indicates if the property is for element type.
        /// </summary>
        bool m_IsElementTypeProperty = true;

        /// <summary>
        /// The built in parameter.
        /// </summary>
        BuiltInParameter m_RevitBuiltInParameter = BuiltInParameter.INVALID;

        /// <summary>
        /// The property calculator to calculate the property value.
        /// </summary>
        PropertyCalculator m_PropertyCalculator;

        /// <summary>
        /// Indicates if the property value is retrieved only from the calculator.
        /// </summary>
        bool m_UseCalculatorOnly = false;

        /// <summary>
        /// Constructor to create an Entry object.
        /// </summary>
        /// <param name="revitParameterName">
        /// The parameter name for this Entry.
        /// </param>
        public Entry(string revitParameterName)
        {
            this.m_RevitParameterName = revitParameterName;
            this.m_PropertyName = revitParameterName;
        }

        /// <summary>
        /// The internationalized name of the parameter in Revit (if it exists).
        /// </summary>
        public string RevitParameterName
        {
            get
            {
                return m_RevitParameterName;
            }
            set
            {
                m_RevitParameterName = value;
            }
        }

        /// <summary>
        /// True if the property comes from the element's type (vs. the element itself).
        /// </summary>
        /// <remarks>
        /// The default value is true.
        /// </remarks>
        public bool IsElementTypeProperty
        {
            get
            {
                return m_IsElementTypeProperty;
            }
            set
            {
                m_IsElementTypeProperty = value;
            }
        }

        /// <summary>
        /// The built-in parameter.
        /// </summary>
        public BuiltInParameter RevitBuiltInParameter
        {
            get
            {
                return m_RevitBuiltInParameter;
            }
            set
            {
                m_RevitBuiltInParameter = value;
            }
        }

        /// <summary>
        /// The name of the property or quantity as stored in the IFC export.
        /// </summary>
        /// <remarks>
        /// Default is empty; if empty the name of the Revit parameter will be used.
        /// </remarks>
        public string PropertyName
        {
            get
            {
                return m_PropertyName;
            }
            set
            {
                m_PropertyName = value;
            }
        }

        /// <summary>
        /// The instance of a class that can calculate the value of the property or quantity.
        /// </summary>
        public PropertyCalculator PropertyCalculator
        {
            get
            {
                return m_PropertyCalculator;
            }
            set
            {
                m_PropertyCalculator = value;
            }
        }

        /// <summary>
        /// Indicates if the property value is retrieved only from the calculator.
        /// </summary>
        public bool UseCalculatorOnly
        {
            get
            {
                return m_UseCalculatorOnly;
            }
            set
            {
                m_UseCalculatorOnly = value;
            }
        }
    }
}