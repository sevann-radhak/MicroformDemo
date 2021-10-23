namespace MicroformAzure.Web.Models
{
    public class GenericResponse<T> where T : class
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; }
        public T Result { get; set; }
    }
}
