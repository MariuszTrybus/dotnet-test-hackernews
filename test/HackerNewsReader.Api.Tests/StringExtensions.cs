using System.Text.RegularExpressions;

namespace HackerNewsReader.Api.Tests;

public static class StringExtensions
{
    public static string ToLf(this string value)
    {
        return value.Replace("\r\n", "\n");
    }
    
    public static string ReplaceTraceId(this string value)
    {
        return Regex.Replace(value, "\"traceId\"\\s*:\\s*\"\\S{55}\"", "\"traceId\":\"fake-trace-id\"");
    }
}