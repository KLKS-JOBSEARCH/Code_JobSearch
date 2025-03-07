﻿using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Code_JobSearch.Controllers
{
    public class HomeController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                if (hashedPassword.Length > 100)
                {

                    hashedPassword = hashedPassword.Substring(0, 100);
                }

                return hashedPassword;
            }
        }



        #region Job
        public ActionResult Index(int page = 1)
        {
            // Lấy danh sách các tin tuyển dụng đã duyệt và còn hạn tuyển dụng
            List<TinTuyenDung> tinTuyenDungs = db.TinTuyenDungs
                                                .Where(t => t.HanTuyenDung.HasValue
                                                            && t.HanTuyenDung > DateTime.Now
                                                            && t.XetDuyet == "Duyệt thành công")
                                                .OrderByDescending(t => t.TinhPhi_TTD)  // Sắp xếp giảm dần theo TinhPhi_TTD (true sẽ lên đầu)
                                                .ThenByDescending(t => t.HanTuyenDung)  // Sau đó sắp xếp giảm dần theo HanTuyenDung
                                                .ToList();


            // Thực hiện phân trang
            int NoOfRecordPerPage = 6;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tinTuyenDungs.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();

            // Trả về view với danh sách tin tuyển dụng đã phân trang
            return View(tinTuyenDungs);
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

            // Lấy danh sách các công việc liên quan theo lĩnh vực
            var relatedJobs = GetRelatedJobs(viewModel.TinTuyenDung.LinhVuc, id);

            ViewBag.Title = "Thông tin tuyển dụng";
            ViewBag.ProductName = viewModel.TinTuyenDung.TieuDe_TTD;
            ViewBag.RelatedJobs = relatedJobs;

            return View(viewModel);
        }

        public List<TinTuyenDung> GetRelatedJobs(string linhVuc, int excludeId)
        {
            return db.TinTuyenDungs
                     .Where(ttd => ttd.LinhVuc == linhVuc && ttd.Id_TTD != excludeId)
                     .ToList();
        }


        #endregion



        #region Trang doanh nghiệp
        //trang doanh nghiệp
        public ActionResult Company(int page = 1)
        {
            List<DoanhNghiep> doanhNghieps = db.DoanhNghieps.ToList();
            //paging
            int NoOfRecordPerPage = 9;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(doanhNghieps.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            doanhNghieps = doanhNghieps.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();
            return View(doanhNghieps);
        }

        public ActionResult DetailsCompany(int id)
        {
            using (var context = new DatabaseDataContext())
            {
                var viewModel = new CompanyDetailsViewModel();

                // Lấy thông tin của doanh nghiệp
                viewModel.NhaTuyenDung = context.NhaTuyenDungs.SingleOrDefault(ntd => ntd.Id_NTD == id);
                if (viewModel.NhaTuyenDung != null)
                {
                    // Lấy thông tin của doanh nghiệp từ Id_DN
                    viewModel.DoanhNghiep = context.DoanhNghieps.SingleOrDefault(dn => dn.Id_DN == viewModel.NhaTuyenDung.Id_DN);

                    // Lấy danh sách tin tuyển dụng từ Id_NTD và sắp xếp theo ngày hạn tuyển dụng mới nhất đến cũ nhất
                    viewModel.TinTuyenDung = context.TinTuyenDungs
                                                .Where(ttd => ttd.Id_NTD == id && ttd.HanTuyenDung.HasValue && ttd.HanTuyenDung > DateTime.Now)
                                                .OrderByDescending(ttd => ttd.HanTuyenDung)
                                                .ToList();
                }

                return View(viewModel);
            }
        }


        #endregion


        #region User profile
        public ActionResult ProfileUser()
        {

            if (Session["KH"] != null)
            {
                UngVien kh = Session["KH"] as UngVien;



                // Truy vấn để lấy thông tin tài khoản của người dùng từ khóa ngoại
                var taiKhoan = (from tk in db.TaiKhoans
                                where tk.TenTK == kh.TenTK
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

                return View(kh);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            UngVien emp = db.UngViens.SingleOrDefault(x => x.Id_UV == id);
            // Kiểm tra xem người dùng có tồn tại không
            if (emp == null)
            {
                return HttpNotFound();
            }

            return View(emp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UngVien emp, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (DatabaseDataContext db = new DatabaseDataContext())
                    {
                        // Lấy thông tin người dùng cần chỉnh sửa từ cơ sở dữ liệu
                        UngVien nvs = db.UngViens.Single(x => x.Id_UV == id);

                        // Cập nhật thông tin từ form chỉnh sửa
                        nvs.HoTen_TKUV = emp.HoTen_TKUV;
                        nvs.Email_TKUV = emp.Email_TKUV;
                        nvs.SoDienThoai_TKUV = emp.SoDienThoai_TKUV;
                        nvs.TenTK = emp.TenTK;

                        // Xử lý upload hình ảnh nếu có
                        if (uploadFile != null && uploadFile.ContentLength > 0)
                        {
                            // Kiểm tra kích thước của file (giới hạn dưới 1MB)
                            if (uploadFile.ContentLength <= 1024 * 1024) // 1MB = 1024 * 1024 bytes
                            {
                                var fileName = Path.GetFileName(uploadFile.FileName);
                                var filePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), fileName);

                                // Xóa hình ảnh cũ nếu tồn tại
                                if (nvs.HinhAnhTKUV != null)
                                {
                                    var oldFilePath = Path.Combine(Server.MapPath("~/Image/Khachhang"), nvs.HinhAnhTKUV);
                                    if (System.IO.File.Exists(oldFilePath))
                                    {
                                        System.IO.File.Delete(oldFilePath);
                                    }
                                }

                                // Lưu hình ảnh mới vào thư mục
                                uploadFile.SaveAs(filePath);

                                // Cập nhật đường dẫn của ảnh trong cơ sở dữ liệu
                                nvs.HinhAnhTKUV = fileName;
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
                        Session["KH"] = nvs;

                        return RedirectToAction("ProfileUser");
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



        #region lọc nâng cao
        [HttpPost]
        public ActionResult TimKiem(string tenTTD, string thanhPho, string hinhThucLamViec, string capBac, string linhVuc, int page = 1)
        {
            var dsTTD = from ttd in db.TinTuyenDungs
                        where ttd.XetDuyet == "Duyệt thành công" && ttd.HanTuyenDung > DateTime.Now
                        select ttd;

            // Xử lý lọc
            if (!string.IsNullOrEmpty(tenTTD))
            {
                dsTTD = dsTTD.Where(ttd => ttd.TieuDe_TTD.Contains(tenTTD));
            }
            if (!string.IsNullOrEmpty(thanhPho) && thanhPho != "Chọn thành phố")
            {
                dsTTD = dsTTD.Where(ttd => ttd.DiaDiem_TTD.Contains(thanhPho));
            }
            if (!string.IsNullOrEmpty(hinhThucLamViec) && hinhThucLamViec != "Chọn hình thức")
            {
                dsTTD = dsTTD.Where(ttd => ttd.HinhThucLamViec == hinhThucLamViec);
            }
            if (!string.IsNullOrEmpty(capBac) && capBac != "Chọn cấp bậc")
            {
                dsTTD = dsTTD.Where(ttd => ttd.CapBacTD == capBac);
            }
            if (!string.IsNullOrEmpty(linhVuc) && linhVuc != "Chọn lĩnh vực")
            {
                dsTTD = dsTTD.Where(ttd => ttd.LinhVuc == linhVuc);
            }
          

            dsTTD = dsTTD.OrderByDescending(ttd => ttd.HanTuyenDung);

            // Danh sách các tỉnh thành để truyền vào dropdown
            var listOfCities = GetListOfCities();
            ViewBag.ListOfCities = new SelectList(listOfCities);

            var Hinhthuc = GetHinhthuc();
            ViewBag.Hinhthuc = new SelectList(Hinhthuc);

            var capbac = GetCapBac();
            ViewBag.Capbac = new SelectList(capbac);

            var linhvuc = GetLinhvuc();
            {
                ViewBag.Linhvuc = new SelectList(linhvuc);
            }

            return View(dsTTD);
        }


        //Xử lý chức năng lọc tìm kiếm nâng cao
        private List<string> GetListOfCities()
        {
            // Danh sách tên của 63 tỉnh thành
            List<string> cities = new List<string>
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
            return cities;
        }
        private List<string> GetHinhthuc()
        {
            List<string> hinhthuc = new List<string>
            {
                    "Toàn thời gian", "Bán thời gian"
            };
            return hinhthuc;
        }
        private List<string> GetCapBac()
        {
            List<string> capbac = new List<string>
            {
                    "Nhân Viên", "Trường phòng","Quản lý","Thực Tập Sinh"
            };
            return capbac;
        }
        private List<string> GetLinhvuc()
        {
            List<string> linhvuc = new List<string>
            {
                    "An toàn lao động","Bán hàng kỹ thuật","Bán lẻ / Bán sỉ", "Báo chí / Truyền hình", "Bảo hiểm", "Bảo trì / Sửa chữa", "Bất động sản",
                    "Biên / Phiên dịch", "Bưu chính - Viễn thông", "Chứng khoáng / Vàng / Ngoại tệ", "Cơ khí / Chế tạo / Tự động hóa", "Công nghệ cao", 
                    "Công nghệ Ô tô", "Công nghệ thông tin", "Dầu khí / Hóa chất", "Dệt may / Giày da","Địa chất / Khoáng sản", "Dịch vụ khách hàng",
                    "Điện / Điện tử / Điện lạnh","Điện tử viễn thông", "Du lịch", "Dược phẩm / Công nghệ sinh học", "Giáo dục / Đào tạo", "Hành chính / Văn phòng",
                    "Hóa học / Sinh học", "IT phần cứng","IT phần mềm", "Kế toán / Kiểm toán", "Khách sạn / Nhà hàng","Kiến trúc", "Marketing / Truyền thông / Quảng cáo",
                    "Ngành nghề khác","Thiết kế đồ họa / Thiết kế nội thất", "Xuất nhập khẩu", "Y tế / Dược", "Ngành nghề khác"
            };
            return linhvuc;
        }
        #endregion



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

    }
}