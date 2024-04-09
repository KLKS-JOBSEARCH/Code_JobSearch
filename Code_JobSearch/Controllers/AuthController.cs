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
        public ActionResult DangKy()
        {
            return View();
        }
        public ActionResult Dangnhap()
        {
            return View();
        }
    }
}