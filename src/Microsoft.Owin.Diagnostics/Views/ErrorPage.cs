﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.33440
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Owin.Diagnostics.Views {
    
    #line 1 "ErrorPage.cshtml"
    using System;
    
    #line default
    #line hidden
    
    #line 2 "ErrorPage.cshtml"
    using System.Globalization;
    
    #line default
    #line hidden
    
    #line 3 "ErrorPage.cshtml"
    using System.Linq;
    
    #line default
    #line hidden
    
    #line 4 "ErrorPage.cshtml"
    using Views;
    
    #line default
    #line hidden
    
    
    public class ErrorPage : Microsoft.Owin.Diagnostics.Views.BaseView {
        
#line hidden
        
        #line 6 "ErrorPage.cshtml"
 
    /// <summary>
    /// 
    /// </summary>
    public Views.ErrorPageModel Model { get; set; }

        #line default
        #line hidden
        
        
        public ErrorPage() {
        }
        
        public override void Execute() {
            
            #line 12 "ErrorPage.cshtml"
  
    Response.StatusCode = 500;
    Response.ReasonPhrase = "Internal Server Error";
    Response.ContentType = "text/html";
    string location = string.Empty;

            
            #line default
            #line hidden
WriteLiteral("\r\n<!DOCTYPE html>\r\n\r\n<html");

WriteLiteral(" lang=\"en\"");

WriteLiteral(" xmlns=\"http://www.w3.org/1999/xhtml\"");

WriteLiteral(">\r\n    <head>\r\n        <meta");

WriteLiteral(" charset=\"utf-8\"");

WriteLiteral(" />\r\n        <title>");

            
            #line 23 "ErrorPage.cshtml"
          Write(Resources.ErrorPageHtml_Title);

            
            #line default
            #line hidden
WriteLiteral("</title>\r\n        <style>\r\n");

WriteLiteral("            ");

            
            #line 25 "ErrorPage.cshtml"
        WriteLiteral(@"body {
    font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;
    font-size: .813em;
    line-height: 1.4em;
    color: #222;
}

h1, h2, h3, h4, h5 {
    /*font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;*/
    font-weight: 100;
}

h1 {
    color: #44525e;
    margin: 15px 0 15px 0;
}

h2 {
    margin: 10px 5px 0 0;
}

h3 {
    color: #363636;
    margin: 5px 5px 0 0;
}

code {
    font-family: consolas, ""Courier New"", courier, monospace;
}

body .titleerror {
    padding: 3px;
}

body .location {
    margin: 3px 0 10px 30px;
}

#header {
    font-size: 18px;
    padding-left: 0px;
    padding-right: 0px;
    padding-top: 15px;
    padding-bottom: 15px;
    border-top: 1px #ddd solid;
    border-bottom: 1px #ddd solid;
    margin-bottom: 0px;
}

#header li {
    display: inline;
    margin: 5px;
    padding: 5px;
    color: #a0a0a0;
}

#header li:hover {
    background: #A9E4F9;
    color: #fff;
}

#header li.selected {
    background: #44C5F2;
    color: #fff;
}

#stackpage ul {
    list-style: none;
    padding-left: 0;
    margin: 0;
    /*border-bottom: 1px #ddd solid;*/
}

#stackpage .stackerror {
    padding: 5px;
    border-bottom: 1px #ddd solid;
}

#stackpage .stackerror:hover {
    background-color: #f0f0f0;
}

#stackpage .frame:hover {
    background-color: #f0f0f0;
    text-decoration: none;
}

#stackpage .frame {
    padding: 2px;
    margin: 0 0 0 30px;
    border-bottom: 1px #ddd solid;
}

#stackpage .frame h3 {
    padding: 5px;
    margin: 0;
}

#stackpage .source {
    padding: 0px;
}

#stackpage .source ol li {
    font-family: consolas, ""Courier New"", courier, monospace;
    white-space: pre;
}

#stackpage .source ol.highlight li {
    /*color: #e22;*/
    /*font-weight: bold;*/
}

#stackpage .source ol.highlight li span {
    /*color: #000;*/
}

#stackpage .frame:hover .source ol.highlight li span {
    color: #fff;
    background: #B20000;
}

#stackpage .source ol.collapsable li {
    color: #888;
}

#stackpage .source ol.collapsable li span {
    color: #606060;
}

.page table {
    border-collapse: separate;
    border-spacing: 0;
    margin: 0 0 20px;
}

.page th {
    vertical-align: bottom;
    padding: 10px 5px 5px 5px;
    font-weight: 400;
    color: #a0a0a0;
    text-align: left;
}

.page td {
    padding: 3px 10px;
}

.page th, .page td {
    border-right: 1px #ddd solid;
    border-bottom: 1px #ddd solid;
    border-left: 1px transparent solid;
    border-top: 1px transparent solid;
    box-sizing: border-box;
}

.page th:last-child, .page td:last-child {
    border-right: 1px transparent solid;
}

.page td.length {
    text-align: right;
}

a {
    color: #1ba1e2;
    text-decoration: none;
}

a:hover {
    color: #13709e;
    text-decoration: underline;
}
");

            
            #line default
            #line hidden
WriteLiteral(" \r\n        </style>\r\n    </head>\r\n    <body>\r\n        <h1>");

            
            #line 29 "ErrorPage.cshtml"
       Write(Resources.ErrorPageHtml_UnhandledException);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");

            
            #line 30 "ErrorPage.cshtml"
        
            
            #line default
            #line hidden
            
            #line 30 "ErrorPage.cshtml"
         if (Model.Options.ShowExceptionDetails)
        {
            foreach (var errorDetail in Model.ErrorDetails)
            {

            
            #line default
            #line hidden
WriteLiteral("                <h2");

WriteLiteral(" class=\"titleerror\"");

WriteLiteral(">");

            
            #line 34 "ErrorPage.cshtml"
                                  Write(errorDetail.Error.GetType().Name);

            
            #line default
            #line hidden
WriteLiteral(": ");

            
            #line 34 "ErrorPage.cshtml"
                                                                     Write(errorDetail.Error.Message);

            
            #line default
            #line hidden
WriteLiteral("</h2>\r\n");

            
            #line 35 "ErrorPage.cshtml"
                
            
            #line default
            #line hidden
            
            #line 35 "ErrorPage.cshtml"
                  
                    StackFrame firstFrame = null;
                    firstFrame = errorDetail.StackFrames.FirstOrDefault();
                    if (firstFrame != null)
                    {
                        location = firstFrame.Function;
                    }
                    else if (errorDetail.Error.TargetSite != null && errorDetail.Error.TargetSite.DeclaringType != null)
                    {
                        location = errorDetail.Error.TargetSite.DeclaringType.FullName + "." + errorDetail.Error.TargetSite.Name;
                    }
                 
            
            #line default
            #line hidden
            
            #line 46 "ErrorPage.cshtml"
                  
                if (!string.IsNullOrEmpty(location) && firstFrame != null && !string.IsNullOrEmpty(firstFrame.File))
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p");

WriteLiteral(" class=\"location\"");

WriteLiteral(">");

            
            #line 49 "ErrorPage.cshtml"
                                   Write(location);

            
            #line default
            #line hidden
WriteLiteral(" in <code");

WriteAttribute("title", Tuple.Create(" title=\"", 1757), Tuple.Create("\"", 1781)
            
            #line 49 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 1765), Tuple.Create<System.Object, System.Int32>(firstFrame.File
            
            #line default
            #line hidden
, 1765), false)
);

WriteLiteral(">");

            
            #line 49 "ErrorPage.cshtml"
                                                                               Write(System.IO.Path.GetFileName(firstFrame.File));

            
            #line default
            #line hidden
WriteLiteral("</code>, line ");

            
            #line 49 "ErrorPage.cshtml"
                                                                                                                                         Write(firstFrame.Line);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 50 "ErrorPage.cshtml"
                }
                else if (!string.IsNullOrEmpty(location))
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p");

WriteLiteral(" class=\"location\"");

WriteLiteral(">");

            
            #line 53 "ErrorPage.cshtml"
                                   Write(location);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 54 "ErrorPage.cshtml"
                }
                else
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p");

WriteLiteral(" class=\"location\"");

WriteLiteral(">");

            
            #line 57 "ErrorPage.cshtml"
                                   Write(Resources.ErrorPageHtml_UnknownLocation);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 58 "ErrorPage.cshtml"
                }
            }
        }
        else
        {

            
            #line default
            #line hidden
WriteLiteral("            <h2>");

            
            #line 63 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_EnableShowExceptions);

            
            #line default
            #line hidden
WriteLiteral("</h2>\r\n");

            
            #line 64 "ErrorPage.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("        <ul");

WriteLiteral(" id=\"header\"");

WriteLiteral(">\r\n            <li");

WriteLiteral(" id=\"stack\"");

WriteLiteral(" tabindex=\"1\"");

WriteLiteral(" class=\"selected\"");

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 67 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_StackButton);

            
            #line default
            #line hidden
WriteLiteral("\r\n            </li>\r\n            <li");

WriteLiteral(" id=\"query\"");

WriteLiteral(" tabindex=\"2\"");

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 70 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_QueryButton);

            
            #line default
            #line hidden
WriteLiteral("\r\n            </li>\r\n            <li");

WriteLiteral(" id=\"cookies\"");

WriteLiteral(" tabindex=\"3\"");

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 73 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_CookiesButton);

            
            #line default
            #line hidden
WriteLiteral("\r\n            </li>\r\n            <li");

WriteLiteral(" id=\"headers\"");

WriteLiteral(" tabindex=\"4\"");

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 76 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_HeadersButton);

            
            #line default
            #line hidden
WriteLiteral("\r\n            </li>\r\n            <li");

WriteLiteral(" id=\"environment\"");

WriteLiteral(" tabindex=\"5\"");

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 79 "ErrorPage.cshtml"
           Write(Resources.ErrorPageHtml_EnvironmentButton);

            
            #line default
            #line hidden
WriteLiteral("\r\n            </li>\r\n        </ul>\r\n        <div");

WriteLiteral(" id=\"stackpage\"");

WriteLiteral(" class=\"page\"");

WriteLiteral(">\r\n");

            
            #line 83 "ErrorPage.cshtml"
            
            
            #line default
            #line hidden
            
            #line 83 "ErrorPage.cshtml"
             if (Model.Options.ShowExceptionDetails)
            {

            
            #line default
            #line hidden
WriteLiteral("                <ul>\r\n");

            
            #line 86 "ErrorPage.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 86 "ErrorPage.cshtml"
                       int tabIndex = 6; 
            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 87 "ErrorPage.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 87 "ErrorPage.cshtml"
                     foreach (var errorDetail in Model.ErrorDetails)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <li>\r\n                            <h2");

WriteLiteral(" class=\"stackerror\"");

WriteLiteral(">");

            
            #line 90 "ErrorPage.cshtml"
                                              Write(errorDetail.Error.GetType().Name);

            
            #line default
            #line hidden
WriteLiteral(": ");

            
            #line 90 "ErrorPage.cshtml"
                                                                                 Write(errorDetail.Error.Message);

            
            #line default
            #line hidden
WriteLiteral("</h2>\r\n                            <ul>\r\n");

            
            #line 92 "ErrorPage.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 92 "ErrorPage.cshtml"
                             foreach (var frame in errorDetail.StackFrames)
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <li");

WriteLiteral(" class=\"frame\"");

WriteAttribute("tabindex", Tuple.Create(" tabindex=\"", 3574), Tuple.Create("\"", 3594)
            
            #line 94 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 3585), Tuple.Create<System.Object, System.Int32>(tabIndex
            
            #line default
            #line hidden
, 3585), false)
);

WriteLiteral(">\r\n");

            
            #line 95 "ErrorPage.cshtml"
                                    
            
            #line default
            #line hidden
            
            #line 95 "ErrorPage.cshtml"
                                       tabIndex++; 
            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 96 "ErrorPage.cshtml"
                                    
            
            #line default
            #line hidden
            
            #line 96 "ErrorPage.cshtml"
                                     if (string.IsNullOrEmpty(frame.File))
                                    {

            
            #line default
            #line hidden
WriteLiteral("                                        <h3>");

            
            #line 98 "ErrorPage.cshtml"
                                       Write(frame.Function);

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 99 "ErrorPage.cshtml"
                                    }
                                    else
                                    {

            
            #line default
            #line hidden
WriteLiteral("                                        <h3>");

            
            #line 102 "ErrorPage.cshtml"
                                       Write(frame.Function);

            
            #line default
            #line hidden
WriteLiteral(" in <code");

WriteAttribute("title", Tuple.Create(" title=\"", 4021), Tuple.Create("\"", 4040)
            
            #line 102 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 4029), Tuple.Create<System.Object, System.Int32>(frame.File
            
            #line default
            #line hidden
, 4029), false)
);

WriteLiteral(">");

            
            #line 102 "ErrorPage.cshtml"
                                                                                    Write(System.IO.Path.GetFileName(frame.File));

            
            #line default
            #line hidden
WriteLiteral("</code></h3>\r\n");

            
            #line 103 "ErrorPage.cshtml"
                                    }

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 105 "ErrorPage.cshtml"
                                    
            
            #line default
            #line hidden
            
            #line 105 "ErrorPage.cshtml"
                                     if (frame.Line != 0 && frame.ContextCode != null)
                                    {

            
            #line default
            #line hidden
WriteLiteral("                                        <div");

WriteLiteral(" class=\"source\"");

WriteLiteral(">\r\n");

            
            #line 108 "ErrorPage.cshtml"
                                            
            
            #line default
            #line hidden
            
            #line 108 "ErrorPage.cshtml"
                                             if (frame.PreContextCode != null)
                                            {

            
            #line default
            #line hidden
WriteLiteral("                                                <ol");

WriteAttribute("start", Tuple.Create(" start=\"", 4503), Tuple.Create("\"", 4532)
            
            #line 110 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 4511), Tuple.Create<System.Object, System.Int32>(frame.PreContextLine
            
            #line default
            #line hidden
, 4511), false)
);

WriteLiteral(" class=\"collapsable\"");

WriteLiteral(">\r\n");

            
            #line 111 "ErrorPage.cshtml"
                                                    
            
            #line default
            #line hidden
            
            #line 111 "ErrorPage.cshtml"
                                                     foreach (var line in frame.PreContextCode)
                                                    {

            
            #line default
            #line hidden
WriteLiteral("                                                        <li><span>");

            
            #line 113 "ErrorPage.cshtml"
                                                             Write(line);

            
            #line default
            #line hidden
WriteLiteral("</span></li>\r\n");

            
            #line 114 "ErrorPage.cshtml"
                                                    }

            
            #line default
            #line hidden
WriteLiteral("                                                </ol>\r\n");

            
            #line 116 "ErrorPage.cshtml"
                                            } 

            
            #line default
            #line hidden
WriteLiteral("\r\n                                            <ol");

WriteAttribute("start", Tuple.Create(" start=\"", 5000), Tuple.Create("\"", 5019)
            
            #line 118 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 5008), Tuple.Create<System.Object, System.Int32>(frame.Line
            
            #line default
            #line hidden
, 5008), false)
);

WriteLiteral(" class=\"highlight\"");

WriteLiteral(">\r\n                                                <li><span>");

            
            #line 119 "ErrorPage.cshtml"
                                                     Write(frame.ContextCode);

            
            #line default
            #line hidden
WriteLiteral("</span></li></ol>\r\n\r\n");

            
            #line 121 "ErrorPage.cshtml"
                                            
            
            #line default
            #line hidden
            
            #line 121 "ErrorPage.cshtml"
                                             if (frame.PostContextCode != null)
                                            {

            
            #line default
            #line hidden
WriteLiteral("                                                <ol");

WriteAttribute("start", Tuple.Create(" start=\'", 5317), Tuple.Create("\'", 5342)
            
            #line 123 "ErrorPage.cshtml"
, Tuple.Create(Tuple.Create("", 5325), Tuple.Create<System.Object, System.Int32>(frame.Line + 1
            
            #line default
            #line hidden
, 5325), false)
);

WriteLiteral(" class=\"collapsable\"");

WriteLiteral(">\r\n");

            
            #line 124 "ErrorPage.cshtml"
                                                    
            
            #line default
            #line hidden
            
            #line 124 "ErrorPage.cshtml"
                                                     foreach (var line in frame.PostContextCode)
                                                    {

            
            #line default
            #line hidden
WriteLiteral("                                                        <li><span>");

            
            #line 126 "ErrorPage.cshtml"
                                                             Write(line);

            
            #line default
            #line hidden
WriteLiteral("</span></li>\r\n");

            
            #line 127 "ErrorPage.cshtml"
                                                    }

            
            #line default
            #line hidden
WriteLiteral("                                                </ol>\r\n");

            
            #line 129 "ErrorPage.cshtml"
                                            } 

            
            #line default
            #line hidden
WriteLiteral("                                        </div>\r\n");

            
            #line 131 "ErrorPage.cshtml"
                                    } 

            
            #line default
            #line hidden
WriteLiteral("                                </li>\r\n");

            
            #line 133 "ErrorPage.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                            </ul>\r\n                        </li>\r\n");

            
            #line 136 "ErrorPage.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                </ul>\r\n");

            
            #line 138 "ErrorPage.cshtml"
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <h3>");

            
            #line 141 "ErrorPage.cshtml"
               Write(string.Format(CultureInfo.CurrentCulture, Resources.ErrorPageHtml_ViewDisabled, "ErrorPageOptions.ShowExceptionDetails"));

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 142 "ErrorPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n\r\n        <div");

WriteLiteral(" id=\"querypage\"");

WriteLiteral(" class=\"page\"");

WriteLiteral(">\r\n");

            
            #line 146 "ErrorPage.cshtml"
            
            
            #line default
            #line hidden
            
            #line 146 "ErrorPage.cshtml"
             if (Model.Options.ShowQuery)
            {
                if (Model.Query.Any())
                {

            
            #line default
            #line hidden
WriteLiteral("                    <table>\r\n                        <thead>\r\n                   " +
"         <tr>\r\n                                <th>");

            
            #line 153 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_VariableColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                                <th>");

            
            #line 154 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_ValueColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                            </tr>\r\n                        </thead>\r\n     " +
"                   <tbody>\r\n");

            
            #line 158 "ErrorPage.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 158 "ErrorPage.cshtml"
                             foreach (var kv in Model.Query.OrderBy(kv => kv.Key))
                            {
                                foreach (var v in kv.Value)
                                {

            
            #line default
            #line hidden
WriteLiteral("                                    <tr>\r\n                                       " +
" <td>");

            
            #line 163 "ErrorPage.cshtml"
                                       Write(kv.Key);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                        <td>");

            
            #line 164 "ErrorPage.cshtml"
                                       Write(v);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                    </tr>\r\n");

            
            #line 166 "ErrorPage.cshtml"
                                }
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </tbody>\r\n                    </table>\r\n");

            
            #line 170 "ErrorPage.cshtml"
                }
                else
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p>");

            
            #line 173 "ErrorPage.cshtml"
                  Write(Resources.ErrorPageHtml_NoQueryStringData);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 174 "ErrorPage.cshtml"
                }
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <h3>");

            
            #line 178 "ErrorPage.cshtml"
               Write(string.Format(CultureInfo.CurrentCulture, Resources.ErrorPageHtml_ViewDisabled, "ErrorPageOptions.ShowQuery"));

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 179 "ErrorPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n        <div");

WriteLiteral(" id=\"cookiespage\"");

WriteLiteral(" class=\"page\"");

WriteLiteral(">\r\n");

            
            #line 182 "ErrorPage.cshtml"
            
            
            #line default
            #line hidden
            
            #line 182 "ErrorPage.cshtml"
             if (Model.Options.ShowCookies)
            {
                if (Model.Cookies.Any())
                {

            
            #line default
            #line hidden
WriteLiteral("                    <table>\r\n                        <thead>\r\n                   " +
"         <tr>\r\n                                <th>");

            
            #line 189 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_VariableColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                                <th>");

            
            #line 190 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_ValueColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                            </tr>\r\n                        </thead>\r\n     " +
"                   <tbody>\r\n");

            
            #line 194 "ErrorPage.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 194 "ErrorPage.cshtml"
                             foreach (var kv in Model.Cookies.OrderBy(kv => kv.Key))
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <tr>\r\n                                    <td>");

            
            #line 197 "ErrorPage.cshtml"
                                   Write(kv.Key);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                    <td>");

            
            #line 198 "ErrorPage.cshtml"
                                   Write(kv.Value);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                </tr>\r\n");

            
            #line 200 "ErrorPage.cshtml"
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </tbody>\r\n                    </table>\r\n");

            
            #line 203 "ErrorPage.cshtml"
                }
                else
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p>");

            
            #line 206 "ErrorPage.cshtml"
                  Write(Resources.ErrorPageHtml_NoCookieData);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 207 "ErrorPage.cshtml"
                }
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <h3>");

            
            #line 211 "ErrorPage.cshtml"
               Write(string.Format(CultureInfo.CurrentCulture, Resources.ErrorPageHtml_ViewDisabled, "ErrorPageOptions.ShowCookies"));

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 212 "ErrorPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n        <div");

WriteLiteral(" id=\"headerspage\"");

WriteLiteral(" class=\"page\"");

WriteLiteral(">\r\n");

            
            #line 215 "ErrorPage.cshtml"
            
            
            #line default
            #line hidden
            
            #line 215 "ErrorPage.cshtml"
             if (Model.Options.ShowHeaders)
            {
                if (Model.Headers.Any())
                {

            
            #line default
            #line hidden
WriteLiteral("                    <table>\r\n                        <thead>\r\n                   " +
"         <tr>\r\n                                <th>");

            
            #line 222 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_VariableColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                                <th>");

            
            #line 223 "ErrorPage.cshtml"
                               Write(Resources.ErrorPageHtml_ValueColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                            </tr>\r\n                        </thead>\r\n     " +
"                   <tbody>\r\n");

            
            #line 227 "ErrorPage.cshtml"
                            
            
            #line default
            #line hidden
            
            #line 227 "ErrorPage.cshtml"
                             foreach (var kv in Model.Headers.OrderBy(kv => kv.Key))
                            {
                                foreach (var v in kv.Value)
                                {

            
            #line default
            #line hidden
WriteLiteral("                                    <tr>\r\n                                       " +
" <td>");

            
            #line 232 "ErrorPage.cshtml"
                                       Write(kv.Key);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                        <td>");

            
            #line 233 "ErrorPage.cshtml"
                                       Write(v);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                    </tr>\r\n");

            
            #line 235 "ErrorPage.cshtml"
                                }
                            }

            
            #line default
            #line hidden
WriteLiteral("                        </tbody>\r\n                    </table>\r\n");

            
            #line 239 "ErrorPage.cshtml"
                }
                else
                {

            
            #line default
            #line hidden
WriteLiteral("                    <p>");

            
            #line 242 "ErrorPage.cshtml"
                  Write(Resources.ErrorPageHtml_NoHeaderData);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");

            
            #line 243 "ErrorPage.cshtml"
                }
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <h3>");

            
            #line 247 "ErrorPage.cshtml"
               Write(string.Format(CultureInfo.CurrentCulture, Resources.ErrorPageHtml_ViewDisabled, "ErrorPageOptions.ShowHeaders"));

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 248 "ErrorPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n        <div");

WriteLiteral(" id=\"environmentpage\"");

WriteLiteral(" class=\"page\"");

WriteLiteral(">\r\n");

            
            #line 251 "ErrorPage.cshtml"
            
            
            #line default
            #line hidden
            
            #line 251 "ErrorPage.cshtml"
             if (Model.Options.ShowEnvironment)
            {

            
            #line default
            #line hidden
WriteLiteral("                <table>\r\n                    <thead>\r\n                        <tr" +
">\r\n                            <th>");

            
            #line 256 "ErrorPage.cshtml"
                           Write(Resources.ErrorPageHtml_VariableColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                            <th>");

            
            #line 257 "ErrorPage.cshtml"
                           Write(Resources.ErrorPageHtml_ValueColumn);

            
            #line default
            #line hidden
WriteLiteral("</th>\r\n                        </tr>\r\n                    </thead>\r\n             " +
"       <tbody>\r\n");

            
            #line 261 "ErrorPage.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 261 "ErrorPage.cshtml"
                         foreach (var kv in Model.Environment.OrderBy(kv => kv.Key))
                        {

            
            #line default
            #line hidden
WriteLiteral("                            <tr>\r\n                                <td>");

            
            #line 264 "ErrorPage.cshtml"
                               Write(kv.Key);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                                <td>");

            
            #line 265 "ErrorPage.cshtml"
                               Write(kv.Value);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                            </tr>\r\n");

            
            #line 267 "ErrorPage.cshtml"
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </tbody>\r\n                </table>\r\n");

            
            #line 270 "ErrorPage.cshtml"
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <h3>");

            
            #line 273 "ErrorPage.cshtml"
               Write(string.Format(CultureInfo.CurrentCulture, Resources.ErrorPageHtml_ViewDisabled, "ErrorPageOptions.ShowEnvironment"));

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");

            
            #line 274 "ErrorPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </div>\r\n        <script");

WriteLiteral(" src=\"http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.js\"");

WriteLiteral("></script>\r\n        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n            //<!--\r\n");

WriteLiteral("            ");

            
            #line 279 "ErrorPage.cshtml"
        WriteLiteral(@"
(function ($) {
    $('.collapsable').hide();
    $('.page').hide();
    $('#stackpage').show();

    $('.frame').click(function () {
        $(this).children('.source').children('.collapsable').toggle('fast');
    });

    $('.frame').keypress(function (e) {
        if (e.which == 13) {
            $(this).children('.source').children('.collapsable').toggle('fast');
        }
    });
    
    $('#header li').click(function () {

        var unselected = $('#header .selected').removeClass('selected').attr('id');
        var selected = $(this).addClass('selected').attr('id');
        
        $('#' + unselected + 'page').hide();
        $('#' + selected + 'page').show('fast');
    });

    $('#header li').keypress(function (e) {
        if (e.which == 13) {
            var unselected = $('#header .selected').removeClass('selected').attr('id');
            var selected = $(this).addClass('selected').attr('id');

            $('#' + unselected + 'page').hide();
            $('#' + selected + 'page').show('fast');
        }
    });
    
})(jQuery);
");

            
            #line default
            #line hidden
WriteLiteral("\r\n        //-->\r\n        </script>\r\n    </body>\r\n</html>\r\n");

        }
    }
}

