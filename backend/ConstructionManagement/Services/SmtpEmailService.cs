using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Net.Mail;
using ConstructionManagement.Configurations;
using Microsoft.Extensions.Options;

namespace ConstructionManagement.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly HttpClient _httpClient;

        public SmtpEmailService(IOptions<EmailOptions> emailOptions, HttpClient httpClient)
        {
            _emailOptions = emailOptions.Value;
            _httpClient = httpClient;
        }

        public async Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (!_emailOptions.Enabled)
            {
                throw new ValidationException("Email provider is not configured.");
            }

            if (string.Equals(_emailOptions.Provider, "Resend", StringComparison.OrdinalIgnoreCase))
            {
                await SendWithResendAsync(toAddress, subject, body, cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(_emailOptions.Host) ||
                string.IsNullOrWhiteSpace(_emailOptions.Username) ||
                string.IsNullOrWhiteSpace(_emailOptions.Password) ||
                string.IsNullOrWhiteSpace(_emailOptions.FromAddress))
            {
                throw new ValidationException("Email provider settings are incomplete. Configure host, credentials, and from address.");
            }

            using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.UseSsl,
                Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.FromAddress, _emailOptions.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(toAddress));

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
        }

        private async Task SendWithResendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_emailOptions.ApiKey) ||
                string.IsNullOrWhiteSpace(_emailOptions.FromAddress))
            {
                throw new ValidationException("Resend settings are incomplete. Configure ApiKey and FromAddress.");
            }

            var fromValue = string.IsNullOrWhiteSpace(_emailOptions.FromName)
                ? _emailOptions.FromAddress
                : $"{_emailOptions.FromName} <{_emailOptions.FromAddress}>";

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _emailOptions.ApiKey);
            request.Content = JsonContent.Create(new
            {
                from = fromValue,
                to = new[] { toAddress },
                subject,
                html = BuildHtmlBody(body),
                text = body
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ValidationException($"Resend rejected the request for {toAddress}: {responseBody}");
            }
        }

        private static string BuildHtmlBody(string body)
        {
            var encoded = WebUtility.HtmlEncode(body).Replace(Environment.NewLine, "<br />");
            return $"<div style=\"font-family: Arial, sans-serif; line-height: 1.6;\">{encoded}</div>";
        }
    }
}
