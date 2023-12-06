using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScavengeRUs.Models;
using ScavengeRUs.Models.Entities;
using ScavengeRUs.Services;
using System.Diagnostics;

namespace ScavengeRUs.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// This doesn't really matter to us
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger, IUserRepository userRepo, SignInManager<ApplicationUser> signInRepo)
        {
            _signInRepo = signInRepo;
            _userRepo = userRepo;
            _logger = logger; 
        }
        /// <summary>
        /// This is the landing page for www.localhost.com/Home/Index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();      //Right click and go to view to see the HTML or see it in the Views/Home folder in the solution explorer
        }
        public IActionResult LogIn()
        {
            
            return View();
        }
        [HttpPost, ActionName("LogIn")]
        public async Task<IActionResult> LogInConfirmed(AccessCode accessCode)
        {
            if (accessCode.Code == null)
            {
                return View("Error", new ErrorViewModel() { Text = "Enter a valid access code." }); 
                    
            }
            var user = await _userRepo.FindByAccessCode(accessCode.Code!);
            if (user == null)
            {
                return View("Error", new ErrorViewModel() { Text = "Enter a valid access code." });
            }
            await _signInRepo.SignInAsync(user, false);
            return RedirectToAction("ViewTasks", "Hunt", new {id = user.Hunt.Id}); // change to redirect to view of hunts
        }
        /// <summary>
        /// This is the landing page for www.localhost.com/Home/Privacy
        /// Only people that are "Admin" can view this 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="Admin")]
        public IActionResult Privacy()
        {
            return View();
        }


        /// <summary>
        /// This is the page displayed if there were a error
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
// This comment does nothing