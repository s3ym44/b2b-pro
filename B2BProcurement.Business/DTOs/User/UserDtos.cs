using System.ComponentModel.DataAnnotations;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.DTOs.User
{
    /// <summary>
    /// Kullanıcı detay DTO'su.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Kullanıcı oluşturma DTO'su.
    /// </summary>
    public class UserCreateDto
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [MaxLength(100, ErrorMessage = "Şifre en fazla 100 karakter olabilir.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur.")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Şirket seçimi zorunludur.")]
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// Giriş DTO'su.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Kayıt DTO'su.
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [MaxLength(100, ErrorMessage = "Şifre en fazla 100 karakter olabilir.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur.")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        // Şirket bilgileri (yeni şirket kaydı için)
        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vergi numarası zorunludur.")]
        [MaxLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir.")]
        public string TaxNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        [Required(ErrorMessage = "Paket seçimi zorunludur.")]
        public int PackageId { get; set; }
    }

    /// <summary>
    /// Giriş sonuç DTO'su.
    /// </summary>
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
        public string? Token { get; set; }
    }
}
