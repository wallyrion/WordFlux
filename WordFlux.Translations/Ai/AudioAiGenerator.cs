using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.Application;

namespace WordFlux.Translations.Ai;
#pragma warning disable SKEXP0001

public class AudioAiGenerator([FromKeyedServices(KeyedKernelType.AudioText)] Kernel kernel) : IAudioAiGenerator
{
    public async Task<byte[]> GenerateAudioFromTextAsync(string text, CancellationToken cancellationToken = default)
    {
        var service = kernel.GetRequiredService<ITextToAudioService>();

        var res = await service.GetAudioContentsAsync(text, cancellationToken: cancellationToken);

        var first = res[0];

        return first.Data == null ? [] : first.Data.Value.ToArray();
    }
}