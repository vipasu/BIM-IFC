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
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;

namespace BIM.IFC.Toolkit
{
    class IFCAnyHandleUtil
    {
        /// <summary>
        /// Validates if the handle is one of the desired entity type.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="nullAllowed">True if allow handle to be null, false if not.</param>
        /// <param name="types">The entity types.</param>
        public static void ValidateSubTypeOf(IFCAnyHandle handle, bool nullAllowed, params IFCEntityType[] types)
        {
            if (handle == null)
            {
                if (!nullAllowed)
                    throw new ArgumentNullException("handle");

                return;
            }
            else
            {
                for (int ii = 0; ii < types.Length; ii++)
                {
                    if (IsSubTypeOf(handle, types[ii]))
                        return;
                }
            }
            throw new ArgumentException("Invalid handle.", "handle");
        }
        
        /// <summary>
        /// Validates if all the handles in the collection are the instances of the desired entity type.
        /// </summary>
        /// <param name="handles">The handles.</param>
        /// <param name="nullAllowed">True if allow handles to be null, false if not.</param>
        /// <param name="types">The entity types.</param>
        public static void ValidateSubTypeOf(ICollection<IFCAnyHandle> handles, bool nullAllowed, params IFCEntityType[] types)
        {
            if (handles == null)
            {
                if (!nullAllowed)
                    throw new ArgumentNullException("handles");

                return;
            }
            else
            {
                foreach (IFCAnyHandle handle in handles)
                {
                    bool foundIsSubType = false;
                    for (int ii = 0; ii < types.Length; ii++)
                    {
                        if (IsSubTypeOf(handle, types[ii]))
                            foundIsSubType = true;
                    }
                    if (!foundIsSubType)
                        throw new ArgumentException("Contains invalid handle.", "handles");
                }
            }
        }

        /// <summary>
        /// Sets string attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, string value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, IFCData.CreateString(value));
        }

        /// <summary>
        /// Sets enumeration attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null or empty, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The enumeration value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, Enum value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, IFCData.CreateEnumeration(value.ToString()));
        }

        /// <summary>
        /// Sets logical attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null or empty, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The logical value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, IFCLogical value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            handle.SetAttribute(name, IFCData.CreateLogical(value));
        }

        /// <summary>
        /// Sets logical attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null or empty, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The logical value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, IFCLogical? value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, IFCData.CreateLogical((IFCLogical)value));
        }

        /// <summary>
        /// Sets instance attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, IFCAnyHandle value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (!IsNullOrHasNoValue(value))
                handle.SetAttribute(name, value);
        }

        /// <summary>
        /// Sets double attribute for the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, double value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            handle.SetAttribute(name, value);
        }

        /// <summary>
        /// Sets double attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, double? value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, (double)value);
        }

        /// <summary>
        /// Sets boolean attribute for the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The boolean value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, bool value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            handle.SetAttribute(name, value);
        }

        /// <summary>
        /// Sets boolean attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The boolean value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, bool? value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, (bool)value);
        }

        /// <summary>
        /// Sets integer attribute for the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The intereger value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, int value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            handle.SetAttribute(name, value);
        }

        /// <summary>
        /// Sets integer attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The intereger value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, int? value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
                handle.SetAttribute(name, (int)value);
        }

        /// <summary>
        /// Sets instance aggregate attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If values collection is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        /// <exception cref="ArgumentException">If the collection contains null object.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, ICollection<IFCAnyHandle> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (values != null)
            {
                if (values.Contains(null))
                    throw new ArgumentException("The collection contains null values.", "values");

                handle.SetAttribute(name, values);
            }
        }

        /// <summary>
        /// Sets integer aggregate attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If values collection is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, ICollection<int> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (values != null)
            {
                handle.SetAttribute(name, values);
            }
        }
        
        /// <summary>
        /// Sets double aggregate attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If values collection is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, ICollection<double> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (values != null)
            {
                handle.SetAttribute(name, values);
            }
        }

        /// <summary>
        /// Sets string aggregate attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If values collection is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, ICollection<string> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (values != null)
            {
                handle.SetAttribute(name, values);
            }
        }

        /// <summary>
        /// Sets IFCValue aggregate attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If values collection is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, ICollection<IFCData> values)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (values != null)
            {
                if (values.Contains(null))
                    throw new ArgumentException("The collection contains null values.", "values");

                IFCAggregate aggregateAttribute = handle.CreateAggregateAttribute(name);
                if (aggregateAttribute != null)
                {
                    foreach (IFCData value in values)
                    {
                        aggregateAttribute.Add(value);
                    }
                }
            }
        }

        /// <summary>
        /// Sets IFCValue attribute for the handle.
        /// </summary>
        /// <remarks>
        /// If value is null, the attribute will be unset.
        /// </remarks>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">If the name is null or empty.</exception>
        public static void SetAttribute(IFCAnyHandle handle, string name, IFCData value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The name is empty.", "name");

            if (value != null)
            {
                handle.SetAttribute(name, value);
            }
        }

        /// <summary>
        /// Gets aggregate attribute values from a handle.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="handle">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The collection of attribute values.</returns>
        public static T GetAggregateInstanceAttribute<T>(IFCAnyHandle handle, string name) where T : ICollection<IFCAnyHandle>, new()
        {
            if (handle == null)
                throw new ArgumentNullException("handle");

            if (!handle.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = handle.GetAttribute(name);

            T aggregateAttribute = default(T);

            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null)
                {
                    aggregateAttribute = new T();
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                        {
                            aggregateAttribute.Add(val.AsInstance());
                        }
                    }
                }
            }
            return aggregateAttribute;
        }

        /// <summary>
        /// Gets the IFCEntityType of a handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>The IFCEntityType.</returns>
        public static IFCEntityType GetEntityType(IFCAnyHandle handle)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");

            if (!handle.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCEntityType compareType;
            if (ExporterCacheManager.HandleTypeCache.TryGetValue(handle, out compareType))
                return compareType;
            
            IFCEntityType entityType = (IFCEntityType)Enum.Parse(typeof(IFCEntityType), handle.TypeName, true);
            ExporterCacheManager.HandleTypeCache[handle] = entityType;

            return entityType;
        }

        /// <summary>
        /// Gets the object type of a handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>The object type.</returns>
        public static string GetObjectType(IFCAnyHandle handle)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");

            if (!handle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(handle, IFCEntityType.IfcObject))
                throw new ArgumentException("Not an IfcObject handle.");

            IFCData ifcData = handle.GetAttribute("ObjectType");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
                return ifcData.AsString();

            throw new InvalidOperationException("Failed to get object type.");
        }

        /// <summary>
        /// Gets the coordinates of an IfcCartesianPoint.
        /// </summary>
        /// <param name="axisPlacement">The IfcCartesianPoint.</param>
        /// <returns>The list of coordinates.</returns>
        public static IList<double> GetCoordinates(IFCAnyHandle cartesianPoint)
        {
            IList<double> coordinates = null;

            if (cartesianPoint == null)
                throw new ArgumentNullException("cartesianPoint");

            if (!cartesianPoint.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(cartesianPoint, IFCEntityType.IfcCartesianPoint))
                throw new ArgumentException("Not an IfcCartesianPoint handle.");

            IFCData ifcData = cartesianPoint.GetAttribute("Coordinates");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                {
                    coordinates = new List<double>();
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Double)
                        {
                            coordinates.Add(val.AsDouble());
                        }
                    }
                }
            }

            return coordinates;
        }

        /// <summary>
        /// Gets an arbitrary instance attribute.
        /// </summary>
        /// <param name="name">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The handle to the attribute.</returns>
        public static IFCAnyHandle GetInstanceAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Instance)
                return ifcData.AsInstance();

            return null;
        }

        /// <summary>
        /// Gets an arbitrary string attribute.
        /// </summary>
        /// <param name="name">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The string.</returns>
        public static string GetStringAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
                return ifcData.AsString();

            return null;
        }

        /// <summary>
        /// Gets an arbitrary integer attribute.
        /// </summary>
        /// <param name="name">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The integer.</returns>
        public static int? GetIntAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Integer)
                return ifcData.AsInteger();

            return null;
        }

        /// <summary>
        /// Gets an arbitrary double attribute.
        /// </summary>
        /// <param name="hnd">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The double.</returns>
        public static double? GetDoubleAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Double)
                return ifcData.AsDouble();

            return null;
        }

        /// <summary>
        /// Gets a boolean attribute.
        /// </summary>
        /// <param name="hnd">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The boolean value.</returns>
        public static bool? GetBooleanAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Double)
                return ifcData.AsBoolean();

            return null;
        }

        /// <summary>
        /// Gets an arbitrary enumeration attribute.
        /// </summary>
        /// <remarks>
        /// This function returns the string value of the enumeration.  It must be then manually
        /// converted to the appropriate enum value by the called.
        /// </remarks>
        /// <param name="name">The handle.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The string.</returns>
        public static string GetEnumerationAttribute(IFCAnyHandle hnd, string name)
        {
            if (hnd == null)
                throw new ArgumentNullException("hnd");

            if (!hnd.HasValue)
                throw new ArgumentException("Invalid handle.");

            IFCData ifcData = hnd.GetAttribute(name);
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Enumeration)
                return ifcData.AsString();

            return null;
        }
        
        /// <summary>
        /// Gets the location of an IfcPlacement.
        /// </summary>
        /// <param name="axisPlacement">The IfcPlacement.</param>
        /// <returns>The IfcCartesianPoint.</returns>
        public static IFCAnyHandle GetLocation(IFCAnyHandle axisPlacement)
        {
            if (!IsSubTypeOf(axisPlacement, IFCEntityType.IfcPlacement))
                throw new ArgumentException("Not an IfcPlacement handle.");

            return GetInstanceAttribute(axisPlacement, "Location");
        }

        /// <summary>
        /// Checks if an object handle has IfcRelDecomposes.
        /// </summary>
        /// <param name="objectHandle">The object handle.</param>
        /// <returns>True if it has, false if not.</returns>
        public static bool HasRelDecomposes(IFCAnyHandle objectHandle)
        {
            if (objectHandle == null)
                throw new ArgumentNullException("objectHandle");

            if (!objectHandle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(objectHandle, IFCEntityType.IfcObject) &&
                !IsSubTypeOf(objectHandle, IFCEntityType.IfcTypeObject))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = objectHandle.GetAttribute("Decomposes");

            if (!ifcData.HasValue)
                return false;
            else if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                    return true;
                else
                    return false;
            }

            throw new InvalidOperationException("Failed to get decomposes.");
        }

        /// <summary>
        /// Gets IfcRelDecomposes of an object handle.
        /// </summary>
        /// <param name="objectHandle">The object handle.</param>
        /// <returns>The collection of IfcRelDecomposes.</returns>
        public static HashSet<IFCAnyHandle> GetRelDecomposes(IFCAnyHandle objectHandle)
        {
            if (objectHandle == null)
                throw new ArgumentNullException("objectHandle");

            if (!objectHandle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(objectHandle, IFCEntityType.IfcObject) &&
                !IsSubTypeOf(objectHandle, IFCEntityType.IfcTypeObject))
                throw new ArgumentException("The operation is not valid for this handle.");

            HashSet<IFCAnyHandle> decomposes = new HashSet<IFCAnyHandle>();
            IFCData ifcData = objectHandle.GetAttribute("IsDecomposedBy");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                {
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                        {
                            decomposes.Add(val.AsInstance());
                        }
                    }
                }
            }
            return decomposes;
        }

        /// <summary>
        /// Gets IfcMaterialDefinitionRepresentation inverse set of an IfcMaterial handle.
        /// </summary>
        /// <param name="objectHandle">The IfcMaterial handle.</param>
        /// <returns>The collection of IfcMaterialDefinitionRepresentation.</returns>
        public static HashSet<IFCAnyHandle> GetHasRepresentation(IFCAnyHandle objectHandle)
        {
            if (objectHandle == null)
                throw new ArgumentNullException("objectHandle");

            if (!objectHandle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(objectHandle, IFCEntityType.IfcMaterial))
                throw new ArgumentException("The operation is not valid for this handle.");

            HashSet<IFCAnyHandle> hasRepresentation = new HashSet<IFCAnyHandle>();
            IFCData ifcData = objectHandle.GetAttribute("HasRepresentation");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                {
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                        {
                            hasRepresentation.Add(val.AsInstance());
                        }
                    }
                }
            }
            return hasRepresentation;
        }

        /// <summary>
        /// Gets representation of a product handle.
        /// </summary>
        /// <param name="productHandle">The product handle.</param>
        /// <returns>The representation handle.</returns>
        public static IFCAnyHandle GetRepresentation(IFCAnyHandle productHandle)
        {
            if (productHandle == null)
                throw new ArgumentNullException("productHandle");

            if (!productHandle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(productHandle, IFCEntityType.IfcProduct))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = productHandle.GetAttribute("Representation");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Instance)
            {
                return ifcData.AsInstance();
            }

            return null;
        }

        /// <summary>
        /// Gets ContextOfItems of a representation handle.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The ContextOfItems handle.</returns>
        public static IFCAnyHandle GetContextOfItems(IFCAnyHandle representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");

            if (!representation.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(representation, IFCEntityType.IfcRepresentation))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = representation.GetAttribute("ContextOfItems");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Instance)
            {
                return ifcData.AsInstance();
            }

            return null;
        }

        /// <summary>
        /// Gets Identifier of a representation handle.
        /// </summary>
        /// <param name="representation">The representation item.</param>
        /// <returns>The RepresentationIdentifier string.</returns>
        public static string GetRepresentationIdentifier(IFCAnyHandle representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");

            if (!representation.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(representation, IFCEntityType.IfcRepresentation))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = representation.GetAttribute("RepresentationIdentifier");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
            {
                return ifcData.AsString();
            }

            return null;
        }

        /// <summary>
        /// Gets RepresentationType of a representation handle.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The RepresentationType string.</returns>
        public static string GetRepresentationType(IFCAnyHandle representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");

            if (!representation.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(representation, IFCEntityType.IfcRepresentation))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = representation.GetAttribute("RepresentationType");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
            {
                return ifcData.AsString();
            }

            return null;
        }

        /// <summary>
        /// Gets set of Items of a representation handle.
        /// </summary>
        /// <param name="representation">The representation handle.</param>
        /// <returns>The set of items.</returns>
        public static HashSet<IFCAnyHandle> GetItems(IFCAnyHandle representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");

            if (!representation.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(representation, IFCEntityType.IfcRepresentation))
                throw new ArgumentException("The operation is not valid for this handle.");

            HashSet<IFCAnyHandle> items = new HashSet<IFCAnyHandle>();
            IFCData ifcData = representation.GetAttribute("Items");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                {
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                        {
                            items.Add(val.AsInstance());
                        }
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// Gets representations of a representation handle.
        /// </summary>
        /// <param name="representation">The representation handle.</param>
        /// <returns>The list of representations.</returns>
        public static List<IFCAnyHandle> GetRepresentations(IFCAnyHandle representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");

            if (!representation.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(representation, IFCEntityType.IfcProductRepresentation))
                throw new ArgumentException("The operation is not valid for this handle.");

            List<IFCAnyHandle> representations = new List<IFCAnyHandle>();
            IFCData ifcData = representation.GetAttribute("Representations");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.Aggregate)
            {
                IFCAggregate aggregate = ifcData.AsAggregate();
                if (aggregate != null && aggregate.Count > 0)
                {
                    foreach (IFCData val in aggregate)
                    {
                        if (val.PrimitiveType == IFCDataPrimitiveType.Instance)
                        {
                            representations.Add(val.AsInstance());
                        }
                    }
                }
            }
            return representations;
        }

        /// <summary>
        /// Gets Name of an IfcProductDefinitionShape handle.
        /// </summary>
        /// <param name="representation">The IfcProductDefinitionShape.</param>
        /// <returns>The Name string.</returns>
        public static string GetProductDefinitionShapeName(IFCAnyHandle productDefinitionShape)
        {
            if (productDefinitionShape == null)
                throw new ArgumentNullException("productDefinitionShape");

            if (!productDefinitionShape.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(productDefinitionShape, IFCEntityType.IfcProductDefinitionShape))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = productDefinitionShape.GetAttribute("Name");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
            {
                return ifcData.AsString();
            }

            return null;
        }

        /// <summary>
        /// Gets Description of an IfcProductDefinitionShape handle.
        /// </summary>
        /// <param name="representation">The IfcProductDefinitionShape.</param>
        /// <returns>The Description string.</returns>
        public static string GetProductDefinitionShapeDescription(IFCAnyHandle productDefinitionShape)
        {
            if (productDefinitionShape == null)
                throw new ArgumentNullException("productDefinitionShape");

            if (!productDefinitionShape.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(productDefinitionShape, IFCEntityType.IfcProductDefinitionShape))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCData ifcData = productDefinitionShape.GetAttribute("Description");
            if (ifcData.PrimitiveType == IFCDataPrimitiveType.String)
            {
                return ifcData.AsString();
            }

            return null;
        }
        
        /// <summary>
        /// Gets representations of a product handle.
        /// </summary>
        /// <param name="productHandle">The product handle.</param>
        /// <returns>The list of representations.</returns>
        public static List<IFCAnyHandle> GetProductRepresentations(IFCAnyHandle productHandle)
        {
            if (productHandle == null)
                throw new ArgumentNullException("productHandle");

            if (!productHandle.HasValue)
                throw new ArgumentException("Invalid handle.");

            if (!IsSubTypeOf(productHandle, IFCEntityType.IfcProduct))
                throw new ArgumentException("The operation is not valid for this handle.");

            IFCAnyHandle representation = GetRepresentation(productHandle);
            if (!IsNullOrHasNoValue(representation))
            {
                return GetRepresentations(representation);
            }

            return new List<IFCAnyHandle>();
        }

        /// <summary>
        /// Checks if the handle is null or has no value.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>True if it is null or has no value, false otherwise.</returns>
        public static bool IsNullOrHasNoValue(IFCAnyHandle handle)
        {
            return handle == null || !handle.HasValue;
        }

        /// <summary>
        /// Checks if the handle is of a particular type.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="type">The type.</param>
        /// <returns>True if the handle is exactly of the specified type.</returns>
        public static bool IsTypeOf(IFCAnyHandle handle, IFCEntityType type)
        {
            if (IsNullOrHasNoValue(handle))
                return false;
            IFCEntityType compareType;
            if (ExporterCacheManager.HandleTypeCache.TryGetValue(handle, out compareType))
                return (compareType == type);
            return handle.IsTypeOf(type.ToString());
        }

        /// <summary>
        /// Checks if the handle is a sub-type of a particular type.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="type">The type.</param>
        /// <returns>True if the handle is exactly of the specified type.</returns>
        public static bool IsSubTypeOf(IFCAnyHandle handle, IFCEntityType type)
        {
            if (IsNullOrHasNoValue(handle))
                return false;
            IFCEntityType subType;
            bool val;

            if (ExporterCacheManager.HandleTypeCache.TryGetValue(handle, out subType))
            {
                if (subType == type)
                    return true;
                KeyValuePair<IFCEntityType, IFCEntityType> subTypeToTypeEntry = new KeyValuePair<IFCEntityType, IFCEntityType>(subType, type);
                if (ExporterCacheManager.HandleIsSubTypeOfCache.TryGetValue(subTypeToTypeEntry, out val))
                    return val;
                val = handle.IsSubTypeOf(type.ToString());
                ExporterCacheManager.HandleIsSubTypeOfCache[subTypeToTypeEntry] = val;
            }
            else
                val = handle.IsSubTypeOf(type.ToString());

            return val;
        }

        /// <summary>
        /// Updates the project information.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="projectName">The project name.</param>
        /// <param name="projectLongName">The project long name.</param>
        /// <param name="projectStatus">The project status.</param>
        public static void UpdateProject(IFCAnyHandle project, string projectName, string projectLongName,
            string projectStatus)
        {
            if (!IsNullOrHasNoValue(project) && project.IsSubTypeOf(IFCEntityType.IfcProject.ToString()))
            {
                SetAttribute(project, "Name", projectName);
                SetAttribute(project, "LongName", projectLongName);
                SetAttribute(project, "Phase", projectStatus);
            }
        }
    }
}
