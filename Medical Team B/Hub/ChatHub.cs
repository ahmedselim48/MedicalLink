using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.API.Hubs;

using Domain.ErrorHandling;
using Mapster;
using MedLink.Application.DTOs.Chat;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Infrastructure.Persistence.Context;
using MedLink.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;



[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // Helpers
    // =========================

    private void Throw(Error error)
    {
        var payload = new
        {
            error.Code,
            error.Description,
            error.StatusCode
        };

        throw new HubException(JsonSerializer.Serialize(payload));
    }

    private string GetUserId()
    {
        var userId =
            Context.User?.FindFirstValue("uid") ??
            Context.User?.FindFirstValue("sub") ??
            Context.User?.FindFirstValue("userId") ??
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            Throw(Error.Unauthorized("User not authenticated"));

        return userId!;
    }


    // =========================
    // Join Appointment Room
    // =========================

    public async Task JoinAppointmentRoom(int appointmentId)
    {
        var userId = GetUserId();

        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.Id == appointmentId &&
                (a.BookedByUserId == userId || a.DoctorId.ToString() == userId));

        if (appointment == null)
            Throw(EntityError<Appointment>.NotFound("Appointment access denied"));

        await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId.ToString());
    }

    // =========================
    // Send Message
    // =========================

    public async Task SendMessage(int appointmentId, SendMessageDto dto)
    {
        var userId = GetUserId();

        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            Throw(Error.Validation("Message content is required"));

        var chatRoom = await _context.ChatRooms
            .Include(c => c.Appointment)
            .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);

        if (chatRoom == null)
            Throw(EntityError<ChatRoom>.NotFound());

        if (chatRoom.Appointment!.BookedByUserId != userId &&
            chatRoom.Appointment.DoctorId.ToString() != userId)
            Throw(Error.Forbidden("You are not allowed to send messages in this chat"));

        var message = new Message
        {
            ChatRoomId = chatRoom.Id,
            SenderId = userId,
            Content = dto.Content
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var result = message.Adapt<MessageDto>();

        await Clients.Group(appointmentId.ToString())
            .SendAsync("ReceiveMessage", result);
    }

    // =========================
    // Edit Message
    // =========================

    public async Task EditMessage(int messageId, EditMessageDto dto)
    {
        var userId = GetUserId();

        if (dto == null || string.IsNullOrWhiteSpace(dto.NewContent))
            Throw(Error.Validation("Message content is required"));

        var message = await _context.Messages
            .Include(m => m.ChatRoom)
            .ThenInclude(c => c.Appointment)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null || message.IsDeleted)
            Throw(EntityError<Message>.NotFound());

        if (message.SenderId != userId)
            Throw(Error.Forbidden("You cannot edit this message"));

        message.Content = dto.NewContent;
        message.IsEdited = true;

        await _context.SaveChangesAsync();

        await Clients.Group(message.ChatRoom.AppointmentId!.ToString())
            .SendAsync("MessageEdited", new
            {
                message.Id,
                message.Content,
                message.IsEdited
            });
    }

    // =========================
    // Delete Message
    // =========================

    public async Task DeleteMessage(int messageId)
    {
        var userId = GetUserId();

        var message = await _context.Messages
            .Include(m => m.ChatRoom)
            .ThenInclude(c => c.Appointment)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            Throw(EntityError<Message>.NotFound());

        if (message.SenderId != userId)
            Throw(Error.Forbidden("You cannot delete this message"));

        message.IsDeleted = true;
        await _context.SaveChangesAsync();

        await Clients.Group(message.ChatRoom.AppointmentId!.ToString())
            .SendAsync("MessageDeleted", messageId);
    }
}


