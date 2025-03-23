using Microsoft.SemanticKernel;

namespace WordFlux.Translations.Ai;

public class AiFunctions
{
    public static readonly KernelFunction TranslationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AIPrompts.GiveTranslations,
        InputVariables =
        [
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
        Template = AIPrompts.GiveAlternativesPrompt,
        InputVariables =
        [
            new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" },
            new() { Name = "existingTranslations", Description = "Existing translations for the term" },
            new() { Name = "srclang", Description = "language of the original term" },
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
        Template = AIPrompts.CardExampleTaskPrompt,
        InputVariables =
        [
            new() { Name = "term", Description = "The term (can be word or phrase)" },
            new() { Name = "learnLang", Description = "language of the term, that is learning" },
            new() { Name = "nativeLang", Description = "native language" },
            new() { Name = "count", Description = "Number of examples" },
            new() { Name = "translations", Description = "existing translations. Can be used as a background context" }
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
        InputVariables =
        [
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
        InputVariables =
        [
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
        InputVariables =
        [
            new() { Name = "src" },
            new() { Name = "dest" },
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
                   It can be only in these 2 languages: [{{$lang1}}, {{$lang2}}]
                   Return results in JSON. Give me up to 3 results for autocomplete. Also, return detected language (can be only 1 of what I provided)
                   Example output: { "autocompletes": [{"term": "fin", "term_translated" : "плавник"}, {"term":"first","term_translated":"первый"}], "lang": "en" } for input = "fin"
                   Example output2: { "autocompletes": [{"term": "aptitude", "term_translated" : "способность"}], "lang": "en" } for input = "aptitude"
                   Note 1: autocompletes must be valid words. Return response as faster as possible
                   Note 2: if provided input is already a correct word / phrase, it must be in response. (e.g. "Get to" => "добраться до"). If there is a typo, you can try to fix it.
                   Note 3: do not provide me autocompletes for other languages except of these 2 that I provided
                   """,
        InputVariables =
        [
            new() { Name = "term" },
            new() { Name = "lang1" },
            new() { Name = "lang2" },
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
        InputVariables =
        [
            new() { Name = "term" },
            new() { Name = "lang1" },
            new() { Name = "lang2" },
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
        Template = AIPrompts.GiveTranslationExamples,
        InputVariables =
        [
            new() { Name = "term", Description = "the original phrase or word that is translated" },
            new() { Name = "translations", Description = "Existing translations for the term" },
            new() { Name = "srclang", Description = "language of the original term" },
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

    public static readonly KernelFunction DivideExportedDescriptionIntoObject = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   There is a input: '{{$term}}' and description for it: {{$translation}} 
                   You should identify what description consist of and map it to the new object.
                   This description can be either simple translation or contain list of translations or definition or example. 
                   Return results in the following JSON:
                   {"translations":[{"translation": "translation1","example":"example of usage of the input", "definition":""}] }
                   Example and definition are optional properties but translation is required;
                   Do not provide additional data except that is already in description;
                   If there are multiple meanings (for translations / definitions), divide them into separate items;
                   If there is single translation and multiple examples, provide examples in the single string for this translation
                   Terms, examples, and definition should not be duplicated.
                   """,
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":[{"translation": "translation1","example":"example of usage of the input", "definition":""}] }
                         """
        },
        InputVariables =
        [
            new() { Name = "term", Description = "Input word, phrase or sentence" },
            new() { Name = "translation", Description = "Description of the word (can contain translation, definition, example)" }
        ]
    });
    
    public static readonly KernelFunction DivideExportedDescriptionIntoObjectList = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   You are translator assistant. I will provide you input which is list of objects.
                   Execute the following script for each object. result must be in List of mapped results in json: {"mapped_objects" :[mapped_object1, mapped_object2, mapped_object3] }.
                   1. There is a term and description for it;
                   2. This description can contain translation / usage example / definition. You should separate them to different fields.
                   3. Return results in the following JSON. Use this example
                    mapped_object1: {"term": "halt", "srcLang":"en", "destLang":"ru", "definition":"definition of the halt", "translations":[{"translation": "остановить","example":"Just when you're thinking things could not have been better, your hookup suddenly halts all forms of communication with you, no explanation."},{"translation": "прекратить"}]}
                    mapped_object2: {"term": "surge", "srcLang":"en", "destLang":"ru", "translations":[{"translation": "всплеск",{"translation": "рост"}, {"translation": "волна"}]}
                   
                   4. Translation should be inside translation field of the object;
                   5. You should not add to the response any generated data. Also, data can not be lost.
                   6. If there are multiple translations, divide them into different translation item inside translations array;
                   7. Translations, examples, and definition should not be duplicated across object;
                   8. Definition is usually in english OR in language of the term. It is usually some official quote from Dictionary. Do not put translation into definition.
                   9. For each object, detect source and destination languages
                   10.Definition can be empty. Omit field in this case
                   After you completed all the steps, revise it. Check that for each item from source list there is an item in response. Order should be preserved.
                   
                   Here is the source list: {{$input}}
                   """,
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"mapped_objects" : [{"term": "original", "srcLang":"en", "destLang":"ru", "definition":"", "translations":[{"translation": "translation1","example":"example of usage of the input"}]}] }
                         """
        },
        InputVariables =
        [
            new() { Name = "input", Description = "Source items." },
        ]
    });

    public static readonly KernelFunction GiveMotivationalPhraseFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AIPrompts.GiveMotivationPrompt
    });
}