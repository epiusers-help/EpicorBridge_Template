using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EpicorBridge.Utils
{
    /// <summary>
    /// Epicor Settings defined in the appsettings.json
    /// </summary>
    public class EpiSettings
    {
        public string Host { get; set; }
        public string Company { get; set; }
        public string Instance { get; set; }       
        public string ApiKey { get; set; }
        public string IntegrationUser { get; set; }
        public string IntegrationPassword { get; set; }              
        public string LicenseTypeGuid { get; set; }        
    }
      
    
    public class EpiUtils
    {
        //this should be the global sessionID associated with all the calls to Epicor
        public string sessionID = string.Empty;


        /// <summary>
        /// Encodes a string to base64, using default encoding.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encdoded string.</returns>
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Decodes a string from base64, using default encoding.
        /// </summary>
        /// <param name="str">base64 encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Base64Decode(string str)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(str));
        }

        /// <summary>
        /// Generates Password Hash
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        /// <summary>
        /// Generates Password Hash String
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// Validates a Session
        /// </summary>
        /// <param name="_sessionID"></param>
        /// <param name="_licenseType"></param>
        /// <param name="_path"></param>
        /// <param name="_user"></param>
        /// <param name="_apiKey"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ValidSession(string _sessionID, string _licenseType, string _path, string _user, string _apiKey, out string msg)
        {                
            if (!string.IsNullOrEmpty(_sessionID))
            {
                //Validate a Session
                var restClient = new RestClient(_path)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var request = new RestRequest("Ice.Lib.SessionModSvc/IsValidSession", Method.POST);
                var callBody = new
                {
                    sessionID = _sessionID,
                    userID = _user
                };
                var content = JsonConvert.SerializeObject(callBody);
                request.AddJsonBody(content);
               
                var license = new
                {
                    ClaimedLicense = _licenseType
                };
                var header = JsonConvert.SerializeObject(license);
                request.AddHeader("License", header);
                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
                request.AddHeader("Accept", "application/json");                   
                request.AddHeader("x-api-key", _apiKey);                             

                IRestResponse response =restClient.Execute(request);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                    {
                            msg = response.Content;
                            return false;                        
                    }
                    case System.Net.HttpStatusCode.OK:
                        {
                            msg = response.Content;
                            return true;
                            
                        }
                    default:
                    {
                            msg = response.Content;
                            return true;                         
                    }                        
                }                
            }
            else
            {
                msg = "SessionID was Empty";
                return false;
            }           
        }

        /// <summary>
        /// Login a User to Epicor, creates a session
        /// </summary>
        /// <param name="_licenseType"></param>
        /// <param name="_path"></param>
        /// <param name="_user"></param>
        /// <param name="_apiKey"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Login(string _licenseType, string _path, string _user, string _apiKey, out string msg)
        {
            //Create New Session           
            var restClient = new RestClient(_path)
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            var request = new RestRequest("Ice.Lib.SessionModSvc/Login", Method.POST);            
           
            var headerLicense = new
            {
                ClaimedLicense = _licenseType
            };
            var header = JsonConvert.SerializeObject(headerLicense);
            request.AddHeader("License", header);

            request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("x-api-key", _apiKey);

            IRestResponse response = restClient.Execute(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.BadRequest:
                default:
                    {
                        msg = response.Content;
                        return false;
                    }

                case System.Net.HttpStatusCode.OK:
                    {
                        msg = response.Content;
                        dynamic val= JsonConvert.DeserializeObject(msg);
                        if(!String.IsNullOrEmpty(val.returnObj.ToString()))
                        {
                            sessionID = val.returnObj.ToString();
                            return true;
                        }
                        else
                        {
                            msg = response.Content;
                            return false;
                        }

                    }
            }            
        }
        /// <summary>
        /// Logout of Session using DeleteSessionByID method of AdminSessionSvc
        /// </summary>
        /// <param name="_sessionID">Session ID</param>
        /// <param name="_licenseType">License Type Consumed</param>
        /// <param name="_path">Server path</param>
        /// <param name="_user">Integration Account</param>
        /// <param name="msg">Return message</param>
        /// <param name="_apiKey">Epicor API Key associated with this application</param>
        /// <returns></returns>
        public bool Logout(string _sessionID, string _licenseType, string _path, string _user, string _apiKey, out string msg)
        {
            //Logout
            msg = "";
            var restClient = new RestClient(_path)
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            var request = new RestRequest("Ice.Lib.AdminSessionSvc/DeleteSessionByID", Method.POST);
            var callBody = new
            {
                sessionId = _sessionID,                
            };
            var content = JsonConvert.SerializeObject(callBody);
            request.AddJsonBody(content);
            var headerLicense = new
            {
                ClaimedLicense = _licenseType,
                SessionID = _sessionID
            };
            var header = JsonConvert.SerializeObject(headerLicense);
            request.AddHeader("License", header);

            request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("x-api-key", _apiKey);

            IRestResponse response = restClient.Execute(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.BadRequest:
                default:
                    {
                        msg = response.Content;
                        return false;
                    }

                case System.Net.HttpStatusCode.OK:
                    {
                        msg = response.Content;
                        dynamic val = JsonConvert.DeserializeObject(msg);
                        if (!String.IsNullOrEmpty(val.returnObj.ToString()))
                        {
                            sessionID = val.returnObj.ToString();
                            return true;
                        }
                        else
                        {
                            msg = response.Content;
                            return false;
                        }
                    }
            }
        }
    }
} 
