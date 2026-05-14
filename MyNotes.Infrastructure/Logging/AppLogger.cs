using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using MyNotes.Application.Contracts.Logging;
using MyNotes.Infrastructure.Logging.Entities;

using Windows.ApplicationModel;
using Windows.Storage;

namespace MyNotes.Infrastructure.Logging;

internal sealed class AppLogger : IAppLogger, IDisposable
{
  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private static readonly string _publicKeyFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Keys", "log_public.pem");
  private readonly string _logFilePath;

  private readonly Aes _aes;
  private static readonly int _aesKeySize = 256;
  private static readonly int _aesIVSize = 128;
  private static readonly int _rsaKeySize = 4096;

  public AppLogger()
  {
    _aes = Aes.Create();
    _aes.KeySize = _aesKeySize;
    _aes.GenerateKey();
    _aes.GenerateIV();

    using (RSA rsa = RSA.Create())
    {
      rsa.ImportFromPem(File.ReadAllText(_publicKeyFilePath));
      byte[] encryptedAesKey = rsa.Encrypt([.. _aes.Key, .. _aes.IV], RSAEncryptionPadding.OaepSHA256);
      byte[] hash = SHA256.HashData(encryptedAesKey);
      string hex = Convert.ToHexStringLower(hash).Replace("-", "");
      string base64 = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');

      var logFolderPath = Path.Combine(_localFolder.Path, "Logs");
      var logFolderInfo = Directory.CreateDirectory(logFolderPath);

      _logFilePath = Path.Combine(logFolderInfo.FullName, $"applog_{base64}.dat");
      File.WriteAllBytes(_logFilePath, encryptedAesKey);
    }
  }

  public bool Disposed { get; private set; }

  private void Dispose(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {
        if (_aes is not null)
        {
          CryptographicOperations.ZeroMemory(_aes.Key);
          CryptographicOperations.ZeroMemory(_aes.IV);
          _aes.Dispose();

          if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length <= 540)
          {
            File.Delete(_logFilePath);
          }
        }
      }

      Disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = false };
  public void Write(Exception e)
  {
    var targetSite = e.TargetSite;
    ExceptionLogEntry entry = new()
    {
      Time = DateTimeOffset.UtcNow,
      HResult = e.HResult,
      Message = e.Message,
      Source = e.Source,
      StackTrace = e.StackTrace,
      TargetSiteName = targetSite?.Name,
      TargetSiteReflectedType = targetSite?.ReflectedType?.ToString(),
      TargetSiteAssembly = targetSite?.ReflectedType?.Assembly.FullName
    };

    var jsonString = JsonSerializer.Serialize(entry, _jsonSerializerOptions);
    if (jsonString is not null)
    {
      using var encryptor = _aes.CreateEncryptor();
      using MemoryStream memoryStream = new();
      using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
      cryptoStream.Write(Encoding.UTF8.GetBytes(jsonString), 0, jsonString.Length);
      cryptoStream.FlushFinalBlock();
      File.AppendAllBytes(_logFilePath, [.. BitConverter.GetBytes((int)memoryStream.Length), .. memoryStream.ToArray()]);
    }
  }
}
