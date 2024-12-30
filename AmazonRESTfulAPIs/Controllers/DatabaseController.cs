using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamoDBController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DynamoDBController(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }

        // Table Operations
        [HttpPost("tables")]
        public async Task<IActionResult> CreateTable([FromBody] TableRequest request)
        {
            try
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = request.TableName,
                    AttributeDefinitions = request.AttributeDefinitions,
                    KeySchema = request.KeySchema,
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = request.ReadCapacityUnits,
                        WriteCapacityUnits = request.WriteCapacityUnits
                    }
                };

                if (request.GlobalSecondaryIndexes?.Count > 0)
                {
                    createRequest.GlobalSecondaryIndexes = request.GlobalSecondaryIndexes;
                }

                var response = await _dynamoDbClient.CreateTableAsync(createRequest);
                return Ok(response.TableDescription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tables")]
        public async Task<IActionResult> ListTables()
        {
            try
            {
                var request = new ListTablesRequest();
                var response = await _dynamoDbClient.ListTablesAsync(request);
                return Ok(response.TableNames);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tables/{tableName}")]
        public async Task<IActionResult> DescribeTable(string tableName)
        {
            try
            {
                var request = new DescribeTableRequest
                {
                    TableName = tableName
                };
                var response = await _dynamoDbClient.DescribeTableAsync(request);
                return Ok(response.Table);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("tables/{tableName}")]
        public async Task<IActionResult> DeleteTable(string tableName)
        {
            try
            {
                var request = new DeleteTableRequest
                {
                    TableName = tableName
                };
                var response = await _dynamoDbClient.DeleteTableAsync(request);
                return Ok($"Table {tableName} deletion initiated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Item Operations
        [HttpPost("tables/{tableName}/items")]
        public async Task<IActionResult> PutItem(string tableName, [FromBody] Dictionary<string, AttributeValue> item)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = item
                };

                var response = await _dynamoDbClient.PutItemAsync(request);
                return Ok(response.Attributes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tables/{tableName}/items/{partitionKey}/{sortKey?}")]
        public async Task<IActionResult> GetItem(string tableName, string partitionKey, string sortKey = null)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue { S = partitionKey } }
                };

                if (!string.IsNullOrEmpty(sortKey))
                {
                    key.Add("SK", new AttributeValue { S = sortKey });
                }

                var request = new GetItemRequest
                {
                    TableName = tableName,
                    Key = key
                };

                var response = await _dynamoDbClient.GetItemAsync(request);
                return Ok(response.Item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("tables/{tableName}/items")]
        public async Task<IActionResult> DeleteItem(string tableName, [FromBody] Dictionary<string, AttributeValue> key)
        {
            try
            {
                var request = new DeleteItemRequest
                {
                    TableName = tableName,
                    Key = key
                };

                var response = await _dynamoDbClient.DeleteItemAsync(request);
                return Ok("Item deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Query Operations
        [HttpPost("tables/{tableName}/query")]
        public async Task<IActionResult> QueryTable(string tableName, [FromBody] QueryRequest queryRequest)
        {
            try
            {
                queryRequest.TableName = tableName;
                var response = await _dynamoDbClient.QueryAsync(queryRequest);
                return Ok(new
                {
                    Items = response.Items,
                    Count = response.Count,
                    ScannedCount = response.ScannedCount,
                    LastEvaluatedKey = response.LastEvaluatedKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Scan Operations
        [HttpPost("tables/{tableName}/scan")]
        public async Task<IActionResult> ScanTable(string tableName, [FromBody] ScanRequest scanRequest)
        {
            try
            {
                scanRequest.TableName = tableName;
                var response = await _dynamoDbClient.ScanAsync(scanRequest);
                return Ok(new
                {
                    Items = response.Items,
                    Count = response.Count,
                    ScannedCount = response.ScannedCount,
                    LastEvaluatedKey = response.LastEvaluatedKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Batch Operations
        [HttpPost("batch-write")]
        public async Task<IActionResult> BatchWriteItems([FromBody] BatchWriteItemRequest request)
        {
            try
            {
                var response = await _dynamoDbClient.BatchWriteItemAsync(request);
                return Ok(new
                {
                    UnprocessedItems = response.UnprocessedItems,
                    ConsumedCapacity = response.ConsumedCapacity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("batch-get")]
        public async Task<IActionResult> BatchGetItems([FromBody] BatchGetItemRequest request)
        {
            try
            {
                var response = await _dynamoDbClient.BatchGetItemAsync(request);
                return Ok(new
                {
                    Responses = response.Responses,
                    UnprocessedKeys = response.UnprocessedKeys,
                    ConsumedCapacity = response.ConsumedCapacity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class TableRequest
        {
            public string TableName { get; set; }
            public List<AttributeDefinition> AttributeDefinitions { get; set; }
            public List<KeySchemaElement> KeySchema { get; set; }
            public long ReadCapacityUnits { get; set; }
            public long WriteCapacityUnits { get; set; }
            public List<GlobalSecondaryIndex> GlobalSecondaryIndexes { get; set; }
        }

        public class QueryParameters
        {
            public string KeyConditionExpression { get; set; }
            public Dictionary<string, AttributeValue> ExpressionAttributeValues { get; set; }
            public string FilterExpression { get; set; }
            public string IndexName { get; set; }
        }
    }
}
