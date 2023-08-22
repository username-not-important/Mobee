// 
// Copyright (C) 2023-2023
// Author: Alireza Joonbakhsh - All Rights Reserved
// 
// Mobee - Mobee.Client.WPF
// FontChangeBehavior.cs
// 
// You may not use, modify, or publish this code
// without crediting the original author

using System.Windows;
using System.Windows.Media;
using Localization;
using Microsoft.Xaml.Behaviors;
using Window = System.Windows.Window;

namespace Mobee.Client.WPF.Behaviors
{
    public class FontChangeBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += (sender, args) =>
            {
                var window = sender as Window;
                if (window == null)
                    return;

                var cultureName = LocalizationManager.CurrentCulture.Name;
                if (cultureName == "fa-IR")
                    window.FontFamily = Application.Current.TryFindResource("PelakFont") as FontFamily;
            };

            base.OnAttached();
        }
    }
}
