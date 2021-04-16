using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace EasyRpc.AspNetCore.Assets
{
    public class FileAssetCacheEntry
    {
        public string FileName { get; set; }

        public string ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public byte[] FileBytes { get; set; }

        public DateTime LastModified { get; set; }

        public string ETag { get; set; }

        public DateTime LastAccess { get; set; }
    }

    public interface IFileAssetCache
    {
        FileAssetCacheEntry LocateEntry(string fileName);

        void AddEntry(FileAssetCacheEntry entry);
    }

    public class FileAssetCacheScoped : IFileAssetCache
    {
        private readonly ConcurrentDictionary<string, FileAssetCacheEntry> _entries = 
            new ConcurrentDictionary<string, FileAssetCacheEntry>();

        private int _currentSize;
        private int _maxSize = 100 * 1000 * 1024; // 100MB
        private int _purge;
        
        public FileAssetCacheEntry LocateEntry(string fileName)
        {
            if (_entries.TryGetValue(fileName, out var entry))
            {
                entry.LastAccess = DateTime.Now;
                
                return entry;
            }

            return null;
        }

        public void AddEntry(FileAssetCacheEntry entry)
        {
            var oldSize = 0;

            entry.LastAccess = DateTime.Now;
            
            _entries.AddOrUpdate(entry.FileName, entry, (s, cacheEntry) =>
            {
                oldSize = cacheEntry.FileBytes.Length;

                return entry;
            });

            Interlocked.Add(ref _currentSize, -oldSize);
            Interlocked.Add(ref _currentSize, entry.FileBytes.Length);

            if (_currentSize > _maxSize)
            {
                if (Interlocked.CompareExchange(ref _purge, 1, 0) == 0)
                {
                    Task.Run(ClearOldEntries);
                }
            }
        }

        private void ClearOldEntries()
        {
            try
            {
                var entries = new List<FileAssetCacheEntry>(_entries.Values);

                entries.Sort((x, y) => DateTime.Compare(x.LastAccess, y.LastAccess));

                var targetSize = (int) (_maxSize * .75);

                foreach (var entry in entries)
                {
                    _entries.TryRemove(entry.FileName, out var oldEntry);

                    Interlocked.Add(ref _currentSize, -entry.FileBytes.Length);

                    if (_currentSize < targetSize)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // log this
            }
            finally
            {
                Interlocked.Exchange(ref _purge, 0);
            }
        }
    }
}
