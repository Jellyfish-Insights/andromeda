using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Andromeda.Common.Logging.Models {

    public class RuntimeLog {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime When { get; set; }

        public string Name { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        [Column(TypeName = "jsonb")]
        public string Data { get; set; }

        public string Exception { get; set; }

        public RuntimeLog() {}

        public RuntimeLog(string name, LogEvent e) {
            Message = e.MessageTemplate.ToString();
            When = e.Timestamp.UtcDateTime;
            Level = e.Level.ToString();
            Data = Property2JSON(e.Properties);
            Name = name;
            Exception = e.Exception?.ToString();
        }

        /**
           Taken from:
           https://github.com/serilog/serilog-formatting-compact/blob/dev/src/Serilog.Formatting.Compact/Formatting/Compact/CompactJsonFormatter.cs
         */
        private string Property2JSON(IReadOnlyDictionary<string, LogEventPropertyValue> properties) {
            var output = new StringWriter();
            var valueFormatter = new JsonValueFormatter(typeTagName: "$type");

            var first = true;

            output.Write('{');
            foreach (var property in properties) {
                var name = property.Key;
                if (name == LoggerFactory.LoggerNamePropertyName) {
                    continue;
                }
                if (name.Length > 0 && name[0] == '@') {
                    // Escape first '@' by doubling
                    name = '@' + name;
                }

                if (!first) output.Write(',');

                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                valueFormatter.Format(property.Value, output);
                first = false;
            }
            output.Write('}');

            return output.ToString();
        }

        /**
           examples:
            ParseJogFullName("a.b.c.d", '.', 1) returns ("a", "b.c.d")
            ParseJogFullName("a.b.c.d", '.', 2) returns ("a.b", "c.d")
            ParseJogFullName("a.b.c.d", '.', 4) returns ("a.b.c.d", "")
            ParseJogFullName("a.b.c.d", '.', 10) returns ("a.b.c.d", "")
         */
        static public (string FirstName, string SurName) ParseLogName(string fullName, char separator = '.', int namesOnFirstName = 1) {
            var x = fullName.Split(separator).ToList();
            if (x.Count() > namesOnFirstName) {
                var firstName = string.Join(separator.ToString(), x.GetRange(0, namesOnFirstName));
                var surName = string.Join(separator.ToString(), x.Skip(namesOnFirstName));
                return (FirstName : firstName, SurName : surName);
            }
            return (FirstName : fullName, SurName : "");
        }
    }
}
