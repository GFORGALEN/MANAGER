using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Helpers
{
    public static class StatusValidators
    {
        public static readonly string[] TaskStatuses = ["Draft", "InProgress", "Blocked", "Done"];

        public static readonly string[] VariationStatuses = ["Draft", "Submitted", "Approved", "Rejected", "NeedInfo"];

        public static string ValidateTaskStatus(string status)
        {
            return Validate(status, TaskStatuses, "Status must be one of: Draft, InProgress, Blocked, Done.");
        }

        public static string ValidateVariationStatus(string status)
        {
            return Validate(status, VariationStatuses, "Status must be one of: Draft, Submitted, Approved, Rejected, NeedInfo.");
        }

        private static string Validate(string status, string[] allowed, string message)
        {
            var normalized = status.Trim();
            var matched = allowed.FirstOrDefault(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase));
            if (matched is null)
            {
                throw new ValidationException(message);
            }

            return matched;
        }
    }
}
