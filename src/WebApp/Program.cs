using ApplicationModels.Models.AccountViewModels;
using ApplicationModels.Models.DataViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TypeScriptBuilder;
using WebApp.Services;
using System.IO;

namespace WebApp {
    public class Program {
        public static void Main(string[] args) {
            #if !RELEASE
            TypeScriptGeneratorOptions generatorOptions =
                new TypeScriptGeneratorOptions {
                EmitIinInterface = false,
                IgnoreNamespaces = true
            };

            TypeScriptGenerator generator =
                new TypeScriptGenerator(generatorOptions)
                    .ExcludeType(typeof(Program))
                    .AddCSType(typeof(TimeSeries))
                    .AddCSType(typeof(TimeSeriesDataGroup))
                    .AddCSType(typeof(DemographicData))
                    .AddCSType(typeof(DemographicDataItem))
                    .AddCSType(typeof(MetricInfo))
                    .AddCSType(typeof(Metric))
                    .AddCSType(typeof(PersonaMetric))
                    .AddCSType(typeof(Tag))
                    .AddCSType(typeof(Source))
                    .AddCSType(typeof(VideoMetric))
                    .AddCSType(typeof(Video))
                    .AddCSType(typeof(ExternalLoginViewModel))
                    .AddCSType(typeof(EditType))
                    .AddCSType(typeof(AddOrRemove))
                    .AddCSType(typeof(VideoEdit))
                    .AddCSType(typeof(TagEdit))
                    .AddCSType(typeof(TagEdits))
                    .AddCSType(typeof(VideoEdits))
                    .AddCSType(typeof(ArchiveMode))
                    .AddCSType(typeof(PersonaVersion))
                    .AddCSType(typeof(PersonaVersionEdit))
                    .AddCSType(typeof(PersonaVersionEdits))
                    .AddCSType(typeof(AccountEdit))
                    .AddCSType(typeof(SingleAccountEdit))
                    .AddCSType(typeof(AccountInfo))
                    .AddCSType(typeof(AllAccountsInfo))
                    .AddCSType(typeof(TimeSeriesChartData))
                    .AddCSType(typeof(ChartObject))
                    .AddCSType(typeof(AuthStateInfo));

            // TypeScriptBuilder does not seem to support adding "| null" to some type, but this is needed for
            // VideoEdit.cs, for the MetaTags attribute, so doing it manually here
            var types = generator.ToString()
                            .Replace(
                "metaTags?: { [index: string]: string }",
                "metaTags?: { [index: string]: string | null }"
                );
            File.WriteAllText("ClientApp/types.ts", types);
            // Putting this initialization logic here prevents Initialize to run on dotnet ef commands
            #endif
            var webHost = BuildWebHost(args);
            #if !NOAUTH
            using (var scope = webHost.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                services.GetService<IDbInitializer>().Initialize();
            }
            #endif
            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseUrls("http://0.0.0.0:5000")
            .Build();
    }
}
