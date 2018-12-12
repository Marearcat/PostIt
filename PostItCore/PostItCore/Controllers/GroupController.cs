﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    public class GroupController : Controller
    {
        UserManager<Models.User> _userManager;

        public GroupController(UserManager<Models.User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 0, string userId = "0", string filter = null)
        {
            ViewData["Title"] = "Groups";
            var context = new PostItDb(Opts());
            var groups = context.Groups.OrderByDescending(x => x.Rep).ToList();
            if (userId != "0")
            {
                var user = await _userManager.FindByIdAsync(userId);
                ViewData["Title"] = user.Nick;
                groups = groups.Where(x => context.Subscribes.Any(y => y.UserId == userId && y.GroupId == x.Id)).ToList();
            }
            var model = new ViewModels.GroupIndex
            {
                Groups = groups,
                Page = page,
                UserId = userId
            };
            if (filter != null && model.Groups.Any())
                model.Groups = model.Groups.Where(x => x.Title.ToLower().Contains(filter.ToLower())).ToList();
            if (groups != null && groups.Count > 10)
            {
                ViewData["Pages"] = groups.Count % 10 == 0 ? groups.Count / 10 : groups.Count / 10 + 1;
                if (groups.Count - page * 10 >= 9)
                    groups = groups.GetRange(page * 10, 10);
                else
                    groups = groups.GetRange(page * 10, groups.Count - page * 10);
            }
            return View(model);
        }

        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(Group model)
        {
            var context = new PostItDb(Opts());
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            context.Groups.Add(new Group
            {
                Title = model.Title,
                AdminId = user.Id,
                Desc = model.Desc,
                Rep = 0
            });
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Info(int Id)
        {
            var context = new PostItDb(Opts());
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            var model = new ViewModels.GroupInfo
            {
                UserId = currentUser.Id
            };
            if (context.Subscribes.Any(x => x.UserId == model.UserId && x.GroupId == Id))
                model.Sub = true;
            else
                model.Sub = false;
            model.Group = context.Groups.First(x => x.Id == Id);
            ViewData["Title"] = model.Group.Title;
            var admin = await _userManager.FindByIdAsync(model.Group.AdminId);
            model.AdminNick = admin.Nick;
            return View(model);
        }
        [HttpPost]
        public IActionResult Info(int Id, bool sub, string userId)
        {
            var context = new PostItDb(Opts());
            if (sub)
                context.Subscribes.Add(new Subscribe { UserId = userId, GroupId = Id });
            else
                context.Subscribes.Remove(context.Subscribes.First(x => x.GroupId == Id && x.UserId == userId));
            context.SaveChangesAsync();
            return RedirectPermanent(@"~/Post/Index?groupId=" + Id);
        }


        public IActionResult Delete(int Id)
        {
            var context = new PostItDb(Opts());
            context.Subscribes.RemoveRange(context.Subscribes.Where(x => x.GroupId == Id));
            context.Posts.RemoveRange(context.Posts.Where(x => x.GroupId == Id));
            context.Groups.Remove(context.Groups.First(x => x.Id == Id));
            context.SaveChangesAsync();
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