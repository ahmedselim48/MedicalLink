using AutoMapper;
using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Application.Specifications.Medical;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Exceptions;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace MedLink.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IGenericRepository<Doctor> _doctorRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IProfileService _profileService;


        public DoctorService(
            IGenericRepository<Doctor> doctorRepo, 
            IMapper mapper, 
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IProfileService profileService)
        {
            _doctorRepo = doctorRepo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _profileService = profileService;
        }

        public async Task<IReadOnlyList<DoctorSearchResultDto>> SearchDoctorsAsync(DoctorSearchParams searchParams)
        {
            var spec = new DoctorSearchSpec(searchParams);
            var doctors = await _doctorRepo.GetAllWithSpecAsync(spec);

            return _mapper.Map<IReadOnlyList<DoctorSearchResultDto>>(doctors);
        }

        public async Task<IReadOnlyList<DoctorSearchResultDto>> GetTopRatedDoctorsAsync(int count)
        {
            var spec = new TopRatedDoctorsSpec(count);
            var doctors = await _doctorRepo.GetAllWithSpecAsync(spec);
            return _mapper.Map<IReadOnlyList<DoctorSearchResultDto>>(doctors);
        }

        public async Task<Doctor> AddDoctorAsync(Doctor doctor)
        {
            var repo = _unitOfWork.Repository<Doctor>();
            await repo.AddAsync(doctor);
            await _unitOfWork.Complete();
            return doctor;
        }

        public async Task<Doctor> CreateDoctorAsync(AdminCreateDoctorDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                //  Create Identity User
                var user = new ApplicationUser
                {
                    FullName = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email.Split('@')[0],
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"User creation failed: {errors}");
                }

                // Assign Role
                if (!await _roleManager.RoleExistsAsync("Doctor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                }
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Create User Profile
                await _profileService.CreateAsync(user.Id, dto.Name);

                // Create Doctor Record
                var doctor = new Doctor
                {
                    Name = dto.Name,
                    UserId = user.Id,
                    SpecialtyId = dto.SpecialtyId,
                    Price = dto.Price,
                    Bio = dto.Bio,
                    City = dto.City,
                    Gender = dto.Gender,
                    Address = dto.Address,
                    ConsultationFee = dto.ConsultationFee
                };

                if (dto.Latitude.HasValue && dto.Longitude.HasValue)
                {
                    doctor.Location = new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 };
                }

                await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
                await _unitOfWork.Complete();

                await _unitOfWork.CommitTransactionAsync();
                return doctor;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            var repo = _unitOfWork.Repository<Doctor>();
            repo.Update(doctor);
            await _unitOfWork.Complete();
        }


        public async Task DeleteDoctorAsync(int id)
        {
            var repo = _unitOfWork.Repository<Doctor>();


            var doctor = await repo.GetByIdAsync(id);

            if (doctor != null)
            {
                doctor.IsDeleted = true;
                await _unitOfWork.Complete();
            }
            else
            {
                throw new NotFoundException($"Doctor with ID {id} not found.");
            }
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Doctor>();
            return await repo.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Doctor>> GetAllDoctorsAsync(ISpecification<Doctor>? spec = null)
        {


            var repo = _unitOfWork.Repository<Doctor>();
            return spec != null
                ? await repo.GetAllWithSpecAsync(spec)
                : await repo.GetAllAsync();
        }






    }
}

