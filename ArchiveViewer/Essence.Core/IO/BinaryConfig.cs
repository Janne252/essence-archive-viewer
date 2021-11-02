using System;
using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfig
  {
    private readonly BinaryConfigTableNodeBase m_root;
    private readonly Stack<BinaryConfigTableNodeBase> m_stack;

    public BinaryConfig()
    {
      m_root = new BinaryConfigTableNode(new DictionaryKey());
      m_stack = new Stack<BinaryConfigTableNodeBase>();
      m_stack.Push(m_root);
    }

    public int Count => CurrentTable.Children.Count;

    public BinaryConfigTableNodeBase CurrentTable => m_stack.Peek();

    public void AddFloat(string key, float value) => CurrentTable.Children.Add(new BinaryConfigFloatNode(key)
    {
        Value = value
    });

    public float GetFloat(string key) => GetNode<BinaryConfigFloatNode>(key).Value;

    public float GetFloatAt(int index) => GetNodeAt<BinaryConfigFloatNode>(index).Value;

    public bool TryGetFloat(string key, out float value)
    {
        if (TryGetNode<BinaryConfigFloatNode>(key, out var node))
      {
        value = node.Value;
        return true;
      }
      value = 0.0f;
      return false;
    }

    public bool TryGetFloatAt(int index, out float value)
    {
        if (TryGetNodeAt<BinaryConfigFloatNode>(index, out var node))
      {
        value = node.Value;
        return true;
      }
      value = 0.0f;
      return false;
    }

    public void AddInt(string key, int value) => CurrentTable.Children.Add(new BinaryConfigIntNode(key)
    {
        Value = value
    });

    public int GetInt(string key) => GetNode<BinaryConfigIntNode>(key).Value;

    public int GetIntAt(int index) => GetNodeAt<BinaryConfigIntNode>(index).Value;

    public bool TryGetInt(string key, out int value)
    {
        if (TryGetNode<BinaryConfigIntNode>(key, out var node))
      {
        value = node.Value;
        return true;
      }
      value = 0;
      return false;
    }

    public bool TryGetIntAt(int index, out int value)
    {
        if (TryGetNodeAt<BinaryConfigIntNode>(index, out var node))
      {
        value = node.Value;
        return true;
      }
      value = 0;
      return false;
    }

    public void AddBool(string key, bool value) => CurrentTable.Children.Add(new BinaryConfigBoolNode(key)
    {
        Value = value
    });

    public bool GetBool(string key) => GetNode<BinaryConfigBoolNode>(key).Value;

    public bool GetBoolAt(int index) => GetNodeAt<BinaryConfigBoolNode>(index).Value;

    public bool TryGetBool(string key, out bool value)
    {
        if (TryGetNode<BinaryConfigBoolNode>(key, out var node))
      {
        value = node.Value;
        return true;
      }
      value = false;
      return false;
    }

    public bool TryGetBoolAt(int index, out bool value)
    {
        if (TryGetNodeAt<BinaryConfigBoolNode>(index, out var node))
      {
        value = node.Value;
        return true;
      }
      value = false;
      return false;
    }

    public void AddString(string key, string value)
    {
      var children = CurrentTable.Children;
      var configStringNode = new BinaryConfigStringNode(key)
      {
          Value = value
      };
      children.Add(configStringNode);
    }

    public void AddWString(string key, string value)
    {
      var children = CurrentTable.Children;
      var configWstringNode = new BinaryConfigWStringNode(key)
      {
          Value = value
      };
      children.Add(configWstringNode);
    }

    public string GetString(string key) => GetNode<BinaryConfigStringNodeBase>(key).Value;

    public string GetStringAt(int index) => GetNodeAt<BinaryConfigStringNodeBase>(index).Value;

    public bool TryGetString(string key, out string value)
    {
        if (TryGetNode<BinaryConfigStringNodeBase>(key, out var node))
      {
        value = node.Value;
        return true;
      }
      value = null;
      return false;
    }

    public bool TryGetStringAt(int index, out string value)
    {
        if (TryGetNodeAt<BinaryConfigStringNodeBase>(index, out var node))
      {
        value = node.Value;
        return true;
      }
      value = null;
      return false;
    }

    public void AddTable(string key)
    {
      var binaryConfigTableNode = new BinaryConfigTableNode(key);
      CurrentTable.Children.Add(binaryConfigTableNode);
      m_stack.Push(binaryConfigTableNode);
    }

    public void AddOrderedTable(string key)
    {
      var orderedTableNode = new BinaryConfigOrderedTableNode(key);
      CurrentTable.Children.Add(orderedTableNode);
      m_stack.Push(orderedTableNode);
    }

    public void PushTable(string key) => m_stack.Push(GetNode<BinaryConfigTableNodeBase>(key));

    public void PushTableAt(int index) => m_stack.Push(GetNodeAt<BinaryConfigTableNodeBase>(index));

    public bool TryPushTable(string key)
    {
        if (!TryGetNode<BinaryConfigTableNodeBase>(key, out var node))
        return false;
      m_stack.Push(node);
      return true;
    }

    public bool TryPushTableAt(int index)
    {
        if (!TryGetNodeAt<BinaryConfigTableNodeBase>(index, out var node))
        return false;
      m_stack.Push(node);
      return true;
    }

    public void PopTable()
    {
      if (m_stack.Count == 1)
        throw new InvalidOperationException("Cannot pop root table.");
      m_stack.Pop();
    }

    public void Save(string fileName) => Save(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public void Save(Stream stream) => Save(stream, false);

    public void Save(Stream stream, bool leaveOpen)
    {
        using var binaryWriter = new BinaryWriter(stream, Chunky.Encoding, leaveOpen);
        m_root.Write(binaryWriter);
    }

    public static BinaryConfig Load(string fileName) => Load(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public static BinaryConfig Load(Stream stream) => Load(stream, false);

    public static BinaryConfig Load(Stream stream, bool leaveOpen) => Load(stream, leaveOpen, null);

    public static BinaryConfig Load(
      Stream stream,
      bool leaveOpen,
      KeyResolver keyResolver)
    {
      if (keyResolver == null)
        keyResolver = key => new DictionaryKey(key);
      using var binaryReader = new BinaryReader(stream, Chunky.Encoding, leaveOpen);
      var binaryConfig = new BinaryConfig();
      BinaryConfigTableNodeBase.ReadChildren(binaryReader, keyResolver, binaryConfig.m_root.Children);
      return binaryConfig;
    }

    private T GetNode<T>(string key) where T : BinaryConfigNode => GetNode<T>(new DictionaryKey(key));

    private T GetNode<T>(DictionaryKey key) where T : BinaryConfigNode
    {
      foreach (var child in CurrentTable.Children)
      {
        if (key == child.Key)
          return (T) child;
      }
      throw new KeyNotFoundException();
    }

    private T GetNodeAt<T>(int index) where T : BinaryConfigNode
    {
      var currentTable = CurrentTable;
      if (index >= 0 && index < currentTable.Children.Count)
        return (T) currentTable.Children[index];
      throw new ArgumentOutOfRangeException(nameof (index));
    }

    private bool TryGetNode<T>(string key, out T node) where T : BinaryConfigNode => TryGetNode<T>(new DictionaryKey(key), out node);

    private bool TryGetNode<T>(DictionaryKey key, out T node) where T : BinaryConfigNode
    {
      foreach (var child in CurrentTable.Children)
      {
        if (key == child.Key)
        {
          if (child is T obj)
          {
            node = obj;
            return true;
          }
          break;
        }
      }
      node = default;
      return false;
    }

    private bool TryGetNodeAt<T>(int index, out T node) where T : BinaryConfigNode
    {
      var currentTable = CurrentTable;
      if (index >= 0 && index < currentTable.Children.Count && currentTable.Children[index] is T child)
      {
        node = child;
        return true;
      }
      node = default;
      return false;
    }
  }
}
