using Amazon.MediaConvert;
using Amazon.MediaConvert.Model;
using Amazon.MediaLive;
using Amazon.MediaLive.Model;
using Amazon.MediaPackage;
using Amazon.MediaPackage.Model;
using Microsoft.AspNetCore.Mvc;
using CreateJobRequest = Amazon.MediaConvert.Model.CreateJobRequest;
using ListChannelsRequest = Amazon.MediaLive.Model.ListChannelsRequest;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaServicesController : ControllerBase
    {
        private readonly IAmazonMediaConvert _mediaConvertClient;
        private readonly IAmazonMediaLive _mediaLiveClient;
        private readonly IAmazonMediaPackage _mediaPackageClient;

        public MediaServicesController(
            IAmazonMediaConvert mediaConvertClient,
            IAmazonMediaLive mediaLiveClient,
            IAmazonMediaPackage mediaPackageClient)
        {
            _mediaConvertClient = mediaConvertClient;
            _mediaLiveClient = mediaLiveClient;
            _mediaPackageClient = mediaPackageClient;
        }

        // MediaConvert Endpoints
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            try
            {
                var response = await _mediaConvertClient.CreateJobAsync(request);
                return Ok(new
                {
                    JobId = response.Job.Id,
                    Status = response.Job.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> ListJobs([FromQuery] string queueArn = null)
        {
            try
            {
                var request = new ListJobsRequest
                {
                    Queue = queueArn,
                    MaxResults = 20
                };
                var response = await _mediaConvertClient.ListJobsAsync(request);
                return Ok(response.Jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetJob(string jobId)
        {
            try
            {
                var request = new GetJobRequest { Id = jobId };
                var response = await _mediaConvertClient.GetJobAsync(request);
                return Ok(response.Job);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // MediaLive Endpoints
        [HttpPost("channels")]
        public async Task<IActionResult> CreateChannel([FromBody] Amazon.MediaLive.Model.CreateChannelRequest request)
        {
            try
            {
                var response = await _mediaLiveClient.CreateChannelAsync(request);
                return Ok(new
                {
                    ChannelId = response.Channel.Id,
                    State = response.Channel.State
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("channels")]
        public async Task<IActionResult> ListChannels()
        {
            try
            {
                var request = new ListChannelsRequest();
                var response = await _mediaLiveClient.ListChannelsAsync(request);
                return Ok(response.Channels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("channel/{channelId}/start")]
        public async Task<IActionResult> StartChannel(string channelId)
        {
            try
            {
                var request = new StartChannelRequest { ChannelId = channelId };
                var response = await _mediaLiveClient.StartChannelAsync(request);
                return Ok(new { State = response.State });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("channel/{channelId}/stop")]
        public async Task<IActionResult> StopChannel(string channelId)
        {
            try
            {
                var request = new StopChannelRequest { ChannelId = channelId };
                var response = await _mediaLiveClient.StopChannelAsync(request);
                return Ok(new { State = response.State });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // MediaPackage Endpoints
        [HttpPost("channels/package")]
        public async Task<IActionResult> CreatePackageChannel([FromBody] Amazon.MediaPackage.Model.CreateChannelRequest request)
        {
            try
            {
                var response = await _mediaPackageClient.CreateChannelAsync(request, CancellationToken.None);
                return Ok(new
                {
                    ChannelId = response.Id,
                    Arn = response.Arn
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("endpoints")]
        public async Task<IActionResult> CreateOriginEndpoint([FromBody] CreateOriginEndpointRequest request)
        {
            try
            {
                var response = await _mediaPackageClient.CreateOriginEndpointAsync(request);
                return Ok(new
                {
                    EndpointId = response.Id,
                    Url = response.Url
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("endpoints/{channelId}")]
        public async Task<IActionResult> ListOriginEndpoints(string channelId)
        {
            try
            {
                var request = new ListOriginEndpointsRequest
                {
                    ChannelId = channelId
                };
                var response = await _mediaPackageClient.ListOriginEndpointsAsync(request);
                return Ok(response.OriginEndpoints);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class MediaJobRequest
        {
            public string InputS3Url { get; set; }
            public string OutputS3Url { get; set; }
            public string JobTemplate { get; set; }
            public Dictionary<string, string> JobMetadata { get; set; }
        }

        public class LiveStreamRequest
        {
            public string ChannelName { get; set; }
            public string InputType { get; set; }
            public string InputUrl { get; set; }
            public List<OutputDestination> OutputDestinations { get; set; }
        }

        public class OutputDestination
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string StreamName { get; set; }
        }
    }
}
