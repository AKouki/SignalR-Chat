﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chat.Web.Data;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Chat.Web.Hubs;
using Chat.Web.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;

namespace Chat.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
            return Ok(messageViewModel);
        }

        [HttpGet("Room/{roomName}")]
        public IActionResult GetMessages(string roomName)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
                return BadRequest();

            var messages = _context.Messages.Where(m => m.ToRoomId == room.Id)
                .Include(m => m.FromUser)
                .Include(m => m.ToRoom)
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .AsEnumerable()
                .Reverse()
                .ToList();

            var messagesViewModel = _mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messages);

            return Ok(messagesViewModel);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(MessageViewModel viewModel)
        {
            if (ShouldReset())
            {
                await DbInitializer.Initialize(_context, _userManager);
                return Redirect("/");
            }
            else
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                var room = _context.Rooms.FirstOrDefault(r => r.Name == viewModel.Room);
                if (room == null)
                    return BadRequest();

                var msg = new Message()
                {
                    Content = Regex.Replace(viewModel.Content, @"<.*?>", string.Empty),
                    FromUser = user,
                    ToRoom = room,
                    Timestamp = DateTime.Now
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();

                // Broadcast the message
                var createdMessage = _mapper.Map<Message, MessageViewModel>(msg);
                await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", createdMessage);

                return CreatedAtAction(nameof(Get), new { id = msg.Id }, createdMessage);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages
                .Include(u => u.FromUser)
                .Where(m => m.Id == id && m.FromUser.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatMessage", message.Id);

            return Ok();
        }

        private bool ShouldReset()
        {
            var rooms = _context.Rooms.Count();
            var messages = _context.Messages.Count();

            if ((rooms + messages) > 500)
            {
                return true;
            }

            return false;
        }
    }
}
