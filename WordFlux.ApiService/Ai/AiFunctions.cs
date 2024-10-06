using Microsoft.SemanticKernel;

namespace WordFlux.ApiService;

public class AiFunctions
{
    public static readonly KernelFunction TranslationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveTranslations,
        InputVariables = [
            new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" },
            new() { Name = "inputLang", Description = "Language of the input" },
            new() { Name = "translationsLang", Description = "Language of the possible translations" },
            new() { Name = "translationsCount", Description = "Number of possible translations" }
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":["to encourage"]}
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
    
    public static readonly KernelFunction CreateCardExampleTaskFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.CardExampleTaskPrompt,
        InputVariables = [
            new() { Name = "term", Description = "The term (can be word or phrase)" },
            new() { Name = "learnLang", Description = "language of the term, that is learning" } ,
            new() { Name = "nativeLang", Description = "native language" } ,
            new() { Name = "count", Description = "Number of examples" } 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         { "sentences": [ {"example_original": "The progress of *mankind* is dependent on education." , "example_translated": "Прогресс *человечества* зависит от образования." }  ] }
                         """
        }
    });   
    
    
    public static readonly KernelFunction DetectLanguageFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Detect language of input: |{{$input}}| and map to the JSON object: {"language": "language of input (e.g. en)"}
                   Only these languages are possible: [{{$possibleLanguages}}].
                   If input has typo, try to fix a typo and put corrected value into suggested_term. If no typo, skip suggested_term field
                   """,
        InputVariables = [
            new() { Name = "input" }, 
            new() { Name = "possibleLanguages" }, 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"language": "en"}
                         """
        }
    });
    
    public static readonly KernelFunction DetectPossibleLanguageFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Detect language of src: |{{$src}}| and language of destination |{{$dest}}| and map to the JSON object: {"srcLanguage": "language of src (e.g. en, ru, uk, hr)", "destLanguage": "language of destination"}
                   If you can't do it, put empty string
                   """,
        InputVariables = [
            new() { Name = "src" }, 
            new() { Name = "dest" }
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"srcLanguage": "en", "destLanguage": "ru"}
                         """
        }
    });
    
    public static readonly KernelFunction DetectLanguagesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Detect language and map to the JSON object: {"srcLang": "language of {{$src}} (e.g. en-US)", "destLang": "language of {{$dest}}"}
                   Only these languages are possible: ["en-US", "ru-RU"]. src and dest can't be in the same language
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
    
    
    public static readonly KernelFunction AutocompleteWithTranslationFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Suggest me autocomplete for this word: {{$term}}. (it is used as a autocomplete for translator)
                   It can be only in these 2 languages: [{{$lang1}}, {{$lang2}}] (only one of them)
                   Return results in JSON. Give me up to 3 results for autocomplete. Also, return detected language (can be only 1 of what I provided)
                   Example output: { "autocompletes": [{"term": "fin", "term_translated" : "плавник"}, {"term":"first","term_translated":"первый"}], "lang": "en" } for input = "fin"
                   Example output2: { "autocompletes": [{"term": "aptitude", "term_translated" : "способность"}], "lang": "en" } for input = "aptitude"
                   Note 1: autocompletes must be valid words. Return response as faster as possible
                   Note 2: if provided input is already a correct word / phrase, it must be in response. (e.g. "Get to" => "добраться до")
                   Note 3: do not provide me autocompletes for other languages except of these 2 that I provided
                   Note 4: response should contain full string (not only the autocomplete part)
                   """,
        InputVariables = [
            new() { Name = "term" }, 
            new() { Name = "lang1" }, 
            new() { Name = "lang2" } ,
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         { "autocompletes": [{"term": "fin", "term_translated":"плавник"}, {"term":"first","term_translated":"первый"}], "lang": "en" }
                         """
        }
    });
    
    public static readonly KernelFunction AutocompleteFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   1. Suggest me autocomplete for input {{$term}}. If word is already correct, it must be present in response.
                   2. Return results in JSON. Give me up to 3 suggestions. They must me in the same language as input lang. Do not suggest me words from another language (e.g. do not suggest "генерация" for input "gen")
                   3. Only these languages are supported: [{{$lang1}}, {{$lang2}}]
                   Example output: { "autocompletes": ["fin", "first"], "lang": "en" } for input = "fin"
                   Example output2: { "autocompletes": ["that's good to"], "lang": "en" } for input = "that's"
                   response should contain full string (not only the autocomplete)
                   """,
        InputVariables = [
            new() { Name = "term" }, 
            new() { Name = "lang1" }, 
            new() { Name = "lang2" } ,
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         { "autocompletes": ["fin", "first"], "lang": "en" }
                         """
        }
    });
    
    
    public static readonly KernelFunction GiveExamplesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveTranslationExamples,
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