using Essence.Core.IO.Checksum;
using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public static class RelicGameData
  {
    public const string Extension = ".rgd";
    private static readonly FourCC AttributeEditorGameDataID = FourCC.Parse("AEGD");
    private const uint AttributeEditorGameDataVersion = 3;
    private static readonly FourCC KeysID = FourCC.Parse("KEYS");
    private const uint KeysVersion = 1;

    public static void Save(string fileName, BinaryConfig binaryConfig)
    {
      using (ChunkyWriter chunkyWriter = new ChunkyWriter(fileName))
      {
        chunkyWriter.PushDataChunk(AttributeEditorGameDataID, 3U, (string) null);
        using (MemoryStream memoryStream = new MemoryStream())
        {
          binaryConfig.Save((Stream) memoryStream, true);
          if (memoryStream.Length > (long) int.MaxValue)
            throw new IOException();
          CRC32 crC32 = new CRC32();
          crC32.Initialize();
          crC32.TransformFinalBlock(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
          chunkyWriter.Write(crC32.CRC32Hash);
          chunkyWriter.Write(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
        }
        chunkyWriter.PopChunk();
        chunkyWriter.PushDataChunk(KeysID, 1U, (string) null);
        HashSet<DictionaryKey> dictionaryKeySet = new HashSet<DictionaryKey>();
        GatherKeys(binaryConfig.CurrentTable, dictionaryKeySet);
        List<DictionaryKey> dictionaryKeyList = new List<DictionaryKey>((IEnumerable<DictionaryKey>) dictionaryKeySet);
        dictionaryKeyList.Sort();
        chunkyWriter.Write((uint) dictionaryKeyList.Count);
        foreach (DictionaryKey dictionaryKey in dictionaryKeySet)
        {
          chunkyWriter.Write(dictionaryKey.Hash);
          chunkyWriter.Write(dictionaryKey.String);
        }
        chunkyWriter.PopChunk();
      }

      static void GatherKeys(BinaryConfigTableNodeBase table, HashSet<DictionaryKey> keys)
      {
        foreach (BinaryConfigNode child in table.Children)
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
      using (ChunkyReader chunkyReader = new ChunkyReader(fileName))
      {
        ChunkHeader chunkHeader = chunkyReader.PushDataChunk(AttributeEditorGameDataID, 3U);
        int num = (int) chunkyReader.ReadUInt32();
        uint count = chunkHeader.Version > 2U ? checked (chunkHeader.Size - 4U) : chunkyReader.ReadUInt32();
        buffer = chunkyReader.ReadBytes((int) count);
        chunkyReader.PopChunk();
        chunkyReader.PushDataChunk(KeysID, 1U);
        uint capacity = chunkyReader.ReadUInt32();
        keys = new Dictionary<ulong, string>((int) capacity);
        for (uint index = 0; index < capacity; ++index)
        {
          ulong key = chunkyReader.ReadUInt64();
          string str = chunkyReader.ReadString();
          keys.Add(key, str);
        }
        chunkyReader.PopChunk();
      }
      using (MemoryStream memoryStream = new MemoryStream(buffer, false))
        return BinaryConfig.Load((Stream) memoryStream, false, (KeyResolver) (key => new DictionaryKey(key, keys[key])));
    }
  }
}
