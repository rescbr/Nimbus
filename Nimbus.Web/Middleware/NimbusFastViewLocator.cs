using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using WebApiContrib.Formatting.Html.Locators;
using System.IO;
using System.Reflection;

namespace Nimbus.Web.Middleware
{
    public class NimbusFastViewLocator : IViewLocator
    {
        private ConcurrentDictionary<string, string> _viewPaths = new ConcurrentDictionary<string,string>();

        /// <summary>
        /// Procura por arquivos cshtml e grava seus caminhos em um dicionário
        /// para aumentar performance.
        /// </summary>
        public NimbusFastViewLocator()
        {
            string sitePath = GetPhysicalSiteRootPath();
            string[] cshtmlFiles = Directory.GetFiles(sitePath, "*.cshtml", SearchOption.AllDirectories);
            foreach (string cshtmlFile in cshtmlFiles)
            {
                string viewName = Path.GetFileNameWithoutExtension(cshtmlFile);
                if (_viewPaths.ContainsKey(viewName.ToLowerInvariant()))
                    throw new Exception(String.Format("{0} is already registered as a View.", viewName));
                _viewPaths[viewName.ToLowerInvariant()] = cshtmlFile;
            }
        }

        public string GetView(string siteRootPath, WebApiContrib.Formatting.Html.IView view)
        {
            string viewPath;
            try
            {
                viewPath = _viewPaths[view.ViewName.ToLowerInvariant()];
            }
            catch {
                throw new Exception(
                    String.Format(
                    "{0} was not on NimbusFastViewLocator dictionary. Have you added {0} and not restarted Nimbus?",
                    view.ViewName));
            }
            if (File.Exists(viewPath))
                return File.ReadAllText(viewPath);
            else
                throw new Exception(
                    String.Format(
                    "{0} was on NimbusFastViewLocator dictionary, but the file {1} was not found.",
                    view.ViewName, viewPath));
        }

        internal static string GetPhysicalSiteRootPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
                       .Replace("file:\\", string.Empty)
                       .Replace("\\bin", string.Empty)
                       .Replace("\\Debug", string.Empty)
                       .Replace("\\Release", string.Empty);
        }
    }
}