using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.EntityFrameworkCore;

namespace Common.Logging {

    static public class LoggerFactory {

        public const string LoggerNamePropertyName = "LoggerName";
        const string OutputTemplateFile = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        const string OutputTemplateConsole = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{LoggerName}] {Message:lj}{NewLine}{Exception}";

        static public string LogFileName(string name) {
            return "logs/" + name + ".log";
        }

        static public Logger GetLogger<T>(string name = "default") where T : DbContext, new() {
            return new LoggerConfiguration()
                       .MinimumLevel.Verbose()
                       .Enrich.WithProperty(LoggerNamePropertyName, name)
                       .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: OutputTemplateConsole, theme: ConsoleTheme.None)
                       .WriteTo.File(LogFileName(name), restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: OutputTemplateFile, fileSizeLimitBytes: 5000000, rollOnFileSizeLimit: true, retainedFileCountLimit: 2)
                       .WriteTo.Database<T>(name, restrictedToMinimumLevel: LogEventLevel.Information)
                       .CreateLogger();
        }

        static public Logger GetEfLogger() {
            return new LoggerConfiguration()
                       .MinimumLevel.Verbose()
                       .Enrich.WithProperty(LoggerNamePropertyName, "EF")
                       .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning, outputTemplate: OutputTemplateConsole, theme: ConsoleTheme.None)
                       .CreateLogger();
        }

        static public Logger GetFacebookLogger() {
            return new LoggerConfiguration()
                       .MinimumLevel.Verbose()
                       .Enrich.WithProperty(LoggerNamePropertyName, "FacebookFetcher")
                       .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: OutputTemplateConsole, theme: ConsoleTheme.None)
                       .WriteTo.File(LogFileName("FacebookFetcher"), restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: OutputTemplateFile, shared: true, fileSizeLimitBytes: 5000000, rollOnFileSizeLimit: true, retainedFileCountLimit: 2)
                       .CreateLogger();
        }

        static public Logger GetTestLogger(string name = "Test") {
            return new LoggerConfiguration()
                       .MinimumLevel.Verbose()
                       .Enrich.WithProperty(LoggerNamePropertyName, name)
                       .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: OutputTemplateConsole, theme: ConsoleTheme.None)
                       .CreateLogger();
        }
    }
}
