/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 16:19:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace ExportExcel
{
    public static class FileUtil
    {
        public static void CreateFileDir(string file_path)
        {
            string full_path = Path.GetFullPath(file_path);
            _create_dir(Path.GetDirectoryName(full_path));
        }

        public static void CreateDir(string folder_path)
        {
            _create_dir(Path.GetFullPath(folder_path));
        }

        private static void _create_dir(string folder_path)
        {
            if (Directory.Exists(folder_path))
                return;

            string parent_folder_path = Path.GetDirectoryName(folder_path);
            _create_dir(parent_folder_path);
            Directory.CreateDirectory(folder_path);
        }

        private const string C_Nest_Res_Dir = "ExportExcel";
        public static byte[] ReadNestedFile(string file_name)
        {
            string file_path = Path.Combine(C_Nest_Res_Dir, file_name);
            string res_path = file_path.Replace("\\", ".").Replace("/", ".");

            Assembly assembly = Assembly.GetEntryAssembly();
            //var tt = assembly.GetManifestResourceNames();
            var stream = assembly.GetManifestResourceStream(res_path);

            if (stream == null)
                return null;
            long len = stream.Length;
            byte[] data = new byte[len];
            stream.Read(data, 0, data.Length);
            stream.Close();
            return data;
        }

        public static string ReadNestedText(string file_name)
        {
            byte[] buff = ReadNestedFile(file_name);
            if (buff == null)
                return null;

            string ret = System.Text.Encoding.UTF8.GetString(buff);
            return ret;
        }
    }
}
