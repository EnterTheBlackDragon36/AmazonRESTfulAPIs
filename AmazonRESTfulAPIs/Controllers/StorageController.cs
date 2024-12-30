using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    public class StorageController : Controller
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private const string TableName = "YourTableName";
        private readonly IAmazonS3 _s3Client;
        public StorageController(IAmazonDynamoDB dynamoDbClient, IAmazonS3 s3Client)
        {
            _dynamoDbClient = dynamoDbClient;
            _s3Client = s3Client;
        }

        #region Dynamo DB
        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Dictionary<string, AttributeValue> item)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = item
            };

            try
            {
                await _dynamoDbClient.PutItemAsync(request);
                return Ok(new { Message = "Item created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // READ
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(string id)
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            };

            try
            {
                var response = await _dynamoDbClient.GetItemAsync(request);
                if (response.Item == null)
                    return NotFound();

                return Ok(response.Item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] Dictionary<string, AttributeValue> updates)
        {
            var expressionAttributeNames = new Dictionary<string, string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            var updateExpressions = new List<string>();

            foreach (var kvp in updates)
            {
                if (kvp.Key != "Id") // Skip the primary key
                {
                    string attributeName = $"#{kvp.Key}";
                    string attributeValue = $":{kvp.Key}";

                    expressionAttributeNames[attributeName] = kvp.Key;
                    expressionAttributeValues[attributeValue] = kvp.Value;
                    updateExpressions.Add($"{attributeName} = {attributeValue}");
                }
            }

            var request = new UpdateItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                },
                UpdateExpression = $"SET {string.Join(", ", updateExpressions)}",
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues
            };

            try
            {
                await _dynamoDbClient.UpdateItemAsync(request);
                return Ok(new { Message = "Item updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            var request = new DeleteItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            };

            try
            {
                await _dynamoDbClient.DeleteItemAsync(request);
                return Ok(new { Message = "Item deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // LIST ALL
        [HttpGet]
        public async Task<IActionResult> ListItems()
        {
            var request = new ScanRequest
            {
                TableName = TableName
            };

            try
            {
                var response = await _dynamoDbClient.ScanAsync(request);
                return Ok(response.Items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public IActionResult Index()
        {
            return View();
        }



        #endregion

        #region Storage Buckets
        // CREATE Bucket
        [HttpPost("bucket/{bucketName}")]
        public async Task<IActionResult> CreateBucket(string bucketName)
        {
            try
            {
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                await _s3Client.PutBucketAsync(putBucketRequest);
                return Ok($"Bucket {bucketName} created successfully");
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error creating bucket: {ex.Message}");
            }
        }

        // CREATE Object
        [HttpPost("bucket/{bucketName}/object")]
        public async Task<IActionResult> UploadFile(string bucketName, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = file.FileName,
                        InputStream = memoryStream,
                        ContentType = file.ContentType
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                    return Ok($"File {file.FileName} uploaded successfully");
                }
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error uploading file: {ex.Message}");
            }
        }

        // READ Bucket List
        [HttpGet("buckets")]
        public async Task<IActionResult> ListBuckets()
        {
            try
            {
                var response = await _s3Client.ListBucketsAsync();
                var buckets = response.Buckets.Select(b => new
                {
                    b.BucketName,
                    b.CreationDate
                });

                return Ok(buckets);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error listing buckets: {ex.Message}");
            }
        }

        // READ Objects in Bucket
        [HttpGet("bucket/{bucketName}/objects")]
        public async Task<IActionResult> ListObjects(string bucketName)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };

                var response = await _s3Client.ListObjectsV2Async(request);
                var objects = response.S3Objects.Select(obj => new
                {
                    obj.Key,
                    obj.Size,
                    obj.LastModified
                });

                return Ok(objects);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error listing objects: {ex.Message}");
            }
        }

        // READ (Download) Object
        [HttpGet("bucket/{bucketName}/object/{objectKey}")]
        public async Task<IActionResult> DownloadFile(string bucketName, string objectKey)
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(bucketName, objectKey);

                return File(response.ResponseStream, response.Headers.ContentType, objectKey);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error downloading file: {ex.Message}");
            }
        }

        // UPDATE Object (Copy with new metadata)
        [HttpPut("bucket/{bucketName}/object/{objectKey}")]
        public async Task<IActionResult> UpdateObjectMetadata(string bucketName, string objectKey,
            [FromBody] Dictionary<string, string> metadata)
        {
            try
            {
                // First, copy the object to itself with new metadata
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = objectKey,
                    DestinationBucket = bucketName,
                    DestinationKey = objectKey,
                    MetadataDirective = S3MetadataDirective.REPLACE
                };

                foreach (var item in metadata)
                {
                    copyRequest.Metadata[item.Key] = item.Value;
                }

                await _s3Client.CopyObjectAsync(copyRequest);
                return Ok($"Object {objectKey} metadata updated successfully");
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error updating object metadata: {ex.Message}");
            }
        }

        // DELETE Object
        [HttpDelete("bucket/{bucketName}/object/{objectKey}")]
        public async Task<IActionResult> DeleteObject(string bucketName, string objectKey)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return Ok($"Object {objectKey} deleted successfully");
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error deleting object: {ex.Message}");
            }
        }

        // DELETE Bucket
        [HttpDelete("bucket/{bucketName}")]
        public async Task<IActionResult> DeleteBucket(string bucketName)
        {
            try
            {
                // First, ensure the bucket is empty
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };
                var listResponse = await _s3Client.ListObjectsV2Async(listRequest);

                if (listResponse.S3Objects.Any())
                {
                    return BadRequest("Bucket must be empty before deletion");
                }

                await _s3Client.DeleteBucketAsync(bucketName);
                return Ok($"Bucket {bucketName} deleted successfully");
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"Error deleting bucket: {ex.Message}");
            }
        }
        #endregion
    }
}
