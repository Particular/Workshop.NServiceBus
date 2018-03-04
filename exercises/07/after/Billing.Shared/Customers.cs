using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Shared
{
    public static class Customers
    {
        public static List<int> GetStrategicCustomers()
        {
            return new List<int>
            {
                3,
                4,
                5
            };
        }
    }
}
