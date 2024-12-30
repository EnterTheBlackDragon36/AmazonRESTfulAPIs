using Amazon.CodeBuild;
using Amazon.CodeBuild.Model;
using Amazon.CodeCommit;
using Amazon.CodeCommit.Model;
using Amazon.CodeDeploy;
using Amazon.CodeDeploy.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperToolsController : ControllerBase
    {
        private readonly IAmazonCodeCommit _codeCommitClient;
        private readonly IAmazonCodeBuild _codeBuildClient;
        private readonly IAmazonCodeDeploy _codeDeployClient;

        public DeveloperToolsController(
            IAmazonCodeCommit codeCommitClient,
            IAmazonCodeBuild codeBuildClient,
            IAmazonCodeDeploy codeDeployClient)
        {
            _codeCommitClient = codeCommitClient;
            _codeBuildClient = codeBuildClient;
            _codeDeployClient = codeDeployClient;
        }

        // CodeCommit Endpoints
        [HttpPost("repositories")]
        public async Task<IActionResult> CreateRepository([FromBody] RepositoryRequest request)
        {
            try
            {
                var createRequest = new CreateRepositoryRequest
                {
                    RepositoryName = request.RepositoryName,
                    RepositoryDescription = request.Description,
                    Tags = request.Tags
                };

                var response = await _codeCommitClient.CreateRepositoryAsync(createRequest);
                return Ok(response.RepositoryMetadata);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> ListRepositories()
        {
            try
            {
                var request = new ListRepositoriesRequest();
                var response = await _codeCommitClient.ListRepositoriesAsync(request);
                return Ok(response.Repositories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("branches")]
        public async Task<IActionResult> CreateBranch([FromBody] BranchRequest request)
        {
            try
            {
                var createRequest = new Amazon.CodeCommit.Model.CreateBranchRequest
                {
                    RepositoryName = request.RepositoryName,
                    BranchName = request.BranchName,
                    CommitId = request.CommitId
                };

                await _codeCommitClient.CreateBranchAsync(createRequest);
                return Ok($"Branch {request.BranchName} created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // CodeBuild Endpoints
        [HttpPost("projects")]
        public async Task<IActionResult> CreateBuildProject([FromBody] BuildProjectRequest request)
        {
            try
            {
                var createRequest = new Amazon.CodeBuild.Model.CreateProjectRequest
                {
                    Name = request.ProjectName,
                    Description = request.Description,
                    Source = new ProjectSource
                    {
                        Type = request.SourceType,
                        Location = request.SourceLocation
                    },
                    Environment = new ProjectEnvironment
                    {
                        Type = request.EnvironmentType,
                        Image = request.EnvironmentImage,
                        ComputeType = request.ComputeType
                    },
                    ServiceRole = request.ServiceRole,
                    Artifacts = new ProjectArtifacts
                    {
                        Type = request.ArtifactsType,
                        Location = request.ArtifactsLocation
                    }
                };

                var response = await _codeBuildClient.CreateProjectAsync(createRequest);
                return Ok(response.Project);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("builds")]
        public async Task<IActionResult> StartBuild([FromBody] StartBuildRequest request)
        {
            try
            {
                var response = await _codeBuildClient.StartBuildAsync(request);
                return Ok(response.Build);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("builds/{projectName}")]
        public async Task<IActionResult> ListBuilds(string projectName)
        {
            try
            {
                var request = new ListBuildsForProjectRequest
                {
                    ProjectName = projectName
                };
                var response = await _codeBuildClient.ListBuildsForProjectAsync(request);
                return Ok(response.Ids);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // CodeDeploy Endpoints
        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] ApplicationRequest request)
        {
            try
            {
                var createRequest = new Amazon.CodeDeploy.Model.CreateApplicationRequest
                {
                    ApplicationName = request.ApplicationName,
                    ComputePlatform = request.ComputePlatform
                };

                var response = await _codeDeployClient.CreateApplicationAsync(createRequest);
                return Ok($"Application {request.ApplicationName} created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deployment-groups")]
        public async Task<IActionResult> CreateDeploymentGroup([FromBody] DeploymentGroupRequest request)
        {
            try
            {
                var createRequest = new CreateDeploymentGroupRequest
                {
                    ApplicationName = request.ApplicationName,
                    DeploymentGroupName = request.DeploymentGroupName,
                    ServiceRoleArn = request.ServiceRoleArn,
                    DeploymentStyle = new DeploymentStyle
                    {
                        DeploymentOption = request.DeploymentOption,
                        DeploymentType = request.DeploymentType
                    },
                    Ec2TagSet = request.Ec2TagSet
                };

                var response = await _codeDeployClient.CreateDeploymentGroupAsync(createRequest);
                return Ok(response.DeploymentGroupId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deployments")]
        public async Task<IActionResult> CreateDeployment([FromBody] DeploymentRequest request)
        {
            try
            {
                var createRequest = new Amazon.CodeDeploy.Model.CreateDeploymentRequest
                {
                    ApplicationName = request.ApplicationName,
                    DeploymentGroupName = request.DeploymentGroupName,
                    Revision = new RevisionLocation
                    {
                        RevisionType = request.RevisionType,
                        S3Location = request.S3Location
                    }
                };

                var response = await _codeDeployClient.CreateDeploymentAsync(createRequest);
                return Ok(response.DeploymentId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class RepositoryRequest
        {
            public string RepositoryName { get; set; }
            public string Description { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class BranchRequest
        {
            public string RepositoryName { get; set; }
            public string BranchName { get; set; }
            public string CommitId { get; set; }
        }

        public class BuildProjectRequest
        {
            public string ProjectName { get; set; }
            public string Description { get; set; }
            public string SourceType { get; set; }
            public string SourceLocation { get; set; }
            public string EnvironmentType { get; set; }
            public string EnvironmentImage { get; set; }
            public string ComputeType { get; set; }
            public string ServiceRole { get; set; }
            public string ArtifactsType { get; set; }
            public string ArtifactsLocation { get; set; }
        }

        public class ApplicationRequest
        {
            public string ApplicationName { get; set; }
            public string ComputePlatform { get; set; }
        }

        public class DeploymentGroupRequest
        {
            public string ApplicationName { get; set; }
            public string DeploymentGroupName { get; set; }
            public string ServiceRoleArn { get; set; }
            public string DeploymentOption { get; set; }
            public string DeploymentType { get; set; }
            public EC2TagSet Ec2TagSet { get; set; }
        }

        public class DeploymentRequest
        {
            public string ApplicationName { get; set; }
            public string DeploymentGroupName { get; set; }
            public string RevisionType { get; set; }
            public Amazon.CodeDeploy.Model.S3Location S3Location { get; set; }
        }
    }
}
