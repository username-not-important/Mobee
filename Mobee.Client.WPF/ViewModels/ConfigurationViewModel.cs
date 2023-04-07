using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.ApplicationServices;

namespace Mobee.Client.WPF.ViewModels
{
    public partial class ConfigurationViewModel: ObservableObject
    {
        public class ServerItem
        {
            public string Name { get; set; }

            public string Address { get; set; }
        }

        public ConfigurationViewModel()
        {
            ServersList = new()
            {
                new ServerItem() { Name = "Global Mobee Server (free)", Address = "mobee.ir"},
                new ServerItem() {Name = "Test Mobee Server (local)", Address = "https://localhost:7016"}
            };

            serverAddress = ServersList.Last().Address;
        }

        public List<ServerItem> ServersList { get; }

        [ObservableProperty]
        private string serverAddress;

        [ObservableProperty]
        [NotifyPropertyChangedFor("CanLaunch")]
        private string filePath;
        
        public bool CanLaunch
        {
            get
            {
                try
                {
                    return File.Exists(filePath);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
    }
}
