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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace BIM.IFC.Export.UI
{
    /// <summary>
    /// Interaction logic for IFCExport.xaml
    /// </summary>
    public partial class IFCExport : Window
    {
        // The list of available configurations
        IFCExportConfigurationsMap m_configMap;

        /// <summary>
        /// The dialog result.
        /// </summary>
        IFCExportResult m_result = IFCExportResult.Invalid;

        /// <summary>
        /// The file to store the previous window bounds.
        /// </summary>
        string m_SettingFile = "IFCExportSettings.txt";

        /// <summary>
        /// Construction of the main export dialog.
        /// </summary>
        /// <param name="configurationsMap">The configurations to show in the dialog.</param>
        /// <param name="selectedConfigName">The current selected configuration name.</param>
        public IFCExport(IFCExportConfigurationsMap configurationsMap, String selectedConfigName)
        {
            m_configMap = configurationsMap;

            InitializeComponent();

            RestorePreviousWindow();

            UpdateCurrentSelectedSetupCombo(selectedConfigName);
       }

        /// <summary>
        /// Restores the previous window. If no previous window found, place on the left top.
        /// </summary>
        private void RestorePreviousWindow()
        {
            // Refresh restore bounds from previous window opening
            Rect restoreBounds = IFCUISettings.LoadWindowBounds(m_SettingFile);
            if (restoreBounds != new Rect())
            {
                this.Left = restoreBounds.Left;
                this.Top = restoreBounds.Top;
                this.Width = restoreBounds.Width;
                this.Height = restoreBounds.Height;
            }       
        }

       /// <summary>
       /// Update the current selected configuration in the combobox. 
       /// </summary>
       /// <param name="selected">The name of selected configuration.</param>
       private void UpdateCurrentSelectedSetupCombo(String selected)
       {
            // TODO: support additional user saved configurations.

            foreach (IFCExportConfiguration curr in m_configMap.Values)
            {
                currentSelectedSetup.Items.Add(curr.Name);  
            }
            if (selected == null || !currentSelectedSetup.Items.Contains(selected))
                currentSelectedSetup.SelectedIndex = 0;
            else
                currentSelectedSetup.SelectedItem = selected;
        }

        /// <summary>
        /// Add a configuration to the map list to show in dialog.
        /// </summary>
        /// <param name="configuration">The configuration to add.</param>
        private void AddToConfigList(IFCExportConfiguration configuration)
        {
            m_configMap.Add(configuration);
        }

        /// <summary>
        /// The dialog result for continue or cancel.
        /// </summary>
        public IFCExportResult Result
        {
            get { return m_result; }
        }

        /// <summary>
        /// Returns the configuration map.
        /// </summary>
        /// <returns>The configuration map.</returns>
        public IFCExportConfigurationsMap GetModifiedConfigurations()
        {
            return m_configMap;
        }

        /// <summary>
        /// Returns the selected configuration.
        /// </summary>
        /// <returns>The selected configuration.</returns>
        public IFCExportConfiguration GetSelectedConfiguration()
        {        
            String selectedConfigName = (String)currentSelectedSetup.SelectedItem;
            if (selectedConfigName == null)
                return null;
            
            return m_configMap[selectedConfigName];
        }

        /// <summary>
        /// Returns the name of selected configuration.
        /// </summary>
        /// <returns>The name of selected configuration.</returns>
        public String GetSelectedConfigurationName()
        {
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration == null)
                return null;
            return configuration.Name;
        }

        /// <summary>
        /// Gets the file extension from selected configuration.
        /// </summary>
        /// <returns>The file extension of selected configuration.</returns>
        public String GetFileExtension()
        {
            IFCExportConfiguration selectedConfig = GetSelectedConfiguration();
            IFCFileFormatAttributes selectedItem = new IFCFileFormatAttributes(selectedConfig.IFCFileType);
            return selectedItem.GetFileExtension();
        }

        /// <summary>
        /// Gets the file filters for save dialog.
        /// </summary>
        /// <returns>The file filter.</returns>
        public String GetFileFilter()
        {
            IFCExportConfiguration selectedConfig = GetSelectedConfiguration();
            IFCFileFormatAttributes selectedItem = new IFCFileFormatAttributes(selectedConfig.IFCFileType);
            return selectedItem.GetFileFilter();
        }

        /// <summary>
        /// Shows the IFC export setup window when clicking the buttonEditSetup.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void buttonEditSetup_Click(object sender, RoutedEventArgs args)
        {
            IFCExportConfiguration selectedConfig = GetSelectedConfiguration();
            IFCExportConfigurationsMap configurationsMap = new IFCExportConfigurationsMap(m_configMap);
            IFCExporterUIWindow editorWindow = new IFCExporterUIWindow(configurationsMap, selectedConfig.Name);
            editorWindow.ShowDialog();
            if (editorWindow.DialogResult.HasValue && editorWindow.DialogResult.Value)
            {
                currentSelectedSetup.Items.Clear();
                m_configMap = configurationsMap;
                String selectedConfigName = editorWindow.GetSelectedConfigurationName();
                UpdateCurrentSelectedSetupCombo(selectedConfigName);
            }
        }

        /// <summary>
        /// Sets the dialog result when clicking the Next button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void buttonNext_Click(object sender, RoutedEventArgs args)
        {
            m_result = IFCExportResult.ExportAndSaveSettings;
            Close();
        }

        /// <summary>
        /// Sets the dialog result when clicking the Cancel button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void buttonCancel_Click(object sender, RoutedEventArgs args)
        {
            m_result = IFCExportResult.Cancel;
            Close();
        }

        /// <summary>
        /// Sets the dialog result when clicking the Save Setup & Close button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void buttonSaveSetAndClose_Click(object sender, RoutedEventArgs args)
        {
            m_result = IFCExportResult.SaveSettings;
            Close();
        }

        /// <summary>
        /// Updates the description when current configuration change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event arguments that contains the event data.</param>
        private void currentSelectedSetup_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            IFCExportConfiguration selectedConfig = GetSelectedConfiguration();

            if (selectedConfig != null)
            {
                // change description
                textBoxSetupDescription.Text = selectedConfig.Description;  
            }  
        }

        /// <summary>
        /// Saves the window bounds when close the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save restore bounds for the next time this window is opened
            IFCUISettings.SaveWindowBounds(m_SettingFile, this.RestoreBounds);
        }
    }
}
