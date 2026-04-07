namespace ConstructionManagement.Configurations
{
    public class OpenAiOptions
    {
        public const string SectionName = "OpenAI";

        public bool Enabled { get; set; }

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-5-mini";
    }
}
