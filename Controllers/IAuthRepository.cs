using System.Threading.Tasks;

using KOLConsole.Models;

namespace KOLConsole.Controllers
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string username, string password);

    }
}
