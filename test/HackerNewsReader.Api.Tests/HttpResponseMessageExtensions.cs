using System.Text;

namespace HackerNewsReader.Api.Tests;

public static class HttpResponseMessageExtensions
{
    public static async Task<string> ToFormattedString(this HttpResponseMessage response)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
        foreach (var header in response.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }
        
        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            
            sb.AppendLine();

            var body = await response.Content.ReadAsStringAsync();
            sb.Append(body);
        }

        return sb.Replace("\r\n", "\n").ToString();
    }
}