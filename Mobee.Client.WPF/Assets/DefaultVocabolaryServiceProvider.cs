using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Localization;

namespace Mobee.Client.WPF.Assets
{
    public class DefaultVocabolaryServiceProvider : IVocabolaryServiceProvider
    {
        Dictionary<string, Vocabolary> all = null;

        public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null)
            => Task.FromResult(1);

        public Task Initialize()
        {
            all = new Dictionary<string, Vocabolary>
            {
                {"fa-IR", new Vocabolary {
                        { "Config.Screen.Size","560" },
                        { "Config.Screen.Welcome","به موبی خوش آمدید" },
                        { "Config.Screen.Subtitle","پخش همزمان فیلم و ویدئو" },
                        { "Config.Screen.ApiVersion","نسخه فعلی:" },
                        { "Config.Screen.Server","سرور" },
                        { "Config.Screen.ServerDescription","از چه طریقی به پارتنر خود وصل شوید:" },
                        { "Config.Screen.User","کاربر" },
                        { "Config.Screen.UserDescription","نام شما و نام گروه شما:" },
                        { "Config.Screen.UserHint","نام شما" },
                        { "Config.Screen.GroupHint","نام گروه" },
                        { "Config.Screen.UserTooltip","(الزامی)" },
                        { "Config.Screen.GroupTooltip","(الزامی) مشترک بین شما و پارتنرتان" },
                        { "Config.Screen.MediaFile","فایل ویدئویی/صوتی" },
                        { "Config.Screen.MediaFileDescription","چیزی که می خواهید تماشا کنید:" },
                        { "Config.Screen.Browse","انتخاب" },
                        { "Config.Screen.Launch","بزن بریم!" },

                        { "MainWindowTitle","موبی" },
                        { "Direction", FlowDirection.RightToLeft.ToString() },
                        
                        // Config.Server.Options:
                        { "Config.Server.Option1", "سرور رایگان موبی"},
                        { "Config.Server.Option2", "سرور رایگان جوین"},

                        { "ChangeYourLanguageText", "Cambia la tua lingua"},
                        { "AboutText","Libraria .net che può aiutarti a gestire la localizzazione della localizzazione della tua applicazione. La libreria include estensopme Wpf, estensione Xamarin and estensione Mvc. " },
                    }
                },
                {"en-US", new Vocabolary {
                        { "Config.Screen.Size","540" },
                        { "Config.Screen.Welcome","Welcome to Mobee" },
                        { "Config.Screen.Subtitle","Synced Video and Movie Player" },
                        { "Config.Screen.ApiVersion","Currently running api version" },
                        { "Config.Screen.Server","Server" },
                        { "Config.Screen.ServerDescription","Connect to your partner through:" },
                        { "Config.Screen.User","User" },
                        { "Config.Screen.UserDescription","Your username and the group name:" },
                        { "Config.Screen.UserHint","Your Alias" },
                        { "Config.Screen.GroupHint","Group Name" },
                        { "Config.Screen.UserTooltip","(Required)" },
                        { "Config.Screen.GroupTooltip","(Required) Same with your Partner's" },
                        { "Config.Screen.MediaFile","Media File" },
                        { "Config.Screen.MediaFileDescription","File to watch on this computer:" },
                        { "Config.Screen.Browse","Browse" },
                        { "Config.Screen.Launch","Let's Go!" },

                        { "MainWindowTitle","Mobee Player" },
                        { "Direction", FlowDirection.RightToLeft.ToString() },

                        // Config.Server.Options:
                        { "Config.Server.Option1", "Mobee Server (free)"},
                        { "Config.Server.Option2", "Joyn Server (free)"},

                        { "ChangeYourLanguageText", "Change your language"},
                        { "AboutText","A library for .net that can help you to manage the localization in your application. The library includes Wpf extension, Xamarin extension and Mvc extension. " },
                    }
                }
            };
            return Task.FromResult(all);
        }

        public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
        {
            string cultureDefault = "fa-IR";
            if (all.ContainsKey(cultureInfo.ToString()))
                cultureDefault = cultureInfo.ToString();
            return Task.FromResult<IVocabolary>(all[cultureDefault]);
        }

        public Task SaveAsync(IVocabolary vocabolary)
            => Task.FromResult(1);
    }
}
