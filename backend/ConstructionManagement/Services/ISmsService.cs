namespace ConstructionManagement.Services
{
    public interface ISmsService
    {
        Task SendAsync(string toPhoneNumber, string body, CancellationToken cancellationToken = default);
    }
}
