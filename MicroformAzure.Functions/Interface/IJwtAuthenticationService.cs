namespace MicroformAzure.Functions.Interface
{
    public interface IJwtAuthenticationService
    {
        public string Authenticate(string applicationName, string applicationKey);
    }
}
