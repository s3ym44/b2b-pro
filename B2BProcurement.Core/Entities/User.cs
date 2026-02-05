using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Kullanıcı entity'si.
    /// Sistemdeki tüm kullanıcıları tanımlar.
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// E-posta adresi (kullanıcı adı olarak kullanılır).
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Şifrelenmiş parola.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Ad.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Soyad.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Telefon numarası.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Şirket ID (Foreign Key).
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Kullanıcı rolü.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Son giriş tarihi.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Kullanıcının ait olduğu şirket.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Kullanıcının bildirimleri.
        /// </summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        #endregion

        #region Computed Properties

        /// <summary>
        /// Kullanıcının tam adı.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        #endregion
    }
}
