using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Areas.Admin.Controllers
{
    public class UngvienController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        private int GetPendingJobCount()
        {
            return db.TinTuyenDungs.Count(ttd => ttd.XetDuyet == "Đang xét duyệt");
        }
        // GET: Admin/Ungvien
        public ActionResult UngvienDetails(int id)
        {
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ad = Session["AD"] as TaiKhoan;

            // Lấy thông tin ứng viên từ cơ sở dữ liệu dựa trên Id_UV truyền vào
            var kh = db.UngViens.FirstOrDefault(uv => uv.Id_UV == id);
            if (kh == null)
            {
                return RedirectToAction("ErrorPage"); // Hoặc xử lý lỗi theo cách khác
            }

            var history = from uv_ttd in db.UV_TTDs
                          join ttd in db.TinTuyenDungs on uv_ttd.Id_TTD equals ttd.Id_TTD
                          where uv_ttd.Id_UV == kh.Id_UV
                          select new HistoryOfCVApplyViewModel
                          {
                              TinTuyenDung = ttd,
                              UV_TTD = db.UV_TTDs.Where(u => u.Id_TTD == uv_ttd.Id_TTD).ToList()
                          };

            return View(history.ToList());
        }
    }
}