using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PostItCore.Models;

namespace PostItCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(int page = 0)
        {
            var context = new PostItDb(Opts());
            var posts = context.Posts.OrderBy(x => x.Date).ToList();
            if(posts != null && posts.Count > 10)
            {
                ViewData["Pages"] = posts.Count % 10 == 0 ? posts.Count / 10 : posts.Count / 10 + 1;
                if (posts.Count - page * 10 >= 9)
                    posts = posts.GetRange(page * 10, 10);
                else
                    posts = posts.GetRange(page * 10, posts.Count - page * 10 + 1);

            }
            return View(posts);
        }

        public IActionResult About()
        {
            

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public DbContextOptions<PostItDb> Opts()
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
