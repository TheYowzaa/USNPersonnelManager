using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace USNPersonnelManager
{
    public partial class SubWindow : Window
    {
        private readonly string dataFile;

        public SubWindow(string selectedFile)
        {
            InitializeComponent();
            dataFile = selectedFile;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPersonnel = (PersonnelDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedPersonnel))
            {
                MessageBox.Show("Please select a personnel to update.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personnelList = LoadPersonnelData();
            var existing = personnelList.Find(p => p.Name == selectedPersonnel);

            if (existing == null)
            {
                MessageBox.Show("Selected personnel does not exist in the roster.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Update only fields that have a selection
            if (RankDropdown.SelectedItem != null)
                existing.Rank = (RankDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? existing.Rank;

            if (LOAStatusDropdown.SelectedItem != null)
                existing.LOAStatus = (LOAStatusDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? existing.LOAStatus;

            if (JoinDatePicker.SelectedDate.HasValue)
                existing.ServerJoinDate = JoinDatePicker.SelectedDate.Value;

            if (LastPromotionDatePicker.SelectedDate.HasValue)
                existing.LastPromotionDate = LastPromotionDatePicker.SelectedDate.Value;

            if (LastVoyageDatePicker.SelectedDate.HasValue)
                existing.LastVoyageDate = LastVoyageDatePicker.SelectedDate.Value;

            if (LastVoyageHostedDatePicker.SelectedDate.HasValue)
                existing.LastVoyageHostedDate = LastVoyageHostedDatePicker.SelectedDate.Value;

            SavePersonnelData(personnelList);

            MessageBox.Show($"Updated info saved for {selectedPersonnel}!", "Update Saved",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private List<PersonnelInfo> LoadPersonnelData()
        {
            if (!File.Exists(dataFile))
                return new List<PersonnelInfo>();

            try
            {
                string json = File.ReadAllText(dataFile);
                return JsonSerializer.Deserialize<List<PersonnelInfo>>(json) ?? new List<PersonnelInfo>();
            }
            catch
            {
                return new List<PersonnelInfo>();
            }
        }

        private void SavePersonnelData(List<PersonnelInfo> data)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save data: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}