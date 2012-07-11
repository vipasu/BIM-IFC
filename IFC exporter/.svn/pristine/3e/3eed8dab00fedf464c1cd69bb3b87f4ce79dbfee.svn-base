//
// BIM IFC export alternate UI library: this library works with Autodesk(R) Revit(R) to provide an alternate user interface for the export of IFC files from Revit.
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

using BIM.IFC.Export.UI.Properties;

namespace BIM.IFC.Export.UI
{
    /// <summary>
    /// Represents a particular setup for an export to IFC.
    /// </summary>
    public class IFCExportConfiguration
    {
        /// <summary>
        /// The name of the configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The IFCVersion of the configuration.
        /// </summary>
        public IFCVersion IFCVersion { get; set; }

        /// <summary>
        /// The IFCFileFormat of the configuration.
        /// </summary>
        public IFCFileFormat IFCFileType { get; set; }

        /// <summary>
        /// The level of space boundaries of the configuration.
        /// </summary>
        public int SpaceBoundaries { get; set; }

        /// <summary>
        /// Whether or not to include base quantities for model elements in the export data. 
        /// Base quantities are generated from model geometry to reflect actual physical quantity values, independent of measurement rules or methods.
        /// </summary>
        public bool ExportBaseQuantities { get; set; } 

        /// <summary>
        /// Whether or not to split walls and columns by building stories.
        /// </summary>
        public bool SplitWallsAndColumns { get; set; }

        /// <summary>
        /// True to include the Revit-specific property sets based on parameter groups. 
        /// False to exclude them.
        /// </summary>
        public bool ExportInternalRevitPropertySets { get; set; }

        /// <summary>
        /// True to include 2D elements supported by IFC export (notes and filled regions). 
        /// False to exclude them.
        /// </summary>
        public bool Export2DElements { get; set; }

        /// <summary>
        /// True to export only the visible elements of the current view (based on filtering and/or element and category hiding). 
        /// False to export the entire model.  
        /// </summary>
        public bool VisibleElementsOfCurrentView { get; set; }

        /// <summary>
        /// True to use a simplified approach to calculation of room volumes (based on extrusion of 2D room boundaries) which is also the default when exporting to IFC 2x2.   
        /// False to use the Revit calculated room geometry to represent the room volumes (which is the default when exporting to IFC 2x3).
        /// </summary>
        public bool Use2DRoomBoundaryForVolume { get; set; }

        /// <summary>
        /// True to use the family and type name for references. 
        /// False to use the type name only.
        /// </summary>
        public bool UseFamilyAndTypeNameForReference { get; set; }

        /// <summary>
        /// True to export the parts as independent building elements
        /// False to export the parts with host element.
        /// </summary>
        public bool ExportPartsAsBuildingElements { get; set; }

        private bool m_isBuiltIn = false;
        private bool m_isInSession = false;
        private static IFCExportConfiguration s_inSessionConfiguration = null;

        /// <summary>
        /// Whether the configuration is builtIn or not.
        /// </summary>
        public bool IsBuiltIn
        {
            get
            {
                return m_isBuiltIn;
            }
        }

        /// <summary>
        /// Whether the configuration is in-session or not.
        /// </summary>
        public bool IsInSession
        {
            get
            {
                return m_isInSession;
            }
        }

        /// <summary>
        /// Creates a new default configuration.
        /// </summary>
        /// <returns>The new default configuration.</returns>
        public static IFCExportConfiguration CreateDefaultConfiguration()
        {
            return new IFCExportConfiguration();
        }

        /// <summary>
        /// Constructs a default configuration.
        /// </summary>
        private IFCExportConfiguration()
        {
            this.Name = "<<Default>>";
            this.IFCVersion = IFCVersion.IFC2x3;
            this.IFCFileType = IFCFileFormat.Ifc;
            this.SpaceBoundaries = 1;
            this.ExportBaseQuantities = false;
            this.SplitWallsAndColumns = false;
            this.VisibleElementsOfCurrentView = false;
            this.Use2DRoomBoundaryForVolume = false;
            this.UseFamilyAndTypeNameForReference = false;
            this.ExportInternalRevitPropertySets = false;
            this.Export2DElements = false;
            this.ExportPartsAsBuildingElements = false;
            this.m_isBuiltIn = false; 
            this.m_isInSession = false;
        }

        /// <summary>
        /// Creates a builtIn configuration by particular options.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        /// <param name="ifcVersion">The IFCVersion.</param>
        /// <param name="spaceBoundaries">The space boundary level.</param>
        /// <param name="exportBaseQuantities">The ExportBaseQuantities.</param>
        /// <param name="splitWalls">The SplitWallsAndColumns.</param>
        /// <param name="internalSets">The ExportInternalRevitPropertySets.</param>
        /// <param name="PlanElems2D">The Export2DElements.</param>
        /// <returns>The builtIn configuration.</returns>
        public static IFCExportConfiguration CreateBuiltInConfiguration(string name, 
                                   IFCVersion ifcVersion, 
                                   int spaceBoundaries,
                                   bool exportBaseQuantities,
                                   bool splitWalls,
                                   bool internalSets,
                                   bool PlanElems2D)
        {
            IFCExportConfiguration configuration = new IFCExportConfiguration();
            configuration.Name = name; 
            configuration.IFCVersion = ifcVersion; 
            configuration.IFCFileType = IFCFileFormat.Ifc; 
            configuration.SpaceBoundaries = spaceBoundaries;
            configuration.ExportBaseQuantities = exportBaseQuantities;                    
            configuration.SplitWallsAndColumns = splitWalls;
            configuration.ExportInternalRevitPropertySets = internalSets;
            configuration.Export2DElements = PlanElems2D;
            configuration.VisibleElementsOfCurrentView = false;   
            configuration.Use2DRoomBoundaryForVolume = false;
            configuration.UseFamilyAndTypeNameForReference = false;
            configuration.ExportPartsAsBuildingElements = false;
            configuration.m_isBuiltIn = true;
            configuration.m_isInSession = false; 
            return configuration;
        }

        public IFCExportConfiguration Clone()
        {
            return new IFCExportConfiguration(this);
        }

        /// <summary>
        /// Constructs a copy from a defined configuration.
        /// </summary>
        /// <param name="other">The configuration to copy.</param>
        private IFCExportConfiguration(IFCExportConfiguration other)
        {
            this.Name = other.Name;
            this.IFCVersion = other.IFCVersion;
            this.IFCFileType = other.IFCFileType;
            this.SpaceBoundaries = other.SpaceBoundaries;
            this.ExportBaseQuantities = other.ExportBaseQuantities;
            this.SplitWallsAndColumns = other.SplitWallsAndColumns;
            this.ExportInternalRevitPropertySets = other.ExportInternalRevitPropertySets;
            this.Export2DElements = other.Export2DElements;
            this.VisibleElementsOfCurrentView = other.VisibleElementsOfCurrentView;
            this.Use2DRoomBoundaryForVolume = other.Use2DRoomBoundaryForVolume;
            this.UseFamilyAndTypeNameForReference = other.UseFamilyAndTypeNameForReference;
            this.ExportPartsAsBuildingElements = other.ExportPartsAsBuildingElements;
            this.m_isBuiltIn = other.m_isBuiltIn;
            this.m_isInSession = other.m_isInSession;
        }

        /// <summary>
        /// Duplicates this configuration by giving a new name.
        /// </summary>
        /// <param name="newName">The new name of the copy configuration.</param>
        /// <returns>The duplicated configuration.</returns>
        public IFCExportConfiguration Duplicate(String newName)
        {
            return new IFCExportConfiguration(newName, this);
        }

        /// <summary>
        /// Constructs a copy configuration by providing name and defined configuration. 
        /// </summary>
        /// <param name="name">The name of copy configuration.</param>
        /// <param name="other">The defined configuration to copy.</param>
        private IFCExportConfiguration(String name, IFCExportConfiguration other)
        {
            this.Name = name;
            this.IFCVersion = other.IFCVersion;
            this.IFCFileType = other.IFCFileType;
            this.SpaceBoundaries = other.SpaceBoundaries;
            this.ExportBaseQuantities = other.ExportBaseQuantities;
            this.SplitWallsAndColumns = other.SplitWallsAndColumns;
            this.ExportInternalRevitPropertySets = other.ExportInternalRevitPropertySets;
            this.Export2DElements = other.Export2DElements;
            this.VisibleElementsOfCurrentView = other.VisibleElementsOfCurrentView;
            this.Use2DRoomBoundaryForVolume = other.Use2DRoomBoundaryForVolume;
            this.UseFamilyAndTypeNameForReference = other.UseFamilyAndTypeNameForReference;
            this.ExportPartsAsBuildingElements = other.ExportPartsAsBuildingElements;
            this.m_isBuiltIn = false;
            this.m_isInSession = false;
        }

        /// <summary>
        /// Gets the in-session  configuration.
        /// </summary>
        /// <returns>The in-session  configuration.</returns>
        public static IFCExportConfiguration GetInSession()
        {
            if (s_inSessionConfiguration == null)
            {
                s_inSessionConfiguration = new IFCExportConfiguration();
                s_inSessionConfiguration.Name = Resources.InSessionConfiguration;
                s_inSessionConfiguration.m_isInSession = true;
            }

            return s_inSessionConfiguration;
        }

        /// <summary>
        /// Set the in-session configuration to cache.
        /// </summary>
        /// <param name="configuration">The the in-session configuration.</param>
        public static void SetInSession(IFCExportConfiguration configuration)
        {
            if (!configuration.IsInSession)
            {
                throw new ArgumentException("SetInSession requires an In-Session configuration", "configuration");
            }
            s_inSessionConfiguration = configuration;
        }

        /// <summary>
        /// Updates the IFCExportOptions with the settings in this configuration.
        /// </summary>
        /// <param name="options">The IFCExportOptions to update.</param>
        /// <param name="filterViewId">The filter view.</param>
        public void UpdateOptions(IFCExportOptions options, ElementId filterViewId)
        {
            options.FileVersion = IFCVersion;
            options.SpaceBoundaryLevel = SpaceBoundaries;
            options.ExportBaseQuantities = ExportBaseQuantities;
            options.WallAndColumnSplitting = SplitWallsAndColumns;
            if (VisibleElementsOfCurrentView) 
                options.FilterViewId = filterViewId;
            else
                options.FilterViewId = ElementId.InvalidElementId;
            options.AddOption("ExportInternalRevitPropertySets", ExportInternalRevitPropertySets.ToString());
            options.AddOption("ExportAnnotations", Export2DElements.ToString());
            options.AddOption("Use2DRoomBoundaryForVolume",Use2DRoomBoundaryForVolume.ToString());
            options.AddOption("UseFamilyAndTypeNameForReference",UseFamilyAndTypeNameForReference.ToString());
            options.AddOption("ExportPartsAsBuildingElements", ExportPartsAsBuildingElements.ToString());

            options.AddOption("FileType", IFCFileType.ToString());
        }

        /// <summary>
        /// The description of the configuration.
        /// </summary>
        public String Description
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                IFCVersionAttributes versionAttributes = new IFCVersionAttributes(IFCVersion);
                builder.AppendLine(GetDescriptionLine(Resources.FileVersion, versionAttributes.ToString()));

                IFCFileFormatAttributes fileFormatAttributes = new IFCFileFormatAttributes(IFCFileType);
                builder.AppendLine(GetDescriptionLine(Resources.FileType, fileFormatAttributes.ToString()));
                builder.AppendLine(GetDescriptionLine(Resources.SplitWallsAndColumns, SplitWallsAndColumns));
                builder.AppendLine(GetDescriptionLine(Resources.ExportBaseQuantities, ExportBaseQuantities));

                IFCSpaceBoundariesAttributes spaceBoundaryAttributes = new IFCSpaceBoundariesAttributes(SpaceBoundaries);
                builder.AppendLine(GetDescriptionLine(Resources.SpaceBoundaries, spaceBoundaryAttributes.ToString()));

                builder.AppendLine(GetDescriptionLine(Resources.VisibleElementsOfCurrentView, VisibleElementsOfCurrentView));
                builder.AppendLine(GetDescriptionLine(Resources.ExportPlanViewElements, Export2DElements));
                builder.AppendLine(GetDescriptionLine(Resources.ExportRevitPropertySets, ExportInternalRevitPropertySets));
                builder.AppendLine(GetDescriptionLine(Resources.Use2DRoomBoundariesForRoomVolume, Use2DRoomBoundaryForVolume));
                builder.AppendLine(GetDescriptionLine(Resources.UseFamilyAndTypeNameForReferences, UseFamilyAndTypeNameForReference));
                builder.AppendLine(GetDescriptionLine(Resources.ExportPartsAsBuildingElements, ExportPartsAsBuildingElements));

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the one line of description string from the resource via a label and the value.
        /// </summary>
        /// <param name="label">The label in the resource.</param>
        /// <param name="value">The value in the resource.</param>
        /// <returns>The description line.</returns>
        private static String GetDescriptionLine(String label, object value)
        {
            return String.Format("{0}: {1}", label, value.ToString());
        }

        /// <summary>
        /// Converts to the string to identify the configuration.
        /// </summary>
        /// <returns>The string to identify the configuration.</returns>
        public override String ToString()
        {
            return Name + (IsBuiltIn? "*":"");
        }

    }
}
