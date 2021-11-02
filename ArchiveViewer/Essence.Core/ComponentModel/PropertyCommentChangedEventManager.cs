using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace Essence.Core.ComponentModel
{
  public class PropertyCommentChangedEventManager : WeakEventManager
  {
    private PropertyCommentChangedEventManager()
    {
    }

    public static void AddListener(
      INotifyPropertyCommentChanged source,
      string propertyName,
      IWeakEventListener listener)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      switch (propertyName)
      {
        case "":
          throw new ArgumentOutOfRangeException(nameof (propertyName));
        case null:
          throw new ArgumentNullException(nameof (propertyName));
        default:
          if (listener == null)
            throw new ArgumentNullException(nameof (listener));
          CurrentManager.AddListener(source, propertyName, listener, (EventHandler<PropertyCommentChangedEventArgs>) null);
          break;
      }
    }

    public static void RemoveListener(
      INotifyPropertyCommentChanged source,
      string propertyName,
      IWeakEventListener listener)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      switch (propertyName)
      {
        case "":
          throw new ArgumentOutOfRangeException(nameof (propertyName));
        case null:
          throw new ArgumentNullException(nameof (propertyName));
        default:
          if (listener == null)
            throw new ArgumentNullException(nameof (listener));
          CurrentManager.RemoveListener(source, propertyName, listener, (EventHandler<PropertyCommentChangedEventArgs>) null);
          break;
      }
    }

    public static void AddHandler(
      INotifyPropertyCommentChanged source,
      string propertyName,
      EventHandler<PropertyCommentChangedEventArgs> handler)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      switch (propertyName)
      {
        case "":
          throw new ArgumentOutOfRangeException(nameof (propertyName));
        case null:
          throw new ArgumentNullException(nameof (propertyName));
        default:
          if (handler == null)
            throw new ArgumentNullException(nameof (handler));
          CurrentManager.AddListener(source, propertyName, (IWeakEventListener) null, handler);
          break;
      }
    }

    public static void RemoveHandler(
      INotifyPropertyCommentChanged source,
      string propertyName,
      EventHandler<PropertyCommentChangedEventArgs> handler)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      switch (propertyName)
      {
        case "":
          throw new ArgumentOutOfRangeException(nameof (propertyName));
        case null:
          throw new ArgumentNullException(nameof (propertyName));
        default:
          if (handler == null)
            throw new ArgumentNullException(nameof (handler));
          CurrentManager.RemoveListener(source, propertyName, (IWeakEventListener) null, handler);
          break;
      }
    }

    protected override ListenerList NewListenerList() => (ListenerList) new ListenerList<PropertyCommentChangedEventArgs>();

    protected override void StartListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged += new PropertyCommentChangedEventHandler(OnPropertyCommentChanged);

    protected override void StopListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged -= new PropertyCommentChangedEventHandler(OnPropertyCommentChanged);

    protected override bool Purge(object source, object data, bool purgeAll)
    {
      bool flag1 = false;
      if (!purgeAll)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) data;
        ICollection keys = hybridDictionary.Keys;
        string[] strArray = new string[keys.Count];
        keys.CopyTo((Array) strArray, 0);
        for (int index = strArray.Length - 1; index >= 0; --index)
        {
          bool flag2 = purgeAll || source == null;
          if (!flag2)
          {
            ListenerList list = (ListenerList) hybridDictionary[(object) strArray[index]];
            if (ListenerList.PrepareForWriting(ref list))
              hybridDictionary[(object) strArray[index]] = (object) list;
            if (list.Purge())
              flag1 = true;
            flag2 = list.IsEmpty;
          }
          if (flag2)
            hybridDictionary.Remove((object) strArray[index]);
        }
        if (hybridDictionary.Count == 0)
        {
          purgeAll = true;
          if (source != null)
            Remove(source);
        }
      }
      if (purgeAll)
      {
        if (source != null)
          StopListening(source);
        flag1 = true;
      }
      return flag1;
    }

    private static PropertyCommentChangedEventManager CurrentManager
    {
      get
      {
        PropertyCommentChangedEventManager manager = (PropertyCommentChangedEventManager) GetCurrentManager(typeof (PropertyCommentChangedEventManager));
        if (manager == null)
        {
          manager = new PropertyCommentChangedEventManager();
          SetCurrentManager(typeof (PropertyCommentChangedEventManager), (WeakEventManager) manager);
        }
        return manager;
      }
    }

    private void AddListener(
      INotifyPropertyCommentChanged source,
      string propertyName,
      IWeakEventListener listener,
      EventHandler<PropertyCommentChangedEventArgs> handler)
    {
      using (WriteLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[(object) source];
        if (hybridDictionary == null)
        {
          hybridDictionary = new HybridDictionary(true);
          this[(object) source] = (object) hybridDictionary;
          StartListening((object) source);
        }
        ListenerList list = (ListenerList) hybridDictionary[(object) propertyName];
        if (list == null)
        {
          list = (ListenerList) new ListenerList<PropertyCommentChangedEventArgs>();
          hybridDictionary[(object) propertyName] = (object) list;
        }
        if (ListenerList.PrepareForWriting(ref list))
          hybridDictionary[(object) propertyName] = (object) list;
        if (handler != null)
          list.AddHandler((Delegate) handler);
        else
          list.Add(listener);
        ScheduleCleanup();
      }
    }

    private void RemoveListener(
      INotifyPropertyCommentChanged source,
      string propertyName,
      IWeakEventListener listener,
      EventHandler<PropertyCommentChangedEventArgs> handler)
    {
      using (WriteLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[(object) source];
        if (hybridDictionary == null)
          return;
        ListenerList list = (ListenerList) hybridDictionary[(object) propertyName];
        if (list != null)
        {
          if (ListenerList.PrepareForWriting(ref list))
            hybridDictionary[(object) propertyName] = (object) list;
          if (handler != null)
            list.RemoveHandler((Delegate) handler);
          else
            list.Remove(listener);
          if (list.IsEmpty)
            hybridDictionary.Remove((object) propertyName);
        }
        if (hybridDictionary.Count != 0)
          return;
        StopListening((object) source);
        Remove((object) source);
      }
    }

    private void OnPropertyCommentChanged(object sender, PropertyCommentChangedEventArgs args)
    {
      ListenerList list = (ListenerList) null;
      using (ReadLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[sender];
        if (hybridDictionary != null)
          list = (ListenerList) hybridDictionary[(object) args.PropertyName];
        if (list == null)
          list = ListenerList.Empty;
        list.BeginUse();
      }
      try
      {
        DeliverEventToList(sender, (EventArgs) args, list);
      }
      finally
      {
        list.EndUse();
      }
    }
  }
}
