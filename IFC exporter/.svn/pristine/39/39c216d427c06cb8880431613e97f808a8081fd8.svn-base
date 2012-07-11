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
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;
using BIM.IFC.Toolkit;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export text notes.
    /// </summary>
    class TextNoteExporter
    {
        /// <summary>
        /// Exports text note elements.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="textNote">
        /// The text note element.
        /// </param>
        /// <param name="productWrapper">
        /// The IFCProductWrapper.
        /// </param>
        public static void Export(ExporterIFC exporterIFC, TextNote textNote, IFCProductWrapper productWrapper)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                string textString = textNote.Text;
                if (String.IsNullOrEmpty(textString))
                    throw new Exception("TextNote does not have test string.");

                ElementId symId = textNote.GetTypeId();
                if (symId == ElementId.InvalidElementId)
                    throw new Exception("TextNote does not have valid type id.");

                PresentationStyleAssignmentCache cache = ExporterCacheManager.PresentationStyleAssignmentCache;
                IFCAnyHandle presHnd = cache.Find(symId);
                if (IFCAnyHandleUtil.IsNullOrHasNoValue(presHnd))
                {
                    TextElementType textElemType = textNote.Symbol;
                    CreatePresentationStyleAssignmentForTextElementType(exporterIFC, textElemType, cache);
                    presHnd = cache.Find(symId);
                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(presHnd))
                        throw new Exception("Failed to create presentation style assignment for TextElementType.");
                }

                HashSet<IFCAnyHandle> presHndSet = new HashSet<IFCAnyHandle>();
                presHndSet.Add(presHnd);

                using (IFCPlacementSetter setter = IFCPlacementSetter.Create(exporterIFC, textNote))
                {
                    double linScale = exporterIFC.LinearScale;
                    const double planScale = 100.0;  // currently hardwired.

                    XYZ orig = textNote.Coord;
                    orig = orig.Multiply(linScale);
                    XYZ yDir = textNote.UpDirection;
                    XYZ xDir = textNote.BaseDirection;
                    XYZ zDir = xDir.CrossProduct(yDir);

                    double sizeX = textNote.LineWidth * linScale * planScale;
                    double sizeY = textNote.Height * linScale * planScale;

                    // When we display text on screen, we "flip" it if the xDir is negative with relation to
                    // the X-axis.  So if it is, we'll flip x and y.
                    bool flipOrig = false;
                    if (xDir.X < 0)
                    {
                        xDir = xDir.Multiply(-1.0);
                        yDir = yDir.Multiply(-1.0);
                        flipOrig = true;
                    }

                    // xFactor, yFactor only used if flipOrig.
                    double xFactor = 0.0, yFactor = 0.0;
                    string boxAlignment = ConvertTextNoteAlignToBoxAlign(textNote, out xFactor, out yFactor);

                    // modify the origin to match the alignment.  In Revit, the origin is at the top-left (unless flipped,
                    // then bottom-right).
                    if (flipOrig)
                    {
                        orig = orig.Add(xDir.Multiply(sizeX * xFactor));
                        orig = orig.Add(yDir.Multiply(sizeY * yFactor));
                    }

                    IFCAnyHandle origin = ExporterUtil.CreateAxis(file, orig, zDir, xDir);

                    IFCAnyHandle extent = IFCInstanceExporter.CreatePlanarExtent(file, sizeX, sizeY);
                    IFCAnyHandle repItemHnd = IFCInstanceExporter.CreateTextLiteralWithExtent(file, textString, origin, Toolkit.IFCTextPath.Left, extent, boxAlignment);
                    IFCAnyHandle annoTextOccHnd = IFCInstanceExporter.CreateStyledItem(file, repItemHnd, presHndSet, null);

                    ElementId catId = textNote.Category != null ? textNote.Category.Id : ElementId.InvalidElementId;
                    HashSet<IFCAnyHandle> bodyItems = new HashSet<IFCAnyHandle>();
                    bodyItems.Add(repItemHnd);
                    IFCAnyHandle bodyRepHnd = RepresentationUtil.CreateAnnotationSetRep(exporterIFC, textNote, catId, exporterIFC.Get2DContextHandle(), bodyItems);

                    if (IFCAnyHandleUtil.IsNullOrHasNoValue(bodyRepHnd))
                        throw new Exception("Failed to create shape representation.");

                    IList<IFCAnyHandle> shapeReps = new List<IFCAnyHandle>();
                    shapeReps.Add(bodyRepHnd);

                    IFCAnyHandle prodShapeHnd = IFCInstanceExporter.CreateProductDefinitionShape(file, null, null, shapeReps);
                    IFCAnyHandle annoHnd = IFCInstanceExporter.CreateAnnotation(file, ExporterIFCUtils.CreateGUID(), exporterIFC.GetOwnerHistoryHandle(),
                        null, null, null, setter.GetPlacement(), prodShapeHnd);

                    productWrapper.AddAnnotation(annoHnd, setter.GetLevelInfo(), true);
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// Creates IfcPresentationStyleAssignment for text element type.
        /// </summary>
        /// <param name="exporterIFC">
        /// The ExporterIFC object.
        /// </param>
        /// <param name="textElemType">
        /// The text note element type.
        /// </param>
        /// <param name="cache">
        /// The cache of IfcPresentationStyleAssignment.
        /// </param>
        static void CreatePresentationStyleAssignmentForTextElementType(ExporterIFC exporterIFC, TextElementType textElemType, PresentationStyleAssignmentCache cache)
        {
            IFCFile file = exporterIFC.GetFile();

            Parameter fontNameParam = textElemType.get_Parameter(BuiltInParameter.TEXT_FONT);
            string fontName = (fontNameParam != null && fontNameParam.StorageType == StorageType.String) ? fontNameParam.AsString() : null;


            Parameter fontSizeParam = textElemType.get_Parameter(BuiltInParameter.TEXT_SIZE);
            double fontSize = (fontSizeParam != null && fontSizeParam.StorageType == StorageType.Double) ? fontSizeParam.AsDouble() : -1.0;

            double scale = exporterIFC.LinearScale;
            double viewScale = 100.0;  // currently hardwired.
            fontSize *= (scale * viewScale);

            string ifcPreDefinedItemName = "Text Font";

            IList<string> fontNameList = new List<string>();
            fontNameList.Add(fontName);

            IFCAnyHandle textSyleFontModelHnd = IFCInstanceExporter.CreateTextStyleFontModel(file, ifcPreDefinedItemName, fontNameList, null, null, null, IFCDataUtil.CreateAsPositiveLengthMeasure(fontSize));

            Parameter fontColorParam = textElemType.get_Parameter(BuiltInParameter.LINE_COLOR);
            int color = (fontColorParam != null && fontColorParam.StorageType == StorageType.Integer) ? fontColorParam.AsInteger() : 0;

            double blueVal = ((double)((color & 0xff0000) >> 16)) / 255.0;
            double greenVal = ((double)((color & 0xff00) >> 8)) / 255.0;
            double redVal = ((double)(color & 0xff)) / 255.0;

            IFCAnyHandle colorHnd = IFCInstanceExporter.CreateColourRgb(file, null, redVal, greenVal, blueVal);
            IFCAnyHandle fontColorHnd = IFCInstanceExporter.CreateTextStyleForDefinedFont(file, colorHnd, null);

            string ifcAttrName = textElemType.Name;
            IFCAnyHandle textStyleHnd = IFCInstanceExporter.CreateTextStyle(file, textElemType.Name, fontColorHnd, null, textSyleFontModelHnd);

            if (IFCAnyHandleUtil.IsNullOrHasNoValue(textStyleHnd))
                return;

            HashSet<IFCAnyHandle> presStyleSet = new HashSet<IFCAnyHandle>();
            presStyleSet.Add(textStyleHnd);

            IFCAnyHandle presStyleHnd = IFCInstanceExporter.CreatePresentationStyleAssignment(file, presStyleSet);
            if (IFCAnyHandleUtil.IsNullOrHasNoValue(presStyleHnd))
                return;

            cache.Register(textElemType.Id, presStyleHnd);
        }

        /// <summary>
        /// Converts text note align to box align.
        /// </summary>
        /// <param name="textNote">
        /// The text note element.
        /// </param>
        /// <param name="xFactor">
        /// The X factor.
        /// </param>
        /// <param name="yFactor">
        /// The Y factor.
        /// </param>
        static string ConvertTextNoteAlignToBoxAlign(TextNote textNote, out double xFactor, out double yFactor)
        {
            TextAlignFlags align = textNote.Align;

            int alignCase = 0;

            if ((align & TextAlignFlags.TEF_ALIGN_LEFT) != 0)
            {
                if ((align & TextAlignFlags.TEF_ALIGN_TOP) != 0)
                {
                    alignCase = 0;
                }
                else if ((align & TextAlignFlags.TEF_ALIGN_BOTTOM) != 0)
                {
                    alignCase = 1;
                }
                else
                {
                    alignCase = 2;
                }
            }
            else if ((align & TextAlignFlags.TEF_ALIGN_RIGHT) != 0)
            {
                if ((align & TextAlignFlags.TEF_ALIGN_TOP) != 0)
                {
                    alignCase = 7;
                }
                else if ((align & TextAlignFlags.TEF_ALIGN_BOTTOM) != 0)
                {
                    alignCase = 8;
                }
                else
                {
                    alignCase = 6;
                }
            }
            else
            {
                if ((align & TextAlignFlags.TEF_ALIGN_TOP) != 0)
                {
                    alignCase = 3;
                }
                else if ((align & TextAlignFlags.TEF_ALIGN_BOTTOM) != 0)
                {
                    alignCase = 5;
                }
                else
                {
                    alignCase = 4;
                }
            }

            xFactor = 0.0;
            yFactor = 0.0;
            string boxAlignmeng = null;

            switch (alignCase)
            {
                case 0:
                    boxAlignmeng = "top-left";
                    xFactor = -1.0;
                    yFactor = 1.0;
                    break;
                case 1:
                    boxAlignmeng = "bottom-left";
                    xFactor = -1.0;
                    yFactor = -1.0;
                    break;
                case 2:
                    boxAlignmeng = "middle-left";
                    xFactor = -1.0;
                    break;
                case 3:
                    boxAlignmeng = "top-middle";
                    yFactor = 1.0;
                    break;
                case 4:
                    boxAlignmeng = "center";
                    break;
                case 5:
                    boxAlignmeng = "bottom-middle";
                    yFactor = -1.0;
                    break;
                case 6:
                    boxAlignmeng = "middle-right";
                    xFactor = 1.0;
                    break;
                case 7:
                    boxAlignmeng = "top-right";
                    xFactor = 1.0;
                    yFactor = 1.0;
                    break;
                case 8:
                    boxAlignmeng = "bottom-right";
                    xFactor = 1.0;
                    yFactor = -1.0;
                    break;
            }

            return boxAlignmeng;
        }
    }
}