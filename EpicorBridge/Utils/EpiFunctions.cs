using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EpicorBridge.Utils
{
    /// <summary>
    /// Stores Function Library IDs associated with this integration
    /// </summary>
    public class EpiFunctions
    {
        //Libraries Used
        public static string EpicorBridge = new string("EpicorBridge");

        //PerCon Functions
        public static string CreatePerCon = new string("PerCon-CreateContact");

        //Order Functions
        /// <summary>
        /// Both are routed through the Order Controller and are dependent on the query string passed
        /// </summary>
        //Sales Order
        public static string CreateSalesOrder = new string("Order-CreateSalesOrder");
        public static string UpdatePONumSO = new string("Order-UpdatePONumSO");
        //Quote
        public static string CreateQuote = new string("Order-CreateQuote");        
        public static string UpdatePONumQuote = new string("Order-UpdatePONumQuote");
        public static string UpdateQuoteStatus = new string("Order-UpdateQuoteStatus");



    }
}
