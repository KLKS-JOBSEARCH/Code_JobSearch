using Code_JobSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Code_JobSearch.Controllers
{
    public class UserController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        // GET: User


        #region CV loader
        public ActionResult CvUser()
        {
            if (Session["KH"] != null)
            {
                // Lấy Id_UV từ phiên làm việc
                UngVien kh = Session["KH"] as UngVien;

                // Truy vấn thông tin kết hợp từ bảng ungvien và hoso
                var viewModel = (from uv in db.UngViens
                                 where uv.Id_UV == kh.Id_UV
                                 select new CvUserModel
                                 {
                                     UngVien = uv,
                                     HoSoXinViec = (from hs in db.HoSoXinViecs
                                                     where hs.Id_UV == uv.Id_UV
                                                     select hs).ToList()
                                 }).SingleOrDefault();

                if (viewModel == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Title = "Thông tin hồ sơ của bạn";
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }
        #endregion


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(HoSoXinViec model)
        {
            if (ModelState.IsValid)
            {
                db.HoSoXinViecs.InsertOnSubmit(model);
                db.SubmitChanges();
                return RedirectToAction("CvUser", "User");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var hoSo = db.HoSoXinViecs.SingleOrDefault(h => h.Id_HSXV == id);
            if (hoSo == null)
            {
                return HttpNotFound();
            }
            return View(hoSo);
        }

        [HttpPost]
        public ActionResult Edit(HoSoXinViec model, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                // Lấy ngày và giờ hiện tại
                DateTime now = DateTime.Now;

                // Lấy hồ sơ hiện tại từ cơ sở dữ liệu
                var existingHoSo = db.HoSoXinViecs.SingleOrDefault(h => h.Id_HSXV == model.Id_HSXV);

                if (existingHoSo != null)
                {
                    // Chỉ cập nhật các trường quan trọng
                    existingHoSo.Ten_HSXV = model.Ten_HSXV;
                    existingHoSo.NoiDung_HSXV = model.NoiDung_HSXV;

                    // Cập nhật trường TGCN_HSXV thành ngày giờ hiện tại
                    existingHoSo.TGCN_HSXV = now;

                    // Xử lý upload file nếu có
                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        // Kiểm tra loại file
                        if (Path.GetExtension(uploadFile.FileName).ToLower() == ".pdf")
                        {
                            // Kiểm tra kích thước của file (giới hạn dưới 1MB)
                            if (uploadFile.ContentLength <= 1024 * 1024) // 1MB = 1024 * 1024 bytes
                            {
                                var fileName = Path.GetFileName(uploadFile.FileName);
                                var filePath = Path.Combine(Server.MapPath("~/Content/CvUser"), fileName);

                                // Xóa file cũ nếu tồn tại
                                if (!string.IsNullOrEmpty(existingHoSo.File_HSXV))
                                {
                                    var oldFilePath = Path.Combine(Server.MapPath("~/Content/CvUser"), existingHoSo.File_HSXV);
                                    if (System.IO.File.Exists(oldFilePath))
                                    {
                                        System.IO.File.Delete(oldFilePath);
                                    }
                                }

                                // Lưu file mới vào thư mục
                                uploadFile.SaveAs(filePath);

                                // Cập nhật tên file vào cơ sở dữ liệu
                                existingHoSo.File_HSXV = fileName;
                            }
                            else
                            {
                                ModelState.AddModelError("", "Kích thước tệp PDF phải nhỏ hơn hoặc bằng 1MB.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Vui lòng chọn một tệp PDF.");
                            return View(model);
                        }
                    }

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    db.SubmitChanges();

                    return RedirectToAction("CvUser", "User");
                }
                else
                {
                    return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy hồ sơ
                }
            }
            return View(model);
        }




        public ActionResult Delete(int id)
        {
            var hoSo = db.HoSoXinViecs.SingleOrDefault(h => h.Id_HSXV == id);
            if (hoSo == null)
            {
                return HttpNotFound();
            }
            db.HoSoXinViecs.DeleteOnSubmit(hoSo);
            db.SubmitChanges();
            return RedirectToAction("CvUser", "User");
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
            UngVien kh = Session["KH"] as UngVien;
            if (kh != null)
            {
                gy.Id_UV = kh.Id_UV;

                gy.TieuDe_GY = tieude;
                gy.NoiDung_GY = noidung;
                gy.MucDoHaiLong = Convert.ToInt16(mucdohailong); // Chuyển đổi giá trị string thành smallint
                gy.NgayGui_GY = DateTime.Now;

                db.Gopies.InsertOnSubmit(gy);
                db.SubmitChanges();

                ViewBag.TB = "thành công!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Xử lý khi không có phiên đăng nhập
                ViewBag.TB = "Đăng nhập trước khi gửi góp ý!";
                return RedirectToAction("Login", "Auth");
            }
        }
        #endregion

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

    }
}