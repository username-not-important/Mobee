namespace Mobee.Client.WPF.Logs
{
    public interface ILogger
    {
        void Log(string operation, string channel);
    }
}