﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DependencyInjection.FeatureTests.Adapters.Support;
using DependencyInjection.FeatureTests.Adapters.Support.GenericPlaceholders;
using IfInjector;

namespace DependencyInjection.FeatureTests.Adapters {
    public class IfInjectorAdapter : FrameworkAdapterBase {
        private readonly Injector injector = new Injector();
        
        public override Assembly FrameworkAssembly {
            get { return typeof(Injector).Assembly; }
        }

        public override void RegisterSingleton(Type serviceType, Type implementationType) {
            GenericHelper.RewriteAndInvoke(
                () => this.injector.Bind<P<X1>, C<X2, X1>>().AsSingleton(true),
                serviceType, implementationType
            );
        }

        public override void RegisterTransient(Type serviceType, Type implementationType) {
            GenericHelper.RewriteAndInvoke(
                () => this.injector.Bind<P<X1>, C<X2, X1>>(),
                serviceType, implementationType
            );
        }

        public override void RegisterInstance(Type serviceType, object instance) {
            GenericHelper.RewriteAndInvoke(
                () => this.injector.Bind<X1>().SetFactoryLambda((Expression<Func<X1>>)(() => (X1)instance)).AsSingleton(true),
                serviceType
            );
        }

        public override object Resolve(Type serviceType) {
            return this.injector.Resolve(serviceType);
        }
    }
}
