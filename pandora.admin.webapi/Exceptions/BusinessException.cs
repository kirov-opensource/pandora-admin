namespace Pandora.Admin.WebAPI.Exceptions
{
    public class BusinessException : ApplicationException
    {

        public BusinessException() { }

        public BusinessException(string message) : base(message)
        {
        }
    }
}
