using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Essence.Core.ViewModels
{
  public abstract class ViewModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
      var propertyChanged = PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
