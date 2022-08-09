using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EpicorBridge.Utils
{
    public class EpiAPIConnect: ControllerBase
    {
        private readonly IOptions<EpiSettings> _epiSettings;
        private readonly EpiUtils _epiUtils;
        
        private readonly string _apiKey;
        private readonly string _user;
        private readonly string _path;
        private readonly string _fxPath;
        private readonly string _baqPath;
        private readonly string _licenseType;

        public EpiAPIConnect(IOptions<EpiSettings> app, EpiUtils utils)
        {
            _epiSettings = app ?? throw new ArgumentNullException(nameof(app));
            _epiUtils = utils ?? throw new ArgumentNullException(nameof(utils));

            _apiKey = $"{_epiSettings.Value.ApiKey}";
            //_path = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v2/";
            _path = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v2/odata/{_epiSettings.Value.Company}";
            _fxPath = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v2/efx/{_epiSettings.Value.Company}";
            _baqPath = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v2/odata/{_epiSettings.Value.Company}";
            _user = $"{_epiSettings.Value.IntegrationUser}:{_epiSettings.Value.IntegrationPassword}";
            _licenseType = $"{_epiSettings.Value.LicenseTypeGuid}";
        }

    
        /// <summary>
        /// Invokes an Epicor Function from the specific function library
        /// </summary>
        /// <param name="library">The Library ID associated with the function</param>
        /// <param name="functionID">The Function ID to be invoked</param>
        /// <param name="fxRequest">The JSON from the call body representing the optional input parameters</param>
        /// <returns></returns>
        public async Task<IActionResult> InvokeFunction(string library, string functionID, dynamic fxRequest)
        {
            if (_epiUtils.ValidSession(_epiUtils.sessionID, _licenseType, _path, _user, _apiKey, out string msg))
            {
                var restClient = new RestClient(_fxPath)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var request = new RestRequest($"{library}/{functionID}", Method.POST);
                //add any optional request parameters
                request.AddParameter("application/json", fxRequest, ParameterType.RequestBody);

                //Web Service License
                var headerLicense = new
                {
                    ClaimedLicense = _licenseType,
                    SessionID = _epiUtils.sessionID
                };
                var header = JsonConvert.SerializeObject(headerLicense);
                request.AddHeader("License", header);
                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
                request.AddHeader("x-api-key", _apiKey);

                IRestResponse response = await restClient.ExecuteAsync(request);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            dynamic content = JsonConvert.DeserializeObject(response.Content);
                            var value = content;
                            return BadRequest(content);
                        }
                    case System.Net.HttpStatusCode.OK:
                    default:
                        {
                            dynamic content = JsonConvert.DeserializeObject(response.Content);
                            var value = content;
                            return Ok(content);
                        }
                }
            }
            else
            {
                return Unauthorized(msg);
            }
        }

        /// <summary>
        /// Executes a Paramaterized BAQ 
        /// </summary>
        /// <param name="BAQID">BAQ ID</param>
        /// <param name="query">Query strings passed into the call</param>
        /// <param name="verb">RestSharp Method</param>
        /// <returns></returns>
        public async Task<IActionResult> ExecuteBAQ(string BAQID, IQueryCollection query , Method verb)
        {
            if (_epiUtils.ValidSession(_epiUtils.sessionID, _licenseType, _path, _user, _apiKey, out string msg))
            {
                var restClient = new RestClient(_baqPath)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var request = new RestRequest($"BaqSvc/{BAQID}/Data", verb);

                //add any optional request parameters, excluding API key of calling app              
                foreach (var p in query)
                {
                    if (p.Key != "api_key")
                    {
                        request.Parameters.Add(new RestSharp.Parameter(p.Key, p.Value, RestSharp.ParameterType.QueryString));
                    }
                }

                //License type as defined in config
                var headerLicense = new
                {
                    ClaimedLicense = _licenseType,
                    SessionID = _epiUtils.sessionID
                };
                var header = JsonConvert.SerializeObject(headerLicense);
                request.AddHeader("License", header);
                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
                request.AddHeader("x-api-key", _apiKey);

                IRestResponse response = await restClient.ExecuteAsync(request);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            dynamic content = JsonConvert.DeserializeObject(response.Content);
                            var value = content.value;
                            return BadRequest(value);
                        }
                    case System.Net.HttpStatusCode.OK:
                    default:
                        {
                            //Trim down the Epcior response to remove the metadata node and return only the value
                            dynamic content = JsonConvert.DeserializeObject(response.Content);                            
                            var value = content.value;
                            return Ok(value);
                        }
                }
            }
            else
            {
                return Unauthorized(msg);
            }
        }

    }
}


