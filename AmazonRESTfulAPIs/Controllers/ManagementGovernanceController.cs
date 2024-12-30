using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Organizations;
using Amazon.Organizations.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementGovernanceController : ControllerBase
    {
        private readonly IAmazonCloudWatch _cloudWatchClient;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly IAmazonOrganizations _organizationsClient;

        public ManagementGovernanceController(
            IAmazonCloudWatch cloudWatchClient,
            IAmazonSimpleSystemsManagement ssmClient,
            IAmazonOrganizations organizationsClient)
        {
            _cloudWatchClient = cloudWatchClient;
            _ssmClient = ssmClient;
            _organizationsClient = organizationsClient;
        }

        // CloudWatch Endpoints
        [HttpPost("metrics")]
        public async Task<IActionResult> PutMetricData([FromBody] MetricDataRequest request)
        {
            try
            {
                var putRequest = new PutMetricDataRequest
                {
                    Namespace = request.Namespace,
                    MetricData = new List<MetricDatum>
                    {
                        new MetricDatum
                        {
                            MetricName = request.MetricName,
                            Value = request.Value,
                            Unit = request.Unit,
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };

                await _cloudWatchClient.PutMetricDataAsync(putRequest);
                return Ok("Metric data published successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] string nameSpace)
        {
            try
            {
                var request = new ListMetricsRequest
                {
                    Namespace = nameSpace
                };
                var response = await _cloudWatchClient.ListMetricsAsync(request);
                return Ok(response.Metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("alarms")]
        public async Task<IActionResult> CreateAlarm([FromBody] AlarmRequest request)
        {
            try
            {
                var putRequest = new PutMetricAlarmRequest
                {
                    AlarmName = request.AlarmName,
                    MetricName = request.MetricName,
                    Namespace = request.Namespace,
                    ComparisonOperator = request.ComparisonOperator,
                    Threshold = request.Threshold,
                    EvaluationPeriods = request.EvaluationPeriods,
                    Period = request.Period,
                    Statistic = request.Statistic
                };

                await _cloudWatchClient.PutMetricAlarmAsync(putRequest);
                return Ok($"Alarm {request.AlarmName} created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Systems Manager Endpoints
        [HttpPost("parameters")]
        public async Task<IActionResult> PutParameter([FromBody] ParameterRequest request)
        {
            try
            {
                var putRequest = new PutParameterRequest
                {
                    Name = request.Name,
                    Value = request.Value,
                    Type = request.Type,
                    Overwrite = request.Overwrite
                };

                var response = await _ssmClient.PutParameterAsync(putRequest);
                return Ok(new { Version = response.Version });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("parameters/{name}")]
        public async Task<IActionResult> GetParameter(string name)
        {
            try
            {
                var request = new GetParameterRequest
                {
                    Name = name,
                    WithDecryption = true
                };

                var response = await _ssmClient.GetParameterAsync(request);
                return Ok(response.Parameter);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("automation")]
        public async Task<IActionResult> StartAutomation([FromBody] AutomationRequest request)
        {
            try
            {
                var startRequest = new StartAutomationExecutionRequest
                {
                    DocumentName = request.DocumentName,
                    Parameters = request.Parameters
                };

                var response = await _ssmClient.StartAutomationExecutionAsync(startRequest);
                return Ok(new { AutomationExecutionId = response.AutomationExecutionId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Organizations Endpoints
        [HttpPost("organizations/account")]
        public async Task<IActionResult> CreateAccount([FromBody] Amazon.Organizations.Model.CreateAccountRequest request)
        {
            try
            {
                var response = await _organizationsClient.CreateAccountAsync(request);
                return Ok(response.CreateAccountStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("organizations/accounts")]
        public async Task<IActionResult> ListAccounts()
        {
            try
            {
                var request = new Amazon.Organizations.Model.ListAccountsRequest();
                var response = await _organizationsClient.ListAccountsAsync(request);
                return Ok(response.Accounts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("organizations/policy")]
        public async Task<IActionResult> CreatePolicy([FromBody] Amazon.Organizations.Model.CreatePolicyRequest request)
        {
            try
            {
                var response = await _organizationsClient.CreatePolicyAsync(request);
                return Ok(response.Policy);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class MetricDataRequest
        {
            public string Namespace { get; set; }
            public string MetricName { get; set; }
            public double Value { get; set; }
            public StandardUnit Unit { get; set; }
        }

        public class AlarmRequest
        {
            public string AlarmName { get; set; }
            public string MetricName { get; set; }
            public string Namespace { get; set; }
            public ComparisonOperator ComparisonOperator { get; set; }
            public double Threshold { get; set; }
            public int EvaluationPeriods { get; set; }
            public int Period { get; set; }
            public Statistic Statistic { get; set; }
        }

        public class ParameterRequest
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public ParameterType Type { get; set; }
            public bool Overwrite { get; set; }
        }

        public class AutomationRequest
        {
            public string DocumentName { get; set; }
            public Dictionary<string, List<string>> Parameters { get; set; }
        }
    }
}
