using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EpicorBridge.Utils
{

    /// <summary>
    /// Handles the Session creation for the Epicor calls.
    /// Every 5-minutes, either refresh the session or get a new one. Use the Web Service license type
    /// Each call to a BAQ or Function also calls to check if the session is valid, and if not attempts a login
    /// </summary>
    public class EpiSessionSvc : BackgroundService
    {
        private readonly EpiUtils _epiUtils;       
        private readonly IOptions<EpiSettings> _epiSettings;
        private readonly ILogger<EpiSessionSvc> _logger;
        private readonly string _user;
        private readonly string _apiKey;        
        private readonly string _path;
        private readonly string _licenseType;
       
        public EpiSessionSvc(EpiUtils utils, IOptions<EpiSettings> app, ILogger<EpiSessionSvc> logger)
        {
            _epiUtils = utils ?? throw new ArgumentNullException(nameof(utils));          
            _epiSettings = app ?? throw new ArgumentNullException(nameof(app));        
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _user = $"{_epiSettings.Value.IntegrationUser}:{_epiSettings.Value.IntegrationPassword}";           
            _apiKey = $"{_epiSettings.Value.ApiKey}";
            _path = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v2/odata/{_epiSettings.Value.Company}";
            _licenseType = $"{_epiSettings.Value.LicenseTypeGuid}";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
               _logger.LogInformation($"Running CreateOrValidateSession at {DateTime.Now}");
                await CreateOrValidateSession();
                await Task.Delay(TimeSpan.FromMinutes(5));               
            }            
        }

        private async Task CreateOrValidateSession()
        {
            _logger.LogInformation("Validating Session...");
            if (_epiUtils.ValidSession(_epiUtils.sessionID, _licenseType, _path, _user, _apiKey, out string msg))
            {
                _logger.LogInformation($"Session {_epiUtils.sessionID} is valid");
            }
            else
            {               
                _logger.LogInformation($"Session {_epiUtils.sessionID} is invalid: {msg}");
                _logger.LogInformation("Getting a new Session...");
                if (_epiUtils.Login(_licenseType, _path, _user, _apiKey, out msg))
                {
                    _logger.LogInformation($"Login Successful: SessionID {_epiUtils.sessionID}");
                }
                else
                {
                    _logger.LogError($"Unable to login: {msg}");
                }
            }
            await Task.Delay(0);
        }
    }
   
}

 
