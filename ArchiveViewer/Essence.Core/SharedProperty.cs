// Decompiled with JetBrains decompiler
// Type: Essence.Core.SharedProperty
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
    private static readonly Lazy<SharedProperty.SharedPropertyDatabase> Database = new Lazy<SharedProperty.SharedPropertyDatabase>((Func<SharedProperty.SharedPropertyDatabase>) (() => new SharedProperty.SharedPropertyDatabase(Environment.MachineName)), true);

    public static string BuildServer => SharedProperty.GetSharedProperty(nameof (BuildServer));

    public static string IdServer => SharedProperty.GetSharedProperty(nameof (IdServer));

    public static string GetSharedProperty(string name) => SharedProperty.Database.Value.SharedProperties[name];

    public static T GetSharedProperty<T>(string name)
    {
      string sharedProperty = SharedProperty.Database.Value.SharedProperties[name];
      return (T) (TypeDescriptor.GetConverter(typeof (T)) ?? throw new NotSupportedException()).ConvertFrom((object) sharedProperty);
    }

    public static string GetSharedProperty(string name, string defaultValue)
    {
      string str;
      return SharedProperty.TryGetSharedProperty(name, out str) ? str : defaultValue;
    }

    public static T GetSharedProperty<T>(string name, T defaultValue)
    {
      T obj;
      return SharedProperty.TryGetSharedProperty<T>(name, out obj) ? obj : defaultValue;
    }

    public static bool TryGetSharedProperty(string name, out string value) => SharedProperty.Database.Value.SharedProperties.TryGetValue(name, out value);

    public static bool TryGetSharedProperty<T>(string name, out T value)
    {
      string str;
      if (SharedProperty.Database.Value.SharedProperties.TryGetValue(name, out str))
      {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof (T));
        if (converter != null)
        {
          if (converter.CanConvertFrom(typeof (string)))
          {
            try
            {
              value = (T) converter.ConvertFrom((object) str);
              return true;
            }
            catch
            {
            }
          }
        }
      }
      value = default (T);
      return false;
    }

    private class SharedPropertyDatabase
    {
      public const string BuildServer = "BuildServer";
      public const string IdServer = "IdServer";
      private static readonly KeyValuePair<string, string>[] DefaultSharedProperties = new KeyValuePair<string, string>[2]
      {
        new KeyValuePair<string, string>(nameof (BuildServer), "relbuildsvr02"),
        new KeyValuePair<string, string>(nameof (IdServer), "relbuildsvr02")
      };
      private const string SharedPropertyPath = "Z:\\Relic_Projects\\EssenseEngine\\SharedProperties\\SharedPropertyV2.xml";

      public SharedPropertyDatabase(string machineName)
      {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> defaultSharedProperty in SharedProperty.SharedPropertyDatabase.DefaultSharedProperties)
          dictionary.Add(defaultSharedProperty.Key, defaultSharedProperty.Value);
        try
        {
          XDocument node = XDocument.Load("Z:\\Relic_Projects\\EssenseEngine\\SharedProperties\\SharedPropertyV2.xml");
          XElement xelement = node.XPathSelectElement("/SharedProperty/Properties[@defaultProperties='true']");
          if (xelement != null)
          {
            foreach (XElement element in xelement.Elements())
              dictionary[element.Name.LocalName] = element.Value;
          }
          if (!string.IsNullOrEmpty(machineName))
          {
            foreach (XElement xpathSelectElement1 in node.XPathSelectElements("/SharedProperty/Properties"))
            {
              bool flag = false;
              foreach (XElement xpathSelectElement2 in xpathSelectElement1.XPathSelectElements("./MachineNameTargets/MachineNameTarget"))
              {
                if (string.Equals(machineName, xpathSelectElement2.Value, StringComparison.OrdinalIgnoreCase))
                {
                  flag = true;
                  break;
                }
              }
              if (flag)
              {
                foreach (XElement element in xpathSelectElement1.Elements())
                {
                  if (element.Name.LocalName != "MachineNameTargets")
                  {
                    string empty;
                    if (!dictionary.TryGetValue(element.Name.LocalName, out empty))
                      empty = string.Empty;
                    dictionary[element.Name.LocalName] = element.Value;
                    Trace.TraceWarning("Overriding shared property [{0}] value [{1}] with [{2}] for [{3}].", (object) element.Name.LocalName, (object) empty, (object) element.Value, (object) machineName);
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Trace.TraceError("Error loading {0}: {1}", (object) "Z:\\Relic_Projects\\EssenseEngine\\SharedProperties\\SharedPropertyV2.xml", (object) ex.Message);
        }
        this.SharedProperties = new ReadOnlyDictionary<string, string>((IDictionary<string, string>) dictionary);
      }

      public ReadOnlyDictionary<string, string> SharedProperties { get; }
    }
  }
}
