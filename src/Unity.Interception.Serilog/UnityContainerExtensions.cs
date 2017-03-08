﻿using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Serilog;
using Serilog.Events;

namespace Unity.Interception.Serilog
{
    public static class UnityContainerExtensions
    {
        public static IUnityContainer ConfigureSerilog(this IUnityContainer container, Action<LoggerConfiguration> configureLogger,
            Type[] expectedExceptions = null, MethodIdentifier[] ignoredMethods = null, LogEventLevel level = LogEventLevel.Debug)
        {
            var configuration = new LoggerConfiguration();
            configureLogger(configuration);
            return container.ConfigureSerilog(configuration, expectedExceptions, ignoredMethods, level);
        }

        public static IUnityContainer ConfigureSerilog(this IUnityContainer container, LoggerConfiguration configuration,
            Type[] expectedExceptions = null, MethodIdentifier[] ignoredMethods = null, LogEventLevel level = LogEventLevel.Debug)
        {
            var properties = GetProperties(container);
            configuration.Enrich.With(new CommonPropertiesEnricher(properties));
            // CreateLogger can only be called once
            ILogger logger = configuration.CreateLogger();
            container.RegisterInstance(logger);
            container.RegisterInstance<ISerilogOptions>(new SerilogOptions(expectedExceptions, ignoredMethods, level));
            return container;
        }

        private static ICommonProperties GetProperties(IUnityContainer container)
        {
            if (!container.IsRegistered<ICommonProperties>())
            {
                container.RegisterType<ICommonProperties, CommonProperties>(new ContainerControlledLifetimeManager());
            }
            return container.Resolve<ICommonProperties>();
        }

        public static IUnityContainer RegisterLoggedType<TFrom, TTo>(this IUnityContainer container,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            InjectionMember[] members = Init(container, injectionMembers);
            return container.RegisterType<TFrom, TTo>(members);
        }

        public static IUnityContainer RegisterLoggedType<TFrom, TTo>(this IUnityContainer container, string name,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            InjectionMember[] members = Init(container, injectionMembers);
            return container.RegisterType<TFrom, TTo>(name, members);
        }

        public static IUnityContainer RegisterLoggedType<TFrom, TTo>(this IUnityContainer container,
            LifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            InjectionMember[] members = Init(container, injectionMembers);
            return container.RegisterType<TFrom, TTo>(lifetimeManager, members);
        }

        public static IUnityContainer RegisterLoggedType<TFrom, TTo>(this IUnityContainer container, string name,
            LifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            InjectionMember[] members = Init(container, injectionMembers);
            return container.RegisterType<TFrom, TTo>(name, lifetimeManager, members);
        }

        public static IUnityContainer RegisterLoggedInstance<TInterface>(this IUnityContainer container,
            TInterface instance)
        {
            InjectionMember[] members = InitForInstance(container, instance);
            return container.RegisterType<TInterface>(members);
        }

        public static IUnityContainer RegisterLoggedInstance<TInterface>(this IUnityContainer container,
            TInterface instance, LifetimeManager lifetimeManager)
        {
            InjectionMember[] members = InitForInstance(container, instance);
            return container.RegisterType<TInterface>(lifetimeManager, members);
        }

        public static IUnityContainer RegisterLoggedInstance<TInterface>(this IUnityContainer container, string name,
            TInterface instance)
        {
            InjectionMember[] members = InitForInstance(container, instance);
            return container.RegisterType<TInterface>(name, members);
        }

        public static IUnityContainer RegisterLoggedInstance<TInterface>(this IUnityContainer container, string name,
            TInterface instance, LifetimeManager lifetimeManager)
        {
            InjectionMember[] members = InitForInstance(container, instance);
            return container.RegisterType<TInterface>(name, lifetimeManager, members);
        }

        #region Helpers

        private static void Init(this IUnityContainer container)
        {
            if (container.Configure<Microsoft.Practices.Unity.InterceptionExtension.Interception>() == null)
                container.AddNewExtension<Microsoft.Practices.Unity.InterceptionExtension.Interception>();
            if (!container.IsRegistered<IStopWatch>())
                container.RegisterType<IStopWatch, Stopwatch>();
        }

        private static InjectionMember[] Init(IUnityContainer container, params InjectionMember[] injectionMembers)
        {
            container.Init();
            InjectionMember[] loggingMembers =
            {
                new Interceptor<InterfaceInterceptor>(),
                new InterceptionBehavior<LoggingInterceptionBehavior>()
            };
            return injectionMembers.Concat(loggingMembers).ToArray();
        }

        private static InjectionMember[] InitForInstance<TInterface>(IUnityContainer container, TInterface instance)
        {
            return Init(container, new InjectionFactory(c => instance));
        }

        #endregion
    }
}