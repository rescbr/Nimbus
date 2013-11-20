using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Nimbus.Web.Utils
{
    public class RazorTemplate
    {
        ITemplateService _templateService;
        public RazorTemplate() {
            var config = new TemplateServiceConfiguration { Resolver = new TemplateResolver() };
            _templateService = new TemplateService(config);
        }

        public string ParseRazorTemplate<T>(string templatePath, T model)
        {
            string cacheName = templatePath;//typeof(T).ToString();

            ITemplate tpl = _templateService.Resolve(cacheName, model);
            //_templateService.Compile(templatePath, typeof(T), cacheName);
            return _templateService.Run(tpl, null);
        }
    }

    internal class TemplateResolver : ITemplateResolver
    {
        public string Resolve(string name)
        {
            //Replace the "~/" to the root path of the web.
            name = name.Replace("~", GetPhysicalSiteRootPath()).Replace("/", "\\");

            if (!File.Exists(name))
                throw new FileNotFoundException(name);

            return File.ReadAllText(name);
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