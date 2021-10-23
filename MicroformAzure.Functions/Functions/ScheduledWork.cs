
using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using MicroformAzure.Functions.Models;

namespace MicroformAzure.Functions.Functions
{
    public class ScheduledWork
    {
        
        private readonly MicroformAzureContext _context;        
        private readonly IApplicationLogsService _BdLog;

        public IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public ScheduledWork(
            MicroformAzureContext context,
            IApplicationLogsService BdLog,
            IConfiguration configuration)
        {            
            _context = context;            
            _BdLog = BdLog;
            Configuration = configuration;
            UrlFunctions = Configuration["UrlFunctions"];
            ILogger log;
        }


        [FunctionName("TimerTrigger")]
        public async Task RunAsync([TimerTrigger("%TimerIntervalTrigger%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");            

            try
            {
                var res = await ScheduledPaymentsDo( log);

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error executing  TimerTrigger function");
            }

        }


        private async Task<string> ScheduledPaymentsDo( ILogger log)
        {
            log.LogInformation($"ScheduledPaymentsDo Task executed at: {DateTime.Now}");                    
  
            try
            {
                List<ScheduledPaymentsEntity> lspe = await _context.ScheduledPayments.Where(c => c.Status != "Finished").ToListAsync();

                foreach (ScheduledPaymentsEntity spe in lspe) {
                    if (spe.Frequency == "D") 
                    { 
                         TimeSpan dif = DateTime.UtcNow - spe.LastExecution;
                         var hours= dif.TotalHours;
                         if (hours > 23){
                             await UpdateScheduledPayment(spe, log);
                            log.LogInformation($"Need call autopayment at: {DateTime.Now}  Frequency D");
                            return null;
                            
                         }                                                
                    }

                    if (spe.Frequency == "M")
                    {
                        TimeSpan dif = DateTime.UtcNow - spe.LastExecution;
                        var days = dif.TotalDays;
                        if (days > 30){
                            await UpdateScheduledPayment(spe, log);
                            log.LogInformation($"Need call autopayment at: {DateTime.Now}  Frequency M");
                        }
                    }
                    if (spe.Frequency == "E" && spe.Status == "New")
                    {
                        if (DateTime.UtcNow > spe.ExecutionExact)                                                
                        {                           
                            try
                            {
                                try
                                {
                                    
                                    using HttpClient httpClient = new HttpClient();
                                    UrlFunctions = Configuration["UrlFunctions"];
                                    string url = $"{UrlFunctions}/api/PaymentWithFlexTokenCreatePermanentTMSToken";
                                    var content = new StringContent(spe.Param1.ToString(), Encoding.UTF8, "application/json");                                    

                                    using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                                    string responseStream = await response.Content.ReadAsStringAsync();
                                    GenericResponse<PaymentResponseDTO> result = JsonConvert
                                        .DeserializeObject<GenericResponse<PaymentResponseDTO>>(responseStream);

                                    if (result!=null)
                                    {
                                        if (result.IsSuccess)
                                        {
                                            log.LogInformation($"Shceduled PaymentWithFlexTokenCreatePermanentTMSToken Success response at {DateTime.UtcNow}");
                                            await _BdLog.Log(_context, $"Shceduled PaymentWithFlexTokenCreatePermanentTMSToken Success response at {DateTime.UtcNow}", 0);
                                        }
                                        else
                                        {
                                            string message = $"NOT AUTHORIZED response: {result.Message}";
                                            log.LogError($"{message} at {DateTime.UtcNow}");

                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex}");
                                    throw new Exception(message: ex.Message.ToString());
                                }

                                await UpdateScheduledPayment(spe, log);
                                

                            }

                            catch (Exception ex)
                            {
                                await _BdLog.Log(_context, "Process: PaymentRequestFunction" + ex.Message, 0);
                            }

                        }
                    }

                }               
            }
            catch (Exception ex)
            {
                await _BdLog.Log(_context, "Process: PaymentRequestFunction" + ex.Message, 0);
            }
            return null;
                        
        }



            public async Task<int> UpdateScheduledPayment(ScheduledPaymentsEntity spe, ILogger log)
        {
            try {

            using IDbContextTransaction transaction = _context.Database.BeginTransaction();

            _context.ScheduledPayments.Attach(spe);
            spe.LastExecution = DateTime.UtcNow;
            spe.Status = "Updated";
            _context.Entry(spe).Property(x => x.LastExecution).IsModified = true;
            _context.Entry(spe).Property(x => x.Status).IsModified = true;
            await _BdLog.Log(_context, "Process: PaymentRequestFunction. ApplicationPayerTokenID: ", spe.Id);

            await _context.SaveChangesAsync();
            transaction.Commit();

                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                    log.LogError(e.InnerException?.Message);
                    log.LogError(e.InnerException?.InnerException?.Message);                    
                }

            return 0;
        }


        [FunctionName(nameof(ScheduledWorkInsert))]
        public async Task<IActionResult> ScheduledWorkInsert(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ScheduledWorkInsert")] HttpRequest req,
          ILogger log)
        {

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ScheduledPaymentsEntity spe;

                try
                {
                    spe = JsonConvert.DeserializeObject<ScheduledPaymentsEntity>(requestBody);

                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                    log.LogError(e.InnerException?.Message);
                    log.LogError(e.InnerException?.InnerException?.Message);

                    return new BadRequestObjectResult(e);
                }


                using IDbContextTransaction transaction = _context.Database.BeginTransaction();

                _context.ScheduledPayments.Attach(spe);                
                spe.Status = "New";
                spe.Frequency = "E";
                spe.Param1 = spe.Param1;
                spe.Param2 = spe.Param2;
                spe.Param3 = spe.Param3;
                _context.Entry(spe).Property(x => x.ExecutionExact).IsModified = true;
                _context.Entry(spe).Property(x => x.Frequency).IsModified = true;
                _context.Entry(spe).Property(x => x.Status).IsModified = true;
                _context.Entry(spe).Property(x => x.Param1).IsModified = true;
                _context.Entry(spe).Property(x => x.Param2).IsModified = true;
                _context.Entry(spe).Property(x => x.Param3).IsModified = true;
                await _BdLog.Log(_context, "Process: ScheduledWorkInsert. ApplicationPayerTokenID: ", spe.Id);

                log.LogInformation($"ScheduledWorkInsert at: {DateTime.Now}  Frequency E");

                await _context.SaveChangesAsync();
                transaction.Commit();


                ApplicationPayerInfoEntity apie = await _context
                  .ApplicationPayerInfo
                  .Include(a => a.ApplicationRequests)
                  .Where(x => x.AccessCode == spe.Param2)                  
                  .FirstOrDefaultAsync();

                var are = apie.ApplicationRequests.Where(x => x.ReferenceId == spe.Param2).FirstOrDefault();
                if (are!=null){
                    return new OkObjectResult(are.UrlRedirectAfterPayment);
                }
                else{
                    return new OkResult();
                }                
            }
            catch (Exception ex)
            {
                await _BdLog.Log(_context, "Process: ScheduledWorkInsert" + ex.Message, 0);
            }
            return null;

        }



    }
}
