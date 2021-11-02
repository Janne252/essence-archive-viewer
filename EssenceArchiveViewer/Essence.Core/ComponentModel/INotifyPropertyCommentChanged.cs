namespace Essence.Core.ComponentModel
{
  public interface INotifyPropertyCommentChanged
  {
    event PropertyCommentChangedEventHandler PropertyCommentChanged;

    string GetComment(string propertyName);

    void SetComment(string propertyName, string comment);
  }
}
