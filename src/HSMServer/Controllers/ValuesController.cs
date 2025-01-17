﻿using System;
using HSMServer.Model;
using HSMServer.MonitoringServerCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HSMServer.Controllers
{
    /// <summary>
    /// Simple test controller for checking endpoint settings & accessibility
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly IMonitoringCore _monitoringCore;
        public ValuesController(IMonitoringCore monitoringCore, ILogger<ValuesController> logger)
        {
            _logger = logger;
            _monitoringCore = monitoringCore;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            _logger.LogInformation($"ValuesController: GET at {DateTime.Now:F}");
            return $"Now is {DateTime.Now:F}";
        }

        [HttpPost]
        public ActionResult<string> Post([FromBody]SampleData input)
        {
            _logger.LogInformation($"Received string {input.Data}");
            return Ok(input);
        }
    }
}
