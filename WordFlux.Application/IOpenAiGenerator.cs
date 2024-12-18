﻿namespace WordFlux.Application;

public interface IOpenAiGenerator
{
    Task<(string sourceLanguage, string destinationLanguage)?> DetectLanguage(string sourceInput, string translatedInput, CancellationToken cancellationToken = default);
    
    Task<List<(string ExampleLearn, string ExampleNative)>?> GetExamplesCardTask(string term, string learnLanguage, string nativeLanguage, int examplesCount,
        IReadOnlyList<string> translations, CancellationToken cancellationToken = default);
}