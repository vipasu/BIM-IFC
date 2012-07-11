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
    /// Provides static methods to create varies IFC properties.
    /// </summary>
    class PropertyUtil
    {
        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLabelProperty(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsLabel(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <param name="cacheAllStrings">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLabelPropertyFromCache(IFCFile file, string propertyName, string value, PropertyValueType valueType, bool cacheAllStrings)
        {
            bool canCache = (value == String.Empty) || cacheAllStrings;
            StringPropertyInfoCache stringInfoCache = null;
            IFCAnyHandle labelHandle = null;
            
            if (canCache)
            {
                stringInfoCache = ExporterCacheManager.StringPropertyInfoCache;
                labelHandle = stringInfoCache.Find(propertyName, value);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(labelHandle))
                    return labelHandle;
            }

            labelHandle = CreateLabelProperty(file, propertyName, value, valueType);

            if (canCache)
            {
                stringInfoCache.Add(propertyName, value, labelHandle);
            }

            return labelHandle;
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="values">The values of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLabelProperty(IFCFile file, string propertyName, IList<string> values, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        foreach (string value in values)
                        {
                            valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        }
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.ListValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        foreach (string value in values)
                        {
                            valueList.Add(IFCDataUtil.CreateAsLabel(value));
                        }
                        return IFCInstanceExporter.CreatePropertyListValue(file, propertyName, null, valueList, null);
                    }
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an identifier property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierProperty(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsIdentifier(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    {
                        return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsIdentifier(value), null);
                    }
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an identifier property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromCache(IFCFile file, string propertyName, string value, PropertyValueType valueType)
        {
            StringPropertyInfoCache stringInfoCache = ExporterCacheManager.StringPropertyInfoCache;
            IFCAnyHandle stringHandle = stringInfoCache.Find(propertyName, value);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(stringHandle))
                return stringHandle;

            stringHandle = CreateIdentifierProperty(file, propertyName, value, valueType);

            stringInfoCache.Add(propertyName, value, stringHandle);
            return stringHandle;
        }
    
        /// <summary>
        /// Create a boolean property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateBooleanProperty(IFCFile file, string propertyName, bool value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsBoolean(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsBoolean(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a boolean property or gets one from cache.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateBooleanPropertyFromCache(IFCFile file, string propertyName, bool value, PropertyValueType valueType)
        {
            BooleanPropertyInfoCache boolInfoCache = ExporterCacheManager.BooleanPropertyInfoCache;
            IFCAnyHandle boolHandle = boolInfoCache.Find(propertyName, value);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(boolHandle))
                return boolHandle;

            boolHandle = CreateBooleanProperty(file, propertyName, value, valueType);
            boolInfoCache.Add(propertyName, value, boolHandle);
            return boolHandle;
        }

        /// <summary>
        /// Create an integer property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIntegerProperty(IFCFile file, string propertyName, int value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsInteger(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsInteger(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create an integer property or gets one from cache.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateIntegerPropertyFromCache(IFCFile file, string propertyName, int value, PropertyValueType valueType)
        {
            bool canCache = (value >= -10 && value <= 10);
            IFCAnyHandle intHandle = null;
            IntegerPropertyInfoCache intInfoCache = null;
            if (canCache)
            {
                intInfoCache = ExporterCacheManager.IntegerPropertyInfoCache;
                intHandle = intInfoCache.Find(propertyName, value);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(intHandle))
                    return intHandle;
            }

            intHandle = CreateIntegerProperty(file, propertyName, value, valueType);
            if (canCache)
            {
                intInfoCache.Add(propertyName, value, intHandle);
            }
            return intHandle;
        }

        internal static double? CanCacheDouble(double scale, double value)
        {
            // We have a partial cache here.
            // For scale = 1.0 (feet), cache multiples of 1/2" up to 10'.
            // For scale = 0.03048 (meter), cache multiples of 50mm up to 10m.
            // For scale = 304.8 (mm), cache multiples of 50mm up to 10m.
            
            if (MathUtil.IsAlmostZero(value))
            {
                return 0.0;
            }
            else
            {
                // approximate tests for most common scales are good enough here.
                if (MathUtil.IsAlmostEqual(scale, 1.0) || MathUtil.IsAlmostEqual(scale, 12.0))
                {
                    double multiplier = 24 / scale;
                    double lengthInHalfInches = Math.Floor(value * multiplier + 0.5);
                    if (lengthInHalfInches > 0 && lengthInHalfInches <= 240 && MathUtil.IsAlmostZero(value * multiplier - lengthInHalfInches))
                    {
                        return lengthInHalfInches / multiplier;
                    }
                }
                else
                {
                    double multiplier = (304.8 / scale) / 50;
                    double lengthIn50mm = Math.Floor(value * multiplier + 0.5);
                    if (lengthIn50mm > 0 && lengthIn50mm <= 200 && MathUtil.IsAlmostZero(value * multiplier - lengthIn50mm))
                    {
                        return lengthIn50mm / multiplier;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Create a real property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateRealProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsReal(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsReal(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a real property, using a cached value if possible.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created or cached property handle.
        /// </returns>
        public static IFCAnyHandle CreateRealPropertyFromCache(IFCFile file, double scale, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheDouble(scale, value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
            {
                value = adjustedValue.GetValueOrDefault();
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.DoublePropertyInfoCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreateRealProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.DoublePropertyInfoCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>
        /// Creates a length measure property or gets one from cache.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateLengthMeasurePropertyFromCache(IFCFile file, double scale, string propertyName, double value, PropertyValueType valueType)
        {
            double? adjustedValue = CanCacheDouble(scale, value);
            bool canCache = adjustedValue.HasValue;
            if (canCache)
            {
                value = adjustedValue.GetValueOrDefault();
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.DoublePropertyInfoCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsLengthMeasure(value));
                        propertyHandle = IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                        break;
                    }
                case PropertyValueType.SingleValue:
                    propertyHandle = IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsLengthMeasure(value), null);
                    break;
                default:
                    throw new InvalidOperationException("Missing case!");
            }

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.DoublePropertyInfoCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>
        /// Creates a volume measure property.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="valueType">The value type of the property.</param>
        /// <returns>The created property handle.</returns>
        public static IFCAnyHandle CreateVolumeMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsVolumeMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsVolumeMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a positive length measure property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePositiveLengthMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            if (value > MathUtil.Eps())
            {
                switch (valueType)
                {
                    case PropertyValueType.EnumeratedValue:
                        {
                            IList<IFCData> valueList = new List<IFCData>();
                            valueList.Add(IFCDataUtil.CreateAsPositiveLengthMeasure(value));
                            return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                        }
                    case PropertyValueType.SingleValue:
                        return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPositiveLengthMeasure(value), null);
                    default:
                        throw new InvalidOperationException("Missing case!");
                }
            }
            return null;
        }

        /// <summary>
        /// Create a positive ratio measure property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePositiveRatioMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            if (value > MathUtil.Eps())
            {
                switch (valueType)
                {
                    case PropertyValueType.EnumeratedValue:
                        {
                            IList<IFCData> valueList = new List<IFCData>();
                            valueList.Add(IFCDataUtil.CreateAsPositiveRatioMeasure(value));
                            return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                        }
                    case PropertyValueType.SingleValue:
                        return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPositiveRatioMeasure(value), null);
                    default:
                        throw new InvalidOperationException("Missing case!");
                }
            }
            return null;
        }

        /// <summary>
        /// Create a label property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsPlaneAngleMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsPlaneAngleMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a label property, or retrieve from cache.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created or cached property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasurePropertyFromCache(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            // We have a partial cache here - we will only cache multiples of 15 degrees.
            bool canCache = false;
            double degreesDiv15 = Math.Floor(value / 15.0 + 0.5);
            double integerDegrees = degreesDiv15 * 15.0;
            if (MathUtil.IsAlmostEqual(value, integerDegrees))
            {
                canCache = true;
                value = integerDegrees;
            }

            IFCAnyHandle propertyHandle;
            if (canCache)
            {
                propertyHandle = ExporterCacheManager.DoublePropertyInfoCache.Find(propertyName, value);
                if (propertyHandle != null)
                    return propertyHandle;
            }

            propertyHandle = CreatePlaneAngleMeasureProperty(file, propertyName, value, valueType);

            if (canCache && !IFCAnyHandleUtil.IsNullOrHasNoValue(propertyHandle))
            {
                ExporterCacheManager.DoublePropertyInfoCache.Add(propertyName, value, propertyHandle);
            }

            return propertyHandle;
        }

        /// <summary>
        /// Create a area measure property.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasureProperty(IFCFile file, string propertyName, double value, PropertyValueType valueType)
        {
            switch (valueType)
            {
                case PropertyValueType.EnumeratedValue:
                    {
                        IList<IFCData> valueList = new List<IFCData>();
                        valueList.Add(IFCDataUtil.CreateAsAreaMeasure(value));
                        return IFCInstanceExporter.CreatePropertyEnumeratedValue(file, propertyName, null, valueList, null);
                    }
                case PropertyValueType.SingleValue:
                    return IFCInstanceExporter.CreatePropertySingleValue(file, propertyName, null, IFCDataUtil.CreateAsAreaMeasure(value), null);
                default:
                    throw new InvalidOperationException("Missing case!");
            }
        }

        /// <summary>
        /// Create a label property from the element's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLabelPropertyFromElement(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateLabelPropertyFromCache(file, ifcPropertyName, propertyValue, valueType, false);
            }
            return null;
        }

        /// <summary>
        /// Create a label property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="revitBuiltInParam">
        /// The built in parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateLabelPropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName,
           BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            // For Instance
            IFCAnyHandle propHnd = CreateLabelPropertyFromElement(file, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateLabelPropertyFromElement(file, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateLabelPropertyFromElementOrSymbol(file, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create an identifier property from the element's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromElement(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            string propertyValue;
            if (ParameterUtil.GetStringValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateIdentifierPropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create an identifier property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="revitBuiltInParam">
        /// The built in parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIdentifierPropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName,
           BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            // For Instance
            IFCAnyHandle propHnd = CreateIdentifierPropertyFromElement(file, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateIdentifierPropertyFromElement(file, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateIdentifierPropertyFromElementOrSymbol(file, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a boolean property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateBooleanPropertyFromElementOrSymbol(IFCFile file, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            int propertyValue;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateBooleanPropertyFromCache(file, ifcPropertyName, propertyValue != 0, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateBooleanPropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create an integer property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateIntegerPropertyFromElementOrSymbol(IFCFile file, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            int propertyValue;
            if (ParameterUtil.GetIntValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateIntegerPropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateIntegerPropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a real property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="scale">
        /// The length scale.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateRealPropertyFromElementOrSymbol(IFCFile file, double scale, Element elem, string revitParameterName, string ifcPropertyName, 
            PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                return CreateRealPropertyFromCache(file, scale, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateRealPropertyFromElementOrSymbol(file, scale, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a boolean property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePositiveLengthMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue *= exporterIFC.LinearScale;
                return CreatePositiveLengthMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePositiveLengthMeasurePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a positive ratio property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePositiveRatioPropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
           string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue *= exporterIFC.LinearScale;
                return CreatePositiveRatioMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePositiveRatioPropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create a plane angle measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreatePlaneAngleMeasurePropertyFromElementOrSymbol(IFCFile file, Element elem, string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                // Although the default units for IFC files is radians, IFC files almost universally use degrees as their unit of measurement. 
                // However, many old IFC files failed to include degrees as the unit of measurement.
                // As such, we assume that the IFC file is in degrees, regardless of whether or not it is explicitly stated in the file.
                propertyValue *= 180 / Math.PI;
                return CreatePlaneAngleMeasurePropertyFromCache(file, ifcPropertyName, propertyValue, valueType);
            }
            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreatePlaneAngleMeasurePropertyFromElementOrSymbol(file, elemType, revitParameterName, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Create an area measure property from the element's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasurePropertyFromElement(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, string ifcPropertyName, PropertyValueType valueType)
        {
            double propertyValue;
            if (ParameterUtil.GetDoubleValueFromElement(elem, revitParameterName, out propertyValue))
            {
                propertyValue *= (exporterIFC.LinearScale * exporterIFC.LinearScale);
                return CreateAreaMeasureProperty(file, ifcPropertyName, propertyValue, valueType);
            }
            return null;
        }

        /// <summary>
        /// Create an area measure property from the element's or type's parameter.
        /// </summary>
        /// <param name="file">
        /// The IFC file.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="elem">
        /// The Element.
        /// </param>
        /// <param name="revitParameterName">
        /// The name of the parameter.
        /// </param>
        /// <param name="revitBuiltInParam">
        /// The built in parameter to use, if revitParameterName isn't found.
        /// </param>
        /// <param name="ifcPropertyName">
        /// The name of the property.
        /// </param>
        /// <param name="valueType">
        /// The value type of the property.
        /// </param>
        /// <returns>
        /// The created property handle.
        /// </returns>
        public static IFCAnyHandle CreateAreaMeasurePropertyFromElementOrSymbol(IFCFile file, ExporterIFC exporterIFC, Element elem,
            string revitParameterName, BuiltInParameter revitBuiltInParam, string ifcPropertyName, PropertyValueType valueType)
        {
            IFCAnyHandle propHnd = CreateAreaMeasurePropertyFromElement(file, exporterIFC, elem, revitParameterName, ifcPropertyName, valueType);
            if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                return propHnd;

            if (revitBuiltInParam != BuiltInParameter.INVALID)
            {
                string builtInParamName = LabelUtils.GetLabelFor(revitBuiltInParam);
                propHnd = CreateAreaMeasurePropertyFromElement(file, exporterIFC, elem, builtInParamName, ifcPropertyName, valueType);
                if (!IFCAnyHandleUtil.IsNullOrHasNoValue(propHnd))
                    return propHnd;
            }

            // For Symbol
            Document document = elem.Document;
            ElementId typeId = elem.GetTypeId();
            Element elemType = document.GetElement(typeId);
            if (elemType != null)
                return CreateAreaMeasurePropertyFromElementOrSymbol(file, exporterIFC, elemType, revitParameterName, revitBuiltInParam, ifcPropertyName, valueType);
            else
                return null;
        }

        /// <summary>
        /// Creates the shared beam and column QTO values.  
        /// </summary>
        /// <remarks>
        /// This code uses the native implementation for creating these quantities, and the native class for storing the information.
        /// This will be obsoleted.
        /// </remarks>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="elemHandle">The element handle.</param>
        /// <param name="element">The beam or column element.</param>
        /// <param name="typeInfo">The FamilyTypeInfo containing the appropriate data.</param>
        public static void CreateBeamColumnBaseQuantities(ExporterIFC exporterIFC, IFCAnyHandle elemHandle, Element element, FamilyTypeInfo typeInfo)
        {
            IFCTypeInfo ifcTypeInfo = new IFCTypeInfo();
            ifcTypeInfo.ScaledDepth = typeInfo.ScaledDepth;
            ifcTypeInfo.ScaledArea = typeInfo.ScaledArea;
            ifcTypeInfo.ScaledInnerPerimeter = typeInfo.ScaledInnerPerimeter;
            ifcTypeInfo.ScaledOuterPerimeter = typeInfo.ScaledOuterPerimeter;
            ExporterIFCUtils.CreateBeamColumnBaseQuantities(exporterIFC, elemHandle, element, ifcTypeInfo);
        }

        /// <summary>
        ///  Creates the shared beam, column and member QTO values.  
        /// </summary>
        /// <param name="exporterIFC">The exporter.</param>
        /// <param name="elemHandle">The element handle.</param>
        /// <param name="element">The element.</param>
        /// <param name="ecData">The IFCExtrusionCreationData containing the appropriate data.</param>
        public static void CreateBeamColumnMemberBaseQuantities(ExporterIFC exporterIFC, IFCAnyHandle elemHandle, Element element, IFCExtrusionCreationData ecData)
        {
            IFCTypeInfo ifcTypeInfo = new IFCTypeInfo();
            ifcTypeInfo.ScaledDepth = ecData.ScaledLength;
            ifcTypeInfo.ScaledArea = ecData.ScaledArea;
            ifcTypeInfo.ScaledInnerPerimeter = ecData.ScaledInnerPerimeter;
            ifcTypeInfo.ScaledOuterPerimeter = ecData.ScaledOuterPerimeter;
            ExporterIFCUtils.CreateBeamColumnBaseQuantities(exporterIFC, elemHandle, element, ifcTypeInfo);
        }

        /// <summary>
        /// Creates property sets for Revit groups and parameters, if export options is set.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC.
        /// </param>
        /// <param name="element">
        /// The Element.
        /// </param>
        /// <param name="productWrapper">
        /// The product wrapper.
        /// </param>
        public static void CreateInternalRevitPropertySets(ExporterIFC exporterIFC, Element element, IFCProductWrapper productWrapper)
        {
            if (exporterIFC == null || element == null || productWrapper == null ||
                !ExporterCacheManager.ExportOptionsCache.ExportInternalRevitPropertySets)
                return;

            IFCFile file = exporterIFC.GetFile();

            ICollection<IFCAnyHandle> elements = productWrapper.GetElements();
            if (elements.Count == 0)
            {
                elements = productWrapper.GetProducts();
                if (elements.Count == 0)
                    return;
            }

            HashSet<IFCAnyHandle> elementSets = new HashSet<IFCAnyHandle>(elements);

            double lengthScale = exporterIFC.LinearScale;
            double angleScale = 180.0 / Math.PI;

            ElementId typeId = element.GetTypeId();
            Element elementType = element.Document.GetElement(typeId);
            int whichStart = elementType != null ? 0 : (element is ElementType ? 1 : 0);
            if (whichStart == 1)
                elementType = element as ElementType;

            Dictionary<BuiltInParameterGroup, int>[] propertyArrIdxMap;
            propertyArrIdxMap = new Dictionary<BuiltInParameterGroup, int>[2]; // one for instance, one for type.
            propertyArrIdxMap[0] = new Dictionary<BuiltInParameterGroup, int>();
            propertyArrIdxMap[1] = new Dictionary<BuiltInParameterGroup, int>();
            List<HashSet<IFCAnyHandle>>[] propertyArr;
            propertyArr = new List<HashSet<IFCAnyHandle>>[2];
            propertyArr[0] = new List<HashSet<IFCAnyHandle>>();
            propertyArr[1] = new List<HashSet<IFCAnyHandle>>();
            List<string>[] propertySetNames;
            propertySetNames = new List<string>[2];
            propertySetNames[0] = new List<string>();
            propertySetNames[1] = new List<string>();

            // pass through: element and element type.  If the element is a ElementType, there will only be one pass.
            for (int which = whichStart; which < 2; which++)
            {
                Element whichElement = (which == 0) ? element : elementType;
                if (whichElement == null)
                    continue;

                bool createType = (which == 1);
                if (createType)
                {
                    if (ExporterCacheManager.TypePropertyInfoCache.HasTypeProperties(typeId))
                    {
                        ExporterCacheManager.TypePropertyInfoCache.AddTypeProperties(typeId, elementSets);
                        continue;
                    }
                }

                bool canUseElementType = ((which == 1) && elementType != null);

                ParameterSet parameters = whichElement.Parameters;
                foreach (Parameter parameter in parameters)
                {
                    Definition parameterDefinition = parameter.Definition;
                    if (parameterDefinition == null)
                        continue;

                    // IFC parameters dealt with separately.
                    // TODO: also sort out parameters passed in Common Psets.
                    BuiltInParameterGroup parameterGroup = parameterDefinition.ParameterGroup;
                    if (parameterGroup == BuiltInParameterGroup.PG_IFC)
                        continue;

                    int idx = -1;
                    if (!propertyArrIdxMap[which].TryGetValue(parameterGroup, out idx))
                    {
                        idx = propertyArr[which].Count;
                        propertyArrIdxMap[which][parameterGroup] = idx;
                        propertyArr[which].Add(new HashSet<IFCAnyHandle>());

                        string groupName = LabelUtils.GetLabelFor(parameterGroup);
                        propertySetNames[which].Add(groupName);
                    }

                    string parameterCaption = parameterDefinition.Name;
                    switch (parameter.StorageType)
                    {
                        case StorageType.None:
                            break;
                        case StorageType.Integer:
                            {
                                int value;
                                if (parameter.HasValue)
                                    value = parameter.AsInteger();
                                else
                                {
                                    if (!canUseElementType)
                                        continue;

                                    Parameter elementTypeParameter = elementType.get_Parameter(parameterDefinition);
                                    if (elementTypeParameter != null && elementTypeParameter.HasValue &&
                                        elementTypeParameter.StorageType == StorageType.Integer)
                                        value = elementTypeParameter.AsInteger();
                                    else
                                        continue;
                                }

                                // YesNo or actual integer?
                                if (parameterDefinition.ParameterType == ParameterType.YesNo)
                                {
                                    propertyArr[which][idx].Add(CreateBooleanPropertyFromCache(file, parameterCaption, value != 0, PropertyValueType.SingleValue));
                                }
                                else
                                {
                                    propertyArr[which][idx].Add(CreateIntegerPropertyFromCache(file, parameterCaption, value, PropertyValueType.SingleValue));
                                }
                                break;
                            }
                        case StorageType.Double:
                            {
                                double value = -1.0;
                                if (parameter.HasValue)
                                    value = parameter.AsDouble();
                                else
                                {
                                    if (!canUseElementType)
                                        continue;

                                    Parameter elementTypeParameter = elementType.get_Parameter(parameterDefinition);
                                    if (elementTypeParameter != null && elementTypeParameter.HasValue &&
                                        elementTypeParameter.StorageType == StorageType.Double)
                                        value = elementTypeParameter.AsDouble();
                                    else
                                        continue;
                                }

                                bool assigned = false;
                                switch (parameterDefinition.ParameterType)
                                {
                                    case ParameterType.Length:
                                        {
                                            propertyArr[which][idx].Add(CreateLengthMeasurePropertyFromCache(file, lengthScale, parameterCaption,
                                                value * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Angle:
                                        {
                                            propertyArr[which][idx].Add(CreatePlaneAngleMeasurePropertyFromCache(file, parameterCaption,
                                                value * angleScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Area:
                                        {
                                            propertyArr[which][idx].Add(CreateAreaMeasureProperty(file, parameterCaption,
                                                value * lengthScale * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                    case ParameterType.Volume:
                                        {
                                            propertyArr[which][idx].Add(CreateVolumeMeasureProperty(file, parameterCaption,
                                                value * lengthScale * lengthScale * lengthScale, PropertyValueType.SingleValue));
                                            assigned = true;
                                            break;
                                        }
                                }

                                if (!assigned)
                                {
                                    propertyArr[which][idx].Add(CreateRealPropertyFromCache(file, lengthScale, parameterCaption, value, PropertyValueType.SingleValue));
                                }
                                break;
                            }
                        case StorageType.String:
                            {
                                string value;
                                if (parameter.HasValue)
                                    value = parameter.AsString();
                                else
                                {
                                    if (!canUseElementType)
                                        continue;

                                    Parameter elementTypeParameter = elementType.get_Parameter(parameterDefinition);
                                    if (elementTypeParameter != null && elementTypeParameter.HasValue &&
                                        elementTypeParameter.StorageType == StorageType.String)
                                        value = elementTypeParameter.AsString();
                                    else
                                        continue;
                                }
                                propertyArr[which][idx].Add(CreateLabelPropertyFromCache(file, parameterCaption, value, PropertyValueType.SingleValue, false));
                                break;
                            }
                        case StorageType.ElementId:
                            {
                                ElementId value = ElementId.InvalidElementId;
                                if (parameter.HasValue)
                                    value = parameter.AsElementId();

                                if (value == ElementId.InvalidElementId)
                                {
                                    if (!canUseElementType)
                                        continue;

                                    Parameter elementTypeParameter = elementType.get_Parameter(parameterDefinition);
                                    if (elementTypeParameter != null && elementTypeParameter.HasValue &&
                                        elementTypeParameter.StorageType == StorageType.ElementId)
                                        value = elementTypeParameter.AsElementId();
                                    else
                                        continue;

                                    if (value == ElementId.InvalidElementId)
                                        continue;
                                }

                                Element paramElement = element.Document.GetElement(value);
                                string valueString;
                                if (paramElement != null && !string.IsNullOrEmpty(paramElement.Name))
                                {
                                    valueString = paramElement.Name;
                                    ElementType paramElementType = paramElement is ElementType ? paramElement as ElementType :
                                        element.Document.GetElement(paramElement.GetTypeId()) as ElementType;
                                    if (paramElementType != null && !string.IsNullOrEmpty(ExporterIFCUtils.GetFamilyName(paramElementType)))
                                        valueString = ExporterIFCUtils.GetFamilyName(paramElementType) + ": " + valueString;
                                }
                                else
                                    valueString = value.ToString();

                                propertyArr[which][idx].Add(CreateLabelPropertyFromCache(file, parameterCaption, valueString, PropertyValueType.SingleValue, true));
                                break;
                            }

                    }
                }
            }

            for (int which = whichStart; which < 2; which++)
            {
                HashSet<IFCAnyHandle> propertySets = new HashSet<IFCAnyHandle>();

                int size = propertyArr[which].Count;
                if (size == 0)
                    continue;

                for (int ii = 0; ii < size; ii++)
                {
                    if (propertyArr[which][ii].Count == 0)
                        continue;

                    IFCAnyHandle propertySet = IFCInstanceExporter.CreatePropertySet(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        propertySetNames[which][ii], null, propertyArr[which][ii]);

                    if (which == 1)
                        propertySets.Add(propertySet);
                    else
                        IFCInstanceExporter.CreateRelDefinesByProperties(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                            null, null, elementSets, propertySet);
                }

                if (which == 1)
                    ExporterCacheManager.TypePropertyInfoCache.AddNewTypeProperties(typeId, propertySets, elementSets);
            }
        }
    }
}
