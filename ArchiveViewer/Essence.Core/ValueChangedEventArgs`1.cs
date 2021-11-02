using System;

namespace Essence.Core
{
  public class ValueChangedEventArgs<T> : EventArgs
  {
    public ValueChangedEventArgs(T oldValue, T newValue)
    {
      OldValue = oldValue;
      NewValue = newValue;
    }

    public T OldValue { get; }

    public T NewValue { get; }
  }
}
