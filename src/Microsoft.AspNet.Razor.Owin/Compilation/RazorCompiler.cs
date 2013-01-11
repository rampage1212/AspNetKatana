﻿// -----------------------------------------------------------------------
// <copyright file="RazorCompiler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Razor;
using System.Web.Razor.Generator;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.CSharp;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using CSCompilation = Roslyn.Compilers.CSharp.Compilation;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    public class RazorCompiler : ICompiler
    {
        private static readonly Regex InvalidClassNameChars = new Regex("[^A-Za-z0-9_]");
        private static readonly Dictionary<DiagnosticSeverity, MessageLevel> SeverityMap = new Dictionary<DiagnosticSeverity, MessageLevel>() 
        {
            { DiagnosticSeverity.Error, MessageLevel.Error },
            { DiagnosticSeverity.Info, MessageLevel.Info },
            { DiagnosticSeverity.Warning, MessageLevel.Warning }
        };

        public bool CanCompile(IFile file)
        {
            return String.Equals(file.Extension, ".cshtml");
        }

        public Task<CompilationResult> Compile(IFile file)
        {
            string className = MakeClassName(file.Name);
            RazorTemplateEngine engine = new RazorTemplateEngine(new RazorEngineHost(new CSharpRazorCodeLanguage())
            {
                DefaultBaseClass = "Microsoft.AspNet.Razor.Owin.PageBase",
                GeneratedClassContext = new GeneratedClassContext(
                    executeMethodName: "Execute",
                    writeMethodName: "Write",
                    writeLiteralMethodName: "WriteLiteral",
                    writeToMethodName: "WriteTo",
                    writeLiteralToMethodName: "WriteLiteralTo",
                    templateTypeName: "Template",
                    defineSectionMethodName: "DefineSection")
                    {
                        ResolveUrlMethodName = "Href"
                    }
            });
            engine.Host.NamespaceImports.Add("System");
            engine.Host.NamespaceImports.Add("System.Linq");
            engine.Host.NamespaceImports.Add("System.Collections.Generic");

            GeneratorResults results;
            using (TextReader rdr = file.OpenRead())
            {
                results = engine.GenerateCode(rdr, className, "RazorCompiled", file.FullPath);
            }

            List<CompilationMessage> messages = new List<CompilationMessage>();
            if (!results.Success)
            {
                foreach (var error in results.ParserErrors)
                {
                    messages.Add(new CompilationMessage(
                        MessageLevel.Error,
                        error.Message,
                        new FileLocation(file.FullPath, error.Location.LineIndex, error.Location.CharacterIndex)));
                }
            }

            // Regardless of success or failure, we're going to try and compile
            return Task.FromResult(CompileCSharp("RazorCompiled." + className, file, results.Success, messages, results.GeneratedCode));
        }

        private CompilationResult CompileCSharp(string fullClassName, IFile file, bool success, List<CompilationMessage> messages, CodeCompileUnit codeCompileUnit)
        {
            // Generate code text
            StringBuilder code = new StringBuilder();
            CSharpCodeProvider provider = new CSharpCodeProvider();
            using (StringWriter writer = new StringWriter(code))
            {
                provider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, new CodeGeneratorOptions());
            }

            // Parse
            SyntaxTree tree = SyntaxTree.ParseText/*.ParseCompilationUnit*/(code.ToString(), "__Generated.cs");

            // Create a compilation
            CSCompilation comp = CSCompilation.Create(
                "Compiled",
                new CompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[] 
                {
                    MetadataFileReference.CreateAssemblyReference(typeof(object).Assembly.Location),
                    MetadataFileReference.CreateAssemblyReference(typeof(Enumerable).Assembly.Location),
                    MetadataFileReference.CreateAssemblyReference(typeof(PageBase).Assembly.Location),
                    MetadataFileReference.CreateAssemblyReference(typeof(Gate.Request).Assembly.Location)
                });

            // Emit to a collectable assembly
            AssemblyBuilder asm = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Razor_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mod = asm.DefineDynamicModule("RazorCompilation");
            ReflectionEmitResult result = comp.Emit(mod);

            // Extract the type
            Type typ = null;
            if (result.Success)
            {
                typ = asm.GetType(fullClassName);
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var span = diagnostic.Location.GetLineSpan(true);
                    var linePosition = span.StartLinePosition;
                    messages.Add(new CompilationMessage(
                        SeverityMap[diagnostic.Info.Severity], 
                        diagnostic.Info.GetMessage(), 
                        new FileLocation(
                            span.Path,
                            linePosition.Line,
                            linePosition.Character,
                            String.Equals(span.Path, "__Generated.cs", StringComparison.OrdinalIgnoreCase))));
                }
            }

            // Create a compilation result
            if (success && result.Success)
            {
                return CompilationResult.Successful(code.ToString(), typ, messages);
            }
            return CompilationResult.Failed(code.ToString(), messages);
        }

        private string MakeClassName(string fileName)
        {
            return "_" + InvalidClassNameChars.Replace(fileName, String.Empty);
        }
    }
}
