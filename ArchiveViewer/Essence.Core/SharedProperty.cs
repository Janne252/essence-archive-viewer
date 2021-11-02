using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Essence.Core
{
  public static class SharedProperty
  {
    public const string StorageRoot = "Z:\\Relic_Projects";
    private static readonly Lazy<SharedPropertyDatabase> Database = new(() => new SharedPropertyDatabase(Environment.MachineName), true);

    public static string BuildServer => GetSharedProperty(nameof (BuildServer));

    public static string IdServer => GetSharedProperty(nameof (IdServer));

    public static string GetSharedProperty(string name) => Database.Value.SharedProperties[name];

    public static T GetSharedProperty<T>(string name)
    {
      var sharedProperty = Database.Value.SharedProperties[name];
      return (T) (TypeDescriptor.GetConverter(typeof (T)) ?? throw new NotSupportedException()).ConvertFrom(sharedProperty);
    }

    public static string GetSharedProperty(string name, string defaultValue)
    {
        return TryGetSharedProperty(name, out var str) ? str : defaultValue;
    }

    public static T GetSharedProperty<T>(string name, T defaultValue)
    {
        return TryGetSharedProperty<T>(name, out var obj) ? obj : defaultValue;
    }

    public static bool TryGetSharedProperty(string name, out string value) => Database.Value.SharedProperties.TryGetValue(name, out value);

    public static bool TryGetSharedProperty<T>(string name, out T value)
    {
        if (Database.Value.SharedProperties.TryGetValue(name, out var str))
      {
        var converter = TypeDescriptor.GetConverter(typeof (T));
        if (converter != null)
        {
          if (converter.CanConvertFrom(typeof (string)))
          {
            try
            {
              value = (T) converter.ConvertFrom(str);
              return true;
            }
            catch
            {
            }
          }
        }
      }
      value = default;
      return false;
    }

    private class SharedPropertyDatabase
    {
      public const string BuildServer = "BuildServer";
      public const string IdServer = "IdServer";
      private static readonly KeyValuePair<string, string>[] DefaultSharedProperties = new KeyValuePair<string, string>[2]
      {
        new(nameof (BuildServer), "relbuildsvr02"),
        new(nameof (IdServer), "relbuildsvr02")
      };

      public SharedPropertyDatabase(string machineName)
      {
        var dictionary = new Dictionary<string, string>();
        foreach (var defaultSharedProperty in DefaultSharedProperties)
          dictionary.Add(defaultSharedProperty.Key, defaultSharedProperty.Value);
        try
        {
          var node = XDocument.Load("Z:\\Relic_Projects\\EssenseEngine\\SharedProperties\\SharedPropertyV2.xml");
          var xelement = node.XPathSelectElement("/SharedProperty/Properties[@defaultProperties='true']");
          if (xelement != null)
          {
            foreach (var element in xelement.Elements())
              dictionary[element.Name.LocalName] = element.Value;
          }
          if (!string.IsNullOrEmpty(machineName))
          {
            foreach (var xpathSelectElement1 in node.XPathSelectElements("/SharedProperty/Properties"))
            {
              var flag = false;
              foreach (var xpathSelectElement2 in xpathSelectElement1.XPathSelectElements("./MachineNameTargets/MachineNameTarget"))
              {
                if (string.Equals(machineName, xpathSelectElement2.Value, StringComparison.OrdinalIgnoreCase))
                {
                  flag = true;
                  break;
                }
              }
              if (flag)
              {
                foreach (var element in xpathSelectElement1.Elements())
                {
                  if (element.Name.LocalName != "MachineNameTargets")
                  {
                      if (!dictionary.TryGetValue(element.Name.LocalName, out var empty))
                      empty = string.Empty;
                    dictionary[element.Name.LocalName] = element.Value;
                    Trace.TraceWarning("Overriding shared property [{0}] value [{1}] with [{2}] for [{3}].", element.Name.LocalName, empty, element.Value, machineName);
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Trace.TraceError("Error loading {0}: {1}", "Z:\\Relic_Projects\\EssenseEngine\\SharedProperties\\SharedPropertyV2.xml", ex.Message);
        }
        SharedProperties = new ReadOnlyDictionary<string, string>(dictionary);
      }

      public ReadOnlyDictionary<string, string> SharedProperties { get; }
    }
  }
}
