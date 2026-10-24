using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Template.Tests.Endpoints;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSecret = "TestFixtureDeterministicKey_AtLeast32BytesLong_PRN232_ASM_2026!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("Jwt:Key", TestJwtSecret);
    }
}
