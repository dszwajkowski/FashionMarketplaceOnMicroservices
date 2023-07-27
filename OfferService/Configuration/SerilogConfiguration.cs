using Serilog;

namespace OfferService.Configuration;

internal static class SerilogConfiguration
{
    internal static void AddSerilog(IConfiguration configuration)
    {
        var seqUrl = configuration["Serilog:SeqUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.Seq(string.IsNullOrEmpty(seqUrl) ? "http://localhost:5341" : seqUrl)
            .Enrich.WithProperty("ServiceName", "OfferService")
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}
