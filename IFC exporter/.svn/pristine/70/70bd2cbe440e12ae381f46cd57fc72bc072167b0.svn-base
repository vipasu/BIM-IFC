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
    /// The cache which holds all export options.
    /// </summary>
    class ExportOptionsCache
    {
        /// <summary>
        /// Private default constructor.
        /// </summary>
        private ExportOptionsCache()
        { }

        /// <summary>
        /// Creates a new export options cache from the data in the ExporterIFC passed from Revit.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC handle passed during export.</param>
        /// <returns>The new cache.</returns>
        public static ExportOptionsCache Create(ExporterIFC exporterIFC, Autodesk.Revit.DB.View filterView)
        {
            IDictionary<String, String> options = exporterIFC.GetOptions();

            ExportOptionsCache cache = new ExportOptionsCache();
            cache.FileVersion = exporterIFC.FileVersion;
            cache.FileName = exporterIFC.FileName;
            cache.ExportBaseQuantities = exporterIFC.ExportBaseQuantities;
            cache.WallAndColumnSplitting = exporterIFC.WallAndColumnSplitting;
            cache.SpaceBoundaryLevel = exporterIFC.SpaceBoundaryLevel;
            // Export Part element only if 'Current View Only' is checked and 'Show Parts' is selected. 
            cache.ExportParts = filterView != null && filterView.PartsVisibility == PartsVisibility.ShowPartsOnly;
            cache.ExportPartsAsBuildingElementsOverride = null;
            cache.ExportAllLevels = false;
            cache.ExportAnnotationsOverride = null;
            cache.ExportInternalRevitPropertySetsOverride = null;
            cache.FilterViewForExport = filterView;

            String use2009GUID = Environment.GetEnvironmentVariable("Assign2009GUIDToBuildingStoriesOnIFCExport");
            cache.Use2009BuildingStoreyGUIDs = (use2009GUID != null && use2009GUID == "1");

            String use2DRoomBoundary = Environment.GetEnvironmentVariable("Use2DRoomBoundaryForRoomVolumeCalculationOnIFCExport");
            bool? use2DRoomBoundaryOption = GetNamedBooleanOption(options, "Use2DRoomBoundaryForVolume");
            cache.Use2DRoomBoundaryForRoomVolumeCreation =
                ((use2DRoomBoundary != null && use2DRoomBoundary == "1") || 
                cache.ExportAs2x2 || 
                (use2DRoomBoundaryOption != null && use2DRoomBoundaryOption.GetValueOrDefault()));

            bool? useFamilyAndTypeNameForReference = GetNamedBooleanOption(options, "UseFamilyAndTypeNameForReference");
            cache.UseFamilyAndTypeNameForReference = (useFamilyAndTypeNameForReference != null) && useFamilyAndTypeNameForReference.GetValueOrDefault();

            // "SingleElement" export option - useful for debugging - only one input element will be processed for export
            String singleElementValue;
            String elementsToExportValue;
            if (options.TryGetValue("SingleElement", out singleElementValue))
            {
                int elementIdAsInt;
                if (Int32.TryParse(singleElementValue, out elementIdAsInt))
                {
                    List<ElementId> ids = new List<ElementId>();
                    ids.Add(new ElementId(elementIdAsInt));
                    cache.ElementsForExport = ids;
                }
                else
                {
                    // Error - the option supplied could not be mapped to int.
                    // TODO: consider logging this error later and handling results better.
                    throw new Exception("Option 'SingleElement' did not map to a usable element id");
                }
            }
            else if (options.TryGetValue("ElementsForExport", out elementsToExportValue))
            {
                String[] elements = elementsToExportValue.Split(';');
                List<ElementId> ids = new List<ElementId>();
                foreach (String element in elements)
                {
                    int elementIdAsInt;
                    if (Int32.TryParse(element, out elementIdAsInt))
                    {
                        ids.Add(new ElementId(elementIdAsInt));
                    }
                    else
                    {
                        // Error - the option supplied could not be mapped to int.
                        // TODO: consider logging this error later and handling results better.
                        throw new Exception("Option 'ElementsForExport' substring " + element + " did not map to a usable element id");
                    }
                }
                cache.ElementsForExport = ids;
            }
            else
            {
                cache.ElementsForExport = new List<ElementId>();
            }

            // "ExportAnnotations" override
            cache.ExportAnnotationsOverride = GetNamedBooleanOption(options, "ExportAnnotations");

            // "Revit property sets" override
            cache.ExportInternalRevitPropertySetsOverride = GetNamedBooleanOption(options, "ExportInternalRevitPropertySets");

            // "ExportSeparateParts" override
            cache.ExportPartsAsBuildingElementsOverride = GetNamedBooleanOption(options, "ExportPartsAsBuildingElements");

            // "FileType" - note - setting is not respected yet
            ParseFileType(options, cache);

            return cache;
        }

        /// <summary>
        /// Utility for processing boolean option from the options collection.
        /// </summary>
        /// <param name="options">The collection of named options for IFC export.</param>
        /// <param name="optionName">The name of the target option.</param>
        /// <returns>The value of the option, or null if the option is not set.</returns>
        private static bool? GetNamedBooleanOption(IDictionary<String, String> options, String optionName)
        {
            String optionString;
            if (options.TryGetValue(optionName, out optionString))
            {
                bool option;
                if (Boolean.TryParse(optionString, out option))
                {
                    return option;
                }
                else
                {
                    // Error - the option supplied could not be mapped to ExportAnnotations.
                    // TODO: consider logging this error later and handling results better.
                    throw new Exception("Option '" + optionName +"' could not be parsed to boolean");
                }
            }
            return null;
        }

        /// <summary>
        /// Utility for processing string option from the options collection.
        /// </summary>
        /// <param name="options">The collection of named options for IFC export.</param>
        /// <param name="optionName">The name of the target option.</param>
        /// <returns>The value of the option, or null if the option is not set.</returns>
        private static string GetNamedStringOption(IDictionary<String, String> options, String optionName)
        {
            String optionString;
            options.TryGetValue(optionName, out optionString);
            return optionString;
        }

        /// <summary>
        /// Utility for parsing IFC file type.
        /// </summary>
        /// <remarks>
        /// If the file type can't be retrieved from the options collection, it will parse the file name extension.
        /// </remarks>
        /// <param name="options">The collection of named options for IFC export.</param>
        /// <param name="cache">The export options cache.</param>
        private static void ParseFileType(IDictionary<String, String> options, ExportOptionsCache cache)
        {            
            String fileTypeString;
            if (options.TryGetValue("FileType", out fileTypeString))
            {
                IFCFileFormat fileType;
                if (Enum.TryParse<IFCFileFormat>(fileTypeString, true, out fileType))
                {
                    cache.IFCFileFormat = fileType;
                }
                else
                {
                    // Error - the option supplied could not be mapped to ExportFileType.
                    // TODO: consider logging this error later and handling results better.
                    throw new Exception("Option 'FileType' did not match an existing IFCFileFormat value");
                }
            }
            else if (!string.IsNullOrEmpty(cache.FileName))
            {
                if (cache.FileName.EndsWith(".ifcXML")) //localization?
                {
                    cache.IFCFileFormat = IFCFileFormat.IfcXML;
                }
                else if (cache.FileName.EndsWith(".ifcZIP"))
                {
                    cache.IFCFileFormat = IFCFileFormat.IfcZIP;
                }
                else
                {
                    cache.IFCFileFormat = IFCFileFormat.Ifc;
                }
            }
        }

        /// <summary>
        /// The file version.
        /// </summary>
        public IFCVersion FileVersion
        {
            get;
            set;
        }

        /// <summary>
        /// The file name.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }
        
        /// <summary>
        /// Identifies if the file version being exported is 2x2.
        /// </summary>
        public bool ExportAs2x2
        {
            get
            {
                return FileVersion == IFCVersion.IFC2x2 || FileVersion == IFCVersion.IFCBCA;
            }
        }

        /// <summary>
        /// Identifies if the file version being exported is 2x3 Coordination View 2.0.
        /// </summary>
        public bool ExportAs2x3CoordinationView2
        {
            get
            {
                return FileVersion == IFCVersion.IFC2x3CV2;
            }
        }

        /// <summary>
        /// Cache variable for the export annotations override (if set independently via the UI or API inputs)
        /// </summary>
        private bool? ExportAnnotationsOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies if the file version being exported supports annotations.
        /// </summary>
        public bool ExportAnnotations
        {
            get
            {
                if (ExportAnnotationsOverride != null) return (bool)ExportAnnotationsOverride;
                return (!ExportAs2x2 && !ExportAs2x3CoordinationView2);
            }
        }
        
        /// <summary>
        /// Whether or not split walls and columns.
        /// </summary>
        public bool WallAndColumnSplitting
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not export base quantities.
        /// </summary>
        public bool ExportBaseQuantities
        {
            get;
            set;
        }

        /// <summary>
        /// The space boundary level.
        /// </summary>
        public int SpaceBoundaryLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not export the Part element from host.
        /// Export Part element only if 'Current View Only' is checked and 'Show Parts' is selected. 
        /// </summary>
        public bool ExportParts
        {
            get;
            set;
        }

        /// <summary>
        /// Cache variable for the ExportPartsAsBuildingElements override (if set independently via the UI)
        /// </summary>
        public bool? ExportPartsAsBuildingElementsOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not export the Parts as independent building elements.
        /// Only if allows export parts and 'Export parts as building elements' is selected. 
        /// </summary>
        public bool ExportPartsAsBuildingElements
        {
            get
            {
                if (ExportPartsAsBuildingElementsOverride != null)
                    return (bool)ExportPartsAsBuildingElementsOverride;
                return false;
            }
        }

        /// <summary>
        /// A collection of elements from which to export (before filtering is applied).  If empty, all elements in the document
        /// are used as the initial set of elements before filtering is applied.
        /// </summary>
        public List<ElementId> ElementsForExport
        {
            get;
            set;
        }

        /// <summary>
        /// The filter view for export.
        /// </summary>
        public View FilterViewForExport
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not to use R2009 GUIDs for exporting Levels.  If this option is set, export will write the old
        /// GUID value into an IfcGUID parameter for the Level, requiring the user to save the file if they want to
        /// ensure that the old GUID is used permanently.
        /// To set this to true, add the environment variable Assign2009GUIDToBuildingStoriesOnIFCExport and set the value to 1.
        /// </summary>
        public bool Use2009BuildingStoreyGUIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not to export all levels, or just export building stories.
        /// This will be set to true by default if there are no building stories in the file.
        /// </summary>
        public bool ExportAllLevels
        {
            get;
            set;
        }

        /// <summary>
        /// Determines how to generate space volumes on export.  True means that we use the 2D room boundary and extrude it upwards based
        /// on the room height.  This is the method used in 2x2 and by user option.  False means using the room geometry.  The user option
        /// is needed for certain governmental requirements, such as in Korea for non-residental buildings.
        /// </summary>
        public bool Use2DRoomBoundaryForRoomVolumeCreation
        {
            get;
            set;
        }

        /// <summary>
        /// Determines how to generate the Reference value for elements.  There are two possibilities:
        /// 1. true: use the family name and the type name.  Ex.  Basic Wall: Generic -8".  This allows distinguishing between two
        /// identical type names in different families.
        /// 2. false: use the type name only.  Ex:  Generic -8".  This allows for proper tagging when the type name is determined
        /// by code (e.g. a construction type).
        /// </summary>
        public bool UseFamilyAndTypeNameForReference
        {
            get;
            set;
        }

        /// <summary>
        /// Override for the RevitPropertySets value from UI or API options.
        /// </summary>
        private bool? ExportInternalRevitPropertySetsOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not to include RevitPropertySets
        /// </summary>
        public bool ExportInternalRevitPropertySets
        {
            get
            {
                if (ExportInternalRevitPropertySetsOverride != null) return (bool)ExportInternalRevitPropertySetsOverride;
                return !ExportAs2x3CoordinationView2;
            }
        }

        /// <summary>
        /// The file format to export.  Not used currently.
        /// </summary>
        // TODO: Connect this to the output file being written by the client.
        public IFCFileFormat IFCFileFormat
        {
            get;
            set;
        }
    }
}
