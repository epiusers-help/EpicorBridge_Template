using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EpicorBridge.Utils;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.ComponentModel.DataAnnotations;

namespace EpicorBridge.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiKeyAuth]  

    public class SampleController : ControllerBase
    {
        private readonly EpiAPIConnect _epiAPIConnect;
        public SampleController(EpiAPIConnect epiAPIConnect)
        {
            _epiAPIConnect = epiAPIConnect ?? throw new ArgumentNullException(nameof(epiAPIConnect));
        }
       
        /// <summary>
        /// Returns a list of all customers from a BAQ
        /// </summary>       
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomerList()
        {
            var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetCustomers, Request.Query, Method.GET); 
            return response;
        }
           

        /// <summary>
        /// Returns list of Bill To Customers from a BAQ with a required parameter
        /// </summary>
        /// <param name="soldToCustNum"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetBillToCustomerList([Required]string soldToCustNum)
        {
            if (!String.IsNullOrEmpty(soldToCustNum))
            {                
                var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetBillToCustomers, Request.Query, Method.GET);
                return response;
            }
            else
                return BadRequest("Input is empty");            
        }

       

        /// <summary>
        /// Returns posted invoice data from a BAQ with optional parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="priorMonth">"true"</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns either the default of current month to date data when called, or if passed with the "priorMonth" parameter vaue of "true", returns the prior month data. 
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetInvoiceData(string priorMonth = "false")
        {
            var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetInvoiceData, Request.Query, Method.GET);
            return response;
        }


      

        /// <summary>
        /// Creates a PerCon by invoking a Function and passing dynamic data in body
        /// Schema of body is defined by Function input parameters and response is the Function output parameters
        /// </summary>
        /// <remarks>
        /// Example PerCon request
        /// 
        ///     POST /CreatePerCon
        ///     {
        ///         "name": "Sample Name",
        ///         "emailAddress": "sampleEmail@email.com"
        ///     }
        ///     
        /// </remarks>
        /// <param name="fxRequest"></param>
        /// <returns>PerConID</returns>
        /// <example>5001</example>
        [HttpPost]
        public async Task<IActionResult> CreatePerCon([FromBody] dynamic fxRequest)
        {
            var response = await _epiAPIConnect.InvokeFunction(EpiFunctions.EpicorBridge, EpiFunctions.CreatePerCon, fxRequest);
            return response;
        }

        /// <summary>
        /// Invoke a Function with complex schema and routing parameters
        /// </summary>
        /// <param name="fxRequest"></param>
        /// <param name="OrderType">oca|tendon</param>
        /// <remarks>
        /// Example Order Schema (Tendons)
        /// 
        ///     POST /CreateOrder 
        ///     {
        ///         "soldToCustNum":7278,
        ///         "billToCustNum":7278,
        ///         "shipToNum":"DEFAULT",
        ///         "poNum":"123456",
        ///         "shipMethod: "FDX 1st Overnight",
        ///         "needByDate":"2020-10-25",
        ///         "productList":"[\r\n{\"PartNum\": \"SPD-001\", \"LotNum\": \"201161010\"},\r\n{\"PartNum\":\"WPL-002\",\"LotNum\": \"171269023\"}\r\n]",
        ///         "repPerConID": 3488
        ///     }
        ///    
        /// Example Quote Schema (OCA)
        /// 
        ///     POST /CreateOrder
        ///     {
        ///         "soldToCustNum":8145,
        ///         "billToCustNum":8145,
        ///         "shipToNum":"",
        ///         "poNum":"123456",
        ///         "needByDate":"2020-10-25",
        ///         "ptName":"Test Name",
        ///         "ptHeight":"Test Height",
        ///         "ptWeight":1000,
        ///         "ptDefect":"Test Defect Note",
        ///         "ptGender":"Male",
        ///         "procedure":"OATS",
        ///         "productList": "[\r\n{\"PartNum\": \"32247001\"},\r\n{\"PartNum\":\"45647010\"}\r\n]",
        ///         "ptAge":25,
        ///         "repPerConID": 3488
        ///     }
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] dynamic fxRequest, [Required] string OrderType = "")
        {
            if (OrderType.ToLower() == ("oca"))
            {
                var response = await _epiAPIConnect.InvokeFunction(EpiFunctions.EpicorBridge, EpiFunctions.CreateQuote, fxRequest);
                return response;
            }
            if (OrderType.ToLower() == ("tendon"))
            {
                var response = await _epiAPIConnect.InvokeFunction(EpiFunctions.EpicorBridge, EpiFunctions.CreateSalesOrder, fxRequest);
                return response;
            }
            else return BadRequest();
        }
    }
}
