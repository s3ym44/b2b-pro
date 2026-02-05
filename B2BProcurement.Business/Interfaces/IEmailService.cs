namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// E-posta servisi arayüzü.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// E-posta gönderir.
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Birden fazla alıcıya e-posta gönderir.
        /// </summary>
        Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// CC ve BCC ile e-posta gönderir.
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, 
            IEnumerable<string>? cc = null, 
            IEnumerable<string>? bcc = null, 
            bool isHtml = true);

        /// <summary>
        /// Şablon ile e-posta gönderir.
        /// </summary>
        Task<bool> SendTemplateEmailAsync(string to, string templateName, object model);

        #region Pre-defined Emails

        /// <summary>
        /// Yeni RFQ bildirimi e-postası gönderir.
        /// </summary>
        Task SendNewRfqEmailAsync(int rfqId, IEnumerable<string> recipientEmails);

        /// <summary>
        /// Yeni teklif bildirimi e-postası gönderir.
        /// </summary>
        Task SendNewQuotationEmailAsync(int quotationId);

        /// <summary>
        /// Teklif onay e-postası gönderir.
        /// </summary>
        Task SendQuotationApprovedEmailAsync(int quotationId);

        /// <summary>
        /// Teklif red e-postası gönderir.
        /// </summary>
        Task SendQuotationRejectedEmailAsync(int quotationId);

        /// <summary>
        /// RFQ süre dolumu e-postası gönderir.
        /// </summary>
        Task SendRfqExpiringEmailAsync(int rfqId, int daysRemaining);

        /// <summary>
        /// Hoş geldiniz e-postası gönderir.
        /// </summary>
        Task SendWelcomeEmailAsync(int userId);

        /// <summary>
        /// Şifre sıfırlama e-postası gönderir.
        /// </summary>
        Task SendPasswordResetEmailAsync(string email, string resetToken);

        #endregion
    }

    /// <summary>
    /// SMTP konfigürasyon ayarları.
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "B2B Procurement";
        public bool EnableSsl { get; set; } = true;
    }
}
