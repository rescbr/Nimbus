using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class DirectoryAssemblyLoader
    {
        private string _directory;

        public DirectoryAssemblyLoader(string path, bool isFile)
        {
            if (isFile)
                _directory = Path.GetDirectoryName(path);
            else
                _directory = path;
        }

        public Assembly LoadDelegate(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.Combine(_directory, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath) == false) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}
