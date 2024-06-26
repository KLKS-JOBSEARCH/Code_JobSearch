using Code_JobSearch.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Code_JobSearch.Models
{
    public class DanhGia_UV_DiemDG
    {
        public UngVien uv { get; set; }
        public bool TongDiem { get; set; }
    }
}