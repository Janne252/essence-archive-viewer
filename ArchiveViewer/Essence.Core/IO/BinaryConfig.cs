// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfig
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      this.m_root = (BinaryConfigTableNodeBase) new BinaryConfigTableNode(new DictionaryKey());
      this.m_stack = new Stack<BinaryConfigTableNodeBase>();
      this.m_stack.Push(this.m_root);
    }

    public int Count => this.CurrentTable.Children.Count;

    public BinaryConfigTableNodeBase CurrentTable => this.m_stack.Peek();

    public void AddFloat(string key, float value) => this.CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigFloatNode(key)
    {
      Value = value
    });

    public float GetFloat(string key) => this.GetNode<BinaryConfigFloatNode>(key).Value;

    public float GetFloatAt(int index) => this.GetNodeAt<BinaryConfigFloatNode>(index).Value;

    public bool TryGetFloat(string key, out float value)
    {
      BinaryConfigFloatNode node;
      if (this.TryGetNode<BinaryConfigFloatNode>(key, out node))
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
      if (this.TryGetNodeAt<BinaryConfigFloatNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0.0f;
      return false;
    }

    public void AddInt(string key, int value) => this.CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigIntNode(key)
    {
      Value = value
    });

    public int GetInt(string key) => this.GetNode<BinaryConfigIntNode>(key).Value;

    public int GetIntAt(int index) => this.GetNodeAt<BinaryConfigIntNode>(index).Value;

    public bool TryGetInt(string key, out int value)
    {
      BinaryConfigIntNode node;
      if (this.TryGetNode<BinaryConfigIntNode>(key, out node))
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
      if (this.TryGetNodeAt<BinaryConfigIntNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = 0;
      return false;
    }

    public void AddBool(string key, bool value) => this.CurrentTable.Children.Add((BinaryConfigNode) new BinaryConfigBoolNode(key)
    {
      Value = value
    });

    public bool GetBool(string key) => this.GetNode<BinaryConfigBoolNode>(key).Value;

    public bool GetBoolAt(int index) => this.GetNodeAt<BinaryConfigBoolNode>(index).Value;

    public bool TryGetBool(string key, out bool value)
    {
      BinaryConfigBoolNode node;
      if (this.TryGetNode<BinaryConfigBoolNode>(key, out node))
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
      if (this.TryGetNodeAt<BinaryConfigBoolNode>(index, out node))
      {
        value = node.Value;
        return true;
      }
      value = false;
      return false;
    }

    public void AddString(string key, string value)
    {
      List<BinaryConfigNode> children = this.CurrentTable.Children;
      BinaryConfigStringNode configStringNode = new BinaryConfigStringNode(key);
      configStringNode.Value = value;
      children.Add((BinaryConfigNode) configStringNode);
    }

    public void AddWString(string key, string value)
    {
      List<BinaryConfigNode> children = this.CurrentTable.Children;
      BinaryConfigWStringNode configWstringNode = new BinaryConfigWStringNode(key);
      configWstringNode.Value = value;
      children.Add((BinaryConfigNode) configWstringNode);
    }

    public string GetString(string key) => this.GetNode<BinaryConfigStringNodeBase>(key).Value;

    public string GetStringAt(int index) => this.GetNodeAt<BinaryConfigStringNodeBase>(index).Value;

    public bool TryGetString(string key, out string value)
    {
      BinaryConfigStringNodeBase node;
      if (this.TryGetNode<BinaryConfigStringNodeBase>(key, out node))
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
      if (this.TryGetNodeAt<BinaryConfigStringNodeBase>(index, out node))
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
      this.CurrentTable.Children.Add((BinaryConfigNode) binaryConfigTableNode);
      this.m_stack.Push((BinaryConfigTableNodeBase) binaryConfigTableNode);
    }

    public void AddOrderedTable(string key)
    {
      BinaryConfigOrderedTableNode orderedTableNode = new BinaryConfigOrderedTableNode(key);
      this.CurrentTable.Children.Add((BinaryConfigNode) orderedTableNode);
      this.m_stack.Push((BinaryConfigTableNodeBase) orderedTableNode);
    }

    public void PushTable(string key) => this.m_stack.Push(this.GetNode<BinaryConfigTableNodeBase>(key));

    public void PushTableAt(int index) => this.m_stack.Push(this.GetNodeAt<BinaryConfigTableNodeBase>(index));

    public bool TryPushTable(string key)
    {
      BinaryConfigTableNodeBase node;
      if (!this.TryGetNode<BinaryConfigTableNodeBase>(key, out node))
        return false;
      this.m_stack.Push(node);
      return true;
    }

    public bool TryPushTableAt(int index)
    {
      BinaryConfigTableNodeBase node;
      if (!this.TryGetNodeAt<BinaryConfigTableNodeBase>(index, out node))
        return false;
      this.m_stack.Push(node);
      return true;
    }

    public void PopTable()
    {
      if (this.m_stack.Count == 1)
        throw new InvalidOperationException("Cannot pop root table.");
      this.m_stack.Pop();
    }

    public void Save(string fileName) => this.Save((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public void Save(Stream stream) => this.Save(stream, false);

    public void Save(Stream stream, bool leaveOpen)
    {
      using (BinaryWriter binaryWriter = new BinaryWriter(stream, Chunky.Encoding, leaveOpen))
        this.m_root.Write(binaryWriter);
    }

    public static BinaryConfig Load(string fileName) => BinaryConfig.Load((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));

    public static BinaryConfig Load(Stream stream) => BinaryConfig.Load(stream, false);

    public static BinaryConfig Load(Stream stream, bool leaveOpen) => BinaryConfig.Load(stream, leaveOpen, (KeyResolver) null);

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

    private T GetNode<T>(string key) where T : BinaryConfigNode => this.GetNode<T>(new DictionaryKey(key));

    private T GetNode<T>(DictionaryKey key) where T : BinaryConfigNode
    {
      foreach (BinaryConfigNode child in this.CurrentTable.Children)
      {
        if (key == child.Key)
          return (T) child;
      }
      throw new KeyNotFoundException();
    }

    private T GetNodeAt<T>(int index) where T : BinaryConfigNode
    {
      BinaryConfigTableNodeBase currentTable = this.CurrentTable;
      if (index >= 0 && index < currentTable.Children.Count)
        return (T) currentTable.Children[index];
      throw new ArgumentOutOfRangeException(nameof (index));
    }

    private bool TryGetNode<T>(string key, out T node) where T : BinaryConfigNode => this.TryGetNode<T>(new DictionaryKey(key), out node);

    private bool TryGetNode<T>(DictionaryKey key, out T node) where T : BinaryConfigNode
    {
      foreach (BinaryConfigNode child in this.CurrentTable.Children)
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
      BinaryConfigTableNodeBase currentTable = this.CurrentTable;
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
