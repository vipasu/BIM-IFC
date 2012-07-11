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


namespace BIM.IFC.Utility
{
    /// <summary>
    /// Provides static methods for parameter related manipulations.
    /// </summary>
    class ParameterUtil
    {
        /// <summary>
        /// Gets string value from parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when propertyName is null or empty.
        /// </exception>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetStringValueFromElement(Element element, string propertyName, out string propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("It is null or empty.", "propertyName");

            propertyValue = string.Empty;

            Parameter parameter = getParameterFromName(element, propertyName);

            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.String)
            {
                if (parameter.AsString() != null)
                {
                    propertyValue = parameter.AsString();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets integer value from parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when propertyName is null or empty.
        /// </exception>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetIntValueFromElement(Element element, string propertyName, out int propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("It is null or empty.", "propertyName");

            propertyValue = 0;

            Parameter parameter = getParameterFromName(element, propertyName);

            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.Integer)
            {
                propertyValue = parameter.AsInteger();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets double value from parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when propertyName is null or empty.
        /// </exception>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetDoubleValueFromElement(Element element, string propertyName, out double propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("It is null or empty.", "propertyName");

            propertyValue = 0.0;

            Parameter parameter = getParameterFromName(element, propertyName);

            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.Double)
            {
                propertyValue = parameter.AsDouble();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets string value from built-in parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="builtInParameter">
        /// The built-in parameter.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when builtInParameter in invalid.
        /// </exception>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetStringValueFromElement(Element element, BuiltInParameter builtInParameter, out string propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (builtInParameter == BuiltInParameter.INVALID)
                throw new ArgumentException("BuiltInParameter is INVALID", "builtInParameter");

            propertyValue = String.Empty;

            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.String)
            {
                propertyValue = parameter.AsString();
                return true;
            }

            return false;
        }

        /// <summary>Gets string value from built-in parameter of an element or its type.</summary>
        /// <param name="element">The element.</param>
        /// <param name="builtInParameter">The built-in parameter.</param>
        /// <param name="nullAllowed">true if we allow the property value to be empty.</param>
        /// <param name="propertyValue">The output property value.</param>
        /// <returns>True if get the value successfully, false otherwise.</returns>
        public static bool GetStringValueFromElementOrSymbol(Element element, BuiltInParameter builtInParameter, bool nullAllowed, out string propertyValue)
        {
            if (GetStringValueFromElement(element, builtInParameter, out propertyValue))
            {
                if (!String.IsNullOrEmpty(propertyValue))
                    return true;
            }

            bool found = false;
            Element elementType = element.Document.GetElement(element.GetTypeId());
            if (elementType != null)
            {
                found = GetStringValueFromElement(elementType, builtInParameter, out propertyValue);
                if (!nullAllowed && !String.IsNullOrEmpty(propertyValue))
                    found = false;
            }

            return found;
        }

        /// <summary>
        /// Sets string value of a built-in parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="builtInParameter">
        /// The built-in parameter.
        /// </param>
        /// <param name="propertyValue">
        /// The property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when builtInParameter in invalid.
        /// </exception>
        public static void SetStringParameter(Element element, BuiltInParameter builtInParameter, string propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (builtInParameter == BuiltInParameter.INVALID)
                throw new ArgumentException("BuiltInParameter is INVALID", "builtInParameter");

            Parameter parameter = element.get_Parameter(builtInParameter);
            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.String)
            {
                parameter.SetValueString(propertyValue);
                return;
            }
            else
            {
                ElementId parameterId = new ElementId(builtInParameter);
                ExporterIFCUtils.AddValueString(element, parameterId, propertyValue);
            }
        }

        /// <summary>
        /// Gets double value from built-in parameter of an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="builtInParameter">
        /// The built-in parameter.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when element is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when builtInParameter in invalid.
        /// </exception>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetDoubleValueFromElement(Element element, BuiltInParameter builtInParameter, out double propertyValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (builtInParameter == BuiltInParameter.INVALID)
                throw new ArgumentException("BuiltInParameter is INVALID", "builtInParameter");

            propertyValue = 0.0;

            Parameter parameter = element.get_Parameter(builtInParameter);

            if (parameter != null && parameter.HasValue && parameter.StorageType == StorageType.Double)
            {
                propertyValue = parameter.AsDouble();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets double value from built-in parameter of an element or its element type.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="builtInParameter">
        /// The built-in parameter.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetDoubleValueFromElementOrSymbol(Element element, BuiltInParameter builtInParameter, out double propertyValue)
        {
            if (GetDoubleValueFromElement(element, builtInParameter, out propertyValue))
                return true;
            else
            {
                Document document = element.Document;
                ElementId typeId = element.GetTypeId();

                Element elemType = document.GetElement(typeId);
                if (elemType != null)
                {
                    return GetDoubleValueFromElement(elemType, builtInParameter, out propertyValue);
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets double value from parameter of an element or its element type.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetDoubleValueFromElementOrSymbol(Element element, string propertyName, out double propertyValue)
        {
            if (GetDoubleValueFromElement(element, propertyName, out propertyValue))
                return true;
            else
            {
                Document document = element.Document;
                ElementId typeId = element.GetTypeId();

                Element elemType = document.GetElement(typeId);
                if (elemType != null)
                {
                    return GetDoubleValueFromElement(elemType, propertyName, out propertyValue);
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets positive double value from parameter of an element or its element type.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <returns>
        /// True if get the value successfully and the value is positive, false otherwise.
        /// </returns>
        public static bool GetPositiveDoubleValueFromElementOrSymbol(Element element, string propertyName, out double propertyValue)
        {
            bool found = GetDoubleValueFromElementOrSymbol(element, propertyName, out propertyValue);
            if (found && (propertyValue > 0.0))
                return true;
            return false;
        }

        /// <summary>
        /// Gets the parameter by name from an element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <returns>
        /// The Parameter.
        /// </returns>
        static Parameter getParameterFromName(Element element, string propertyName)
        {
            ParameterSet parameterIds = element.Parameters;
            if (parameterIds.Size == 0)
                return null;

            IList<Parameter> parameters = new List<Parameter>();
            IList<Definition> paramDefinitions = new List<Definition>();

            // We will do two passes.  In the first pass, we will look at parameters in the IFC group.
            // In the second pass, we will look at all other groups.
            ParameterSetIterator parameterIt = parameterIds.ForwardIterator();

            while (parameterIt.MoveNext())
            {
                Parameter parameter = parameterIt.Current as Parameter;

                Definition paramDefinition = parameter.Definition;
                if (paramDefinition == null)
                    continue;
                if (paramDefinition.ParameterGroup != BuiltInParameterGroup.PG_IFC)
                {
                    parameters.Add(parameter);
                    paramDefinitions.Add(paramDefinition);
                    continue;
                }

                if (NamingUtil.IsEqualIgnoringCaseAndSpaces(paramDefinition.Name, propertyName))
                    return parameter;
            }

            int size = paramDefinitions.Count;
            for (int ii = 0; ii < size; ii++)
            {
                if (NamingUtil.IsEqualIgnoringCaseAndSpaces(paramDefinitions[ii].Name, propertyName))
                    return parameters[ii];
            }

            return null;
        }

        /// <summary>
        /// Gets string value from parameter of an element or its element type.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="propertyValue">
        /// The output property value.
        /// </param>
        /// <returns>
        /// True if get the value successfully, false otherwise.
        /// </returns>
        public static bool GetStringValueFromElementOrSymbol(Element element, string propertyName, out string propertyValue)
        {
            if (GetStringValueFromElement(element, propertyName, out propertyValue))
                return true;
            else
            {
                Document document = element.Document;
                ElementId typeId = element.GetTypeId();

                Element elemType = document.GetElement(typeId);
                if (elemType != null)
                {
                    return GetStringValueFromElement(elemType, propertyName, out propertyValue);
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets integer value from parameter of an element or its element type.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyValue">The output property value.</param>
        /// <returns>True if get the value successfully, false otherwise.</returns>
        public static bool GetIntValueFromElementOrSymbol(Element element, string propertyName, out int propertyValue)
        {
            if (GetIntValueFromElement(element, propertyName, out propertyValue))
                return true;
            else
            {
                Document document = element.Document;
                ElementId typeId = element.GetTypeId();

                Element elemType = document.GetElement(typeId);
                if (elemType != null)
                {
                    return GetIntValueFromElement(elemType, propertyName, out propertyValue);
                }
                else
                    return false;
            }
        }
    }
}
