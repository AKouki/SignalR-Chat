using Chat.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Chat.Web.Models
{
    public class Repository
    {
        private readonly ApplicationDbContext _db;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
        }

        public MessageViewModel AddMessage(string from, string roomName, string message)
        {
            var user = _db.Users.Where(u => u.UserName == from).First();
            var room = _db.Rooms.Where(r => r.Name == roomName).First();

            Message msg = new Message();
            msg.Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty); // Allow only links and images
            msg.Timestamp = DateTime.Now.Ticks.ToString();
            msg.FromUser = user;
            msg.ToRoom = room;

            _db.Messages.Add(msg);
            _db.SaveChanges();

            return new MessageViewModel(msg, user.Avatar);
        }

        public Room CreateRoom(string name, string who)
        {
            try
            {
                var user = _db.Users.Where(u => u.UserName == who).First();

                Room room = new Room();
                room.Name = name;
                room.UserAccount = user;

                _db.Rooms.Add(room);
                _db.SaveChanges();

                return (room);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool DeleteRoom(string roomName, string userName)
        {
            try
            {
                var room = _db.Rooms.Where(r => r.Name == roomName && r.UserAccount.UserName == userName).First();

                _db.Rooms.Remove(room);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool RoomExist(string name)
        {
            return _db.Rooms.Any(r => r.Name == name);
        }

        public List<Message> GetMessageHistory(string roomName, int maxMessages)
        {
            return _db.Messages.Where(m => m.ToRoom.Name == roomName)
                               .OrderByDescending(m => m.Timestamp)
                               .Take(maxMessages)
                               .AsEnumerable()
                               .Reverse()
                               .ToList();
        }

    }
}