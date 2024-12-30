using Amazon.Amplify;
using Amazon.Amplify.Model;
using Amazon.AppSync;
using Amazon.AppSync.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Mvc;


namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrontEndMobileController : ControllerBase
    {
        private readonly IAmazonAmplify _amplifyClient;
        private readonly IAmazonAppSync _appSyncClient;
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;

        public FrontEndMobileController(
            IAmazonAmplify amplifyClient,
            IAmazonAppSync appSyncClient,
            IAmazonCognitoIdentityProvider cognitoClient)
        {
            _amplifyClient = amplifyClient;
            _appSyncClient = appSyncClient;
            _cognitoClient = cognitoClient;
        }

        // Amplify Apps
        [HttpPost("apps")]
        public async Task<IActionResult> CreateApp([FromBody] AmplifyAppRequest request)
        {
            try
            {
                var createRequest = new CreateAppRequest
                {
                    Name = request.Name,
                    Repository = request.Repository,
                    Platform = request.Platform,
                    Description = request.Description,
                    EnvironmentVariables = request.EnvironmentVariables
                };

                var response = await _amplifyClient.CreateAppAsync(createRequest);
                return Ok(response.App);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("apps")]
        public async Task<IActionResult> ListApps()
        {
            try
            {
                var request = new ListAppsRequest();
                var response = await _amplifyClient.ListAppsAsync(request);
                return Ok(response.Apps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Amplify Branches
        [HttpPost("apps/{appId}/branches")]
        public async Task<IActionResult> CreateBranch(string appId, [FromBody] BranchRequest request)
        {
            try
            {
                var createRequest = new CreateBranchRequest
                {
                    AppId = appId,
                    BranchName = request.BranchName,
                    Description = request.Description,
                    Stage = request.Stage,
                    EnvironmentVariables = request.EnvironmentVariables
                };

                var response = await _amplifyClient.CreateBranchAsync(createRequest);
                return Ok(response.Branch);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // AppSync APIs
        [HttpPost("graphql-apis")]
        public async Task<IActionResult> CreateGraphqlApi([FromBody] GraphqlApiRequest request)
        {
            try
            {
                var createRequest = new CreateGraphqlApiRequest
                {
                    Name = request.Name,
                    AuthenticationType = request.AuthenticationType,
                    UserPoolConfig = request.UserPoolConfig
                };

                var response = await _appSyncClient.CreateGraphqlApiAsync(createRequest);
                return Ok(response.GraphqlApi);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("graphql-apis/{apiId}/schemas")]
        public async Task<IActionResult> UpdateGraphqlSchema(string apiId, [FromBody] SchemaRequest request)
        {
            try
            {
                var updateRequest = new StartSchemaCreationRequest
                {
                    ApiId = apiId,
                    Definition = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(request.Schema))
                };

                await _appSyncClient.StartSchemaCreationAsync(updateRequest);
                return Ok("Schema update initiated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Cognito User Pools
        [HttpPost("user-pools")]
        public async Task<IActionResult> CreateUserPool([FromBody] UserPoolRequest request)
        {
            try
            {
                var createRequest = new CreateUserPoolRequest
                {
                    PoolName = request.PoolName,
                    Policies = request.Policies,
                    AutoVerifiedAttributes = request.AutoVerifiedAttributes,
                    UsernameAttributes = request.UsernameAttributes,
                    Schema = request.Schema
                };

                var response = await _cognitoClient.CreateUserPoolAsync(createRequest);
                return Ok(new { UserPoolId = response.UserPool.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("user-pools/{userPoolId}/clients")]
        public async Task<IActionResult> CreateUserPoolClient(string userPoolId, [FromBody] UserPoolClientRequest request)
        {
            try
            {
                var createRequest = new CreateUserPoolClientRequest
                {
                    UserPoolId = userPoolId,
                    ClientName = request.ClientName,
                    GenerateSecret = request.GenerateSecret,
                    AllowedOAuthFlows = request.AllowedOAuthFlows,
                    AllowedOAuthScopes = request.AllowedOAuthScopes,
                    CallbackURLs = request.CallbackUrls,
                    LogoutURLs = request.LogoutUrls
                };

                var response = await _cognitoClient.CreateUserPoolClientAsync(createRequest);
                return Ok(response.UserPoolClient);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Deployments
        [HttpPost("apps/{appId}/deployments")]
        public async Task<IActionResult> CreateDeployment(string appId, [FromBody] DeploymentRequest request)
        {
            try
            {
                var createRequest = new StartDeploymentRequest
                {
                    AppId = appId,
                    BranchName = request.BranchName
                };

                var response = await _amplifyClient.StartDeploymentAsync(createRequest);
                return Ok(response.JobSummary);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class AmplifyAppRequest
        {
            public string Name { get; set; }
            public string Repository { get; set; }
            public string Platform { get; set; }
            public string Description { get; set; }
            public Dictionary<string, string> EnvironmentVariables { get; set; }
        }

        public class BranchRequest
        {
            public string BranchName { get; set; }
            public string Description { get; set; }
            public string Stage { get; set; }
            public Dictionary<string, string> EnvironmentVariables { get; set; }
        }

        public class GraphqlApiRequest
        {
            public string Name { get; set; }
            public string AuthenticationType { get; set; }
            public UserPoolConfig UserPoolConfig { get; set; }
        }

        public class SchemaRequest
        {
            public string Schema { get; set; }
        }

        public class UserPoolRequest
        {
            public string PoolName { get; set; }
            public UserPoolPolicyType Policies { get; set; }
            public List<string> AutoVerifiedAttributes { get; set; }
            public List<string> UsernameAttributes { get; set; }
            public List<SchemaAttributeType> Schema { get; set; }
        }

        public class UserPoolClientRequest
        {
            public string ClientName { get; set; }
            public bool GenerateSecret { get; set; }
            public List<string> AllowedOAuthFlows { get; set; }
            public List<string> AllowedOAuthScopes { get; set; }
            public List<string> CallbackUrls { get; set; }
            public List<string> LogoutUrls { get; set; }
        }

        public class DeploymentRequest
        {
            public string BranchName { get; set; }
        }
    }
}
