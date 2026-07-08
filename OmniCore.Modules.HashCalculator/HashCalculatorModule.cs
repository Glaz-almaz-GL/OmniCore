using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using OmniCore.Core.Entities;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Modules.Hash.Abstractions.Factories;
using OmniCore.Modules.Hash.Abstractions.Interfaces;
using OmniCore.Modules.Hash.Abstractions.Providers.Cryptographic.Legacy;
using OmniCore.Modules.Hash.Abstractions.Providers.Cryptographic.SHA2;
using OmniCore.Modules.Hash.Abstractions.Providers.Cryptographic.Sha3;
using OmniCore.Modules.Hash.Abstractions.Providers.NonCryptographic;
using OmniCore.Modules.Hash.Abstractions.Providers.NonCryptographic.XXH;
using OmniCore.Modules.Hash.Abstractions.Providers.NonCryptographic.XXH3;
using OmniCore.Modules.HashCalculator.Resources.Languages;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.HashCalculator
{
    public sealed class HashCalculatorModule(IStringLocalizer<HashResources> localizer, ILogger<HashCalculatorModule>? logger = null) : IModule
    {
        /// <inheritdoc/>
        public string Title => localizer["ModuleName"];

        /// <inheritdoc/>
        public string Description => localizer["ModuleDescription"];

        /// <inheritdoc/>
        public string Icon => Icons.Material.Filled.Calculate;

        /// <inheritdoc/>
        public string BaseRoute => "hash-calculator";

        /// <inheritdoc/>
        public OSPlatforms SupportedOS => OSPlatforms.All;

        /// <inheritdoc/>
        public bool Initialized { get; private set; } = false;

        /// <inheritdoc/>
        public bool HideInNavMenu => false;

        private bool _servicesRegistered = false;

        /// <inheritdoc/>
        public void AddModuleServices(IServiceCollection services)
        {
            services.AddMudServices();
            services.AddLocalization();

            // Legacy hash providers
            services.AddSingleton<IHashProvider, MD5HashProvider>();
            services.AddSingleton<IHashProvider, SHA1HashProvider>();

            // SHA-2 hash providers
            services.AddSingleton<IHashProvider, SHA256HashProvider>();
            services.AddSingleton<IHashProvider, SHA384HashProvider>();
            services.AddSingleton<IHashProvider, SHA512HashProvider>();

            // SHA-3 hash providers
            services.AddSingleton<IHashProvider, SHA3_256HashProvider>();
            services.AddSingleton<IHashProvider, SHA3_384HashProvider>();
            services.AddSingleton<IHashProvider, SHA3_512HashProvider>();

            // CRC hash providers
            services.AddSingleton<IHashProvider, Crc32HashProvider>();

            // XXH hash providers
            services.AddSingleton<IHashProvider, XxHash32Provider>();
            services.AddSingleton<IHashProvider, XxHash64Provider>();
            services.AddSingleton<IHashProvider, XxHash128Provider>();

            // XXH3 hash providers
            services.AddSingleton<IHashProvider, XxHash3Provider>();

            // Register the HashProviderFactory as a singleton
            services.AddSingleton<IHashProviderFactory, HashProviderFactory>();

            _servicesRegistered = true;

            logger?.LogInformation("Hash calculator module services registered successfully.");
        }

        /// <inheritdoc/>
        public IReadOnlyList<INavigationItem> GetNavigationItems()
        {
            return [
                new NavigationItem(
                    localizer["Page_Hash_Calculator_Title"],
                    localizer["Page_Hash_Calculator_Description"],
                    Icon,
                    "calculator",
                    SupportedOS,
                    true
                )
            ];
        }

        /// <inheritdoc/>
        public void Initialize(IServiceProvider serviceProvider)
        {
            if (!_servicesRegistered)
            {
                throw new InvalidOperationException("Hash calculator services are not registered.");
            }

            Initialized = true;
            logger?.LogInformation("HashCalculator module initialized successfully.");
        }
    }
}
