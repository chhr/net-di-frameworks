﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DependencyInjection.FeatureTests.Adapters;
using DependencyInjection.FeatureTests.Documentation;
using DependencyInjection.FeatureTests.TestTypes;
using DependencyInjection.FeatureTests.XunitSupport;
using Xunit;

namespace DependencyInjection.FeatureTests {
    [DisplayOrder(4)]
    [DisplayName("Automatic factories (Func<TService>)")]
    [Description(@"
        When TService is registered in a container, many frameworks automatically provide Func<..., TService>.

        This is very useful for mixing lifetimes: for example, singleton (Service) may have a Func dependency on transient (DataContext),
        which allows it to get/dispose a new instance of transient when it is needed.
    ")]
    [SpecialCase(typeof(NinjectAdapter), "Ninject supports this through Ninject.Extensions.Factory, but attempts to install it from NuGet clash with Castle version.", Skip = true)]
    public class FuncTests {
        [Feature]
        [DisplayName("No parameters")]
        [Description("Registration of TService automatically provides Func<TService>.")]
        public void FactoryWithNoParameters(IFrameworkAdapter framework) {
            framework.Register<IService, IndependentService>();
            framework.Register<ServiceWithSimpleConstructorDependency>();

            var func = framework.Resolve<Func<ServiceWithSimpleConstructorDependency>>();

            Assert.NotNull(func);
            var result = func();
            Assert.NotNull(result);
        }

        [Feature]
        [DisplayName("Parameter for dependency")]
        [Description(@"
            Registration of TService automatically provides Func<..., TService>, where any
            arguments to the factory define constructor parameter overrides.

            For example, registration Service(IService1, IService2) can provide Func<IService2, Service>
            or Func<IService1, Service> or Func<IService1, Service2, Service>. Remaining parameters are
            provided by the container.
        ")]
        [DependsOnFeature("FactoryWithNoParameters")]
        public void FactoryWithParameter(IFrameworkAdapter framework) {
            framework.Register<IService, IndependentService>();
            framework.Register<ServiceWithTwoConstructorDependencies>();
            var service2 = new IndependentService2();

            var func = framework.Resolve<Func<IService2, ServiceWithTwoConstructorDependencies>>();
           
            Assert.NotNull(func);
            var result = func(service2);
            Assert.NotNull(result);
            Assert.Same(service2, result.Service2);
        }

        [Feature]
        [DisplayName("Singleton using transient")]
        [Description(@"
            When TService is transient and Func<TService> was obtained by singleton, Func should return new
            instance on each call.
        ")]
        [DependsOnFeature("FactoryWithNoParameters")]
        public void TransientFactoryUsedBySingletonStillCreatesTransient(IFrameworkAdapter framework) {
            framework.RegisterTransient<IService, IndependentService>();
            framework.RegisterSingleton<ServiceWithFuncConstructorDependency>();

            var service = framework.Resolve<ServiceWithFuncConstructorDependency>();
            var first = service.Factory();
            var second = service.Factory();

            Assert.NotSame(first, second);
        }
    }
}
