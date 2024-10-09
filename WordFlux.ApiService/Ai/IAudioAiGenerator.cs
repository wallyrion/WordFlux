namespace WordFlux.ApiService.Ai;

public interface IAudioAiGenerator
{
    Task<byte[]> GenerateAudioFromTextAsync(string text, CancellationToken cancellationToken = default);
}

