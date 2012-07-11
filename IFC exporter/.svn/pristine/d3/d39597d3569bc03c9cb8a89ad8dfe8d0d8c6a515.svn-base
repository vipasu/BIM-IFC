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
    /// The class contains input parameters to ExportBody
    /// </summary>
    public class BodyExporterOptions
    {
        public enum BodyTessellationLevel
        {
            Default,
            Coarse
        }

        /// <summary>
        /// Try to export the solids as extrusions, if possible.
        /// </summary>
        private bool m_TryToExportAsExtrusion = false;

        /// <summary>
        /// If the body contains geometries that are identical except position and orientation, use mapped items to reuse the geometry.
        /// NOTE: This functionality is untested, and should be used with caution.
        /// </summary>
        private bool m_UseMappedGeometriesIfPossible = false;

        /// <summary>
        /// If the element is part of a group, and has unmodified geoemtry, use mapped items to share the geometry between groups.
        /// NOTE: This functionality is untested, and should be used with caution.
        /// </summary>
        private bool m_UseGroupsIfPossible = false;

        /// <summary>
        /// The parameters used in the solid faceter.
        /// </summary>
        private SolidOrShellTessellationControls m_TessellationControls = null;

        /// <summary>
        /// The tessellation level, used to set the SolidOrShellTessellationControls, and for internal facetation.
        /// </summary>
        private BodyTessellationLevel m_TessellationLevel = BodyTessellationLevel.Default;

        /// <summary>
        /// Constructs a default BodyExporterOptions object.
        /// </summary>
        public BodyExporterOptions() { }

        /// <summary>
        /// Constructs a BodyExporterOptions object with the tryToExportAsExtrusion parameter overridden.
        /// </summary>
        /// <param name="tryToExportAsExtrusion">
        /// Export as extrusion if possible.
        /// </param>
        public BodyExporterOptions(bool tryToExportAsExtrusion)
        {
            TryToExportAsExtrusion = tryToExportAsExtrusion;
        }

        /// <summary>
        /// Try to export the solids as extrusions, if possible.
        /// </summary>
        public bool TryToExportAsExtrusion
        {
            get { return m_TryToExportAsExtrusion; }
            set { m_TryToExportAsExtrusion = value; }
        }

        /// <summary>
        /// If the body contains geometries that are identical except position and orientation, use mapped items to reuse the geometry.
        /// NOTE: This functionality is untested, and should be used with caution.
        /// </summary>
        public bool UseMappedGeometriesIfPossible
        {
            get { return m_UseMappedGeometriesIfPossible; }
            set { m_UseMappedGeometriesIfPossible = value; }
        }

        /// <summary>
        /// If the element is part of a group, and has unmodified geoemtry, use mapped items to share the geometry between groups.
        /// NOTE: This functionality is untested, and should be used with caution.
        /// </summary>
        public bool UseGroupsIfPossible
        {
            get { return m_UseGroupsIfPossible; }
            set { m_UseGroupsIfPossible = value; }
        }

        /// <summary>
        /// The accuracy parameter used in the solid faceter.
        /// </summary>
        public SolidOrShellTessellationControls TessellationControls
        {
            get 
            {
                if (m_TessellationControls == null)
                    m_TessellationControls = new SolidOrShellTessellationControls();
                return m_TessellationControls;
            }
            set { m_TessellationControls = value; }
        }

        public BodyTessellationLevel TessellationLevel
        {
            get { return m_TessellationLevel; }

            set
            {
                m_TessellationLevel = value;
                switch (m_TessellationLevel)
                {
                    case BodyTessellationLevel.Coarse:
                        {
                            TessellationControls.LevelOfDetail = 0.25;
                            TessellationControls.MinAngleInTriangle = 0;
                            //TessellationControls.MinExternalAngleBetweenTriangles = 2.0 * Math.PI;
                            return;
                        }
                    case BodyTessellationLevel.Default:
                        {
                            TessellationControls = null;    // will be recreated by getter if necessary.
                            return;
                        }
                }
            }
        }
    }
}
