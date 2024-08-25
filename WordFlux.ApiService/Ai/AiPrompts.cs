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
                                              You are translator. There is a $term = '{{$term}}' that can be ONLY in these languages: {{$languages}}
                                              Determine the original language and translate it to another.
                                              If $term has typo, try to fix a typo and put corrected value into suggested_term.
                                              If $term has no grammar mistakes, do not include suggested_term into json.
                                              Provide up to 4 translations, but most proper translations must be first.
                                              $term may contain a note (clarification) that should be considered as a clue for the context. (example_output1)
                                              Provide original and translated language in the response in srcL and outL fields respectively.
                                              Response must be in JSON.
                                              example_output1: {"translations":["bow"], "srcL": "en-US", "outL": "ru-RU"} for input '$term' = 'лук (для стрельбы)'
                                              example_output2: {"suggested_term": "поощрять", "translations":["to encourage", "to promote", "to reward"], "srcL": "ru-RU", "outL": "en-US"} for input '$term' = 'поощрать'
                                              example_output3: {"suggested_term": "Tell me where I was wrong?", "translations":["Подскажи, где я был неправ?"], "srcL": "en-US", "outL": "ru-RU"} for input '$term' = 'Tall me where was I wrong?'
                                              """;

    public const string GiveTranslationExamples = """
                                               There is a $term = '{{$term}}' and list of translation_items for it: {{$translations}} in {{$destLang}}.
                                               For each translation_item you should give example of usage and map to the object:
                                               {"tr": "translation_item in {{$destLang}}. You should not change it.", "l": "level of complexity of translation from A0 to C2", "p": "integer Value 0-100 estimate how often this translation is used.", "e_tr": "example of usages for *translation_item*. Should be in {{$destLang}}.", "e_or": "'e_tr' field but translated back to {{$srcLang}}."}
                                               Mapped objects should be in the same order and same count as original 'translations' array.
                                               Highlight term with '*' in 'e_tr' and 'e_or' example fields.
                                               'e_tr' should be in {{$destLang}} and 'e_or' should be in {{$srcLang}}.
                                               Response must be in JSON format in the following template:
                                               Consider example1 for input 'term' = "кошка" and 'translations' = ["cat", "feline"]: {"translations": [{"tr": "cat", "l": "A1", "p": "98", "e_tr": "This *cat* is very playful", "e_or": "Эта *кошка* очень игривая"}, {"tr": "feline", "l": "B2", "p": "30", "e_tr": "His *feline* reflexes allowed him to catch the ball.", "e_or": "Его *кошачья* реакция позволила ему поймать мяч"}]
                                               Consider example2 for input 'term' = "my" and 'translations' = ["мой", "моя"]: {"translations": [{"tr": "мой", "l": "A1", "p": "90", "e_or": "This is *my* house", "e_tr": "Это *мой* дом"}, {"tr": "моя", "l": "A1", "p": "80", "e_or": "This is *my* car", "e_tr": "Это *моя* машина"}]
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