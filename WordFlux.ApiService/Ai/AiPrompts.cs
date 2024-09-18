namespace WordFlux.ApiService;

/*public static class AiSystemMessages
{

    public const string exampleResponseJson = """
                                              {"term":"adjictable", "suggestedTerm": "addictive", "l": "B2", "list":[{"tr":"вызывающий привыкание ", "l": "B2", "e_tr":"Социальные сети могут *вызывать привыкание*","e_or":"Social media can be very *addictive*", "u_f": 90}]},
                                              {"term":"кошка", "l": "A1", "list":[{"l": "A1", "tr":"cat","e_tr":"This *cat* is very playful","e_or":"Эта *кошка* очень игривая", "u_f": 98}]}
                                              {"term":"лук (для стрельбы)", "l": "B1", "list":[{"tr":"bow", "l": "B1", "e_tr":"He practices with his *bow* every weekend","e_or":"Он практикуется с луком каждые выходные", "u_f": 90}]}
                                              """;


    public const string RequestForAssistantWithArguments = $$$"""
                                                               1. There is a term '{{$term}}'. Can be in English or Russian, determine language as original.
                                                               2. If 'term' in English, translate from English to Russian. If there is a typo, suggest the correct term in 'suggestedTerm'.
                                                               3. If 'term' in Russian, translate from Russian to English. If there is a typo, suggest the correct term in 'suggestedTerm'.
                                                               5. Return me JSON with translations (up to 5) for this term in JSON format. There are examples of output: {{{exampleResponseJson}}} where 'tr' - translation of 'term'; 'e_tr' - must be same language as 'tr'; 'e_or' - must be same language as 'term' or 'suggestedTerm'.
                                                               6. The term should be highlighted with * in usages.
                                                               7. Translations should be sorted from the most popular to the least popular.
                                                               8. u_f - popularity of translation from 0 to 100 (how often it is used in the context)
                                                               9. there can be a note (clarification) inside 'term' that should be considered as a clue for the context.
                                                               10. For each term and its translation, estimate the level from A0 to C2.
                                                               """;

}*/

public static class AiSystemMessages
{
    /*public const string GiveTranslations =
        """
        
           {
             "description": "Translate and correct word / phrase / sentence between English and Russian with consideration of context.",
             "steps": [
               {
                 "step": "Introduce the term",
                 "instruction": "There is variable $term = {{$term}}. Use it for the next instructions"
               },
               {
                 "step": "Identify the language of $term.",
                 "instruction": "If $term is in English, translate it into Russian. If $term is in Russian, translate it into English."
               },
               {
                 "step": "Handle typos.",
                 "instruction": "If $term is not a valid word, attempt to correct the typo and include the corrected value in 'suggested_term'."
               },
               {
                 "step": "Provide translations.",
                 "instruction": "You can provide up to 4 translations, but the most proper (most popular) translations must be listed first. Consider any notes or clarifications in $term as clues for the context."
               },
               {
                 "step": "Output format.",
                 "instruction": "The response must be in JSON format. If there are no corrections, exclude the 'corrected' field."
               }
             ],
             "example_outputs": [
               {
                 "input": "$term = 'лук (для стрельбы)'",
                 "output": {"translations": ["bow"]}
               },
               {
                 "input": "$term = 'поощрать'",
                 "output": {"suggested_term": "поощрять", "translations": ["to encourage", "to promote", "to reward"]}
               },
               {
                 "input": "$term = 'tall me where was I wrong?'",
                 "output": {"suggested_term": "Tell me where I was wrong?", "translations": ["Подскажи, где я был неправ?"]}
               }
             ]
           }
           
        """;*/

    public const string GiveTranslations = """
                                              You must translate this input: {{$term}} that in {{$inputLang}} language into {{$translationsLang}} language. Follow my instructions:
                                              1. You can provide more than 1 translation, and up to {{$translationsCount}} (in {{$translationsLang}}). Most obvious translations must be on the top.
                                              2. input may contain a note (clarification) that should be considered as a clue for the context. (e.g. example_output1)
                                              3. Response must be in JSON. Consider the following examples. Examples are in en-ru but you must provide translations in {{$translationsLang}}
                                              example_output1: {"translations":["bow"]} for input = 'лук (для стрельбы)'
                                              example_output2: {"translations":["to encourage", "to promote", "to reward"]} for input = 'поощрать'
                                              """;

    public const string GiveTranslationExamples = """
                                               There is a $term = '{{$term}}' and list of translation_items for it: {{$translations}} in {{$destLang}}.
                                               For each translation_item give example of usage and map to object:
                                               {"tr": "translation_item, should not be changed", "l": "level of translation_item from A0 to C2", "e_or": "Example of usage of *$term*. Must be in {{$srcLang}}.", "e_tr": "example of usage of *translation_item*. Should be in {{$destLang}}."}
                                               Mapped objects should be in the same order and same count as original 'translation_items' array.
                                               Highlight term with '*'.
                                               'e_or' and 'e_tr' fields must have the same example message but 'e_or' contain *$term* (in {{$srcLang}}) and 'e_tr' contain *translation_item* (in {{$destLang}}
                                               Response must be in JSON format in the following template: (note that examples are in ru-en, but you must consider only {{$srcLang}} and {{$destlang}}
                                               example1 for input 'term' = "кошка" and 'translations' = ["cat", "feline"]: {"translations": [{"tr": "cat", "l": "A1", "e_tr": "This *cat* is very playful", "e_or": "Эта *кошка* очень игривая"}, {"tr": "feline", "l": "B2", "e_tr": "His *feline* reflexes allowed him to catch the ball.", "e_or": "Его *кошачья* реакция позволила ему поймать мяч"}]
                                               example2 for input 'term' = "my" and 'translations' = ["мой", "моя"]: {"translations": [{"tr": "мой", "l": "A1", "e_or": "This is *my* house", "e_tr": "Это *мой* дом"}, {"tr": "моя", "l": "A1", "e_or": "This is *my* car", "e_tr": "Это *моя* машина"}]
                                               """;

    public const string GiveAlternativesPrompt = """
                                            There is a $term = '{{$term}}' in '{{$srcLang}}' and existing translations for it: $existingTranslations = {{$existingTranslations}} in '{{$destLang}}'.
                                            Give me alternative translations for $term in {{$destLang}} and return response in JSON, for examples: {"translations": ["to long for", "to yearn for"]}
                                            Exclude $existingTranslations from the result. Double check you do not provide duplicates
                                           """;
    
    
    public const string GiveMotivationPrompt = """
                                            Give me some rangom quote or motivational phrase. It can be one or several sentences.
                                           """;
    
    
}