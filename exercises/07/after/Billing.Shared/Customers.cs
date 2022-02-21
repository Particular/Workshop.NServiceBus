namespace Billing.Shared
{
    using System.Collections.Generic;

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