using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.DAL.Enums
{
    public enum SubscriptionPeriod
    {
        [Description("Monthly")]
        Monthly = 1,

        [Description("Quarterly")]
        Quarterly = 2,

        [Description("Half-Yearly")]
        HalfYearly = 3,

        [Description("Yearly")]
        Yearly = 4
    }

}
