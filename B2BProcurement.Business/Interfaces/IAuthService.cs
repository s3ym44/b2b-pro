using B2BProcurement.Business.DTOs.User;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Kimlik doğrulama servisi arayüzü.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Kullanıcı girişi yapar.
        /// </summary>
        /// <param name="dto">Giriş bilgileri.</param>
        /// <returns>Giriş yapan kullanıcı bilgileri veya null.</returns>
        Task<LoginResultDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Yeni kullanıcı ve şirket kaydı oluşturur.
        /// </summary>
        /// <param name="dto">Kayıt bilgileri.</param>
        /// <returns>Oluşturulan kullanıcı bilgileri.</returns>
        Task<UserDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Kullanıcı şifresini değiştirir.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <param name="oldPassword">Mevcut şifre.</param>
        /// <param name="newPassword">Yeni şifre.</param>
        /// <returns>Başarılı ise true.</returns>
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        /// <summary>
        /// Kullanıcının oturumunu sonlandırır.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        Task LogoutAsync(int userId);

        /// <summary>
        /// Şifre sıfırlama talebi oluşturur.
        /// </summary>
        /// <param name="email">Kullanıcı e-posta adresi.</param>
        /// <returns>Başarılı ise true.</returns>
        Task<bool> RequestPasswordResetAsync(string email);

        /// <summary>
        /// Şifre sıfırlama işlemini tamamlar.
        /// </summary>
        /// <param name="token">Sıfırlama token'ı.</param>
        /// <param name="newPassword">Yeni şifre.</param>
        /// <returns>Başarılı ise true.</returns>
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        /// <summary>
        /// Kullanıcıyı kimliğine göre getirir.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <returns>Kullanıcı bilgileri.</returns>
        Task<UserDto?> GetUserByIdAsync(int userId);

        /// <summary>
        /// E-posta adresine göre kullanıcı getirir.
        /// </summary>
        /// <param name="email">E-posta adresi.</param>
        /// <returns>Kullanıcı bilgileri.</returns>
        Task<UserDto?> GetUserByEmailAsync(string email);
    }
}
