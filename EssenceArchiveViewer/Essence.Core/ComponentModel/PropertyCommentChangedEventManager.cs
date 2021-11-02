using System;
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
          CurrentManager.AddListener(source, propertyName, listener, null);
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
          CurrentManager.RemoveListener(source, propertyName, listener, null);
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
          CurrentManager.AddListener(source, propertyName, null, handler);
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
          CurrentManager.RemoveListener(source, propertyName, null, handler);
          break;
      }
    }

    protected override ListenerList NewListenerList() => new ListenerList<PropertyCommentChangedEventArgs>();

    protected override void StartListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged += new PropertyCommentChangedEventHandler(OnPropertyCommentChanged);

    protected override void StopListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged -= new PropertyCommentChangedEventHandler(OnPropertyCommentChanged);

    protected override bool Purge(object source, object data, bool purgeAll)
    {
      var flag1 = false;
      if (!purgeAll)
      {
        var hybridDictionary = (HybridDictionary) data;
        var keys = hybridDictionary.Keys;
        var strArray = new string[keys.Count];
        keys.CopyTo(strArray, 0);
        for (var index = strArray.Length - 1; index >= 0; --index)
        {
          var flag2 = purgeAll || source == null;
          if (!flag2)
          {
            var list = (ListenerList) hybridDictionary[strArray[index]];
            if (ListenerList.PrepareForWriting(ref list))
              hybridDictionary[strArray[index]] = list;
            if (list.Purge())
              flag1 = true;
            flag2 = list.IsEmpty;
          }
          if (flag2)
            hybridDictionary.Remove(strArray[index]);
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
        var manager = (PropertyCommentChangedEventManager) GetCurrentManager(typeof (PropertyCommentChangedEventManager));
        if (manager == null)
        {
          manager = new PropertyCommentChangedEventManager();
          SetCurrentManager(typeof (PropertyCommentChangedEventManager), manager);
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
        var hybridDictionary = (HybridDictionary) this[source];
        if (hybridDictionary == null)
        {
          hybridDictionary = new HybridDictionary(true);
          this[source] = hybridDictionary;
          StartListening(source);
        }
        var list = (ListenerList) hybridDictionary[propertyName];
        if (list == null)
        {
          list = new ListenerList<PropertyCommentChangedEventArgs>();
          hybridDictionary[propertyName] = list;
        }
        if (ListenerList.PrepareForWriting(ref list))
          hybridDictionary[propertyName] = list;
        if (handler != null)
          list.AddHandler(handler);
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
        var hybridDictionary = (HybridDictionary) this[source];
        if (hybridDictionary == null)
          return;
        var list = (ListenerList) hybridDictionary[propertyName];
        if (list != null)
        {
          if (ListenerList.PrepareForWriting(ref list))
            hybridDictionary[propertyName] = list;
          if (handler != null)
            list.RemoveHandler(handler);
          else
            list.Remove(listener);
          if (list.IsEmpty)
            hybridDictionary.Remove(propertyName);
        }
        if (hybridDictionary.Count != 0)
          return;
        StopListening(source);
        Remove(source);
      }
    }

    private void OnPropertyCommentChanged(object sender, PropertyCommentChangedEventArgs args)
    {
      var list = (ListenerList) null;
      using (ReadLock)
      {
        var hybridDictionary = (HybridDictionary) this[sender];
        if (hybridDictionary != null)
          list = (ListenerList) hybridDictionary[args.PropertyName];
        if (list == null)
          list = ListenerList.Empty;
        list.BeginUse();
      }
      try
      {
        DeliverEventToList(sender, args, list);
      }
      finally
      {
        list.EndUse();
      }
    }
  }
}
