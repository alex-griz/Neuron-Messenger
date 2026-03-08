using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Neuron
{
    public partial class NeuronRegistration : Window
    {
        public string Username;
        public NeuronRegistration()
        {
            InitializeComponent();
        }

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            string Name = NameBox.Text;
            Username = UsernameBox.Text;
            string Password = PasswordBox.Password;

            using var rsa = RSA.Create(2048);
            var public_key = rsa.ExportRSAPublicKey();
            var private_key = rsa.ExportRSAPrivateKey();

            Random rnd = new Random();
            string security_key = rnd.Next(100001, 999999).ToString();
            string encrypted_private_key = Uri.EscapeDataString(Convert.ToBase64String(AES_Encrypt(private_key,security_key)));
            string safe_public_key = Uri.EscapeDataString(Convert.ToBase64String(public_key));

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/Reg?username={Username}&name={Name}&password={Password}&public_key={safe_public_key}&private_key={encrypted_private_key}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            switch (result)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка регистрации", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    MessageBox.Show("Успешная регистрация!", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    File.WriteAllText("Security_key.txt",security_key);
                    break;
                case 2:
                    MessageBox.Show("Такое имя пользователя уже существует! Пожалуйста, выберите другое", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
            }
        }
        static byte[] AES_Encrypt(byte[] data, string password)
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}