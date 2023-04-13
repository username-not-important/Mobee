using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mobee.Client.WPF.ViewModels;

namespace Mobee.Client.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ChatSidebar.xaml
    /// </summary>
    public partial class ChatSidebar : UserControl
    {
        public ChatSidebar()
        {
            DataContextChanged += OnDataContextChanged;

            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null && !(DataContext is ChatViewModel))
                return;

            var viewModel = DataContext as ChatViewModel;

            viewModel.Messages.CollectionChanged += MessagesOnCollectionChanged;
        }
        
        private void MessagesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _MessagesScrollviewer.ScrollToEnd();
            }
        }

        private void _MessageTextbox_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext == null && !(DataContext is ChatViewModel))
                    return;

                var viewModel = DataContext as ChatViewModel;
                
                viewModel.SendMessage();
            }
        }
    }
}
