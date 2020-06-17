using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cinemapandas4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cinemapandas4.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context { get; set; }
        private PasswordHasher<User> regHasher = new PasswordHasher<User> ();
        private PasswordHasher<LoginUser> logHasher = new PasswordHasher<LoginUser> ();

        public HomeController (MyContext context)
        {
            _context = context;
        }

        [HttpGet ("")]
        public IActionResult Index ()
        {
            return View ();
        }

        [HttpPost ("register")]
        public IActionResult Register (User u)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.FirstOrDefault (usr => usr.Email == u.Email) != null)
                {
                    ModelState.AddModelError ("Email", "Email is already in use, try logging in!");
                    return View ("Index");
                }
                string hash = regHasher.HashPassword (u, u.Password);
                u.Password = hash;
                _context.Users.Add (u);
                _context.SaveChanges ();
                HttpContext.Session.SetInt32 ("userId", u.UserId);
                return Redirect ("/home");
            }
            return View ("Index");
        }

        [HttpPost ("login")]
        public IActionResult Login (LoginUser lu)
        {
            if (ModelState.IsValid)
            {
                User userInDB = _context.Users.FirstOrDefault (u => u.Email == lu.LoginEmail);
                if (userInDB == null)
                {
                    ModelState.AddModelError ("Email", "Invalid Email or Password!");
                    return View ("Index");
                }
                var result = logHasher.VerifyHashedPassword (lu, userInDB.Password, lu.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError ("Password", "Invalid Email or Password!");
                    return View ("Index");
                }
                HttpContext.Session.SetInt32 ("userId", userInDB.UserId);
                return Redirect ("/home");
            }
            return View ("Index");
        }

        [HttpGet ("home")]
        public IActionResult Home ()
        {
            int? userId = HttpContext.Session.GetInt32 ("userId");
            if (userId == null)
            {
                return Redirect ("/");
            }
            return View ();
        }

        [HttpGet ("logout")]
        public IActionResult Logout ()
        {
            HttpContext.Session.Clear ();
            return Redirect ("/");
        }

    }
}