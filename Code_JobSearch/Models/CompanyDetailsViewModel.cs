using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class CompanyDetailsViewModel
    {
        public NhaTuyenDung NhaTuyenDung { get; set; }
        public DoanhNghiep DoanhNghiep { get; set; }
        public List<TinTuyenDung> TinTuyenDung { get; set; }
    }
}