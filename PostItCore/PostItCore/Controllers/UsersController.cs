using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PostItCore.Models;
using PostItCore.ViewModels;
using System.Security.Claims;

namespace PostItCore.Controllers
{
    public class UsersController : Controller
    {
            UserManager<Models.User> _userManager;

            public UsersController(UserManager<Models.User> userManager)
            {
                _userManager = userManager;
            }

            public IActionResult Index() => View(_userManager.Users.ToList());

            

    }
    
}