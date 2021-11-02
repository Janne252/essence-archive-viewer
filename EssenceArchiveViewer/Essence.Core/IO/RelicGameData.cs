using Essence.Core.IO.Checksum;
using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public static class RelicGameData
  { private static readonly FourCC AttributeEditorGameDataID = FourCC.Parse("AEGD");
    private static readonly FourCC KeysID = FourCC.Parse("KEYS");

    public static void Save(string fileName, BinaryConfig binaryConfig)
    {
        using var chunkyWriter = new ChunkyWriter(fileName);
        chunkyWriter.PushDataChunk(AttributeEditorGameDataID, 3U, null);
        using (var memoryStream = new MemoryStream())
        {
            binaryConfig.Save(memoryStream, true);
            if (memoryStream.Length > int.MaxValue)
                throw new IOException();
            var crC32 = new CRC32();
            crC32.Initialize();
            crC32.TransformFinalBlock(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
            chunkyWriter.Write(crC32.CRC32Hash);
            chunkyWriter.Write(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
        }
        chunkyWriter.PopChunk();
        chunkyWriter.PushDataChunk(KeysID, 1U, null);
        var dictionaryKeySet = new HashSet<DictionaryKey>();
        GatherKeys(binaryConfig.CurrentTable, dictionaryKeySet);
        var dictionaryKeyList = new List<DictionaryKey>(dictionaryKeySet);
        dictionaryKeyList.Sort();
        chunkyWriter.Write((uint) dictionaryKeyList.Count);
        foreach (var dictionaryKey in dictionaryKeySet)
        {
            chunkyWriter.Write(dictionaryKey.Hash);
            chunkyWriter.Write(dictionaryKey.String);
        }
        chunkyWriter.PopChunk();

        static void GatherKeys(BinaryConfigTableNodeBase table, HashSet<DictionaryKey> keys)
      {
        foreach (var child in table.Children)
        {
          if (child.Key.String != null)
            keys.Add(child.Key);
          if (child is BinaryConfigTableNodeBase table2)
            GatherKeys(table2, keys);
        }
      }
    }

    public static BinaryConfig Load(string fileName)
    {
      byte[] buffer;
      Dictionary<ulong, string> keys;
      using (var chunkyReader = new ChunkyReader(fileName))
      {
        var chunkHeader = chunkyReader.PushDataChunk(AttributeEditorGameDataID, 3U);
        _ = (int) chunkyReader.ReadUInt32();
        var count = chunkHeader.Version > 2U ? checked (chunkHeader.Size - 4U) : chunkyReader.ReadUInt32();
        buffer = chunkyReader.ReadBytes((int) count);
        chunkyReader.PopChunk();
        chunkyReader.PushDataChunk(KeysID, 1U);
        var capacity = chunkyReader.ReadUInt32();
        keys = new Dictionary<ulong, string>((int) capacity);
        for (uint index = 0; index < capacity; ++index)
        {
          var key = chunkyReader.ReadUInt64();
          var str = chunkyReader.ReadString();
          keys.Add(key, str);
        }
        chunkyReader.PopChunk();
      }
      using var memoryStream = new MemoryStream(buffer, false);
      return BinaryConfig.Load(memoryStream, false, key => new DictionaryKey(key, keys[key]));
        }
  }
}
