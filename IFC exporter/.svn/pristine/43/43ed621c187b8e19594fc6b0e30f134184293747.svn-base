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

// The enums below specify sub-element values to be used in the CreateSubElementGUID function.
// This ensures that their GUIDs are consistent across exports.
// Note that sub-element GUIDs can not be stored on import, so they do not survive roundtrips.
namespace BIM.IFC.Toolkit
{
    enum IFCAssemblyInstanceSubElements
    {
        RelContainedInSpatialStructure = 1
    }
    
    enum IFCBeamSubElements
    {
        PSetBeamCommon = 1
    }

    enum IFCBuildingSubElements
    {
        RelContainedInSpatialStructure = 1,
        RelAggregatesProducts = 2,
        RelAggregatesBuildingStoreys = 3
    }

    enum IFCBuildingStoreySubElements
    {
        RelContainedInSpatialStructure = 1,
        RelAggregates = 2
    }

    // Curtain Walls can be created from a variety of elements, including Walls and Roofs.
    // As such, start their subindexes high enough to not bother potential hosts.
    enum IFCCurtainWallSubElements
    {
        RelAggregates = 1024
    }

    enum IFCDoorSubElements
    {    
        DoorLining = 1,
        DoorPanelStart = 2,
        DoorPanelEnd = DoorPanelStart + 15
    }

    enum IFCRampSubElements
    {
        PSetRampCommon = 1,
        ContainedRamp = 2,
        ContainmentRelation = 3 // same as IFCStairSubElements.ContainmentRelation
    }

    enum IFCRoofSubElements
    {
        PSetRoofCommon = 1,
        RoofSlabStart = 2,
        RoofSlabEnd = RoofSlabStart + 15
    }

    enum IFCSlabSubElements
    {
        PSetSlabCommon = 1
    }

    enum IFCStairSubElements
    {
        PSetStairCommon = 1,
        ContainedStair = 2,
        ContainmentRelation = 3
    }

    enum IFCWallSubElements
    {
        PSetWallCommon = 1,
        RelAggregatesReserved = IFCCurtainWallSubElements.RelAggregates
    }
}
