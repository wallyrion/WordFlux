using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordFlux.ApiService.DbConfigurations;

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