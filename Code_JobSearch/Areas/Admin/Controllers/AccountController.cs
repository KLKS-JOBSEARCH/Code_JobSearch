using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {

        DatabaseDataContext db = new DatabaseDataContext();
        private int GetPendingJobCount()
        {
            return db.TinTuyenDungs.Count(ttd => ttd.XetDuyet == "Đang xét duyệt");
        }
        // GET: Admin/Account
        public ActionResult staffAccount()
        {
            ViewBag.PendingJobCount = GetPendingJobCount();
            return View();
        }
    }
}