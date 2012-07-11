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
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// A description mapping of a group of Revit parameters and/or calculated values to an IfcPropertySet or IfcElementQuantity.
    /// </summary>
    /// <remarks>
    /// A property or quantity set mapping is valid for only one entity type.
    /// </remarks>
    abstract class Description
    {
        /// <summary>
        /// The name to be used to create property set or quantity.
        /// </summary>
        string m_Name = String.Empty;

        /// <summary>
        /// The types of element appropriate for this property or quantity set.
        /// </summary>
        List<IFCEntityType> m_IFCEntityTypes = new List<IFCEntityType>();

        /// <summary>
        /// The object type of element appropriate for this property or quantity set.
        /// </summary>
        string m_ObjectType = String.Empty;

        /// <summary>
        /// The index used to create a consistent GUID for this item.
        /// It is expected that this index will come from the list in IFCSubElementEnums.cs.
        /// </summary>
        int m_SubElementIndex = -1;

        /// <summary>
        /// The redirect calculator associated with this property or quantity set.
        /// </summary>
        DescriptionCalculator m_DescriptionCalculator;

        /// <summary>
        /// Identifies if the input handle is sub type of one IFCEntityType in the EntityTypes list.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>True if it is sub type, false otherwise.</returns>
        public bool IsSubTypeOfEntityTypes(IFCAnyHandle handle)
        {
            foreach (IFCEntityType entityType in EntityTypes)
            {
                if (IFCAnyHandleUtil.IsSubTypeOf(handle, entityType))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Identifies if the input handle matches the type of element, and optionally the object type, 
        /// to which this description applies.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        /// <returns>
        /// True if it matches, false otherwise.
        /// </returns>
        public bool IsAppropriateType(IFCAnyHandle handle)
        {
            if (handle == null || !IsSubTypeOfEntityTypes(handle))
                return false;
            if (ObjectType == "")
                return true;

            string objectType = IFCAnyHandleUtil.GetObjectType(handle);
            return (NamingUtil.IsEqualIgnoringCaseAndSpaces(ObjectType, objectType));
        }

        /// <summary>
        /// Identifies if the input handle matches the type of element only to which this description applies.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        /// <returns>
        /// True if it matches, false otherwise.
        /// </returns>
        public bool IsAppropriateEntityType(IFCAnyHandle handle)
        {
            if (handle == null || !IsSubTypeOfEntityTypes(handle))
                return false;
            return true;         
        }

        /// <summary>
        /// Identifies if the input handle matches the object type only to which this description applies.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        /// <returns>
        /// True if it matches, false otherwise.
        /// </returns>
        public bool IsAppropriateObjectType(IFCAnyHandle handle)
        {
            if (handle == null)
                return false;
            if (ObjectType == "")
                return true;

            string objectType = IFCAnyHandleUtil.GetObjectType(handle);
            return (NamingUtil.IsEqualIgnoringCaseAndSpaces(ObjectType, objectType));
        }
        
        /// <summary>
        /// The name of the property or quantity set.
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        /// <summary>
        /// The type of element appropriate for this property or quantity set.
        /// </summary>
        public List<IFCEntityType> EntityTypes
        {
            get
            {
                return m_IFCEntityTypes;
            }
        }

        /// <summary>
        /// The object type of element appropriate for this property or quantity set.
        /// Primarily used for identifying proxies.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return m_ObjectType;
            }
            set
            {
                m_ObjectType = value;
            }
        }

        /// <summary>
        /// The index used to create a consistent GUID for this item.
        /// It is expected that this index will come from the list in IFCSubElementEnums.cs.
        /// </summary>
        public int SubElementIndex
        {
            get { return m_SubElementIndex; }
            set { m_SubElementIndex = value; }
        }
        
        /// <summary>
        /// The redirect calculator associated with this property or quantity set.
        /// </summary>
        public DescriptionCalculator DescriptionCalculator
        {
            get
            {
                return m_DescriptionCalculator;
            }
            set
            {
                m_DescriptionCalculator = value;
            }
        }
    }
}
