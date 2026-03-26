using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.ViewModels.Media.Providers;

internal class ImageCollectionKey : IEquatable<ImageCollectionKey>
{
  public required Guid Key { get; init; }

  public required WeakReference<ObservableCollection<ImageViewModel>> CollectionReference { get; init; }

  public bool Equals(ImageCollectionKey? other) => other is not null && this.Key == other.Key;

  public override bool Equals(object? obj) => Equals(obj);

  public override int GetHashCode() => Key.GetHashCode();

  public static bool operator ==(ImageCollectionKey i1, ImageCollectionKey i2) => i1.Equals(i2);
  public static bool operator !=(ImageCollectionKey i1, ImageCollectionKey i2) => !i1.Equals(i2);
}
