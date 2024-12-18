using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBPv1._1.Model
{
    public class Workload
    {
        public int WorkloadID  { get; set; }  
        public int TeacherID { get; set; }
        public Teachers teachers { get; set; }
        public int GroupID { get; set; }
        public Groups groups { get; set; }
        public int Hours { get; set; }
        public string Subject { get; set; }
        public string ClassType { get; set; }
        public decimal Payment {  get; set; }

    }
}
