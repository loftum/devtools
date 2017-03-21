using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Pat.Models;

namespace Pat.ViewModels
{
    public class VmFactory
    {
        private readonly List<object> _proxies = new List<object>();
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        public VmBuilder<T> Build<T>()
        {
            return new VmBuilder<T>(this, Generator);
        }

        public void Add(object proxy)
        {
            _proxies.Add(proxy);
        }

        public void Save()
        {
            foreach (var persistors in _proxies.OfType<IPersistor>())
            {
                persistors.Save();
            }
        }

        public T Create<T>() where T : class
        {
            return Build<T>()
                .WithPropertyChangeNotifier()
                .Create();
        }

        public T Create<T>(T target) where T : class
        {
            return Build<T>()
                .WithTarget(target)
                .WithPropertyChangeNotifier()
                .Create();
        }
    }



    public class VmBuilder<T>
    {
        private readonly ProxyGenerator _generator;
        private readonly ProxyGenerationOptions _options = new ProxyGenerationOptions();
        private T _target;
        private readonly VmFactory _factory;
        private readonly bool _isInterfaceProxy;
        private readonly Type _type = typeof(T);

        public VmBuilder(VmFactory factory, ProxyGenerator generator)
        {
            _generator = generator;
            _factory = factory;
            _isInterfaceProxy = _type.IsInterface;
        }

        public VmBuilder<T> WithTarget(T target)
        {
            _target = target;
            return this;
        }

        public VmBuilder<T> WithPropertyChangeNotifier()
        {
            _options.AddMixinInstance(new PropertyChangeNotifier());
            return this;
        }

        public VmBuilder<T> Persistent(Action<object> save)
        {
            _options.AddMixinInstance(new DelegatePersistor(() => save(_target)));
            return this;
        }

        public T Create()
        {
            var proxy = GenerateProxy();
            _factory.Add(proxy);
            return proxy;
        }

        private T GenerateProxy()
        {
            if (_isInterfaceProxy)
            {
                return _target == null
                    ? (T) _generator.CreateInterfaceProxyWithoutTarget(_type, _options, new PropertyChangedInterceptor())
                    : (T)_generator.CreateInterfaceProxyWithTarget(_type, _target, _options, new PropertyChangedInterceptor());
            }
            return _target == null
                ? (T) _generator.CreateClassProxy(_type, _options, new PropertyChangedInterceptor())
                : (T) _generator.CreateClassProxyWithTarget(_type, _target, _options, new PropertyChangedInterceptor());
        }
    }

    public interface IPersistor
    {
        void Save();
    }

    public class DelegatePersistor : IPersistor
    {
        private readonly Action _save;

        public DelegatePersistor(Action save)
        {
            _save = save;
        }

        public void Save()
        {
            _save();
        }
    }

    public interface IPropertyChangeNotifier : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName);
    }

    public class PropertyChangedInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            if (!invocation.Method.IsSpecialName || !invocation.Method.Name.StartsWith("set_"))
            {
                return;
            }
            var proxy = invocation.Proxy as IPropertyChangeNotifier;
            
            if (proxy == null)
            {
                return;
            }
            var propertyName = invocation.Method.Name.Substring("set_".Length);
            proxy.OnPropertyChanged(propertyName);
        }
    }

    public class PropertySetterHook : IProxyGenerationHook
    {
        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
            var what = memberInfo.Name;
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Property;
        }

        public void MethodsInspected()
        {
        }
    }
}