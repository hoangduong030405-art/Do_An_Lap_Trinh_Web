namespace WebBanHang.CoreBusiness.Exceptions
{
    public class CreditLimitExceededException : BusinessRuleException
    {
        public CreditLimitExceededException(string message) : base(message)
        {
        }
    }
}
