
namespace shaco.Base
{
	public interface ILog
    {
        void InfoDelegate(string message);
        void InfoFormatDelegate(string message, params object[] args);
        void WarningDelegate(string message);
        void ErrorDelegate(string message);
        void ExceptionDelegate(string message);
    }
}
