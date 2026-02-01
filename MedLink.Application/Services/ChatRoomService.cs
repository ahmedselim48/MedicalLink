using Domain.ErrorHandling;
using MapsterMapper;
using MedLink.Application.DTOs.Chat;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Chat;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Services
{




    public class ChatRoomService : IChatRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatRoomService> _logger;

        public ChatRoomService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatRoomService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<ChatRoom>> GetOrCreateChatRoomAsync(int appointmentId)
        {
            try
            {
                // البحث عن الشات روم الحالي
                var spec = new ChatRoomByAppointmentSpec(appointmentId);
                var chatRoom = await _unitOfWork.Repository<ChatRoom>()
                    .GetEntityWithAsync(spec);

                if (chatRoom != null)
                    return Result.Success(chatRoom);

                // التحقق من وجود الموعد
                var appointment = await _unitOfWork.Repository<Appointment>()
                    .GetByIdAsync(appointmentId);

                if (appointment == null)
                    return Result.Failure<ChatRoom>(
                        Error.NotFound("Appointment not found"));

                // إنشاء شات روم جديد
                chatRoom = new ChatRoom
                {
                    AppointmentId = appointmentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ChatRoom>().AddAsync(chatRoom);
                await _unitOfWork.Complete();

                _logger.LogInformation($"Created chat room {chatRoom.Id} for appointment {appointmentId}");
                return Result.Success(chatRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateChatRoomAsync");
                return Result.Failure<ChatRoom>(
                    Error.InternalServer("Failed to create chat room"));
            }
        }

        public async Task<bool> CanUserAccessAsync(int appointmentId, string userId)
        {
            try
            {
                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null)
                    return false;

                var user = await _userManager.FindByIdAsync(userId); // ✅ استخدام FindByIdAsync
                if (user == null)
                    return false;

                // 1. هل المستخدم هو المريض؟
                bool isPatient = appointment.UserId == userId;

                // 2. هل المستخدم هو الطبيب؟
                bool isDoctor = false;

                // بما أن Doctor مافيهاش UserId، هنجيب الطبيب من Appointment
                var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(appointment.DoctorId);
                if (doctor != null)
                {
                    // نحتاج طريقة لمعرفة إذا كان هذا المستخدم هو الطبيب
                    // الحل المؤقت: لو الأسماء أو الإيميلات متطابقة
                    if (user.Id == doctor.UserId || user.Id == doctor.Id.ToString())
                    {
                        isDoctor = true;
                    }
                }

                // 3. هل المستخدم هو Admin؟
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                return isPatient || isDoctor || isAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanUserAccessAsync");
                return false;
            }
        }

        public async Task<Result<ChatRoomInfoDto>> GetChatRoomInfoAsync(int appointmentId, string currentUserId)
        {
            try
            {
                var appointment = await _unitOfWork.Repository<Appointment>()
                    .GetByIdAsync(appointmentId);

                if (appointment == null)
                    return Result.Failure<ChatRoomInfoDto>(
                        Error.NotFound("Appointment not found"));

                // التحقق من الصلاحيات
                if (!await CanUserAccessAsync(appointmentId, currentUserId))
                    return Result.Failure<ChatRoomInfoDto>(
                        Error.Forbidden("Access denied"));

                // الحصول على أو إنشاء الشات روم
                var chatRoomResult = await GetOrCreateChatRoomAsync(appointmentId);
                if (chatRoomResult.IsFailure)
                    return Result.Failure<ChatRoomInfoDto>(chatRoomResult.Error);

                // تحديد المستخدم الآخر
                string otherUserId;
                string otherUserName;

                bool isPatient = appointment.UserId == currentUserId;

                if (isPatient)
                {
                    // المريض يتحدث مع الطبيب
                    var doctor = await _unitOfWork.Repository<Doctor>()
                        .GetByIdAsync(appointment.DoctorId);

                    if (doctor == null)
                        return Result.Failure<ChatRoomInfoDto>(
                            Error.NotFound("Doctor not found"));

                    // إذا كان الطبيب مرتبط بمستخدم
                    if (!string.IsNullOrEmpty(doctor.UserId))
                    {
                        otherUserId = doctor.UserId;
                        var doctorUser = await _userManager.FindByIdAsync(doctor.UserId);
                        otherUserName = doctorUser?.FullName ?? doctor.Name ?? "Doctor";
                    }
                    else
                    {
                        // إذا لم يكن مرتبط بمستخدم، استخدم معلومات الطبيب مباشرة
                        otherUserId = $"doctor_{doctor.Id}";
                        otherUserName = doctor.Name ?? "Doctor";
                    }
                }
                else
                {
                    // الطبيب يتحدث مع المريض
                    otherUserId = appointment.UserId;
                    var patientUser = await _userManager.FindByIdAsync(otherUserId);
                    otherUserName = patientUser?.FullName ?? appointment.PatientName ?? "Patient";
                }

                return Result.Success(new ChatRoomInfoDto
                {
                    AppointmentId = appointmentId,
                    ChatRoomId = chatRoomResult.Value.Id,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUserName,
                    AppointmentDate = appointment.CreatedAt // أو تاريخ آخر تحديث
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatRoomInfoAsync");
                return Result.Failure<ChatRoomInfoDto>(
                    Error.InternalServer("Failed to get chat room info"));
            }
        }

        public async Task<Result<List<ChatRoomDto>>> GetUserChatRoomsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Result.Failure<List<ChatRoomDto>>(Error.NotFound("User not found"));

                // جلب جميع المواعيد الخاصة بالمستخدم
                List<Appointment> appointments = new();

                // المواعيد التي يكون فيها المستخدم هو المريض
                var patientAppointments = await _unitOfWork.Repository<Appointment>()
                    .FindAsync(a => a.UserId == userId);
                appointments.AddRange(patientAppointments);

                // المواعيد التي يكون فيها المستخدم هو الطبيب
                var doctor = await _unitOfWork.Repository<Doctor>()
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor != null)
                {
                    var doctorAppointments = await _unitOfWork.Repository<Appointment>()
                        .FindAsync(a => a.DoctorId == doctor.Id);
                    appointments.AddRange(doctorAppointments);
                }

                var chatRooms = new List<ChatRoomDto>();

                foreach (var appointment in appointments.Distinct())
                {
                    var chatRoomResult = await GetOrCreateChatRoomAsync(appointment.Id);

                    if (chatRoomResult.IsSuccess)
                    {
                        var chatRoomInfo = await GetChatRoomInfoAsync(appointment.Id, userId);

                        if (chatRoomInfo.IsSuccess)
                        {
                            // جلب آخر رسالة
                            var messages = await _unitOfWork.Repository<Message>()
                                .FindAsync(m => m.ChatRoomId == chatRoomResult.Value.Id && !m.IsDeleted);

                            var lastMessage = messages
                                .OrderByDescending(m => m.CreatedAt)
                                .FirstOrDefault();

                            chatRooms.Add(new ChatRoomDto
                            {
                                AppointmentId = appointment.Id,
                                ChatRoomId = chatRoomResult.Value.Id,
                                OtherUserId = chatRoomInfo.Value.OtherUserId,
                                OtherUserName = chatRoomInfo.Value.OtherUserName,
                                LastMessage = lastMessage?.Content,
                                LastMessageTime = lastMessage?.CreatedAt,
                                UnreadCount = 0 // يمكنك إضافة منطق لحساب الرسائل غير المقروءة
                            });
                        }
                    }
                }

                return Result.Success(chatRooms.OrderByDescending(c => c.LastMessageTime).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserChatRoomsAsync");
                return Result.Failure<List<ChatRoomDto>>(
                    Error.InternalServer("Failed to get chat rooms"));
            }
        }
    }
}



