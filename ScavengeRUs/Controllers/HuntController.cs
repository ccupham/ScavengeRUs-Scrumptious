using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScavengeRUs.Models.Entities;
using ScavengeRUs.Services;
using Microsoft.AspNetCore.Identity;

namespace ScavengeRUs.Controllers
{
    /// <summary>
    /// This class is the controller for any page related to hunts
    /// </summary>
    public class HuntController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IHuntRepository _huntRepo;

        /// <summary>
        /// Injecting the user repository and hunt repository (Db classes)
        /// </summary>
        /// <param name="userRepo"></param>
        /// <param name="HuntRepo"></param>
        public HuntController(IUserRepository userRepo, IHuntRepository HuntRepo)
        {
            _userRepo = userRepo;
            _huntRepo = HuntRepo;
        }
        /// <summary>
        /// www.localhost.com/hunt/index Returns a list of all hunts
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var hunts = await _huntRepo.ReadAllAsync();
            return View(hunts);
        }
        /// <summary>
        /// www.localhost.com/hunt/create This is the get method for creating a hunt
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// www.localhost.com/hunt/create This is the post method for creating a hunt
        /// </summary>
        /// <param name="hunt"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Hunt hunt)
        {
            if (ModelState.IsValid)
            {
                // This line will prevent the admin from creating a hunt that ends before it starts.
                // However, it will not display an error message or warn the user 
                if(hunt.StartDate < hunt.EndDate)
                {
                    hunt.CreationDate = DateTime.Now;
                    await _huntRepo.CreateAsync(hunt);
                    return RedirectToAction("Index");
                }

            }
            return View(hunt);
           
        }
        /// <summary>
        /// www.localhost.com/hunt/details/{huntId} This is the details view of a hunt
        /// </summary>
        /// <param name="huntId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details([Bind(Prefix ="Id")]int huntId)
        {
            if (huntId == 0)
            {
                return RedirectToAction("Index");
            }
            var hunt = await _huntRepo.ReadAsync(huntId);
            if (hunt == null)
            {
                return RedirectToAction("Index");
            }
            return View(hunt);
        }
        /// <summary>
        /// www.localhost.com/hunt/delete/{huntId} This is the get method for deleting a hunt
        /// </summary>
        /// <param name="huntId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([Bind(Prefix = "Id")]int huntId)
        {
            if (huntId == 0)
            {
                return RedirectToAction("Index");
            }
            var hunt = await _huntRepo.ReadAsync(huntId);
            if (hunt == null)
            {
                return RedirectToAction("Index");
            }
            return View(hunt);
        }
        /// <summary>
        /// www.localhost.com/hunt/delete/{huntId} This is the post method for deleteing a hunt.
        /// </summary>
        /// <param name="huntId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([Bind(Prefix = "id")] int huntId)
        {
            await _huntRepo.DeleteAsync(huntId);
            return RedirectToAction("Index");
        }
        /// <summary>
        /// www.localhost.com/hunt/viewplayers/{huntId} Returns a list of all players in a specified hunt
        /// </summary>
        /// <param name="huntId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewPlayers([Bind(Prefix = "Id")] int huntId)
        {
            var hunt = await _huntRepo.ReadHuntWithRelatedData(huntId);
            ViewData["Hunt"] = hunt;
            if(hunt == null)
            {
                return RedirectToAction("Index");
            }
            
            return View(hunt.Players);
        }
        /// <summary>
        /// www.localhost.com/hunt/addplayertohunt{huntid} Get method for adding a player to a hunt. 
        /// </summary>
        /// <param name="huntId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPlayerToHunt([Bind(Prefix ="Id")]int huntId)
        {
            var hunt = await _huntRepo.ReadAsync(huntId);
            ViewData["Hunt"] = hunt;
            return View();
            
        }
        /// <summary>
        /// www.localhost.com/hunt/addplayertohunt{huntid} Post method for the form submission. This creates a user and assigns the access code for the hunt. 
        /// </summary>
        /// <param name="huntId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddPlayerToHunt([Bind(Prefix = "Id")] int huntId ,ApplicationUser user)
        {

            if (huntId == 0)
            {
                RedirectToAction("Index");
            }
            var hunt = await _huntRepo.ReadAsync(huntId);
            var existingUser = await _userRepo.ReadAsync(user.Email);
            var newUser = new ApplicationUser();
            if (existingUser == null)
            {
                newUser.Email = user.Email;
                newUser.PhoneNumber = user.PhoneNumber;
                newUser.FirstName = user.FirstName;
                newUser.LastName = user.LastName;
                newUser.AccessCode = user.AccessCode;
                newUser.UserName = user.Email;
            }
            else
            {
                newUser = existingUser;
                newUser.AccessCode = user.AccessCode;
            }
            if (newUser.AccessCode!.Code == null)       //If the admin didn't specify an access code (If we need to, I have the field readonly currently)
            {
                newUser.AccessCode = new AccessCode()
                {
                    Hunt = hunt,                        //Setting foriegn key
                    Code = $"{newUser.PhoneNumber}/{hunt.HuntName!.Replace(" ", string.Empty)}",            //This is the access code generation
                };
                newUser.AccessCode.Users.Add(newUser);  //Setting foriegn key
            }
            else
            {
                newUser.AccessCode = new AccessCode()
                {
                    Hunt = hunt,
                    Code = newUser.AccessCode.Code,
                };
                newUser.AccessCode.Users.Add(newUser);
            }
            await _huntRepo.AddUserToHunt(huntId, newUser); //This methods adds the user to the database and adds the database relationship to a hunt.
            return RedirectToAction("Index");
        }
        /// <summary>
        /// www.localhost.com/hunt/removeuser/{username}/{huntId} This is the get method for removing a user from a hunt.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUser([Bind(Prefix ="Id")]string username, [Bind(Prefix ="huntId")]int huntid)
        {
            ViewData["Hunt"] = huntid;
            var user = await _userRepo.ReadAsync(username);
            return View(user);

        }
        /// <summary>
        /// www.localhost.com/hunt/removeuser/{username}/{huntId} This is the post method for removing a user from a hunt.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserConfirmed(string username, int huntid)
        {
            await _huntRepo.RemoveUserFromHunt(username, huntid);
            return RedirectToAction("Index");

        }
        /// <summary>
        /// This method generates a view of all task associated with a hunt. Pasing the huntid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Player, Admin")]
        public async Task<IActionResult> ViewTasks([Bind(Prefix ="Id")]int huntid)
        {
            var currentUser = await _userRepo.ReadAsync(User.Identity?.Name!);
            var hunt = await _huntRepo.ReadHuntWithRelatedData(huntid);
            ViewData["Hunt"] = hunt;
            if (hunt == null)
            {
                return RedirectToAction("Index");
            }
            
            var tasks = await _huntRepo.GetLocations(hunt.HuntLocations);
                foreach (var item in tasks)
                {
                    if (currentUser.TasksCompleted.Count() > 0)
                    {
                        var usertask = currentUser.TasksCompleted.FirstOrDefault(a => a.Id == item.Id);
                        if (usertask != null && tasks.Contains(usertask))
                        {
                            item.Completed = "Completed";
                        }
                    }
                    else
                    {
                        item.Completed = "Not completed";
                    }
                }
            return View(tasks);
            
        }
        /// <summary>
        /// This method shows all tasks that can be added to the hunt. Exculding the tasks that are already added
        /// </summary>
        /// <param name="huntid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageTasks([Bind(Prefix ="Id")]int huntid)
        {
            var hunt = await _huntRepo.ReadHuntWithRelatedData(huntid);
            //var existingLocations = await _huntRepo.GetLocations(hunt.HuntLocations);

            ViewData["Hunt"] = hunt;
            var allLocations = await _huntRepo.GetAllLocations();
            //var locations = allLocations.Except(existingLocations);
            return View(allLocations);
        }
        /// <summary>
        /// This method is the post method for adding a task. This gets executed when you click "Add Task"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTask(int id, int huntid)
        {
            var hunt = await _huntRepo.ReadHuntWithRelatedData(huntid);
            ViewData["Hunt"] = hunt;
            await _huntRepo.AddLocation(id, huntid);
            return RedirectToAction("ManageTasks", new {id=huntid});
        }
        /// <summary>
        /// This is the get method for removing a task from a hunt. This is executed when clicking "Remove" from the Hunt/ViewTasks screen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        //public async Task<IActionResult> RemoveTasks(int id, int huntid)
        //{
        //    var hunt = await _huntRepo.ReadAsync(huntid);
        //    ViewData["Hunt"] = hunt;
        //    var task = await _huntRepo.ReadLocation(id);
        //    return View(task);
        //}
        /// <summary>
        /// This is the post method for removing a task. This is executed when you click "Remove" from the Hunt/RemoveTask screen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveTask(int id, int huntid)
        {
            await _huntRepo.RemoveTaskFromHunt(id, huntid);
            return RedirectToAction("ManageTasks", "Hunt", new {id=huntid});
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>  Update(int id)
        {
            var hunt = await _huntRepo.ReadAsync(id);

            return View(hunt);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Update(int id, Hunt hunt)
        {
            _huntRepo.Update(id, hunt);
            return RedirectToAction("Index");
        }
        /// <summary>
        /// This method associates ViewMap with this controller and 
        /// allows the view to access the current state (selected hunt)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huntid"></param>
        /// <returns></returns>
        public async Task<IActionResult> ViewMap([Bind(Prefix = "Id")] int huntid)
        {
            var currentUser = await _userRepo.ReadAsync(User.Identity?.Name!);
            var hunt = await _huntRepo.ReadHuntWithRelatedData(huntid);
            ViewData["Hunt"] = hunt;

            if (hunt == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
