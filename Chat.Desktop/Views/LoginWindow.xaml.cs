using Chat.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
using System.Windows.Shapes;

namespace Chat.Desktop.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly string loginUrl = "https://localhost:44354/Identity/Account/Login";
        private readonly string registerUrl = "https://localhost:44354/Identity/Account/Register";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            await ConnectAsync();
        }

        private void btnSignup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {registerUrl}")
            {
                CreateNoWindow = true
            });
        }

        private async Task ConnectAsync()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using (var client = new HttpClient(handler))
            {
                var response = await client.GetAsync(loginUrl);
                var content = await response.Content.ReadAsStringAsync();
                var token = GetToken(content);

                string username = txtUsername.Text;
                string password = txtPassword.Password;
                string str = string.Format("&Username={0}&Password={1}&RememberMe=false", username, password);
                content = token + str;

                // Post login data
                response = await client.PostAsync(loginUrl, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                content = await response.Content.ReadAsStringAsync();
                token = GetToken(content);

                bool isAuthed = false;
                foreach (Cookie cookie in handler.CookieContainer.GetCookies(new Uri(loginUrl)))
                {
                    if (!isAuthed && cookie.Name.Equals(".AspNetCore.Identity.Application"))
                        isAuthed = true;
                }

                if (isAuthed)
                {
                    User.AuthCookie = handler.CookieContainer;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("We couldn't authenticate you\nPlease enter valid info!");
                }
            }

            await Task.CompletedTask;
        }

        private string GetToken(string content)
        {
            var startIndex = content.IndexOf("__RequestVerificationToken");
            var endIndex = content.IndexOf("\" />", startIndex);

            if (startIndex == -1)
                return null;

            var length = endIndex - startIndex;
            content = content.Substring(startIndex, length);
            content = content.Replace("\" type=\"hidden\" value=\"", "=");

            return content;
        }
    }
}
