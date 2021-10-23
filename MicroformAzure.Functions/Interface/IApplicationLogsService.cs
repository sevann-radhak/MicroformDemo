using System.Threading.Tasks;

namespace MicroformAzure.Functions.Interface
{
    public interface IApplicationLogsService
    {
        public Task<int> Log(MicroformAzureContext context, string Message, int RelatedId);
    }
}
