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
using System.IO;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Win32;

namespace BIM.IFC.Export.UI
{
    /// <summary>
    /// This class implements the methods of interface IExternalApplication to register the IFC export alternate UI to override the IFC export command in Autodesk Revit.
    /// </summary>
    public class IFCCommandOverrideApplication : IExternalApplication
    {
        #region IExternalApplication Members

        /// <summary>
        /// The binding to the Export IFC command in Revit.
        /// </summary>
        private AddInCommandBinding m_ifcCommandBinding;  

        /// <summary>
        /// Implementation of Shutdown for the external application.
        /// </summary>
        /// <param name="application">The Revit application.</param>
        /// <returns>The result (typically Succeeded).</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            // Clean up
            m_ifcCommandBinding.Executed -= OnIFCExport;

            return Result.Succeeded;
        }

        /// <summary>
        /// Implementation of Startup for the external application.
        /// </summary>
        /// <param name="application">The Revit application.</param>
        /// <returns>The result (typically Succeeded).</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            // Register execution override
            RevitCommandId commandId = RevitCommandId.LookupCommandId("ID_EXPORT_IFC");
            m_ifcCommandBinding = application.CreateAddInCommandBinding(commandId);
            m_ifcCommandBinding.Executed += OnIFCExport; 

            return Result.Succeeded;
        }
        #endregion

        /// <summary>
        /// The last successful export location
        /// </summary>
        private String m_mruExportPath = null;

        /// <summary>
        /// The last selected configuration
        /// </summary>
        private String m_mruConfiguration = null;
        
        /// <summary>
        /// Implementation of the command binding event for the IFC export command.
        /// </summary>
        /// <param name="sender">The event sender (Revit UIApplication).</param>
        /// <param name="args">The arguments (command binding).</param>
        public void OnIFCExport(object sender, CommandEventArgs args)
        {
            try
            {
                // Prepare basic objects
                UIApplication uiApp = sender as UIApplication;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                IFCExportConfigurationsMap configurationsMap = new IFCExportConfigurationsMap();
                configurationsMap.Add(IFCExportConfiguration.GetInSession());
                configurationsMap.AddBuiltInConfigurations();
                configurationsMap.AddSavedConfigurations(doc);

                String mruSelection = null;
                if (m_mruConfiguration != null && configurationsMap.HasName(m_mruConfiguration))
                    mruSelection =  m_mruConfiguration;

                IFCExport mainWindow = new IFCExport(configurationsMap, mruSelection);
                mainWindow.ShowDialog();

                // If user chose to continue
                if (mainWindow.Result == IFCExportResult.ExportAndSaveSettings)
                {
                    // change options
                    IFCExportConfiguration selectedConfig = mainWindow.GetSelectedConfiguration();

                    // Prepare the export options
                    IFCExportOptions exportOptions = new IFCExportOptions();
                    selectedConfig.UpdateOptions(exportOptions, uiDoc.ActiveView.Id);  

                    // prompt for the file name
                    SaveFileDialog fileDialog = new SaveFileDialog();
                    fileDialog.AddExtension = true;

                    String defaultDirectory = m_mruExportPath != null ? m_mruExportPath : null;
                    
                    if (defaultDirectory == null)
                    {
                        String revitFilePath = doc.PathName;
                        if (!String.IsNullOrEmpty(revitFilePath))
                        {
                            defaultDirectory = Path.GetDirectoryName(revitFilePath);
                        }
                    }

                    if (defaultDirectory == null)
                    {
                        defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    String defaultFileName = doc.Title;
                    if (String.IsNullOrEmpty(defaultFileName))
                    {
                        defaultFileName = "Project";
                    }
                    else
                    {
                        defaultFileName = Path.GetFileNameWithoutExtension(defaultFileName);
                    }
                    String defaultExtension = mainWindow.GetFileExtension();

                    fileDialog.FileName = defaultFileName;
                    fileDialog.DefaultExt = defaultExtension;
                    fileDialog.Filter = mainWindow.GetFileFilter();
                    fileDialog.InitialDirectory = defaultDirectory;
                    bool? fileDialogResult = fileDialog.ShowDialog();
                
                    // If user chose to continue
                    if (fileDialogResult.HasValue && fileDialogResult.Value)
                    {
                        // Prompt the user for the file location and path
                        String fullName = fileDialog.FileName;
                        String path = Path.GetDirectoryName(fullName);
                        String fileName = Path.GetFileName(fullName);

                        // IFC export requires an open transaction, although no changes should be made
                        Transaction transaction = new Transaction(doc, "Export IFC");   
                        transaction.Start();
                        FailureHandlingOptions failureOptions = transaction.GetFailureHandlingOptions();
                        failureOptions.SetClearAfterRollback(false);
                        transaction.SetFailureHandlingOptions(failureOptions);

                        // There is no UI option for this, but these two options can be useful for debugging/investigating
                        // issues in specific file export.  The first one supports export of only one element
                        //exportOptions.AddOption("SingleElement", "174245");
                        // The second one supports export only of a list of elements
                        //exportOptions.AddOption("ElementsForExport", "174245;205427");

                        bool result = doc.Export(path, fileName, exportOptions); // pass in the options here
                   
                        if (!result)
                        {
                            //TODO localization
                            TaskDialog taskDialog = new TaskDialog("Error exporting IFC file");
                            taskDialog.MainInstruction = "The IFC export process encountered an error.";
                            taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                            taskDialog.Show();
                        }

                        // Always roll back the transaction started earlier
                        transaction.RollBack();

                        // Remember last successful export location
                        m_mruExportPath = path;    
                    }      
                }
                if (mainWindow.Result == IFCExportResult.SaveSettings || mainWindow.Result == IFCExportResult.ExportAndSaveSettings)
                {
                    configurationsMap = mainWindow.GetModifiedConfigurations();
                    configurationsMap.UpdateSavedConfigurations(doc);

                    // Remember last selected configuration
                    m_mruConfiguration = mainWindow.GetSelectedConfiguration().Name;
                }
            }
            catch (Exception e)
            {
                //TODO localization
                TaskDialog taskDialog = new TaskDialog("Error exporting IFC file");
                taskDialog.MainInstruction = "The IFC export process encountered an error.";
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                taskDialog.ExpandedContent = e.ToString();
                taskDialog.Show();
            }
        }

        // If user canceled either dialog, a silent return (no notification or logging needed)
    }
}
