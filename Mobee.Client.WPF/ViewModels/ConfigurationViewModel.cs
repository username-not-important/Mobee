using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localization;
using Microsoft.VisualBasic.ApplicationServices;

namespace Mobee.Client.WPF.ViewModels
{
    public partial class ConfigurationViewModel: ObservableValidator
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
                new ServerItem() { Name = LocalizationManager.Instance["Config.Server.Option1"], Address = "https://mobees.ir"},
                new ServerItem() { Name = LocalizationManager.Instance["Config.Server.Option2"], Address = "https://joyn.ir"},
#if DEBUG
                new ServerItem() {Name = "Test Mobee Server (local)", Address = "https://localhost:7016"},
                new ServerItem() {Name = "Test Joyn Server (local)", Address = "https://localhost:44321"}
#endif
            };

            serverAddress = ServersList.First().Address;
        }

        public List<ServerItem> ServersList { get; }

        [ObservableProperty]
        private string serverAddress;
        
        [Required]
        [MinLength(3)]
        [ObservableProperty]
        [NotifyPropertyChangedFor("CanLaunch")]
        private string? userName;
        
        [Required]
        [MinLength(3)]
        [ObservableProperty]
        [NotifyPropertyChangedFor("CanLaunch")]
        private string? groupName;
        
        [Required]
        [ObservableProperty]
        [CustomValidation(typeof(ConfigurationViewModel), nameof(ValidateFilePath))]
        [NotifyPropertyChangedFor("CanLaunch")]
        private string filePath;
        
        public static ValidationResult ValidateFilePath(string filePath, ValidationContext context)
        {
            ConfigurationViewModel instance = (ConfigurationViewModel)context.ObjectInstance;
            
            try
            {
                var exists = File.Exists(filePath);
                if (exists)
                    return ValidationResult.Success;
                else
                    return new("File does not exist!");
            }
            catch (Exception e)
            {
                return new("File read error!");
            }
        }

        public bool CanLaunch
        {
            get
            {
                ValidateAllProperties();

                return !HasErrors;
            }
        }
    }
}
