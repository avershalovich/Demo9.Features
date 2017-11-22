using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo9.Console
{
    public class Settings
    {
        public static Guid OnlineEventId = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["interaction.OnlineEventlId"]) ?
            Guid.Parse("{D6CA9D8C-1F03-4591-B668-33E249E6DCB9}") // "Submit success" event
            : Guid.Parse(ConfigurationManager.AppSettings["interaction.OnlineChannelId"]);

        public static Guid OnlineChannelId = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["interaction.OnlineChannelId"]) ?
            Guid.Parse("{59BD107F-D725-4BA1-91C6-61BEE3CB768C}") // "Other apps" channel
            : Guid.Parse(ConfigurationManager.AppSettings["interaction.OnlineChannelId"]);
    }
}
