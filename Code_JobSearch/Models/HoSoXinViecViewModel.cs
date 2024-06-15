using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class HoSoXinViecViewModel
    {
        public string Ten_HSXV { get; set; }
        public DateTime? TGCN_HSXV { get; set; }
        public string NoiDung_HSXV { get; set; }
        public string File_HSXV { get; set; }

        public int Id_UV { get; set; }
        public string HoTen_TKUV { get; set; }
        public string Email_TKUV { get; set; }
        public string SoDienThoai_TKUV { get; set; }
        public string DiaChiUV { get; set; }
        public string HocVan { get; set; }
        public string KyNang { get; set; }
        public string DuAnThamGia { get; set; }
        public string MoTaBanThan { get; set; }
    }

}