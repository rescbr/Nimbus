using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;
using WebApiContrib.Formatting.Html.ViewParsers;
using WebApiContrib.Formatting.Razor;
using System.Collections.Concurrent;

namespace Nimbus.Web.Middleware
{
    /// <summary>
    /// NimbusViewParser é um clone do WebApiContrib.Formatting.Razor.RazorViewParser
    /// que realiza o caching local dos templates, devido a bug(?) no RazorEngine.Core.Compilation.
    /// </summary>
    public class NimbusViewParser : IViewParser
    {
        public class TemplateCacheNameType
        {
            private string _viewName;
            public string ViewName
            {
                get { return _viewName; }
            }

            private Type _modelType;
            public Type ModelType
            {
                get { return _modelType; }
            }

            public TemplateCacheNameType(string viewName, Type modelType)
            {
                _viewName = viewName;
                _modelType = modelType;
            }
        }

        public class TemplateCacheHashTemplate
        {
            private int _hash;

            public int Hash
            {
                get { return _hash; }
                set { _hash = value; }
            }
            private ITemplate _template;

            public ITemplate Template
            {
                get { return _template; }
                set { _template = value; }
            }
        }


        private readonly ITemplateService _templateService;
        private ConcurrentDictionary<TemplateCacheNameType, TemplateCacheHashTemplate> _templateCache;

		public NimbusViewParser(ITemplateService templateService)
		{
			if (templateService == null)
				throw new ArgumentNullException("templateService");

			_templateService = templateService;
		}
        public NimbusViewParser()
        {
            var config = new TemplateServiceConfiguration { Resolver = new TemplateResolver() };
            _templateService = new TemplateService(config);
        }

        public NimbusViewParser(ITemplateResolver resolver)
		{
			if (resolver == null)
				throw new ArgumentNullException("resolver");

			var config = new TemplateServiceConfiguration { Resolver = resolver };
			_templateService = new TemplateService(config);
		}
        
        
        public byte[] ParseView(IView view, string viewTemplate, System.Text.Encoding encoding)
        {
            var parsedView = GetParsedView(view, viewTemplate);

            return encoding.GetBytes(parsedView);
        }

        private string GetParsedView(IView view, string viewTemplate)
        {

            _templateService.Compile(viewTemplate, view.ModelType, view.ViewName);
            return _templateService.Run(view.ViewName, view.Model, null);
        }

        ///// <summary>
        ///// Compila e faz cache do template (RazorEngine.Core.Templating.TemplateService)
        ///// </summary>
        ///// <param name="razorTemplate">The string template.</param>
        ///// <param name="modelType">The model type.</param>
        ///// <param name="cacheName">The name of the template type in the cache.</param>
        //private void CompileTemplate(string razorTemplate, Type modelType, string cacheName)
        //{            
        //    if (razorTemplate == null) throw new ArgumentNullException("razorTemplate");
        //    if (cacheName == null) throw new ArgumentNullException("cacheName");

        //    int hashCode = razorTemplate.GetHashCode();

        //    ITemplate tpl = _templateService.CreateTemplate(razorTemplate, modelType, null);
            
        //    Type type = CreateTemplateType(razorTemplate, modelType);
        //    var item = new CachedTemplateItem(hashCode, type);

        //    _cache.AddOrUpdate(cacheName, item, (n, i) => item);
        //}
    }
}
