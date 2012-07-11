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

namespace BIM.IFC.Utility
{
    /// <summary>
    /// Used to keep a cache of a mapping of an ElementId to a handle.
    /// </summary>
    class ElementToHandleCache
    {
        /// <summary>
        /// The dictionary mapping from an ElementId to an  handle. 
        /// </summary>
        private Dictionary<ElementId, IFCAnyHandle> elementIdToHandleDictionary = new Dictionary<ElementId, IFCAnyHandle>();

        /// <summary>
        /// Finds the handle from the dictionary.
        /// </summary>
        /// <param name="elementId">
        /// The element elementId.
        /// </param>
        /// <returns>
        /// The handle.
        /// </returns>
        public IFCAnyHandle Find(ElementId elementId)
        {
            IFCAnyHandle handle;
            if (elementIdToHandleDictionary.TryGetValue(elementId, out handle))
            {
                return handle;
            }
            return null;
        }

        /// <summary>
        /// Adds the handle to the dictionary.
        /// </summary>
        /// <param name="elementId">
        /// The element elementId.
        /// </param>
        /// <param name="handle">
        /// The handle.
        /// </param>
        public void Register(ElementId elementId, IFCAnyHandle handle)
        {
            if (elementIdToHandleDictionary.ContainsKey(elementId))
                return;

            elementIdToHandleDictionary[elementId] = handle;
        }
    }
}
