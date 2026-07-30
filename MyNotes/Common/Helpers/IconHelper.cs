using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Models.Navigations;

using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace MyNotes.Common.Helpers;

internal static class IconHelper
{
  private static Uri GetMainUri(int icon) => new Uri($"ms-appx:///Assets/Icons/Main/{icon}");

  private static readonly float PrimaryIconScale = 1.0f;
  private static readonly float BadgeIconScale = 0.5f;

  public static BitmapImage GetIconImage(int icon) => new() { UriSource = GetMainUri(icon), DecodePixelType = DecodePixelType.Logical };

  public static async Task<BitmapImage> GetIconImage(int icon, GroupIconBadge groupIconBadge, bool showBadge)
  {
    var iconUri = GetMainUri(icon);

    var bitmapImage = new BitmapImage() { DecodePixelType = DecodePixelType.Logical, DecodePixelWidth = 40, DecodePixelHeight = 40 };

    if (showBadge && groupIconBadge != GroupIconBadge.None)
    {
      var iconFile = await StorageFile.GetFileFromApplicationUriAsync(iconUri);
      int badge = groupIconBadge switch
      {
        GroupIconBadge.Folder => (int)Templates.Icon.Emoji_OpenFileFolder,
        _ => throw new ArgumentException("")
      };
      var badgeFile = await StorageFile.GetFileFromApplicationUriAsync(GetMainUri(badge));
      using var iconStream = await iconFile.OpenReadAsync();
      using var badgeStream = await badgeFile.OpenReadAsync();

      var iconDecoder = await BitmapDecoder.CreateAsync(iconStream);
      var badgeDecoder = await BitmapDecoder.CreateAsync(badgeStream);

      var outputWidth = iconDecoder.PixelWidth;
      var outputHeight = iconDecoder.PixelHeight;

      var iconWidth = (uint)(outputWidth * PrimaryIconScale);
      var iconHeight = (uint)(outputHeight * PrimaryIconScale);
      var badgeWidth = (uint)(outputWidth * BadgeIconScale);
      var badgeHeight = (uint)(outputHeight * BadgeIconScale);

      var iconPixelData = await iconDecoder.GetPixelDataAsync(
        BitmapPixelFormat.Bgra8,
        BitmapAlphaMode.Straight,
        new BitmapTransform()
        {
          ScaledWidth = iconWidth,
          ScaledHeight = iconHeight,
          InterpolationMode = BitmapInterpolationMode.Fant
        },
        ExifOrientationMode.IgnoreExifOrientation,
        ColorManagementMode.DoNotColorManage);

      var badgePixelData = await badgeDecoder.GetPixelDataAsync(
        BitmapPixelFormat.Bgra8,
        BitmapAlphaMode.Straight,
        new BitmapTransform()
        {
          ScaledWidth = badgeWidth,
          ScaledHeight = badgeHeight,
          InterpolationMode = BitmapInterpolationMode.Fant
        },
        ExifOrientationMode.IgnoreExifOrientation,
        ColorManagementMode.DoNotColorManage);

      var iconBytes = iconPixelData.DetachPixelData();
      var badgeBytes = badgePixelData.DetachPixelData();

      byte[] outputBytes = new byte[outputWidth * outputHeight * 4];

      for (uint y = 0; y < iconHeight; y++)
      {
        for (uint x = 0; x < iconWidth; x++)
        {
          uint iconIndex = (y * iconWidth + x) * 4;
          uint dstIndex = (y * outputWidth + x) * 4;

          byte alpha = iconBytes[iconIndex + 3];
          if (alpha == 0)
          {
            continue;
          }

          outputBytes[dstIndex] = iconBytes[iconIndex];
          outputBytes[dstIndex + 1] = iconBytes[iconIndex + 1];
          outputBytes[dstIndex + 2] = iconBytes[iconIndex + 2];
          outputBytes[dstIndex + 3] = alpha;
        }
      }

      for (uint y = 0; y < badgeHeight; y++)
      {
        for (uint x = 0; x < badgeWidth; x++)
        {
          uint dstX = outputWidth - badgeWidth + x;
          uint dstY = outputHeight - badgeHeight + y;

          uint badgeIndex = (y * badgeWidth + x) * 4;
          uint dstIndex = (dstY * outputWidth + dstX) * 4;

          byte alpha = badgeBytes[badgeIndex + 3];
          if (alpha == 0)
          {
            continue;
          }

          outputBytes[dstIndex] = badgeBytes[badgeIndex];
          outputBytes[dstIndex + 1] = badgeBytes[badgeIndex + 1];
          outputBytes[dstIndex + 2] = badgeBytes[badgeIndex + 2];
          outputBytes[dstIndex + 3] = alpha;
        }
      }

      using var outputStream = new InMemoryRandomAccessStream();
      var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
      encoder.SetPixelData(
        BitmapPixelFormat.Bgra8,
        BitmapAlphaMode.Straight,
        outputWidth,
        outputHeight,
        96.0, 96.0,
        outputBytes);
      await encoder.FlushAsync();

      outputStream.Seek(0);
      await bitmapImage.SetSourceAsync(outputStream);
    }
    else
    {
      bitmapImage.UriSource = iconUri;
    }

    return bitmapImage;
  }
}
