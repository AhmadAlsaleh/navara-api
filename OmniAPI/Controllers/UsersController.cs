using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using SmartLifeLtd.Controllers;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.IServices;

namespace OmniAPI.Controllers
{
    public class UsersController : UsersController<OmniDbContext>
    {
        public UsersController(OmniDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersService usersService, IHostingEnvironment env
            ) : base(context, userManager, signInManager, usersService, env)
        {
        }
    }
}
