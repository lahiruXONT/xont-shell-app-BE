using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class UserTask
    {
        public string TaskCode { get; set; }
        public string Description { get; set; }
        public string ExecutionScript { get; set; }
        public string Icon { get; set; }

        public string Caption { get; set; }
        public string TaskType { get; set; }//VR019
        public string UserName { get; set; }//VR019
        public string url { get; set; }
        public string ExclusivityMode { get; set; } //V2049
        public string ApplicationCode { get; set; } //V2049

    }
}
