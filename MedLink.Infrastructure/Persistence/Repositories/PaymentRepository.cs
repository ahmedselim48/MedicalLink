using MedLink.Domain.Entities.Payments;
using MedLink.Infrastructure.Persistence.Context;
using MedLink.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && !p.IsDeleted);
        }

        public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId && !p.IsDeleted);
        }

        public async Task<List<Payment>> GetPaymentsByUserAsync(string userId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(p => p.Appointment.BookedByUserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> ExistsForAppointmentAsync(int appointmentId)
        {
            return await _context.Payments.AnyAsync(p => p.AppointmentId == appointmentId && !p.IsDeleted);
        }
    }
}

