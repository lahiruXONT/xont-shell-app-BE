using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class Password
    {
        public string MinLength { get; set; }
        public string MaxLength { get; set; }
        public string NoOfSpecialCharacters { get; set; }
        public int ReusePeriod { get; set; }
        public int NoOfAttempts { get; set; }
        public int ExpirePeriodInMonths { get; set; }
        public int UserExpirePeriodInMonths { get; set; }

    }
}
