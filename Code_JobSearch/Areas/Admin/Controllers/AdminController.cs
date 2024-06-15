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
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;

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
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;

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
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;
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
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;
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
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;

            var viewModel = new CompanyDetailsViewModel();

            // Lấy thông tin của doanh nghiệp
            viewModel.NhaTuyenDung = db.NhaTuyenDungs.SingleOrDefault(ntd => ntd.Id_NTD == id);
            if (viewModel.NhaTuyenDung != null)
            {
                // Lấy thông tin của doanh nghiệp từ Id_DN
                viewModel.DoanhNghiep = db.DoanhNghieps.SingleOrDefault(dn => dn.Id_DN == viewModel.NhaTuyenDung.Id_DN);

                // Lấy danh sách tin tuyển dụng từ Id_NTD và sắp xếp theo ngày hạn tuyển dụng mới nhất đến cũ nhất
                viewModel.TinTuyenDung = db.TinTuyenDungs
                                            .Where(ttd => ttd.Id_NTD == id
                                                        && ttd.HanTuyenDung.HasValue
                                                        && ttd.HanTuyenDung > DateTime.Now
                                                        && ttd.XetDuyet == "Duyệt thành công")
                                            .OrderByDescending(ttd => ttd.HanTuyenDung)
                                            .ToList();
            }

            return View(viewModel);
        }



        #region JOBS PORTAL
        public ActionResult JobPortal(int page = 1, string filter = "newest")
        {
            ViewBag.PendingJobCount = GetPendingJobCount();

            // Kiểm tra xem phiên làm việc có tồn tại hay không
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;

            // Lấy danh sách tin tuyển dụng từ cơ sở dữ liệu và lọc theo trạng thái và hạn tuyển dụng
            var tinTuyenDungs = db.TinTuyenDungs
                .Where(t => t.XetDuyet == "Duyệt thành công" && t.HanTuyenDung >= DateTime.Now)
                .AsQueryable();

            // Sắp xếp theo thời gian đăng tuyển
            if (filter == "newest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderByDescending(t => t.ThoiGianDangTuyen);
            }
            else if (filter == "oldest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderBy(t => t.ThoiGianDangTuyen);
            }

            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tinTuyenDungs.Count()) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            // tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage);

            return View(tinTuyenDungs);
        }
        private int GetPendingJobCount()
        {
            return db.TinTuyenDungs.Count(ttd => ttd.XetDuyet == "Đang xét duyệt");
        }
        public ActionResult JobPortalWait(int page = 1, string filter = "newest")
        {
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;
            var tinTuyenDungs = db.TinTuyenDungs.AsQueryable();

            // Lọc các việc làm có trạng thái đang chờ xét duyệt
            tinTuyenDungs = tinTuyenDungs.Where(t => t.XetDuyet == "Đang xét duyệt");

            // Sắp xếp theo thời gian đăng tuyển
            if (filter == "newest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderByDescending(t => t.ThoiGianDangTuyen);
            }
            else if (filter == "oldest")
            {
                tinTuyenDungs = tinTuyenDungs.OrderBy(t => t.ThoiGianDangTuyen);
            }

            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tinTuyenDungs.Count()) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;

            var tinTuyenDungPaged = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            return View(tinTuyenDungPaged);
        }

        public ActionResult DetailJobPortal(int id)
        {
            ViewBag.PendingJobCount = GetPendingJobCount();
            if (Session["AD"] == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            TaiKhoan ac = Session["AD"] as TaiKhoan;
            var viewModel = (from ttd in db.TinTuyenDungs
                             join ntd in db.NhaTuyenDungs on ttd.Id_NTD equals ntd.Id_NTD
                             join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                             where ttd.Id_TTD == id
                             select new JobDetailsViewModel
                             {
                                 TinTuyenDung = ttd,
                                 DoanhNghiep = dn,
                                 NhaTuyenDung = ntd
                             }).SingleOrDefault();

            if (viewModel == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Thông tin tuyển dụng";
            ViewBag.ProductName = viewModel.TinTuyenDung.TieuDe_TTD;

            return View(viewModel);
        }

        public ActionResult ConfirmJob(int id)
        {
            TinTuyenDung ttd = db.TinTuyenDungs.Single(o => o.Id_TTD == id);

            ttd.XetDuyet = "Duyệt thành công";
            db.SubmitChanges();
            return RedirectToAction("JobPortalWait");
        }

        public ActionResult RejectJob(int id)
        {
            TinTuyenDung ttd = db.TinTuyenDungs.Single(o => o.Id_TTD == id);

            ttd.XetDuyet = "Duyệt không thành công";
            db.SubmitChanges();
            return RedirectToAction("JobPortalWait");
        }
        #endregion

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }


    }
}