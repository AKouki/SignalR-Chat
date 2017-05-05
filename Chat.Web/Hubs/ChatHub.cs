using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Chat.Web.Models.ViewModels;
using Chat.Web.Models;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Chat.Web.Helpers;

namespace Chat.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public  readonly static List<UserViewModel> _Connections = new List<UserViewModel>();
        private readonly static List<RoomViewModel> _Rooms = new List<RoomViewModel>();
        private readonly static Repository _Repo = new Repository(new ApplicationDbContext());
        private readonly static Dictionary<string, string> Map = new Dictionary<string, string>();

        public void Send(string roomName, string message)
        {
            try
            {
                if (message.StartsWith("/private"))
                {
                    SendPrivate(message);
                }
                else
                {
                    // Save message in database
                    MessageViewModel messageView = _Repo.AddMessage(WhatsMyName(), roomName, message);
                    if (messageView != null && !string.IsNullOrEmpty(messageView.Content))
                    {
                        // Send the message
                        messageView.Content = BasicEmojis.ParseEmojis(messageView.Content);
                        Clients.Group(roomName).newMessage(messageView);
                    }
                }
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message not send!");
            }
        }

        public void SendPrivate(string message)
        {
            // message format: /private(receiverName) Lorem ipsum...
            // Parsing message to find receiver and message
            string[] split = message.Split(')');
            string receiver = split[0].Split('(')[1];
            string userId;
            if (Map.TryGetValue(receiver, out userId))
            {
                // Who is the sender;
                var sender = _Connections.Where(u => u.Username == WhatsMyName()).First();

                // Build the message
                MessageViewModel messageView = new MessageViewModel()
                {
                    From = sender.DisplayName,
                    Avatar = sender.Avatar,
                    To = "",
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                    Timestamp = DateTime.Now.ToLongTimeString()
                };
                
                // Send the message
                Clients.Client(userId).newMessage(messageView);
                Clients.Caller.newMessage(messageView);
            }
        }

        public void Join(string roomName)
        {
            try
            {
                UserViewModel user = _Connections.Where(u => u.Username == Context.User.Identity.Name).First();
                if (user.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.CurrentRoom))
                        Clients.OthersInGroup(user.CurrentRoom).removeUser(user);

                    // Leave old room and join to the new one
                    Leave(user.CurrentRoom);
                    Groups.Add(Context.ConnectionId, roomName);
                    user.CurrentRoom = roomName;

                    // Tell target room users to add new user in their list
                    Clients.OthersInGroup(roomName).addUser(user);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to join to this room!" + ex.Message);
            }
        }

        private void Leave(string roomName)
        {
            Groups.Remove(Context.ConnectionId, roomName);
        }

        public void CreateRoom(string roomName)
        {
            try
            {
                // Accept: Letters, numbers and one space between words.
                Match match = Regex.Match(roomName, @"^\w+( \w+)*$");
                if (!match.Success)
                {
                    Clients.Caller.onError("Invalid room name!\nRoom name must contain only letters and numbers.");
                }
                else if (roomName.Length < 5 || roomName.Length > 20)
                {
                    Clients.Caller.onError("Room name must be between 5-20 characters!");
                }
                else if (_Repo.RoomExist(roomName))
                {
                    Clients.Caller.onError("Another chat room with this name exists");
                }
                else
                {
                    // Save new room in database
                    var room = _Repo.CreateRoom(roomName, WhatsMyName());
                    if (room != null)
                    {
                        // Tell others to update theyr room list
                        _Rooms.Add(new RoomViewModel(room));
                        Clients.All.addChatRoom(new RoomViewModel(room));
                    }
                }

            }
            catch (Exception) { }
        }

        public void DeleteRoom(string roomName)
        {
            try
            {
                // Delete room from Database
                bool roomDeleted = _Repo.DeleteRoom(roomName, WhatsMyName());
                if (roomDeleted)
                {
                    // Delete room from server list
                    RoomViewModel roomView = _Rooms.First<RoomViewModel>(r => r.Name == roomName);
                    _Rooms.Remove(roomView);

                    // Delete room from Clients and move them to the Lobby
                    Clients.Group(roomName).onRoomDeleted(string.Format("Room {0} has been deleted.\nYou are now moved to the Lobby!", roomName));
                    Clients.All.removeChatRoom(roomView);
                }
            }
            catch (Exception) { }
        }

        public IEnumerable<MessageViewModel> GetMessageHistory(string roomName)
        {
            try
            {
                // Last 20 messages
                List<Message> list = _Repo.GetMessageHistory(roomName, 20);

                // Convert result to soemthing that users are allowed to see
                List<MessageViewModel> listViewModel = new List<MessageViewModel>();
                foreach (var item in list)
                {
                    MessageViewModel mvd = new MessageViewModel(item, item.FromUser.Avatar);
                    mvd.Content = BasicEmojis.ParseEmojis(mvd.Content);
                    listViewModel.Add(mvd);
                }

                return listViewModel;
            }
            catch (Exception)
            {
                Clients.Caller.onError("We couldn't load message history!");
                return null;
            }
        }

        public IEnumerable<RoomViewModel> GetRooms()
        {
            using (var db = new ApplicationDbContext())
            {
                // First run?
                if (_Rooms.Count == 0)
                {
                    foreach (var room in db.Rooms)
                        _Rooms.Add(new RoomViewModel(room));
                }
            }

            return _Rooms.ToList();
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        #region OnConnected/OnDisconnected
        public override Task OnConnected()
        {
            using (var db = new ApplicationDbContext())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == Context.User.Identity.Name).FirstOrDefault();
                    var userView = new UserViewModel(user);
                    userView.Device = GetDevice();
                    _Connections.Add(userView);

                    Map.Add(WhatsMyName(), Context.ConnectionId);

                    Clients.Caller.getProfileInfo(user.DisplayName, user.Avatar);
                }
                catch (Exception) { }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var user = _Connections.Where(u => u.Username == Context.User.Identity.Name).First();
                _Connections.Remove(user);

                Clients.OthersInGroup(user.CurrentRoom).removeUser(user);

                Map.Remove(user.Username);
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnRecconect: " + ex.Message);
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string username = WhatsMyName();
           
            var user = _Connections.Where(u => u.Username == WhatsMyName()).First();
            Clients.Caller.getProfileInfo(user.DisplayName, user.Avatar);

            return base.OnReconnected();
        }
        #endregion

        private string WhatsMyName()
        {
            return Context.User.Identity.Name;
        }

        private string GetDevice()
        {
            string device = Context.Headers.Get("Device");

            if (device != null && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
    }
}