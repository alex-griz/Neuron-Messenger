using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Neuron
{
    public partial class ProfileView : Window
    {
        public static string target_username = "";
        public ProfileView()
        {
            InitializeComponent();

            LoadData();
        }
        private async void LoadData()
        {
            DataBase db = new DataBase();
            DataTable profileTable = new DataTable();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.LoadProfile, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.AddWithValue("@UN", target_username);

            connection.Open();
            adapter.Fill(profileTable);

            UsernameBox.Text = target_username;
            NameBox.Text = profileTable.Rows[0][1].ToString();
            BioBox.Text = profileTable.Rows[0][2].ToString();
            string avatar_path = profileTable.Rows[0][3].ToString();
            if (!File.Exists(avatar_path))
            {
                await DownloadImage(avatar_path);
            }
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(avatar_path);
            bitmap.DecodePixelHeight = 160;
            bitmap.DecodePixelWidth = 160;
            bitmap.EndInit();

            AvatarBox.Source = bitmap;
            AvatarBox.Stretch = Stretch.UniformToFill;

            if (target_username != MainWindow.Login)
            {
                UsernameBox.IsReadOnly = true;
                NameBox.IsReadOnly = true;
                BioBox.IsReadOnly = true;
                SaveButton.Visibility = Visibility.Hidden;
            }
        }
        public void ChangeProfilePhoto(object sender , RoutedEventArgs e)
        {

        }
        private async Task DownloadImage(string path)
        {
            var response = await MainWindow.client.GetAsync($"http://localhost:5156/Download?file_name={path}",
             HttpCompletionOption.ResponseHeadersRead);
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(Path.Combine("FileStorage", path), FileMode.Create, FileAccess.Write, FileShare.None, 32768, true);

            await contentStream.CopyToAsync(fileStream);
        }
        private async void SaveProfile(object sender, RoutedEventArgs e)
        {
            string new_username = UsernameBox.Text;
            string new_name = NameBox.Text;
            string new_bio = BioBox.Text;

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/ChangeProfileData?username={new_username}&name={new_name}&bio={new_bio}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            if (result == 0)
            {
                MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
    }
}