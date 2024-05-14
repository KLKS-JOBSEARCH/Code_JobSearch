using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Controllers
{
    [HandleError]
    public class AuthController : Controller
    {
        // GET: Auth
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


        #region Auth User
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(UngVien kh, TaiKhoan ac, FormCollection f)
        {
            var hoten = f["HotenKH"];
            var tendn = f["TenDN"];
            var matkhau = f["MatKhau"];
            var rematkhau = f["ReMatkhau"];
            var email = f["email"];
            var sdt = f["sdt"];

            // Lưu trữ dữ liệu đã nhập vào ViewBag để hiển thị lại trong input nếu có lỗi
            ViewBag.HotenKH = hoten;
            ViewBag.TenDN = tendn;
            ViewBag.Email = email;
            ViewBag.SDT = sdt;


            if (string.IsNullOrEmpty(hoten) ||
               string.IsNullOrEmpty(tendn) ||
               string.IsNullOrEmpty(matkhau) ||
               string.IsNullOrEmpty(rematkhau) ||
               string.IsNullOrEmpty(email) ||
               string.IsNullOrEmpty(sdt))
            {
                ViewData["Loi"] = "Vui lòng điền đầy đủ thông tin!";
                return View();
            }


            if (matkhau != rematkhau)
            {
                ViewData["Loi1"] = "Mật khẩu nhập lại không khớp!";
                return View();
            }
            // Kiểm tra mật khẩu hợp lệ
            if (string.IsNullOrEmpty(matkhau) || matkhau.Length < 8)
            {
                ViewData["Loi4"] = "Mật khẩu không hợp lệ! Mật khẩu phải lớn hơn 8 ký tự.";
                return View();
            }

            if (!matkhau.Any(char.IsUpper))
            {
                ViewData["Loi4"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự in hoa.";
                return View();
            }

            if (!matkhau.Any(char.IsLower))
            {
                ViewData["Loi4"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự thường.";
                return View();
            }

            if (!matkhau.Any(char.IsDigit))
            {
                ViewData["Loi4"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự số.";
                return View();
            }

            if (!matkhau.Any(c => !char.IsLetterOrDigit(c)))
            {
                ViewData["Loi4"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự đặc biệt.";
                return View();
            }

            // Kiểm tra sự tồn tại của tên đăng nhập
            if (db.TaiKhoans.Any(t => t.TenTK == tendn))
            {
                ViewData["Loi2"] = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            // Kiểm tra sự tồn tại của email
            if (db.UngViens.Any(t => t.Email_TKUV == email))
            {
                ViewData["Loi3"] = "Email đã tồn tại!";
                return View();
            }

            // Kiểm tra sự tồn tại của số điện thoại
            if (db.UngViens.Any(t => t.SoDienThoai_TKUV == sdt))
            {
                ViewData["Loi5"] = "Số điện thoại đã tồn tại!";
                return View();
            }

            // Kiểm tra số điện thoại có đúng 10 chữ số
            if (sdt.Length != 10)
            {
                ViewData["Loi5"] = "Số điện thoại phải có đúng 10 chữ số!";
                return View();
            }

            ac.TenTK = tendn;
            ac.MatKhau = HashPassword(matkhau);


            db.TaiKhoans.InsertOnSubmit(ac);
            db.SubmitChanges();


            kh.TenTK = ac.TenTK;
            kh.HoTen_TKUV = hoten;
            kh.Email_TKUV = email;
            kh.HinhAnhTKUV = null;
            kh.SoDienThoai_TKUV = sdt;


            db.UngViens.InsertOnSubmit(kh);
            db.SubmitChanges();

            ViewBag.TB = "Đăng ký thành công!";
            return RedirectToAction("Login", "Auth");
        }



        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            // Khai báo các biến nhận giá trị từ form f
            var tendn = f["TenDN"];
            var matkhau = f["MatKhau"];
            ViewBag.TenDN = tendn;


            if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Tên đăng nhập không được bỏ trống!";
            }
            if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được bỏ trống!";
            }

            if (!string.IsNullOrEmpty(tendn) && !string.IsNullOrEmpty(matkhau))
            {
                TaiKhoan ac = db.TaiKhoans.SingleOrDefault(t => t.TenTK == tendn);

                if (ac != null)
                {
                    // Mã hóa mật khẩu nhập vào và so sánh với mật khẩu đã lưu trong cơ sở dữ liệu
                    string hashedPasswordInput = HashPassword(matkhau);

                    if (hashedPasswordInput == ac.MatKhau)
                    {
                        UngVien kh = db.UngViens.SingleOrDefault(t => t.TenTK == tendn);
                        NhanVien nv = db.NhanViens.SingleOrDefault(t => t.TenTK == tendn);

                        if (nv != null && ac.TenTK == nv.TenTK)
                        {
                            ViewBag.TB = "Đăng nhập thành công!";
                            Session["NV"] = nv;
                            return RedirectToAction("Index", "Admin", "Admin");
                        }

                        if (kh != null && ac.TenTK == kh.TenTK)
                        {
                            ViewBag.TB = "Đăng nhập thành công!";
                            Session["KH"] = kh;
                            return RedirectToAction("Index", "Home");
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Loi5"] = "Mật khẩu sai, vui lòng nhập lại!";
                    }
                }
                else
                {
                    ViewData["Loi5"] = "Tên đăng nhập hoặc mật khẩu sai, vui lòng nhập lại!";
                }
            }


            return View();
        }
        #endregion



        #region Auth Employer
        [HttpGet]
        public ActionResult Employer_Register()
        {
            ViewBag.DanhSachThanhPho = danhSachThanhPho;
            return View();
        }

        [HttpPost]
        public ActionResult Employer_Register(NhaTuyenDung ntd, TaiKhoan ac, DoanhNghiep dn, FormCollection f)
        {
            var hoten = f["HotenKH"];
            var tendn = f["TenDN"];
            var email = f["email"];
            var matkhau = f["MatKhau"];
            var rematkhau = f["ReMatkhau"];
            var sothue = f["Sothue"];
            var congty = f["Congty"];
            var sdt = f["SDT"];
            var tp = f["tp"];
            

            // Lưu trữ dữ liệu đã nhập vào ViewBag để hiển thị lại trong input nếu có lỗi
            ViewBag.HotenKH = hoten;
            ViewBag.TenDN = tendn;
            ViewBag.Email = email;
            ViewBag.Congty = congty;
            ViewBag.SDT = sdt;
            ViewBag.SDT = sothue;


            if (string.IsNullOrEmpty(hoten) ||
               string.IsNullOrEmpty(tendn) ||
               string.IsNullOrEmpty(matkhau) ||
               string.IsNullOrEmpty(rematkhau) ||
               string.IsNullOrEmpty(email) ||
               string.IsNullOrEmpty(congty) ||
               string.IsNullOrEmpty(sdt) ||
               string.IsNullOrEmpty(tp))
            {
                ViewData["Loino"] = "Vui lòng điền đầy đủ thông tin!";
                return View();
            }


            if (matkhau != rematkhau)
            {
                ViewData["Loimatkhaunhaplai"] = "Mật khẩu nhập lại không khớp!";
                return View();
            }


            // Kiểm tra sự tồn tại của tên đăng nhập
            if (db.TaiKhoans.Any(t => t.TenTK == tendn))
            {
                ViewData["Loitendangnhap"] = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            // Kiểm tra sự tồn tại của email
            if (db.UngViens.Any(t => t.Email_TKUV == email))
            {
                ViewData["Loiemail"] = "Email đã tồn tại!";
                return View();
            }
            // Kiểm tra mật khẩu hợp lệ
            if (string.IsNullOrEmpty(matkhau) || matkhau.Length < 8)
            {
                ViewData["Loimk"] = "Mật khẩu không hợp lệ! Mật khẩu phải lớn hơn 8 ký tự.";
                return View();
            }

            if (!matkhau.Any(char.IsUpper))
            {
                ViewData["Loimk"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự in hoa.";
                return View();
            }

            if (!matkhau.Any(char.IsLower))
            {
                ViewData["Loimk"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự thường.";
                return View();
            }

            if (!matkhau.Any(char.IsDigit))
            {
                ViewData["Loimk"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự số.";
                return View();
            }

            if (!matkhau.Any(c => !char.IsLetterOrDigit(c)))
            {
                ViewData["Loimk"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự đặc biệt.";
                return View();
            }
            // Kiểm tra giá trị của tp có null hoặc rỗng không
            if (string.IsNullOrEmpty(tp))
            {
                ViewData["Loithanhpho"] = "Vui lòng chọn thành phố!";
                return View();
            }

            // Kktra sdt
            if (sdt.Length != 10 || !sdt.All(char.IsDigit))
            {
                ViewData["Loisdt"] = "Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số.";
                return View();
            }

            if (sothue.Length != 10 || !sothue.All(char.IsDigit))
            {
                ViewData["Loithue"] = "Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số.";
                return View();
            }

            ac.TenTK = tendn;
            ac.MatKhau = HashPassword(matkhau);


            db.TaiKhoans.InsertOnSubmit(ac);
            db.SubmitChanges();

            dn.Ten_DN = congty;
            dn.MaSoThue_DN = sothue;
            dn.DiaDiem_DN = tp;
            db.DoanhNghieps.InsertOnSubmit(dn);
            db.SubmitChanges();

            ntd.TenTK = ac.TenTK;
            ntd.HoTen_NTD = hoten;
            ntd.Email_NTD = email;
            ntd.SoDienThoai_NTD = sdt;

            db.NhaTuyenDungs.InsertOnSubmit(ntd);
            db.SubmitChanges();

            ViewBag.TB = "Đăng ký thành công!";
            return RedirectToAction("Login_employer", "Auth");
        }


        public ActionResult Login_employer()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login_Employer(FormCollection f)
        {
            // Khai báo các biến nhận giá trị từ form f
            var tendn = f["TenDN"];
            var matkhau = f["MatKhau"];
            ViewBag.TenDN = tendn;


            if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Tên đăng nhập không được bỏ trống!";
            }
            if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được bỏ trống!";
            }

            if (!string.IsNullOrEmpty(tendn) && !string.IsNullOrEmpty(matkhau))
            {
                TaiKhoan ac = db.TaiKhoans.SingleOrDefault(t => t.TenTK == tendn);

                if (ac != null)
                {
                    // Mã hóa mật khẩu nhập vào và so sánh với mật khẩu đã lưu trong cơ sở dữ liệu
                    string hashedPasswordInput = HashPassword(matkhau);

                    if (hashedPasswordInput == ac.MatKhau)
                    {
                        NhaTuyenDung ntd = db.NhaTuyenDungs.SingleOrDefault(t => t.TenTK == tendn);


                        if (ntd != null && ac.TenTK == ntd.TenTK)
                        {
                            ViewBag.TB = "Đăng nhập thành công!";
                            Session["NTD"] = ntd;
                            return RedirectToAction("Index", "Employer");
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["Loipass"] = "Mật khẩu sai, vui lòng nhập lại!";
                    }
                }
                else
                {
                    ViewData["Loipass"] = "Tên đăng nhập hoặc mật khẩu sai, vui lòng nhập lại!";
                }
            }


            return View();
        }
        #endregion


        #region forget password
        public static bool SendResetPasswordEmail(string email, string tentk)
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
                    Subject = "Cập nhật lại mật khẩu",
                    Body = $@"<html>
                        <head>
                            <!-- Bootstrap CSS -->
                            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css' rel='stylesheet'>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='content'>
                                    <p class='title'><strong>Kính gửi! {tentk},</strong></p>
                                    <p>Vui lòng nhấn vào nút bên dưới để cập nhật lại mật khẩu của bạn:</p>
                                    <!-- Sử dụng thẻ <a> với các thuộc tính href và style để tạo nút màu xanh lá cây -->
                                    <a href='https://localhost:44354/Auth/UpdatePassword' style='display: inline-block; background-color: #28a745; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Cập nhật mật khẩu</a>
                                </div>
                            </div>
                        </body>
                    </html>",
                    IsBodyHtml = true,
                };



                mailMessage.To.Add(email);

                client.Send(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }

        }

        [HttpGet]
        public ActionResult ForgetPassword(string tentk)
        {
            // Kiểm tra xem tên tài khoản có tồn tại trong cơ sở dữ liệu không
            var user = db.TaiKhoans.FirstOrDefault(u => u.TenTK == tentk);
            if (user != null)
            {
                string email = db.NhanViens.Where(t => t.TenTK == tentk).Select(o => o.Email_NV).FirstOrDefault();
                if (email == null)
                {
                    email = db.UngViens.Where(t => t.TenTK == tentk).Select(o => o.Email_TKUV).FirstOrDefault();
                    if (email == null)
                    {
                        email = db.NhaTuyenDungs.Where(t => t.TenTK == tentk).Select(o => o.Email_NTD).FirstOrDefault();
                    }
                }

                // Gửi email reset mật khẩu
                bool isEmailSent = SendResetPasswordEmail(email, tentk);
                if (!isEmailSent)
                {
                    TempData["ErrorEmailMessage"] = "Gửi email thất bại";
                    return RedirectToAction("Login", "Auth");
                }

                TempData["SuccessMessage"] = "Một email đặt lại mật khẩu đã được gửi đến địa chỉ email của bạn. Vui lòng kiểm tra kỹ hòm thư cũng như hòm thư rác";
            }
            else
            {
                // Nếu không tìm thấy tên tài khoản trong cơ sở dữ liệu, bạn có thể hiển thị thông báo lỗi cho người dùng
                TempData["ErrorMessage"] = "Tên tài khoản không tồn tại trong hệ thống.";
            }
            return RedirectToAction("Login", "Auth");
        }




        public ActionResult UpdatePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdatePassword(FormCollection f)
        {
            var tendn = f["TenDN"];
            var matkhau = f["MatKhau"];
            var rematkhau = f["ReMatkhau"];
            ViewBag.TenDN = tendn;

            var user = db.TaiKhoans.FirstOrDefault(u => u.TenTK == tendn);


            if (user != null)
            {

                if (string.IsNullOrEmpty(matkhau) || matkhau.Length < 8)
                {
                    ViewData["Loi2"] = "Mật khẩu không hợp lệ! Mật khẩu phải lớn hơn 8 ký tự.";
                    return View();
                }

                if (!matkhau.Any(char.IsUpper))
                {
                    ViewData["Loi2"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự in hoa.";
                    return View();
                }

                if (!matkhau.Any(char.IsLower))
                {
                    ViewData["Loi2"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự thường.";
                    return View();
                }

                if (!matkhau.Any(char.IsDigit))
                {
                    ViewData["Loi2"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự số.";
                    return View();
                }

                if (!matkhau.Any(c => !char.IsLetterOrDigit(c)))
                {
                    ViewData["Loi2"] = "Mật khẩu không hợp lệ! Mật khẩu phải chứa ít nhất một ký tự đặc biệt.";
                    return View();
                }
                if (matkhau != rematkhau)
                {
                    ViewData["Loi3"] = "Mật khẩu nhập lại không khớp!";
                    return View();
                }
                user.MatKhau = HashPassword(matkhau);
                db.SubmitChanges();

                ViewBag.SuccessUpdatePass = "Succces !";
                return View();

            }
            else
            {
                ViewData["Loi1"] = "Tên tài khoản không có trong hệ thống";
                return View();

            }

        }
    }
    #endregion
}