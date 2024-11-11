using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordFlux.Domain.Domain;

namespace WordFlux.Infrastructure.Persistence.DbConfigurations;

internal sealed class DeckConfiguration : IEntityTypeConfiguration<Deck>
{
    public void Configure(EntityTypeBuilder<Deck> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(256);

        builder.OwnsOne(x => x.Export, export =>
        {
            export.ToJson();

            export.OwnsMany(x => x.Items, i =>
            {
            });
            /*export.OwnsMany(p => p.Items, item =>
            {
                item.ToJson();
            });*/

        });
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.Cards)
            .WithOne(x => x.Deck)
            .HasForeignKey(x => x.DeckId);
    }
}