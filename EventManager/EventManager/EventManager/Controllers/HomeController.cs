using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EventManager.Models;
using System.Collections.Generic;

namespace EventManager.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;

        public HomeController()
        {
        }

        public HomeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        ApplicationDbContext dbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            IEnumerable<Event> events = dbContext.Events;
            return View(events);
        }

        [Authorize]
        public ActionResult MyEvents()
        {
            IEnumerable<Event> events = dbContext.Events.SqlQuery("SELECT * FROM Events JOIN Additionals ON Events.Id=Additionals.EventId AND Additionals.UserId='" + User.Identity.GetUserId() + "'");

            ViewBag.Books = events;

            return View(events);
        }

        [Authorize]
        public ActionResult Unregister(int id)
        {
            List<Additional> additional = dbContext.Additional.SqlQuery("SELECT * FROM Additionals WHERE UserId='" + User.Identity.GetUserId() + "' AND EventId='" + id + "'").ToList();
            dbContext.Additional.Remove(additional[0]);
            dbContext.SaveChanges();
            return RedirectToAction("MyEvents");
        }

        [Authorize(Roles = "admin")]
        public ActionResult Remove(int id)
        {
            Event Event = dbContext.Events.Find(id);
            try
            {
                if (Event != null)
                {
                    dbContext.Events.Remove(Event);
                    dbContext.SaveChanges();
                }
            }
            catch
            {
                return View("ErrorRemove");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult AddEvent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddEvent(Event event1)
        {
            IEnumerable<Event> events = dbContext.Events.SqlQuery("SELECT * FROM Events WHERE Name=N'" + event1.Name + "'");
            if (events.Count() == 0)
            {
                if (ModelState.IsValid)
                {
                    dbContext.Events.Add(event1);
                    dbContext.SaveChanges();
                    ViewBag.Text = "Мероприятие \"" + event1.Name + "\" успешно добавлено!";
                }
            }
            else
            {
                ViewBag.Text = "Мероприятие \"" + event1.Name + "\" уже существует!";
            }
            return View();
        }

        [Authorize]
        public async Task<ActionResult> Register(int id)
        {
            var result = dbContext.Additional.SqlQuery("SELECT * FROM Additionals WHERE UserId='" + User.Identity.GetUserId() + "' AND EventId='" + id + "'");
            if (result.Count() == 0)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { userId = user.Id, code = code, eventid = id }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Подтверждение регистрации на мероприятие \"" + dbContext.Events.Find(id).Name + "\"", "Подтвердите вашу регистрацию на мероприятие \"" + dbContext.Events.Find(id).Name + "\", щелкнув <a href='" + callbackUrl + "'>здесь</a>");
                ViewBag.Message = "На ваш электронный адрес отправлены дальнейшие инструкции по завершению регистрации.";
            }
            else
            {
                ViewBag.Message = "Вы уже зарегистрированы на это мероприятие!";
            }
            return View("MessageRegister");
        }

        public async Task<ActionResult> ConfirmEmail(string userId, string code, int eventid)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            Additional additional = new Additional
            {
                UserId = userId,
                EventId = eventid
            };
            var res = dbContext.Additional.SqlQuery("SELECT * FROM Additionals WHERE UserId='" + userId + "' AND EventId='" + eventid + "'");
            if (result.Succeeded && res.Count() == 0)
            {
                dbContext.Additional.Add(additional);
                dbContext.SaveChanges();
                ViewBag.Event = await dbContext.Events.FindAsync(eventid);
                return View("Register");
            }
            return RedirectToAction("Index");
        }
    }
}