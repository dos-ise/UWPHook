using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Net;
using UWPHook.SteamGridDb;

namespace UWPHook
{
    public partial class SelectImagesWindow : Window
    {
        private SteamGridDb.SteamGridDbApi _api;
        private Dictionary<GameResponse, BitmapImage> gameImageMap = new Dictionary<GameResponse, BitmapImage>();

        public SelectImagesWindow(SteamGridDb.SteamGridDbApi api)
        {
            InitializeComponent();
            _api = api;
        }

        public async Task LoadImages(SteamGridDb.GameResponse[] games)
        {
            string tmpGridDirectory = System.IO.Path.GetTempPath() + "UWPHook\\tmp_grid\\";
            foreach (var game in games)
            {
                var gameGridsVertical = await _api.GetGameGrids(game.Id, "600x900,342x482,660x930");

                // Check if the returned list is empty
                if (gameGridsVertical.Length == 0)
                {
                    continue; // Skip this game if no grids are found
                }

                string filePath = $"{tmpGridDirectory}{game.Name}p.png";

                await SaveImage(gameGridsVertical[0].Url, filePath, System.Drawing.Imaging.ImageFormat.Png);

                var imageUri = new Uri(filePath);
                var bitmapImage = new BitmapImage(imageUri);
                gameImageMap.Add(game, bitmapImage);

                imageListBox.Items.Add(bitmapImage);
            }
        }


        private async Task SaveImage(string imageUrl, string destinationFilename, System.Drawing.Imaging.ImageFormat format)
        {
            await Task.Run(() =>
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(imageUrl))
                    {
                        if (stream != null)
                        {
                            using (var bitmap = new System.Drawing.Bitmap(stream))
                            {
                                bitmap.Save(destinationFilename, format);
                            }
                        }
                    }
                }
            });
        }

        private void imageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedImage = imageListBox.SelectedItem as BitmapImage;
            if (selectedImage != null)
            {
                // Find the corresponding GameResponse
                foreach (var kvp in gameImageMap)
                {
                    if (kvp.Value == selectedImage)
                    {
                        var selectedGame = kvp.Key;
                        // Do something with the selected GameResponse
                        MessageBox.Show($"Selected game: {selectedGame.Name}");
                        break;
                    }
                }
            }
        }
    }
}
