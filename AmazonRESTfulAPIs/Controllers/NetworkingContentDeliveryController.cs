using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.Route53;
using Amazon.Route53.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkingContentDeliveryController : ControllerBase
    {
        private readonly IAmazonCloudFront _cloudFrontClient;
        private readonly IAmazonRoute53 _route53Client;

        public NetworkingContentDeliveryController(
            IAmazonCloudFront cloudFrontClient,
            IAmazonRoute53 route53Client)
        {
            _cloudFrontClient = cloudFrontClient;
            _route53Client = route53Client;
        }

        // CloudFront Endpoints
        [HttpGet("distributions")]
        public async Task<IActionResult> ListDistributions()
        {
            try
            {
                var request = new ListDistributionsRequest();
                var response = await _cloudFrontClient.ListDistributionsAsync(request);
                return Ok(response.DistributionList.Items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("distribution/{distributionId}")]
        public async Task<IActionResult> GetDistribution(string distributionId)
        {
            try
            {
                var request = new GetDistributionRequest
                {
                    Id = distributionId
                };
                var response = await _cloudFrontClient.GetDistributionAsync(request);
                return Ok(response.Distribution);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("distribution")]
        public async Task<IActionResult> CreateDistribution([FromBody] CreateDistributionRequest request)
        {
            try
            {
                var response = await _cloudFrontClient.CreateDistributionAsync(request);
                return Ok(new
                {
                    DistributionId = response.Distribution.Id,
                    DomainName = response.Distribution.DomainName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("distribution/{distributionId}")]
        public async Task<IActionResult> DeleteDistribution(string distributionId)
        {
            try
            {
                // First, get the ETag
                var getRequest = new GetDistributionRequest { Id = distributionId };
                var getResponse = await _cloudFrontClient.GetDistributionAsync(getRequest);

                var deleteRequest = new DeleteDistributionRequest
                {
                    Id = distributionId,
                    IfMatch = getResponse.ETag
                };
                await _cloudFrontClient.DeleteDistributionAsync(deleteRequest);
                return Ok($"Distribution {distributionId} deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Route 53 Endpoints
        [HttpGet("hosted-zones")]
        public async Task<IActionResult> ListHostedZones()
        {
            try
            {
                var request = new ListHostedZonesRequest();
                var response = await _route53Client.ListHostedZonesAsync(request);
                return Ok(response.HostedZones);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("hosted-zone")]
        public async Task<IActionResult> CreateHostedZone([FromBody] HostedZoneRequest request)
        {
            try
            {
                var createRequest = new CreateHostedZoneRequest
                {
                    Name = request.DomainName,
                    CallerReference = DateTime.UtcNow.Ticks.ToString(),
                    HostedZoneConfig = new HostedZoneConfig
                    {
                        Comment = request.Comment,
                        PrivateZone = request.IsPrivate
                    }
                };

                var response = await _route53Client.CreateHostedZoneAsync(createRequest);
                return Ok(response.HostedZone);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("record-set")]
        public async Task<IActionResult> CreateRecordSet([FromBody] RecordSetRequest request)
        {
            try
            {
                var change = new Change
                {
                    Action = ChangeAction.CREATE,
                    ResourceRecordSet = new ResourceRecordSet
                    {
                        Name = request.Name,
                        Type = request.Type,
                        TTL = request.TTL,
                        ResourceRecords = new List<ResourceRecord>
                        {
                            new ResourceRecord { Value = request.Value }
                        }
                    }
                };

                var changeRequest = new ChangeResourceRecordSetsRequest
                {
                    HostedZoneId = request.HostedZoneId,
                    ChangeBatch = new ChangeBatch
                    {
                        Changes = new List<Change> { change }
                    }
                };

                var response = await _route53Client.ChangeResourceRecordSetsAsync(changeRequest);
                return Ok(response.ChangeInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("health-checks")]
        public async Task<IActionResult> ListHealthChecks()
        {
            try
            {
                var request = new ListHealthChecksRequest();
                var response = await _route53Client.ListHealthChecksAsync(request);
                return Ok(response.HealthChecks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models for Request Validation
        public class HostedZoneRequest
        {
            public string DomainName { get; set; }
            public string Comment { get; set; }
            public bool IsPrivate { get; set; }
        }

        public class RecordSetRequest
        {
            public string HostedZoneId { get; set; }
            public string Name { get; set; }
            public RRType Type { get; set; }
            public long TTL { get; set; }
            public string Value { get; set; }
        }

        public class CacheInvalidationRequest
        {
            public string DistributionId { get; set; }
            public List<string> Paths { get; set; }
        }
    }
}
