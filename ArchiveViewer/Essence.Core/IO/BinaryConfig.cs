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
      m_root = (BinaryConfigTableNodeBase) new BinaryConfigTableNode(new DictionaryKey());
      m_stack = new Stack<BinaryConfigTableNodeBase>();
      m_stack.Push(m_root);
    }

    public int Count => CurrentTable.Children.Count;

    public BinaryConfigTableNodeBase CurrentTable => m_stack.Peek();

    public void AddFloat(string key, float value) => CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigFloatNode(key)
    {
      Value = value
    });

    public float GetFloat(string key) => GetNode<BinaryConfigFloatNode>(key).Value;

    public float GetFloatAt(int index) => GetNodeAt<BinaryConfigFloatNode>(index).Value;

    public bool TryGetFloat(string key, out float value)
    {
      BinaryConfigFloatNode node;
      if (TryGetNode<BinaryConfigFloatNode>(key, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0.0f;
      return false;
    }

    public bool TryGetFloatAt(int index, out float value)
    {
      BinaryConfigFloatNode node;
      if (TryGetNodeAt<BinaryConfigFloatNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0.0f;
      return false;
    }

    public void AddInt(string key, int value) => CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigIntNode(key)
    {
      Value = value
    });

    public int GetInt(string key) => GetNode<BinaryConfigIntNode>(key).Value;

    public int GetIntAt(int index) => GetNodeAt<BinaryConfigIntNode>(index).Value;

    public bool TryGetInt(string key, out int value)
    {
      BinaryConfigIntNode node;
      if (TryGetNode<BinaryConfigIntNode>(key, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0;
      return false;
    }

    public bool TryGetIntAt(int index, out int value)
    {
      BinaryConfigIntNode node;
      if (TryGetNodeAt<BinaryConfigIntNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0;
      return false;
    }

    public void AddBool(string key, bool value) => CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigBoolNode(key)
    {
      Value = value
    });

    public bool GetBool(string key) => GetNode<BinaryConfigBoolNode>(key).Value;

    public bool GetBoolAt(int index) => GetNodeAt<BinaryConfigBoolNode>(index).Value;

    public bool TryGetBool(string key, out bool value)
    {
      BinaryConfigBoolNode node;
      if (TryGetNode<BinaryConfigBoolNode>(key, out node))
      {
        value = node.Value;
        return true;
      }
      value = false;
      return false;
    }

    public bool TryGetBoolAt(int index, out bool value)
    {
      BinaryConfigBoolNode node;
      if (TryGetNodeAt<BinaryConfigBoolNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = false;
      return false;
    }

    public void AddString(string key, string value)
    {
      List<BinaryConfigNode> children = CurrentTable.Children;
      BinaryConfigStringNode configStringNode = new BinaryConfigStringNode(key);
      configStringNode.Value = value;
      children.Add((BinaryConfigNode) configStringNode);
    }

    public void AddWString(string key, string value)
    {
      List<BinaryConfigNode> children = CurrentTable.Children;
      BinaryConfigWStringNode configWstringNode = new BinaryConfigWStringNode(key);
      configWstringNode.Value = value;
      children.Add((BinaryConfigNode) configWstringNode);
    }

    public string GetString(string key) => GetNode<BinaryConfigStringNodeBase>(key).Value;

    public string GetStringAt(int index) => GetNodeAt<BinaryConfigStringNodeBase>(index).Value;

    public bool TryGetString(string key, out string value)
    {
      BinaryConfigStringNodeBase node;
      if (TryGetNode<BinaryConfigStringNodeBase>(key, out node))
      {
        value = node.Value;
        return true;
      }
      value = (string) null;
      return false;
    }

    public bool TryGetStringAt(int index, out string value)
    {
      BinaryConfigStringNodeBase node;
      if (TryGetNodeAt<BinaryConfigStringNodeBase>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = (string) null;
      return false;
    }

    public void AddTable(string key)
    {
      BinaryConfigTableNode binaryConfigTableNode = new BinaryConfigTableNode(key);
      CurrentTable.Children.Add((BinaryConfigNode) binaryConfigTableNode);
      m_stack.Push((BinaryConfigTableNodeBase) binaryConfigTableNode);
    }

    public void AddOrderedTable(string key)
    {
      BinaryConfigOrderedTableNode orderedTableNode = new BinaryConfigOrderedTableNode(key);
      CurrentTable.Children.Add((BinaryConfigNode) orderedTableNode);
      m_stack.Push((BinaryConfigTableNodeBase) orderedTableNode);
    }

    public void PushTable(string key) => m_stack.Push(GetNode<BinaryConfigTableNodeBase>(key));

    public void PushTableAt(int index) => m_stack.Push(GetNodeAt<BinaryConfigTableNodeBase>(index));

    public bool TryPushTable(string key)
    {
      BinaryConfigTableNodeBase node;
      if (!TryGetNode<BinaryConfigTableNodeBase>(key, out node))
        return false;
      m_stack.Push(node);
      return true;
    }

    public bool TryPushTableAt(int index)
    {
      BinaryConfigTableNodeBase node;
      if (!TryGetNodeAt<BinaryConfigTableNodeBase>(index, out node))
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

    public void Save(string fileName) => Save((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public void Save(Stream stream) => Save(stream, false);

    public void Save(Stream stream, bool leaveOpen)
    {
      using (BinaryWriter binaryWriter = new BinaryWriter(stream, Chunky.Encoding, leaveOpen))
        m_root.Write(binaryWriter);
    }

    public static BinaryConfig Load(string fileName) => Load((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public static BinaryConfig Load(Stream stream) => Load(stream, false);

    public static BinaryConfig Load(Stream stream, bool leaveOpen) => Load(stream, leaveOpen, (KeyResolver) null);

    public static BinaryConfig Load(
      Stream stream,
      bool leaveOpen,
      KeyResolver keyResolver)
    {
      if (keyResolver == null)
        keyResolver = (KeyResolver) (key => new DictionaryKey(key));
      using (BinaryReader binaryReader = new BinaryReader(stream, Chunky.Encoding, leaveOpen))
      {
        BinaryConfig binaryConfig = new BinaryConfig();
        BinaryConfigTableNodeBase.ReadChildren(binaryReader, keyResolver, binaryConfig.m_root.Children);
        return binaryConfig;
      }
    }

    private T GetNode<T>(string key) where T : BinaryConfigNode => GetNode<T>(new DictionaryKey(key));

    private T GetNode<T>(DictionaryKey key) where T : BinaryConfigNode
    {
      foreach (BinaryConfigNode child in CurrentTable.Children)
      {
        if (key == child.Key)
          return (T) child;
      }
      throw new KeyNotFoundException();
    }

    private T GetNodeAt<T>(int index) where T : BinaryConfigNode
    {
      BinaryConfigTableNodeBase currentTable = CurrentTable;
      if (index >= 0 && index < currentTable.Children.Count)
        return (T) currentTable.Children[index];
      throw new ArgumentOutOfRangeException(nameof (index));
    }

    private bool TryGetNode<T>(string key, out T node) where T : BinaryConfigNode => TryGetNode<T>(new DictionaryKey(key), out node);

    private bool TryGetNode<T>(DictionaryKey key, out T node) where T : BinaryConfigNode
    {
      foreach (BinaryConfigNode child in CurrentTable.Children)
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
      node = default (T);
      return false;
    }

    private bool TryGetNodeAt<T>(int index, out T node) where T : BinaryConfigNode
    {
      BinaryConfigTableNodeBase currentTable = CurrentTable;
      if (index >= 0 && index < currentTable.Children.Count && currentTable.Children[index] is T child)
      {
        node = child;
        return true;
      }
      node = default (T);
      return false;
    }
  }
}
