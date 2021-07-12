using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class HttpTriggerCSharp1
    {
        [FunctionName("HttpTriggerCSharp1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response. " + Environment.GetEnvironmentVariable("AzureSqlConnectionString")
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetPartyId")]
        public static async Task<IActionResult> GetPartyId([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
        ILogger log)
        {
            string id = "";
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = data.matchId;
            if(string.IsNullOrWhiteSpace(id))
            {
                return new BadRequestObjectResult("No matchId provided");
            }
            using(SqlConnection con = GetConnection())
            {
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = string.Format("SELECT partyId FROM ttt_parties WHERE matchId='{0}';", id);
                using(SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if(await reader.ReadAsync())
                        return new OkObjectResult(reader.GetString(0));
                }
            }
            return new NotFoundObjectResult(null);
        }

        private static SqlConnection GetConnection()
        {
            SqlConnection con = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQLConnectionString"));
            con.Open();
            return con;
        }
    }
}
