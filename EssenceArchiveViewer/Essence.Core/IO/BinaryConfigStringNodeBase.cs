namespace Essence.Core.IO
{
  public abstract class BinaryConfigStringNodeBase : BinaryConfigNode
  {
    public BinaryConfigStringNodeBase(string key)
      : base(key)
    {
    }

    internal BinaryConfigStringNodeBase(DictionaryKey key)
      : base(key)
    {
    }

    public string Value { get; set; }
  }
}
