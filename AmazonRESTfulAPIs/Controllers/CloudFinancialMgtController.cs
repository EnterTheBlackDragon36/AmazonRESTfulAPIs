using Microsoft.AspNetCore.Mvc;
using Amazon.CloudWatch;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Amazon.Budgets;
using Amazon.Budgets.Model;
using Amazon.S3;
using Microsoft.VisualBasic;
using System;
using DateInterval = Amazon.CostExplorer.Model.DateInterval;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudFinancialMgtController : ControllerBase
    {
        private readonly IAmazonCostExplorer _costExplorer;
        private readonly IAmazonBudgets _budgetClient;

        public CloudFinancialMgtController(IAmazonCostExplorer costExplorer, IAmazonBudgets budgetClient)
        {
            _costExplorer = costExplorer;
            _budgetClient = budgetClient;
        }

        // Get Cost and Usage Data
        [HttpGet("costs")]
        public async Task<IActionResult> GetCostAndUsage([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var request = new GetCostAndUsageRequest
                {
                    TimePeriod = new DateInterval
                    {
                        Start = startDate.ToString("yyyy-MM-dd"),
                        End = endDate.ToString("yyyy-MM-dd")
                    },
                    Granularity = Granularity.MONTHLY,
                    Metrics = new List<string> { "UnblendedCost", "UsageQuantity" }
                };

                var response = await _costExplorer.GetCostAndUsageAsync(request);
                return Ok(response.ResultsByTime);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create Budget
        [HttpPost("budgets")]
        public async Task<IActionResult> CreateBudget([FromBody] BudgetRequest budgetRequest)
        {
            try
            {
                var request = new CreateBudgetRequest
                {
                    AccountId = budgetRequest.AccountId,
                    Budget = new Budget
                    {
                        BudgetName = budgetRequest.BudgetName,
                        BudgetLimit = new Spend
                        {
                            Amount = budgetRequest.Amount,
                            Unit = "USD"
                        },
                        TimeUnit = TimeUnit.MONTHLY,
                        BudgetType = BudgetType.COST
                    }
                };

                var response = await _budgetClient.CreateBudgetAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get Budget Information
        [HttpGet("budgets/{budgetName}")]
        public async Task<IActionResult> GetBudget(string budgetName, string accountId)
        {
            try
            {
                var request = new DescribeBudgetRequest
                {
                    AccountId = accountId,
                    BudgetName = budgetName
                };

                var response = await _budgetClient.DescribeBudgetAsync(request);
                return Ok(response.Budget);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get Cost Forecast
        [HttpGet("forecast")]
        public async Task<IActionResult> GetCostForecast()
        {
            try
            {
                var request = new GetCostForecastRequest
                {
                    TimePeriod = new DateInterval
                    {
                        Start = DateTime.Now.ToString("yyyy-MM-dd"),
                        End = DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd")
                    },
                    Metric = "UNBLENDED_COST",
                    Granularity = Granularity.MONTHLY
                };

                var response = await _costExplorer.GetCostForecastAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class BudgetRequest
    {
        public string AccountId { get; set; }
        public string BudgetName { get; set; }
        public decimal Amount { get; set; }
    }
}
