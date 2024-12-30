using Amazon.ECR;
using Amazon.ECR.Model;
using Amazon.ECS;
using Amazon.ECS.Model;
using Amazon.EKS;
using Amazon.EKS.Model;
using Microsoft.AspNetCore.Mvc;
using CreateClusterRequest = Amazon.ECS.Model.CreateClusterRequest;
using CreateRepositoryRequest = Amazon.ECR.Model.CreateRepositoryRequest;
using DescribeClusterRequest = Amazon.EKS.Model.DescribeClusterRequest;
using ListClustersRequest = Amazon.EKS.Model.ListClustersRequest;


namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainersController : ControllerBase
    {
        private readonly IAmazonECS _ecsClient;
        private readonly IAmazonECR _ecrClient;
        private readonly IAmazonEKS _eksClient;

        public ContainersController(
            IAmazonECS ecsClient,
            IAmazonECR ecrClient,
            IAmazonEKS eksClient)
        {
            _ecsClient = ecsClient;
            _ecrClient = ecrClient;
            _eksClient = eksClient;
        }

        #region ECS Operations

        // Create ECS Cluster
        [HttpPost("clusters")]
        public async Task<IActionResult> CreateCluster([FromBody] CreateClusterRequest request)
        {
            try
            {
                var response = await _ecsClient.CreateClusterAsync(new CreateClusterRequest
                {
                    ClusterName = request.ClusterName,
                    CapacityProviders = request.CapacityProviders
                    //Tags = request.Tags?.Select(t => new Tag { Key = t.Key, Value = t.Value }).ToList()
                });

                return Ok(response.Cluster);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // List ECS Clusters
        [HttpGet("clusters")]
        public async Task<IActionResult> ListClusters()
        {
            try
            {
                var response = await _ecsClient.ListClustersAsync(new Amazon.ECS.Model.ListClustersRequest());
                var clusters = await _ecsClient.DescribeClustersAsync(new DescribeClustersRequest
                {
                    Clusters = response.ClusterArns
                });

                return Ok(clusters.Clusters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create ECS Service
        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
        {
            try
            {
                var response = await _ecsClient.CreateServiceAsync(request);
                return Ok(response.Service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // List Tasks in Cluster
        [HttpGet("clusters/{clusterName}/tasks")]
        public async Task<IActionResult> ListTasks(string clusterName)
        {
            try
            {
                var response = await _ecsClient.ListTasksAsync(new ListTasksRequest
                {
                    Cluster = clusterName
                });

                var tasks = await _ecsClient.DescribeTasksAsync(new DescribeTasksRequest
                {
                    Cluster = clusterName,
                    Tasks = response.TaskArns
                });

                return Ok(tasks.Tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region ECR Operations

        // Create ECR Repository
        [HttpPost("repositories")]
        public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryRequest request)
        {
            try
            {
                var response = await _ecrClient.CreateRepositoryAsync(new CreateRepositoryRequest
                {
                    RepositoryName = request.RepositoryName,
                    ImageScanningConfiguration = new ImageScanningConfiguration { ScanOnPush = true },
                    EncryptionConfiguration = new EncryptionConfiguration { EncryptionType = "AES256" }
                });

                return Ok(response.Repository);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // List ECR Repositories
        [HttpGet("repositories")]
        public async Task<IActionResult> ListRepositories()
        {
            try
            {
                var response = await _ecrClient.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());
                return Ok(response.Repositories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get ECR Authorization Token
        [HttpGet("auth-token")]
        public async Task<IActionResult> GetAuthorizationToken()
        {
            try
            {
                var response = await _ecrClient.GetAuthorizationTokenAsync(new GetAuthorizationTokenRequest());
                return Ok(response.AuthorizationData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region EKS Operations

        // Create EKS Cluster
        [HttpPost("eks/clusters")]
        public async Task<IActionResult> CreateEksCluster([FromBody] CreateEksClusterRequest request)
        {
            try
            {
                var response = await _eksClient.CreateClusterAsync(new Amazon.EKS.Model.CreateClusterRequest
                {
                    Name = request.ClusterName,
                    RoleArn = request.RoleArn,
                    ResourcesVpcConfig = new VpcConfigRequest
                    {
                        SubnetIds = request.SubnetIds,
                        SecurityGroupIds = request.SecurityGroupIds
                    },
                    Version = request.KubernetesVersion
                });

                return Ok(response.Cluster);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // List EKS Clusters
        [HttpGet("eks/clusters")]
        public async Task<IActionResult> ListEksClusters()
        {
            try
            {
                var response = await _eksClient.ListClustersAsync(new ListClustersRequest());
                return Ok(response.Clusters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get EKS Cluster Details
        [HttpGet("eks/clusters/{clusterName}")]
        public async Task<IActionResult> GetEksCluster(string clusterName)
        {
            try
            {
                var response = await _eksClient.DescribeClusterAsync(new DescribeClusterRequest
                {
                    Name = clusterName
                });

                return Ok(response.Cluster);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }

    // Request Models
    //public class CreateClusterRequest
    //{
    //    public string ClusterName { get; set; }
    //    public List<string> CapacityProviders { get; set; }
    //    public Dictionary<string, string> Tags { get; set; }
    //}

    public class CreateEksClusterRequest
    {
        public string ClusterName { get; set; }
        public string RoleArn { get; set; }
        public List<string> SubnetIds { get; set; }
        public List<string> SecurityGroupIds { get; set; }
        public string KubernetesVersion { get; set; }
    }
}
