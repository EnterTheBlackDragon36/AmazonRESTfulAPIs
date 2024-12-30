using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineLearningController : ControllerBase
    {
        private readonly IAmazonSageMaker _sageMakerClient;

        public MachineLearningController(IAmazonSageMaker sageMakerClient)
        {
            _sageMakerClient = sageMakerClient;
        }

        // Training Jobs Endpoints
        [HttpPost("training-jobs")]
        public async Task<IActionResult> CreateTrainingJob([FromBody] TrainingJobRequest request)
        {
            try
            {
                var createRequest = new CreateTrainingJobRequest
                {
                    TrainingJobName = request.TrainingJobName,
                    AlgorithmSpecification = new Amazon.SageMaker.Model.AlgorithmSpecification
                    {
                        TrainingImage = request.AlgorithmImage,
                        TrainingInputMode = request.InputMode
                    },
                    RoleArn = request.RoleArn,
                    InputDataConfig = request.InputDataConfig,
                    OutputDataConfig = new Amazon.SageMaker.Model.OutputDataConfig
                    {
                        S3OutputPath = request.OutputS3Path
                    },
                    ResourceConfig = new Amazon.SageMaker.Model.ResourceConfig
                    {
                        InstanceType = request.InstanceType,
                        InstanceCount = request.InstanceCount,
                        VolumeSizeInGB = request.VolumeSizeInGB
                    },
                    StoppingCondition = new StoppingCondition
                    {
                        MaxRuntimeInSeconds = request.MaxRuntimeInSeconds
                    }
                };

                var response = await _sageMakerClient.CreateTrainingJobAsync(createRequest);
                return Ok(new { TrainingJobArn = response.TrainingJobArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("training-jobs")]
        public async Task<IActionResult> ListTrainingJobs()
        {
            try
            {
                var request = new ListTrainingJobsRequest();
                var response = await _sageMakerClient.ListTrainingJobsAsync(request);
                return Ok(response.TrainingJobSummaries);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Model Endpoints
        [HttpPost("models")]
        public async Task<IActionResult> CreateModel([FromBody] ModelRequest request)
        {
            try
            {
                var createRequest = new CreateModelRequest
                {
                    ModelName = request.ModelName,
                    PrimaryContainer = new ContainerDefinition
                    {
                        Image = request.ContainerImage,
                        ModelDataUrl = request.ModelDataUrl
                    },
                    ExecutionRoleArn = request.ExecutionRoleArn
                };

                var response = await _sageMakerClient.CreateModelAsync(createRequest);
                return Ok(new { ModelArn = response.ModelArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> ListModels()
        {
            try
            {
                var request = new ListModelsRequest();
                var response = await _sageMakerClient.ListModelsAsync(request);
                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoints Configuration
        [HttpPost("endpoint-configs")]
        public async Task<IActionResult> CreateEndpointConfig([FromBody] EndpointConfigRequest request)
        {
            try
            {
                var createRequest = new CreateEndpointConfigRequest
                {
                    EndpointConfigName = request.EndpointConfigName,
                    ProductionVariants = new List<ProductionVariant>
                    {
                        new ProductionVariant
                        {
                            ModelName = request.ModelName,
                            VariantName = request.VariantName,
                            InitialInstanceCount = request.InitialInstanceCount,
                            InstanceType = request.InstanceType,
                            InitialVariantWeight = (float)1.0
                        }
                    }
                };

                var response = await _sageMakerClient.CreateEndpointConfigAsync(createRequest);
                return Ok(new { EndpointConfigArn = response.EndpointConfigArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoints
        [HttpPost("endpoints")]
        public async Task<IActionResult> CreateEndpoint([FromBody] EndpointRequest request)
        {
            try
            {
                var createRequest = new Amazon.SageMaker.Model.CreateEndpointRequest
                {
                    EndpointName = request.EndpointName,
                    EndpointConfigName = request.EndpointConfigName
                };

                var response = await _sageMakerClient.CreateEndpointAsync(createRequest);
                return Ok(new { EndpointArn = response.EndpointArn });
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
                var request = new Amazon.SageMaker.Model.ListEndpointsRequest();
                var response = await _sageMakerClient.ListEndpointsAsync(request);
                return Ok(response.Endpoints);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("endpoints/{endpointName}")]
        public async Task<IActionResult> DeleteEndpoint(string endpointName)
        {
            try
            {
                var request = new Amazon.SageMaker.Model.DeleteEndpointRequest
                {
                    EndpointName = endpointName
                };
                await _sageMakerClient.DeleteEndpointAsync(request);
                return Ok($"Endpoint {endpointName} deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models for Request Validation
        public class TrainingJobRequest
        {
            public string TrainingJobName { get; set; }
            public string AlgorithmImage { get; set; }
            public string InputMode { get; set; }
            public string RoleArn { get; set; }
            public List<Channel> InputDataConfig { get; set; }
            public string OutputS3Path { get; set; }
            public string InstanceType { get; set; }
            public int InstanceCount { get; set; }
            public int VolumeSizeInGB { get; set; }
            public int MaxRuntimeInSeconds { get; set; }
        }

        public class ModelRequest
        {
            public string ModelName { get; set; }
            public string ContainerImage { get; set; }
            public string ModelDataUrl { get; set; }
            public string ExecutionRoleArn { get; set; }
        }

        public class EndpointConfigRequest
        {
            public string EndpointConfigName { get; set; }
            public string ModelName { get; set; }
            public string VariantName { get; set; }
            public int InitialInstanceCount { get; set; }
            public string InstanceType { get; set; }
        }

        public class EndpointRequest
        {
            public string EndpointName { get; set; }
            public string EndpointConfigName { get; set; }
        }
    }
}
