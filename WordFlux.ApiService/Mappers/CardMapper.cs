﻿using System.Linq.Expressions;
using WordFlux.ApiService.Domain;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Mappers;

public static class CardMapper
{
    public static Expression<Func<Card, CardDto>> ToCardDto()
    {
        return x => new CardDto(x.Id, x.CreatedAt, x.Term, x.Level, x.Translations, x.ReviewInterval, x.Deck.Name, x.ImageUrl, x.NativeLanguage, x.LearnLanguage, x.SourceLanguage, x.TargetLanguage)
        {
            CardTaskExamples = x.ExampleTasks
        };
    }
}