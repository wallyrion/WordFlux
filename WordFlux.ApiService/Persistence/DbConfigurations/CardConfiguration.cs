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
        
        builder.Property(e => e.SourceLanguage)
            .HasMaxLength(5)
            .IsUnicode(false);

        builder.Property(e => e.TargetLanguage)
            .HasMaxLength(5) 
            .IsUnicode(false);        
        builder.Property(e => e.NativeLanguage)
            .HasMaxLength(5)
            .IsUnicode(false);

        builder.Property(e => e.LearnLanguage)
            .HasMaxLength(5) 
            .IsUnicode(false);
    }
}