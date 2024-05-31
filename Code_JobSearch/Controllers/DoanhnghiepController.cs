﻿using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Code_JobSearch.Controllers
{
    public class DoanhNghiepController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        // GET: DoanhNghiep

        public ActionResult Index()
        {

            return View();

        }
        public ActionResult FilterPosts(int month, int year)
        {
            // Truy vấn cơ sở dữ liệu để lấy số lượng tin đã đăng và số lượng ứng viên đã ứng tuyển
            if (month == 0)
            {
                month = DateTime.Now.Month;
                year = DateTime.Now.Year;
            }
            var postCounts = db.TinTuyenDungs
                                .Where(o => o.ThoiGianDangTuyen.Value.Month == month &&
                                            o.ThoiGianDangTuyen.Value.Year == year &&
                                            o.XetDuyet == "Duyệt thành công")
                                .GroupBy(o => o.ThoiGianDangTuyen.Value.Day)
                                .Select(g => new { day = g.Key, postCount = g.Count() })
                                .OrderBy(x => x.day)
                                .ToList();

            var candidateCounts = db.UV_TTDs
                                    .Where(o => o.ThoiGianUngTuyen.Value.Month == month &&
                                                o.ThoiGianUngTuyen.Value.Year == year &&
                                                o.TinhTrangUngTuyen == "Đã ứng tuyển")
                                    .GroupBy(o => o.ThoiGianUngTuyen.Value.Day)
                                    .Select(g => new { day = g.Key, candidateCount = g.Count() })
                                    .OrderBy(x => x.day)
                                    .ToList();

            // Trả về dữ liệu dưới dạng JSON hoặc PartialView để cập nhật biểu đồ cột
            return Json(new { posts = postCounts, candidates = candidateCounts }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult ListTTD(int? id, int page = 1, int pageSize = 5)
        {
            if (id.HasValue)
            {
                ViewBag.idntd = id.Value;

                var listttd = db.TinTuyenDungs
                                .Where(o => o.Id_NTD == id)
                                .OrderByDescending(o => o.ThoiGianDangTuyen)
                                .ToList();

                int totalItems = listttd.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var paginatedPosts = listttd.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.TotalItems = totalItems;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages; // Add this line to pass the total number of pages to the view
                ViewBag.isDel = TempData["isDel"];
                ViewBag.isEdit = TempData["isEdit"];

                return View(paginatedPosts);
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }



        #region Góp ý
        [HttpGet]
        public ActionResult GopY()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GopY(GopY gy, FormCollection f)
        {
            var tieude = f["TieuDeGY"];
            var noidung = f["NoiDungGY"];
            var mucdohailong = f["MucDoHaiLong"];

            ViewBag.HotenKH = tieude;
            ViewBag.TenDN = noidung;

            if (string.IsNullOrEmpty(tieude) ||
              string.IsNullOrEmpty(noidung))
            {
                ViewData["Loi"] = "Vui lòng điền đầy đủ thông tin!";
                return View();
            }
            // Kiểm tra radio button đã được chọn và có giá trị hợp lệ hay không
            if (string.IsNullOrEmpty(mucdohailong) || !int.TryParse(mucdohailong, out int mucDo))
            {
                ViewData["Loi2"] = "Vui lòng chọn mức độ hài lòng!";
                return View();
            }

            // Kiểm tra giá trị của radio button nằm trong khoảng từ 1 đến 5
            if (mucDo < 1 || mucDo > 5)
            {
                ViewData["Loi2"] = "Mức độ hài lòng phải từ 1 đến 5!";
                return View();
            }
            // Lấy thông tin ứng viên từ session
            NhaTuyenDung ntd = Session["NTD"] as NhaTuyenDung;
            if (ntd != null)
            {
                gy.Id_NTD = ntd.Id_NTD;

                gy.TieuDe_GY = tieude;
                gy.NoiDung_GY = noidung;
                gy.MucDoHaiLong = Convert.ToInt16(mucdohailong); // Chuyển đổi giá trị string thành smallint
                gy.NgayGui_GY = DateTime.Now;

                db.Gopies.InsertOnSubmit(gy);
                db.SubmitChanges();

                ViewBag.TB = "thành công!";
                return RedirectToAction("ListTTD", "DoanhNghiep");
            }
            else
            {

                ViewBag.TB = "Đăng nhập trước khi gửi góp ý!";
                return RedirectToAction("Login_DoanhNghiep", "Auth");

            }
        }
        #endregion
        #region Doanh nghiệp info
        public ActionResult DoanhNghiepInfo(string id)
        {


            if (!id.IsEmpty())
            {
                var iddn = Convert.ToInt32(id);
                var dn = db.DoanhNghieps.Where(o => o.Id_DN == iddn).FirstOrDefault();


                return View(dn);
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }
        private List<string> danhSachThanhPho = new List<string>
        {
            "Hà Nội", "Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Hải Dương", "Hưng Yên", "Bắc Ninh", "Hà Nam",
            "Nam Định", "Ninh Bình", "Thái Bình", "Vĩnh Phúc", "Phú Thọ", "Bắc Giang", "Bắc Kạn", "Cao Bằng", "Hà Giang",
            "Lạng Sơn", "Lào Cai", "Tuyên Quang", "Yên Bái", "Thái Nguyên", "Lai Châu", "Sơn La", "Điện Biên", "Hòa Bình",
            "Quảng Ninh", "Bắc Ninh", "Hà Nam", "Ninh Bình", "Thanh Hóa", "Nghệ An", "Hà Tĩnh", "Quảng Bình", "Quảng Trị",
            "Thừa Thiên Huế", "Quảng Nam", "Quảng Ngãi", "Bình Định", "Phú Yên", "Khánh Hòa", "Ninh Thuận", "Bình Thuận",
            "Kon Tum", "Gia Lai", "Đắk Lắk", "Đắk Nông", "Lâm Đồng", "Bình Phước", "Bình Dương", "Đồng Nai", "Tây Ninh",
            "Bà Rịa - Vũng Tàu", "Long An", "Tiền Giang", "Bến Tre", "Trà Vinh", "Vĩnh Long", "Đồng Tháp", "An Giang",
            "Kiên Giang", "Cần Thơ", "Hậu Giang", "Sóc Trăng", "Bạc Liêu", "Cà Mau"
        };
        [HttpGet]
        public ActionResult EditDN(int id)
        {
            DoanhNghiep emp = db.DoanhNghieps.SingleOrDefault(x => x.Id_DN == id);
            ViewBag.DSTP = danhSachThanhPho;
            // Kiểm tra xem người dùng có tồn tại không
            if (emp == null)
            {
                return HttpNotFound();
            }

            return View(emp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDN(int id, DoanhNghiep emp, HttpPostedFileBase uploadFile)
        {
            ViewBag.DSTP = danhSachThanhPho;
            if (ModelState.IsValid)
            {
                try
                {
                    using (DatabaseDataContext db = new DatabaseDataContext())
                    {
                        // Lấy thông tin người dùng cần chỉnh sửa từ cơ sở dữ liệu
                        DoanhNghiep dns = db.DoanhNghieps.Single(x => x.Id_DN == id);

                        // Cập nhật thông tin từ form chỉnh sửa
                        dns.Ten_DN = emp.Ten_DN;
                        dns.MaSoThue_DN = emp.MaSoThue_DN;
                        dns.LinkWeb_DN = emp.LinkWeb_DN;
                        dns.DiaDiem_DN = emp.DiaDiem_DN;
                        dns.MoTa_DN = emp.MoTa_DN;

                        // Xử lý upload hình ảnh nếu có
                        if (uploadFile != null && uploadFile.ContentLength > 0)
                        {
                            // Kiểm tra kích thước của file (giới hạn dưới 1MB)
                            if (uploadFile.ContentLength <= 1024 * 1024) // 1MB = 1024 * 1024 bytes
                            {
                                var fileName = Path.GetFileName(uploadFile.FileName);
                                var filePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), fileName);

                                // Xóa hình ảnh cũ nếu tồn tại
                                if (dns.Logo_DN != null)
                                {
                                    var oldFilePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), dns.Logo_DN);
                                    if (System.IO.File.Exists(oldFilePath))
                                    {
                                        System.IO.File.Delete(oldFilePath);
                                    }
                                }

                                // Lưu hình ảnh mới vào thư mục
                                uploadFile.SaveAs(filePath);

                                // Cập nhật đường dẫn của ảnh trong cơ sở dữ liệu
                                dns.Logo_DN = fileName;
                            }
                            else
                            {
                                // Nếu kích thước vượt quá 1MB, xử lý lỗi
                                ModelState.AddModelError("", "Kích thước tệp ảnh phải nhỏ hơn hoặc bằng 1MB.");
                                return View(emp);
                            }
                        }
                        if (string.IsNullOrEmpty(emp.Ten_DN))
                        {
                            ViewData["Loi1"] = "Tên doanh nghiệp không bỏ trống";
                            return View(emp);
                        }
                        if (string.IsNullOrEmpty(emp.MaSoThue_DN))
                        {
                            ViewData["Loi2"] = "Mã số thuế không bỏ trống";
                            return View(emp);
                        }
                        if (emp.MaSoThue_DN.All(char.IsDigit) || emp.MaSoThue_DN.Length > 10 || emp.MaSoThue_DN.Length < 10)
                        {
                            ViewData["Loi2"] = "Mã số thuế là 10 chữ số";
                            return View(emp);
                        }
                        //if (string.IsNullOrEmpty(emp.LinkWeb_DN))
                        //{
                        //    ViewData["Loi3"] = "Website không bỏ trống";
                        //    return View(emp);
                        //}
                        //if (string.IsNullOrEmpty(emp.MoTa_DN))
                        //{
                        //    ViewData["Loi4"] = "Mô tả không bỏ trống";
                        //    return View(emp);
                        //}

                        db.SubmitChanges();


                        return RedirectToAction("DoanhNghiepInfo", new { id = dns.Id_DN.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    ViewBag.Message = "Có lỗi xảy ra: " + ex.Message;
                }
            }
            return View(emp);
        }
        #endregion
        //code update
        #region NhaTuyenDung profile
        public ActionResult ProfileNhaTuyenDung()
        {

            if (Session["NTD"] != null)
            {
                NhaTuyenDung ntd = Session["NTD"] as NhaTuyenDung;



                // Truy vấn để lấy thông tin tài khoản của người dùng từ khóa ngoại
                var taiKhoan = (from tk in db.TaiKhoans
                                where tk.TenTK == ntd.TenTK
                                select tk).FirstOrDefault();

                if (taiKhoan != null)
                {
                    // Hiển thị thông tin tài khoản
                    ViewBag.TenTaiKhoan = taiKhoan.TenTK;
                    ViewBag.MatKhau = taiKhoan.MatKhau;
                }
                else
                {
                    // Xử lý trường hợp không tìm thấy thông tin tài khoản
                    ViewBag.TenTaiKhoan = "Không có thông tin";
                    ViewBag.MatKhau = "Không có thông tin";
                }

                return View(ntd);
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }
        private List<string> danhSachViTriCongTac = new List<string>
        {
            "Nhân viên", "Phó phòng","Trưởng phòng", "Phó giám đốc", "Giám đốc", "Tổng giám đốc"
        };

        [HttpGet]
        public ActionResult EditNTD(int id)
        {
            NhaTuyenDung emp = db.NhaTuyenDungs.SingleOrDefault(x => x.Id_NTD == id);
            // Kiểm tra xem người dùng có tồn tại không
            if (emp == null)
            {
                return HttpNotFound();
            }

            return View(emp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNTD(int id, NhaTuyenDung emp, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (DatabaseDataContext db = new DatabaseDataContext())
                    {
                        // Lấy thông tin người dùng cần chỉnh sửa từ cơ sở dữ liệu
                        NhaTuyenDung ntds = db.NhaTuyenDungs.Single(x => x.Id_NTD == id);
                        if (string.IsNullOrEmpty(emp.HoTen_NTD))
                        {
                            ViewData["Loi_HoTenNTD"] = "Họ tên không bỏ trống";
                            return View(emp);
                        }
                        if (string.IsNullOrEmpty(emp.SoDienThoai_NTD))
                        {
                            ViewData["Loi_SDTNTD"] = "Số điện thoại không bỏ trống";
                            return View(emp);
                        }
                        if (!emp.SoDienThoai_NTD.All(char.IsDigit) || emp.SoDienThoai_NTD.Length < 10 || emp.SoDienThoai_NTD.Length > 10)
                        {
                            ViewData["Loi_SDTNTD"] = "Số điện thoại gồm 10 chữ số";
                            return View(emp);
                        }
                        // Cập nhật thông tin từ form chỉnh sửa
                        ntds.HoTen_NTD = emp.HoTen_NTD;
                        ntds.Email_NTD = emp.Email_NTD;
                        ntds.SoDienThoai_NTD = emp.SoDienThoai_NTD;
                        ntds.ViTriCongTac = emp.ViTriCongTac;
                        ntds.TenTK = emp.TenTK;

                        // Xử lý upload hình ảnh nếu có
                        if (uploadFile != null && uploadFile.ContentLength > 0)
                        {
                            // Kiểm tra kích thước của file (giới hạn dưới 1MB)
                            if (uploadFile.ContentLength <= 1024 * 1024) // 1MB = 1024 * 1024 bytes
                            {
                                var fileName = Path.GetFileName(uploadFile.FileName);
                                var filePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), fileName);

                                // Xóa hình ảnh cũ nếu tồn tại
                                if (ntds.HinhAnh_NTD != null)
                                {
                                    var oldFilePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), ntds.HinhAnh_NTD);
                                    if (System.IO.File.Exists(oldFilePath))
                                    {
                                        System.IO.File.Delete(oldFilePath);
                                    }
                                }

                                // Lưu hình ảnh mới vào thư mục
                                uploadFile.SaveAs(filePath);

                                // Cập nhật đường dẫn của ảnh trong cơ sở dữ liệu
                                ntds.HinhAnh_NTD = fileName;
                            }
                            else
                            {
                                // Nếu kích thước vượt quá 1MB, xử lý lỗi
                                ModelState.AddModelError("", "Kích thước tệp ảnh phải nhỏ hơn hoặc bằng 1MB.");
                                return View(emp);
                            }
                        }

                        // Lưu thay đổi vào cơ sở dữ liệu
                        db.SubmitChanges();

                        // Cập nhật thông tin người dùng trong session
                        Session["NTD"] = ntds;

                        return RedirectToAction("ProfileNhaTuyenDung");
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    ViewBag.Message = "Có lỗi xảy ra: " + ex.Message;
                }
            }
            return View(emp);
        }
        public ActionResult DetailsJob(int id)
        {
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
        #endregion
        //--------------------
        #region #region CREATE, EDIT, DELETE
        public ActionResult Create()
        {
            ViewBag.DSThanhPho = danhSachThanhPho;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, TinTuyenDung ttd)
        {
            ViewBag.DSThanhPho = danhSachThanhPho;
            if (ModelState.IsValid)
            {
                var logoDN = (from ntd in db.NhaTuyenDungs
                              join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                              where ntd.Id_NTD == id
                              select dn.Logo_DN).FirstOrDefault();
                ttd.Id_NTD = id;
                ttd.ThoiGianDangTuyen = DateTime.Now;
                ttd.TrangThaiDang = false;
                ttd.XetDuyet = "Đang xét duyệt";

                if (string.IsNullOrEmpty(ttd.TieuDe_TTD))
                {
                    ViewData["Loi1"] = "Tiêu đề không bỏ trống";
                    return View();
                }
                if (!ttd.HanTuyenDung.HasValue || ttd.HanTuyenDung.Value < DateTime.Now.Date.AddDays(10))
                {
                    ViewData["Loi2"] = "Hạn tuyển dụng cần ít nhất 10 ngày";
                    return View();
                }
                if (string.IsNullOrEmpty(ttd.MucLuongTD))
                {
                    ViewData["Loi3"] = "Mức lương không bỏ trống";
                    return View();
                }


                if (ttd.SoLuongTuyen.ToString() == "" || !ttd.SoLuongTuyen.ToString().All(char.IsDigit) || ttd.SoLuongTuyen < 1)
                {
                    ViewData["Loi4"] = "Số lượng tuyển tối thiểu 1 người (là số tự nhiên)";
                    return View();
                }

                if (string.IsNullOrEmpty(ttd.KinhNghiemLam))
                {
                    ViewData["Loi5"] = "Kinh nghiệm không bỏ trống";
                    return View();
                }
                if (string.IsNullOrEmpty(ttd.MoTa_TTD))
                {
                    ViewData["Loi6"] = "Mô tả không bỏ trống";
                    return View();
                }
                if (string.IsNullOrEmpty(logoDN))
                {
                    ViewBag.ErrorLogo = "Logo doanh nghiệp chưa có, vui lòng vào thông tin doanh nghiệp để cập nhật";
                    return View();
                }
                ttd.Logo_DN_TTD = logoDN;
                db.TinTuyenDungs.InsertOnSubmit(ttd);
                db.SubmitChanges();

                return RedirectToAction("ListTTD", new { id = id.ToString() });
            }

            return View(ttd);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {

            ViewBag.DSThanhPho = danhSachThanhPho;
            var tinTuyenDung = db.TinTuyenDungs.FirstOrDefault(t => t.Id_TTD == id);
            var idnts = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == id)?.Id_NTD;
            if (tinTuyenDung == null)
            {
                return HttpNotFound();
            }
            if (tinTuyenDung.XetDuyet == "Duyệt thành công")
            {
                TempData["isEdit"] = "Không thể sửa do công việc đã duyệt thành công";
                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }
            var logoDN = (from ttdung in db.TinTuyenDungs
                          join ntd in db.NhaTuyenDungs on ttdung.Id_NTD equals ntd.Id_NTD
                          join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                          where ttdung.Id_TTD == id
                          select dn.Logo_DN).FirstOrDefault();

            ViewBag.LogoDN = logoDN;

            return View(tinTuyenDung);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TinTuyenDung ttd)
        {
            ViewBag.DSThanhPho = danhSachThanhPho;

            if (ModelState.IsValid)
            {
                var logoDN = (from ttdung in db.TinTuyenDungs
                              join ntd in db.NhaTuyenDungs on ttdung.Id_NTD equals ntd.Id_NTD
                              join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                              where ttdung.Id_TTD == id
                              select dn.Logo_DN).FirstOrDefault();

                if (string.IsNullOrEmpty(ttd.TieuDe_TTD))
                {
                    ViewData["Loi1"] = "Tiêu đề không bỏ trống";
                    return View(ttd);
                }
                if (!ttd.HanTuyenDung.HasValue || ttd.HanTuyenDung.Value < DateTime.Now.Date.AddDays(10))
                {
                    ViewData["Loi2"] = "Hạn tuyển dụng cần ít nhất 10 ngày";
                    return View(ttd);
                }
                if (string.IsNullOrEmpty(ttd.MucLuongTD))
                {
                    ViewData["Loi3"] = "Mức lương không bỏ trống";
                    return View(ttd);
                }

                if (string.IsNullOrEmpty(ttd.SoLuongTuyen.ToString()) || !ttd.SoLuongTuyen.ToString().All(char.IsDigit) || ttd.SoLuongTuyen < 1)
                {
                    ViewData["Loi4"] = "Số lượng tuyển tối thiểu 1 người";
                    return View(ttd);
                }

                if (string.IsNullOrEmpty(ttd.KinhNghiemLam))
                {
                    ViewData["Loi5"] = "Kinh nghiệm không bỏ trống";
                    return View(ttd);
                }
                if (string.IsNullOrEmpty(ttd.MoTa_TTD))
                {
                    ViewData["Loi6"] = "Mô tả không bỏ trống";
                    return View(ttd);
                }
                if (string.IsNullOrEmpty(logoDN))
                {
                    ViewBag.ErrorLogo = "Logo doanh nghiệp chưa có, vui lòng vào thông tin doanh nghiệp để cập nhật";
                    return View(ttd);
                }
                TinTuyenDung ttds = db.TinTuyenDungs.Single(x => x.Id_TTD == id);
                var idnts = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == id)?.Id_NTD;
                if (ttds.XetDuyet == "Duyệt thành công")
                {
                    ViewBag.isEdit = "Không thể cập nhật do công việc đã duyệt thành công";
                    return View(ttd);
                }
                ttds.TieuDe_TTD = ttd.TieuDe_TTD;
                ttds.HanTuyenDung = ttd.HanTuyenDung;
                ttds.MucLuongTD = ttd.MucLuongTD;
                ttds.YeuCauGioiTinh = ttd.YeuCauGioiTinh;
                ttds.CapBacTD = ttd.CapBacTD;
                ttds.SoLuongTuyen = ttd.SoLuongTuyen;
                ttds.HinhThucLamViec = ttd.HinhThucLamViec;
                ttds.DiaDiem_TTD = ttd.DiaDiem_TTD;
                ttds.MoTa_TTD = ttd.MoTa_TTD;
                ttds.KinhNghiemLam = ttd.KinhNghiemLam;
                db.SubmitChanges();


                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }

            return View(ttd);
        }

        public ActionResult Delete(int id)
        {
            var tinTuyenDung = db.TinTuyenDungs.SingleOrDefault(t => t.Id_TTD == id);
            var idnts = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == id)?.Id_NTD;
            if (tinTuyenDung == null)
            {
                return HttpNotFound();
            }
            if (tinTuyenDung.XetDuyet == "Duyệt thành công")
            {
                TempData["isDel"] = "Không thể xóa do công việc đã duyệt thành công";
                ViewBag.isDel = "Không thể xóa do công việc đã duyệt thành công";
                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }
            if (tinTuyenDung.TrangThaiDang == true)
            {
                TempData["isDel"] = "Không thể xóa do công việc đã được đăng";
                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }


            db.TinTuyenDungs.DeleteOnSubmit(tinTuyenDung);
            db.SubmitChanges();

            return RedirectToAction("ListTTD", new { id = idnts.ToString() });
        }


        #endregion
        #region list ứng tuyển
        public ActionResult ListUT(int id, int page = 1, int pageSize = 5)
        {
            var uvs = db.UV_TTDs.Where(u => u.Id_TTD == id)
                                 .OrderByDescending(u => u.Id_UT)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            int totalItems = db.UV_TTDs.Count(u => u.Id_TTD == id);
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.TotalItems = totalItems;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.IdTTD = id;

            return View(uvs);
        }

        public ActionResult UpdateTTUT(int id)
        {
            UV_TTD uvs = db.UV_TTDs.Single(o => o.Id_UT == id);

            uvs.TinhTrangUngTuyen = "Đậu";
            db.SubmitChanges();
            return RedirectToAction("ListUT", new { id = uvs.Id_TTD });
        }

        public ActionResult XemChiTiet(int id)
        {
            var uvttd = db.UV_TTDs.SingleOrDefault(o => o.Id_UT == id);
            if (uvttd == null)
            {
                ViewBag.KoTimThay = "Không có hồ sơ";
                return RedirectToAction("ListUT", new { id = uvttd.Id_TTD });
            }
            return View(uvttd);

        }
        public ActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string filePath = Server.MapPath("~/Content/CvUser/") + fileName;

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileType = MimeMapping.GetMimeMapping(filePath);
            return File(fileBytes, fileType, fileName);
        }
        public ActionResult ViewFile(string fileName)
        {
            string filePath = Path.Combine(Server.MapPath("~/Content/CvUser/"), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            return File(filePath, "application/pdf");
        }
        #endregion

        #region Đăng tin
        public ActionResult DangTin()
        {
            var phitin = db.PhiTinTuyenDungs.SingleOrDefault(o => o.ApDungPhi == true);
            ViewBag.PhiTin = phitin.Id_PTTD;
            return View();
        }
        public ActionResult DangTin(int? id, int phitin)
        {
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == id);
            if (ttd.TrangThaiDang == false)
            {
                if (phitin == 0)
                {
                    ttd.TrangThaiDang = true;
                }
                else
                {
                    ttd.Id_PTTD = phitin;
                    ttd.TrangThaiDang = true;
                }
                db.SubmitChanges();
            }

            return View();
        }
        #endregion
    }
}