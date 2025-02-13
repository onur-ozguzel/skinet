using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole { Id = "5aba7e4d-0446-4396-9499-06c3b6e11ca1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "928a2c10-7986-4a42-97dc-a18a56f3b4d6", Name = "Customer", NormalizedName = "CUSTOMER" }
        );
    }
}
