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

using Autodesk.Revit.DB.IFC;

namespace BIM.IFC.Export.UI
{
    /// <summary>
    /// Represents the types of files that can be produced during IFC export.
    /// </summary>
    public class IFCFileFormatAttributes
    {
        /// <summary>
        /// The IFC file format into which a file may be exported.
        /// </summary>
        public IFCFileFormat FileType { get; set; }

        /// <summary>
        /// Constructs the file format choices.
        /// </summary>
        /// <param name="fileType"></param>
        public IFCFileFormatAttributes(IFCFileFormat fileType)
        {
            FileType = fileType;
        }

        /// <summary>
        /// Converts the IFCFileFormat to string.
        /// </summary>
        /// <returns>The string of IFCFileFormat.</returns>
        public override String ToString()
        {
            switch (FileType)
            {
                case IFCFileFormat.Ifc:
                    return "IFC";
                case IFCFileFormat.IfcXML:
                    return "IFC XML";
                case IFCFileFormat.IfcZIP:
                    return "Zipped IFC";
                case IFCFileFormat.IfcXMLZIP:
                    return "Zipped IFC XML";
                default:
                    return "unrecognized file type option";
            }
        }

        /// <summary>
        /// Gets the string of IFCFileFormat extension.
        /// </summary>
        /// <returns>The string of IFCFileFormat extension.</returns>
        public String GetFileExtension()
        {
            switch (FileType)
            {
                case IFCFileFormat.Ifc:
                    return "ifc";
                case IFCFileFormat.IfcXML:
                    return "ifcxml";
                case IFCFileFormat.IfcZIP:
                    return "ifczip";
                case IFCFileFormat.IfcXMLZIP:
                    return "ifczip";
                default:
                    return "unrecognized file type option";
            }
        }

        /// <summary>
        /// Gets the string of IFCFileFormat filter.
        /// </summary>
        /// <returns>The string of IFCFileFormat filter.</returns>
        public String GetFileFilter()
        {
            switch (FileType)
            {
                case IFCFileFormat.Ifc:
                    return "IFC-SPF|*.ifc";
                case IFCFileFormat.IfcXML:
                    return "Industry Foundation Classes XML(.ifcxml)|*.ifcxml";
                case IFCFileFormat.IfcZIP:
                    return "Zipped Industry Foundation Classes(.ifczip)|*.ifczip";
                case IFCFileFormat.IfcXMLZIP:
                    return "Zipped Industry Foundation Classes(.ifczip)|*.ifczip";
                default:
                    return "unrecognized file type option";
            }
        }
    }
}
