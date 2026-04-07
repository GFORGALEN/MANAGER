using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ConstructionManagement.Configurations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConstructionManagement.Services
{
    public class ProjectAiService : IProjectAiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly OpenAiOptions _options;
        private readonly ILogger<ProjectAiService> _logger;

        public ProjectAiService(
            HttpClient httpClient,
            AppDbContext context,
            IOptions<OpenAiOptions> options,
            ILogger<ProjectAiService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AiWeeklySummaryDto?> GenerateWeeklySummaryAsync(Guid projectId, AiWeeklySummaryRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new ValidationException("AI weekly summary is disabled. Configure OpenAI settings first.");
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ValidationException("OpenAI API key is missing.");
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
                    item.Status,
                    item.Budget,
                    item.StartDate,
                    item.EndDate,
                    item.CreatedAt,
                    AttachmentCount = item.Attachments.Count,
                    Tasks = item.TaskItems
                        .OrderByDescending(task => task.CreatedAt)
                        .Select(task => new
                        {
                            task.Title,
                            task.Status,
                            task.Description,
                            task.StartDate,
                            task.DueDate,
                            task.CreatedAt,
                            AssignedUsers = task.TaskAssignments
                                .Select(assignment => assignment.User.Name)
                                .ToList()
                        })
                        .Take(18)
                        .ToList(),
                    Variations = item.Variations
                        .OrderByDescending(variation => variation.CreatedAt)
                        .Select(variation => new
                        {
                            variation.Title,
                            variation.Description,
                            variation.Amount,
                            variation.Status,
                            variation.CreatedAt
                        })
                        .Take(10)
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var doneCount = project.Tasks.Count(task => string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase));
            var inProgressCount = project.Tasks.Count(task => string.Equals(task.Status, "InProgress", StringComparison.OrdinalIgnoreCase));
            var blockedCount = project.Tasks.Count(task => string.Equals(task.Status, "Blocked", StringComparison.OrdinalIgnoreCase));
            var draftCount = project.Tasks.Count(task => string.Equals(task.Status, "Draft", StringComparison.OrdinalIgnoreCase));
            var overdueCount = project.Tasks.Count(task =>
                !string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase) &&
                task.DueDate < now);

            var variationBreakdown = project.Variations
                .GroupBy(variation => variation.Status)
                .Select(group => $"{group.Key}: {group.Count()}")
                .ToArray();

            var taskLines = project.Tasks.Count == 0
                ? ["- No tasks recorded yet."]
                : project.Tasks.Select(task =>
                {
                    var assignees = task.AssignedUsers.Count > 0 ? string.Join(", ", task.AssignedUsers) : "Unassigned";
                    var dueLabel = task.DueDate.ToString("yyyy-MM-dd");
                    var description = string.IsNullOrWhiteSpace(task.Description) ? "No description." : task.Description.Trim();
                    return $"- {task.Title} | Status: {task.Status} | Due: {dueLabel} | Assignees: {assignees} | Notes: {description}";
                }).ToArray();

            var variationLines = project.Variations.Count == 0
                ? ["- No variations logged."]
                : project.Variations.Select(variation =>
                {
                    var description = string.IsNullOrWhiteSpace(variation.Description) ? "No description." : variation.Description.Trim();
                    return $"- {variation.Title} | Status: {variation.Status} | Amount: {variation.Amount:0.##} | Notes: {description}";
                }).ToArray();

            var contextNotes = string.IsNullOrWhiteSpace(request.ContextNotes)
                ? "None provided."
                : request.ContextNotes.Trim();

            var systemPrompt = """
You are assisting a construction project manager.
Generate a concise weekly project summary for internal PM reporting.
Use plain business English.
Ground the summary only in the provided project, task, and variation data.
Do not mention AI.
If no strong risks or decisions exist, keep those sections short and practical rather than inventing drama.
""";

            var userPrompt = $"""
Project context
- Project code: {project.Code}
- Project name: {project.Name}
- Address: {project.Address}
- Client: {project.ClientName ?? "Unknown"}
- Status: {project.Status}
- Budget: {(project.Budget.HasValue ? project.Budget.Value.ToString("0.##") : "Unknown")}
- Start date: {FormatDate(project.StartDate)}
- End date: {FormatDate(project.EndDate)}
- Created at: {project.CreatedAt:yyyy-MM-dd}
- Attachment count: {project.AttachmentCount}

Operational snapshot
- Total tasks reviewed: {project.Tasks.Count}
- Done: {doneCount}
- In progress: {inProgressCount}
- Blocked: {blockedCount}
- Draft: {draftCount}
- Overdue and not done: {overdueCount}
- Variations reviewed: {project.Variations.Count}
- Variation breakdown: {(variationBreakdown.Length == 0 ? "None" : string.Join(", ", variationBreakdown))}

Recent tasks
{string.Join(Environment.NewLine, taskLines)}

Recent variations
{string.Join(Environment.NewLine, variationLines)}

Additional PM context
{contextNotes}

Create one weekly summary with:
- headline
- summary
- progressHighlights
- riskAlerts
- nextWeekPlan
- openDecisions
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
                        name = "construction_weekly_summary",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            additionalProperties = false,
                            properties = new
                            {
                                headline = new { type = "string" },
                                summary = new { type = "string" },
                                progressHighlights = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                riskAlerts = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                nextWeekPlan = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                openDecisions = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[] { "headline", "summary", "progressHighlights", "riskAlerts", "nextWeekPlan", "openDecisions" }
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
                _logger.LogWarning(ex, "OpenAI weekly summary request timed out for project {ProjectId}", projectId);
                throw new ValidationException("OpenAI request timed out. Please try again.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "OpenAI weekly summary request was canceled for project {ProjectId}", projectId);
                throw new ValidationException("AI request was canceled before the result came back. Try again and wait a bit longer.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "OpenAI weekly summary request failed at the network layer for project {ProjectId}", projectId);
                throw new ValidationException("OpenAI network request failed. Check your connection and try again.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI weekly summary request failed with status {StatusCode}: {Body}", response.StatusCode, rawResponse);

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

                throw new ValidationException("AI weekly summary generation failed. Check the OpenAI configuration and try again.");
            }

            var outputText = ExtractOutputText(rawResponse);
            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new ValidationException("AI weekly summary returned an empty result.");
            }

            AiWeeklySummaryDto? summary;
            try
            {
                summary = JsonSerializer.Deserialize<AiWeeklySummaryDto>(outputText, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "OpenAI weekly summary returned invalid JSON: {Body}", outputText);
                throw new ValidationException("AI weekly summary returned an unreadable result.");
            }

            if (summary is null)
            {
                throw new ValidationException("AI weekly summary returned no content.");
            }

            summary.Headline = summary.Headline.Trim();
            summary.Summary = summary.Summary.Trim();
            summary.ProgressHighlights = SanitizeList(summary.ProgressHighlights, 5);
            summary.RiskAlerts = SanitizeList(summary.RiskAlerts, 5);
            summary.NextWeekPlan = SanitizeList(summary.NextWeekPlan, 5);
            summary.OpenDecisions = SanitizeList(summary.OpenDecisions, 5);

            if (string.IsNullOrWhiteSpace(summary.Headline) || string.IsNullOrWhiteSpace(summary.Summary))
            {
                throw new ValidationException("AI weekly summary returned incomplete content.");
            }

            return summary;
        }

        private static List<string> SanitizeList(IEnumerable<string> items, int maxItems)
        {
            return items
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Take(maxItems)
                .ToList();
        }

        private static string FormatDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "Unknown";
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
