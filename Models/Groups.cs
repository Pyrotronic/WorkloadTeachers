using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBPv1._1.Model
{
    public class Groups
    {
        [Key]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Specialty { get; set; }
        public string Department { get; set; }
        public int StudentCount { get; set; }
        public List<Workload> Workload { get; set; } = new List<Workload>();
    }
}
