using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;

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


            ViewBag.CurrentPage = "Index";
            return View();
        }

        public ActionResult UserProfilePortal(int page = 1)
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            NhanVien nv = Session["NV"] as NhanVien;
            List<UngVien> ungvien = db.UngViens.ToList();

            //paging
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(ungvien.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            ungvien = ungvien.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            ViewBag.CurrentPage = "UserProfilePortal";
            return View(ungvien);
        }

        public ActionResult EmployerProfilePortal(int page = 1)
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            NhanVien nv = Session["NV"] as NhanVien;
            List<NhaTuyenDung> NTD = db.NhaTuyenDungs.ToList();

            //paging
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(NTD.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            NTD = NTD.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            ViewBag.CurrentPage = "UserProfilePortal";
            return View(NTD);
        }


        public ActionResult CompanyPortal(int page = 1)
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            NhanVien nv = Session["NV"] as NhanVien;
            List<DoanhNghiep> dn = db.DoanhNghieps.ToList();

            //paging
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dn.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            dn = dn.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            ViewBag.CurrentPage = "UserProfilePortal";
            return View(dn);
        }

        public ActionResult DetailsCompany(int id)
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            NhanVien nv = Session["NV"] as NhanVien;


            var viewModel = new CompanyDetailsViewModel();

            // Lấy thông tin của doanh nghiệp
            viewModel.NhaTuyenDung = db.NhaTuyenDungs.SingleOrDefault(ntd => ntd.Id_NTD == id);
            if (viewModel.NhaTuyenDung != null)
            {
                // Lấy thông tin của doanh nghiệp từ Id_DN
                viewModel.DoanhNghiep = db.DoanhNghieps.SingleOrDefault(dn => dn.Id_DN == viewModel.NhaTuyenDung.Id_DN);

                // Lấy danh sách tin tuyển dụng từ Id_NTD và sắp xếp theo ngày hạn tuyển dụng mới nhất đến cũ nhất
                viewModel.TinTuyenDung = db.TinTuyenDungs
                                            .Where(ttd => ttd.Id_NTD == id && ttd.HanTuyenDung.HasValue && ttd.HanTuyenDung > DateTime.Now)
                                            .OrderByDescending(ttd => ttd.HanTuyenDung)
                                            .ToList();
            }

            return View(viewModel);
        }

        public ActionResult JobPortal(int page = 1, string filter = "newest")
        {
            if (Session["NV"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            var tinTuyenDungs = db.TinTuyenDungs.AsQueryable();

            // Sắp xếp theo thời gian đăng tuyển
            if (filter == "newest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderByDescending(t => t.ThoiGianDangTuyen);
            }
            else if (filter == "oldest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderBy(t => t.ThoiGianDangTuyen);
            }

            // Không cần chuyển đổi thành List ở đây
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tinTuyenDungs.Count()) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            // tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            // Chỉ phân trang, không cần chuyển đổi thành List
            tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage);

            return View(tinTuyenDungs);
        }




        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }


    }
}