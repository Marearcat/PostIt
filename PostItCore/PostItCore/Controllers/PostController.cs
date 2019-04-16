using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private UserManager<User> _userManager;
        PostItDb context;

        public PostController(UserManager<User> userManager, PostItDb db)
        {
            _userManager = userManager;
            context = db;
        }

        public IActionResult Index(int page = 0, string userId = "0", int groupId = 0)
        {
            ViewData["Title"] = "Posts";
            var posts = context.Posts.OrderByDescending(x => x.Date).ToList();
            if(userId != "0")
            {
                posts = posts.Where(x => x.UserId == userId).ToList();
            }
            else if(groupId != 0)
            {
                posts = posts.Where(x => x.GroupId == groupId).ToList();
            }
            if (posts != null && posts.Count > 10)
            {
                ViewData["Pages"] = posts.Count % 10 == 0 ? posts.Count / 10 : posts.Count / 10 + 1;
                if (posts.Count - page * 10 > 9)
                    posts = posts.GetRange(page * 10, 10);
                else
                    posts = posts.GetRange(page * 10, posts.Count - page * 10);
            }
            var model = new ViewModels.PostIndex
            {
                Posts = posts,
                UserId = userId,
                GroupId = groupId,
                Page = page
            };
            return View(model);
        }

        public async Task<IActionResult> Info(int postId)
        {
            var model = new ViewModels.PostInfo();
            var post = context.Posts.Where(x => x.Id == postId).First();
            model.Id = postId;
            model.Rep = post.Rep;
            model.Head = post.Head;
            model.Date = post.Date;
            model.Desc = post.Desc;
            model.GroupId = post.GroupId;
            if (model.GroupId == 0)
                model.GroupName = "Global";
            else
                model.GroupName = context.Groups
                                        .Where(x => x.Id == model.GroupId)
                                        .First()
                                        .Title;
            model.UserId = post.UserId;
            var user = await _userManager.FindByIdAsync(model.UserId);
            model.UserNick = user.Nick;
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
                model.Favor = context.Favors.Any(x => x.UserId == currentUser.Id && x.PostId == postId && x.IsPost);
                model.CurrentId = currentUser.Id;
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Info(int postId, bool favor, string userId)
        {
            if (userId != null && postId != 0)
            {
                var groupId = context.Posts.Where(x => x.Id == postId).First().GroupId;
                var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
                if (favor)
                {
                    context.Favors.Add(new Favor { UserId = currentUser.Id, PostId = postId, IsPost = true });
                    context.Users.Where(x => x.Id == userId).First().Rep++;
                    context.Posts.Where(x => x.Id == postId).First().Rep++;
                    if (groupId != 0)
                        context.Groups.Where(x => x.Id == groupId).First().Rep++;
                    context.SaveChanges();
                }
                else
                {
                    context.Favors.Remove(context.Favors.Where(x => x.PostId == postId && x.UserId == currentUser.Id && x.IsPost).First());
                    context.Users.Where(x => x.Id == userId).First().Rep--;
                    context.Posts.Where(x => x.Id == postId).First().Rep--;
                    if (groupId != 0)
                        context.Groups.Where(x => x.Id == groupId).First().Rep--;
                    context.SaveChanges();
                }
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
            if (model.UserId != null && model.Head != null && model.Desc != null)
            {
                context.Posts.Add(new Post { UserId = model.UserId, Date = DateTime.Now, Desc = model.Desc, GroupId = model.GroupId, Head = model.Head, Rep = 0 });
                context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CommentCreate(int postId)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            ViewData["userId"] = user.Id;
            ViewData["postId"] = postId;
            return View();
        }
        [HttpPost]
        public IActionResult CommentCreate(ViewModels.CreateComment model)
        {
            if (model.PostId != 0 && model.UserId != null && model.Desc != null)
            {
                context.Comments.Add(new Comment { UserId = model.UserId, Date = DateTime.Now, Desc = model.Desc, PostId = model.PostId, Rep = 0 });
                context.SaveChanges();
            }
            return RedirectPermanent(@"~/Post/Info?postId=" + model.PostId);
        }
        
        public async Task<IActionResult> CommentIndex(int postId, int page = 0)
        {
            var comments = context.Comments.Where(x => x.PostId == postId).OrderByDescending(x => x.Date).ToList();
            if (comments != null && comments.Count > 10)
            {
                ViewData["Pages"] = comments.Count % 10 == 0 ? comments.Count / 10 : comments.Count / 10 + 1;
                if (comments.Count - page * 10 > 9)
                    comments = comments.GetRange(page * 10, 10);
                else
                    comments = comments.GetRange(page * 10, comments.Count - page * 10);
            }
            List<ViewModels.CommentsInfo> model = new List<ViewModels.CommentsInfo>();
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            ViewData["currentId"] = currentUser.Id;
            foreach(var comment in comments)
            {
                var user = await _userManager.FindByIdAsync(comment.UserId);
                bool favor = context.Favors.Any(x => x.UserId == currentUser.Id && x.PostId == comment.Id && !x.IsPost);
                model.Add(new ViewModels.CommentsInfo { Id = comment.Id, Date = comment.Date, Desc = comment.Desc, PostId = comment.PostId, Rep = comment.Rep, UserId = comment.UserId, UserNick = user.Nick, Page = page, Favor = favor});
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CommentIndex(int postId, int commentId, int page, bool favor, string userId)
        {
            if (postId != 0 && commentId != 0 && page >= 0 && userId != null)
            {
                var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
                if (favor)
                {
                    context.Favors.Add(new Favor { UserId = currentUser.Id, PostId = commentId, IsPost = false });
                    context.Users.Where(x => x.Id == userId).First().Rep++;
                    context.Comments.Where(x => x.Id == commentId).First().Rep++;
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Favors.Remove(context.Favors.Where(x => x.PostId == commentId && x.UserId == currentUser.Id && !x.IsPost).First());
                    context.Users.Where(x => x.Id == userId).First().Rep--;
                    context.Comments.Where(x => x.Id == commentId).First().Rep--;
                    await context.SaveChangesAsync();
                }
            }
            return RedirectPermanent(@"~/Post/CommentIndex?postId=" + postId + @"&page=" + page);
        }

        public IActionResult Delete(int Id)
        {
            //context.Comments.RemoveRange(context.Comments.Where(x => x.PostId == Id));
            var comments = context.Comments.Where(x => x.PostId == Id);
            foreach(var comment in comments)
            {
                context.Favors.RemoveRange(context.Favors.Where(x => x.PostId == comment.Id && !x.IsPost));
                context.Comments.Remove(comment);
            }
            context.Favors.RemoveRange(context.Favors.Where(x => x.PostId == Id && x.IsPost));
            context.Posts.Remove(context.Posts.First(x => x.Id == Id));
            context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult CommentDelete(int commentId)
        {
            var comment = context.Comments.First(x => x.Id == commentId);
            int postId = comment.PostId;
            context.Comments.Remove(comment);
            context.Favors.RemoveRange(context.Favors.Where(x => x.PostId == commentId && !x.IsPost));
            context.SaveChangesAsync();
            return RedirectPermanent(@"~/Post/CommentIndex?postId=" + postId);
        }

        public async Task<IActionResult> Features(int page = 0, string userName = "0")
        {
            ViewData["Title"] = "Features";
            var posts = context.Posts.OrderByDescending(x => x.Date).ToList();
            var user = await _userManager.FindByEmailAsync(userName);
            posts = posts.Where(x => context.Favors.Any(y => y.IsPost && y.PostId == x.Id && y.UserId == user.Id)).ToList();
            if (posts != null && posts.Count > 10)
            {
                ViewData["Pages"] = posts.Count % 10 == 0 ? posts.Count / 10 : posts.Count / 10 + 1;
                if (posts.Count - page * 10 > 9)
                    posts = posts.GetRange(page * 10, 10);
                else
                    posts = posts.GetRange(page * 10, posts.Count - page * 10);
            }
            var model = new ViewModels.PostIndex
            {
                Posts = posts,
                UserId = "0",
                GroupId = 0,
                Page = page
            };
            return View(model);
        }
    }
}