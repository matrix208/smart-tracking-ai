using System.Reflection;
using System.Runtime.Loader;

namespace Tracking.PluginLoader.LoadContexts;

public sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginAssemblyPath)
        : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Tracking.SDK MUST be shared with the host.
        // The plugin implements IProtocolPlugin from Tracking.SDK,
        // so loading another copy would break type identity.
        if (assemblyName.Name == "Tracking.SDK")
        {
            return null;
        }

        // All other plugin dependencies are resolved from the
        // plugin directory by AssemblyDependencyResolver.
        var path = _resolver.ResolveAssemblyToPath(assemblyName);

        if (path != null)
        {
            Console.WriteLine(
                $"Plugin dependency: {assemblyName.Name} -> {path}");

            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}
