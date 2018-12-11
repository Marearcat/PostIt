using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    public class PostController : Controller
    {
        private UserManager<User> _userManager;

        public PostController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index(int page = 0)
        {
            var context = new PostItDb(Opts());
            var posts = context.Posts.OrderByDescending(x => x.Date).ToList();
            if (posts != null && posts.Count > 10)
            {
                ViewData["Pages"] = posts.Count % 10 == 0 ? posts.Count / 10 : posts.Count / 10 + 1;
                if (posts.Count - page * 10 >= 9)
                    posts = posts.GetRange(page * 10, 10);
                else
                    posts = posts.GetRange(page * 10, posts.Count - page * 10 + 1);
            }
            return View(posts);
        }

        public async Task<IActionResult> Info(int postId)
        {
            var context = new PostItDb(Opts());
            var model = new ViewModels.PostInfo();
            var post = context.Posts.Where(x => x.Id == postId).First();
            model.Id = post.Id;
            model.Rep = post.Rep;
            model.Head = post.Head;
            model.Date = post.Date;
            model.Desc = post.Desc;
            model.GroupId = post.GroupId;
            if (model.GroupId == 0)
                model.GroupName = "Global";
            else
                model.GroupName = context.Groups
                                        .Where(x => x.Id == model.Id)
                                        .First()
                                        .Title;
            model.UserId = post.UserId;
            var user = await _userManager.FindByIdAsync(model.UserId);
            model.UserNick = user.Nick;
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            model.Favor = context.Favors.Any(x => x.UserId == currentUser.Id && x.PostId == postId);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Info(int postId, bool favor)
        {
            var context = new PostItDb(Opts());
            var groupId = context.Posts.Where(x => x.Id == postId).First().GroupId;
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (favor)
            {
                context.Favors.Add(new Favor { UserId = currentUser.Id, PostId = postId });
                context.Users.Where(x => x.Id == currentUser.Id).First().Rep++;
                context.Posts.Where(x => x.Id == postId).First().Rep++;
                if (groupId != 0)
                    context.Groups.Where(x => x.Id == groupId).First().Rep++;
                await context.SaveChangesAsync();
            }
            else
            {
                context.Favors.Remove(context.Favors.Where(x=> x.PostId == postId && x.UserId == currentUser.Id).First());
                context.Users.Where(x => x.Id == currentUser.Id).First().Rep--;
                context.Posts.Where(x => x.Id == postId).First().Rep--;
                if (groupId != 0)
                    context.Groups.Where(x => x.Id == groupId).First().Rep--;
                await context.SaveChangesAsync();
            }
            return RedirectPermanent($"~/Post/Info?postId=" + postId);
        }

        public async Task<IActionResult> Create(int? groupId = 0)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            ViewData["userId"] = user.Id;
            ViewData["groupId"] = groupId;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Models.Post model)
        {
            var context = new PostItDb(Opts());
            context.Posts.Add(new Post { UserId = model.UserId, Date = DateTime.Now, Desc = model.Desc, GroupId = model.GroupId, Head = model.Head, Rep = 0 });
            context.SaveChanges();
            return RedirectToAction("Index");
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