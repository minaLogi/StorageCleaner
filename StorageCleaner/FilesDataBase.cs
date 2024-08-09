using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Windows.Storage;

namespace StorageCleaner
{
    public static class FilesDataBase
    {
        public static bool IgnoreFileName = false;
        public static ConcurrentBag<DuplicatingFiles> Duplications
        {
            get => _duplications;
            set
            {
                _duplications = value;
                
            }
        }
        public static ConcurrentBag<DuplicatingFiles> _duplications = new();
        public static ConcurrentBag<InfoFile> Files = new();
        public static void CompareFileData(StorageFile file)
        {
            int hash = ComputeFileHash(file);
            var infoFile = new InfoFile(file.Name, file.Path, hash);
            lock (Files)
            {
                Files.Add(infoFile);
                lock (Duplications)
                {
                    if (Files.Where(x => x.Data == hash).Count() > 1 && (Files.Where(x => x.Name == file.Name).Count() > 1 || IgnoreFileName))
                    {
                        var w = Duplications.Where(x => x.Name.First() == infoFile.Name);
                        //同一ファイルを発見
                        if (w.Count() > 0)
                        {
                            w.First().Add(ref infoFile);
                        }
                        else
                        {
                            var r = Files.Where(x => x.Data == hash).First();
                            Duplications.Add(new DuplicatingFiles(ref r, ref infoFile));
                        }
                    }
                }
            }
        }

        public static int ComputeFileHash(StorageFile file)
        {
            try
            {
                var hashProvider = MD5.Create();
                FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read);
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, bs.Length);
                var r = BitConverter.ToInt32(hashProvider.ComputeHash(bs), 0);
                return r;
            }catch (Exception ex)
            {
                Debug.WriteLine(ex.Message); return 0;
            }
            
        }
    }
    public struct InfoFile
    {
        public string Name;
        public string FullPath;
        public int Data;

        public InfoFile(string name, string fullpath, int data)
        {
            Name = name;
            FullPath = fullpath;
            Data = data;
        }
    }
    public struct DuplicatingFiles
    {
        public List<string> Name;
        public List<string> FullPath;
        public List<InfoFile> infoFiles;

        public DuplicatingFiles(ref InfoFile file1, ref InfoFile file2)
        {
            Name = new List<string> { file1.Name, file2.Name };
            FullPath = new List<string> { file1.FullPath, file2.FullPath };
            infoFiles = new List<InfoFile> { file1, file2 };
        }

        public void Add(ref InfoFile file)
        {
            Name.Add(file.Name);
            FullPath.Add(file.FullPath);
            infoFiles.Add(file);
        }
    }
}
