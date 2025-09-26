using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;

namespace PDM.Src.Converters
{
    public class ImageDataToImageSourceConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<string, WeakReference<ImageSource>> _imageCache = new();
        private const int MaxCacheSize = 100;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[] imageData || imageData.Length == 0)
                return null;

            // Create cache key based on data hash
            var cacheKey = GetCacheKey(imageData);
            
            // Try to get from cache first
            if (_imageCache.TryGetValue(cacheKey, out var weakRef) && 
                weakRef.TryGetTarget(out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                using var ms = new MemoryStream(imageData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = 250; // Match your MaxWidth
                image.DecodePixelHeight = 100; // Match your MaxHeight
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                // Add to cache with size limit
                if (_imageCache.Count > MaxCacheSize)
                {
                    // Remove old entries
                    var keysToRemove = new List<string>();
                    foreach (var kvp in _imageCache)
                    {
                        if (!kvp.Value.TryGetTarget(out _))
                            keysToRemove.Add(kvp.Key);
                    }
                    foreach (var key in keysToRemove)
                        _imageCache.TryRemove(key, out _);
                }

                _imageCache[cacheKey] = new WeakReference<ImageSource>(image);
                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetCacheKey(byte[] data)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(data, 0, Math.Min(1024, data.Length)); // Hash first 1KB for performance
            return System.Convert.ToBase64String(hash);
        }
    }
}