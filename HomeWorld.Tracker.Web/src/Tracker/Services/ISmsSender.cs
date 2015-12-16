using System.Threading.Tasks;

namespace HomeWorld.Tracker.Web.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
