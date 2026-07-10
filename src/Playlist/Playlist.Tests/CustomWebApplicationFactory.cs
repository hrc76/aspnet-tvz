using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Playlist.Data;

namespace Playlist.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Ovdje pripremamo sigurnu testnu verziju aplikacije bez prave Azure baze.
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

            builder.ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();

                var musicBarDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MusicBarDbContext>));
                if (musicBarDescriptor != null)
                {
                    services.Remove(musicBarDescriptor);
                }

                // MusicBar podaci koriste bazu samo u memoriji i nestaju nakon testova.
                services.AddDbContext<MusicBarDbContext>(options =>
                {
                    options.UseInMemoryDatabase("PlaylistTests");
                });

                var identityDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (identityDescriptor != null)
                {
                    services.Remove(identityDescriptor);
                }

                // Identity korisnici i role takoder koriste izoliranu bazu u memoriji.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IdentityTests");
                });

                // Lazna autentikacija omogucuje testiranje Admin ruta bez stvarnog logina.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
            });
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "TestScheme";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Posebno zaglavlje simulira potpuno neprijavljenog korisnika.
            if (Request.Headers.TryGetValue("X-Test-Auth", out var value) && value == "none")
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Svi ostali testni zahtjevi dobivaju identitet s Admin rolom.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
