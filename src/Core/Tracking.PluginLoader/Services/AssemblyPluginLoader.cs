using System.Reflection;
using Tracking.PluginLoader.LoadContexts;
using Tracking.PluginLoader.Models;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Services;

public sealed class AssemblyPluginLoader
{
    public IProtocolPlugin Load(PluginPackage package)
    {
        Console.WriteLine("===== Plugin Debug =====");
        Console.WriteLine($"Folder       : {package.Folder}");
        Console.WriteLine($"Assembly     : {package.Manifest.Assembly}");
        Console.WriteLine($"AssemblyPath : {package.AssemblyPath}");
        Console.WriteLine($"Exists       : {File.Exists(package.AssemblyPath)}");
        Console.WriteLine("========================");

        var context = new PluginLoadContext(package.AssemblyPath);

        var assembly = context.LoadFromAssemblyPath(package.AssemblyPath);

        var type = assembly.GetType(package.Manifest.EntryPoint);

        if (type is null)
            throw new InvalidOperationException(
                $"Entry point '{package.Manifest.EntryPoint}' was not found.");

        return (IProtocolPlugin)Activator.CreateInstance(type)!;
    }
}