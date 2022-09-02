using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            await Reset(db);

            bool hasUsers = await db.Users.AnyAsync();
            if (!hasUsers)
                await CreateUsers(userManager);

            await CreateRooms(db);
            await CreateMessages(db);
        }

        private static async Task Reset(ApplicationDbContext db)
        {
            db.Messages.RemoveRange(db.Messages.ToList());
            await db.SaveChangesAsync();

            db.Rooms.RemoveRange(db.Rooms.ToList());
            await db.SaveChangesAsync();
        }

        private static async Task CreateUsers(UserManager<ApplicationUser> userManager)
        {
            var users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "admin",
                    Email = "admin@admin.com",
                    FullName = "James Smith"
                },
                new ApplicationUser()
                {
                    UserName = "admin2",
                    Email = "admin2@admin.com",
                    FullName = "Maria Nikolaou"
                }
            };

            string password = "admin";
            await userManager.CreateAsync(users[0], password);
            await userManager.CreateAsync(users[1], password);
        }

        private static async Task CreateRooms(ApplicationDbContext db)
        {
            var users = db.Users.ToList();

            var rooms = new List<Room>()
            {
                new Room() { Name = "Lobby", Admin = users[0] },
                new Room() { Name = "Marketing", Admin = users[1] },
                new Room() { Name = "Programmers", Admin = users[0] },
                new Room() { Name = "Designers", Admin = users[0] },
                new Room() { Name = "Support", Admin = users[1] },
                new Room() { Name = "Accounting", Admin = users[1] },
                new Room() { Name = "Brainstorming", Admin = users[0] },
                new Room() { Name = "Happy Time", Admin = users[0] }
            };

            db.Rooms.AddRange(rooms);
            await db.SaveChangesAsync();
        }

        private static async Task CreateMessages(ApplicationDbContext db)
        {
            var lobby = db.Rooms.FirstOrDefault(r => r.Name == "Lobby");
            var users = db.Users.ToList();

            var messagesArray = new string[]
            {
                "Hello guys, how are you doing?",
                "Good, and you?",
                "I'm good :)",
                "By the way, how was your vacation? did you have a good time?",
                "It was great. Let me show you a picture",
                "Okay...",
                "<a href=\"/uploads/demo.jpg\" target=\"_blank\"><img src=\"/uploads/demo.jpg\" class=\"post-image\"></a>"
            };

            var timestamp = DateTime.Now.AddMinutes(new Random().Next(-15, -12));

            var messages = new List<Message>()
            {
                new Message()
                {
                    Content = messagesArray[0],
                    FromUser = users[1],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(1)
                },
                new Message()
                {
                    Content = messagesArray[1],
                    FromUser = users[0],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(2)
                },
                new Message()
                {
                    Content = messagesArray[2],
                    FromUser = users[1],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(3)
                },
                new Message()
                {
                    Content = messagesArray[3],
                    FromUser = users[1],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(4)
                },
                new Message()
                {
                    Content = messagesArray[4],
                    FromUser = users[0],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(50)
                },
                new Message()
                {
                    Content = messagesArray[5],
                    FromUser = users[1],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(51)
                },
                new Message()
                {
                    Content = messagesArray[6],
                    FromUser = users[0],
                    ToRoom = lobby,
                    Timestamp = timestamp.AddSeconds(52)
                }
            };

            db.Messages.AddRange(messages);
            await db.SaveChangesAsync();
        }
    }
}
