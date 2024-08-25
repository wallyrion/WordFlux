using Microsoft.SemanticKernel;

namespace WordFlux.ApiService;

public class AiFunctions
{
    public static readonly KernelFunction TranslationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveTranslations,
        InputVariables = [new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" }],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":["to encourage"], "suggested_term": "поощрять", "srcL": "en-US", "outL": "ru-RU"}
                         """
        }
    }); 
    
    public static readonly KernelFunction GiveAlternativesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveAlternativesPrompt,
        InputVariables = [
            new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" },
            new() { Name = "existingTranslations", Description = "Existing translations for the term" } ,
            new() { Name = "srclang", Description = "language of the original term" } ,
            new() { Name = "destLang", Description = "language of the translations" } 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":["to encourage"]}
                         """
        }
    });   
    
    public static readonly KernelFunction DetectLanguageFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Detect language and map to the JSON object: {"srcLang": "language of {{$src}} (e.g. en-US)", "destLang": "language of {{$dest}}"}
                   Only these languages are possible: ["en-US", "ru-RU"] 
                   """,
        InputVariables = [
            new() { Name = "src" }, 
            new() { Name = "dest" } ,
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"srcLang": "en-US", "destLang": "ru-RU"}
                         """
        }
    });
    
    
    public static readonly KernelFunction GiveExamplesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.translationExamples,
        InputVariables = [
            new() { Name = "term", Description = "the original phrase or word that is translated" }, 
            new() { Name = "translations", Description = "Existing translations for the term" } ,
            new() { Name = "srclang", Description = "language of the original term" } ,
            new() { Name = "destLang", Description = "language of the translations" } 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations": [{"tr": "translation", "l": "A0", "p": "55", "e_tr": "example of 'translation'", "e_or": "example translated"}]}
                         """
        }
    });
    
    public static readonly KernelFunction GetLevelFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   There is a $term = '{{$term}}'. Determine the level of complexity from A0 to C2 and return it..
                   Example1: for input term = 'кошка' should return such JSON: {"level": "A1"}
                   Example2: for input term = 'father' should return such JSON: {"level": "A0"}
                   """,
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"level": "A1"}
                         """
        },
        InputVariables = [new() { Name = "term", Description = "Input word, phrase or sentence" }]
    });

    
       
    public static readonly KernelFunction GiveMotivationalPhraseFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveMotivationPrompt
    });   
}