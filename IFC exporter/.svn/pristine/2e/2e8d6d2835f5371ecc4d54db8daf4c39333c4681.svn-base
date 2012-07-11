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

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// A description mapping of a group of Revit parameters and/or calculated values to an IfcPropertySet.
    /// </summary>
    /// <remarks>
    /// The mapping includes: the name of the IFC property set, the entity type this property to which this set applies,
    /// and an array of property set entries.  A property set description is valid for only one entity type.
    /// </remarks>
    class PropertySetDescription : Description
    {
        /// <summary>
        /// The entries stored in this property set description.
        /// </summary>
        IList<PropertySetEntry> m_Entries = new List<PropertySetEntry>();

        /// <summary>
        /// The entries stored in this property set description.
        /// </summary>
        public IList<PropertySetEntry> Entries
        {
            get
            {
                return m_Entries;
            }
        }
    }
}
