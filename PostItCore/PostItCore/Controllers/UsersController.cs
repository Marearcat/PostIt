using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PostItCore.Models;
using PostItCore.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Collections.Generic;

namespace PostItCore.Controllers
{
    public class UsersController : Controller
    {
        UserManager<Models.User> _userManager;

        public UsersController(UserManager<Models.User> userManager)
        {
                _userManager = userManager;
        }

        public IActionResult Index(int page = 0, int groupId = 0, string filter = null)
        {
            ViewData["Title"] = "Users";
            var context = new PostItDb(Opts());
            var users = _userManager.Users.OrderByDescending(x => x.Rep).ToList();
            if (groupId != 0)
            {
                ViewData["Title"] = context.Groups.First(x => x.Id == groupId).Title;
                users = users.Where(x => context.Subscribes.Any(y => y.UserId == x.Id && y.GroupId == groupId)).ToList();
            }
            var model = new ViewModels.UsersIndex
            {
                Users = users,
                Page = page,
                GroupId = groupId
            };
            if (filter != null)
                model.Users = model.Users.Where(x => x.Nick.ToLower().Contains(filter.ToLower())).ToList();
            if (users != null && users.Count > 10)
            {
                ViewData["Pages"] = users.Count % 10 == 0 ? users.Count / 10 : users.Count / 10 + 1;
                if (users.Count - page * 10 >= 9)
                    users = users.GetRange(page * 10, 10);
                else
                    users = users.GetRange(page * 10, users.Count - page * 10);
            }
            return View(model);
        }

        public IActionResult Info(string Id) => RedirectPermanent(@"~/Post/Index?userId=" + Id);

        public async Task<IActionResult> Type(string Id)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var model = new ViewModels.Type { DestId = Id, DepId = user.Id, DepName = user.Nick };
            return View(model);
        }
        [HttpPost]
        public IActionResult Type(ViewModels.Type model)
        {
            var context = new PostItDb(Opts());
            context.Messages.Add(new Models.Mail { DepId = model.DepId, DestId = model.DestId, Text = model.Text, Date = DateTime.Now });
            context.SaveChangesAsync();
            return RedirectToAction("Mail");
        }

        public async Task<IActionResult> Mail(int page = 0, string filter = null)
        {
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            var context = new PostItDb(Opts());
            var sms = context.Messages.Where(x => x.DestId == currentUser.Id).OrderByDescending(x => x.Date).ToList();
            var model = new List<ViewModels.Mail>();
            foreach(var msg in sms)
            {
                var user = _userManager.Users.First(x => x.Id == msg.DepId);
                model.Add(new ViewModels.Mail { DepId = msg.DepId, Page = page, Text = msg.Text, DepName = user.Nick });
            }
            if (filter != null)
                model = model.Where(x => x.DepName.ToLower().Contains(filter.ToLower()) || x.Text.ToLower().Contains(filter.ToLower())).ToList();
            if (sms != null && sms.Count > 10)
            {
                ViewData["Pages"] = sms.Count % 10 == 0 ? sms.Count / 10 : sms.Count / 10 + 1;
                if (sms.Count - page * 10 >= 9)
                    sms = sms.GetRange(page * 10, 10);
                else
                    sms = sms.GetRange(page * 10, sms.Count - page * 10);
            }
            return View(model);
        }

        public static DbContextOptions<PostItDb> Opts()
        {
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("appsettings.json");
            // создаем конфигурацию
            var config = builder.Build();
            // получаем строку подключения
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PostItDb>();
            return optionsBuilder
                .UseSqlServer(connectionString)
                .Options;
        }

    }

}