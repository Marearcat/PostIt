using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PostItCore.Filters;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        UserManager<Models.User> _userManager;
        RoleManager<IdentityRole> _roleManager;
        PostItDb context;

        public AdminController(UserManager<Models.User> userManager, RoleManager<IdentityRole> roleManager, PostItDb db)
        {
            context = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Admins()
        {
            var thisUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var users = await _userManager.GetUsersInRoleAsync("admin");
            users.Remove(users.First(x => x.Id == thisUser.Id));
            return View(users);
        }

        [LogsActionFilter]
        [Authorize(Roles = "admin")]
        public IActionResult CheckLogs()
        {
            var logs = context.Logs.OrderByDescending(x => x.Id).ToList();
            return View(logs);
        }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> CommonUsers(int page = 0)
        {
            var thisUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var users = context.Users.ToList();
            var admins = await _userManager.GetUsersInRoleAsync("admin");
            foreach(var user in admins)
            {
                users.Remove(users.First(x => x.Id == user.Id));
            }
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MakeAdmin(string Id)
        {

            if (!context.Roles.Any(x => x.Name == "admin"))
                await _roleManager.CreateAsync(new IdentityRole { Name = "admin" });
            var user = await _userManager.FindByIdAsync(Id);
            var admins = await _userManager.GetUsersInRoleAsync("admin");
            if (!admins.Contains(user))
            {
                await _userManager.AddToRoleAsync(user, "admin");
                return RedirectToAction("CommonUsers");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "admin");
                return RedirectToAction("Admins");
            }
        }
    }
}