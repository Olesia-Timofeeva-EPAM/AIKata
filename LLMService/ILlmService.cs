namespace LLMService
{
    public interface ILlmService
    {
        Task<string> SummarizeAsync(string transcript);
    }
}
