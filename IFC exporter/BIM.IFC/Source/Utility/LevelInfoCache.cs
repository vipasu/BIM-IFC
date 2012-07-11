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
    /// Used to keep a cache of the heights of levels.
    /// </summary>
    class LevelInfoCache
    {
        /// <summary>
        /// The dictionary mapping from an ElementId to a level height.
        /// </summary>
        private Dictionary<ElementId, KeyValuePair<ElementId, double>> elementIdToLevelHeight = new Dictionary<ElementId, KeyValuePair<ElementId, double>>();

        /// <summary>
        /// A list of levels, sorted by elevation.  The user is expected to create the list in the proper order; this is done in Exporter.cs.
        /// </summary>
        List<ElementId> m_LevelsByElevation;

        /// <summary>
        /// Finds the height of the level from the dictionary.
        /// </summary>
        /// <param name="elementId">
        /// The level element elementId.
        /// </param>
        /// <returns>
        /// The height.  Returns -1.0 if there is no entry in the cache, since valid entries must always be non-negative.
        /// </returns>
        public double FindHeight(ElementId elementId)
        {
            KeyValuePair<ElementId, double> info;
            if (elementIdToLevelHeight.TryGetValue(elementId, out info))
            {
                return info.Value;
            }
            return -1.0;
        }

        /// <summary>
        /// Finds the next level id, if any, of the level from the dictionary.
        /// </summary>
        /// <param name="elementId">
        /// The level element elementId.
        /// </param>
        /// <returns>
        /// The next level Id.  Returns InvalidElementId if there is no entry in the cache.
        /// </returns>
        public ElementId FindNextLevel(ElementId elementId)
        {
            KeyValuePair<ElementId, double> info;
            if (elementIdToLevelHeight.TryGetValue(elementId, out info))
            {
                return info.Key;
            }
            return ElementId.InvalidElementId;
        }
        
        /// <summary>
        /// Adds the height and next level id (if valid) to the dictionary.
        /// </summary>
        /// <param name="elementId">
        /// The level element elementId.
        /// </param>
        /// <param name="nextLevelId">
        /// The next level ElementId.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        public void Register(ElementId elementId, ElementId nextLevelId, double height)
        {
            if (elementIdToLevelHeight.ContainsKey(elementId))
                return;

            elementIdToLevelHeight[elementId] = new KeyValuePair<ElementId, double>(nextLevelId, height);
        }

        /// <summary>
        /// A list of levels, sorted by elevation.  The user is expected to create the list in the proper order; this is done in Exporter.cs.
        /// </summary>
        public List<ElementId> LevelsByElevation
        {
            get
            {
                if (m_LevelsByElevation == null)
                    m_LevelsByElevation = new List<ElementId>();
                return m_LevelsByElevation;
            }
            set { m_LevelsByElevation = value; }
        }

        /// <summary>
        /// Adds an IFCLevelInfo to the LevelsByElevation list, also updating the native cache item.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter data object.
        /// </param>
        /// <param name="levelId">
        /// The level ElementId.
        /// </param>
        /// <param name="info">
        /// The IFCLevelInfo.
        /// </param>
        public void AddLevelInfo(ExporterIFC exporterIFC, ElementId levelId, IFCLevelInfo info)
        {
            LevelsByElevation.Add(levelId);
            exporterIFC.AddBuildingStorey(levelId, info);
        }


        /// <summary>
        /// Get the IFCLevelInfo corresponding to a level.
        /// </summary>
        /// <param name="exporterIFC">
        /// The exporter data object.
        /// </param>
        /// <param name="levelId">
        /// The level ElementId.
        /// </param>
        /// <returns>
        /// The IFCLevelInfo.
        /// </returns>
        public IFCLevelInfo GetLevelInfo(ExporterIFC exporterIFC, ElementId levelId)
        {
            return exporterIFC.GetLevelInfos()[levelId];
        }
    }
}
