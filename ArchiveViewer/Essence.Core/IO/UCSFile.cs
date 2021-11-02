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
      Database = new SortedDictionary<int, string>();
      m_doNotEscape = doNotEscape;
    }

    public int Add(string value)
    {
      int locID = 1;
      foreach (int key in Database.Keys)
      {
        if (key >= locID)
          locID = key + 1;
      }
      Set(locID, value);
      return locID;
    }

    public bool Remove(int key) => Database.Remove(key);

    public void Set(int locID, string text) => Database[locID] = text;

    public string Get(int locID)
    {
      string str;
      return Database.TryGetValue(locID, out str) ? str : (string) null;
    }

    public SortedDictionary<int, string> Database { get; }

    public void Read(string fileName)
    {
      Database.Clear();
      using (UCSReader ucsReader = new UCSReader(fileName, !m_doNotEscape))
      {
        foreach (KeyValuePair<int, string> keyValuePair in ucsReader.Read())
          Database[keyValuePair.Key] = keyValuePair.Value;
      }
    }

    public void Write(string fileName)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      using (UCSWriter ucsWriter = new UCSWriter(fileName, !m_doNotEscape))
      {
        foreach (KeyValuePair<int, string> keyValuePair in Database)
          ucsWriter.Write(keyValuePair.Key, keyValuePair.Value);
      }
    }
  }
}
