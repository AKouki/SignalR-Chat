using Chat.Desktop.Models;
using Chat.Desktop.Views;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Chat.Desktop
{
    class ChatHubManager
    {
        public string Url = "http://localhost:2325/";
        public HubConnection Connection { get; set; }
        public IHubProxy Proxy { get; set; }

        public ChatHubManager(CookieContainer cookieContainer)
        {
            Connection = new HubConnection(Url);
            Connection.CookieContainer = cookieContainer;
            Connection.Headers.Add("Device", "Desktop");

            Proxy = Connection.CreateHubProxy("chatHub");

            RegisterEvents();
        }

        private void RegisterEvents()
        {

            Proxy.On<MessageViewModel>("newMessage", (message) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.AddMessage(message);
                      }));
            });

            Proxy.On<RoomViewModel>("addChatRoom", (room) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.AddChatRoom(room);
                      }));
            });

            Proxy.On<RoomViewModel>("removeChatRoom", (room) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.RemoveChatRoom(room);
                      }));
            });

            Proxy.On<UserViewModel>("addUser", (user) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.AddUser(user);
                      }));
            });

            Proxy.On<UserViewModel>("removeUser", (user) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.RemoveUser(user);
                      }));
            });

            Proxy.On<string>("onError", (errorMessage) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          new InfoDialog(errorMessage);
                      }));
            });

            Proxy.On<string>("onRoomDeleted", (message) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.JoinLobby();
                          new InfoDialog(message);
                      }));
            });

            Proxy.On<string, string>("getProfileInfo", (displayName, avatar) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                      new Action(() =>
                      {
                          MainWindow.Instance.UpdateProfileInfo(displayName, avatar);
                      }));
            });
        }

        public async Task Start()
        {
            await Connection.Start();
        }

        public void Stop()
        {
            Connection.Stop();
        }

        public async Task Send(string roomName, string message)
        {
            await Proxy.Invoke("send", roomName, message);
        }

        public async Task Join(string roomName)
        {
            await Proxy.Invoke("join", roomName);
        }

        public async Task CreateRoom(string roomName)
        {
            await Proxy.Invoke("createRoom", roomName);
        }

        public async Task DeleteRoom(string roomName)
        {
            await Proxy.Invoke("deleteRoom", roomName);
        }

        public async Task<ObservableCollection<MessageViewModel>> GetMessageHistory(string roomName)
        {
            var data = await Proxy.Invoke<ObservableCollection<MessageViewModel>>("getMessageHistory", roomName);
            return data;
        }

        public async Task<ObservableCollection<RoomViewModel>> GetRooms()
        {
            var data = await Proxy.Invoke<ObservableCollection<RoomViewModel>>("getRooms");
            return data;
        }

        public async Task<ObservableCollection<UserViewModel>> GetUsers(string roomName)
        {
            var data = await Proxy.Invoke<ObservableCollection<UserViewModel>>("getUsers", roomName);
            return data;
        }

    }
}