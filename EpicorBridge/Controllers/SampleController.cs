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
        /// Executes a BAQ
        /// </summary>       
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CallBAQ()
        {
            var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetCustomers, Request.Query, Method.GET); 
            return response;
        }
           

        /// <summary>
        /// Executes a BAQ with a required BAQ Parameter
        /// </summary>
        /// <param name="zipCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CallMandatoryParameterizedBAQ([Required]string zipCode)
        {
            if (!String.IsNullOrEmpty(zipCode))
            {                
                var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetCustomersParam, Request.Query, Method.GET);
                return response;
            }
            else
                return BadRequest("Input is empty");            
        }



        /// <summary>
        /// Executes a BAQ with an optional BAQ Parameter and set a default value if none is provided
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="zipCode">"true"</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CallOptionalParameterizedBAQ(string zipCode = "90210")
        {
            var response = await _epiAPIConnect.ExecuteBAQ(EpiBAQs.GetCustomersParamOptional, Request.Query, Method.GET);
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
