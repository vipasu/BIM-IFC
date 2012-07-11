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
using BIM.IFC.Exporter;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Utility
{
    /// <summary>
    /// Provides static methods to create openings.
    /// </summary>
    class OpeningUtil
    {
        /// <summary>
        /// Creates openings if there is necessary.
        /// </summary>
        /// <param name="elementHandle">
        /// The element handle to create openings.
        /// </param>
        /// <param name="element">
        /// The element to create openings.
        /// </param>
        /// <param name="info">
        /// The extrusion datas.
        /// </param>
        /// <param name="extraParams">
        /// The extrusion creation data.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="originalPlacement">
        /// The original placement handle.
        /// </param>
        /// <param name="setter">
        /// The IFCPlacementSetter.
        /// </param>
        /// <param name="wrapper">
        /// The IFCProductWrapper.
        /// </param>
        private static void CreateOpeningsIfNecessaryBase(IFCAnyHandle elementHandle, Element element, IList<IFCExtrusionData> info,
           IFCExtrusionCreationData extraParams, ExporterIFC exporterIFC,
           IFCAnyHandle originalPlacement, IFCPlacementSetter setter, IFCProductWrapper wrapper)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(elementHandle))
                return;

            int sz = info.Count;
            if (sz == 0)
                return;

            IFCFile file = exporterIFC.GetFile();
            ElementId categoryId = CategoryUtil.GetSafeCategoryId(element);
            Document document = element.Document;

            string openingObjectType = "Opening";

            int openingNumber = 1;
            for (int curr = info.Count - 1; curr >= 0; curr--)
            {
                IFCAnyHandle extrusionHandle = ExtrusionExporter.CreateExtrudedSolidFromExtrusionData(exporterIFC, element, info[curr]);
                if (IFCAnyHandleUtil.IsNullOrHasNoValue(extrusionHandle))
                    continue;

                IFCAnyHandle styledItemHnd = BodyExporter.CreateSurfaceStyleForRepItem(exporterIFC, document,
                    extrusionHandle, ElementId.InvalidElementId);
                        
                HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
                bodyItems.Add(extrusionHandle);

                IFCAnyHandle contextOfItems = exporterIFC.Get3DContextHandle("Body");
                IFCAnyHandle bodyRep = RepresentationUtil.CreateSweptSolidRep(exporterIFC, element, categoryId, contextOfItems, bodyItems, null);
                IList<IFCAnyHandle> representations = new List<IFCAnyHandle>();
                representations.Add(bodyRep);

                IFCAnyHandle openingRep = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, representations);

                IFCAnyHandle openingPlacement = ExporterUtil.CopyLocalPlacement(file, originalPlacement);
                string guid = ExporterIFCUtils.CreateGUID();
                IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                string openingName = NamingUtil.GetNameOverride(element, NamingUtil.CreateIFCName(exporterIFC, openingNumber++));
                string elementId = NamingUtil.CreateIFCElementId(element);
                IFCAnyHandle openingElement = IFCInstanceExporter.CreateOpeningElement(file, guid, ownerHistory,
                   openingName, null, openingObjectType, openingPlacement, openingRep, elementId);
                wrapper.AddElement(openingElement, setter, extraParams, true);
                if (ExporterCacheManager.ExportOptionsCache.ExportBaseQuantities && (extraParams != null))
                    ExporterIFCUtils.CreateOpeningQuantities(exporterIFC, openingElement, extraParams);

                string voidGuid = ExporterIFCUtils.CreateGUID();
                IFCInstanceExporter.CreateRelVoidsElement(file, voidGuid, ownerHistory, null, null, elementHandle, openingElement);
            }
        }

        /// <summary>
        /// Creates openings if there is necessary.
        /// </summary>
        /// <param name="elementHandle">
        /// The element handle to create openings.
        /// </param>
        /// <param name="element">
        /// The element to create openings.
        /// </param>
        /// <param name="info">
        /// The extrusion datas.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="originalPlacement">
        /// The original placement handle.
        /// </param>
        /// <param name="setter">
        /// The IFCPlacementSetter.
        /// </param>
        /// <param name="wrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void CreateOpeningsIfNecessary(IFCAnyHandle elementHandle, Element element, IList<IFCExtrusionData> info,
           ExporterIFC exporterIFC, IFCAnyHandle originalPlacement,
           IFCPlacementSetter setter, IFCProductWrapper wrapper)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(elementHandle))
                return;

            using (IFCExtrusionCreationData extraParams = new IFCExtrusionCreationData())
            {
                CreateOpeningsIfNecessaryBase(elementHandle, element, info, extraParams, exporterIFC, originalPlacement, setter, wrapper);
            }
        }

        /// <summary>
        /// Creates openings if there is necessary.
        /// </summary>
        /// <param name="elementHandle">
        /// The element handle to create openings.
        /// </param>
        /// <param name="element">
        /// The element to create openings.
        /// </param>
        /// <param name="extraParams">
        /// The extrusion creation data.
        /// </param>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="originalPlacement">
        /// The original placement handle.
        /// </param>
        /// <param name="setter">
        /// The IFCPlacementSetter.
        /// </param>
        /// <param name="wrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void CreateOpeningsIfNecessary(IFCAnyHandle elementHandle, Element element, IFCExtrusionCreationData extraParams,
           ExporterIFC exporterIFC, IFCAnyHandle originalPlacement,
           IFCPlacementSetter setter, IFCProductWrapper wrapper)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(elementHandle))
                return;

            ElementId categoryId = CategoryUtil.GetSafeCategoryId(element);

            IList<IFCExtrusionData> info = extraParams.GetOpenings();
            CreateOpeningsIfNecessaryBase(elementHandle, element, info, extraParams, exporterIFC, originalPlacement, setter, wrapper);
            extraParams.ClearOpenings();
        }

        public static bool NeedToCreateOpenings(IFCAnyHandle elementHandle, IFCExtrusionCreationData extraParams)
        {
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(elementHandle))
                return false;

            if (extraParams == null)
                return false;

            IList<IFCExtrusionData> info = extraParams.GetOpenings();
            return (info.Count > 0);
        }
    }
}
