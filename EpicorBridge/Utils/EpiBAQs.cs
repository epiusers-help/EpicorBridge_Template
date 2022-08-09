using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EpicorBridge.Utils
{
    /// <summary>
    /// Stores BAQ IDs associated with this integration
    /// </summary>
    public class EpiBAQs
    {
        //Customer Specific 
        public static string GetCustomers = new string("WEB_Customers");
        public static string GetBillToCustomers = new string("WEB_GetBillToCustomers");
       

        //PerCon Specific
        public static string GetPerCons = new string("WEB_PerCons");
        public static string GetLinkedCustomers = new string("WEB_PerConCustCnt");

        //Invoice Specific
        public static string GetInvoiceData = new string("WEB_GetInvoiceData"); 


    }

}
