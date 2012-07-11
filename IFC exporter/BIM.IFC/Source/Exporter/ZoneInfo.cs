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

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// The class contains information for creating IFC zone.
    /// </summary>
    public class ZoneInfo
    {
        /// <summary>
        /// The object type of this zone.
        /// </summary>
        private string m_ObjectType = String.Empty;

        /// <summary>
        /// The description.
        /// </summary>
        private string m_Description = String.Empty;

        /// <summary>
        /// The associated room handles.
        /// </summary>
        private HashSet<IFCAnyHandle> m_AssocRoomHandles = new HashSet<IFCAnyHandle>();

        /// <summary>
        /// The associated IfcClassificationReference handles.
        /// </summary>
        private Dictionary<string, IFCAnyHandle> m_ClassificationReferences = new Dictionary<string, IFCAnyHandle>();

        /// <summary>
        /// The associated ePSet_SpatialZoneEnergyAnalysis handle, if any.
        /// </summary>
        private IFCAnyHandle m_EnergyAnalysisProperySetHandle = null;

        /// <summary>
        /// Constructs a ZoneInfo object.
        /// </summary>
        /// <param name="objectType">The type of zone.</param>
        /// <param name="description">The description.</param>
        /// <param name="roomHandle">The room handle for this zone.</param>
        /// <param name="classificationReferences">The room handle for this zone.</param>
        /// <param name="energyAnalysisHnd">The ePSet_SpatialZoneEnergyAnalysis handle for this zone.</param>
        public ZoneInfo(string objectType, string description, IFCAnyHandle roomHandle, 
            Dictionary<string, IFCAnyHandle> classificationReferences, IFCAnyHandle energyAnalysisHnd)
        {
            ObjectType = objectType;
            Description = description;
            RoomHandles.Add(roomHandle);
            ClassificationReferences = classificationReferences;
            EnergyAnalysisProperySetHandle = energyAnalysisHnd;
        }

        /// <summary>
        /// The object type of this zone.
        /// </summary>
        public string ObjectType
        {
            get { return m_ObjectType; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    m_ObjectType = value;
            }
        }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description
        {
            get { return m_Description; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    m_Description = value;
            }
        }

        /// <summary>
        /// The associated room handles.
        /// </summary>
        public HashSet<IFCAnyHandle> RoomHandles
        {
            get { return m_AssocRoomHandles; }
        }

        /// <summary>
        /// The associated IfcClassificationReference handles.
        /// </summary>
        public Dictionary<string, IFCAnyHandle> ClassificationReferences
        {
            get { return m_ClassificationReferences; }
            set { m_ClassificationReferences = value; }
        }

        /// <summary>
        /// The associated ePSet_SpatialZoneEnergyAnalysis handle.
        /// </summary>
        public IFCAnyHandle EnergyAnalysisProperySetHandle
        {
            get { return m_EnergyAnalysisProperySetHandle; }
            set { m_EnergyAnalysisProperySetHandle = value; }
        }
    }
}
