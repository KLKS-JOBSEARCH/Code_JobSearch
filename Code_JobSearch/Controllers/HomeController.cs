using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

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
            List<TinTuyenDung> tinTuyenDungs = db.TinTuyenDungs.ToList();
            //List<TinTuyenDung> tinTuyenDungs = db.TinTuyenDungs
            //                                .Where(t => t.HanTuyenDung.HasValue && t.HanTuyenDung > DateTime.Now)
            //                                .OrderByDescending(t => t.HanTuyenDung)
            //                                .ToList();

            //paging
            int NoOfRecordPerPage = 9;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tinTuyenDungs.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;
            tinTuyenDungs = tinTuyenDungs.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();
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

            ViewBag.Title = "Thông tin tuyển dụng";
            ViewBag.ProductName = viewModel.TinTuyenDung.TieuDe_TTD;

            return View(viewModel);
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

        public ActionResult DetailsCompany()
        {
            return View();
        }
        #endregion
        //chưa xử lý



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
        public ActionResult TimKiem(string tenTTD, string thanhPho, string hinhThucLamViec, string capBac, int page = 1)
        {
            var dsTTD = from ttd in db.TinTuyenDungs
                        select ttd;

            // Lọc theo tiêu đề và thành phố nếu có giá trị được nhập
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
            // Phân trang
            int NoOfRecordPerPage = 9;
            int NoOfRecordSkip = (page - 1) * NoOfRecordPerPage;
            List<TinTuyenDung> dsTTDPaged = dsTTD.Skip(NoOfRecordSkip).Take(NoOfRecordPerPage).ToList();
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dsTTD.Count()) / Convert.ToDouble(NoOfRecordPerPage)));
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPages;

            // Danh sách các tỉnh thành để truyền vào dropdown
            var listOfCities = GetListOfCities();
            ViewBag.ListOfCities = new SelectList(listOfCities);

            var Hinhthuc = GetHinhthuc();
            ViewBag.Hinhthuc = new SelectList(Hinhthuc);

            var capbac = GetCapBac();
            ViewBag.Capbac = new SelectList(capbac);

            return View("TimKiem", dsTTDPaged);
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
        #endregion


        
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

    }
}