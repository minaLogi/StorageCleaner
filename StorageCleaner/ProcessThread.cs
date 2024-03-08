using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Windows.Networking.Vpn;
using Windows.Storage;

namespace StorageCleaner
{
    public class ProcessThread
    {
        public ThreadMode ThreadMode;
        private static bool LoadEnded = false;
        private StorageFolder RootFolder;
        public int ID;
        public bool IsRunning { get; private set; } = false;
        public static Queue<StorageFile> Queue = new();
        public static int fileCount = 0;
        public static int checkedCount
        {
            get {
                var qc = 0;
                lock (Queue)
                {
                    qc = Queue.Count;
                }
                return fileCount - qc;
            }
        }
        public ProcessThread(int id, ThreadMode threadMode, StorageFolder rootFolder)
        {
            this.ID = id;
            ThreadMode = threadMode;
            RootFolder = rootFolder;
        }
        public async void Start()
        {
            IsRunning = true;
            switch (ThreadMode)
            {
                case ThreadMode.Find:
                    await Task.Factory.StartNew(StartFindingFiles);
                    break;
                case ThreadMode.Calc:
                    await Task.Factory.StartNew(CalcFiles);
                    
                    break;
            }
            
        }
        public async void StartFindingFiles()
        {
            LoadEnded = await FindFilesAsync(RootFolder);
            Debug.WriteLine("Queue Ended");
        }
        public async Task<bool> FindFilesAsync(StorageFolder folder)
        {
            var folders = await folder.GetFoldersAsync();
            var files = await folder.GetFilesAsync();
            if (folders.Count != 0)
            {
                foreach (var i in folders)
                {
                    await FindFilesAsync(i);
                }
            }
            
            if(files.Count != 0)
            {
                foreach (var i in files)
                {
                    lock (Queue)
                    {
                        Queue.Enqueue(i);
                    }
                    fileCount++;
                }
            }
            IsRunning = false;
            return true;
        }
        public async void CalcFiles()
        {
            while ((LoadEnded == false || Queue.Count != 0) && DuplicationsWindow.IsRunning)
            {
                StorageFile target = null;
                GC.Collect();
                lock (Queue)
                {
                    if (Queue.Count > 0)
                    {
                        target = Queue.Dequeue();
                    }

                }
                if(target != null)
                {
                    Debug.WriteLine(target.Path);
                    FilesDataBase.CompareFileData(target);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
            IsRunning = false;
            if (ID == 1)
            {
                while (DuplicationsWindow.Instance.ProcessThreads.Select(t => t.IsRunning).Contains(true))
                {
                    await Task.Delay(1000);
                }
                await Task.Delay(1000);
                foreach (var i in FilesDataBase.Files)
                {
                    Debug.WriteLine(i.Name);
                }
                foreach (var i in FilesDataBase.Duplications)
                {
                    foreach (var j in i.FullPath)
                    {
                        Debug.Write(j+",");
                    }
                    Debug.Write("\n");
                }
            }
        }

    }
    public enum ThreadMode
    {
        Find,
        Calc
    }
}
