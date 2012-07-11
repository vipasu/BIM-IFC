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

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// The class contains output information from ExportBody
    /// </summary>
    public class BodyData
    {
        /// <summary>
        /// The representation handle.
        /// </summary>
        private IFCAnyHandle m_RepresentationHnd = null;

        /// <summary>
        /// The offset transform.
        /// </summary>
        private Transform m_BrepOffsetTransform = Transform.Identity;

        /// <summary>
        /// The exported material Ids
        /// </summary>
        private HashSet<ElementId> m_MaterialIds = new HashSet<ElementId>();

        /// <summary>
        /// Constructs a default BodyData object.
        /// </summary>
        public BodyData() { }

        /// <summary>
        /// Constructs a BodyData object.
        /// </summary>
        /// <param name="representationHnd">
        /// The representation handle.
        /// </param>
        /// <param name="brepOffsetTransform">
        /// The offset transform.
        /// </param>
        /// <param name="materialIds">
        /// The material ids.
        /// </param>
        public BodyData(IFCAnyHandle representationHnd, Transform brepOffsetTransform, HashSet<ElementId> materialIds)
        {
            this.m_RepresentationHnd = representationHnd;
            if (brepOffsetTransform != null)
                this.m_BrepOffsetTransform = brepOffsetTransform;
            if (materialIds != null)
                this.m_MaterialIds = materialIds;
        }

        /// <summary>
        /// Copies a BodyData object.
        /// </summary>
        /// <param name="representationHnd">
        /// The representation handle.
        /// </param>
        /// <param name="brepOffsetTransform">
        /// The offset transform.
        /// </param>
        /// <param name="materialIds">
        /// The material ids.
        /// </param>
        public BodyData(BodyData bodyData)
        {
            this.m_RepresentationHnd = bodyData.RepresentationHnd;
            this.m_BrepOffsetTransform = bodyData.BrepOffsetTransform;
            this.m_MaterialIds = bodyData.MaterialIds;
        }
        
        /// <summary>
        /// The representation handle.
        /// </summary>
        public IFCAnyHandle RepresentationHnd
        {
            get { return m_RepresentationHnd; }
            set { m_RepresentationHnd = value; }
        }

        /// <summary>
        /// The offset transform.
        /// </summary>
        public Transform BrepOffsetTransform
        {
            get { return m_BrepOffsetTransform; }
            set { m_BrepOffsetTransform = value; }
        }

        /// <summary>
        /// The associated material ids.
        /// </summary>
        public HashSet<ElementId> MaterialIds
        {
            get { return m_MaterialIds; }
            set { m_MaterialIds = value; }
        }

        /// <summary>
        /// Add a material id to the set of material ids.
        /// </summary>
        /// <param name="matId">The new material</param>
        public void AddMaterial(ElementId matId)
        {
            MaterialIds.Add(matId);
        }
    }
}
