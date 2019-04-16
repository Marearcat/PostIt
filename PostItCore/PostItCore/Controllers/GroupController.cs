using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        UserManager<Models.User> _userManager;
        PostItDb context;

        public GroupController(UserManager<Models.User> userManager, PostItDb db)
        {
            context = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 0, string userName = "0", string filter = "")
        {
            if (filter == null)
                filter = "";
            ViewData["Title"] = "Groups";
            var groups = context.Groups.OrderByDescending(x => x.Rep).ToList();
            var user = new Models.User();
            string userId = "0";
            if (userName != "0")
            {
                user = await _userManager.FindByEmailAsync(userName);
                userId = user.Id;
                ViewData["Title"] = user.Nick;
                groups = groups.Where(x => context.Subscribes.Any(y => y.UserId == user.Id && y.GroupId == x.Id) || x.AdminId == user.Id).ToList();
            }
           
            groups = groups.Where(x => x.Title.ToLower().Contains(filter.ToLower())).ToList();
            ViewData["Filter"] = filter;
            
            if (groups != null && groups.Count > 10)
            {
                ViewData["Pages"] = groups.Count % 10 == 0 ? groups.Count / 10 : groups.Count / 10 + 1;
                if (groups.Count - page * 10 > 9)
                    groups = groups.GetRange(page * 10, 10);
                else
                    groups = groups.GetRange(page * 10, groups.Count - page * 10);
            }
            var model = new ViewModels.GroupIndex
            {
                Groups = groups,
                Page = page,
                UserId = userId
            };
            return View(model);
        }

        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(Group model)
        {
            try
            {
                if (model.Title != null && model.Desc != null && !context.Groups.Any(x => x.Title == model.Title))
                {
                    var user = await _userManager.FindByEmailAsync(User.Identity.Name);
                    
                    context.Groups.Add(new Group
                    {
                        Title = model.Title,
                        AdminId = user.Id,
                        Desc = model.Desc,
                        Rep = 0
                    });
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                context.Logs.Add(new Log { UserName = User.Identity.Name, State = "/CreateGroup?" + "title=" + HttpContext.Request.Form["Title"] + "&desc=" + HttpContext.Request.Form["Desc"] });
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Info(int Id)
        {
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
            if (userId != null && Id != 0)
            {
                if (sub)
                    context.Subscribes.Add(new Subscribe { UserId = userId, GroupId = Id });
                else
                    context.Subscribes.Remove(context.Subscribes.First(x => x.GroupId == Id && x.UserId == userId));
                context.SaveChangesAsync();
            }
            return RedirectPermanent(@"~/Group/Info?Id=" + Id);
        }


        public IActionResult Delete(int Id)
        {
            var posts = context.Posts.Where(x => x.GroupId == Id);
            foreach(var post in posts)
            {
                var comments = context.Comments.Where(x => x.PostId == post.Id);
                foreach (var comment in comments)
                {
                    context.Favors.RemoveRange(context.Favors.Where(x => x.PostId == comment.Id && !x.IsPost));
                    context.Comments.Remove(comment);
                }
                context.Favors.RemoveRange(context.Favors.Where(x => x.PostId == post.Id && x.IsPost));
                context.Posts.Remove(context.Posts.First(x => x.Id == post.Id));
            }
            context.Subscribes.RemoveRange(context.Subscribes.Where(x => x.GroupId == Id));
            //context.Posts.RemoveRange(context.Posts.Where(x => x.GroupId == Id));
            context.Groups.Remove(context.Groups.First(x => x.Id == Id));
            context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
    }
}