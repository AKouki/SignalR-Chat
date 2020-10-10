using Chat.Desktop.Helpers;
using Chat.Desktop.ViewModels;
using Chat.Desktop.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        HubConnection connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:44354/chatHub", options =>
            {
                options.Cookies = User.AuthCookie;
            }).Build();

        public MainWindow()
        {
            InitializeComponent();
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
            await Connect();
            await GetRooms();
            await GetUsers(User.CurrentRoom);
            await GetMessages(User.CurrentRoom);

            ListBoxRooms.SelectedIndex = 0;
        }

        public async Task Connect()
        {
            await connection.StartAsync();
            await connection.SendAsync("Join", "Lobby");
            User.CurrentRoom = "Lobby";
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

            connection.On<RoomViewModel>("removeChatRoom", (room) =>
            {
                var roomToRemove = Rooms.Where(r => r.Id == room.Id).FirstOrDefault();
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

        public async Task SendPrivate(string userName, string message)
        {
            await connection.SendAsync("SendpRIVATE", userName, message);
        }
        public async Task SendToRoom(string roomName, string message)
        {
            await connection.SendAsync("SendToRoom", roomName, message);
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
                {
                    await connection.SendAsync("SendPrivate", receiver, message);
                    txtMessage.Text = "";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(text))
                {
                    await connection.SendAsync("SendToRoom", User.CurrentRoom, text.Trim());
                    txtMessage.Text = "";
                }
            }
        }

        public async Task GetRooms()
        {
            Rooms = await connection.InvokeAsync<ObservableCollection<RoomViewModel>>("GetRooms");
            ListBoxRooms.ItemsSource = Rooms;
        }

        public async Task GetUsers(string roomName)
        {
            Users = await connection.InvokeAsync<ObservableCollection<UserViewModel>>("GetUsers", roomName);
            ListViewUsers.ItemsSource = Users;
        }

        public async Task GetMessages(string roomName)
        {
            Messages = await connection.InvokeAsync<ObservableCollection<MessageViewModel>>("GetMessageHistory", roomName);
            ListViewMessages.ItemsSource = Messages;
        }

        private async void ListBoxRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRoom = ListBoxRooms.SelectedItem as RoomViewModel;
            if (selectedRoom != null)
            {
                await connection.SendAsync("Join", selectedRoom.Name);
                User.CurrentRoom = selectedRoom.Name;
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
                await connection.SendAsync("CreateRoom", createRoomWnd.RoomName);
            }
        }

        private async void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            await connection.SendAsync("DeleteRoom", User.CurrentRoom);
        }

        private void btnSignout_Click(object sender, RoutedEventArgs e)
        {
            User.AuthCookie = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
