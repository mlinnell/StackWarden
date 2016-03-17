﻿using System.Collections.Generic;
using System.Linq;
using StackWarden.Core.Configuration;
using StackWarden.Core.Extensions;

namespace StackWarden.Monitoring.Configuration
{
    public abstract class MonitorConfigurationDrivenFactory<TDefinition, TMonitor> : ConfigurationDrivenFactory<TDefinition, TMonitor>, IMonitorFactory
        where TMonitor: IMonitor
        where TDefinition: IMonitorConfiguration
    {
        private readonly IMonitorResultHandlerFactory _resultHandlerFactory;

        protected override string ConfigExtension => "monitorconfig";

        protected MonitorConfigurationDrivenFactory(string configPath, IConfigurationReader configurationReader, IMonitorResultHandlerFactory resultHandlerFactory)
            : base(configPath, configurationReader)
        {
            _resultHandlerFactory = resultHandlerFactory.ThrowIfNull(nameof(resultHandlerFactory));
        }

        protected override void ApplyPostBuildConfiguration(TDefinition config, TMonitor instance)
        {
            if (config.Interval.HasValue)
                instance.Interval = config.Interval.Value;

            if (config.Tags?.Any() ?? false)
                instance.Tags.AddRange(config.Tags);

            foreach (var currentHandler in config.Handlers.Select(x => _resultHandlerFactory.Build(x, true)))
                instance.Updated += currentHandler.Handle;

            if (!string.IsNullOrWhiteSpace(config.DisplayName))
                instance.Name = config.DisplayName;
        }

        // Explicit IFactory<IMonitor> implementation
        IEnumerable<string> IFactory<IMonitor>.SupportedValues => SupportedValues;

        IMonitor IFactory<IMonitor>.Build(string name, bool useExistingInstance)
        {
            return Build(name, useExistingInstance);
        }

        IEnumerable<IMonitor> IFactory<IMonitor>.BuildAll()
        {
            return BuildAll().Cast<IMonitor>();
        }
    }
}