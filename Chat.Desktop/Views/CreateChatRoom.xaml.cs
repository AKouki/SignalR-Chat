using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for CreateChatRoom.xaml
    /// </summary>
    public partial class CreateChatRoom : Window
    {
        public string RoomName => txtNewRoomName.Text.Trim();
        public CreateChatRoom()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtNewRoomName.Focus();
        }

        private void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
