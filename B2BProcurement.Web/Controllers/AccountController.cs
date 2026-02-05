using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using B2BProcurement.Business.DTOs.User;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Kullanıcı kimlik doğrulama controller'ı.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICompanyService _companyService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            IAuthService authService,
            ICompanyService companyService,
            ApplicationDbContext context)
        {
            _authService = authService;
            _companyService = companyService;
            _context = context;
        }

        #region Login

        /// <summary>
        /// Login sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login işlemi.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.LoginAsync(model);
                
                if (result.Success && result.User != null)
                {
                    // Claims oluştur
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                        new Claim(ClaimTypes.Name, $"{result.User.FirstName} {result.User.LastName}"),
                        new Claim(ClaimTypes.Email, result.User.Email),
                        new Claim(ClaimTypes.Role, result.User.Role.ToString()),
                        new Claim("CompanyId", result.User.CompanyId.ToString()),
                        new Claim("CompanyName", result.User.CompanyName ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Return URL varsa oraya yönlendir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, result.Message ?? "Giriş başarısız.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Register

        /// <summary>
        /// Kayıt sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            await LoadRegisterViewData();
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// Kayıt işlemi.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRegisterViewData();
                return View(model);
            }

            // Vergi numarası format kontrolü
            if (!IsValidTaxNumber(model.TaxNumber))
            {
                ModelState.AddModelError("TaxNumber", "Geçersiz vergi numarası formatı.");
                await LoadRegisterViewData();
                return View(model);
            }

            // E-posta benzersizlik kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                await LoadRegisterViewData();
                return View(model);
            }

            // Vergi numarası benzersizlik kontrolü
            if (await _context.Companies.AnyAsync(c => c.TaxNumber == model.TaxNumber))
            {
                ModelState.AddModelError("TaxNumber", "Bu vergi numarası zaten kayıtlı.");
                await LoadRegisterViewData();
                return View(model);
            }

            try
            {
                var registerDto = new RegisterDto
                {
                    CompanyName = model.CompanyName,
                    TaxNumber = model.TaxNumber,
                    SectorId = model.SectorId,
                    PackageId = model.PackageId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password
                };

                var result = await _authService.RegisterAsync(registerDto);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, "Kayıt işlemi başarısız.");
                await LoadRegisterViewData();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadRegisterViewData();
                return View(model);
            }
        }

        private async Task LoadRegisterViewData()
        {
            var sectors = await _context.Sectors
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            // SQLite decimal ORDER BY desteklemediği için istemci tarafında sıralıyoruz
            var packages = await _context.Packages
                .Where(p => p.IsActive)
                .ToListAsync();
            packages = packages.OrderBy(p => p.Price).ToList();

            ViewBag.Sectors = sectors;
            ViewBag.Packages = packages;
        }

        private bool IsValidTaxNumber(string taxNumber)
        {
            if (string.IsNullOrEmpty(taxNumber)) return false;
            
            // Türkiye vergi numarası: 10 veya 11 haneli
            taxNumber = taxNumber.Trim();
            if (taxNumber.Length != 10 && taxNumber.Length != 11) return false;
            
            return taxNumber.All(char.IsDigit);
        }

        #endregion

        #region Logout

        /// <summary>
        /// Çıkış işlemi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        #endregion

        #region Forgot Password

        /// <summary>
        /// Şifremi unuttum sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Şifremi unuttum işlemi.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            
            // Güvenlik için kullanıcı bulunamasa bile aynı mesajı göster
            TempData["SuccessMessage"] = "Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi.";
            
            if (user != null)
            {
                await _authService.RequestPasswordResetAsync(model.Email);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        /// <summary>
        /// Şifremi unuttum onay sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Reset Password

        /// <summary>
        /// Şifre sıfırlama sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Geçersiz token.");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        /// <summary>
        /// Şifre sıfırlama işlemi.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _authService.ResetPasswordAsync(model.Token, model.NewPassword);
                
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Access Denied

        /// <summary>
        /// Erişim reddedildi sayfası.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }

    #region ViewModels

    public class RegisterViewModel
    {
        // Step 1: Firma Bilgileri
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şirket unvanı zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Şirket Unvanı")]
        public string CompanyName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vergi numarası zorunludur.")]
        [System.ComponentModel.DataAnnotations.StringLength(11, MinimumLength = 10, ErrorMessage = "Vergi numarası 10 veya 11 haneli olmalıdır.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Vergi Numarası")]
        public string TaxNumber { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Sektör")]
        public int SectorId { get; set; }

        // Step 2: Kullanıcı Bilgileri
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ad zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Soyad zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "E-posta zorunludur.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre zorunludur.")]
        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Step 3: Paket Seçimi
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Paket seçimi zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Paket")]
        public int PackageId { get; set; }

        // Terms
        [MustBeTrue(ErrorMessage = "Kullanım şartlarını kabul etmelisiniz.")]
        public bool AcceptTerms { get; set; }
    }

    /// <summary>
    /// Boolean değerin true olmasını zorunlu kılan validation attribute.
    /// </summary>
    public class MustBeTrueAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool boolValue && boolValue;
        }
    }

    public class ForgotPasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "E-posta zorunludur.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Token { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    #endregion
}
