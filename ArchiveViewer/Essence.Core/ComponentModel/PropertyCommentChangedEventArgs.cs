using System;

namespace Essence.Core.ComponentModel
{
  public class PropertyCommentChangedEventArgs : EventArgs
  {
    public PropertyCommentChangedEventArgs(string propertyName) => PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));

    public string PropertyName { get; }
  }
}
