using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult CvUser()
        {
            return View();
        }

        public ActionResult GopY()
        {
            return View();
        }
    }
}