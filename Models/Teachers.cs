using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBPv1._1.Model
{
    public class Teachers
    {
        [Key]
        public int TeacherID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }
        public int Experience { get; set; }
        public List<Workload> Workload {  get; set; } = new List<Workload>();
    }
}
