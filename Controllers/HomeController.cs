using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cinemapandas4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cinemapandas4.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context { get; set; }
        private PasswordHasher<User> regHasher = new PasswordHasher<User> ();
        private PasswordHasher<LoginUser> logHasher = new PasswordHasher<LoginUser> ();

        public  User GetUser()
        {
            return _context.Users.FirstOrDefault( u =>  u.UserId == HttpContext.Session.GetInt32("userId"));
        }

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
                    ModelState.AddModelError ("LoginEmail", "Invalid Email or Password!");
                    return View ("Index");
                }
                var result = logHasher.VerifyHashedPassword (lu, userInDB.Password, lu.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError ("LoginPassword", "Invalid Email or Password!");
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
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            ViewBag.User = current;
            List<Movie> AllMovies = _context.Movies
                                            .Include(m => m.Organizer)
                                            .Include(m => m.Guests)
                                            .ThenInclude(wp => wp.MovieGoer)
                                            .Where( m => m.ScreeningTime >= DateTime.Now )
                                            .OrderBy( m => m.ScreeningTime )
                                            .ToList();
            return View (AllMovies);
        }

        [HttpGet("movie/new")]
        public IActionResult NewMovie()
        {
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            return View();
        }

        [HttpPost("movie/create")]
        public IActionResult CreateMovie(Movie newMovie)
        {
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            if(ModelState.IsValid)
            {
                newMovie.UserId = current.UserId;
                _context.Movies.Add(newMovie);
                _context.SaveChanges();
                return RedirectToAction("Home");
            }
            return View("NewMovie");
        }
        [HttpGet("movie/{movieId}/delete")]
        public IActionResult DeleteMovie(int movieId)
        {
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            Movie remove = _context.Movies.FirstOrDefault( m => m.MovieId == movieId );
            _context.Movies.Remove(remove);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }
        [HttpGet("movie/{movieId}/{status}")]
        public IActionResult ToggleParty(int movieId, string status)
        {
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            if(status == "join")
            {
                WatchParty newParty = new WatchParty();
                newParty.UserId = current.UserId;
                newParty.MovieId = movieId;
                _context.Parties.Add(newParty);
            }
            else if(status == "leave")
            {
                WatchParty backout = _context.Parties.FirstOrDefault( w => w.UserId == current.UserId && w.MovieId == movieId );
                _context.Parties.Remove(backout);
            }
            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        [HttpGet("movie/{movieId}")]
        public IActionResult DisplayMovie(int movieId)
        {
            User current = GetUser();
            if (current == null)
            {
                return Redirect ("/");
            }
            Movie showing = _context.Movies
                                    .Include( m => m.Guests )
                                    .ThenInclude( w => w.MovieGoer )
                                    .Include( m => m.Organizer )
                                    .FirstOrDefault( m => m.MovieId == movieId );
            return View(showing);

        }

        [HttpGet ("logout")]
        public IActionResult Logout ()
        {
            HttpContext.Session.Clear ();
            return Redirect ("/");
        }

        

    }
}