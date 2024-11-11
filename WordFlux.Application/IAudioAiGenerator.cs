namespace WordFlux.Application;

public interface IAudioAiGenerator
{
    Task<byte[]> GenerateAudioFromTextAsync(string text, CancellationToken cancellationToken = default);
}

