using Chat.Desktop.Helpers;
using Chat.Desktop.ViewModels;
using Chat.Desktop.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chat.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<RoomViewModel> Rooms { get; set; }
        public ObservableCollection<MessageViewModel> Messages { get; set; }
        public ObservableCollection<UserViewModel> Users { get; set; }

        private const string BaseUri = "https://localhost:44354";

        private readonly HttpClient httpClient;

        HubConnection connection = new HubConnectionBuilder()
            .WithUrl($"{BaseUri}/chatHub", options => { options.Cookies = User.AuthCookie; })
            .Build();

        public MainWindow()
        {
            InitializeComponent();

            HttpClientHandler handler = new HttpClientHandler()
            {
                CookieContainer = User.AuthCookie
            };

            httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(BaseUri);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rooms = new ObservableCollection<RoomViewModel>();
            Messages = new ObservableCollection<MessageViewModel>();
            Users = new ObservableCollection<UserViewModel>();

            ListBoxRooms.ItemsSource = Rooms;
            ListViewMessages.ItemsSource = Messages;
            ListViewUsers.ItemsSource = Users;

            RegisterEvents();
            await connection.StartAsync();

            await GetRooms();

            ListBoxRooms.SelectedIndex = 0;
        }

        private void RegisterEvents()
        {
            connection.On<MessageViewModel>("newMessage", (message) =>
            {
                Messages.Add(message);
                ListViewMessages.Items.MoveCurrentToLast();
                ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
            });

            connection.On<string, string>("getProfileInfo", (displayName, avatar) =>
            {
                txtUsername.Text = displayName;
                Uri uri = new Uri(@"/Images/Avatars/" + avatar, UriKind.Relative);
                imgAvatar.Source = new BitmapImage(uri);
            });

            connection.On<UserViewModel>("addUser", (user) =>
            {
                Users.Add(user);
                txtOnlineCounter.Text = $"WHO'S HERE ({Users.Count})";
            });

            connection.On<UserViewModel>("removeUser", (user) =>
            {
                var userToRemove = Users.Where(u => u.Username == user.Username).FirstOrDefault();
                Users.Remove(userToRemove);
                txtOnlineCounter.Text = $"WHO'S HERE ({Users.Count})";
            });

            connection.On<RoomViewModel>("addChatRoom", (room) =>
            {
                Rooms.Add(room);
            });

            connection.On<int>("removeChatRoom", (roomId) =>
            {
                var roomToRemove = Rooms.Where(r => r.Id == roomId).FirstOrDefault();
                Rooms.Remove(roomToRemove);
            });

            connection.On<string>("onError", (error) =>
            {
                MessageBox.Show(error);
            });

            connection.On<string>("onRoomDeleted", (message) =>
            {
                ListBoxRooms.SelectedIndex = 0;
                txtOnlineCounter.Text = $"WHO'S HERE ({Users.Count})";
            });
        }

        private async Task SendMessage()
        {
            var text = txtMessage.Text;
            if (text.StartsWith("/"))
            {
                int startIndex = text.IndexOf('(') + 1;
                int length = text.IndexOf(')') - startIndex;
                var receiver = text.Substring(startIndex, length);
                var message = text.Substring(text.IndexOf(')') + 1);

                if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(message))
                    await connection.SendAsync("SendPrivate", receiver, message);
            }
            else
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var json = JsonSerializer.Serialize(new MessageViewModel() { Room = User.CurrentRoom.Name, Content = text.Trim() });
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    await httpClient.PostAsync("/api/Messages", data);
                }
            }

            txtMessage.Text = "";
        }

        public async Task GetRooms()
        {
            var response = await httpClient.GetAsync("/api/Rooms");
            var content = await response.Content.ReadAsStringAsync();

            Rooms = JsonSerializer.Deserialize<ObservableCollection<RoomViewModel>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            ListBoxRooms.ItemsSource = Rooms;
        }

        public async Task GetUsers(string roomName)
        {
            Users = await connection.InvokeAsync<ObservableCollection<UserViewModel>>("GetUsers", roomName);
            ListViewUsers.ItemsSource = Users;
        }

        public async Task GetMessages(string roomName)
        {
            var response = await httpClient.GetAsync("/api/Messages/Room/" + roomName);
            var content = await response.Content.ReadAsStringAsync();

            Messages = JsonSerializer.Deserialize<ObservableCollection<MessageViewModel>>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            ListViewMessages.ItemsSource = Messages;
        }

        private async void ListBoxRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRoom = ListBoxRooms.SelectedItem as RoomViewModel;
            if (selectedRoom != null)
            {
                await connection.SendAsync("Join", selectedRoom.Name);
                User.CurrentRoom = selectedRoom;
                txtRoomName.Text = selectedRoom.Name;

                await GetUsers(selectedRoom.Name);
                await GetMessages(selectedRoom.Name);

                ListViewMessages.Items.MoveCurrentToLast();
                ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);

                txtOnlineCounter.Text = $"WHO'S HERE ({Users.Count})";
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewUsers.ItemsSource);
                view.Filter = Filter;
            }

        }

        private void ListViewUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = ListViewUsers.SelectedItem as UserViewModel;
            if (selectedUser != null)
            {
                var text = txtMessage.Text;
                if (txtMessage.Text.StartsWith("/private"))
                    text = txtMessage.Text.Split(")")[1].Trim();

                txtMessage.Text = $"/private({selectedUser.Username}) {text}";
                txtMessage.Focus();
                ListViewUsers.SelectedIndex = -1;
            }
        }

        private void txtSearchUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListViewUsers.ItemsSource).Refresh();
        }

        private bool Filter(object item)
        {
            if (string.IsNullOrEmpty(txtSearchUser.Text))
                return true;

            var user = (UserViewModel)item;

            return user.FullName.IndexOf(txtSearchUser.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void btnCreateChatRoom_Click(object sender, RoutedEventArgs e)
        {
            var createRoomWnd = new CreateChatRoom();
            if (createRoomWnd.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(new RoomViewModel() { Name = createRoomWnd.RoomName });
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                await httpClient.PostAsync("/api/Rooms", data);
            }
        }

        private async void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            await httpClient.DeleteAsync("/api/Rooms/" + User.CurrentRoom.Id);
        }

        private async void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async void btnSignout_Click(object sender, RoutedEventArgs e)
        {
            await connection.StopAsync();
            User.AuthCookie = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
