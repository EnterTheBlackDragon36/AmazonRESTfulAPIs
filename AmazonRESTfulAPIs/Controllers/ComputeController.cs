using Microsoft.AspNetCore.Mvc;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Amazon.ECS;
using Amazon.ECS.Model;
using Amazon.SageMaker.Model;
using Amazon.SQS;
using LaunchTemplateSpecification = Amazon.AutoScaling.Model.LaunchTemplateSpecification;
using CreateClusterRequest = Amazon.ECS.Model.CreateClusterRequest;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputeController : ControllerBase
    {
        private readonly IAmazonEC2 _ec2Client;
        private readonly IAmazonAutoScaling _autoScalingClient;
        private readonly IAmazonECS _ecsClient;

        public ComputeController(
            IAmazonEC2 ec2Client,
            IAmazonAutoScaling autoScalingClient,
            IAmazonECS ecsClient)
        {
            _ec2Client = ec2Client;
            _autoScalingClient = autoScalingClient;
            _ecsClient = ecsClient;
        }

        #region EC2 Operations

        // Get all EC2 instances
        [HttpGet("ec2/instances")]
        public async Task<IActionResult> GetEC2Instances()
        {
            try
            {
                var request = new DescribeInstancesRequest();
                var response = await _ec2Client.DescribeInstancesAsync(request);

                var instances = response.Reservations
                    .SelectMany(r => r.Instances)
                    .ToList();

                return Ok(instances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Launch new EC2 instance
        [HttpPost("ec2/instances")]
        public async Task<IActionResult> LaunchEC2Instance([FromBody] LaunchInstanceRequest request)
        {
            try
            {
                var launchRequest = new RunInstancesRequest
                {
                    ImageId = request.ImageId,
                    InstanceType = request.InstanceType,
                    MinCount = 1,
                    MaxCount = 1,
                    SecurityGroupIds = request.SecurityGroupIds,
                    SubnetId = request.SubnetId
                };

                var response = await _ec2Client.RunInstancesAsync(launchRequest);
                return Ok(response.ContentLength);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Stop EC2 instance
        [HttpPost("ec2/instances/{instanceId}/stop")]
        public async Task<IActionResult> StopEC2Instance(string instanceId)
        {
            try
            {
                var request = new StopInstancesRequest
                {
                    InstanceIds = new List<string> { instanceId }
                };

                var response = await _ec2Client.StopInstancesAsync(request);
                return Ok(response.StoppingInstances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Auto Scaling Operations

        // Create Auto Scaling Group
        [HttpPost("autoscaling/groups")]
        public async Task<IActionResult> CreateAutoScalingGroup([FromBody] CreateAutoScalingGroupRequest request)
        {
            try
            {
                var createRequest = new Amazon.AutoScaling.Model.CreateAutoScalingGroupRequest
                {
                    AutoScalingGroupName = request.AutoScalingGroupName,
                    LaunchTemplate = new LaunchTemplateSpecification
                    {
                        LaunchTemplateId = request.LaunchTemplateId,
                        Version = request.LaunchTemplateVersion
                    },
                    MinSize = request.MinSize,
                    MaxSize = request.MaxSize,
                    DesiredCapacity = request.DesiredCapacity,
                    VPCZoneIdentifier = request.SubnetIds
                };

                await _autoScalingClient.CreateAutoScalingGroupAsync(createRequest);
                return Ok("Auto Scaling Group created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get Auto Scaling Groups
        [HttpGet("autoscaling/groups")]
        public async Task<IActionResult> GetAutoScalingGroups()
        {
            try
            {
                var request = new DescribeAutoScalingGroupsRequest();
                var response = await _autoScalingClient.DescribeAutoScalingGroupsAsync(request);
                return Ok(response.AutoScalingGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region ECS Operations

        // Create ECS Cluster
        [HttpPost("ecs/clusters")]
        public async Task<IActionResult> CreateECSCluster([FromBody] CreateClusterRequest request)
        {
            try
            {
                var response = await _ecsClient.CreateClusterAsync(request);
                return Ok(response.Cluster);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get ECS Clusters
        [HttpGet("ecs/clusters")]
        public async Task<IActionResult> GetECSClusters()
        {
            try
            {
                var request = new DescribeClustersRequest();
                var response = await _ecsClient.DescribeClustersAsync(request);
                return Ok(response.Clusters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create ECS Service
        [HttpPost("ecs/services")]
        public async Task<IActionResult> CreateECSService([FromBody] CreateServiceRequest request)
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

        #endregion
    }

    // Request Models
    public class LaunchInstanceRequest
    {
        public string ImageId { get; set; }
        public InstanceType InstanceType { get; set; }
        public List<string> SecurityGroupIds { get; set; }
        public string SubnetId { get; set; }
    }

    public class CreateAutoScalingGroupRequest
    {
        public string AutoScalingGroupName { get; set; }
        public string LaunchTemplateId { get; set; }
        public string LaunchTemplateVersion { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public int DesiredCapacity { get; set; }
        public string SubnetIds { get; set; }
    }
}
