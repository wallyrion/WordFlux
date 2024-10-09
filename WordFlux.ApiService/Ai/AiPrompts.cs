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
                                              1. Provide up to {{$translationsCount}} translations.
                                              - Translations must be accurate and ordered by their common usage or frequency in everyday language.
                                              2. If the input contains a note or clarification (e.g., in parentheses), use it to determine the correct context for the translation.
                                              3. Response must be in JSON. Consider the following examples
                                              example_output1: {"translations":["bow"]} for input = 'лук (для стрельбы)'
                                              example_output2: {"translations":["to encourage", "to promote", "to reward"]} for input = 'поощрать'
                                              Before sending the response, double-check that all translations are correctly spelled and fully in {{$translationsLang}}.
                                              """;

    
    /*'e_or' and 'e_tr' fields must have the same example message but 'e_or' contain *$term* (in {{$srcLang}}) and 'e_tr' contain *translation_item* (in {{$destLang}}*/
    
    public const string GiveTranslationExamples = """
                                               There is a $term = '{{$term}}' and list of translation_items for it: {{$translations}} in {{$destLang}};
                                               For each translation_item give example of usage. (sentences that clearly describes how this item is used when translating from $term). You must map the object for each item
                                               {"tr": "translation_item (do not change it)", "l": "level of translation_item from A0 to C2", "e_or": "Example of usage of *translation_item* (to describe $term). Sentence must be in {{$srcLang}}.", "e_tr": "translated example of usage of *translation_item*. Sentence must be in {{$destLang}}."};
                                               Mapped objects should be in the same order and same count as original 'translation_items' array;
                                               Highlight term with '*';
                                               Sentences must be real life examples and not just nonsense random text;
                                               Response must be in JSON format in the following template: (note that examples are in ru-en, but you must generate example only in {{$srcLang}} and {{$destlang}}.
                                               example1 for input 'term' = "кошка" and 'translations' = ["cat", "feline"]: {"translations": [{"tr": "cat", "l": "A1", "e_tr": "This *cat* is very playful", "e_or": "Эта *кошка* очень игривая"}, {"tr": "feline", "l": "B2", "e_tr": "His *feline* reflexes allowed him to catch the ball.", "e_or": "Его *кошачья* реакция позволила ему поймать мяч"}];
                                               example2 for input 'term' = "my" and 'translations' = ["мой", "моя"]: {"translations": [{"tr": "мой", "l": "A1", "e_or": "This is *my* house", "e_tr": "Это *мой* дом"}, {"tr": "моя", "l": "A1", "e_or": "This is *my* car", "e_tr": "Это *моя* машина"}];
                                               """;

    public const string GiveAlternativesPrompt = """
                                            There is a $term = '{{$term}}' in '{{$srcLang}}' and existing translations for it: $existingTranslations = {{$existingTranslations}} in '{{$destLang}}'.
                                            Give me alternative translations for $term in {{$destLang}} and return response in JSON, for examples: {"translations": ["to long for", "to yearn for"]}
                                            Exclude $existingTranslations from the result. Double check you do not provide duplicates
                                           """;
    
    
    public const string GiveMotivationPrompt = """
                                            Give me some rangom quote or motivational phrase. It can be one or several sentences.
                                           """;
    
    /*public const string GiveAlternativesPrompt = """
                                                  There is a $term = '{{$term}}' in '{{$srcLang}}' and existing translations for it: $existingTranslations = {{$existingTranslations}} in '{{$destLang}}'.
                                                  Give me alternative translations for $term in {{$destLang}} and return response in JSON, for examples: {"translations": ["to long for", "to yearn for"]}
                                                  Exclude $existingTranslations from the result. Double check you do not provide duplicates
                                                 """;*/

    public const string CardExampleTaskPrompt = """
                                                There is a $term = '{{$term}}' in {{$learnLang}}. Generate {{$count}} example sentences that contain this term. Mask term inside sentences with *;
                                                After that, translate them into {{$nativeLang}};
                                                Return response in JSON as example { "sentences": [ {"example_original": "The progress of *mankind* is dependent on education." , "example_translated": "Прогресс *человечества* зависит от образования." }  ] };
                                                Double check that example_original must be in {{$learnLang}} and example_translated must be in {{$destLang}};
                                                Note that examples must be real word sentences and sound fluently (just as a human would said it);
                                                Consider the following translations: {{$translations}} as a background context;
                                                After you have completed all the steps, run the procedure of finding nonsense in your sentences. Be strict as this content is important and mistake can't be made. Take as much time as need.
                                                """;
}