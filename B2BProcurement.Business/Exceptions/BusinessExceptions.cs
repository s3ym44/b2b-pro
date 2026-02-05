namespace B2BProcurement.Business.Exceptions
{
    /// <summary>
    /// İş kuralı ihlali exception'ı.
    /// </summary>
    public class BusinessException : Exception
    {
        public string Code { get; }

        public BusinessException(string message, string code = "BUSINESS_ERROR") : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Kayıt bulunamadı exception'ı.
    /// </summary>
    public class NotFoundException : Exception
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public NotFoundException(string entityName, object entityId) 
            : base($"{entityName} bulunamadı. (Id: {entityId})")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// Yetkilendirme hatası exception'ı.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Bu işlem için yetkiniz yok.") : base(message)
        {
        }
    }

    /// <summary>
    /// Paket limit aşımı exception'ı.
    /// </summary>
    public class PackageLimitExceededException : BusinessException
    {
        public string LimitType { get; }

        public PackageLimitExceededException(string limitType, string message) 
            : base(message, "PACKAGE_LIMIT_EXCEEDED")
        {
            LimitType = limitType;
        }
    }

    /// <summary>
    /// Geçersiz durum exception'ı.
    /// </summary>
    public class InvalidStateException : BusinessException
    {
        public string CurrentState { get; }
        public string ExpectedState { get; }

        public InvalidStateException(string currentState, string expectedState, string message) 
            : base(message, "INVALID_STATE")
        {
            CurrentState = currentState;
            ExpectedState = expectedState;
        }
    }
}
