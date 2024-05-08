using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class JobDetailsViewModel
    {
        public TinTuyenDung TinTuyenDung { get; set; }
        public DoanhNghiep DoanhNghiep { get; set; }
        public NhaTuyenDung NhaTuyenDung { get; set; }
    }
}