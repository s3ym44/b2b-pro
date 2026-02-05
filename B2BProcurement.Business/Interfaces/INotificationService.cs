using B2BProcurement.Business.DTOs.Notification;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Bildirim servisi arayüzü.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Bildirimi kimliğine göre getirir.
        /// </summary>
        /// <param name="id">Bildirim kimliği.</param>
        /// <returns>Bildirim bilgileri.</returns>
        Task<NotificationDto?> GetByIdAsync(int id);

        /// <summary>
        /// Kullanıcının tüm bildirimlerini listeler.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <returns>Bildirim listesi.</returns>
        Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId);

        /// <summary>
        /// Kullanıcının okunmamış bildirimlerini listeler.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <returns>Okunmamış bildirim listesi.</returns>
        Task<IEnumerable<NotificationDto>> GetUnreadAsync(int userId);

        /// <summary>
        /// Kullanıcının okunmamış bildirim sayısını getirir.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <returns>Okunmamış bildirim sayısı.</returns>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Yeni bildirim oluşturur.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        /// <param name="title">Bildirim başlığı.</param>
        /// <param name="message">Bildirim mesajı.</param>
        /// <param name="type">Bildirim tipi.</param>
        /// <param name="relatedEntityId">İlgili entity kimliği (opsiyonel).</param>
        /// <param name="relatedEntityType">İlgili entity tipi (opsiyonel).</param>
        Task CreateAsync(int userId, string title, string message, NotificationType type, 
            int? relatedEntityId = null, string? relatedEntityType = null);

        /// <summary>
        /// Şirketteki tüm kullanıcılara bildirim gönderir.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="title">Bildirim başlığı.</param>
        /// <param name="message">Bildirim mesajı.</param>
        /// <param name="type">Bildirim tipi.</param>
        Task CreateForCompanyAsync(int companyId, string title, string message, NotificationType type);

        /// <summary>
        /// Bildirimi okundu olarak işaretler.
        /// </summary>
        /// <param name="notificationId">Bildirim kimliği.</param>
        Task MarkAsReadAsync(int notificationId);

        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler.
        /// </summary>
        /// <param name="userId">Kullanıcı kimliği.</param>
        Task MarkAllAsReadAsync(int userId);

        /// <summary>
        /// Bildirimi siler.
        /// </summary>
        /// <param name="notificationId">Bildirim kimliği.</param>
        Task DeleteAsync(int notificationId);

        /// <summary>
        /// Eski bildirimleri temizler.
        /// </summary>
        /// <param name="olderThanDays">Gün sayısı.</param>
        /// <returns>Silinen bildirim sayısı.</returns>
        Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30);

        #region Önceden Tanımlı Bildirimler

        /// <summary>
        /// Yeni RFQ bildirimi gönderir.
        /// </summary>
        Task NotifyNewRfqAsync(int rfqId);

        /// <summary>
        /// Yeni teklif bildirimi gönderir.
        /// </summary>
        Task NotifyNewQuotationAsync(int quotationId);

        /// <summary>
        /// Teklif onay bildirimi gönderir.
        /// </summary>
        Task NotifyQuotationApprovedAsync(int quotationId);

        /// <summary>
        /// Teklif red bildirimi gönderir.
        /// </summary>
        Task NotifyQuotationRejectedAsync(int quotationId);

        /// <summary>
        /// RFQ süre dolumu bildirimi gönderir.
        /// </summary>
        Task NotifyRfqExpiringAsync(int rfqId, int daysRemaining);

        #endregion
    }
}
