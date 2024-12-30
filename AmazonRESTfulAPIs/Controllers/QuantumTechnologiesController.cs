using Amazon.Braket;
using Amazon.Braket.Model;
using Microsoft.AspNetCore.Mvc;
using GetDeviceRequest = Amazon.Braket.Model.GetDeviceRequest;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuantumTechnologiesController : ControllerBase
    {
        private readonly IAmazonBraket _braketClient;

        public QuantumTechnologiesController(IAmazonBraket braketClient)
        {
            _braketClient = braketClient;
        }

        [HttpGet("devices")]
        public async Task<IActionResult> ListQuantumDevices()
        {
            try
            {
                var request = new SearchDevicesRequest();
                var response = await _braketClient.SearchDevicesAsync(request);
                return Ok(response.Devices);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetDevice(string deviceArnId)
        {
            try
            {
                var request = new GetDeviceRequest
                {
                    DeviceArn = deviceArnId
                };
                var response = await _braketClient.GetDeviceAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("quantum-task")]
        public async Task<IActionResult> CreateQuantumTask([FromBody] CreateQuantumTaskRequest request)
        {
            try
            {
                var response = await _braketClient.CreateQuantumTaskAsync(request);
                return Ok(new
                {
                    QuantumTaskArn = response.QuantumTaskArn,
                    Status = response.HttpStatusCode
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("quantum-task/{taskArn}")]
        public async Task<IActionResult> GetQuantumTask(string taskArn)
        {
            try
            {
                var request = new GetQuantumTaskRequest
                {
                    QuantumTaskArn = taskArn
                };
                var response = await _braketClient.GetQuantumTaskAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("quantum-tasks")]
        public async Task<IActionResult> ListQuantumTasks([FromQuery] string deviceId, [FromQuery] DateTime? startTime)
        {
            try
            {
                var request = new SearchQuantumTasksRequest
                {
                    Filters = new List<SearchQuantumTasksFilter>()
                };

                if (!string.IsNullOrEmpty(deviceId))
                {
                    request.Filters.Add(new SearchQuantumTasksFilter
                    {
                        Name = "deviceId",
                        Operator = "EQUAL",
                        Values = new List<string> { deviceId }
                    });
                }

                if (startTime.HasValue)
                {
                    request.Filters.Add(new SearchQuantumTasksFilter
                    {
                        Name = "createdAt",
                        Operator = "GREATER_THAN",
                        Values = new List<string> { startTime.Value.ToString("O") }
                    });
                }

                var response = await _braketClient.SearchQuantumTasksAsync(request);
                return Ok(response.QuantumTasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("quantum-job")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            try
            {
                var response = await _braketClient.CreateJobAsync(request);
                return Ok(new
                {
                    JobArn = response.JobArn,
                    Status = response.HttpStatusCode
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("quantum-job/{jobArn}")]
        public async Task<IActionResult> CancelJob(string jobArn)
        {
            try
            {
                var request = new CancelJobRequest
                {
                    JobArn = jobArn
                };
                var response = await _braketClient.CancelJobAsync(request);
                return Ok($"Job {jobArn} cancelled successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom models for request validation
        public class QuantumTaskRequest
        {
            public string DeviceArn { get; set; }
            public string OutputS3Bucket { get; set; }
            public string OutputS3KeyPrefix { get; set; }
            public string Program { get; set; }
            public int Shots { get; set; }
        }

        public class QuantumJobRequest
        {
            public string JobName { get; set; }
            public string DeviceArn { get; set; }
            public string InputDataConfig { get; set; }
            public string OutputDataConfig { get; set; }
            public string SourceModule { get; set; }
            public Dictionary<string, string> HyperParameters { get; set; }
        }

        
    }
}
