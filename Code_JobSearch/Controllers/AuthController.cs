using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Employer_Register()
        {
            return View();
        }
    }
}