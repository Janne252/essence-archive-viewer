// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.UCSFile
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class UCSFile
  {
    private readonly bool m_doNotEscape;

    public UCSFile()
      : this(false)
    {
    }

    public UCSFile(bool doNotEscape)
    {
      this.Database = new SortedDictionary<int, string>();
      this.m_doNotEscape = doNotEscape;
    }

    public int Add(string value)
    {
      int locID = 1;
      foreach (int key in this.Database.Keys)
      {
        if (key >= locID)
          locID = key + 1;
      }
      this.Set(locID, value);
      return locID;
    }

    public bool Remove(int key) => this.Database.Remove(key);

    public void Set(int locID, string text) => this.Database[locID] = text;

    public string Get(int locID)
    {
      string str;
      return this.Database.TryGetValue(locID, out str) ? str : (string) null;
    }

    public SortedDictionary<int, string> Database { get; }

    public void Read(string fileName)
    {
      this.Database.Clear();
      using (UCSReader ucsReader = new UCSReader(fileName, !this.m_doNotEscape))
      {
        foreach (KeyValuePair<int, string> keyValuePair in ucsReader.Read())
          this.Database[keyValuePair.Key] = keyValuePair.Value;
      }
    }

    public void Write(string fileName)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      using (UCSWriter ucsWriter = new UCSWriter(fileName, !this.m_doNotEscape))
      {
        foreach (KeyValuePair<int, string> keyValuePair in this.Database)
          ucsWriter.Write(keyValuePair.Key, keyValuePair.Value);
      }
    }
  }
}
