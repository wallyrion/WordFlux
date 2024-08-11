namespace WordFlux.ApiService;

public static class AiSystemMessages
{
    
    public const string exampleResponseJson = """
                                              {"term":"to run", "list":[{"tr":"бежать","e_tr":"Мне нравится *бегать* утром","e_or":"I like *to run* in the morning", "u_f": 95}]},
                                              {"term":"adjictable", "suggestedTerm": "addictive", "list":[{"tr":"вызывающий привыкание ","e_tr":"Социальные сети могут *вызывать привыкание*","e_or":"Social media can be very *addictive*", "u_f": 90}]},
                                              {"term":"кошка", "list":[{"tr":"cat","e_tr":"This *cat* is very playful","e_or":"Эта *кошка* очень игривая", "u_f": 98}]}
                                              """;
    
    
    public const string RequestForAssistantWithArguments = $$$$""""
                                                               1. There is a term '{{$term}}'. Can be in English or Russian, determine language as original.
                                                               2. If 'term' in English, translate from English to Russian.
                                                               3. If 'term' in Russian, translate from Russian to English.
                                                               4. Check term '{{$term}}' is valid word or sentense in English or Russian. If not, try to give correct word in suggestedTerm field. Skip this field if term is valid.
                                                               5. Return me JSON with translations (up to 5) for this term in JSON format. There are examples of output: {{{{AiSystemMessages.exampleResponseJson}}}} where 'tr' - translation of 'term'; 'e_tr' - must be same language as 'tr'; 'e_or' - must be same language as 'term' or 'suggestedTerm'.
                                                               6. The term should be highlighted with * in usages.
                                                               7. Translations should be sorted from the most popular to the least popular.
                                                               8. u_f - popularity of translation from 0 to 100 (how often it is used in the context)
                                                               """";

}