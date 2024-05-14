using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Code_JobSearch.Models
{
    public class CvUserModel
    {
        public UngVien UngVien { get; set; }
        public List<HoSoXinViec> HoSoXinViec { get; set; }
    }
}