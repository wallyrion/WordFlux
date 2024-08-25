using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordFlux.ApiService.Domain;

namespace WordFlux.ApiService.Persistence.DbConfigurations;

internal sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.OwnsMany(x => x.Translations, y =>
        {
            y.ToJson();
        });
    }
}