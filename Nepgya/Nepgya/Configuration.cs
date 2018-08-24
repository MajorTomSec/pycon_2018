// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;

namespace Nepgya
{
    class Configuration
    {
        public string DiscordToken { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TokenType DiscordTokenType { get; set; }
        public string Logs { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public LogEventLevel MinimumLogLevel { get; set; }
    }
}