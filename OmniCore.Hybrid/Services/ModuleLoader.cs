using Microsoft.Extensions.Logging;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Hybrid.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace OmniCore.Hybrid.Services
{
    /// <summary>
    /// Загрузчик модулей, отвечающий за обнаружение, загрузку сборок и создание экземпляров модулей.
    /// </summary>
    internal sealed partial class ModuleLoader(ILogger<ModuleLoader>? logger = null) : IModuleLoader
    {
        private readonly ILogger<ModuleLoader>? _logger = logger;

        /// <inheritdoc/>
        public IReadOnlyCollection<Type> DiscoverModuleTypes(IEnumerable<Assembly> assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);

            List<Type> moduleTypes = [];

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in LoadTypes(assembly))
                {
                    bool isModule = typeof(IModule).IsAssignableFrom(type);
                    bool isValid = !type.IsInterface && !type.IsAbstract;

                    if (isModule && isValid)
                    {
                        if (_logger?.IsEnabled(LogLevel.Debug) == true)
                        {
                            _logger.LogDebug(
                                "Module type found: {TypeName} in assembly {AssemblyName}",
                                type.FullName,
                                assembly.FullName);
                        }

                        moduleTypes.Add(type);
                    }
                }
            }

            return moduleTypes.AsReadOnly();
        }

        private IEnumerable<Type> LoadTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger?.LogWarning(ex, "Failed to load some types for assembly {AssemblyName}", assembly.FullName);
                return ex.Types.Where(t => t != null)!;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load types for assembly {AssemblyName}", assembly.FullName);
                return [];
            }
        }

        /// <inheritdoc/>
        public bool IsModuleSupported(IModule module, OSPlatforms os)
        {
            ArgumentNullException.ThrowIfNull(module);

            bool supported = (module.SupportedOS & os) != OSPlatforms.None;

            if (!supported && _logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug(
                    "Module skipped: {ModuleName} (Supported OS: {SupportedOS}, current: {CurrentOS})",
                    module.GetType().Name,
                    module.SupportedOS,
                    os);
            }

            return supported;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<Assembly> LoadAssemblies(string directoryPath)
        {
            ArgumentNullException.ThrowIfNull(directoryPath);

            List<Assembly> loadedAssemblies = [];

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    _logger?.LogError("Directory not found: {DirPath}", directoryPath);
                    throw new DirectoryNotFoundException($"Assemblies directory not found: {directoryPath}");
                }

                string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly);

                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger.LogDebug("DLL files found in {DirPath}: {Count}", directoryPath, dllFiles.Length);
                }

                foreach (string dll in dllFiles)
                {
                    try
                    {
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                        loadedAssemblies.Add(assembly);
                    }
                    catch (BadImageFormatException)
                    {
                        // Игнорируем нативные библиотеки или библиотеки другой архитектуры
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error loading assembly {DllName}", Path.GetFileName(dll));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error accessing directory {DirPath}", directoryPath);
            }

            return loadedAssemblies.AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IModule> LoadModules(string source, OSPlatforms currentOS, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            List<IModule> loadedModules = [];

            IEnumerable<Assembly> assemblies = LoadAssemblies(source);
            List<Type> moduleTypes = [.. DiscoverModuleTypes(assemblies)];

            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("Total module types found: {Count}", moduleTypes.Count);
            }

            foreach (Type type in moduleTypes)
            {
                if (!TryCreateModuleInstance(type, serviceProvider, out IModule? moduleInstance)
                    || moduleInstance is null)
                {
                    continue;
                }

                if (!IsModuleSupported(moduleInstance, currentOS))
                {
                    continue;
                }

                loadedModules.Add(moduleInstance);
            }

            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("Total modules loaded: {Count}", loadedModules.Count);
            }

            return loadedModules;
        }

        /// <inheritdoc/>
        public bool TryCreateModuleInstance(Type moduleType, IServiceProvider serviceProvider, out IModule? module)
        {
            ArgumentNullException.ThrowIfNull(moduleType);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            module = null;

            module = (IModule)ActivatorUtilities.CreateInstance(serviceProvider, moduleType);

            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("Instance created: {ModuleName}", moduleType.Name);
            }

            return true;
        }
    }
}