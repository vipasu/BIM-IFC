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
    /// Provides static methods for naming and string related operations.
    /// </summary>
    class NamingUtil
    {
        /// <summary>
        /// Removes spaces in a string.
        /// </summary>
        /// <param name="originalString">
        /// The original string.
        /// </param>
        /// <param name="newString">
        /// The output string.
        /// </param>
        public static void RemoveSpaces(string originalString, out string newString)
        {
            newString = string.Empty;
            string[] subSections = originalString.Split(' ');
            int size = subSections.Length;
            for (int ii = 0; ii < size; ii++)
                newString += subSections[ii];
            return;
        }

        /// <summary>
        /// Removes underscores in a string.
        /// </summary>
        /// <param name="originalString">
        /// The original string.
        /// </param>
        /// <param name="newString">
        /// The output string.
        /// </param>
        public static void RemoveUnderscores(string originalString, out string newString)
        {
            newString = string.Empty;
            string[] subSections = originalString.Split('_');
            int size = subSections.Length;
            for (int ii = 0; ii < size; ii++)
                newString += subSections[ii];
            return;
        }

        /// <summary>
        /// Checks if two strings are equal ignoring case and spaces.
        /// </summary>
        /// <param name="string1">
        /// The string to be compared.
        /// </param>
        /// <param name="string2">
        /// The other string to be compared.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public static bool IsEqualIgnoringCaseAndSpaces(string string1, string string2)
        {
            string nospace1 = string.Empty;
            string nospace2 = string.Empty;
            RemoveSpaces(string1, out nospace1);
            RemoveSpaces(string2, out nospace2);
            return (string.Compare(nospace1, nospace2, true) == 0);
        }

        /// <summary>
        /// Checks if two strings are equal ignoring case, spaces and underscores.
        /// </summary>
        /// <param name="string1">
        /// The string to be compared.
        /// </param>
        /// <param name="string2">
        /// The other string to be compared.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public static bool IsEqualIgnoringCaseSpacesAndUnderscores(string string1, string string2)
        {
            string nospace1 = string.Empty;
            string nospace2 = string.Empty;
            RemoveSpaces(string1, out nospace1);
            RemoveSpaces(string2, out nospace2);
            string nospaceOrUndescore1 = string.Empty;
            string nospaceOrUndescore2 = string.Empty;
            RemoveUnderscores(nospace1, out nospaceOrUndescore1);
            RemoveUnderscores(nospace2, out nospaceOrUndescore2);
            return (string.Compare(nospaceOrUndescore1, nospaceOrUndescore2, true) == 0);
        }

        /// <summary>
        /// Gets override string value from element parameter.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="paramName">
        /// The parameter name.
        /// </param>
        /// <param name="originalValue">
        /// The original value.
        /// </param>
        /// <returns>
        /// The string contains the string value.
        /// </returns>
        public static string GetOverrideStringValue(Element element, string paramName, string originalValue)
        {
            string strValue;

            // get the IFC Name Override 
            if (element != null)
            {
                if (ParameterUtil.GetStringValueFromElement(element, paramName, out strValue))
                {
                    return strValue;
                }
            }

            return originalValue;
        }

        /// <summary>
        /// Gets override name from element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="originalValue">
        /// The original value.
        /// </param>
        /// <returns>
        /// The string contains the name string value.
        /// </returns>
        public static string GetNameOverride(Element element, string originalValue)
        {
            string nameOverride = "NameOverride";
            return GetOverrideStringValue(element, nameOverride, originalValue);
        }

        /// <summary>
        /// Gets override description from element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="originalValue">
        /// The original value.
        /// </param>
        /// <returns>
        /// The string contains the description string value.
        /// </returns>
        public static string GetDescriptionOverride(Element element, string originalValue)
        {
            string nameOverride = "IfcDescription";
            return GetOverrideStringValue(element, nameOverride, originalValue);
        }

        /// <summary>
        /// Gets override object type from element.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="originalValue">
        /// The original value.
        /// </param>
        /// <returns>
        /// The string contains the object type string value.
        /// </returns>
        public static string GetObjectTypeOverride(Element element, string originalValue)
        {
            string nameOverride = "ObjectTypeOverride";
            return GetOverrideStringValue(element, nameOverride, originalValue);
        }

        /// <summary>
        /// Creates an IFC name from export state.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="index">
        /// The index of the name. If it is larger than 0, it is appended to the name.
        /// </param>
        /// <returns>
        /// The string contains the name string value.
        /// </returns>
        public static string CreateIFCName(ExporterIFC exporterIFC, int index)
        {
            string elementName = exporterIFC.GetName();
            if (index >= 0)
            {
                elementName += ":";
                elementName += index.ToString();
            }

            return elementName;
        }

        /// <summary>
        /// Creates an IFC family name from export state.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="index">
        /// The index of the name. If it is larger than 0, it is appended to the name.
        /// </param>
        /// <returns>
        /// The string contains the name string value.
        /// </returns>
        public static string CreateIFCFamilyName(ExporterIFC exporterIFC, int index)
        {
            string elementName = exporterIFC.GetFamilyName();
            if (index >= 0)
            {
                elementName += ":";
                elementName += index.ToString();
            }

            return elementName;
        }

        /// <summary>
        /// Creates an IFC object name from export state.
        /// </summary>
        /// <remarks>
        /// It is combined with family name and element type id.
        /// </remarks>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The string contains the name string value.
        /// </returns>
        public static string CreateIFCObjectName(ExporterIFC exporterIFC, Element element)
        {
            ElementId typeId = element != null ? element.GetTypeId() : ElementId.InvalidElementId;

            string objectName = exporterIFC.GetFamilyName();
            if (typeId != ElementId.InvalidElementId)
            {
                if (objectName == "")
                    return typeId.ToString();
                else
                    return (objectName + ":" + typeId.ToString());
            }
            return "";
        }

        /// <summary>
        /// Creates an IFC element id string from element id.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The string contains the name string value.
        /// </returns>
        public static string CreateIFCElementId(Element element)
        {
            if (element == null)
                return "NULL";

            string elemIdString = element.Id.ToString();
            return elemIdString;
        }

        /// <summary>
        /// Parses the name string and gets the parts separately.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="lastName">
        /// The output last name.
        /// </param>
        /// <param name="firstName">
        /// The output first name.
        /// </param>
        /// <param name="middleNames">
        /// The output middle names.
        /// </param>
        /// <param name="prefixTitles">
        /// The output prefix titles.
        /// </param>
        /// <param name="suffixTitles">
        /// The output suffix titles.
        /// </param>
        public static void ParseName(string name, out string lastName, out string firstName, out List<string> middleNames, out List<string> prefixTitles, out List<string> suffixTitles)
        {
            lastName = string.Empty;
            firstName = string.Empty;
            middleNames = null;
            prefixTitles = null;
            suffixTitles = null;

            if (String.IsNullOrEmpty(name))
                return;

            string currName = name;
            List<string> names = new List<string>();
            int noEndlessLoop = 0;
            int index = 0;
            bool foundComma = false;

            do
            {
                int currIndex = index;   // index might get reset by comma.

                currName = currName.TrimStart(' ');
                if (String.IsNullOrEmpty(currName))
                    break;

                int comma = foundComma ? currName.Length : currName.IndexOf(',');
                if (comma == -1) comma = currName.Length;
                int space = currName.IndexOf(' ');
                if (space == -1) space = currName.Length;

                // treat comma as space, mark found.
                if (comma < space)
                {
                    foundComma = true;
                    index = -1; // start inserting at the beginning again.
                    space = comma;
                }

                if (space == currName.Length)
                {
                    names.Add(currName);
                    break;
                }
                else if (space == 0)
                {
                    if (comma == 0)
                        continue;
                    else
                        break;   // shouldn't happen
                }

                names.Insert(currIndex, currName.Substring(0, space));
                index++;
                currName = currName.Substring(space + 1);
                noEndlessLoop++;

            } while (noEndlessLoop < 100);


            // parse names.
            // assuming anything ending in a dot is a prefix.
            int sz = names.Count;
            for (index = 0; index < sz; index++)
            {
                if (names[index].LastIndexOf('.') == names[index].Length - 1)
                {
                    if (prefixTitles == null)
                        prefixTitles = new List<string>();
                    prefixTitles.Add(names[index]);
                }
                else
                    break;
            }

            if (index < sz)
            {
                firstName = names[index++];
            }

            // suffixes, if any.  Note this misses "III", "IV", etc., but this is not that important!
            int lastIndex;
            for (lastIndex = sz - 1; lastIndex >= index; lastIndex--)
            {
                if (names[lastIndex].LastIndexOf('.') == names[lastIndex].Length - 1)
                {
                    if (suffixTitles == null)
                        suffixTitles = new List<string>();
                    suffixTitles.Insert(0, names[lastIndex]);
                }
                else
                    break;
            }

            if (lastIndex >= index)
            {
                lastName = names[lastIndex--];
            }

            // rest are middle names.
            for (; index <= lastIndex; index++)
            {
                if (middleNames == null)
                    middleNames = new List<string>();
                middleNames.Add(names[index]);
            }
        }
    }
}