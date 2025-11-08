using System;
using System.Windows;
using System.Windows.Controls;

namespace USNPersonnelManager
{
    public partial class AddSailorWindow : Window
    {
        public string SailorName { get; private set; }
        public string Rank { get; private set; }
        public string LOAStatus { get; private set; }
        public DateTime? ServerJoinDate { get; private set; }

        public AddSailorWindow()
        {
            InitializeComponent();
            RankComboBox.SelectedIndex = 0;
            JoinDatePicker.SelectedDate = DateTime.Now;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SailorName = NameTextBox.Text.Trim();
            Rank = (RankComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            ServerJoinDate = JoinDatePicker.SelectedDate;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}