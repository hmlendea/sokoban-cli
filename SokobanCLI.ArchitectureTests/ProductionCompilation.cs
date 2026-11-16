using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Compiles the production sources in memory so rules can resolve symbols and overloads.
    /// </summary>
    internal static class ProductionCompilation
    {
        private static readonly Lazy<CSharpCompilation> lazyCompilation = new(Build);

        internal static CSharpCompilation Compilation => lazyCompilation.Value;

        internal static IEnumerable<SemanticModel> GetModels()
            => Compilation.SyntaxTrees.Select(tree => Compilation.GetSemanticModel(tree));

        private static CSharpCompilation Build()
        {
            CSharpParseOptions parseOptions = new(LanguageVersion.Latest);

            List<SyntaxTree> trees = ProductionSources
                .GetProductionFiles()
                .Select(filePath => CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), parseOptions, filePath))
                .ToList();

            return CSharpCompilation.Create(
                "SokobanCLI.ArchitectureAnalysis",
                trees,
                GetReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        private static List<MetadataReference> GetReferences()
        {
            SortedSet<string> paths = new(StringComparer.Ordinal);
            string? platformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

            if (platformAssemblies is not null)
            {
                foreach (string path in platformAssemblies.Split(Path.PathSeparator))
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        paths.Add(path);
                    }
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
                {
                    continue;
                }

                paths.Add(assembly.Location);
            }

            Assembly productionAssembly = typeof(SokobanCLI.Models.World).Assembly;

            foreach (AssemblyName name in productionAssembly.GetReferencedAssemblies())
            {
                Assembly? dependency = TryLoad(name);

                if (dependency is not null && !string.IsNullOrWhiteSpace(dependency.Location))
                {
                    paths.Add(dependency.Location);
                }
            }

            List<MetadataReference> references = [];

            foreach (string path in paths)
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }

            return references;
        }

        private static Assembly? TryLoad(AssemblyName name)
        {
            try
            {
                return Assembly.Load(name);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (FileLoadException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }
    }
}