﻿using System;
using System.Collections.Generic;
using System.Linq;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

// long names, Java heritage
namespace DependencyInjection.FeatureTests.Adapters {
    public class SpringAdapter : FrameworkAdapterBase {
        private readonly GenericApplicationContext context = new GenericApplicationContext();
        private readonly IObjectDefinitionFactory factory = new DefaultObjectDefinitionFactory();

        private bool contextRefreshed = false;

        public override void RegisterSingleton(Type serviceType, Type implementationType, string key) {
            this.Register(key, implementationType, serviceType, true);
        }

        public override void RegisterTransient(Type serviceType, Type implementationType, string key) {
            this.Register(key, implementationType, serviceType, false);
        }

        public override void RegisterInstance(Type serviceType, object instance, string key) {
            key = key ?? string.Format("{0} ({1})", serviceType, instance);
            this.context.ObjectFactory.RegisterSingleton(key, instance);
        }

        private void Register(string key, Type implementationType, Type serviceType, bool singleton) {
            var builder = ObjectDefinitionBuilder.RootObjectDefinition(this.factory, implementationType)
                                                 .SetAutowireMode(AutoWiringMode.AutoDetect)
                                                 .SetSingleton(singleton);

            key = key ?? string.Format("{0} ({1})", serviceType, implementationType);
            this.context.RegisterObjectDefinition(key, builder.ObjectDefinition);            
        }

        private void EnsureContextRefreshed() {
            if (this.contextRefreshed)
                return;

            this.context.Refresh();
            this.contextRefreshed = true;
        }

        protected override object DoGetInstance(Type serviceType, string key) {
            this.EnsureContextRefreshed();
            if (string.IsNullOrEmpty(key))
                return this.GetAllInstances(serviceType).First();
            
            return this.context.GetObject(key, serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType) {
            this.EnsureContextRefreshed();
            return this.context.GetObjectsOfType(serviceType).Values.Cast<object>();
        }
    }
}
