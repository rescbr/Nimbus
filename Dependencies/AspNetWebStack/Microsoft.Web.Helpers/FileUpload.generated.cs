﻿using Resources;

#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    
    #line 3 "FileUpload.cshtml"
    using System.Globalization;
    
    #line default
    #line hidden
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    
    #line 4 "FileUpload.cshtml"
    using System.Web;
    
    #line default
    #line hidden
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.WebPages.Html;
    
    #line 5 "FileUpload.cshtml"
    using Microsoft.Internal.Web.Utils;
    
    #line default
    #line hidden
    
    public class FileUpload : System.Web.WebPages.HelperPage
    {
        
        #line 7 "FileUpload.cshtml"

    private class FileUploadTracker {
        private static readonly object _countKey = new object();
        private static readonly object _scriptAlreadyRendered = new object();
        private readonly HttpContextBase _httpContext;

        public FileUploadTracker(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        public bool ScriptAlreadyRendered {
            get {
                bool? rendered = _httpContext.Items[_scriptAlreadyRendered] as bool?;
                return rendered.HasValue && rendered.Value;
            }
            set {
                _httpContext.Items[_scriptAlreadyRendered] = value;
            }
        }

        public int RenderCount {
            get {
                int? count = _httpContext.Items[_countKey] as int?;
                if (!count.HasValue) {
                    count = 0;
                }
                return count.Value;
            }
            set {
                _httpContext.Items[_countKey] = value;
            }
        }
    }

        #line default
        #line hidden

public static System.Web.WebPages.HelperResult GetHtml(string name = null,
            int initialNumberOfFiles = 1,
            bool allowMoreFilesToBeAdded = true,
            bool includeFormTag = true,
            string addText = null,
            string uploadText = null) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 47 "FileUpload.cshtml"
                                       
    
#line default
#line hidden


#line 48 "FileUpload.cshtml"
WriteTo(@__razor_helper_writer, _GetHtml(new HttpContextWrapper(HttpContext.Current), name, initialNumberOfFiles, allowMoreFilesToBeAdded,
                includeFormTag, addText, uploadText));

#line default
#line hidden


#line 49 "FileUpload.cshtml"
                                                    

#line default
#line hidden

});

}


internal static System.Web.WebPages.HelperResult _GetHtml(HttpContextBase context, string name, int initialNumberOfFiles, 
        bool allowMoreFilesToBeAdded, bool includeFormTag, string addText, string uploadText) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 53 "FileUpload.cshtml"
                                                                                               
    
    if (initialNumberOfFiles < 0) {
        throw new ArgumentOutOfRangeException(
            "initialNumberOfFiles",
            String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, "0"));
    }
    var tracker = new FileUploadTracker(context);
    int count = tracker.RenderCount++;

    name = name ?? "fileUpload";
    uploadText = uploadText ?? HelpersToolkitResources.FileUpload_Upload;
    addText = addText ?? HelpersToolkitResources.FileUpload_AddMore;


    if (allowMoreFilesToBeAdded && !tracker.ScriptAlreadyRendered) {
        tracker.ScriptAlreadyRendered = true;


#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, @"        <script type=""text/javascript""> 
            if (!window[""FileUploadHelper""]) window[""FileUploadHelper""] = {};  
            FileUploadHelper.addInputElement = function(index, name) {  
                var inputElem = document.createElement(""input"");  
                inputElem.type = ""file"";  
                inputElem.name = name;  
                var divElem = document.createElement(""div"");  
                divElem.appendChild(inputElem.cloneNode(false));   
                var inputs = document.getElementById(""file-upload-"" + index);  
                inputs.appendChild(divElem);  
            } 
        </script>
");



#line 83 "FileUpload.cshtml"
    }

    if (includeFormTag) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "        ");

WriteLiteralTo(@__razor_helper_writer, "<form action=\"\" enctype=\"multipart/form-data\" method=\"post\">\r\n");



#line 87 "FileUpload.cshtml"
    }

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <div class=\"file-upload\" id=\"file-upload-");



#line 88 "FileUpload.cshtml"
               WriteTo(@__razor_helper_writer, count);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\">\r\n");



#line 89 "FileUpload.cshtml"
         for(int i = 0; i < initialNumberOfFiles; i++) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "            <div>\r\n                <input name=\"");



#line 91 "FileUpload.cshtml"
WriteTo(@__razor_helper_writer, name);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" type=\"file\" />\r\n            </div>\r\n");



#line 93 "FileUpload.cshtml"
        }

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    </div>\r\n");



#line 95 "FileUpload.cshtml"

    if (allowMoreFilesToBeAdded || includeFormTag) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "        <div class=\"file-upload-buttons\">\r\n");



#line 98 "FileUpload.cshtml"
         if (allowMoreFilesToBeAdded) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "            <a href=\"#\" onclick=\"FileUploadHelper.addInputElement(");



#line 99 "FileUpload.cshtml"
                                   WriteTo(@__razor_helper_writer, count);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, ", ");



#line 99 "FileUpload.cshtml"
                                           WriteTo(@__razor_helper_writer, HttpUtility.JavaScriptStringEncode(name, addDoubleQuotes: true));

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "); return false;\">");



#line 99 "FileUpload.cshtml"
                                                                                                                             WriteTo(@__razor_helper_writer, addText);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "</a>\r\n");



#line 100 "FileUpload.cshtml"
        }

#line default
#line hidden



#line 101 "FileUpload.cshtml"
         if (includeFormTag) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "            <input type=\"submit\" value=\"");



#line 102 "FileUpload.cshtml"
         WriteTo(@__razor_helper_writer, uploadText);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" />\r\n");



#line 103 "FileUpload.cshtml"
        }

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "        </div>\r\n");



#line 105 "FileUpload.cshtml"
    }
    
    if (includeFormTag) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "        ");

WriteLiteralTo(@__razor_helper_writer, "</form>\r\n");



#line 109 "FileUpload.cshtml"
    }

#line default
#line hidden

});

}


        public FileUpload()
        {
        }
    }
}
#pragma warning restore 1591
