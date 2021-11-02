using System.IO;

namespace Essence.Core.IO
{
  public abstract class BinaryConfigNode
  {
    public BinaryConfigNode(string key)
      : this(new DictionaryKey(key))
    {
    }

    internal BinaryConfigNode(DictionaryKey key) => Key = key;

    public DictionaryKey Key { get; set; }

    protected internal abstract uint GetNodeType();

    protected internal abstract long Write(BinaryWriter binaryWriter);

    protected static void WritePadding(BinaryWriter binaryWriter, int alignment)
    {
      long num1 = (long) (alignment - 1);
      long num2 = (long) alignment - (binaryWriter.BaseStream.Position & num1) & num1;
      for (long index = 0; index < num2; ++index)
        binaryWriter.Write((byte) 0);
    }
  }
}
