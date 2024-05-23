using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        // GET: Admin/Admin
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            NhanVien nv = Session["NV"] as NhanVien;

            int soLuongUngVien = db.UngViens.Count();
            int soLuongNhaTuyenDung = db.NhaTuyenDungs.Count();

            IQueryable<TinTuyenDung> tinTuyenDungsQuery = db.TinTuyenDungs;

            // Lấy ngày hiện tại
            DateTime today = DateTime.Now;

            // Chỉ lấy các tin tuyển dụng còn hạn
            tinTuyenDungsQuery = tinTuyenDungsQuery.Where(t => t.HanTuyenDung >= today);

            if (fromDate != null && toDate != null)
            {
                tinTuyenDungsQuery = tinTuyenDungsQuery.Where(t => t.ThoiGianDangTuyen >= fromDate && t.ThoiGianDangTuyen <= toDate);
            }

            int soLuongBaiViet = tinTuyenDungsQuery.Count();

            // Lấy số lượng tin tuyển dụng đã hết hạn
            int soLuongTinHetHan = db.TinTuyenDungs.Count(t => t.HanTuyenDung < today);

            ViewBag.SoLuongUngVien = soLuongUngVien;
            ViewBag.SoLuongNhaTuyenDung = soLuongNhaTuyenDung;
            ViewBag.SoLuongBaiViet = soLuongBaiViet;
            ViewBag.SoLuongTinHetHan = soLuongTinHetHan;

            return View();
        }







        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }


    }
}