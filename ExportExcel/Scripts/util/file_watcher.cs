using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 16:19:10
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class FileWatcher
    {
        public class Changes
        {
            public List<(string, WatcherChangeTypes)> _data;
            public Changes()
            {
                _data = new List<(string, WatcherChangeTypes)>();
            }

            public void Add(string file_path, WatcherChangeTypes type)
            {
                _data.Add((file_path, type));
            }

            public void Clear()
            {
                _data.Clear();
            }

            public void CopyFrom(Changes other)
            {
                if (other == null)
                    return;
                _data.AddRange(other._data);
            }

            public int Count { get { return _data.Count; } }

            public void Print()
            {
                foreach (var p in _data)
                    Console.WriteLine("{0}, {1}", p.Item2, p.Item1);
            }
        }

        public const int C_DICT_CAP = 1000;
        public List<FileSystemWatcher> _watcher_list = new List<FileSystemWatcher>();
        public Dictionary<string, long> _file_dict = new Dictionary<string, long>(C_DICT_CAP);
        public Changes _changes = new Changes();
        public Changes _out_changes = new Changes();

        public FileWatcher()
        {
        }

        public static FileWatcher Create(ExeConfig setting)
        {
            FileWatcher watcher = new FileWatcher();

            foreach (var file_path in setting.excel_paths)
            {
                var full_path = Path.GetFullPath(file_path);
                if (File.Exists(full_path))
                {
                    watcher._add_watch_path(Path.GetDirectoryName(full_path), Path.GetFileName(full_path));
                }
                else if (Directory.Exists(full_path))
                {
                    watcher._add_watch_path(full_path, "*.xlsx");
                }
            }
            return watcher;
        }



        public Changes Select()
        {
            _out_changes.Clear();

            lock (this)
            {
                if (_changes.Count == 0)
                    return _out_changes;

                _out_changes.CopyFrom(_changes);
                _changes.Clear();

                //excel 保存的时候,会生成很奇怪的路径
                if (_file_dict.Count > C_DICT_CAP)
                    _file_dict.Clear();
            }
            return _out_changes;
        }

        public void _add_watch_path(string dir, string filter)
        {
            if (!File.Exists(dir) && !Directory.Exists(dir))
                return;

            var watcher = new FileSystemWatcher();
            _watcher_list.Add(watcher);
            watcher.Path = dir;
            watcher.Filter = filter;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += _on_changed;
            watcher.Deleted += _on_changed;
            watcher.Created += _on_changed;
            watcher.Renamed += _on_rename;

            watcher.NotifyFilter =// NotifyFilters.Attributes |
                                    //NotifyFilters.CreationTime |
                                    //NotifyFilters.DirectoryName |
                                    NotifyFilters.FileName |
                                    //NotifyFilters.LastAccess |
                                    NotifyFilters.LastWrite |
                                    //NotifyFilters.Security |
                                    NotifyFilters.Size;

            watcher.EnableRaisingEvents = true;// Begin watching.
        }

        public void _on_rename(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == e.FullPath)
                return;
            _on_file_change(e.OldFullPath, WatcherChangeTypes.Deleted);
            _on_file_change(e.FullPath, WatcherChangeTypes.Created);
        }

        public void _on_changed(object sender, FileSystemEventArgs e)
        {
            _on_file_change(e.FullPath, e.ChangeType);
        }

        public void _on_file_change(string file_path, WatcherChangeTypes type)
        {
            string file_name = Path.GetFileName(file_path);
            if (file_name.StartsWith("~"))
                return;
            //Console.WriteLine($"{file_path} {type}");
            lock (this)
            {
                switch (type)
                {
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                        long t = File.GetLastWriteTimeUtc(file_path).Ticks;
                        if (_file_dict.TryGetValue(file_path, out var old_t))
                        {
                            if (old_t == t)
                                return;
                        }
                        _file_dict[file_path] = t;
                        _changes.Add(file_path, type);
                        break;

                    case WatcherChangeTypes.Deleted:
                        _file_dict.Remove(file_path);
                        _changes.Add(file_path, type);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

