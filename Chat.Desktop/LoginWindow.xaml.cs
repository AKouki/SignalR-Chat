using Chat.Desktop.Helpers;
using Chat.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chat.Desktop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string loginUrl = "http://localhost:2325/Account/Login";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            await ConnectAsync();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://localhost:2325/Account/Register");
        }

        private async Task ConnectAsync()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();

            using (var httpClient = new HttpClient(handler))
            {
                // Navigate to Login
                var response = await httpClient.GetAsync(loginUrl);
                var content = await response.Content.ReadAsStringAsync();
                var token = GetToken(content);

                string username = txtUsername.Text;
                string password = txtPassword.Password;
                string str = string.Format("&Username={0}&Password={1}&RememberMe=false", username, password);
                content = token + str;

                // Post login data
                response = await httpClient.PostAsync(loginUrl, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                content = await response.Content.ReadAsStringAsync();
                token = GetToken(content);

                bool isAuthed = false;
                foreach (Cookie cookie in handler.CookieContainer.GetCookies(new Uri(loginUrl)))
                {
                    if (!isAuthed && cookie.Name.Equals(".AspNet.ApplicationCookie"))
                        isAuthed = true;
                }

                if (isAuthed)
                {
                    User.AuthCookie = handler.CookieContainer;
                    MainWindow mw = new MainWindow();
                    mw.Show();
                    this.Close();
                }
                else
                {
                    new InfoDialog("We couldn't authenticate you\nPlease enter valid info!");
                }
            }

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
