using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class ActiveUserTask
    {
        public string UserName { set; get; } //[char](30) ='',
        public string BusinessUnit { set; get; } //[char](4) ='',
        public string SessionID { set; get; } //[varchar](50) ='',
        public string TaskCode { set; get; } //[char](10) ='',
        public DateTime StartDateTime { set; get; } //[datetime] =getdate,
        public DateTime? EndDateTime { set; get; } //[datetime] =NULL,
        public string Status { set; get; } //[char](1)='',
        public string ExecutionType { set; get; } //char(1) = '0'
        public string ExclusivityMode { set; get; } //char(1) = '0' //V2049
        public string PowerUser { set; get; } //[char](30) ='', //V2049
        public string StatusFlag { set; get; } //[char](1)='', //V2049
        public string WorkstationID { get; set; }//V2049
        public string ApplicationCode { get; set; }//V2049
        public string TerritoryCode { get; set; }//V2049

    }
}
