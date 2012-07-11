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
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export a Revit element as IfcBuildingElementProxy.
    /// </summary>
    class ProxyElementExporter
    {
        /// <summary>
        /// Exports an element as building element proxy.
        /// </summary>
        /// <remarks>
        /// This function is called from the Export function, but can also be called directly if you do not
        /// want CreateInternalPropertySets to be called.
        /// </remarks>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        /// <returns>True if exported successfully, false otherwise.</returns>
        public static bool ExportBuildingElementProxy(ExporterIFC exporterIFC, Element element,
            GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            if (element == null || geometryElement == null)
                return false;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                using (IFCPlacementSetter placementSetter = IFCPlacementSetter.Create(exporterIFC, element))
                {
                    using (IFCExtrusionCreationData ecData = new IFCExtrusionCreationData())
                    {
                        ecData.SetLocalPlacement(placementSetter.GetPlacement());

                        ElementId categoryId = CategoryUtil.GetSafeCategoryId(element);

                        BodyExporterOptions bodyExporterOptions = new BodyExporterOptions(true);
                        IFCAnyHandle representation = RepresentationUtil.CreateBRepProductDefinitionShape(element.Document.Application, exporterIFC, element,
                            categoryId, geometryElement, bodyExporterOptions, null, ecData);

                        if (IFCAnyHandleUtil.IsNullOrHasNoValue(representation))
                        {
                            ecData.ClearOpenings();
                            return false;
                        }

                        string guid = ExporterIFCUtils.CreateGUID(element);
                        IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
                        string objectType = exporterIFC.GetFamilyName();
                        IFCAnyHandle localPlacement = ecData.GetLocalPlacement();
                        string elementTag = NamingUtil.CreateIFCElementId(element);

                        IFCAnyHandle buildingElementProxy = IFCInstanceExporter.CreateBuildingElementProxy(file, guid,
                            ownerHistory, objectType, null, objectType, localPlacement, representation, elementTag, Toolkit.IFCElementComposition.Element);

                        productWrapper.AddElement(buildingElementProxy, placementSetter.GetLevelInfo(), ecData, LevelUtil.AssociateElementToLevel(element));
                    }
                    tr.Commit();
                    return true;
                }
            }
        }

        /// <summary>
        /// Exports an element as building element proxy.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="element">The element.</param>
        /// <param name="geometryElement">The geometry element.</param>
        /// <param name="productWrapper">The IFCProductWrapper.</param>
        /// <returns>True if exported successfully, false otherwise.</returns>
        public static bool Export(ExporterIFC exporterIFC, Element element,
            GeometryElement geometryElement, IFCProductWrapper productWrapper)
        {
            bool exported = false;
            if (element == null || geometryElement == null)
                return exported;

            IFCFile file = exporterIFC.GetFile();

            using (IFCTransaction tr = new IFCTransaction(file))
            {
                exported = ExportBuildingElementProxy(exporterIFC, element, geometryElement, productWrapper);
                if (exported)
                {
                    PropertyUtil.CreateInternalRevitPropertySets(exporterIFC, element, productWrapper);
                    tr.Commit();
                }
            }

            return exported;
        }
    }
}
