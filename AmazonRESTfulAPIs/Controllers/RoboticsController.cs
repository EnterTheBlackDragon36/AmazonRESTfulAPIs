using Amazon.RoboMaker;
using Amazon.RoboMaker.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoboticsController : ControllerBase
    {
        private readonly IAmazonRoboMaker _roboMakerClient;

        public RoboticsController(IAmazonRoboMaker roboMakerClient)
        {
            _roboMakerClient = roboMakerClient;
        }

        [HttpPost("simulation-job")]
        public async Task<IActionResult> CreateSimulationJob([FromBody] CreateSimulationJobRequest request)
        {
            try
            {
                var response = await _roboMakerClient.CreateSimulationJobAsync(request);
                return Ok(new
                {
                    SimulationJobArn = response.Arn,
                    Status = response.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("simulation-job/{jobId}")]
        public async Task<IActionResult> GetSimulationJob(string jobId)
        {
            try
            {
                var request = new DescribeSimulationJobRequest
                {
                    Job = jobId
                };
                var response = await _roboMakerClient.DescribeSimulationJobAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("simulation-jobs")]
        public async Task<IActionResult> ListSimulationJobs()
        {
            try
            {
                var request = new ListSimulationJobsRequest();
                var response = await _roboMakerClient.ListSimulationJobsAsync(request);
                return Ok(response.SimulationJobSummaries);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("robot-applications")]
        public async Task<IActionResult> CreateRobotApplication([FromBody] CreateRobotApplicationRequest request)
        {
            try
            {
                var response = await _roboMakerClient.CreateRobotApplicationAsync(request);
                return Ok(new
                {
                    ApplicationArn = response.Arn,
                    Version = response.Version
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("robot-applications")]
        public async Task<IActionResult> ListRobotApplications()
        {
            try
            {
                var request = new ListRobotApplicationsRequest();
                var response = await _roboMakerClient.ListRobotApplicationsAsync(request);
                return Ok(response.RobotApplicationSummaries);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("simulation-applications")]
        public async Task<IActionResult> CreateSimulationApplication([FromBody] CreateSimulationApplicationRequest request)
        {
            try
            {
                var response = await _roboMakerClient.CreateSimulationApplicationAsync(request);
                return Ok(new
                {
                    ApplicationArn = response.Arn,
                    Version = response.Version
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("simulation-job/{jobId}")]
        public async Task<IActionResult> CancelSimulationJob(string jobId)
        {
            try
            {
                var request = new CancelSimulationJobRequest
                {
                    Job = jobId
                };
                await _roboMakerClient.CancelSimulationJobAsync(request);
                return Ok($"Simulation job {jobId} cancelled successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom models for request validation
        public class SimulationJobRequest
        {
            public string RobotApplicationArn { get; set; }
            public string SimulationApplicationArn { get; set; }
            public DataSourceConfig DataSources { get; set; }
            public int MaxJobDurationInSeconds { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class DataSourceConfig
        {
            public string Name { get; set; }
            public S3KeyOutput S3Bucket { get; set; }
        }

        public class S3KeyOutput
        {
            public string BucketName { get; set; }
            public string Prefix { get; set; }
        }
    }
}
