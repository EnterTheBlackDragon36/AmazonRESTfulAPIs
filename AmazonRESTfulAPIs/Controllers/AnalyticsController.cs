using Amazon.Athena;
using Amazon.Athena.Model;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.QuickSight;
using Amazon.QuickSight.Model;
using Microsoft.AspNetCore.Mvc;
using Tag = Amazon.QuickSight.Model.Tag;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAmazonAthena _athenaClient;
        private readonly IAmazonQuickSight _quickSightClient;
        private readonly IAmazonKinesis _kinesisClient;

        public AnalyticsController(
            IAmazonAthena athenaClient,
            IAmazonQuickSight quickSightClient,
            IAmazonKinesis kinesisClient)
        {
            _athenaClient = athenaClient;
            _quickSightClient = quickSightClient;
            _kinesisClient = kinesisClient;
        }

        // Athena Endpoints
        [HttpPost("queries")]
        public async Task<IActionResult> StartQuery([FromBody] QueryExecutionRequest request)
        {
            try
            {
                var startQueryRequest = new StartQueryExecutionRequest
                {
                    QueryString = request.QueryString,
                    QueryExecutionContext = new QueryExecutionContext
                    {
                        Database = request.DatabaseName
                    },
                    ResultConfiguration = new ResultConfiguration
                    {
                        OutputLocation = request.OutputLocation
                    }
                };

                var response = await _athenaClient.StartQueryExecutionAsync(startQueryRequest);
                return Ok(new { QueryExecutionId = response.QueryExecutionId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("queries/{queryExecutionId}")]
        public async Task<IActionResult> GetQueryResults(string queryExecutionId)
        {
            try
            {
                var request = new Amazon.Athena.Model.GetQueryResultsRequest
                {
                    QueryExecutionId = queryExecutionId
                };

                var response = await _athenaClient.GetQueryResultsAsync(request);
                return Ok(response.ResultSet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("queries/{queryExecutionId}/status")]
        public async Task<IActionResult> GetQueryStatus(string queryExecutionId)
        {
            try
            {
                var request = new GetQueryExecutionRequest
                {
                    QueryExecutionId = queryExecutionId
                };

                var response = await _athenaClient.GetQueryExecutionAsync(request);
                return Ok(response.QueryExecution.Status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // QuickSight Endpoints
        [HttpPost("dashboards")]
        public async Task<IActionResult> CreateDashboard([FromBody] DashboardRequest request)
        {
            try
            {
                var createRequest = new CreateDashboardRequest
                {
                    AwsAccountId = request.AwsAccountId,
                    DashboardId = request.DashboardId,
                    Name = request.Name,
                    Permissions = request.Permissions,
                    SourceEntity = request.SourceEntity,
                    Tags = request.Tags
                };

                var response = await _quickSightClient.CreateDashboardAsync(createRequest);
                return Ok(response.DashboardId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dashboards/{awsAccountId}")]
        public async Task<IActionResult> ListDashboards(string awsAccountId)
        {
            try
            {
                var request = new Amazon.QuickSight.Model.ListDashboardsRequest
                {
                    AwsAccountId = awsAccountId
                };

                var response = await _quickSightClient.ListDashboardsAsync(request);
                return Ok(response.DashboardSummaryList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Kinesis Data Streams Endpoints
        [HttpPost("streams")]
        public async Task<IActionResult> CreateStream([FromBody] StreamRequest request)
        {
            try
            {
                var createRequest = new CreateStreamRequest
                {
                    StreamName = request.StreamName,
                    ShardCount = request.ShardCount
                };

                await _kinesisClient.CreateStreamAsync(createRequest);
                return Ok($"Stream {request.StreamName} creation initiated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("streams/{streamName}/records")]
        public async Task<IActionResult> PutRecords(string streamName, [FromBody] List<KinesisRecord> records)
        {
            try
            {
                var putRecordsRequest = new PutRecordsRequest
                {
                    StreamName = streamName,
                    Records = records.Select(r => new PutRecordsRequestEntry
                    {
                        Data = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(r.Data)),
                        PartitionKey = r.PartitionKey
                    }).ToList()
                };

                var response = await _kinesisClient.PutRecordsAsync(putRecordsRequest);
                return Ok(new
                {
                    FailedRecordCount = response.FailedRecordCount,
                    Records = response.Records
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("streams/{streamName}/records")]
        public async Task<IActionResult> GetRecords(string streamName)
        {
            try
            {
                // First, get a shard iterator
                var shardIteratorRequest = new GetShardIteratorRequest
                {
                    StreamName = streamName,
                    ShardId = "shardId-000000000000", // You might want to make this configurable
                    ShardIteratorType = ShardIteratorType.TRIM_HORIZON
                };

                var shardIteratorResponse = await _kinesisClient.GetShardIteratorAsync(shardIteratorRequest);

                // Then, get records using the shard iterator
                var getRecordsRequest = new GetRecordsRequest
                {
                    ShardIterator = shardIteratorResponse.ShardIterator,
                    Limit = 100 // Configurable
                };

                var response = await _kinesisClient.GetRecordsAsync(getRecordsRequest);
                return Ok(response.Records);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class QueryExecutionRequest
        {
            public string QueryString { get; set; }
            public string DatabaseName { get; set; }
            public string OutputLocation { get; set; }
        }

        public class DashboardRequest
        {
            public string AwsAccountId { get; set; }
            public string DashboardId { get; set; }
            public string Name { get; set; }
            public List<ResourcePermission> Permissions { get; set; }
            public DashboardSourceEntity SourceEntity { get; set; }
            public List<Tag> Tags { get; set; }
        }

        public class StreamRequest
        {
            public string StreamName { get; set; }
            public int ShardCount { get; set; }
        }

        public class KinesisRecord
        {
            public string Data { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}
