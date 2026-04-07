using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ConstructionManagement.Configurations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConstructionManagement.Services
{
    public class TaskDraftAiService : ITaskDraftAiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly OpenAiOptions _options;
        private readonly ILogger<TaskDraftAiService> _logger;

        public TaskDraftAiService(
            HttpClient httpClient,
            AppDbContext context,
            IOptions<OpenAiOptions> options,
            ILogger<TaskDraftAiService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AiTaskDraftSuggestionDto?> GenerateTaskDraftAsync(Guid projectId, AiTaskDraftRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new ValidationException("AI task generation is disabled. Configure OpenAI settings first.");
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ValidationException("OpenAI API key is missing.");
            }

            var siteDescription = request.SiteDescription.Trim();
            if (string.IsNullOrWhiteSpace(siteDescription))
            {
                throw new ValidationException("Site description is required.");
            }

            var project = await _context.Projects
                .Where(item => item.ProjectId == projectId)
                .Select(item => new
                {
                    item.ProjectId,
                    item.Code,
                    item.Name,
                    item.Address,
                    item.Description,
                    item.ClientName,
                    item.Status
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return null;
            }

            var systemPrompt = """
You are assisting a construction project manager.
Generate a practical work task draft from a field description.
Keep output concise, operational, and realistic for a construction site.
Do not mention AI.
Write the result in bilingual format with simplified Chinese first and concise English second.
For title, summary, category, and each execution step, use this pattern:
Chinese text / English text
Keep the Chinese natural for site workers and the English concise for portfolio/demo readability.
""";

            var userPrompt = $"""
Project context
- Project code: {project.Code}
- Project name: {project.Name}
- Address: {project.Address}
- Client: {project.ClientName ?? "Unknown"}
- Status: {project.Status}
- Project description: {project.Description ?? "None"}

Field description
{siteDescription}

Create one task draft with:
- title
- summary
- priority
- category
- estimatedHours
- executionSteps
""";

            var requestBody = new
            {
                model = string.IsNullOrWhiteSpace(_options.Model) ? "gpt-5-mini" : _options.Model,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = new object[]
                        {
                            new { type = "input_text", text = systemPrompt }
                        }
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "input_text", text = userPrompt }
                        }
                    }
                },
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "construction_task_draft",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            additionalProperties = false,
                            properties = new
                            {
                                title = new { type = "string" },
                                summary = new { type = "string" },
                                priority = new
                                {
                                    type = "string",
                                    @enum = new[] { "Low", "Medium", "High", "Critical" }
                                },
                                category = new { type = "string" },
                                estimatedHours = new { type = "number" },
                                executionSteps = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[] { "title", "summary", "priority", "category", "estimatedHours", "executionSteps" }
                        }
                    }
                }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            HttpResponseMessage response;
            string rawResponse;
            try
            {
                response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "OpenAI task draft request timed out for project {ProjectId}", projectId);
                throw new ValidationException("OpenAI request timed out. Please try again.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "OpenAI task draft request was canceled for project {ProjectId}", projectId);
                throw new ValidationException("AI request was canceled before the result came back. Try again and wait a bit longer.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "OpenAI task draft request failed at the network layer for project {ProjectId}", projectId);
                throw new ValidationException("OpenAI network request failed. Check your connection and try again.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI task draft request failed with status {StatusCode}: {Body}", response.StatusCode, rawResponse);

                if ((int)response.StatusCode == 401)
                {
                    throw new ValidationException("OpenAI API key is invalid or unauthorized.");
                }

                if ((int)response.StatusCode == 429)
                {
                    var quotaMessage = ExtractApiErrorMessage(rawResponse);
                    if (!string.IsNullOrWhiteSpace(quotaMessage))
                    {
                        throw new ValidationException($"OpenAI request was rate-limited or out of quota: {quotaMessage}");
                    }

                    throw new ValidationException("OpenAI request was rate-limited or out of quota.");
                }

                if ((int)response.StatusCode >= 500)
                {
                    throw new ValidationException("OpenAI service is temporarily unavailable. Please try again.");
                }

                var apiErrorMessage = ExtractApiErrorMessage(rawResponse);
                if (!string.IsNullOrWhiteSpace(apiErrorMessage))
                {
                    throw new ValidationException($"OpenAI request failed: {apiErrorMessage}");
                }

                throw new ValidationException("AI task generation failed. Check the OpenAI configuration and try again.");
            }

            var outputText = ExtractOutputText(rawResponse);
            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new ValidationException("AI task generation returned an empty result.");
            }

            AiTaskDraftSuggestionDto? suggestion;
            try
            {
                suggestion = JsonSerializer.Deserialize<AiTaskDraftSuggestionDto>(outputText, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "OpenAI task draft returned invalid JSON: {Body}", outputText);
                throw new ValidationException("AI task generation returned an unreadable result.");
            }

            if (suggestion is null)
            {
                throw new ValidationException("AI task generation returned no draft.");
            }

            suggestion.Title = suggestion.Title.Trim();
            suggestion.Summary = suggestion.Summary.Trim();
            suggestion.Category = suggestion.Category.Trim();
            suggestion.Priority = suggestion.Priority.Trim();
            suggestion.EstimatedHours = Math.Round(Math.Max(0.5m, suggestion.EstimatedHours), 1, MidpointRounding.AwayFromZero);
            suggestion.ExecutionSteps = suggestion.ExecutionSteps
                .Where(step => !string.IsNullOrWhiteSpace(step))
                .Select(step => step.Trim())
                .Take(8)
                .ToList();

            if (string.IsNullOrWhiteSpace(suggestion.Title) || string.IsNullOrWhiteSpace(suggestion.Summary) || suggestion.ExecutionSteps.Count == 0)
            {
                throw new ValidationException("AI task generation returned an incomplete draft.");
            }

            return suggestion;
        }

        private static string? ExtractApiErrorMessage(string rawResponse)
        {
            try
            {
                using var document = JsonDocument.Parse(rawResponse);
                if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                    errorElement.ValueKind == JsonValueKind.Object &&
                    errorElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;
        }

        private static string? ExtractOutputText(string rawResponse)
        {
            using var document = JsonDocument.Parse(rawResponse);
            if (!document.RootElement.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var item in outputElement.EnumerateArray())
            {
                if (item.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contentItem in contentElement.EnumerateArray())
                    {
                        if (contentItem.TryGetProperty("type", out var typeElement))
                        {
                            var type = typeElement.GetString();
                            if (string.Equals(type, "output_text", StringComparison.OrdinalIgnoreCase) &&
                                contentItem.TryGetProperty("text", out var textElement))
                            {
                                return textElement.GetString();
                            }

                            if (string.Equals(type, "refusal", StringComparison.OrdinalIgnoreCase) &&
                                contentItem.TryGetProperty("refusal", out var refusalElement))
                            {
                                throw new ValidationException(refusalElement.GetString() ?? "The model refused this request.");
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
