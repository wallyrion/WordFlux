namespace WordFlux.Application.Common.Options;

public class OpensearchOptions
{
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool SkipSslVerification { get; set; }
}