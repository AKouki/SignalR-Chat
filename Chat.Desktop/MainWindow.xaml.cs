using Chat.Desktop.Helpers;
using Chat.Desktop.Models;
using Chat.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Threading;

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

        ChatHubManager hub;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                hub = new ChatHubManager(User.AuthCookie);
                await hub.Start();

                Rooms = new ObservableCollection<RoomViewModel>();
                Messages = new ObservableCollection<MessageViewModel>();
                Users = new ObservableCollection<UserViewModel>();

                Rooms = await hub.GetRooms();

                // Bind users to ListBox
                ListBoxRooms.ItemsSource = Rooms;

                Closing += (_, __) => hub.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Main Window: " + ex.Message);
            }
        }
        private bool Filter(object item)
        {
            if (String.IsNullOrEmpty(txtSearchUser.Text))
                return true;

            UserViewModel user = (UserViewModel)item;
            return user.DisplayName.IndexOf(txtSearchUser.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await hub.Send(User.CurrentRoom, txtMessage.Text);
        }

        public void AddChatRoom(RoomViewModel room)
        {
            Rooms.Add(room);
        }

        public async void RemoveChatRoom(RoomViewModel room)
        {
            RoomViewModel roomToDelete = Rooms.Where(r => r.Name == room.Name).First();
            Rooms.Remove(roomToDelete);

            await hub.Join(Rooms[0].Name);
        }

        public void AddMessage(MessageViewModel message)
        {
            Messages.Add(message);

            ListViewMessages.Items.MoveCurrentToLast();
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
        }

        public void AddUser(UserViewModel user)
        {
            Users.Add(user);

            txtOnlineCounter.Text = string.Format("WHO'S HERE ({0})", Users.Count);
        }

        public void RemoveUser(UserViewModel user)
        {
            UserViewModel userToDelete = Users.Where(u => u.DisplayName == user.DisplayName).First();
            Users.Remove(userToDelete);

            txtOnlineCounter.Text = string.Format("WHO'S HERE ({0})", Users.Count);
        }

        public void JoinLobby()
        {
            // 'Join' code is on SelectionChanged Event.
            ListBoxRooms.SelectedIndex = 0;
        }

        public void UpdateProfileInfo(string displayName, string avatar)
        {
            imgMyAvatar.Source = Utils.Base64ToBitmap(avatar);
            txtUsername.Text = displayName;

            ListBoxRooms.SelectedIndex = 0;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            User.AuthCookie = null;

            new LoginWindow().Show();
            this.Close();
        }

        private async void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            CreateChatRoom createRoomWnd = new CreateChatRoom();
            if (createRoomWnd.ShowDialog() == true)
            {
                await hub.CreateRoom(createRoomWnd.RoomName);
            }
        }

        private async void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            await hub.DeleteRoom(User.CurrentRoom);
        }

        private async void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await hub.Send(User.CurrentRoom, txtMessage.Text);
                txtMessage.Text = "";
            }
        }

        private void txtSearchUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CollectionViewSource.GetDefaultView(ListViewUsers.ItemsSource).Refresh();
            }
            catch (Exception) { }
        }

        private async void ListBoxRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RoomViewModel selectedRoom = ListBoxRooms.SelectedItem as RoomViewModel;
            if (selectedRoom != null)
            {
                await hub.Join(selectedRoom.Name);
                User.CurrentRoom = selectedRoom.Name;
                txtRoomName.Text = User.CurrentRoom;

                // Get users and messages
                Users = await hub.GetUsers(selectedRoom.Name);
                Messages = await hub.GetMessageHistory(selectedRoom.Name);

                // Bind them to ListViews
                ListViewMessages.ItemsSource = Messages;
                ListViewUsers.ItemsSource = Users;

                // Auto-scroll to bottom
                ListViewMessages.Items.MoveCurrentToLast();
                ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);

                txtOnlineCounter.Text = string.Format("WHO'S HERE ({0})", Users.Count);

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewUsers.ItemsSource);
                view.Filter = Filter;
            }
        }

        private void ListViewUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserViewModel selectedUser = ListViewUsers.SelectedItem as UserViewModel;
            if (selectedUser != null)
            {
                txtMessage.Text = string.Format("/private({0})", selectedUser.Username) + " " + txtMessage.Text;
                txtMessage.Focus();
            }
        }

        #region Instance
        public static MainWindow Instance;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
        }
        #endregion
    }
}
