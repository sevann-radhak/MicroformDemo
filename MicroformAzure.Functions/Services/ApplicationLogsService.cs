using CyberSource.Api;
using CyberSource.Model;
using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Interface;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace MicroformAzure.Functions.Services
{
    public class ApplicationLogsService : IApplicationLogsService
    {
     
        private MicroformAzureContext _context;
       
   
        public async Task<int> Log(            
            MicroformAzureContext context, string Message, int RelatedId)
        {
            
            _context = context;

            ApplicationLogsEntity _log = new ApplicationLogsEntity();
            _log.Message = Message;
            _log.RelatedEventId = RelatedId;
            await _context.ApplicationLogs.AddAsync(_log);

            return 0;
        }




    }
}