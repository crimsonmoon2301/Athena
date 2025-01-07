using Galadarbs_IT23033.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galadarbs_IT23033.MVVM.ViewModel
{
    internal class BuilderConstants : ObservableObject
    {
        public static readonly Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            { "neutral" , "Any Language"},
            { "ar-sa", "Arabic (Saudi Arabia)" },
            { "bg-bg","Bulgarian" },
            { "cs-cz","Czech" },
            { "da-dk", "Danish" },
            { "de-de", "German" },
            { "el-gr", "Greek" },
            { "en-gb", "English (United Kingdom)" },
            { "en-us", "English (United States)" },
            { "es-es", "Spanish (Spain)" },
            { "es-mx", "Spanish (Mexico)" },
            { "et-ee", "Estonian" },
            { "fi-fi", "Finnish" },
            { "fr-ca", "French (Canada)" },
            { "fr-fr", "French (France)" },
            { "he-il", "Hebrew" },
            { "hr-hr", "Croatian" },
            { "hu-hu", "Hungarian" },
            { "it-it", "Italian" },
            { "ja-jp", "Japanese" },
            { "ko-kr", "Korean" },
            { "lt-lt", "Lithuanian" },
            { "lv-lv", "Latvian" },
            { "nb-no", "Norwegian (Bokmal)" },
            { "nl-nl", "Dutch" },
            { "pl-pl", "Polish" },
            { "pt-br", "Portuguese (Brazil)" },
            { "pt-pt", "Portuguese (Portugal)" },
            { "ro-ro", "Romanian" },
            { "ru-ru", "Russian" },
            { "sk-sk", "Slovak" },
            { "sl-si", "Slovenian" },
            { "sr-latn-rs", "Serbian (Latin)" },
            { "sv-se", "Swedish" },
            { "th-th", "Thai" },
            { "tr-tr", "Turkish" },
            { "uk-ua", "Ukrainian" },
            { "zh-cn", "Chinese (Simplified)" },
            { "zh-hk", "Chinese (Hong Kong)" },
            { "zh-tw", "Chinese (Traditional)" },
        };

        public static readonly Dictionary<string, string> Editions = new Dictionary<string, string>
        {
            { "CORE", "Windows Home" },
            { "PROFESSIONAL","Windows Pro" },
            { "COREN", "Windows Home N" },
            { "PROFESSIONALN", "Windows Pro N"}
        };

        public static readonly Dictionary<string, string> ServerEditions = new Dictionary<string, string>
        {
            { "serverstandard", "Windows Server Standard" },
            { "serverstandardcore", "Windows Server Standard (Core)"},
            { "serverdatacenter", "Windows Server Datacenter" },
            { "serverdatacentercore", "Windows Server Datacenter (Core)" },
            { "serverturbine","Windows Server Datacenter: Azure Edition" },
            { "serverturbinecore", "Windows Server Datacenter: Azure Edition (Core)" }
        };
    }
}
