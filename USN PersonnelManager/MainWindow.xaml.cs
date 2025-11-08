using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace USNPersonnelManager
{
    public partial class MainWindow : Window
    {
        private string currentDataFile = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSubWindow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFile))
            {
                MessageBox.Show("Please load a personnel JSON file first.", "No File Selected",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubWindow subWindow = new SubWindow(currentDataFile);
            subWindow.ShowDialog();
            LoadPersonnelData();
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Personnel JSON File",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    currentDataFile = openFileDialog.FileName;
                    LoadPersonnelData();
                    MessageBox.Show($"File loaded successfully:\n{currentDataFile}", "Load Complete",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load file: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadPersonnelData()
        {
            if (string.IsNullOrEmpty(currentDataFile) || !File.Exists(currentDataFile))
                return;

            try
            {
                string json = File.ReadAllText(currentDataFile);
                var data = JsonSerializer.Deserialize<List<PersonnelInfo>>(json);
                if (data == null)
                    return;

                PersonnelList.Children.Clear();

                foreach (var person in data)
                {
                    var expander = new Expander
                    {
                        Header = person.Name,
                        Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                        Foreground = GetRankColor(person.Rank),
                        FontSize = 18,
                        Margin = new Thickness(0, 5, 0, 5),
                        Padding = new Thickness(10),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                        BorderThickness = new Thickness(1)
                    };
                    var stack = new StackPanel { Margin = new Thickness(15, 5, 0, 5) };
                    stack.Children.Add(new TextBlock { Text = $"LOA Status: {person.LOAStatus}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"LOA Ends: {(person.LOAEndDate.HasValue ? $"{person.LOAEndDate:dd-MM-yyyy} ({(person.LOAEndDate.Value - DateTime.Now).Days} days left)" : "N/A")}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"Joined Server: {FormatDate(person.ServerJoinDate)}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"Last Promotion: {FormatDate(person.LastPromotionDate)}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"Last Voyage: {FormatDate(person.LastVoyageDate)}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"Last Voyage Hosted: {FormatDate(person.LastVoyageHostedDate)}", Foreground = Brushes.White });
                    stack.Children.Add(new TextBlock { Text = $"Guild Member: {(!string.IsNullOrEmpty(person.GuildMember) ? person.GuildMember : "No")}", Foreground = Brushes.White });

                    expander.Content = stack;
                    PersonnelList.Children.Add(expander);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private SolidColorBrush GetRankColor(string rank)
        {
            switch (rank)
            {
                case "O-1":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2709A"));
                case "O-3":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3242C"));
                case "O-4":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DF2A02"));
                case "O-5":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DF2A02"));
                case "O-6":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DA861D"));
                case "O-7": // ⭐
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                case "O-8": // ⭐⭐
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                case "O-9": // ⭐⭐⭐
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                case "O-10": // ⭐⭐⭐⭐
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));

                case "E-8":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B051D3"));
                case "E-7":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d1a6e3"));
                case "E-6":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4f81d8"));
                case "E-4":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27afdd"));
                case "E-3":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e8029"));
                case "E-2":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64c54b"));
                default:
                    return Brushes.White;
            }
        }

        private string FormatDate(DateTime? date)
        {
            if (date == null)
                return "N/A";

            var daysAgo = (DateTime.Now - date.Value).Days;
            return $"{date.Value:dd-MM-yyyy} ({daysAgo} days ago)";
        }

        private void AddSailorButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDataFile))
            {
                MessageBox.Show("Please load a personnel JSON file first.", "No File Selected",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddSailorWindow { Owner = this };
            if (addWindow.ShowDialog() == true)
            {
                AddSailor(addWindow.SailorName, addWindow.Rank, "OFF LOA", addWindow.ServerJoinDate);
            }
        }

        private void AddSailor(string name, string rank, string loaStatus,
                               DateTime? joinDate = null, DateTime? lastPromotion = null,
                               DateTime? lastVoyage = null, DateTime? lastVoyageHosted = null)
        {
            if (string.IsNullOrEmpty(currentDataFile))
            {
                MessageBox.Show("Please load a personnel JSON file first.", "No File Selected",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<PersonnelInfo> personnelList = new List<PersonnelInfo>();
            if (File.Exists(currentDataFile))
            {
                string json = File.ReadAllText(currentDataFile);
                personnelList = JsonSerializer.Deserialize<List<PersonnelInfo>>(json) ?? new List<PersonnelInfo>();
            }

            if (personnelList.Exists(p => p.Name == name))
            {
                MessageBox.Show($"{name} already exists in the roster.", "Duplicate Sailor",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            personnelList.Add(new PersonnelInfo
            {
                Name = name,
                Rank = rank,
                LOAStatus = loaStatus,
                ServerJoinDate = joinDate ?? DateTime.Now,
                LastPromotionDate = lastPromotion,
                LastVoyageDate = lastVoyage,
                LastVoyageHostedDate = lastVoyageHosted
            });

            string updatedJson = JsonSerializer.Serialize(personnelList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(currentDataFile, updatedJson);

            MessageBox.Show($"{name} added successfully!", "Sailor Added",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            LoadPersonnelData();
        }
    }

    public class PersonnelInfo
    {
        public string Name { get; set; }
        public string Rank { get; set; }
        public string LOAStatus { get; set; }
        public DateTime? ServerJoinDate { get; set; }
        public DateTime? LastPromotionDate { get; set; }
        public DateTime? LastVoyageDate { get; set; }
        public DateTime? LastVoyageHostedDate { get; set; }
        public DateTime? LOAEndDate { get; set; }
        public string GuildMember { get; set; }
    }

}