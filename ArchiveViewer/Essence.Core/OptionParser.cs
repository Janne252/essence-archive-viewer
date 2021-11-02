// Decompiled with JetBrains decompiler
// Type: Essence.Core.OptionParser
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Essence.Core
{
  public sealed class OptionParser
  {
    private List<OptionParser.IOption> options = new List<OptionParser.IOption>();
    private List<string> unnamedValues = new List<string>();
    private Dictionary<string, List<string>> unhandledValues = new Dictionary<string, List<string>>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

    public OptionParser()
      : this("-", "--", false)
    {
    }

    public OptionParser(string shortPrefix, string longPrefix, bool allowUnhandled)
    {
      if (shortPrefix == null)
        throw new ArgumentNullException(nameof (shortPrefix));
      if (longPrefix == null)
        throw new ArgumentNullException(nameof (longPrefix));
      this.ShortPrefix = shortPrefix;
      this.LongPrefix = longPrefix;
      this.AllowUnhandled = allowUnhandled;
    }

    public string ShortPrefix { get; }

    public string LongPrefix { get; }

    public bool AllowUnhandled { get; }

    private OptionParser.IOption GetOption<T>(T name) => this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => o.Equals((object) (T) name))) ?? throw new ArgumentException(string.Format("Option '{0}' not defined.", (object) name));

    private OptionParser.TypedValueOption<OptionType> GetOption<T, OptionType>(T name)
    {
      OptionParser.IOption option = this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => o.Equals((object) (T) name)));
      if (option == null)
        throw new ArgumentException(string.Format("Option '{0}' not defined.", (object) name));
      if (!(option is OptionParser.ValueOption valueOption))
        throw new ArgumentException(string.Format("Option '{0}' does not supply a value.", (object) name));
      return valueOption is OptionParser.TypedValueOption<OptionType> typedValueOption ? typedValueOption : throw new ArgumentException(string.Format("Option '{0}' is of a different type than {1}.", (object) name, (object) typeof (OptionType).Name));
    }

    public void Register(string longName, string description) => this.Register(char.MinValue, longName, description);

    public void Register(char shortName, string longName, string description)
    {
      if (longName == null)
        throw new ArgumentNullException(nameof (longName));
      if (longName.Length <= 1)
        throw new ArgumentException(string.Format("Argument longName '{0}' must be at least two characters in length.", (object) longName));
      if (description == null)
        throw new ArgumentNullException(nameof (description));
      if (shortName != char.MinValue && this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<char>) o).Equals(shortName))) != null)
        throw new ArgumentException(string.Format("Option '{0}' already defined.", (object) shortName));
      if (this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<string>) o).Equals(longName))) != null)
        throw new ArgumentException(string.Format("Option '{0}' already defined.", (object) longName));
      this.options.Add((OptionParser.IOption) new OptionParser.SwitchOption(shortName, longName, description));
    }

    public void Register<OptionType>(string longName, string description) where OptionType : new() => this.Register<OptionType>(char.MinValue, longName, description);

    public void Register<OptionType>(char shortName, string longName, string description) where OptionType : new() => this.Register<OptionType>(shortName, longName, description, true, false, new OptionType());

    public void Register<OptionType>(string longName, string description, OptionType defaultValue) => this.Register<OptionType>(char.MinValue, longName, description, defaultValue);

    public void Register<OptionType>(
      char shortName,
      string longName,
      string description,
      OptionType defaultValue)
    {
      this.Register<OptionType>(shortName, longName, description, false, false, defaultValue);
    }

    public void Register<OptionType>(
      string longName,
      string description,
      bool required,
      bool variable,
      OptionType defaultValue)
    {
      this.Register<OptionType>(char.MinValue, longName, description, required, variable, defaultValue);
    }

    public void Register<OptionType>(
      char shortName,
      string longName,
      string description,
      bool required,
      bool variable,
      OptionType defaultValue)
    {
      if (longName == null)
        throw new ArgumentNullException(nameof (longName));
      if (longName.Length <= 1)
        throw new ArgumentException(string.Format("Argument longName '{0}' must be at least two characters in length.", (object) longName));
      if (description == null)
        throw new ArgumentNullException(nameof (description));
      if (shortName != char.MinValue && this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<char>) o).Equals(shortName))) != null)
        throw new ArgumentException(string.Format("Option '{0}' already defined.", (object) shortName));
      if (this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<string>) o).Equals(longName))) != null)
        throw new ArgumentException(string.Format("Option '{0}' already defined.", (object) longName));
      this.options.Add((OptionParser.IOption) new OptionParser.TypedValueOption<OptionType>(shortName, longName, description, required, variable, defaultValue));
    }

    public string[] GetUsage(bool fixedWidth)
    {
      OptionParser.Usage[] source = new OptionParser.Usage[this.options.Count];
      for (int index = 0; index < this.options.Count; ++index)
      {
        OptionParser.IOption option = this.options[index];
        StringBuilder stringBuilder = new StringBuilder();
        if (option.ShortName != char.MinValue)
        {
          stringBuilder.Append(this.ShortPrefix);
          stringBuilder.Append(option.ShortName);
        }
        if (!string.IsNullOrEmpty(option.LongName))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(", ");
          stringBuilder.Append(this.LongPrefix);
          stringBuilder.Append(option.LongName);
        }
        if (option is OptionParser.ValueOption)
          stringBuilder.Append(" <value>");
        source[index] = new OptionParser.Usage(stringBuilder.ToString(), option.GetExtendedDescription());
      }
      string format = fixedWidth ? string.Format("{{0,{0}}} | {{1}}", (object) -((IEnumerable<OptionParser.Usage>) source).Max<OptionParser.Usage>((Func<OptionParser.Usage, int>) (u => u.Name.Length))) : "{0} | {1}";
      string[] usage = new string[source.Length];
      for (int index = 0; index < source.Length; ++index)
        usage[index] = string.Format(format, (object) source[index].Name, (object) source[index].Description);
      return usage;
    }

    public bool WasSupplied(char shortName) => this.GetOption<char>(shortName).Supplied;

    public bool WasSupplied(string longName) => this.GetOption<string>(longName).Supplied;

    public OptionType GetValue<OptionType>(char shortName)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<char, OptionType>(shortName);
      return option.Values.Count >= 1 ? option.Values[0] : option.DefaultValue;
    }

    public OptionType GetValue<OptionType>(string longName)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<string, OptionType>(longName);
      return option.Values.Count >= 1 ? option.Values[0] : option.DefaultValue;
    }

    public bool TryGetValue<OptionType>(char shortName, out OptionType value)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<char, OptionType>(shortName);
      if (option.Values.Count >= 1)
      {
        value = option.Values[0];
        return true;
      }
      value = option.DefaultValue;
      return false;
    }

    public bool TryGetValue<OptionType>(string longName, out OptionType value)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<string, OptionType>(longName);
      if (option.Values.Count >= 1)
      {
        value = option.Values[0];
        return true;
      }
      value = option.DefaultValue;
      return false;
    }

    public OptionType[] GetValues<OptionType>(char shortName)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<char, OptionType>(shortName);
      return option.Variable ? option.Values.ToArray() : throw new ArgumentException(string.Format("Option '{0}' does not accept a variable amount of arguments.", (object) shortName));
    }

    public OptionType[] GetValues<OptionType>(string longName)
    {
      OptionParser.TypedValueOption<OptionType> option = this.GetOption<string, OptionType>(longName);
      return option.Variable ? option.Values.ToArray() : throw new ArgumentException(string.Format("Option '{0}' does not accept a variable amount of arguments.", (object) longName));
    }

    public ReadOnlyCollection<string> GetUnnamedValues() => this.unnamedValues.AsReadOnly();

    public void RemoveUnnamedValue(string unnamedValue) => this.unnamedValues.RemoveAll((Predicate<string>) (item => item == unnamedValue));

    public void RemoveUnnamedValues(Predicate<string> predicate) => this.unnamedValues.RemoveAll(predicate);

    public bool WasUnhandledSupplied(string name)
    {
      if (!this.AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      return this.unhandledValues.ContainsKey(name);
    }

    public string GetUnhandledValue(string name)
    {
      if (!this.AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      List<string> stringList;
      return this.unhandledValues.TryGetValue(name, out stringList) && stringList.Count >= 1 ? stringList[0] : (string) null;
    }

    public bool TryGetUnhandledValue(string name, out string value)
    {
      if (!this.AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      List<string> stringList;
      if (this.unhandledValues.TryGetValue(name, out stringList) && stringList.Count >= 1)
      {
        value = stringList[0];
        return true;
      }
      value = (string) null;
      return false;
    }

    public ReadOnlyCollection<string> GetValues(string name)
    {
      if (!this.AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      List<string> stringList;
      return this.unhandledValues.TryGetValue(name, out stringList) ? stringList.AsReadOnly() : new ReadOnlyCollection<string>((IList<string>) new string[0]);
    }

    public void Parse()
    {
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      string[] destinationArray = new string[Math.Max(0, commandLineArgs.Length - 1)];
      if (commandLineArgs.Length != 0)
        Array.Copy((Array) commandLineArgs, 1, (Array) destinationArray, 0, commandLineArgs.Length - 1);
      this.Parse(destinationArray);
    }

    public void Parse(params string[] args)
    {
      foreach (OptionParser.IOption option in this.options)
        option.Reset();
      this.unnamedValues.Clear();
      for (int index = 0; index < args.Length; ++index)
      {
        string str1 = args[index];
        bool flag1 = str1.StartsWith(this.ShortPrefix);
        bool flag2 = str1.StartsWith(this.LongPrefix);
        if (flag1 | flag2)
        {
          OptionParser.IOption option = (OptionParser.IOption) null;
          if (flag1 && str1.Length == this.ShortPrefix.Length + 1)
          {
            char shortName = str1[this.ShortPrefix.Length];
            option = this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<char>) o).Equals(shortName)));
          }
          else if (flag2)
          {
            string longName = str1.Substring(this.LongPrefix.Length);
            option = this.options.FirstOrDefault<OptionParser.IOption>((Func<OptionParser.IOption, bool>) (o => ((IEquatable<string>) o).Equals(longName)));
          }
          switch (option)
          {
            case null:
              if (!this.AllowUnhandled)
                throw new ApplicationException(string.Format("Argument '{0}' not recognised.", (object) str1));
              string str2 = (string) null;
              if (flag2)
                str2 = this.LongPrefix;
              else if (flag1)
                str2 = this.ShortPrefix;
              string key = str1.Substring(str2.Length);
              List<string> stringList;
              if (!this.unhandledValues.TryGetValue(key, out stringList))
              {
                stringList = new List<string>();
                this.unhandledValues.Add(key, stringList);
              }
              if (index + 1 < args.Length)
              {
                string str3 = args[index + 1];
                if (!str3.StartsWith(this.ShortPrefix) && !str3.StartsWith(this.LongPrefix))
                {
                  stringList.Add(str3);
                  ++index;
                  continue;
                }
                continue;
              }
              continue;
            case OptionParser.SwitchOption _:
              ((OptionParser.SwitchOption) option).Supplied = true;
              continue;
            case OptionParser.ValueOption _:
              OptionParser.ValueOption valueOption = (OptionParser.ValueOption) option;
              if (valueOption.Supplied && !valueOption.Variable)
                throw new ApplicationException(string.Format("Argument '{0}' supplied more than once.", (object) str1));
              if (index + 1 == args.Length)
                throw new ApplicationException(string.Format("Argument '{0}' missing value.", (object) str1));
              string str4 = args[++index];
              valueOption.Parse(str1, str4);
              continue;
            default:
              throw new NotImplementedException();
          }
        }
        else
          this.unnamedValues.Add(str1);
      }
      foreach (OptionParser.IOption option in this.options)
      {
        if (option is OptionParser.ValueOption valueOption && valueOption.Required && !valueOption.Supplied)
          throw new ApplicationException(string.Format("Required argument '{0}{1}' not supplied.", (object) this.LongPrefix, (object) valueOption.LongName));
      }
    }

    private interface IOption : IEquatable<char>, IEquatable<string>
    {
      char ShortName { get; }

      string LongName { get; }

      string Description { get; }

      bool Supplied { get; }

      string GetExtendedDescription();

      void Reset();
    }

    private sealed class SwitchOption : OptionParser.IOption, IEquatable<char>, IEquatable<string>
    {
      public SwitchOption(char shortName, string longName, string description)
      {
        this.ShortName = shortName;
        this.LongName = longName;
        this.Description = description;
        this.Reset();
      }

      public char ShortName { get; }

      public string LongName { get; }

      public string Description { get; }

      public bool Supplied { get; set; }

      public bool Equals(char shortName) => (int) this.ShortName == (int) shortName;

      public bool Equals(string longName) => this.LongName.Equals(longName, StringComparison.InvariantCultureIgnoreCase);

      public string GetExtendedDescription() => this.Description;

      public void Reset() => this.Supplied = false;

      public override bool Equals(object obj)
      {
        char? nullable = obj as char?;
        if (nullable.HasValue)
          return this.Equals(nullable.Value);
        return obj is string longName && this.Equals(longName);
      }

      public override int GetHashCode() => this.LongName.GetHashCode();

      public override string ToString() => this.LongName;
    }

    private abstract class ValueOption : OptionParser.IOption, IEquatable<char>, IEquatable<string>
    {
      public ValueOption(
        char shortName,
        string longName,
        string description,
        bool required,
        bool variable)
      {
        this.ShortName = shortName;
        this.LongName = longName;
        this.Description = description;
        this.Required = required;
        this.Variable = variable;
      }

      public char ShortName { get; }

      public string LongName { get; }

      public string Description { get; }

      public bool Required { get; }

      public bool Variable { get; }

      public abstract bool Supplied { get; }

      public bool Equals(char shortName) => (int) this.ShortName == (int) shortName;

      public bool Equals(string longName) => this.LongName.Equals(longName, StringComparison.InvariantCultureIgnoreCase);

      public abstract string GetExtendedDescription();

      public abstract void Parse(string arg, string value);

      public abstract void Reset();

      public override bool Equals(object obj)
      {
        char? nullable = obj as char?;
        if (nullable.HasValue)
          return this.Equals(nullable.Value);
        return obj is string longName && this.Equals(longName);
      }

      public override int GetHashCode() => this.LongName.GetHashCode();

      public override string ToString() => this.LongName;
    }

    private sealed class TypedValueOption<T> : OptionParser.ValueOption
    {
      public TypedValueOption(
        char shortName,
        string longName,
        string description,
        bool required,
        bool variable,
        T defaultValue)
        : base(shortName, longName, description, required, variable)
      {
        this.DefaultValue = defaultValue;
        this.Values = new List<T>();
        this.Reset();
      }

      public override bool Supplied => this.Values.Count > 0;

      public T DefaultValue { get; }

      public List<T> Values { get; }

      public override string GetExtendedDescription()
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(this.Description);
        if (!this.Required)
        {
          T defaultValue = this.DefaultValue;
          ref T local = ref defaultValue;
          string str = (object) local != null ? local.ToString() : (string) null;
          if (!string.IsNullOrEmpty(str))
          {
            if (stringBuilder.Length > 0)
              stringBuilder.Append(" ");
            stringBuilder.AppendFormat("Defaults to {0}.", (object) str);
          }
        }
        if (this.Variable)
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" ");
          stringBuilder.AppendFormat("Multiple arguments allowed.");
        }
        if (typeof (T).IsEnum)
        {
          string[] names = Enum.GetNames(typeof (T));
          if (names.Length != 0)
          {
            if (stringBuilder.Length > 0)
              stringBuilder.Append(" ");
            stringBuilder.Append("Supported values are ");
            for (int index = 0; index < names.Length; ++index)
            {
              stringBuilder.Append(names[index]);
              if (index + 2 < names.Length)
                stringBuilder.Append(", ");
              else if (index + 1 < names.Length)
                stringBuilder.Append(" and ");
            }
            stringBuilder.Append(".");
          }
        }
        return stringBuilder.ToString();
      }

      public override void Parse(string arg, string value)
      {
        if (typeof (T).IsEnum)
        {
          try
          {
            this.Values.Add((T) Enum.Parse(typeof (T), value, true));
          }
          catch (Exception ex)
          {
            throw new ApplicationException(string.Format("Argument '{0}' value '{1}' must be one of '{2}'.", (object) arg, (object) value, (object) string.Join("; ", Enum.GetNames(typeof (T)))), ex);
          }
        }
        else
        {
          try
          {
            this.Values.Add((T) Convert.ChangeType((object) value, typeof (T)));
          }
          catch (Exception ex)
          {
            throw new ApplicationException(string.Format("Argument '{0}' value '{1}' must be of type {2}.", (object) arg, (object) value, (object) typeof (T).Name), ex);
          }
        }
      }

      public override void Reset() => this.Values.Clear();
    }

    private class Usage
    {
      public Usage(string name, string description)
      {
        this.Name = name;
        this.Description = description;
      }

      public string Name { get; }

      public string Description { get; }
    }
  }
}
