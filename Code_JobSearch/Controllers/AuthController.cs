using Code_JobSearch.Models;
using FluentEmail.Core;
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
        //code update
        private List<string> danhSachViTriCongTac = new List<string>
        {
            "Nhân viên", "Phó phòng","Trưởng phòng", "Phó giám đốc", "Giám đốc", "Tổng giám đốc"
        };
        //---------------

        #region Auth User
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection f)
        {
            var hoten = f["HotenKH"];
            var tendn = f["TenDN"];
            var matkhau = f["MatKhau"];
            var rematkhau = f["ReMatkhau"];
            var email = f["email"];
            var sdt = f["sdt"];
            var validPrefixes = new List<string> { "09", "03", "07", "08", "05" };

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
                TempData["ErrorMessageTDNUV"] = "Tên đăng nhập đã tồn tại, trả về trang đăng nhập?";
                return View();
            }

            // Kiểm tra sự tồn tại của email
            if (db.UngViens.Any(t => t.Email_TKUV == email))
            {
                ViewData["Loi3"] = "Email đã tồn tại!";
                return View();
            }



            // Kiểm tra định dạng số điện thoại
            if (!validPrefixes.Any(prefix => sdt.StartsWith(prefix)))
            {
                ViewData["Loi5"] = "Số điện thoại không hợp lệ";
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
                ViewData["Loi5"] = "Số điện thoại không hợp lệ";
                return View();
            }
            //code update

            bool isEmailSent = VerifyEmail(email);
            if (!isEmailSent)
            {
                TempData["ErrorEmailMessageRegister"] = "Gửi mã xác thực đăng ký thất bại. Có thể do email không đúng!";
                return View();
            }
            TempData["SuccessMessageRegister"] = "Bạn hãy vui lòng kiểm tra email để nhận mã xác thực đăng ký.";

            TempData["UngVien"] = new UngVien
            {
                HoTen_TKUV = hoten,
                Email_TKUV = email,
                SoDienThoai_TKUV = sdt,
                TenTK = tendn
            };

            TempData["TaiKhoan"] = new TaiKhoan
            {
                TenTK = tendn,
                MatKhau = HashPassword(matkhau)
            };
            return RedirectToAction("RegisterCodeVerify");
        }
        public ActionResult RegisterCodeVerify()
        {

            return View();
        }
        [HttpPost]
        public ActionResult RegisterCodeVerify(FormCollection f)
        {
            var maxacthuc = f["MaXacThuc"];
            if (!VerificationCodeManager.IsCodeValid(maxacthuc))
            {
                ViewData["LoiMaXacThuc"] = "Mã xác thực không đúng hoặc đã hết hạn";
                return View();
            }
            var uv = TempData["UngVien"] as UngVien;
            var ac = TempData["TaiKhoan"] as TaiKhoan;
            db.TaiKhoans.InsertOnSubmit(ac);
            db.SubmitChanges();
            db.UngViens.InsertOnSubmit(uv);
            db.SubmitChanges();
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
                ViewData["Loidangnhap"] = "Tên đăng nhập không được bỏ trống!";
            }
            if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loidangnhap"] = "Mật khẩu không được bỏ trống!";
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
                        // Kiểm tra nếu tài khoản là ứng viên
                        UngVien kh = db.UngViens.SingleOrDefault(t => t.TenTK == tendn);
                        if (kh != null)
                        {
                            ViewBag.TB = "Đăng nhập thành công!";
                            Session["KH"] = kh;
                            return RedirectToAction("Index", "Home");
                        }

                        // Kiểm tra nếu tài khoản là admin
                        TaiKhoan ad = db.TaiKhoans.SingleOrDefault(t => t.TenTK == tendn && t.TenTK == "admin");
                        if (ad != null)
                        {
                            ViewBag.TB = "Đăng nhập thành công!";
                            Session["AD"] = ad;
                            return RedirectToAction("Index", "Admin", new { area = "" });
                        }

                        // Nếu không khớp với bất kỳ loại tài khoản nào
                        ViewData["Loisaimatkhau"] = "Tài khoản không thuộc loại hợp lệ!";
                    }
                    else
                    {
                        ViewData["Loisaimatkhau"] = "Mật khẩu sai, vui lòng nhập lại!";
                    }
                }
                else
                {
                    ViewData["Loisaimatkhau"] = "Tên đăng nhập hoặc mật khẩu sai, vui lòng nhập lại!";
                }
            }

            return View();
        }

        #endregion



        #region Auth DoanhNghiep
        [HttpGet]
        public ActionResult DoanhNghiep_Register()
        {
            ViewBag.DanhSachThanhPho = danhSachThanhPho;
            ViewBag.DanhSachViTriCT = danhSachViTriCongTac;
            return View();
        }

        [HttpPost]
        public ActionResult DoanhNghiep_Register(FormCollection f)
        {
            ViewBag.DanhSachThanhPho = danhSachThanhPho;
            ViewBag.DanhSachViTriCT = danhSachViTriCongTac;
            var hoten = f["HotenKH"];
            var tendn = f["TenDN"];
            var email = f["email"];
            var matkhau = f["MatKhau"];
            var rematkhau = f["ReMatkhau"];
            var sothue = f["Sothue"];
            var congty = f["Congty"];
            var sdt = f["SDT"];
            var tp = f["tp"];
            var vtct = f["vtct"];


            // Lưu trữ dữ liệu đã nhập vào ViewBag để hiển thị lại trong input nếu có lỗi
            ViewBag.HotenKH = hoten;
            ViewBag.TenDN = tendn;
            ViewBag.Email = email;
            ViewBag.Congty = congty;
            ViewBag.SDT = sdt;
            ViewBag.SoThue = sothue;


            if (string.IsNullOrEmpty(hoten) ||
               string.IsNullOrEmpty(matkhau) ||
               string.IsNullOrEmpty(rematkhau) ||
               string.IsNullOrEmpty(email) ||
               string.IsNullOrEmpty(congty) ||
               string.IsNullOrEmpty(sdt) ||
               string.IsNullOrEmpty(tp) ||
            string.IsNullOrEmpty(vtct))
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
                TempData["ErrorMessageTDNNTD"] = "Tên đăng nhập đã tồn tại, trả về trang đăng nhập?";
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
            if (string.IsNullOrEmpty(vtct))
            {
                ViewData["Loivitricongtac"] = "Vui lòng chọn vị trí công tác!";
                return View();
            }

            // Kktra sdt
            if (sdt.Length != 10 || !sdt.All(char.IsDigit))
            {
                ViewData["Loisdt"] = "Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số.";
                return View();
            }
            if (db.NhaTuyenDungs.Any(t => t.SoDienThoai_NTD == sdt))
            {
                ViewData["Loisdt"] = "Số điện thoại đã tồn tại.";
                return View();
            }
            if (sothue.Length != 10 || !sothue.All(char.IsDigit))
            {
                ViewData["Loithue"] = "Mã số thuế! Vui lòng nhập đúng 10 số.";
                return View();
            }
            if (db.DoanhNghieps.Any(t => t.MaSoThue_DN == sothue))
            {
                ViewData["Loithue"] = "Mã số thuế đã tồn tại!";
                return View();
            }
            bool isEmailSent = VerifyEmail(email);
            if (!isEmailSent)
            {
                TempData["ErrorEmailMessageRegister"] = "Gửi mã xác thực đăng ký thất bại. Có thể do email không đúng!";
                return View();
            }
            TempData["SuccessMessageRegister"] = "Bạn hãy vui lòng kiểm tra email để nhận mã xác thực đăng ký.";

            TempData["DoanhNghiep"] = new DoanhNghiep
            {
                Ten_DN = tendn,
                DiaDiem_DN = tp,
                MaSoThue_DN = sothue
            };
            TempData["NhaTuyenDung"] = new NhaTuyenDung
            {
                TenTK = tendn,
                HoTen_NTD = hoten,
                Email_NTD = email,
                SoDienThoai_NTD = sdt,
                ViTriCongTac = vtct,
            };
            TempData["TaiKhoanNTD"] = new TaiKhoan
            {
                TenTK = tendn,
                MatKhau = HashPassword(matkhau)
            };

            return RedirectToAction("DoanhNghiep_RegisterCodeVerify");
        }

        public ActionResult DoanhNghiep_RegisterCodeVerify()
        {

            return View();
        }
        [HttpPost]
        public ActionResult DoanhNghiep_RegisterCodeVerify(FormCollection f)
        {
            var maxacthuc = f["MaXacThuc"];
            if (!VerificationCodeManager.IsCodeValid(maxacthuc))
            {
                ViewData["LoiMaXacThuc"] = "Mã xác thực không đúng hoặc đã hết hạn";
                return View();
            }
            var ac = TempData["TaiKhoanNTD"] as TaiKhoan;
            var ntd = TempData["NhaTuyenDung"] as NhaTuyenDung;
            var dn = TempData["DoanhNghiep"] as DoanhNghiep;
            db.TaiKhoans.InsertOnSubmit(ac);
            db.SubmitChanges();
            db.DoanhNghieps.InsertOnSubmit(dn);
            db.SubmitChanges();
            db.NhaTuyenDungs.InsertOnSubmit(ntd);
            db.SubmitChanges();
            var iddn = db.DoanhNghieps.OrderByDescending(d => d.Id_DN).Select(d => d.Id_DN).FirstOrDefault();
            var ntdupdate = db.NhaTuyenDungs.SingleOrDefault(o => o.Id_NTD.Equals(ntd.Id_NTD));
            ntdupdate.Id_DN = iddn;
            db.SubmitChanges();
            return RedirectToAction("Login_DoanhNghiep", "Auth");
        }
        public ActionResult Login_DoanhNghiep()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login_DoanhNghiep(FormCollection f)
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
                            return RedirectToAction("Index", "DoanhNghiep");
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
        //code update
        public class CodeVerify
        {
            public string Code { get; set; }
            public DateTime time { get; set; }
        }
        public static class VerificationCodeManager
        {
            private static readonly List<CodeVerify> ListCodeVerify = new List<CodeVerify>();
            public static string CodeVerify { get; private set; }
            public static DateTime ExpirationTime { get; private set; }

            public static void GenerateNewCode()
            {
                string newCode;
                do
                {
                    newCode = GenerateVerificationCode();
                }
                while (IsCodeExists(newCode));

                CodeVerify = newCode;
                ExpirationTime = DateTime.Now.AddMinutes(2); // Set expiration time to 2 minutes
            }
            private static bool IsCodeExists(string code)
            {
                return ListCodeVerify.Any(cv => cv.Code == code);
            }

            private static string GenerateVerificationCode()
            {
                const int codeLength = 6;
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
                var random = new Random();
                var code = new char[codeLength];

                for (int i = 0; i < codeLength; i++)
                {
                    code[i] = chars[random.Next(chars.Length)];
                }

                return new string(code);
            }

            public static void AddCodeToList(CodeVerify codeVerify)
            {
                ListCodeVerify.Add(codeVerify);
            }

            public static bool IsCodeValid(string code)
            {
                var codeVerify = ListCodeVerify.FirstOrDefault(cv => cv.Code == code);
                if (codeVerify != null && DateTime.Now <= codeVerify.time)
                {
                    return true;
                }
                return false;
            }

            public static void CleanupExpiredCodes()
            {
                ListCodeVerify.RemoveAll(cv => DateTime.Now > cv.time);
            }
        }


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

                VerificationCodeManager.GenerateNewCode();
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
                                    <p>Dưới đây là mã xác thực để cập nhật lại mật khẩu chỉ có hạn 2 phút:</p>
                                    <p><strong>{VerificationCodeManager.CodeVerify}</strong></p>
                                   
                                </div>
                            </div>
                        </body>
                    </html>",
                    IsBodyHtml = true,
                };



                mailMessage.To.Add(email);

                client.Send(mailMessage);
                var codeVerify = new CodeVerify
                {
                    Code = VerificationCodeManager.CodeVerify,
                    time = VerificationCodeManager.ExpirationTime
                };
                VerificationCodeManager.AddCodeToList(codeVerify);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public static bool VerifyEmail(string email)
        {

            try
            {
                var client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("jobstarvn@gmail.com", "jxtt juxf gbqy nbdp"),
                    EnableSsl = true,
                };
                VerificationCodeManager.GenerateNewCode();
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("jobstarvn@gmail.com"),
                    Subject = "Xác thực đăng ký",
                    Body = $@"<html>
                        <head>
                            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css' rel='stylesheet'>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='content'>
                                    <p class='title'><strong>Kính gửi! {email},</strong></p>
                                    <p>Dưới đây là mã xác thực để đăng ký chỉ có hạn 2 phút:</p>
                                    <p><strong>{VerificationCodeManager.CodeVerify}</strong></p>
                                   
                                </div>
                            </div>
                        </body>
                    </html>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);
                client.Send(mailMessage);
                var codeVerify = new CodeVerify
                {
                    Code = VerificationCodeManager.CodeVerify,
                    time = VerificationCodeManager.ExpirationTime
                };
                VerificationCodeManager.AddCodeToList(codeVerify);
                return true;
            }
            catch
            {
                return false;
            }

        }
        [HttpGet]
        public ActionResult ForgetPassword_DoanhNghiep(string tentk)
        {
            var user = db.TaiKhoans.FirstOrDefault(u => u.TenTK == tentk);
            if (user != null)
            {
                string email = db.NhaTuyenDungs.Where(t => t.TenTK == tentk).Select(u => u.Email_NTD).FirstOrDefault();
                bool isEmailSent1 = SendResetPasswordEmail(email, tentk);
                if (!isEmailSent1)
                {
                    TempData["ErrorEmailMessage"] = "Gửi email thất bại";
                    return RedirectToAction("Login_DoanhNghiep", "Auth");
                }
                TempData["SuccessMessage"] = "Bạn hãy vui lòng kiểm tra email để nhận mã xác thực đổi mật khẩu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Tên tài khoản không tồn tại trong hệ thống.";
            }
            return RedirectToAction("Login_DoanhNghiep", "Auth");
        }
        [HttpGet]
        public ActionResult ForgetPassword(string tentk)
        {
            // Kiểm tra xem tên tài khoản có tồn tại trong cơ sở dữ liệu không
            var user = db.TaiKhoans.FirstOrDefault(u => u.TenTK == tentk);
            if (user != null)
            {
                string email = db.UngViens.Where(t => t.TenTK == tentk).Select(u => u.Email_TKUV).FirstOrDefault();
                bool isEmailSent = SendResetPasswordEmail(email, tentk);
                if (!isEmailSent)
                {
                    TempData["ErrorEmailMessage"] = "Gửi email thất bại";
                    return RedirectToAction("Login", "Auth");
                }
                TempData["SuccessMessage"] = "Bạn hãy vui lòng kiểm tra email để nhận mã xác thực đổi mật khẩu.";

            }
            else
            {
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
            var maxacthuc = f["MaXacThuc"];
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

                if (!VerificationCodeManager.IsCodeValid(maxacthuc))
                {
                    ViewData["Loi4"] = "Mã xác thực không đúng hoặc đã hết hạn";
                    return View();
                }
                user.MatKhau = HashPassword(matkhau);
                db.SubmitChanges();
                var ntd = db.NhaTuyenDungs.FirstOrDefault(t => t.TenTK.Equals(tendn));
                if (ntd != null)
                {
                    ViewBag.SuccessUpdatePassNTD = "Succces !";
                }
                else
                {
                    ViewBag.SuccessUpdatePass = "Succces !";
                }
                return View();

            }
            else
            {
                ViewData["Loi1"] = "Tên tài khoản không có trong hệ thống";
                return View();

            }

        }
    }
    //---------
    #endregion
}