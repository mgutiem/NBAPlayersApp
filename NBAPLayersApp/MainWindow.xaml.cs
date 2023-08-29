using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// NBA players App
/// by: Miguel Gutierrez
/// </summary>
namespace NBAPLayersApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ApiBaseUrl = "https://www.balldontlie.io/api/v1/players";
        private int currentPage = 1;
        private int totalPages = 1;
        private int pageSize = 50; // Number of players per page 
        private List<Player> players;
        public MainWindow()
        {
            InitializeComponent();
            LoadPlayers();
        }

        private async void LoadPlayers()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Send a GET request to the API to retrieve players for the current page and specified page size
                    var response = await httpClient.GetAsync($"{ApiBaseUrl}?page={currentPage}&per_page={pageSize}");
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a JSON string
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON string into an ApiResponse object using JsonConvert from Newtonsoft.Json
                    var result = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);

                    // Update the total pages and player list based on the retrieved data
                    totalPages = result.Meta.TotalPages;
                    players = result.Data;

                    // Update UI
                    UpdateComboBox(); // Update the position filter combo box
                    UpdatePlayerListBox(); // Update the player list box
                    UpdatePaginationButtons(); // Update the pagination buttons
                }
            }
            catch (Exception ex)
            {
                // Display an error message box if an exception occurs during loading of players
                MessageBox.Show($"Error loading players: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateComboBox()
        {
            // Get the positions of players, excluding null or empty positions, and make them distinct
            var positions = players
                .Select(p => p.Position)
                .Where(pos => !string.IsNullOrEmpty(pos))
                .Distinct()
                .ToList();

            positions.Insert(0, "All players"); // Insert "All players" option at the beginning
            comboBoxPositions.ItemsSource = positions; // Set the position filter combo box data source
            comboBoxPositions.SelectedIndex = 0; // Select the "All players" option by default
        }

        
        private int GetGlobalIndex(int localIndex)
        {
            //calculates the global index based on the current page and the local index within the filtered players
            return (currentPage - 1) * pageSize + localIndex + 1;
        }

        private void UpdatePlayerListBox()
        {
            var selectedPosition = comboBoxPositions.SelectedItem as string;

            var filteredPlayers = players;
            if (selectedPosition != "All players")
                filteredPlayers = players.Where(p => p.Position == selectedPosition).ToList();

            listBoxPlayers.DisplayMemberPath = "DisplayName"; // Set the appropriate property name for display

            // Generate the display name with an increasing number before the player's full name
            var playersWithDisplayName = filteredPlayers
                .Select((p, index) => new { Index = GetGlobalIndex(index), FullName = $"{p.FirstName} {p.LastName}" })
                .Select(p => new { DisplayName = $"{p.Index}. {p.FullName}" });

            listBoxPlayers.ItemsSource = playersWithDisplayName;
        }

        private void UpdatePaginationButtons()
        {
            // Enable or disable the next button based on the current page and total pages
            buttonNext.IsEnabled = currentPage < totalPages;

            // Enable or disable the previous button based on the current page
            buttonPrevious.IsEnabled = currentPage > 1;
        }

        private void comboBoxPositions_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Update the player list box when the selected position in the combo box changes
            UpdatePlayerListBox();
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            currentPage++; // Increment the current page
            LoadPlayers(); // Load players for the updated page
        }

        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            currentPage--; // Decrement the current page
            LoadPlayers(); // Load players for the updated page
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ApiResponse
    {
        [JsonProperty("data")]
        public List<Player> Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class Player
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("team")]
        public Team Team { get; set; }
    }

    public class Meta
    {
        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    public class Team
    {
        [JsonProperty("full_name")]
        public string FullName { get; set; }
    }
}
