using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Language/Culture controller for switching languages.
    /// </summary>
    public class CultureController : Controller
    {
        /// <summary>
        /// Changes the current culture and stores it in a cookie.
        /// </summary>
        /// <param name="culture">Culture code (e.g., "tr-TR", "en-US")</param>
        /// <param name="returnUrl">URL to redirect after changing culture</param>
        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            // Validate culture
            var supportedCultures = new[] { "tr-TR", "en-US" };
            if (!supportedCultures.Contains(culture))
            {
                culture = "tr-TR";
            }

            // Set culture cookie
            Response.Cookies.Append(
                "B2BProcurement.Culture",
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Redirect back
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
