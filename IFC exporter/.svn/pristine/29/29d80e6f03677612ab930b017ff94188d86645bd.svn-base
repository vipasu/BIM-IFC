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

namespace BIM.IFC.Exporter.PropertySet
{
    /// <summary>
    /// Provides static methods to create varies IFC classifications.
    /// </summary>
    class ClassificationUtil
    {
        /// <summary>
        /// Creates uniformat classification.
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC.</param>
        /// <param name="file">The file.</param>
        /// <param name="element">The element.</param>
        /// <param name="elemHnd">The element handle.</param>
        public static void CreateUniformatClassification(ExporterIFC exporterIFC, IFCFile file, Element element, IFCAnyHandle elemHnd)
        {
            // Create Uniformat classification, if it is set.
            string uniformatCode = "";
            if (ParameterUtil.GetStringValueFromElementOrSymbol(element, BuiltInParameter.UNIFORMAT_CODE, false, out uniformatCode) ||
                ParameterUtil.GetStringValueFromElementOrSymbol(element, "Assembly Code", out uniformatCode))
            {
                string uniformatDescription = "";
                if (!ParameterUtil.GetStringValueFromElementOrSymbol(element, BuiltInParameter.UNIFORMAT_DESCRIPTION, false, out uniformatDescription))
                    ParameterUtil.GetStringValueFromElementOrSymbol(element, "Assembly Description", out uniformatDescription);

                IFCAnyHandle classification;
                if (!ExporterCacheManager.ClassificationCache.TryGetValue("UniFormat", out classification))
                {
                    classification = IFCInstanceExporter.CreateClassification(file, "http://www.csiorg.net/uniformat", "1998", null, "UniFormat");
                    ExporterCacheManager.ClassificationCache.Add("UniFormat", classification);
                }

                IFCAnyHandle classificationReference = IFCInstanceExporter.CreateClassificationReference(file,
                  "http://www.csiorg.net/uniformat", uniformatCode, uniformatDescription, classification);

                HashSet<IFCAnyHandle> relatedObjects = new HashSet<IFCAnyHandle>();
                relatedObjects.Add(elemHnd);

                IFCAnyHandle relAssociates = IFCInstanceExporter.CreateRelAssociatesClassification(file, ExporterIFCUtils.CreateGUID(),
                   exporterIFC.GetOwnerHistoryHandle(), "UniFormatClassification", "", relatedObjects, classificationReference);
            }
        }
    }
}
