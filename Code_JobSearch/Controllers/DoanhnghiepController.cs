﻿using Code_JobSearch.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.WebPages;
using static Code_JobSearch.Controllers.AuthController;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Code_JobSearch.Controllers
{
    public class DoanhNghiepController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();


        // Danh sách vị trí công tác
        private List<string> danhSachViTriCongTac = new List<string>
        {
            "Nhân viên", "Phó phòng","Trưởng phòng", "Phó giám đốc", "Giám đốc", "Tổng giám đốc"
        };

        // Danh sách thành phố
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

        // Danh sách lĩnh vực
        private List<string> danhSachLinhVuc = new List<string>
        {
            "An toàn lao động","Bán hàng kỹ thuật","Bán lẻ / Bán sỉ", "Báo chí / Truyền hình", "Bảo hiểm",
            "Bảo trì / Sửa chữa", "Bất động sản", "Biên / Phiên dịch", "Bưu chính - Viễn thông",
            "Chứng khoáng / Vàng / Ngoại tệ", "Cơ khí / Chế tạo / Tự động hóa", "Công nghệ cao",
            "Công nghệ Ô tô", "Công nghệ thông tin", "Dầu khí / Hóa chất", "Dệt may / Giày da",
            "Địa chất / Khoáng sản", "Dịch vụ khách hàng",  "Điện / Điện tử / Điện lạnh",
            "Điện tử viễn thông", "Du lịch", "Dược phẩm / Công nghệ sinh học",
            "Giáo dục / Đào tạo", "Hành chỉnh / Văn phòng",  "Hóa học / Sinh học",
            "IT phần cứng","IT phần mềm", "Kế toán / Kiểm toán", "Khách sạn / Nhà hàng","Kiến trúc",
            "Marketing / Truyền thông / Quảng cáo","Thiết kế đồ họa / Thiết kế nội thất",
            "Xuất nhập khẩu", "Y tế / Dược", "Ngành nghề khác"
        };

        #region Màn hình chính
        public ActionResult Index()
        {

            return View();

        }
        public ActionResult FilterPosts(int? month, int? year)
        {

            NhaTuyenDung ntd = Session["NTD"] as NhaTuyenDung;

            var postCounts = db.TinTuyenDungs
                                .Where(o => o.ThoiGianDangTuyen.Value.Month == month &&
                                            o.ThoiGianDangTuyen.Value.Year == year &&
                                            o.XetDuyet == "Duyệt thành công" && o.Id_NTD == ntd.Id_NTD)
                                .GroupBy(o => o.ThoiGianDangTuyen.Value.Day)
                                .Select(g => new { day = g.Key, postCount = g.Count() })
                                .OrderBy(x => x.day)
                                .ToList();

            var candidateCounts = db.UV_TTDs
                                    .Where(o => o.ThoiGianUngTuyen.Value.Month == month &&
                                                o.ThoiGianUngTuyen.Value.Year == year &&
                                                (o.TinhTrangUngTuyen == "Đã ứng tuyển"
                                                || o.TinhTrangUngTuyen == "Đậu"
                                                || o.TinhTrangUngTuyen == "Rớt"
                                                || o.TinhTrangUngTuyen == "Đang xét duyệt") &&
                                                db.TinTuyenDungs.Any(ttd => ttd.Id_TTD == o.Id_TTD && ttd.Id_NTD == ntd.Id_NTD))
                                    .GroupBy(o => o.ThoiGianUngTuyen.Value.Day)
                                    .Select(g => new { day = g.Key, candidateCount = g.Count() })
                                    .OrderBy(x => x.day)
                                    .ToList();

            var passedCandidateCounts = db.UV_TTDs
                                          .Where(o => o.ThoiGianUngTuyen.Value.Month == month &&
                                                      o.ThoiGianUngTuyen.Value.Year == year &&
                                                      o.TinhTrangUngTuyen == "Đậu" &&
                                                      db.TinTuyenDungs.Any(ttd => ttd.Id_TTD == o.Id_TTD && ttd.Id_NTD == ntd.Id_NTD))
                                          .GroupBy(o => o.ThoiGianUngTuyen.Value.Day)
                                          .Select(g => new { day = g.Key, passedCandidateCount = g.Count() })
                                          .OrderBy(x => x.day)
                                          .ToList();

            return Json(new { posts = postCounts, candidates = candidateCounts, passedCandidates = passedCandidateCounts }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Chi tiết tin tuyển dụng
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
            ViewBag.isDangTin = TempData["isDangTin"];
            return View(viewModel);
        }
        #endregion


        #region Danh sách tin tuyển dụng
        public ActionResult ListTTD(int? id, string searchTitle, string searchStatus, string searchApproval, int page = 1, int pageSize = 5)
        {
            if (id.HasValue)
            {
                ViewBag.idntd = id.Value;

                var query = db.TinTuyenDungs.Where(o => o.Id_NTD == id);

                // Tìm kiếm theo tiêu đề tin
                if (!string.IsNullOrEmpty(searchTitle))
                {
                    query = query.Where(o => o.TieuDe_TTD.ToLower().Contains(searchTitle));
                }

                // Tìm kiếm theo trạng thái đăng
                if (!string.IsNullOrEmpty(searchStatus))
                {
                    bool status = searchStatus == "1"; // Giả sử 1 là đã đăng, 0 là chưa đăng
                    query = query.Where(o => o.TrangThaiDang == status);
                }

                // Tìm kiếm theo trạng thái xét duyệt
                if (!string.IsNullOrEmpty(searchApproval))
                {
                    query = query.Where(o => o.XetDuyet == searchApproval);
                }

                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var paginatedPosts = query.OrderByDescending(o => o.Id_TTD)
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToList();

                ViewBag.TotalItems = totalItems;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                ViewBag.isDel = TempData["isDel"];
                ViewBag.isEdit = TempData["isEdit"];
                ViewBag.MessageMoMo = TempData["MessageMoMo"];


                ViewBag.SearchTitle = searchTitle;
                ViewBag.SearchStatus = searchStatus;
                ViewBag.SearchApproval = searchApproval;

                return View(paginatedPosts);
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }
        #endregion

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

        #region Thông tin doanh nghiệp
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
        #endregion

        #region Sửa thông tin doanh nghiệp
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
                        ViewBag.HADN = dns.Logo_DN;
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
                        if (!emp.MaSoThue_DN.All(char.IsDigit) || emp.MaSoThue_DN.Length > 10 || emp.MaSoThue_DN.Length < 10)
                        {
                            ViewData["Loi2"] = "Mã số thuế là 10 chữ số";
                            return View(emp);
                        }


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

        #region Sửa thông tin nhà tuyển dụng
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
                    var validPrefixes = new List<string> { "09", "03", "07", "08", "05" };
                    using (DatabaseDataContext db = new DatabaseDataContext())
                    {
                        // Lấy thông tin người dùng cần chỉnh sửa từ cơ sở dữ liệu
                        NhaTuyenDung ntds = db.NhaTuyenDungs.Single(x => x.Id_NTD == id);
                        ViewBag.HA = ntds.HinhAnh_NTD;

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
                        if (!emp.SoDienThoai_NTD.All(char.IsDigit) || emp.SoDienThoai_NTD.Length != 10)
                        {
                            ViewData["Loi_SDTNTD"] = "Số điện thoại gồm 10 chữ số";
                            return View(emp);
                        }
                        if (!validPrefixes.Any(prefix => emp.SoDienThoai_NTD.StartsWith(prefix)))
                        {
                            ViewData["Loi_SDTNTD"] = "Số điện thoại không hợp lệ";
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
        #endregion

        #region Xem thông tin nhà tuyển dụng
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




        #endregion

        #region Thêm tin tuyển dụng
        public ActionResult Create()
        {

            ViewBag.DSThanhPho = danhSachThanhPho;
            ViewBag.DSLinhVuc = danhSachLinhVuc;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, TinTuyenDung ttd)
        {
            ViewBag.DSThanhPho = danhSachThanhPho;
            ViewBag.DSLinhVuc = danhSachLinhVuc;
            if (ModelState.IsValid)
            {
                var logoDN = (from ntd in db.NhaTuyenDungs
                              join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                              where ntd.Id_NTD == id
                              select dn.Logo_DN).FirstOrDefault();
                ttd.Id_NTD = id;

                ttd.TrangThaiDang = false;


                if (string.IsNullOrEmpty(ttd.TieuDe_TTD))
                {
                    ViewData["Loi1"] = "Tiêu đề không bỏ trống";
                    return View();
                }

                if (string.IsNullOrEmpty(ttd.MucLuongTD))
                {
                    ViewData["Loi3"] = "Mức lương không bỏ trống";
                    return View();
                }


                if (ttd.SoLuongTuyen.ToString() == "" || !ttd.SoLuongTuyen.ToString().All(char.IsDigit) || ttd.SoLuongTuyen < 1)
                {
                    ViewData["Loi4"] = "Số lượng tuyển tối thiểu 1 người";
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
        #endregion

        #region Sửa tin tuyển dụng
        [HttpGet]
        public ActionResult Edit(int id)
        {

            ViewBag.DSThanhPho = danhSachThanhPho;
            ViewBag.DSLinhVuc = danhSachLinhVuc;
            var tinTuyenDung = db.TinTuyenDungs.FirstOrDefault(t => t.Id_TTD == id);
            var idnts = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == id)?.Id_NTD;
            if (tinTuyenDung == null)
            {
                return HttpNotFound();
            }
            if (tinTuyenDung.XetDuyet == "Duyệt thành công" || tinTuyenDung.XetDuyet == "Duyệt không thành công")
            {
                TempData["isEdit"] = "Không thể sửa do công việc đã được duyệt";
                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }
            var logoDN = (from ttdung in db.TinTuyenDungs
                          join ntd in db.NhaTuyenDungs on ttdung.Id_NTD equals ntd.Id_NTD
                          join dn in db.DoanhNghieps on ntd.Id_DN equals dn.Id_DN
                          where ttdung.Id_TTD == id
                          select dn.Logo_DN).FirstOrDefault();

            ViewBag.LogoDN = logoDN;
            TempData["isEdit"] = null;
            return View(tinTuyenDung);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TinTuyenDung ttd)
        {
            ViewBag.DSThanhPho = danhSachThanhPho;
            ViewBag.DSLinhVuc = danhSachLinhVuc;
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
                var thoigiandang = db.TinTuyenDungs.FirstOrDefault(o => o.Id_TTD == id).ThoiGianDangTuyen;
                ViewBag.TGDT = thoigiandang;
                if (thoigiandang.HasValue)
                {
                    if (ttd.HanTuyenDung == null)
                    {
                        ViewData["Loi2"] = "Hạn tuyển dụng không bỏ trống";
                        return View(ttd);
                    }
                    if (ttd.HanTuyenDung.Value.Date < thoigiandang.Value.Date.AddDays(8))
                    {
                        ViewData["Loi2"] = "Hạn tuyển dụng cách ngày đăng ít nhất 8 ngày";
                        return View(ttd);
                    }
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

                if (ttd.ThoiGianDangTuyen.HasValue)
                {
                    ttds.HanTuyenDung = ttd.HanTuyenDung;
                }
                ttds.HanTuyenDung = ttd.HanTuyenDung;
                ttds.MucLuongTD = ttd.MucLuongTD;
                ttds.YeuCauGioiTinh = ttd.YeuCauGioiTinh;
                ttds.CapBacTD = ttd.CapBacTD;
                ttds.SoLuongTuyen = ttd.SoLuongTuyen;
                ttds.HinhThucLamViec = ttd.HinhThucLamViec;
                ttds.DiaDiem_TTD = ttd.DiaDiem_TTD;
                ttds.LinhVuc = ttd.LinhVuc;
                ttds.MoTa_TTD = ttd.MoTa_TTD;
                ttds.KinhNghiemLam = ttd.KinhNghiemLam;
                db.SubmitChanges();


                return RedirectToAction("ListTTD", new { id = idnts.ToString() });
            }

            return View(ttd);
        }
        #endregion

        #region Xóa tin tuyển dụng
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
            TempData["isDel"] = null;
            return RedirectToAction("ListTTD", new { id = idnts.ToString() });
        }
        #endregion

        #region Kiểm tra trạng thái ứng viên
        public JsonResult Check_TTUV(int id)
        {
            UV_TTD uvs = db.UV_TTDs.Single(o => o.Id_UT == id);
            bool canProceed = uvs.TinhTrangUngTuyen == "Đã ứng tuyển" || uvs.TinhTrangUngTuyen == "Đang xét duyệt";
            return Json(new { canProceed = canProceed, message = canProceed ? "" : "Trạng thái ứng tuyển đã được xét" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Tính trung bình cộng điểm thông qua id_UV
        public double AverageMucDoDiem(int idUV)
        {
            var sum = db.DanhGia_UVs
                            .Where(dg => dg.Id_UV == idUV)
                            .Sum(dg => dg.MucDoDiem);
            var count = db.DanhGia_UVs.Where(dg => dg.Id_UV == idUV).Count();
            var average = sum / (double)count;
            return average.HasValue ? Math.Round(average.Value, 2) : -1;
        }



        #region Danh sách ứng viên hệ thống

        public ActionResult ListUV(int? id, int page = 1, int pageSize = 5)
        {
            if (id.HasValue)
            {
                ViewBag.IDNTD = id.Value;
                var query = db.UngViens.ToList();
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var paginatedPosts = query.Skip((page - 1) * pageSize)
                                         .Take(pageSize).ToList();
                //Danh sách điểm
                List<double> ListDiemDG = new List<double>();
                foreach (UngVien uv in db.UngViens)
                {
                    var dtb = AverageMucDoDiem(uv.Id_UV);
                    ListDiemDG.Add(dtb);
                }
                ViewBag.DiemDanhGia = ListDiemDG.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.TotalItems = totalItems;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;

                return View(paginatedPosts);
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }
        #endregion

        #region Danh sách đánh giá ứng viên

        public ActionResult ListDG(int? id, string searchLinhVuc, int page = 1, int pageSize = 5)
        {
            ViewBag.LinhVucList = danhSachLinhVuc;
            if (id.HasValue)
            {

                var query = db.DanhGia_UVs.Where(o => o.Id_UV.Equals(id));
                ViewBag.UV = db.UngViens.SingleOrDefault(o => o.Id_UV.Equals(id));
                ViewBag.HSUV = db.HoSoXinViecs.SingleOrDefault(o => o.Id_UV == id);
                if (query.Count() > 0)
                {


                    int totalItems = query.Count();
                    int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                    var paginatedPosts = query.OrderByDescending(o => o.Id_DGUV)
                                            .Skip((page - 1) * pageSize)
                                             .Take(pageSize).ToList();

                    var transformedPosts = paginatedPosts.Select(dg_uv => new DanhGia_UV_NTD_TTD
                    {
                        uv = db.UngViens.FirstOrDefault(uv => uv.Id_UV == dg_uv.Id_UV),
                        ntd = db.NhaTuyenDungs.FirstOrDefault(ntd => ntd.Id_NTD == dg_uv.Id_NTD),
                        dn = (from dn in db.DoanhNghieps
                              join ntd in db.NhaTuyenDungs
                              on dn.Id_DN equals ntd.Id_DN
                              where ntd.Id_NTD.Equals(dg_uv.Id_NTD)
                              select dn).FirstOrDefault(),
                        ttd = db.TinTuyenDungs.FirstOrDefault(ttd => ttd.Id_TTD == dg_uv.Id_TTD),
                        dg = dg_uv
                    });

                    // Tìm kiếm theo lĩnh vực
                    if (!string.IsNullOrEmpty(searchLinhVuc))
                    {
                        transformedPosts = transformedPosts.Where(o => o.ttd.LinhVuc == searchLinhVuc).ToList();
                    }
                    else
                    {
                        transformedPosts = transformedPosts.ToList();
                    }
                    var sum = transformedPosts.Where(o => o.uv.Id_UV.Equals(id)).Sum(o => o.dg.MucDoDiem);
                    var count = transformedPosts.Where(o => o.uv.Id_UV.Equals(id)).Count();
                    var average = sum / (double)count;
                    double TongDiem = Math.Round(average.Value, 2);

                    ViewBag.TongDiem = TongDiem;
                    ViewBag.TotalItems = totalItems;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalPages = totalPages;
                    ViewBag.idUV = id;
                    ViewBag.SearchLinhVuc = searchLinhVuc;
                    return View(transformedPosts);
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login_DoanhNghiep", "Auth");
            }
        }
        #endregion

        #region Gửi lời nhắn đến ứng viên
        public static bool SendMessageDN(string email, string dn, string title, string text, string filePath)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("jobstarvn@gmail.com", "jxtt juxf gbqy nbdp"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("jobstarvn@gmail.com"),
                    Subject = title,
                    Body = $@"<html>
                <head>
                    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css' rel='stylesheet'>
                </head>
                <body>
                    <div class='container'>
                        <div class='content'>
                            <h3>Thân gửi bạn! Đây lời nhắn của doanh nghiệp {dn}</h3>
                            <h4>Nội dung</h4>
                            <p>{text}</p>
                        </div>
                    </div>
                </body>
            </html>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                if (!string.IsNullOrEmpty(filePath))
                {
                    mailMessage.Attachments.Add(new Attachment(filePath));
                }

                client.Send(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        public ActionResult SendEmailDN(int id, int idNTD, string subject, string body, HttpPostedFileBase file)
        {
            try
            {

                string filePath = null;
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/CvUser/"), fileName);
                    file.SaveAs(path);
                    filePath = path;
                }
                var uv = db.UngViens.FirstOrDefault(o => o.Id_UV.Equals(id));

                var iddn = db.NhaTuyenDungs.FirstOrDefault(o => o.Id_NTD.Equals(idNTD)).Id_DN;
                string tendn = db.DoanhNghieps.FirstOrDefault(o => o.Id_DN.Equals(iddn)).Ten_DN;

                bool isSent = SendMessageDN(uv.Email_TKUV, tendn, subject, body, filePath);


                return Json(new { success = isSent });


            }
            catch
            {
                return Json(new { success = false });
            }
        }


        #endregion

        #region Đánh giá ứng viên theo tin
        [HttpGet]
        public ActionResult DanhGiaUV(int? idut)
        {
            var ut = db.UV_TTDs.SingleOrDefault(o => o.Id_UT.Equals(idut));
            var dguv = db.DanhGia_UVs.SingleOrDefault(o => o.Id_TTD.Equals(ut.Id_TTD) && o.Id_UV.Equals(ut.Id_UV));
            ViewBag.idut = idut;
            if (dguv != null)
            {

                TempData["isDanhGia"] = "Ứng viên đã được đánh giá trong tin tuyển dụng này!";
                return RedirectToAction("XemChiTiet", new { id = idut });
            }
            else
            {
                if (ut.TinhTrangUngTuyen != "Đậu")
                {
                    TempData["isDanhGia"] = "Ứng viên chưa đậu không thể đánh giá!";
                    return RedirectToAction("XemChiTiet", new { id = idut });
                }
                //Mở khóa chỗ này để xét thời gian đánh giá ứng viên tối thiểu cách ngày ứng viên đậu 21 ngày
                //else
                //{
                //    if (ut.ThoiGianDaXetDuyet.Value.Date < DateTime.Now.Date.AddDays(21))
                //    {
                //        TempData["isDanhGia"] = "Thời gian đã xét duyệt tới hiện tại phải ít nhất 21 ngày mới có thể đánh giá!";
                //        return RedirectToAction("XemChiTiet", new { id = idut });
                //    }
                //}
            }
            return View();
        }
        [HttpPost]
        public ActionResult DanhGiaUV(int? idut, DanhGia_UV dg, FormCollection f)
        {
            var ut = db.UV_TTDs.SingleOrDefault(o => o.Id_UT.Equals(idut));
            var dguv = db.DanhGia_UVs.SingleOrDefault(o => o.Id_TTD.Equals(ut.Id_TTD) && o.Id_UV.Equals(ut.Id_UV));
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD.Equals(ut.Id_TTD));
            ViewBag.idut = idut;
            if (dguv == null)
            {
                var noidung = f["NoiDungDG"];
                var mucdodanhgia = f["MucDoDanhGia"];

                ViewBag.NoiDungDG = noidung;

                if (string.IsNullOrEmpty(noidung))
                {
                    ViewData["Loi1"] = "Vui lòng điền nội dung!";
                    return View();
                }

                if (string.IsNullOrEmpty(mucdodanhgia) || !int.TryParse(mucdodanhgia, out int mucDo))
                {
                    ViewData["Loi2"] = "Vui lòng chọn mức độ đánh giá!";
                    return View();
                }

                if (mucDo < 1 || mucDo > 5)
                {
                    ViewData["Loi2"] = "Mức độ đánh giá phải từ 1 đến 5!";
                    return View();
                }

                dg.Id_NTD = ttd.Id_NTD;
                dg.Id_UV = ut.Id_UV;
                dg.Id_TTD = ut.Id_TTD;
                dg.NoiDung_DG = noidung;
                dg.MucDoDiem = Convert.ToInt16(mucdodanhgia);
                dg.ThoiGian_DG = DateTime.Now;

                db.DanhGia_UVs.InsertOnSubmit(dg);
                db.SubmitChanges();

                ViewBag.TB = "Thành công!";
                return RedirectToAction("XemChiTiet", new { id = idut });
            }
            else
            {
                ViewBag.LoiDG = "Ứng viên đã được đánh giá trong tin tuyển dụng này";

            }
            return View();
        }
        #endregion

        #region Gửi thông báo cho ứng viên Đậu hoặc Rớt
        public static bool SendMessageDau_Rot(string email, string tieudeTin, string dn, string title, string text, string status, string filePath)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("jobstarvn@gmail.com", "jxtt juxf gbqy nbdp"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("jobstarvn@gmail.com"),
                    Subject = title,
                    Body = $@"<html>
                <head>
                    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css' rel='stylesheet'>
                </head>
                <body>
                    <div class='container'>
                        <div class='content'>
                            <h3>Thân gửi bạn! Đây là thông báo {status} ứng tuyển</h3>
                            <h4>Tin ứng tuyển: {tieudeTin}</h4>
                            <h4>Doanh nghiệp: {dn}<h4>
                            <h4>Nội dung</h4>
                            <p>{text}</p>
                        </div>
                    </div>
                </body>
            </html>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                if (!string.IsNullOrEmpty(filePath))
                {
                    mailMessage.Attachments.Add(new Attachment(filePath));
                }

                client.Send(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        public ActionResult SendEmail(int id, string subject, string body, string status, HttpPostedFileBase file)
        {
            try
            {

                string filePath = null;
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/CvUser/"), fileName);
                    file.SaveAs(path);
                    filePath = path;
                }
                var ut = db.UV_TTDs.FirstOrDefault(o => o.Id_UT.Equals(id));
                string tieudeTTD = db.TinTuyenDungs.FirstOrDefault(o => o.Id_TTD.Equals(ut.Id_TTD)).TieuDe_TTD;
                var idntd = db.TinTuyenDungs.FirstOrDefault(o => o.Id_TTD.Equals(ut.Id_TTD)).Id_NTD;
                var iddn = db.NhaTuyenDungs.FirstOrDefault(o => o.Id_NTD.Equals(idntd)).Id_DN;
                string tendn = db.DoanhNghieps.FirstOrDefault(o => o.Id_DN.Equals(iddn)).Ten_DN;

                bool isSent = SendMessageDau_Rot(ut.EmailUV_TD, tieudeTTD, tendn, subject, body, status, filePath);

                if (isSent)
                {
                    UV_TTD uvs = db.UV_TTDs.Single(o => o.Id_UT == id);
                    if (uvs.TinhTrangUngTuyen == "Đã ứng tuyển" || uvs.TinhTrangUngTuyen == "Đang xét duyệt")
                    {
                        uvs.TinhTrangUngTuyen = status;
                        uvs.ThoiGianDaXetDuyet = DateTime.Now;
                        db.SubmitChanges();

                    }
                }
                return Json(new { success = isSent });


            }
            catch
            {
                return Json(new { success = false });
            }
        }


        #endregion

        #region Chi tiết ứng viên ứng tuyển
        public ActionResult XemChiTiet(int id)
        {
            var uvttd = db.UV_TTDs.SingleOrDefault(o => o.Id_UT == id);
            var dg = db.DanhGia_UVs.SingleOrDefault(o => o.Id_TTD == uvttd.Id_TTD && o.Id_UV == uvttd.Id_UV);
            if (uvttd == null)
            {
                ViewBag.KoTimThay = "Không có hồ sơ";
                return RedirectToAction("ListUT", new { id = uvttd.Id_TTD });
            }
            if (dg != null)
            {
                ViewBag.NoiDungDG = dg.NoiDung_DG;
                ViewBag.DiemDG = dg.MucDoDiem;
            }
            ViewBag.isDanhGia = TempData["isDanhGia"];
            return View(uvttd);

        }
        #endregion

        #region Tải file CV của ứng viên
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
        #endregion

        #region Xem CV ứng viên trên website
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

        #region Xem CV ứng viên bên danh sách ứng tuyển
        public ActionResult ViewFile_DSUV(int id)
        {
            var ut = db.UV_TTDs.SingleOrDefault(o => o.Id_UT == id);
            string fileName = ut.File_CV;
            string filePath = Path.Combine(Server.MapPath("~/Content/CvUser/"), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            return File(filePath, "application/pdf");
        }
        #endregion

        #region Danh sách ứng viên ứng tuyển

        public ActionResult ListUT(int id, string searchStatusUV, int page = 1, int pageSize = 5)
        {
            var query = db.UV_TTDs.Where(o => o.Id_TTD == id);


            // Tìm kiếm theo tình trạng ứng tuyển
            if (!string.IsNullOrEmpty(searchStatusUV))
            {
                query = query.Where(o => o.TinhTrangUngTuyen == searchStatusUV);
            }

            var allResults = query.OrderByDescending(u => u.Id_UT).ToList();
            var filteredResults = new List<UV_TTD>();
            var passedApplicants = new HashSet<int>();

            foreach (var item in allResults)
            {
                if (item.TinhTrangUngTuyen == "Đậu")
                {
                    if (!passedApplicants.Contains(item.Id_UV.Value))
                    {
                        filteredResults.Add(item);
                        passedApplicants.Add(item.Id_UV.Value);
                    }
                }
                else if (!passedApplicants.Contains(item.Id_UV.Value))
                {
                    filteredResults.Add(item);
                }
            }
            for (int i = 0; i < filteredResults.Count; i++)
            {
                for (int j = 0; j < filteredResults.Count; j++)
                {
                    if (filteredResults[j].Id_UV == filteredResults[i].Id_UV
                        && filteredResults[j].TinhTrangUngTuyen != filteredResults[i].TinhTrangUngTuyen
                        && filteredResults[i].TinhTrangUngTuyen == "Đậu")
                    {
                        filteredResults.Remove(filteredResults[j]);
                    }
                }
            }
            int totalItems = filteredResults.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var uvs = filteredResults.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            ViewBag.TotalItems = totalItems;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.IdTTD = id;
            ViewBag.SearchStatusUV = searchStatusUV;
            return View(uvs);
        }
        #endregion

        #region Thanh toán MoMo
        public ActionResult Payment(string giatien, int IdTTD, DateTime hantd)
        {
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == IdTTD);
            var ntd = db.NhaTuyenDungs.SingleOrDefault(o => o.Id_NTD == ttd.Id_NTD);
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = $"Đăng tin tuyển dụng có mã tin {IdTTD} của tài khoản {ntd.TenTK}";

            string returnUrl = $"https://localhost:44354/DoanhNghiep/ConfirmPaymentClient?idttd={IdTTD}&htd={hantd}";
            string notifyurl = $"https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/DoanhNghiep/SavePayment";

            string amount = giatien;
            string orderid = DateTime.Now.Ticks.ToString();
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);
            string url = jmessage.GetValue("payUrl")?.ToString();
            return Redirect(jmessage.GetValue("payUrl").ToString());
        }



        public ActionResult ConfirmPaymentClient(int idttd, string htd)
        {

            string errorCode = Request.QueryString["errorCode"];
            string orderId = Request.QueryString["orderId"];
            string requestId = Request.QueryString["requestId"];
            string amount = Request.QueryString["amount"];
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == idttd);
            var phitin = db.PhiTinTuyenDungs.SingleOrDefault(o => o.Gia_PTTD.ToString() == amount);
            DateTime hanTuyenDung;
            if (!DateTime.TryParse(htd, out hanTuyenDung))
            {
                throw new ArgumentException("Invalid datetime format for htd");
            }

            if (errorCode == "0")
            {
                ttd.Id_PTTD = phitin.Id_PTTD;
                ttd.TrangThaiDang = true;
                ttd.ThoiGianDangTuyen = DateTime.Now;
                ttd.HanTuyenDung = hanTuyenDung;
                ttd.TinhPhi_TTD = true;
                ttd.XetDuyet = "Đang xét duyệt";
                db.SubmitChanges();
                TempData["MessageMoMo"] = "Thanh toán thành công!";

            }
            else
            {
                TempData["MessageMoMo"] = "Thanh toán thất bại!";
            }
            return RedirectToAction("ListTTD", new { id = ttd.Id_NTD, page = 1, pageSize = 5 });
        }

        public void SavePayment()
        {


        }
        #endregion

        #region Đăng tin
        public ActionResult DangTin(int idttd)
        {
            var phitin = db.PhiTinTuyenDungs.SingleOrDefault(o => o.ApDungPhi == true);
            ViewBag.PhiTin = phitin.Gia_PTTD;
            ViewBag.IdPhiTin = phitin.Id_PTTD;
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == idttd);
            if (ttd == null)
            {
                return HttpNotFound();
            }
            if (ttd.TrangThaiDang == true)
            {
                TempData["isDangTin"] = "Không thể đăng do tin đã được đăng";
                return RedirectToAction("DetailsJob", new { id = idttd });
            }
            TempData["isDangTin"] = null;
            return View(ttd);
        }
        [HttpPost]
        public ActionResult DangTin(int idttd, int idphitin, DateTime? htd)
        {
            var ttd = db.TinTuyenDungs.SingleOrDefault(o => o.Id_TTD == idttd);
            var phitin = db.PhiTinTuyenDungs.SingleOrDefault(o => o.ApDungPhi == true);
            ViewBag.PhiTin = phitin.Gia_PTTD;
            ViewBag.IdPhiTin = phitin.Id_PTTD;
            if (!htd.HasValue)
            {
                ViewData["LoiHTD"] = "Hạn tuyển dụng bắt buộc nhập";
                return View(ttd);
            }
            if (ttd.TrangThaiDang == false)
            {
                if (htd.Value.Date < DateTime.Now.Date.AddDays(8))
                {
                    ViewData["LoiHTD"] = "Hạn tuyển dụng cần ít nhất 8 ngày";
                    return View(ttd);
                }
                if (idphitin == 0)
                {
                    ttd.TrangThaiDang = true;
                    ttd.ThoiGianDangTuyen = DateTime.Now;
                    ttd.HanTuyenDung = htd;
                    ttd.TinhPhi_TTD = false;
                    ttd.XetDuyet = "Đang xét duyệt";
                }
                else
                {
                    string gt = phitin.Gia_PTTD.ToString();
                    return RedirectToAction("Payment", new { giatien = gt, IdTTD = idttd, hantd = htd });


                }
                db.SubmitChanges();
            }
            else
            {
                ViewBag.LoiDangTin = "Tin tuyển dụng đã đăng";
                return View(ttd);
            }
            return RedirectToAction("ListTTD", new { id = ttd.Id_NTD, page = 1, pageSize = 5 });
        }
        #endregion
    }
}