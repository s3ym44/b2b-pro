using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Data.Context;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// E-posta servisi implementasyonu.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<SmtpSettings> settings,
            ApplicationDbContext context,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _context = context;
            _logger = logger;
        }

        #region Core Send Methods

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(new[] { to }, subject, body, isHtml);
        }

        public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(to.First(), subject, body, to.Skip(1), null, isHtml);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                if (cc != null)
                {
                    foreach (var email in cc)
                    {
                        mailMessage.CC.Add(email);
                    }
                }

                if (bcc != null)
                {
                    foreach (var email in bcc)
                    {
                        mailMessage.Bcc.Add(email);
                    }
                }

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
                return false;
            }
        }

        public async Task<bool> SendTemplateEmailAsync(string to, string templateName, object model)
        {
            // Template rendering would be implemented here
            // For now, just return false as placeholder
            _logger.LogWarning("Template email not implemented: {Template}", templateName);
            return await Task.FromResult(false);
        }

        #endregion

        #region Pre-defined Emails

        public async Task SendNewRfqEmailAsync(int rfqId, IEnumerable<string> recipientEmails)
        {
            var rfq = await _context.RFQs.FirstOrDefaultAsync(r => r.Id == rfqId);
            if (rfq == null) return;

            var subject = $"🆕 Yeni Teklif Talebi: {rfq.Title}";
            var body = GetEmailTemplate("new_rfq", new
            {
                rfq.Title,
                rfq.RfqNumber,
                EndDate = rfq.EndDate.ToString("dd.MM.yyyy")
            });

            foreach (var email in recipientEmails)
            {
                await SendEmailAsync(email, subject, body, true);
            }
        }

        public async Task SendNewQuotationEmailAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .ThenInclude(r => r!.Company)
                .Include(q => q.SupplierCompany)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation?.RFQ?.Company == null) return;

            var emails = await _context.Users
                .Where(u => u.CompanyId == quotation.RFQ.CompanyId && u.IsActive)
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToListAsync();

            var subject = $"📥 Yeni Teklif Alındı: {quotation.RFQ.Title}";
            var body = GetEmailTemplate("new_quotation", new
            {
                RfqTitle = quotation.RFQ.Title,
                SupplierName = quotation.SupplierCompany?.CompanyName ?? "Tedarikçi",
                quotation.TotalAmount,
                quotation.QuotationNumber
            });

            foreach (var email in emails.Where(e => e != null))
            {
                await SendEmailAsync(email!, subject, body, true);
            }
        }

        public async Task SendQuotationApprovedEmailAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation == null) return;

            var emails = await _context.Users
                .Where(u => u.CompanyId == quotation.SupplierCompanyId && u.IsActive)
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToListAsync();

            var subject = $"✅ Teklifiniz Onaylandı: {quotation.RFQ?.Title}";
            var body = GetEmailTemplate("quotation_approved", new
            {
                RfqTitle = quotation.RFQ?.Title,
                quotation.QuotationNumber,
                quotation.TotalAmount
            });

            foreach (var email in emails.Where(e => e != null))
            {
                await SendEmailAsync(email!, subject, body, true);
            }
        }

        public async Task SendQuotationRejectedEmailAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation == null) return;

            var emails = await _context.Users
                .Where(u => u.CompanyId == quotation.SupplierCompanyId && u.IsActive)
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToListAsync();

            var subject = $"❌ Teklifiniz Reddedildi: {quotation.RFQ?.Title}";
            var body = GetEmailTemplate("quotation_rejected", new
            {
                RfqTitle = quotation.RFQ?.Title,
                quotation.QuotationNumber
            });

            foreach (var email in emails.Where(e => e != null))
            {
                await SendEmailAsync(email!, subject, body, true);
            }
        }

        public async Task SendRfqExpiringEmailAsync(int rfqId, int daysRemaining)
        {
            var rfq = await _context.RFQs.FirstOrDefaultAsync(r => r.Id == rfqId);
            if (rfq == null) return;

            var emails = await _context.Users
                .Where(u => u.CompanyId == rfq.CompanyId && u.IsActive)
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToListAsync();

            var subject = $"⏰ RFQ Süresi Dolmak Üzere: {rfq.Title}";
            var body = GetEmailTemplate("rfq_expiring", new
            {
                rfq.Title,
                rfq.RfqNumber,
                DaysRemaining = daysRemaining,
                EndDate = rfq.EndDate.ToString("dd.MM.yyyy HH:mm")
            });

            foreach (var email in emails.Where(e => e != null))
            {
                await SendEmailAsync(email!, subject, body, true);
            }
        }

        public async Task SendWelcomeEmailAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var subject = "🎉 B2B Procurement'a Hoş Geldiniz!";
            var body = GetEmailTemplate("welcome", new
            {
                user.FirstName,
                user.LastName,
                CompanyName = user.Company?.CompanyName ?? ""
            });

            await SendEmailAsync(user.Email, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var subject = "🔐 Şifre Sıfırlama";
            var body = GetEmailTemplate("password_reset", new
            {
                ResetLink = $"https://yoursite.com/Account/ResetPassword?token={resetToken}"
            });

            await SendEmailAsync(email, subject, body, true);
        }

        #endregion

        #region Template Helper

        private string GetEmailTemplate(string templateName, object model)
        {
            // Simple template engine - in production use Razor or similar
            var baseTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }
        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #8B5CF6, #7C3AED); color: white; padding: 30px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 30px; }
        .content h2 { color: #333; margin-top: 0; }
        .info-box { background: #f8f9fa; border-radius: 6px; padding: 15px; margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e5e7eb; }
        .info-row:last-child { border-bottom: none; }
        .info-label { color: #666; }
        .info-value { font-weight: 600; color: #333; }
        .btn { display: inline-block; background: #8B5CF6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 20px; }
        .footer { background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>B2B Procurement</h1>
        </div>
        <div class='content'>
            {{CONTENT}}
        </div>
        <div class='footer'>
            Bu e-posta B2B Procurement sistemi tarafından otomatik olarak gönderilmiştir.<br>
            © 2026 B2B Procurement. Tüm hakları saklıdır.
        </div>
    </div>
</body>
</html>";

            var content = templateName switch
            {
                "new_rfq" => $@"
                    <h2>🆕 Yeni Teklif Talebi</h2>
                    <p>Sektörünüzde yeni bir teklif talebi yayınlandı.</p>
                    <div class='info-box'>
                        <div class='info-row'><span class='info-label'>Başlık</span><span class='info-value'>{{Title}}</span></div>
                        <div class='info-row'><span class='info-label'>RFQ No</span><span class='info-value'>{{RfqNumber}}</span></div>
                        <div class='info-row'><span class='info-label'>Son Teklif Tarihi</span><span class='info-value'>{{EndDate}}</span></div>
                    </div>
                    <a href='#' class='btn'>Detayları Görüntüle</a>",

                "new_quotation" => $@"
                    <h2>📥 Yeni Teklif Alındı</h2>
                    <p>RFQ'nuza yeni bir teklif geldi.</p>
                    <div class='info-box'>
                        <div class='info-row'><span class='info-label'>RFQ</span><span class='info-value'>{{RfqTitle}}</span></div>
                        <div class='info-row'><span class='info-label'>Tedarikçi</span><span class='info-value'>{{SupplierName}}</span></div>
                        <div class='info-row'><span class='info-label'>Teklif No</span><span class='info-value'>{{QuotationNumber}}</span></div>
                        <div class='info-row'><span class='info-label'>Toplam Tutar</span><span class='info-value'>{{TotalAmount}} TRY</span></div>
                    </div>
                    <a href='#' class='btn'>Teklifi İncele</a>",

                "quotation_approved" => $@"
                    <h2>✅ Tebrikler! Teklifiniz Onaylandı</h2>
                    <p>Verdiğiniz teklif onaylandı.</p>
                    <div class='info-box'>
                        <div class='info-row'><span class='info-label'>RFQ</span><span class='info-value'>{{RfqTitle}}</span></div>
                        <div class='info-row'><span class='info-label'>Teklif No</span><span class='info-value'>{{QuotationNumber}}</span></div>
                    </div>
                    <a href='#' class='btn'>Detayları Görüntüle</a>",

                "quotation_rejected" => $@"
                    <h2>❌ Teklifiniz Reddedildi</h2>
                    <p>Maalesef verdiğiniz teklif reddedildi.</p>
                    <div class='info-box'>
                        <div class='info-row'><span class='info-label'>RFQ</span><span class='info-value'>{{RfqTitle}}</span></div>
                        <div class='info-row'><span class='info-label'>Teklif No</span><span class='info-value'>{{QuotationNumber}}</span></div>
                    </div>",

                "rfq_expiring" => $@"
                    <h2>⏰ RFQ Süresi Dolmak Üzere</h2>
                    <p>RFQ'nuzun süresi dolmak üzere, lütfen gerekli aksiyonları alın.</p>
                    <div class='info-box'>
                        <div class='info-row'><span class='info-label'>RFQ</span><span class='info-value'>{{Title}}</span></div>
                        <div class='info-row'><span class='info-label'>RFQ No</span><span class='info-value'>{{RfqNumber}}</span></div>
                        <div class='info-row'><span class='info-label'>Kalan Süre</span><span class='info-value'>{{DaysRemaining}} gün</span></div>
                        <div class='info-row'><span class='info-label'>Bitiş Tarihi</span><span class='info-value'>{{EndDate}}</span></div>
                    </div>
                    <a href='#' class='btn'>RFQ'ya Git</a>",

                "welcome" => $@"
                    <h2>🎉 Hoş Geldiniz!</h2>
                    <p>Merhaba {{FirstName}} {{LastName}},</p>
                    <p>B2B Procurement platformuna hoş geldiniz. Artık tedarik süreçlerinizi dijital olarak yönetebilirsiniz.</p>
                    <a href='#' class='btn'>Platformu Keşfet</a>",

                "password_reset" => $@"
                    <h2>🔐 Şifre Sıfırlama</h2>
                    <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın.</p>
                    <a href='{{ResetLink}}' class='btn'>Şifremi Sıfırla</a>
                    <p style='margin-top: 20px; font-size: 12px; color: #666;'>Bu isteği siz yapmadıysanız bu e-postayı görmezden gelebilirsiniz.</p>",

                _ => "<p>Bildirim içeriği</p>"
            };

            // Simple placeholder replacement
            var result = baseTemplate.Replace("{{CONTENT}}", content);

            // Replace model properties
            if (model != null)
            {
                foreach (var prop in model.GetType().GetProperties())
                {
                    var value = prop.GetValue(model)?.ToString() ?? "";
                    result = result.Replace($"{{{{{prop.Name}}}}}", value);
                }
            }

            return result;
        }

        #endregion
    }
}
