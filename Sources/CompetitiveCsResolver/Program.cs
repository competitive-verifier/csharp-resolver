﻿using CompetitiveCsResolver;
using Microsoft.Build.Locator;
using System.Runtime.Loader;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

var instance = MSBuildLocator.RegisterDefaults();
AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) =>
{
    var path = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
    if (File.Exists(path))
    {
        return assemblyLoadContext.LoadFromAssemblyPath(path);
    }

    return null;
};
ConsoleApp.Run<CompetitiveCsResolverCommand>(args);
