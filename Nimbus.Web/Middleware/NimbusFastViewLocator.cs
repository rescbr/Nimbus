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
        private ConcurrentDictionary<string, string> _viewPaths = 
            new ConcurrentDictionary<string,string>(StringComparer.OrdinalIgnoreCase);

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
                string viewName = GetKeyFromViewFile(cshtmlFile);
                if (_viewPaths.ContainsKey(viewName))
                    throw new Exception(String.Format("{0} is already registered as a View.", viewName));
                _viewPaths[viewName] = cshtmlFile;
            }
        }

        public string GetView(string siteRootPath, WebApiContrib.Formatting.Html.IView view)
        {
            //TODO: aqui podemos adicionar um tratador de erro para o tipo HttpError

            string viewPath;
            string viewKey = GetKeyFromModelType(view.ModelType);
            if (viewKey == null)
            {
                viewKey = view.ViewName;
            }

            try
            {
                viewPath = _viewPaths[viewKey];
            }
            catch {
                throw new Exception(
                    String.Format(
                    "The view for {0} was not found on NimbusFastViewLocator dictionary. Have you added {0} and not restarted Nimbus?",
                    viewKey));
            }
            if (File.Exists(viewPath))
                return File.ReadAllText(viewPath);
            else
                throw new Exception(
                    String.Format(
                    "The view for {0} was on NimbusFastViewLocator dictionary, but the file {1} was not found.",
                    viewKey, viewPath));
        }

        internal static string GetKeyFromModelType(Type modelType)
        {
            if (modelType == null) return null;
            if (!modelType.Namespace.StartsWith("Nimbus.Web")) return modelType.Name;

            string modelSuffix = "Model";
            List<string> sepType = modelType.Namespace.Split(Type.Delimiter).ToList();
            
            string viewNamespace = sepType[2]; //Nimbus.Web.XXXX
            string viewName = modelType.Name.Remove(modelType.Name.Length - modelSuffix.Length);

            return String.Format("{0}.{1}", viewNamespace, viewName);

        }

        internal static string GetKeyFromViewFile(string viewPath)
        {
            List<string> sepPath = viewPath.Split(Path.DirectorySeparatorChar).ToList();
            sepPath.RemoveAll(s => s.Contains("View"));
            string viewName = Path.GetFileNameWithoutExtension(sepPath[sepPath.Count - 1]);
            string viewNamespace = sepPath[sepPath.Count - 2];
            
            if (viewNamespace == "Shared") return viewName;

            return String.Format("{0}.{1}", viewNamespace, viewName);
           
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