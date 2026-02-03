using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Entities.Content;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Enums;
using MedLink.Domain.Identity;
using MedLink.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MedLink.Infrastructure.Persistence.Seed
{
    public static class ApplicationDbContextSeed
    {
        // SRID 4326 = WGS84 (standard GPS coordinate system)
        private const int Srid = 4326;

        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Roles
            await SeedRolesAsync(roleManager);

            // 2. Users
            var adminUser = await SeedAdminUserAsync(userManager);
            var patientUser = await SeedPatientUserAsync(userManager);

            // 3. Specializations
            await SeedSpecializationsAsync(context);

            // 4. Content (Languages, FAQs, Abouts)
            await SeedContentAsync(context);

            // 5. User Profiles
            await SeedUserProfilesAsync(context, patientUser?.Id, adminUser?.Id);

            // 6. Doctors
            await SeedDoctorsAsync(context, userManager);

            // 7. Doctor Availabilities (Granular Slots) - ensuring we have slots for testing
            await SeedDoctorAvailabilitiesAsync(context);

            // 8. Appointments & Related (Payments, Reviews, ChatRooms, Messages)
            await SeedAppointmentsAndRelatedAsync(context, patientUser);

            // 9. Favorites
            await SeedFavoritesAsync(context, patientUser);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Patient", "Doctor", "User" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<ApplicationUser?> SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@medlink.com";
            var existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser != null) return existingUser;

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                return adminUser;
            }
            return null;
        }

        private static async Task<ApplicationUser?> SeedPatientUserAsync(UserManager<ApplicationUser> userManager)
        {
            var patientEmail = "patient@medlink.com";
            var existingUser = await userManager.FindByEmailAsync(patientEmail);
            if (existingUser != null) return existingUser;

            var patientUser = new ApplicationUser
            {
                UserName = patientEmail,
                Email = patientEmail,
                FullName = "John Doe",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(patientUser, "Patient@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(patientUser, "Patient");
                return patientUser;
            }
            return null;
        }

        private static async Task SeedSpecializationsAsync(ApplicationDbContext context)
        {
            if (await context.Specializations.AnyAsync()) return;

            var specializations = new List<Specialization>
            {
                new Specialization { Name = "Cardiology", Description = "Heart Specialist" },
                new Specialization { Name = "Dermatology", Description = "Skin Specialist" },
                new Specialization { Name = "Pediatrics", Description = "Child Specialist" },
                new Specialization { Name = "Orthopedics", Description = "Bone Specialist" },
                new Specialization { Name = "Neurology", Description = "Brain Specialist" }
            };

            await context.Specializations.AddRangeAsync(specializations);
            await context.SaveChangesAsync();
        }

        private static async Task SeedContentAsync(ApplicationDbContext context)
        {
            // Languages
            if (!await context.Languages.AnyAsync())
            {
                await context.Languages.AddRangeAsync(
                    new Language { Name = "English", Code = "en" },
                    new Language { Name = "Arabic", Code = "ar" }
                );
            }

            // FAQs
            if (!await context.FAQs.AnyAsync())
            {
                await context.FAQs.AddRangeAsync(
                    new FAQ { Question = "How do I book an appointment?", Answer = "You can book an appointment by selecting a doctor and choosing an available slot." },
                    new FAQ { Question = "Can I cancel my appointment?", Answer = "Yes, you can cancel your appointment up to 24 hours before the scheduled time." },
                    new FAQ { Question = "Is my data secure?", Answer = "Yes, we use the latest encryption standards to protect your data." }
                );
            }

            // Abouts
            if (!await context.Abouts.AnyAsync())
            {
                await context.Abouts.AddAsync(new About
                {
                    TermsOfService = "Terms and Conditions of MedLink...",
                    PrivacyPolicy = "Privacy Policy of MedLink...",
                    ReleaseNotes = "v1.0 - Initial Launch",
                    ContactInformation = "support@medlink.com"
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUserProfilesAsync(ApplicationDbContext context, string? patientUserId, string? adminUserId)
        {
            if (patientUserId != null && !await context.UserProfiles.AnyAsync(up => up.UserId == patientUserId))
            {
                context.UserProfiles.Add(new UserProfile
                {
                    UserId = patientUserId,
                    FullName = "John Doe",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = Gender.Male,
                    ImageUrl = "https://randomuser.me/api/portraits/men/1.jpg",
                    MedicalHistory = "No major issues. Allergic to peanuts.",
                    PreferredLanguage = "en"
                });
            }

            if (adminUserId != null && !await context.UserProfiles.AnyAsync(up => up.UserId == adminUserId))
            {
                context.UserProfiles.Add(new UserProfile
                {
                    UserId = adminUserId,
                    FullName = "System Admin",
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Gender = Gender.Male,
                    ImageUrl = "https://randomuser.me/api/portraits/lego/1.jpg",
                    PreferredLanguage = "en"
                });
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedDoctorsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.Doctors.AnyAsync()) return;

            var cardio = await context.Specializations.FirstAsync(s => s.Name == "Cardiology");
            var derma = await context.Specializations.FirstAsync(s => s.Name == "Dermatology");
            var pedia = await context.Specializations.FirstAsync(s => s.Name == "Pediatrics");

            var doctorData = new List<(string Name, string Email, int SpecialtyId, string Bio, decimal Price, Point Loc, Gender Gender, string Image, string Address)>
            {
                ("Dr. Ahmed Ali", "ahmed.ali@medlink.com", cardio.Id, "Expert Cardiologist with over 15 years experience.", 500, CreatePoint(31.2357, 30.0444), Gender.Male, "https://randomuser.me/api/portraits/men/32.jpg", "123 Tahrir St, Cairo"),
                ("Dr. Mona Sayed", "mona.sayed@medlink.com", derma.Id, "Certified Dermatologist specializing in cosmetic procedures.", 300, CreatePoint(31.2089, 30.0131), Gender.Female, "https://randomuser.me/api/portraits/women/44.jpg", "45 Dokki St, Giza"),
                ("Dr. Khaled Ibrahim", "khaled.ibrahim@medlink.com", pedia.Id, "Friendly Pediatrician dedicated to children's health.", 250, CreatePoint(29.9187, 31.2001), Gender.Male, "https://randomuser.me/api/portraits/men/85.jpg", "10 Corniche Rd, Alexandria"),
                ("Dr. Sarah Nabil", "sarah.nabil@medlink.com", cardio.Id, "Cardiology consultant focusing on preventive care.", 450, CreatePoint(31.3283, 30.0875), Gender.Female, "https://randomuser.me/api/portraits/women/65.jpg", "99 Merghany St, Heliopolis, Cairo")
            };

            foreach (var data in doctorData)
            {
                var user = new ApplicationUser
                {
                    UserName = data.Email.Split('@')[0],
                    Email = data.Email,
                    FullName = data.Name,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Doctor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Doctor");

                    var doctor = new Doctor
                    {
                        Name = data.Name,
                        UserId = user.Id,
                        SpecialtyId = data.SpecialtyId,
                        Bio = data.Bio,
                        Price = data.Price,
                        Location = data.Loc,
                        Gender = data.Gender,
                        ImageUrl = data.Image,
                        Address = data.Address,
                        City = data.Email.Contains("Cairo") ? "Cairo" : (data.Email.Contains("Giza") ? "Giza" : "Alexandria")
                    };

                    context.Doctors.Add(doctor);
                    
                    // Also seed a profile for the doctor user
                    context.UserProfiles.Add(new UserProfile
                    {
                        UserId = user.Id,
                        FullName = data.Name,
                        Gender = data.Gender,
                        ImageUrl = data.Image,
                        PreferredLanguage = "en"
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDoctorAvailabilitiesAsync(ApplicationDbContext context)
        {
            var doctors = await context.Doctors.Include(d => d.Availabilities).ToListAsync();
            var today = DateTime.Today;

            foreach (var doctor in doctors)
            {
                // Only seed if the doctor has zero availabilities
                if (!doctor.Availabilities.Any())
                {
                    var newSlots = new List<DoctorAvailability>();
                    // Create slots for the next 7 days
                    for (int day = 1; day < 8; day++)
                    {
                        var date = today.AddDays(day);

                        // Create 30-min slots from 10:00 to 14:00
                        for (int hour = 10; hour < 14; hour++)
                        {
                            // Slot 1: Hour:00
                            newSlots.Add(new DoctorAvailability
                            {
                                DoctorId = doctor.Id,
                                Date = date,
                                StartTime = new TimeSpan(hour, 0, 0),
                                EndTime = new TimeSpan(hour, 30, 0),
                                IsBooked = false
                            });

                            // Slot 2: Hour:30
                            newSlots.Add(new DoctorAvailability
                            {
                                DoctorId = doctor.Id,
                                Date = date,
                                StartTime = new TimeSpan(hour, 30, 0),
                                EndTime = new TimeSpan(hour + 1, 0, 0),
                                IsBooked = false
                            });
                        }
                    }
                    context.DoctorAvailabilities.AddRange(newSlots);
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedAppointmentsAndRelatedAsync(ApplicationDbContext context, ApplicationUser? patientUser)
        {
            if (patientUser == null || await context.Appointments.AnyAsync()) return;

            var doctor = await context.Doctors.Include(d => d.Availabilities).FirstOrDefaultAsync(d => d.Name == "Dr. Ahmed Ali");
            if (doctor == null) return;

            // Re-fetch availability to ensure we have fresh ones from the previous step
            var availability = doctor.Availabilities.FirstOrDefault(a => !a.IsBooked);

            // If no availability found (unlikely), create one just for the seed appointment
            if (availability == null)
            {
                availability = new DoctorAvailability
                {
                    DoctorId = doctor.Id,
                    Date = DateTime.Today.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(9, 30, 0)
                };
                context.DoctorAvailabilities.Add(availability);
                await context.SaveChangesAsync();
            }

            // 1. Completed Appointment with Payment and Review
            var completedAppointment = new Appointment
            {
                DoctorId = doctor.Id,
                ScheduleId = availability.Id,
                BookedByUserId = patientUser.Id,
                PatientName = patientUser.FullName ?? "Patient",
                PatientPhone = "01000000000",
                Status = AppointmentStatus.Completed,
                Fee = doctor.Price,
                Payment = new Payment
                {
                    Amount = doctor.Price,
                    Currency = "EGP",
                    Status = PaymentStatus.Succeeded,
                    PaidAt = DateTime.UtcNow.AddHours(-2),
                    Method = PaymentMethod.Stripe,
                    StripePaymentIntentId = "pi_mock_123456",
                    StripeClientSecret = "secret_mock_123456",
                    FailureReason = string.Empty,
                    RefundReason = string.Empty
                }
            };

            // Mark availability as booked
            availability.IsBooked = true;

            context.Appointments.Add(completedAppointment);
            await context.SaveChangesAsync(); // Save to get ID

            // Add Review for completed appointment
            context.Reviews.Add(new Review
            {
                DoctorId = doctor.Id,
                UserId = patientUser.Id,
                Rating = 5,
                Comment = "Great doctor! Very professional.",
                CreatedAt = DateTime.UtcNow
            });


            // 2. Pending Appointment (Chat Room)
            // Need another availability
            var availability2 = doctor.Availabilities.FirstOrDefault(a => !a.IsBooked && a.Id != availability.Id);
            if (availability2 == null)
            {
                availability2 = new DoctorAvailability
                {
                    DoctorId = doctor.Id,
                    Date = DateTime.Today.AddDays(2),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0)
                };
                context.DoctorAvailabilities.Add(availability2);
                await context.SaveChangesAsync();
            }

            var pendingAppointment = new Appointment
            {
                DoctorId = doctor.Id,
                ScheduleId = availability2.Id,
                BookedByUserId = patientUser.Id,
                PatientName = patientUser.FullName ?? "Patient",
                PatientPhone = "01000000000",
                Status = AppointmentStatus.Pending,
                Fee = doctor.Price
            };

            availability2.IsBooked = true;

            context.Appointments.Add(pendingAppointment);
            await context.SaveChangesAsync();

            // Create ChatRoom for Pending Appointment
            var chatRoom = new ChatRoom
            {
                AppointmentId = pendingAppointment.Id
            };
            context.ChatRooms.Add(chatRoom);
            await context.SaveChangesAsync();

            // Add Messages
            context.Messages.AddRange(
                new Message { ChatRoomId = chatRoom.Id, SenderUserId = patientUser.Id, Content = "Hello Doctor, I have a question about my appointment.", SentAt = DateTime.UtcNow.AddMinutes(-10) },
                new Message { ChatRoomId = chatRoom.Id, SenderUserId = patientUser.Id, Content = "Is there any preparation needed?", SentAt = DateTime.UtcNow.AddMinutes(-5) }
            );


            await context.SaveChangesAsync();
        }

        private static async Task SeedFavoritesAsync(ApplicationDbContext context, ApplicationUser? patientUser)
        {
            if (patientUser == null || await context.Favorites.AnyAsync()) return;

            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.Name == "Dr. Mona Sayed");
            if (doctor != null)
            {
                context.Favorites.Add(new Favorite
                {
                    DoctorId = doctor.Id,
                    UserId = patientUser.Id
                });
                await context.SaveChangesAsync();
            }
        }

        private static Point CreatePoint(double longitude, double latitude)
        {
            return new Point(longitude, latitude) { SRID = Srid };
        }
    }
}