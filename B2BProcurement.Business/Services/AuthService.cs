using AutoMapper;
using B2BProcurement.Business.DTOs.User;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Kimlik doğrulama servisi implementasyonu.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "E-posta adresi veya şifre hatalı."
                };
            }

            // Şifre kontrolü (basit hash karşılaştırması - gerçek projede BCrypt kullanılmalı)
            var passwordHash = HashPassword(dto.Password);
            if (user.PasswordHash != passwordHash)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "E-posta adresi veya şifre hatalı."
                };
            }

            return new LoginResultDto
            {
                Success = true,
                Message = "Giriş başarılı.",
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <inheritdoc/>
        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            // E-posta kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                throw new BusinessException("Bu e-posta adresi zaten kullanılıyor.", "EMAIL_EXISTS");
            }

            // Vergi numarası kontrolü
            if (await _context.Companies.AnyAsync(c => c.TaxNumber == dto.TaxNumber))
            {
                throw new BusinessException("Bu vergi numarası zaten kayıtlı.", "TAX_NUMBER_EXISTS");
            }

            // Paket kontrolü - seçilen paketi kullan veya varsayılan bul
            int packageId = dto.PackageId;
            if (packageId <= 0)
            {
                var defaultPackage = await _context.Packages.FirstOrDefaultAsync(p => p.IsActive);
                packageId = defaultPackage?.Id ?? 1; // Varsayılan 1 olsun
            }

            // Şirket oluştur
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                TaxNumber = dto.TaxNumber,
                SectorId = dto.SectorId,
                PackageId = packageId
            };

            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            // Kullanıcı oluştur
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Role = UserRole.CompanyAdmin,
                CompanyId = company.Id
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        /// <inheritdoc/>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new NotFoundException("Kullanıcı", userId);

            // Mevcut şifre kontrolü
            if (user.PasswordHash != HashPassword(oldPassword))
            {
                throw new BusinessException("Mevcut şifre hatalı.", "INVALID_PASSWORD");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public Task LogoutAsync(int userId)
        {
            // Session yönetimi için kullanılabilir
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Güvenlik için kullanıcı bulunamasa bile true dön
                return true;
            }

            // TODO: Token oluştur ve e-posta gönder
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // TODO: Token doğrulama ve şifre sıfırlama
            await Task.CompletedTask;
            return true;
        }

        /// <inheritdoc/>
        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        /// <inheritdoc/>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);

            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        private static string HashPassword(string password)
        {
            // Demo için basit hash - gerçek projede BCrypt kullanılmalı
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "_hashed"));
        }
    }
}
