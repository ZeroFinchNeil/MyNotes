using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using MyNotes.Models.Media;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.Views.Media;

using Windows.Foundation;
using Windows.Foundation.Collections;

namespace MyNotes.Views.Windows;
internal sealed partial class ImageViewerWindow : Window
{
  #region Object Lifetime Management

  public ImageViewerWindow(ImageCollectionKey imageCollectionKey)
  {
    InitializeComponent();

    this.Content = new ImageViewerPage(imageCollectionKey);
    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  private void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;
  }
  #endregion
}
