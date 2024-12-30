using Amazon.DatabaseMigrationService;
using Amazon.DatabaseMigrationService.Model;
using Amazon.Transfer;
using Amazon.Transfer.Model;
using Microsoft.AspNetCore.Mvc;
using CreateEndpointRequest = Amazon.DatabaseMigrationService.Model.CreateEndpointRequest;
using DescribeEndpointsRequest = Amazon.DatabaseMigrationService.Model.DescribeEndpointsRequest;


namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationTransferController : ControllerBase
    {
        private readonly IAmazonDatabaseMigrationService _dmsClient;
        private readonly IAmazonTransfer _transferClient;

        public MigrationTransferController(
            IAmazonDatabaseMigrationService dmsClient,
            IAmazonTransfer transferClient)
        {
            _dmsClient = dmsClient;
            _transferClient = transferClient;
        }

        // Database Migration Service Endpoints
        [HttpPost("replication-tasks")]
        public async Task<IActionResult> CreateReplicationTask([FromBody] CreateReplicationTaskRequest request)
        {
            try
            {
                var response = await _dmsClient.CreateReplicationTaskAsync(request);
                return Ok(response.ReplicationTask);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("replication-tasks")]
        public async Task<IActionResult> ListReplicationTasks()
        {
            try
            {
                var request = new DescribeReplicationTasksRequest();
                var response = await _dmsClient.DescribeReplicationTasksAsync(request);
                return Ok(response.ReplicationTasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("endpoints")]
        public async Task<IActionResult> CreateEndpoint([FromBody] CreateEndpointRequest request)
        {
            try
            {
                var response = await _dmsClient.CreateEndpointAsync(request);
                return Ok(response.Endpoint);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("endpoints")]
        public async Task<IActionResult> ListEndpoints()
        {
            try
            {
                var request = new DescribeEndpointsRequest();
                var response = await _dmsClient.DescribeEndpointsAsync(request);
                return Ok(response.Endpoints);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Transfer Service Endpoints
        [HttpPost("servers")]
        public async Task<IActionResult> CreateTransferServer([FromBody] TransferServerRequest request)
        {
            try
            {
                var createRequest = new CreateServerRequest
                {
                    EndpointType = request.EndpointType,
                    IdentityProviderType = request.IdentityProviderType,
                    Protocols = request.Protocols,
                    Tags = request.Tags?.Select(t => new Amazon.Transfer.Model.Tag { Key = t.Key, Value = t.Value }).ToList()
                };

                var response = await _transferClient.CreateServerAsync(createRequest);
                return Ok(new { ServerId = response.ServerId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("servers")]
        public async Task<IActionResult> ListServers()
        {
            try
            {
                var request = new ListServersRequest();
                var response = await _transferClient.ListServersAsync(request);
                return Ok(response.Servers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] TransferUserRequest request)
        {
            try
            {
                var createRequest = new CreateUserRequest
                {
                    ServerId = request.ServerId,
                    UserName = request.UserName,
                    Role = request.Role,
                    HomeDirectory = request.HomeDirectory,
                    SshPublicKeyBody = request.SshPublicKey
                };

                var response = await _transferClient.CreateUserAsync(createRequest);
                return Ok(response.ServerId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/{serverId}")]
        public async Task<IActionResult> ListUsers(string serverId)
        {
            try
            {
                var request = new ListUsersRequest
                {
                    ServerId = serverId
                };
                var response = await _transferClient.ListUsersAsync(request);
                return Ok(response.Users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("data-migration")]
        public async Task<IActionResult> CreateDataMigration([FromBody] DataMigrationRequest request)
        {
            try
            {
                var migrationRequest = new CreateReplicationTaskRequest
                {
                    ReplicationInstanceArn = request.ReplicationInstanceArn,
                    SourceEndpointArn = request.SourceEndpointArn,
                    TargetEndpointArn = request.TargetEndpointArn,
                    ReplicationTaskIdentifier = request.TaskIdentifier,
                    MigrationType = request.MigrationType,
                    TableMappings = request.TableMappings
                };

                var response = await _dmsClient.CreateReplicationTaskAsync(migrationRequest);
                return Ok(response.ReplicationTask);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models for Request Validation
        public class TransferServerRequest
        {
            public string EndpointType { get; set; }
            public string IdentityProviderType { get; set; }
            public List<string> Protocols { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class TransferUserRequest
        {
            public string ServerId { get; set; }
            public string UserName { get; set; }
            public string Role { get; set; }
            public string HomeDirectory { get; set; }
            public string SshPublicKey { get; set; }
        }

        public class DataMigrationRequest
        {
            public string ReplicationInstanceArn { get; set; }
            public string SourceEndpointArn { get; set; }
            public string TargetEndpointArn { get; set; }
            public string TaskIdentifier { get; set; }
            public string MigrationType { get; set; }
            public string TableMappings { get; set; }
        }
    }
}
