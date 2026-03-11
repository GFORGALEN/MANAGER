using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using ConstructionManagement.Configurations;
using Microsoft.Extensions.Options;

namespace ConstructionManagement.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsOptions _smsOptions;

        public TwilioSmsService(HttpClient httpClient, IOptions<SmsOptions> smsOptions)
        {
            _httpClient = httpClient;
            _smsOptions = smsOptions.Value;
        }

        public async Task SendAsync(string toPhoneNumber, string body, CancellationToken cancellationToken = default)
        {
            if (!_smsOptions.Enabled)
            {
                throw new ValidationException("SMS provider is not configured.");
            }

            if (!string.Equals(_smsOptions.Provider, "Twilio", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Only the Twilio SMS provider is currently supported.");
            }

            if (string.IsNullOrWhiteSpace(_smsOptions.AccountSid) ||
                string.IsNullOrWhiteSpace(_smsOptions.AuthToken) ||
                string.IsNullOrWhiteSpace(_smsOptions.FromNumber))
            {
                throw new ValidationException("SMS provider settings are incomplete. Configure AccountSid, AuthToken, and FromNumber.");
            }

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.twilio.com/2010-04-01/Accounts/{_smsOptions.AccountSid}/Messages.json");

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_smsOptions.AccountSid}:{_smsOptions.AuthToken}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = toPhoneNumber,
                ["From"] = _smsOptions.FromNumber,
                ["Body"] = body
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ValidationException($"SMS provider rejected the request for {toPhoneNumber}: {responseBody}");
            }
        }
    }
}
