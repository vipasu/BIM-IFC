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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace BIM.IFC.Export.UI
{
    /// <summary>
    /// The IFC export UI options window.
    /// </summary>
    public partial class IFCExporterUIWindow : Window
    {
        /// <summary>
        /// The map contains the configurations.
        /// </summary>
        IFCExportConfigurationsMap m_configurationsMap;

        /// <summary>
        /// The file to store the previous window bounds.
        /// </summary>
        string m_SettingFile = "IFCExporterUIWindowSettings.txt";

        /// <summary>
        /// Constructs a new IFC export options window.
        /// </summary>
        /// <param name="exportOptions">The export options that will be populated by settings in the window.</param>
        /// <param name="currentViewId">The Revit current view id.</param>
        public IFCExporterUIWindow(IFCExportConfigurationsMap configurationsMap, String currentConfigName)
        {
            InitializeComponent();

            RestorePreviousWindow();

            m_configurationsMap = configurationsMap;

            InitializeConfigurationList(currentConfigName);

            IFCExportConfiguration originalConfiguration = m_configurationsMap[currentConfigName];
            InitializeConfigurationOptions();
            UpdateActiveConfigurationOptions(originalConfiguration);
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
        /// Initializes the listbox by filling the available configurations from the map.
        /// </summary>
        /// <param name="currentConfigName">The current configuration name.</param>
        private void InitializeConfigurationList(String currentConfigName)
        {
            foreach (IFCExportConfiguration configuration in m_configurationsMap.Values)
            {
                listBoxConfigurations.Items.Add(configuration);
                if (configuration.Name == currentConfigName)
                    listBoxConfigurations.SelectedItem = configuration;
            }
        }

        /// <summary>
        /// Updates and resets the listbox.
        /// </summary>
        /// <param name="currentConfigName">The current configuration name.</param>
        private void UpdateConfigurationsList(String currentConfigName)
        {
            listBoxConfigurations.Items.Clear();
            InitializeConfigurationList(currentConfigName);
        }

        /// <summary>
        /// Initializes the comboboxes via the configuration options.
        /// </summary>
        private void InitializeConfigurationOptions()
        {
            comboboxIfcType.Items.Add(new IFCVersionAttributes(IFCVersion.IFC2x2));
            comboboxIfcType.Items.Add(new IFCVersionAttributes(IFCVersion.IFC2x3));
            comboboxIfcType.Items.Add(new IFCVersionAttributes(IFCVersion.IFC2x3CV2));
            comboboxIfcType.Items.Add(new IFCVersionAttributes(IFCVersion.IFCCOBIE));
            comboboxIfcType.Items.Add(new IFCVersionAttributes(IFCVersion.IFCBCA));
            // TODO - how to handle IFC4?

            foreach (IFCFileFormat fileType in Enum.GetValues(typeof(IFCFileFormat)))
            {
                IFCFileFormatAttributes item = new IFCFileFormatAttributes(fileType);
                comboboxFileType.Items.Add(item);
            }

            for (int level = 0; level <= 2; level++)
            {
                IFCSpaceBoundariesAttributes item = new IFCSpaceBoundariesAttributes(level);
                comboboxSpaceBoundaries.Items.Add(item);
            }
        }

        /// <summary>
        /// Updates the active configuration options to the controls.
        /// </summary>
        /// <param name="configuration">The active configuration.</param>
        private void UpdateActiveConfigurationOptions(IFCExportConfiguration configuration)
        {
            foreach (IFCVersionAttributes attribute in comboboxIfcType.Items.Cast<IFCVersionAttributes>())
            {
                if (attribute.Version == configuration.IFCVersion)
                {
                    comboboxIfcType.SelectedItem = attribute;
                }
            }

            foreach (IFCFileFormatAttributes format in comboboxFileType.Items.Cast<IFCFileFormatAttributes>())
            {
                if (configuration.IFCFileType == format.FileType)
                    comboboxFileType.SelectedItem = format;
            }

            foreach (IFCSpaceBoundariesAttributes attribute in comboboxSpaceBoundaries.Items.Cast<IFCSpaceBoundariesAttributes>())
            {
                if (configuration.SpaceBoundaries == attribute.Level)
                    comboboxSpaceBoundaries.SelectedItem = attribute;
            }

            checkboxExportBaseQuantities.IsChecked = configuration.ExportBaseQuantities;
            checkboxSplitWalls.IsChecked = configuration.SplitWallsAndColumns;
            checkbox2dElements.IsChecked = configuration.Export2DElements;
            checkboxInternalPropertySets.IsChecked = configuration.ExportInternalRevitPropertySets;
            checkboxVisibleElementsCurrView.IsChecked = configuration.VisibleElementsOfCurrentView;
            checkBoxUse2DRoomVolumes.IsChecked = configuration.Use2DRoomBoundaryForVolume;
            checkBoxFamilyAndTypeName.IsChecked = configuration.UseFamilyAndTypeNameForReference;
            checkBoxExportPartsAsBuildingElements.IsChecked = configuration.ExportPartsAsBuildingElements;

            UIElement[] configurationElements = new UIElement[]{comboboxIfcType, 
                                                                comboboxFileType, 
                                                                comboboxSpaceBoundaries, 
                                                                checkboxExportBaseQuantities,
                                                                checkboxSplitWalls,
                                                                checkbox2dElements,
                                                                checkboxInternalPropertySets,
                                                                checkboxVisibleElementsCurrView,
                                                                checkBoxUse2DRoomVolumes,
                                                                checkBoxFamilyAndTypeName,
                                                                checkBoxExportPartsAsBuildingElements};
            foreach (UIElement element in configurationElements)
            {
                element.IsEnabled = !configuration.IsBuiltIn;
            }
        }

        /// <summary>
        /// Updates the controls.
        /// </summary>
        /// <param name="isBuiltIn">Value of whether the configuration is builtIn or not.</param>
        /// <param name="isInSession">Value of whether the configuration is in-session or not.</param>
        private void UpdateConfigurationControls(bool isBuiltIn, bool isInSession)
        {
            buttonDeleteSetup.IsEnabled = !isBuiltIn && !isInSession;
            buttonRenameSetup.IsEnabled = !isBuiltIn && !isInSession;
        }

        /// <summary>
        /// Helper method to convert CheckBox.IsChecked to usable bool.
        /// </summary>
        /// <param name="checkBox">The check box.</param>
        /// <returns>True if the box is checked, false if unchecked or uninitialized.</returns>
        private bool GetCheckbuttonChecked(CheckBox checkBox)
        {
            if (checkBox.IsChecked.HasValue)
                return checkBox.IsChecked.Value;
            return false;
        }

        /// <summary>
        /// The OK button callback.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // close the window
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancel button callback.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            // close the window
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Remove a configuration from the listbox and the map.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void buttonDeleteSetup_Click(object sender, RoutedEventArgs e)
        {
            IFCExportConfiguration configuration = (IFCExportConfiguration)listBoxConfigurations.SelectedItem;
            m_configurationsMap.Remove(configuration.Name);
            listBoxConfigurations.Items.Remove(configuration);
            listBoxConfigurations.SelectedIndex = 0;
        }

        /// <summary>
        /// Shows the rename control and updates with the results.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void buttonRenameSetup_Click(object sender, RoutedEventArgs e)
        {
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            String oldName = configuration.Name;
            RenameExportSetupWindow renameWindow = new RenameExportSetupWindow(m_configurationsMap, oldName);
            renameWindow.Owner = this;
            renameWindow.ShowDialog();
            if (renameWindow.DialogResult.HasValue && renameWindow.DialogResult.Value)
            {
                String newName = renameWindow.GetName();
                configuration.Name = newName;
                m_configurationsMap.Remove(oldName);
                m_configurationsMap.Add(configuration);
                UpdateConfigurationsList(newName);
            }
        }

        /// <summary>
        /// Shows the duplicate control and updates with the results.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void buttonDuplicateSetup_Click(object sender, RoutedEventArgs e)
        {
            String name = GetDuplicateSetupName(null);
            NewExportSetupWindow nameWindow = new NewExportSetupWindow(m_configurationsMap, name);
            nameWindow.Owner = this;
            nameWindow.ShowDialog();
            if (nameWindow.DialogResult.HasValue && nameWindow.DialogResult.Value)
            {
                CreateNewEditableConfiguration(GetSelectedConfiguration(), nameWindow.GetName());
            }
        }

        /// <summary>
        /// Shows the new setup control and updates with the results.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void buttonNewSetup_Click(object sender, RoutedEventArgs e)
        {
            String name = GetNewSetupName();
            NewExportSetupWindow nameWindow = new NewExportSetupWindow(m_configurationsMap, name);
            nameWindow.Owner = this;
            nameWindow.ShowDialog();
            if (nameWindow.DialogResult.HasValue && nameWindow.DialogResult.Value)
            {
                CreateNewEditableConfiguration(null, nameWindow.GetName());
            }
        }

        /// <summary>
        /// Gets the new setup name.
        /// </summary>
        /// <returns>The new setup name.</returns>
        private String GetNewSetupName()
        {
            return GetFirstIncrementalName("Setup");
        }

        /// <summary>
        /// Gets the new duplicated setup name.
        /// </summary>
        /// <param name="configuration">The configuration to duplicate.</param>
        /// <returns>The new duplicated setup name.</returns>
        private String GetDuplicateSetupName(IFCExportConfiguration configuration)
        {
            if (configuration == null)
                configuration = GetSelectedConfiguration();
            return GetFirstIncrementalName(configuration.Name);
        }

        /// <summary>
        /// Gets the new incremental name for configuration.
        /// </summary>
        /// <param name="nameRoot">The name of a configuration.</param>
        /// <returns>the new incremental name for configuration.</returns>
        private String GetFirstIncrementalName(String nameRoot)
        {
            bool found = true;
            int number = 0;
            String newName = "";
            do
            {
                number++;
                newName = nameRoot + " " + number;
                if (!m_configurationsMap.HasName(newName))
                    found = false;
            }
            while (found);

            return newName;
        }

        

        /// <summary>
        /// Creates a new configuration, either a default or a copy configuration.
        /// </summary>
        /// <param name="configuration">The specific configuration, null to create a defult configuration.</param>
        /// <param name="name">The name of the new configuration.</param>
        /// <returns>The new configuration.</returns>
        private IFCExportConfiguration CreateNewEditableConfiguration(IFCExportConfiguration configuration, String name)
        {
            // create new configuration based on input, or default configuration.
            IFCExportConfiguration newConfiguration;
            if (configuration == null)
            {
                newConfiguration = IFCExportConfiguration.CreateDefaultConfiguration();
                newConfiguration.Name = name;
            }
            else
                newConfiguration = configuration.Duplicate(name);
            m_configurationsMap.Add(newConfiguration);

            // set new configuration as selected
            listBoxConfigurations.Items.Add(newConfiguration);
            listBoxConfigurations.SelectedItem = newConfiguration;

            return configuration;
        }

        /// <summary>
        /// Gets the selected configuration from the list box.
        /// </summary>
        /// <returns>The selected configuration.</returns>
        private IFCExportConfiguration GetSelectedConfiguration()
        {
            IFCExportConfiguration configuration = (IFCExportConfiguration)listBoxConfigurations.SelectedItem;
            return configuration;
        }

        /// <summary>
        /// Gets the name of selected configuration.
        /// </summary>
        /// <returns>The selected configuration name.</returns>
        public String GetSelectedConfigurationName()
        {
            return GetSelectedConfiguration().Name;
        }

        /// <summary>
        /// Updates the controls after listbox selection changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void listBoxConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                UpdateActiveConfigurationOptions(configuration);
                UpdateConfigurationControls(configuration.IsBuiltIn, configuration.IsInSession);
            }
        }

        /// <summary>
        /// Updates the result after the ExportBaseQuantities is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkboxExportBaseQuantities_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.ExportBaseQuantities = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the SplitWalls is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkboxSplitWalls_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.SplitWallsAndColumns = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the InternalPropertySets is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkboxInternalPropertySets_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.ExportInternalRevitPropertySets = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the 2dElements is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkbox2dElements_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.Export2DElements = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the VisibleElementsCurrView is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkboxVisibleElementsCurrView_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.VisibleElementsOfCurrentView = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the Use2DRoomVolumes is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkBoxUse2DRoomVolumes_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.Use2DRoomBoundaryForVolume = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the result after the FamilyAndTypeName is picked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkBoxFamilyAndTypeName_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.UseFamilyAndTypeNameForReference = GetCheckbuttonChecked(checkBox);
            }
        }

        /// <summary>
        /// Updates the configuration IFCVersion when IFCType changed in the combobox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void comboboxIfcType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IFCVersionAttributes attributes = (IFCVersionAttributes)comboboxIfcType.SelectedItem;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.IFCVersion = attributes.Version;
            }
        }

        /// <summary>
        /// Updates the configuration IFCFileType when FileType changed in the combobox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void comboboxFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IFCFileFormatAttributes attributes = (IFCFileFormatAttributes)comboboxFileType.SelectedItem;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.IFCFileType = attributes.FileType;
            }
        }

        /// <summary>
        /// Updates the configuration SpaceBoundaries when the space boundary level changed in the combobox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void comboboxSpaceBoundaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IFCSpaceBoundariesAttributes attributes = (IFCSpaceBoundariesAttributes)comboboxSpaceBoundaries.SelectedItem;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.SpaceBoundaries = attributes.Level;
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

        /// <summary>
        /// Updates the configuration ExportPartsAsBuildingElements when the Export separate parts changed in the combobox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void checkBoxExportPartsAsBuildingElements_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            IFCExportConfiguration configuration = GetSelectedConfiguration();
            if (configuration != null)
            {
                configuration.ExportPartsAsBuildingElements = GetCheckbuttonChecked(checkBox);
            }
        }

    }
}
