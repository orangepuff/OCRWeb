using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCRWeb.ProjectManagement.Domain.Entity;

namespace OCRWeb.ProjectManagement.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="Project"/> to [project].[Projects]. DB columns use the type-prefix
/// convention (s=nvarchar, i=int, dt=datetime2).
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasColumnName("sName").HasMaxLength(200).IsRequired();

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");
    }
}
