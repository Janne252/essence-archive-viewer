namespace Essence.Core.ComponentModel
{
  public interface ICustomType
  {
    string FullName { get; }

    string Name { get; }

    string DisplayName { get; }

    string Description { get; }

    bool IsSubclassOf(ICustomType customType);
  }
}
