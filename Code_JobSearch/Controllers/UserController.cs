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

        #region CREATE, EDIT, DELETE, DOWNLOAD CV
        public ActionResult Create()
        {
            if (Session["KH"] != null)
            {
                // Lấy Id_UV 
                UngVien kh = Session["KH"] as UngVien;

                var ungVien = db.UngViens.SingleOrDefault(uv => uv.Id_UV == kh.Id_UV);

                if (ungVien == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new HoSoXinViecViewModel
                {
                    Id_UV = ungVien.Id_UV,
                    HoTen_TKUV = ungVien.HoTen_TKUV,
                    Email_TKUV = ungVien.Email_TKUV,
                    SoDienThoai_TKUV = ungVien.SoDienThoai_TKUV,
                };

                return View(viewModel);
            }

            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu session không tồn tại
        }


        [HttpPost]
        public ActionResult Create(HoSoXinViecViewModel model, HttpPostedFileBase uploadFile)
        {
            if (Session["KH"] != null)
            {
                if (ModelState.IsValid)
                {
                    var ungVien = Session["KH"] as UngVien;

                    var hoSoXinViec = new HoSoXinViec
                    {
                        Ten_HSXV = string.IsNullOrEmpty(model.Ten_HSXV) ? "Chưa đặt tiêu đề" : model.Ten_HSXV,
                        TGCN_HSXV = DateTime.Now,
                        Id_UV = ungVien.Id_UV,
                        HoTenUV = ungVien.HoTen_TKUV,
                        EmailUV = ungVien.Email_TKUV,
                        SoDienThoaiUV = ungVien.SoDienThoai_TKUV,
                        DiaChiUV = model.DiaChiUV,
                        HocVan = model.HocVan,
                        KyNang = model.KyNang,
                        DuAnThamGia = model.DuAnThamGia,
                        MoTaBanThan = model.MoTaBanThan,
                    };

                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        if (Path.GetExtension(uploadFile.FileName).ToLower() == ".pdf" && uploadFile.ContentLength <= 5 * 1024 * 1024)
                        {
                            var fileName = Path.GetFileName(uploadFile.FileName);
                            var filePath = Path.Combine(Server.MapPath("~/Content/CvUser"), fileName);
                            uploadFile.SaveAs(filePath);
                            hoSoXinViec.File_HSXV = fileName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Vui lòng chọn một tệp PDF có kích thước nhỏ hơn hoặc bằng 5MB.");
                            return View(model);
                        }
                    }
                    else // Nếu không có tệp được chọn
                    {
                        ModelState.AddModelError("", "Vui lòng chọn một tệp để tải lên.");
                        return View(model);
                    }

                    db.HoSoXinViecs.InsertOnSubmit(hoSoXinViec);
                    db.SubmitChanges();
                    return RedirectToAction("CvUser", "User");
                }

                return View(model);
            }

            return RedirectToAction("Login", "Auth"); // Chuyển hướng đến trang đăng nhập nếu session không tồn tại
        }



        public ActionResult Edit(int id)
        {
            if (Session["KH"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            TaiKhoan ac = Session["KH"] as TaiKhoan;
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
                DateTime now = DateTime.Now;
                var existingHoSo = db.HoSoXinViecs.SingleOrDefault(h => h.Id_HSXV == model.Id_HSXV);
                if (existingHoSo != null)
                {
                    // Cập nhật các trường nếu có sự thay đổi
                    if (existingHoSo.Ten_HSXV != model.Ten_HSXV)
                        existingHoSo.Ten_HSXV = model.Ten_HSXV;

                    if (existingHoSo.DiaChiUV != model.DiaChiUV)
                        existingHoSo.DiaChiUV = model.DiaChiUV;

                    if (existingHoSo.HocVan != model.HocVan)
                        existingHoSo.HocVan = model.HocVan;

                    if (existingHoSo.KyNang != model.KyNang)
                        existingHoSo.KyNang = model.KyNang;

                    if (existingHoSo.DuAnThamGia != model.DuAnThamGia)
                        existingHoSo.DuAnThamGia = model.DuAnThamGia;

                    if (existingHoSo.MoTaBanThan != model.MoTaBanThan)
                        existingHoSo.MoTaBanThan = model.MoTaBanThan;

                    existingHoSo.TGCN_HSXV = now;

                    // Xử lý upload file nếu có
                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        // Kiểm tra loại file
                        if (Path.GetExtension(uploadFile.FileName).ToLower() == ".pdf")
                        {
                            // Kiểm tra kích thước của file (giới hạn dưới 5MB)
                            if (uploadFile.ContentLength <= 5 * 1024 * 1024) // 5MB = 5 * 1024 * 1024 bytes
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

                                uploadFile.SaveAs(filePath);
                                existingHoSo.File_HSXV = fileName;
                            }
                            else
                            {
                                ModelState.AddModelError("", "Kích thước tệp PDF phải nhỏ hơn hoặc bằng 5MB.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Vui lòng chọn một tệp PDF.");
                            return View(model);
                        }
                    }

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

                ViewBag.TB = "Đăng nhập trước khi gửi góp ý!";
                return RedirectToAction("Login", "Auth");

            }
        }
        #endregion

        #region Ứng tuyển công việc
        public ActionResult UngtuyenCV(int? idTTD)
        {
            if (Session["KH"] == null)
            {
                return RedirectToAction("Login", "Auth"); // Chuyển hướng nếu người dùng chưa đăng nhập
            }

            if (!idTTD.HasValue)
            {
                return RedirectToAction("Error"); // Chuyển hướng nếu idTTD không có giá trị
            }

            UngVien kh = Session["KH"] as UngVien;
            TinTuyenDung tinTuyenDung = db.TinTuyenDungs.SingleOrDefault(t => t.Id_TTD == idTTD.Value);
            List<HoSoXinViec> hoSoXinViecs = db.HoSoXinViecs.Where(h => h.Id_UV == kh.Id_UV).ToList();

            UngtuyenViewmodel model = new UngtuyenViewmodel
            {
                UngVien = kh,
                TinTuyenDung = tinTuyenDung,
                HoSoXinViecList = hoSoXinViecs
            };

            return View(model);
        }

        // POST: UngtuyenCV
        [HttpPost]
        public ActionResult UngtuyenCV(UngtuyenViewmodel model, HttpPostedFileBase File_CV)
        {
            if (Session["KH"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            UngVien kh = Session["KH"] as UngVien;

            // Kiểm tra lại xem có lỗi gì không trước khi tiếp tục
            if (!ModelState.IsValid)
            {
                // Nạp lại danh sách hồ sơ xin việc vào model để hiển thị trong view
                model.HoSoXinViecList = db.HoSoXinViecs.Where(h => h.Id_UV == kh.Id_UV).ToList();
                model.UngVien = kh;

                return View(model);
            }

            // Tạo đối tượng UV_TTD để lưu vào database
            UV_TTD uv_ttd = new UV_TTD
            {
                Id_UV = kh.Id_UV,
                Id_TTD = model.TinTuyenDung.Id_TTD,
                HoTenUV_TD = kh.HoTen_TKUV,
                EmailUV_TD = kh.Email_TKUV,
                SoDienThoaiUV_TD = kh.SoDienThoai_TKUV,
                ThoiGianUngTuyen = DateTime.Now,
                TinhTrangUngTuyen = "Đang xét duyệt",
                NoiDung_ThuGioiThieu = string.IsNullOrEmpty(model.NoiDung_ThuGioiThieu) ? "Chưa có nội dung thư!" : model.NoiDung_ThuGioiThieu,
                Id_HSXV = model.SelectedHoSoId // Gán Id_HSXV từ lựa chọn của người dùng
            };

            if (model.SelectedHoSoId.HasValue && model.SelectedHoSoId > 0)
            {
                // Người dùng chọn một hồ sơ xin việc từ danh sách
                HoSoXinViec hoSo = db.HoSoXinViecs.FirstOrDefault(h => h.Id_HSXV == model.SelectedHoSoId);
                if (hoSo != null)
                {
                    // Sao chép thông tin từ HoSoXinViec sang UV_TTD
                    uv_ttd.DiaChiUV = hoSo.DiaChiUV;
                    uv_ttd.HocVan = hoSo.HocVan;
                    uv_ttd.KyNang = hoSo.KyNang;
                    uv_ttd.DuAnThamGia = hoSo.DuAnThamGia;
                    uv_ttd.MoTaBanThan = hoSo.MoTaBanThan;
                    uv_ttd.File_CV = hoSo.File_HSXV;
                }
            }
            else
            {
                if (File_CV != null && File_CV.ContentLength > 0)
                {
                    // Giới hạn kích thước tệp là 5MB
                    if (File_CV.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Kích thước tệp phải nhỏ hơn hoặc bằng 5MB.");
                        model.HoSoXinViecList = db.HoSoXinViecs.Where(h => h.Id_UV == kh.Id_UV).ToList();
                        model.UngVien = kh;
                        return View(model);
                    }

                    // Kiểm tra phần mở rộng của tệp có phải là .pdf không
                    if (Path.GetExtension(File_CV.FileName).ToLower() != ".pdf")
                    {
                        ModelState.AddModelError("", "Vui lòng chọn một tệp PDF.");
                        model.HoSoXinViecList = db.HoSoXinViecs.Where(h => h.Id_UV == kh.Id_UV).ToList();
                        model.UngVien = kh;
                        return View(model);
                    }

                    // Lưu tệp và gán tên vào đối tượng UV_TTD
                    string fileName = Path.GetFileName(File_CV.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Content/CvUser"), fileName);
                    File_CV.SaveAs(filePath);
                    uv_ttd.File_CV = fileName;

                    // Lấy các thông tin nhập từ người dùng
                    uv_ttd.DiaChiUV = model.DiaChiUV;
                    uv_ttd.HocVan = model.HocVan;
                    uv_ttd.KyNang = model.KyNang;
                    uv_ttd.DuAnThamGia = model.DuAnThamGia;
                    uv_ttd.MoTaBanThan = model.MoTaBanThan;
                }
                else // Nếu không có tệp được chọn
                {
                    ModelState.AddModelError("", "Vui lòng chọn một tệp để tải lên.");
                    model.HoSoXinViecList = db.HoSoXinViecs.Where(h => h.Id_UV == kh.Id_UV).ToList();
                    model.UngVien = kh;
                    return View(model);
                }
            }

            db.UV_TTDs.InsertOnSubmit(uv_ttd);
            db.SubmitChanges();

            return RedirectToAction("HistoryofCVApply", "User"); // Chuyển hướng đến trang thành công hoặc trang khác
        }


        #endregion

        #region Xem lịch sử công việc đã ứng tuyển
        public ActionResult HistoryofCVApply()
        {
            if (Session["KH"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            UngVien kh = Session["KH"] as UngVien;

            var history = (from uv_ttd in db.UV_TTDs
                           join ttd in db.TinTuyenDungs on uv_ttd.Id_TTD equals ttd.Id_TTD
                           where uv_ttd.Id_UV == kh.Id_UV
                           orderby uv_ttd.ThoiGianUngTuyen descending
                           group uv_ttd by ttd into g
                           select new HistoryOfCVApplyViewModel
                           {
                               TinTuyenDung = g.Key,
                               UV_TTD = g.ToList()
                           }).ToList();

            return View(history);
        }

        #endregion

    }
}