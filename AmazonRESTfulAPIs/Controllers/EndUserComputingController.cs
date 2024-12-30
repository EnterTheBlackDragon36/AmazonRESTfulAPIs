using Amazon.AppStream;
using Amazon.AppStream.Model;
using Amazon.EC2.Model;
using Amazon.WorkSpaces;
using Amazon.WorkSpaces.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndUserComputingController : ControllerBase
    {
        private readonly IAmazonWorkSpaces _workSpacesClient;
        private readonly IAmazonAppStream _appStreamClient;

        public EndUserComputingController(
            IAmazonWorkSpaces workSpacesClient,
            IAmazonAppStream appStreamClient)
        {
            _workSpacesClient = workSpacesClient;
            _appStreamClient = appStreamClient;
        }

        // WorkSpaces Endpoints
        [HttpPost("workspaces")]
        public async Task<IActionResult> CreateWorkspace([FromBody] WorkspaceRequest request)
        {
            try
            {
                var createRequest = new CreateWorkspacesRequest
                {
                        Workspaces = new List<WorkspaceRequest>
                        {
                            new WorkspaceRequest
                            {
                                DirectoryId = request.DirectoryId,
                                UserName = request.UserName,
                                BundleId = request.BundleId,
                                VolumeEncryptionKey = request.VolumeEncryptionKey,
                                UserVolumeEncryptionEnabled = request.UserVolumeEncryptionEnabled,
                                RootVolumeEncryptionEnabled = request.RootVolumeEncryptionEnabled,
                            }
                        }
                };

                var response = await _workSpacesClient.CreateWorkspacesAsync(createRequest);
                return Ok(response.FailedRequests.Count == 0
                    ? "Workspace creation initiated successfully"
                    : "Some workspace requests failed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("workspaces")]
        public async Task<IActionResult> ListWorkspaces([FromQuery] string directoryId = null)
        {
            try
            {
                var request = new DescribeWorkspacesRequest
                {
                    DirectoryId = directoryId
                };
                var response = await _workSpacesClient.DescribeWorkspacesAsync(request);
                return Ok(response.Workspaces);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("workspaces/{workspaceId}/modify")]
        public async Task<IActionResult> ModifyWorkspace(string workspaceId, [FromBody] ModifyWorkspaceRequest request)
        {
            try
            {
                var modifyRequest = new ModifyWorkspacePropertiesRequest
                {
                    WorkspaceId = workspaceId,
                    WorkspaceProperties = new WorkspaceProperties
                    {
                        RunningMode = request.RunningMode,
                        RunningModeAutoStopTimeoutInMinutes = (int)request.AutoStopTimeoutMinutes
                    }
                };

                await _workSpacesClient.ModifyWorkspacePropertiesAsync(modifyRequest);
                return Ok("Workspace modified successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // AppStream 2.0 Endpoints
        [HttpPost("fleets")]
        public async Task<IActionResult> CreateFleet([FromBody] FleetRequest request)
        {
            try
            {
                var createRequest = new Amazon.AppStream.Model.CreateFleetRequest
                {
                    Name = request.FleetName,
                    ImageName = request.ImageName,
                    InstanceType = request.InstanceType,
                    FleetType = request.FleetType,
                    MaxConcurrentSessions = request.MaxConcurrentSessions,
                    DisconnectTimeoutInSeconds = request.DisconnectTimeoutInSeconds,
                    Description = request.Description,
                    DisplayName = request.DisplayName,
                    EnableDefaultInternetAccess = request.EnableDefaultInternetAccess,
                    DomainJoinInfo = request.DomainJoinInfo
                };

                var response = await _appStreamClient.CreateFleetAsync(createRequest);
                return Ok(response.Fleet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("fleets")]
        public async Task<IActionResult> ListFleets()
        {
            try
            {
                var request = new Amazon.AppStream.Model.DescribeFleetsRequest();
                var response = await _appStreamClient.DescribeFleetsAsync(request);
                return Ok(response.Fleets);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stacks")]
        public async Task<IActionResult> CreateStack([FromBody] StackRequest request)
        {
            try
            {
                var createRequest = new CreateStackRequest
                {
                    Name = request.StackName,
                    Description = request.Description,
                    DisplayName = request.DisplayName,
                    StorageConnectors = request.StorageConnectors,
                    UserSettings = request.UserSettings
                };

                var response = await _appStreamClient.CreateStackAsync(createRequest);
                return Ok(response.Stack);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("fleet-stack-association")]
        public async Task<IActionResult> AssociateFleetWithStack([FromBody] FleetStackAssociationRequest request)
        {
            try
            {
                var associateRequest = new AssociateFleetRequest
                {
                    FleetName = request.FleetName,
                    StackName = request.StackName
                };

                await _appStreamClient.AssociateFleetAsync(associateRequest);
                return Ok("Fleet associated with stack successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AppStreamUserRequest request)
        {
            try
            {
                var createRequest = new CreateUserRequest
                {
                    UserName = request.UserName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    AuthenticationType = request.AuthenticationType
                };

                await _appStreamClient.CreateUserAsync(createRequest);
                return Ok("User created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class ModifyWorkspaceRequest
        {
            public string RunningMode { get; set; }
            public int? AutoStopTimeoutMinutes { get; set; }
        }

        public class FleetRequest
        {
            public string FleetName { get; set; }
            public string ImageName { get; set; }
            public string InstanceType { get; set; }
            public string FleetType { get; set; }
            public int MaxConcurrentSessions { get; set; }
            public int DisconnectTimeoutInSeconds { get; set; }
            public string Description { get; set; }
            public string DisplayName { get; set; }
            public bool EnableDefaultInternetAccess { get; set; }
            public DomainJoinInfo DomainJoinInfo { get; set; }
        }

        public class StackRequest
        {
            public string StackName { get; set; }
            public string Description { get; set; }
            public string DisplayName { get; set; }
            public List<Amazon.AppStream.Model.StorageConnector> StorageConnectors { get; set; }
            public List<Amazon.AppStream.Model.UserSetting> UserSettings { get; set; }
        }

        public class FleetStackAssociationRequest
        {
            public string FleetName { get; set; }
            public string StackName { get; set; }
        }

        public class AppStreamUserRequest
        {
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AuthenticationType { get; set; }
        }
    }
}
