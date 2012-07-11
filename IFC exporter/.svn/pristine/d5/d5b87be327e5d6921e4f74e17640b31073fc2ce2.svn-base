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
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// Represents the type of the container for a property.
    /// </summary>
    enum PropertyValueType
    {
        /// <summary>
        /// A single property (IfcSingleValue)
        /// </summary>
        SingleValue,
        /// <summary>
        /// An enumerated property (IfcEnumeratedValue)
        /// </summary>
        EnumeratedValue,
        /// <summary>
        /// A list property (IfcListValue)
        /// </summary>
        ListValue
    }

    /// <summary>
    /// Represents the type of a property.
    /// </summary>
    enum PropertyType
    {
        /// <summary>
        /// A label (string value).
        /// </summary>
        Label,
        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean,
        /// <summary>
        /// A real number value.
        /// </summary>
        Integer,
        /// <summary>
        /// An integer number value.
        /// </summary>
        Real,
        /// <summary>
        /// A positive length value.
        /// </summary>
        PositiveLength,
        /// <summary>
        /// A positive ratio value.
        /// </summary>
        PositiveRatio,
        /// <summary>
        /// An angular value.
        /// </summary>
        PlaneAngle,
        /// <summary>
        /// An area value.
        /// </summary>
        Area,
        /// <summary>
        /// An identifier value.
        /// </summary>
        Identifier
    }

    /// <summary>
    /// Represents a mapping from a Revit parameter or calculated quantity to an IFC property.
    /// </summary>
    class PropertySetEntry : Entry
    {
        /// <summary>
        /// The type of the IFC property set entry. Default is label.
        /// </summary>
        PropertyType m_PropertyType = PropertyType.Label;

        /// <summary>
        /// The value type of the IFC property set entry.
        /// </summary>
        PropertyValueType m_PropertyValueType = PropertyValueType.SingleValue;

        /// <summary>
        /// Constructs a PropertySetEntry object.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        private PropertySetEntry(string revitParameterName)
            : base(revitParameterName)
        {

        }

        /// <summary>
        /// The type of the IFC property set entry.
        /// </summary>
        public PropertyType PropertyType
        {
            get
            {
                return m_PropertyType;
            }
            private set
            {
                m_PropertyType = value;
            }
        }

        /// <summary>
        /// The value type of the IFC property set entry.
        /// </summary>
        public PropertyValueType PropertyValueType
        {
            get
            {
                return m_PropertyValueType;
            }
            private set
            {
                m_PropertyValueType = value;
            }
        }

        /// <summary>
        /// Creates an entry of type real.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateReal(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Real;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type boolean.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateBoolean(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Boolean;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type label.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateLabel(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Label;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type identifier.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateIdentifier(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Identifier;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type integer.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateInteger(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Integer;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type area.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateArea(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.Area;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type positive length.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePositiveLength(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PositiveLength;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type positive ratio.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePositiveRatio(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PositiveRatio;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type enumerated value.
        /// The type of the enumarated value is also given.
        /// Note that the enumeration list is not supported here.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreateEnumeratedValue(string revitParameterName, PropertyType propertyType)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = propertyType;
            pse.PropertyValueType = PropertyValueType.EnumeratedValue;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type list value.
        /// The type of the list value is also given.
        /// </summary>
        /// <param name="revitParameterName">Revit parameter name.</param>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The PropertySetEntry.</returns>
        public static PropertySetEntry CreateListValue(string revitParameterName, PropertyType propertyType)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = propertyType;
            pse.PropertyValueType = PropertyValueType.ListValue;
            return pse;
        }

        /// <summary>
        /// Creates an entry of type plane angle.
        /// </summary>
        /// <param name="revitParameterName">
        /// Revit parameter name.
        /// </param>
        /// <returns>
        /// The PropertySetEntry.
        /// </returns>
        public static PropertySetEntry CreatePlaneAngle(string revitParameterName)
        {
            PropertySetEntry pse = new PropertySetEntry(revitParameterName);
            pse.PropertyType = PropertyType.PlaneAngle;
            return pse;
        }

        /// <summary>
        /// Process to create element property.
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
        /// The element type of which this property is created for.
        /// </param>
        /// <returns>
        /// Then created property handle.
        /// </returns>
        public IFCAnyHandle ProcessEntry(IFCFile file, ExporterIFC exporterIFC,
           IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            bool useProperty = (!UseCalculatorOnly && (!String.IsNullOrEmpty(RevitParameterName)) || (RevitBuiltInParameter != BuiltInParameter.INVALID));

            IFCAnyHandle propHnd = null;

            if (useProperty)
            {
                propHnd = CreatePropertyFromElementOrSymbol(file, exporterIFC, element);
            }

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
            {
                propHnd = CreatePropertyFromCalculator(file, exporterIFC, extrusionCreationData, element, elementType);
            }

            return propHnd;
        }

        /// <summary>
        /// Creates a property from element or its type's parameter.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="element">The element.</param>
        /// <returns>The property handle.</returns>
        IFCAnyHandle CreatePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element element)
        {
            IFCAnyHandle propHnd = null;
            PropertyType propertyType = PropertyType;
            PropertyValueType valueType = PropertyValueType;

            string ifcPropertyName = (!String.IsNullOrEmpty(PropertyName)) ? PropertyName : RevitParameterName;

            switch (propertyType)
            {
                case PropertyType.Label:
                    {
                        propHnd = PropertyUtil.CreateLabelPropertyFromElementOrSymbol(file, element, RevitParameterName, RevitBuiltInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Identifier:
                    {
                        propHnd = PropertyUtil.CreateIdentifierPropertyFromElementOrSymbol(file, element, RevitParameterName, RevitBuiltInParameter, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Boolean:
                    {
                        propHnd = PropertyUtil.CreateBooleanPropertyFromElementOrSymbol(file, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Integer:
                    {
                        propHnd = PropertyUtil.CreateIntegerPropertyFromElementOrSymbol(file, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Real:
                    {
                        double scale = exporterIFC.LinearScale;
                        propHnd = PropertyUtil.CreateRealPropertyFromElementOrSymbol(file, scale, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.PositiveLength:
                    {
                        propHnd = PropertyUtil.CreatePositiveLengthMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.PositiveRatio:
                    {
                        propHnd = PropertyUtil.CreatePositiveRatioPropertyFromElementOrSymbol(file, exporterIFC, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.PlaneAngle:
                    {
                        propHnd = PropertyUtil.CreatePlaneAngleMeasurePropertyFromElementOrSymbol(file, element, RevitParameterName, ifcPropertyName, valueType);
                        break;
                    }
                case PropertyType.Area:
                    {
                        propHnd = PropertyUtil.CreateAreaMeasurePropertyFromElementOrSymbol(file, exporterIFC, element, RevitParameterName, RevitBuiltInParameter, ifcPropertyName, valueType);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            return propHnd;
        }

        /// <summary>
        /// Creates a property from the calculator.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="extrusionCreationData">The IFCExtrusionCreationData.</param>
        /// <param name="element">The element.</param>
        /// <param name="elementType">The element type.</param>
        /// <returns>The property handle.</returns>
        IFCAnyHandle CreatePropertyFromCalculator(IFCFile file, ExporterIFC exporterIFC,
           IFCExtrusionCreationData extrusionCreationData, Element element, ElementType elementType)
        {
            IFCAnyHandle propHnd = null;

            if (PropertyCalculator != null && PropertyCalculator.Calculate(exporterIFC, extrusionCreationData, element, elementType))
            {
                PropertyType propertyType = PropertyType;
                PropertyValueType valueType = PropertyValueType;

                switch (propertyType)
                {
                    case PropertyType.Label:
                        {
                            if (PropertyCalculator.CalculatesMutipleValues)
                                propHnd = PropertyUtil.CreateLabelProperty(file, PropertyName, PropertyCalculator.GetStringValues(), valueType);
                            else
                                propHnd = PropertyUtil.CreateLabelPropertyFromCache(file, PropertyName, PropertyCalculator.GetStringValue(), valueType, false);
                            break;
                        }
                    case PropertyType.Identifier:
                        {
                            propHnd = PropertyUtil.CreateIdentifierPropertyFromCache(file, PropertyName, PropertyCalculator.GetStringValue(), valueType);
                            break;
                        }
                    case PropertyType.Boolean:
                        {
                            propHnd = PropertyUtil.CreateBooleanPropertyFromCache(file, PropertyName, PropertyCalculator.GetBooleanValue(), valueType);
                            break;
                        }
                    case PropertyType.Integer:
                        {
                            if (PropertyCalculator.CalculatesMultipleParameters)
                                propHnd = PropertyUtil.CreateIntegerPropertyFromCache(file, PropertyName, PropertyCalculator.GetIntValue(PropertyName), valueType);
                            else
                                propHnd = PropertyUtil.CreateIntegerPropertyFromCache(file, PropertyName, PropertyCalculator.GetIntValue(), valueType);
                            break;
                        }
                    case PropertyType.Real:
                        {
                            double scale = exporterIFC.LinearScale;
                            propHnd = PropertyUtil.CreateRealPropertyFromCache(file, scale, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PositiveLength:
                        {
                            if (PropertyCalculator.CalculatesMultipleParameters)
                                propHnd = PropertyUtil.CreatePositiveLengthMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(PropertyName), valueType);
                            else
                                propHnd = PropertyUtil.CreatePositiveLengthMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PositiveRatio:
                        {
                            propHnd = PropertyUtil.CreatePositiveRatioMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.PlaneAngle:
                        {
                            propHnd = PropertyUtil.CreatePlaneAngleMeasurePropertyFromCache(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    case PropertyType.Area:
                        {
                            propHnd = PropertyUtil.CreateAreaMeasureProperty(file, PropertyName, PropertyCalculator.GetDoubleValue(), valueType);
                            break;
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }

            return propHnd;
        }
    }
}
