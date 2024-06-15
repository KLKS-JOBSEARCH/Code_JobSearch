using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class UngtuyenViewmodel
    {
        public TinTuyenDung TinTuyenDung { get; set; }
        public UngVien UngVien { get; set; }
        public List<HoSoXinViec> HoSoXinViecList { get; set; }
        public string NoiDung_ThuGioiThieu { get; set; }
        public HttpPostedFileBase File_CV { get; set; }
        public bool UseHoSoXinViec { get; set; }
        public int? SelectedHoSoId { get; set; } // Id của hồ sơ xin việc được chọn

        // Các trường cho khi người dùng tải lên CV mới
        public string DiaChiUV { get; set; } // Địa chỉ
        public string HocVan { get; set; } // Học vấn
        public string KyNang { get; set; } // Kỹ năng
        public string DuAnThamGia { get; set; } // Dự án tham gia
        public string MoTaBanThan { get; set; } // Mô tả bản thân
    }


}