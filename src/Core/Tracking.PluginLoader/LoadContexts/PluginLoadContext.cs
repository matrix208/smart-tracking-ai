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
        // استخدم نفس نسخ الـ SDK الموجودة في البرنامج الرئيسي
        if (assemblyName.Name is
            "Tracking.SDK" or
            "Tracking.PluginLoader")
        {
            return null;
        }

        var path = _resolver.ResolveAssemblyToPath(assemblyName);

        if (path != null)
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}