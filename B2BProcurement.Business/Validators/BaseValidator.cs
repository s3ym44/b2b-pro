using FluentValidation;

namespace B2BProcurement.Business.Validators
{
    /// <summary>
    /// Örnek validator sınıfı.
    /// FluentValidation kullanarak doğrulama kuralları tanımlar.
    /// </summary>
    /// <typeparam name="T">Doğrulanacak tip.</typeparam>
    public abstract class BaseValidator<T> : AbstractValidator<T> where T : class
    {
        /// <summary>
        /// BaseValidator yapıcı metodu.
        /// Alt sınıflar burada kendi kurallarını tanımlamalıdır.
        /// </summary>
        protected BaseValidator()
        {
            // Alt sınıflarda RuleFor() kullanılarak kurallar tanımlanır.
        }
    }
}
