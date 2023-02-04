using CommunityToolkit.Mvvm.ComponentModel;

namespace Mobee.Client.WPF.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isConnected;
}