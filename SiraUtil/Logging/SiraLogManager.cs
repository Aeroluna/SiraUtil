﻿using HarmonyLib;
using IPA.Logging;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiraUtil.Logging
{
    internal class SiraLogManager
    {
        private static Logger? _defaultLogger;
        private readonly ZenjectManager _zenjectManager;
        private readonly Dictionary<Assembly, LoggerContext> _loggerAssemblies = new Dictionary<Assembly, LoggerContext>();

        internal SiraLogManager(ZenjectManager zenjectManager)
        {
            _zenjectManager = zenjectManager;
        }

        internal void AddLogger(Assembly assembly, Logger logger)
        {
            if (!_loggerAssemblies.ContainsKey(assembly))
            {
                var zenjector = _zenjectManager.ZenjectorFromAssembly(assembly);
                if (zenjector is not null)
                {
                    _loggerAssemblies.Add(assembly, new LoggerContext(logger, zenjector.Slog));
                }
                else
                {
                    Plugin.Log.Warn("There is no zenjector associated with this assembly. Make sure to get your Zenjector from BSIPA's [Init] injector.");
                }
            }
        }

        internal LoggerContext LoggerFromAssembly(Assembly assembly)
        {
            if (_loggerAssemblies.TryGetValue(assembly, out LoggerContext context))
                return context;

            if (_defaultLogger is null)
                _defaultLogger = (AccessTools.Constructor(typeof(StandardLogger), new Type[] { typeof(string) }).Invoke(new object[] { "???" }) as StandardLogger)!;
            Plugin.Log.Warn($"{assembly.GetName().Name}, you are depending on a SiraLog, but you haven't setup your own! You can setup your own by calling .UseLogger() on your zenjector.");
            Plugin.Log.Warn("Using the default SiraLog...");
            return new LoggerContext(_defaultLogger, false);
        }

        internal struct LoggerContext
        {
            public Logger logger;
            public bool debugMode;

            public LoggerContext(Logger logger, bool defaultToDebugMode)
            {
                this.logger = logger;
                debugMode = defaultToDebugMode;
            }
        }
    }
}