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

            // Populate the dropdown dynamically
            LoadPersonnelDropdown();
        }

        private void LoadPersonnelDropdown()
        {
            PersonnelDropdown.Items.Clear();

            var personnelList = LoadPersonnelData();

            foreach (var person in personnelList)
            {
                PersonnelDropdown.Items.Add(new ComboBoxItem
                {
                    Content = person.Name
                });
            }

            if (PersonnelDropdown.Items.Count > 0)
                PersonnelDropdown.SelectedIndex = 0;
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

            // Update all editable fields
            if (RankDropdown.SelectedItem != null)
                existing.Rank = (RankDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? existing.Rank;

            if (LOAStatusDropdown.SelectedItem != null)
                existing.LOAStatus = (LOAStatusDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? existing.LOAStatus;

            if (LOAEndDatePicker.SelectedDate.HasValue)
                existing.LOAEndDate = LOAEndDatePicker.SelectedDate.Value;

            if (JoinDatePicker.SelectedDate.HasValue)
                existing.ServerJoinDate = JoinDatePicker.SelectedDate.Value;

            if (LastPromotionDatePicker.SelectedDate.HasValue)
                existing.LastPromotionDate = LastPromotionDatePicker.SelectedDate.Value;

            if (LastVoyageDatePicker.SelectedDate.HasValue)
                existing.LastVoyageDate = LastVoyageDatePicker.SelectedDate.Value;

            if (LastVoyageHostedDatePicker.SelectedDate.HasValue)
                existing.LastVoyageHostedDate = LastVoyageHostedDatePicker.SelectedDate.Value;

            if (GuildComboBox.SelectedItem != null)
                existing.GuildMember = (GuildComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? existing.GuildMember;

            existing.SPDMemberships = new List<string>();
            foreach (ListBoxItem item in SPDMembershipListBox.SelectedItems)
            {
                existing.SPDMemberships.Add(item.Content.ToString());
            }

            SavePersonnelData(personnelList);

            MessageBox.Show($"Updated info saved for {selectedPersonnel}!", "Update Saved",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            // Refresh dropdown in case names changed (optional)
            LoadPersonnelDropdown();

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TerminateButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPersonnel = (PersonnelDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedPersonnel))
            {
                MessageBox.Show("Please select a personnel to terminate.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to terminate {selectedPersonnel}?",
                                         "Confirm Termination",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var personnelList = LoadPersonnelData();
            var removed = personnelList.RemoveAll(p => p.Name == selectedPersonnel);
            if (removed > 0)
            {
                SavePersonnelData(personnelList);
                MessageBox.Show($"{selectedPersonnel} has been terminated.", "Termination Complete",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPersonnelDropdown(); // Refresh dropdown
                this.Close();
            }
            else
            {
                MessageBox.Show("Personnel not found in the roster.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearVoyagesButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPersonnel = (PersonnelDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedPersonnel))
            {
                MessageBox.Show("Please select a personnel.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to clear the last voyage date for {selectedPersonnel}?",
                                         "Confirm Clear",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var personnelList = LoadPersonnelData();
            var person = personnelList.Find(p => p.Name == selectedPersonnel);

            if (person != null)
            {
                person.LastVoyageDate = null;
                SavePersonnelData(personnelList);
                MessageBox.Show($"Last voyage date cleared for {selectedPersonnel}.",
                                "Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearVoyagesHostedButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPersonnel = (PersonnelDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedPersonnel))
            {
                MessageBox.Show("Please select a personnel.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to clear the last voyage hosted date for {selectedPersonnel}?",
                                         "Confirm Clear",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var personnelList = LoadPersonnelData();
            var person = personnelList.Find(p => p.Name == selectedPersonnel);

            if (person != null)
            {
                person.LastVoyageHostedDate = null;
                SavePersonnelData(personnelList);
                MessageBox.Show($"Last voyage hosted date cleared for {selectedPersonnel}.",
                                "Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}