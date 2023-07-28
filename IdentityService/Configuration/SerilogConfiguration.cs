using Serilog;

namespace IdentityService.Configuration;

internal static class SerilogConfiguration
{
    internal static void ConfigureSerilog(IConfiguration configuration)
    {
        var seqUrl = configuration["Serilog:SeqUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.Seq(string.IsNullOrEmpty(seqUrl) ? "http://localhost:5341" : seqUrl)
            .Enrich.WithProperty("ServiceName", "IdentityService")
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}
