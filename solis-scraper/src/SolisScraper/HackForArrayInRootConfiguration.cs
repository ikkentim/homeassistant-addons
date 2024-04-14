using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace SolisScraper;

public class HackForArrayInRootConfiguration(IConfigurationSection array) : IConfiguration
{
    public IConfigurationSection GetSection(string key)
    {
        if(key == "Instances") return array;

        return Empty.Instance;
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        yield return array;
    }

    public IChangeToken GetReloadToken()
    {
        return array.GetReloadToken();
    }

    public string this[string key]
    {
        get => null;
        set {}
    }

    private class Empty : IConfigurationSection, IChangeToken, IDisposable
    {
        public static Empty Instance = new();

        public IConfigurationSection GetSection(string key)
        {
            return this;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            yield break;
        }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return this;
        }

        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;

        public void Dispose()
        {
        }

        public IChangeToken GetReloadToken()
        {
            return this;
        }

        public string this[string key]
        {
            get => null;
            set { }
        }

        public string Key => string.Empty;
        public string Path => string.Empty;
        public string Value { get => null; set { ; } }
    }
}