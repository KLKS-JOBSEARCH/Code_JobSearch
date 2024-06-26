using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class DanhGia_UV_NTD_TTD
    {
        public UngVien uv { get; set; }
        public NhaTuyenDung ntd { get; set; }
        public DoanhNghiep dn { get; set; }
        public TinTuyenDung ttd { get; set; }
        public DanhGia_UV dg { get; set; }
    }
}