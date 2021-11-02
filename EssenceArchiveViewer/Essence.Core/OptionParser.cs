using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Essence.Core
{
  public sealed class OptionParser
  {
    private readonly List<IOption> options = new();
    private readonly List<string> unnamedValues = new();
    private readonly Dictionary<string, List<string>> unhandledValues = new(StringComparer.InvariantCultureIgnoreCase);

    public OptionParser()
      : this("-", "--", false)
    {
    }

    public OptionParser(string shortPrefix, string longPrefix, bool allowUnhandled)
    {
        ShortPrefix = shortPrefix ?? throw new ArgumentNullException(nameof (shortPrefix));
      LongPrefix = longPrefix ?? throw new ArgumentNullException(nameof (longPrefix));
      AllowUnhandled = allowUnhandled;
    }

    public string ShortPrefix { get; }

    public string LongPrefix { get; }

    public bool AllowUnhandled { get; }

    private IOption GetOption<T>(T name) => options.FirstOrDefault<IOption>(o => o.Equals(name)) ?? throw new ArgumentException(
        $"Option '{name}' not defined.");

    private TypedValueOption<OptionType> GetOption<T, OptionType>(T name)
    {
      var option = options.FirstOrDefault<IOption>(o => o.Equals(name));
      if (option == null)
        throw new ArgumentException($"Option '{name}' not defined.");
      if (option is not ValueOption valueOption)
        throw new ArgumentException($"Option '{name}' does not supply a value.");
      return valueOption is TypedValueOption<OptionType> typedValueOption ? typedValueOption : throw new ArgumentException(
          $"Option '{name}' is of a different type than {typeof(OptionType).Name}.");
    }

    public void Register(string longName, string description) => Register(char.MinValue, longName, description);

    public void Register(char shortName, string longName, string description)
    {
      if (longName == null)
        throw new ArgumentNullException(nameof (longName));
      if (longName.Length <= 1)
        throw new ArgumentException($"Argument longName '{longName}' must be at least two characters in length.");
      if (description == null)
        throw new ArgumentNullException(nameof (description));
      if (shortName != char.MinValue && options.FirstOrDefault<IOption>(o => o.Equals(shortName)) != null)
        throw new ArgumentException($"Option '{shortName}' already defined.");
      if (options.FirstOrDefault<IOption>(o => o.Equals(longName)) != null)
        throw new ArgumentException($"Option '{longName}' already defined.");
      options.Add(new SwitchOption(shortName, longName, description));
    }

    public void Register<OptionType>(string longName, string description) where OptionType : new() => Register<OptionType>(char.MinValue, longName, description);

    public void Register<OptionType>(char shortName, string longName, string description) where OptionType : new() => Register<OptionType>(shortName, longName, description, true, false, new OptionType());

    public void Register<OptionType>(string longName, string description, OptionType defaultValue) => Register<OptionType>(char.MinValue, longName, description, defaultValue);

    public void Register<OptionType>(
      char shortName,
      string longName,
      string description,
      OptionType defaultValue)
    {
      Register<OptionType>(shortName, longName, description, false, false, defaultValue);
    }

    public void Register<OptionType>(
      string longName,
      string description,
      bool required,
      bool variable,
      OptionType defaultValue)
    {
      Register<OptionType>(char.MinValue, longName, description, required, variable, defaultValue);
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
        throw new ArgumentException($"Argument longName '{longName}' must be at least two characters in length.");
      if (description == null)
        throw new ArgumentNullException(nameof (description));
      if (shortName != char.MinValue && options.FirstOrDefault<IOption>(o => o.Equals(shortName)) != null)
        throw new ArgumentException($"Option '{shortName}' already defined.");
      if (options.FirstOrDefault<IOption>(o => o.Equals(longName)) != null)
        throw new ArgumentException($"Option '{longName}' already defined.");
      options.Add(new TypedValueOption<OptionType>(shortName, longName, description, required, variable, defaultValue));
    }

    public string[] GetUsage(bool fixedWidth)
    {
      var source = new Usage[options.Count];
      for (var index = 0; index < options.Count; ++index)
      {
        var option = options[index];
        var stringBuilder = new StringBuilder();
        if (option.ShortName != char.MinValue)
        {
          stringBuilder.Append(ShortPrefix);
          stringBuilder.Append(option.ShortName);
        }
        if (!string.IsNullOrEmpty(option.LongName))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(", ");
          stringBuilder.Append(LongPrefix);
          stringBuilder.Append(option.LongName);
        }
        if (option is ValueOption)
          stringBuilder.Append(" <value>");
        source[index] = new Usage(stringBuilder.ToString(), option.GetExtendedDescription());
      }
      var format = fixedWidth ? $"{{0,{-source.Max<Usage>(u => u.Name.Length)}}} | {{1}}" : "{0} | {1}";
      var usage = new string[source.Length];
      for (var index = 0; index < source.Length; ++index)
        usage[index] = string.Format(format, source[index].Name, source[index].Description);
      return usage;
    }

    public bool WasSupplied(char shortName) => GetOption<char>(shortName).Supplied;

    public bool WasSupplied(string longName) => GetOption<string>(longName).Supplied;

    public OptionType GetValue<OptionType>(char shortName)
    {
      var option = GetOption<char, OptionType>(shortName);
      return option.Values.Count >= 1 ? option.Values[0] : option.DefaultValue;
    }

    public OptionType GetValue<OptionType>(string longName)
    {
      var option = GetOption<string, OptionType>(longName);
      return option.Values.Count >= 1 ? option.Values[0] : option.DefaultValue;
    }

    public bool TryGetValue<OptionType>(char shortName, out OptionType value)
    {
      var option = GetOption<char, OptionType>(shortName);
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
      var option = GetOption<string, OptionType>(longName);
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
      var option = GetOption<char, OptionType>(shortName);
      return option.Variable ? option.Values.ToArray() : throw new ArgumentException(
          $"Option '{shortName}' does not accept a variable amount of arguments.");
    }

    public OptionType[] GetValues<OptionType>(string longName)
    {
      var option = GetOption<string, OptionType>(longName);
      return option.Variable ? option.Values.ToArray() : throw new ArgumentException(
          $"Option '{longName}' does not accept a variable amount of arguments.");
    }

    public ReadOnlyCollection<string> GetUnnamedValues() => unnamedValues.AsReadOnly();

    public void RemoveUnnamedValue(string unnamedValue) => unnamedValues.RemoveAll(item => item == unnamedValue);

    public void RemoveUnnamedValues(Predicate<string> predicate) => unnamedValues.RemoveAll(predicate);

    public bool WasUnhandledSupplied(string name)
    {
      if (!AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      return unhandledValues.ContainsKey(name);
    }

    public string GetUnhandledValue(string name)
    {
      if (!AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      return unhandledValues.TryGetValue(name, out var stringList) && stringList.Count >= 1 ? stringList[0] : null;
    }

    public bool TryGetUnhandledValue(string name, out string value)
    {
      if (!AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      if (unhandledValues.TryGetValue(name, out var stringList) && stringList.Count >= 1)
      {
        value = stringList[0];
        return true;
      }
      value = null;
      return false;
    }

    public ReadOnlyCollection<string> GetValues(string name)
    {
      if (!AllowUnhandled)
        throw new ApplicationException("Unhandled values not allowed.");
      return unhandledValues.TryGetValue(name, out var stringList) ? stringList.AsReadOnly() : new ReadOnlyCollection<string>(Array.Empty<string>());
    }

    public void Parse()
    {
      var commandLineArgs = Environment.GetCommandLineArgs();
      var destinationArray = new string[Math.Max(0, commandLineArgs.Length - 1)];
      if (commandLineArgs.Length != 0)
        Array.Copy(commandLineArgs, 1, destinationArray, 0, commandLineArgs.Length - 1);
      Parse(destinationArray);
    }

    public void Parse(params string[] args)
    {
      foreach (var option in options)
        option.Reset();
      unnamedValues.Clear();
      for (var index = 0; index < args.Length; ++index)
      {
        var str1 = args[index];
        var flag1 = str1.StartsWith(ShortPrefix);
        var flag2 = str1.StartsWith(LongPrefix);
        if (flag1 | flag2)
        {
          var option = (IOption) null;
          if (flag1 && str1.Length == ShortPrefix.Length + 1)
          {
            var shortName = str1[ShortPrefix.Length];
            option = options.FirstOrDefault<IOption>(o => o.Equals(shortName));
          }
          else if (flag2)
          {
            var longName = str1.Substring(LongPrefix.Length);
            option = options.FirstOrDefault<IOption>(o => o.Equals(longName));
          }
          switch (option)
          {
            case null:
              if (!AllowUnhandled)
                throw new ApplicationException($"Argument '{str1}' not recognised.");
              var str2 = (string) null;
              if (flag2)
                str2 = LongPrefix;
              else if (flag1)
                str2 = ShortPrefix;
              var key = str1.Substring(str2.Length);
              if (!unhandledValues.TryGetValue(key, out var stringList))
              {
                stringList = new List<string>();
                unhandledValues.Add(key, stringList);
              }
              if (index + 1 < args.Length)
              {
                var str3 = args[index + 1];
                if (!str3.StartsWith(ShortPrefix) && !str3.StartsWith(LongPrefix))
                {
                  stringList.Add(str3);
                  ++index;
                  continue;
                }
                continue;
              }
              continue;
            case SwitchOption _:
              ((SwitchOption) option).Supplied = true;
              continue;
            case ValueOption _:
              var valueOption = (ValueOption) option;
              if (valueOption.Supplied && !valueOption.Variable)
                throw new ApplicationException($"Argument '{str1}' supplied more than once.");
              if (index + 1 == args.Length)
                throw new ApplicationException($"Argument '{str1}' missing value.");
              var str4 = args[++index];
              valueOption.Parse(str1, str4);
              continue;
            default:
              throw new NotImplementedException();
          }
        }
        else
          unnamedValues.Add(str1);
      }
      foreach (var option in options)
      {
        if (option is ValueOption {Required: true, Supplied: false} valueOption)
          throw new ApplicationException($"Required argument '{LongPrefix}{valueOption.LongName}' not supplied.");
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

    private sealed class SwitchOption : IOption, IEquatable<char>, IEquatable<string>
    {
      public SwitchOption(char shortName, string longName, string description)
      {
        ShortName = shortName;
        LongName = longName;
        Description = description;
        Reset();
      }

      public char ShortName { get; }

      public string LongName { get; }

      public string Description { get; }

      public bool Supplied { get; set; }

      public bool Equals(char shortName) => ShortName == shortName;

      public bool Equals(string longName) => LongName.Equals(longName, StringComparison.InvariantCultureIgnoreCase);

      public string GetExtendedDescription() => Description;

      public void Reset() => Supplied = false;

      public override bool Equals(object obj)
      {
          if (obj is char nullable)
          return Equals(nullable);
        return obj is string longName && Equals(longName);
      }

      public override int GetHashCode() => LongName.GetHashCode();

      public override string ToString() => LongName;
    }

    private abstract class ValueOption : IOption, IEquatable<char>, IEquatable<string>
    {
      public ValueOption(
        char shortName,
        string longName,
        string description,
        bool required,
        bool variable)
      {
        ShortName = shortName;
        LongName = longName;
        Description = description;
        Required = required;
        Variable = variable;
      }

      public char ShortName { get; }

      public string LongName { get; }

      public string Description { get; }

      public bool Required { get; }

      public bool Variable { get; }

      public abstract bool Supplied { get; }

      public bool Equals(char shortName) => ShortName == shortName;

      public bool Equals(string longName) => LongName.Equals(longName, StringComparison.InvariantCultureIgnoreCase);

      public abstract string GetExtendedDescription();

      public abstract void Parse(string arg, string value);

      public abstract void Reset();

      public override bool Equals(object obj)
      {
          if (obj is char nullable)
          return Equals(nullable);
        return obj is string longName && Equals(longName);
      }

      public override int GetHashCode() => LongName.GetHashCode();

      public override string ToString() => LongName;
    }

    private sealed class TypedValueOption<T> : ValueOption
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
        DefaultValue = defaultValue;
        Values = new List<T>();
        Reset();
      }

      public override bool Supplied => Values.Count > 0;

      public T DefaultValue { get; }

      public List<T> Values { get; }

      public override string GetExtendedDescription()
      {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Description);
        if (!Required)
        {
          var defaultValue = DefaultValue;
          ref var local = ref defaultValue;
          var str = local is { } ? local.ToString() : null;
          if (!string.IsNullOrEmpty(str))
          {
            if (stringBuilder.Length > 0)
              stringBuilder.Append(" ");
            stringBuilder.AppendFormat("Defaults to {0}.", str);
          }
        }
        if (Variable)
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" ");
          stringBuilder.AppendFormat("Multiple arguments allowed.");
        }
        if (typeof (T).IsEnum)
        {
          var names = Enum.GetNames(typeof (T));
          if (names.Length != 0)
          {
            if (stringBuilder.Length > 0)
              stringBuilder.Append(" ");
            stringBuilder.Append("Supported values are ");
            for (var index = 0; index < names.Length; ++index)
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
            Values.Add((T) Enum.Parse(typeof (T), value, true));
          }
          catch (Exception ex)
          {
            throw new ApplicationException(
                $"Argument '{arg}' value '{value}' must be one of '{string.Join("; ", Enum.GetNames(typeof(T)))}'.", ex);
          }
        }
        else
        {
          try
          {
            Values.Add((T) Convert.ChangeType(value, typeof (T)));
          }
          catch (Exception ex)
          {
            throw new ApplicationException($"Argument '{arg}' value '{value}' must be of type {typeof(T).Name}.", ex);
          }
        }
      }

      public override void Reset() => Values.Clear();
    }

    private class Usage
    {
      public Usage(string name, string description)
      {
        Name = name;
        Description = description;
      }

      public string Name { get; }

      public string Description { get; }
    }
  }
}
