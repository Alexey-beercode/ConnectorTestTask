using FuturesTestTask.MarketDataService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database.Configurations
{
    public class FuturesDataConfiguration : IEntityTypeConfiguration<FuturesData>
    {
        public void Configure(EntityTypeBuilder<FuturesData> builder)
        {
            builder.ToTable("futures-data"); 

            builder.HasKey(x => x.Id); 
            builder.Property(x => x.Id)
                .ValueGeneratedNever(); 

            builder.Property(x => x.Timestamp)
                .IsRequired()
                .HasColumnType("timestamp"); 

            builder.Property(x => x.QuarterPrice)
                .IsRequired()
                .HasPrecision(18, 8);

            builder.Property(x => x.BiQuarterPrice)
                .IsRequired()
                .HasPrecision(18, 8); 

            builder.Property(x => x.PriceDifference)
                .IsRequired()
                .HasPrecision(18, 8);
            
            builder.HasIndex(x => x.Timestamp)
                .HasDatabaseName("idx_futures_data_timestamp");
        }
    }
}