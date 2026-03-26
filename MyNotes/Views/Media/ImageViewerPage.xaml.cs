using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using MyNotes.Models.Media;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

using Windows.Foundation;
using Windows.Foundation.Collections;

namespace MyNotes.Views.Media;

internal sealed partial class ImageViewerPage : Page
{
  private readonly ImageCollectionViewModel ViewModel;

  public ImageViewerPage(ImageCollectionKey imageCollectionKey)
  {
    InitializeComponent();
    var provider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModel = provider.Resolve(imageCollectionKey);
  }
}
