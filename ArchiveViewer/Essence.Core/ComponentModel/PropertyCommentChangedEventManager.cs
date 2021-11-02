// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.PropertyCommentChangedEventManager
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
          PropertyCommentChangedEventManager.CurrentManager.AddListener(source, propertyName, listener, (EventHandler<PropertyCommentChangedEventArgs>) null);
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
          PropertyCommentChangedEventManager.CurrentManager.RemoveListener(source, propertyName, listener, (EventHandler<PropertyCommentChangedEventArgs>) null);
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
          PropertyCommentChangedEventManager.CurrentManager.AddListener(source, propertyName, (IWeakEventListener) null, handler);
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
          PropertyCommentChangedEventManager.CurrentManager.RemoveListener(source, propertyName, (IWeakEventListener) null, handler);
          break;
      }
    }

    protected override WeakEventManager.ListenerList NewListenerList() => (WeakEventManager.ListenerList) new WeakEventManager.ListenerList<PropertyCommentChangedEventArgs>();

    protected override void StartListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged += new PropertyCommentChangedEventHandler(this.OnPropertyCommentChanged);

    protected override void StopListening(object source) => ((INotifyPropertyCommentChanged) source).PropertyCommentChanged -= new PropertyCommentChangedEventHandler(this.OnPropertyCommentChanged);

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
            WeakEventManager.ListenerList list = (WeakEventManager.ListenerList) hybridDictionary[(object) strArray[index]];
            if (WeakEventManager.ListenerList.PrepareForWriting(ref list))
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
            this.Remove(source);
        }
      }
      if (purgeAll)
      {
        if (source != null)
          this.StopListening(source);
        flag1 = true;
      }
      return flag1;
    }

    private static PropertyCommentChangedEventManager CurrentManager
    {
      get
      {
        PropertyCommentChangedEventManager manager = (PropertyCommentChangedEventManager) WeakEventManager.GetCurrentManager(typeof (PropertyCommentChangedEventManager));
        if (manager == null)
        {
          manager = new PropertyCommentChangedEventManager();
          WeakEventManager.SetCurrentManager(typeof (PropertyCommentChangedEventManager), (WeakEventManager) manager);
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
      using (this.WriteLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[(object) source];
        if (hybridDictionary == null)
        {
          hybridDictionary = new HybridDictionary(true);
          this[(object) source] = (object) hybridDictionary;
          this.StartListening((object) source);
        }
        WeakEventManager.ListenerList list = (WeakEventManager.ListenerList) hybridDictionary[(object) propertyName];
        if (list == null)
        {
          list = (WeakEventManager.ListenerList) new WeakEventManager.ListenerList<PropertyCommentChangedEventArgs>();
          hybridDictionary[(object) propertyName] = (object) list;
        }
        if (WeakEventManager.ListenerList.PrepareForWriting(ref list))
          hybridDictionary[(object) propertyName] = (object) list;
        if (handler != null)
          list.AddHandler((Delegate) handler);
        else
          list.Add(listener);
        this.ScheduleCleanup();
      }
    }

    private void RemoveListener(
      INotifyPropertyCommentChanged source,
      string propertyName,
      IWeakEventListener listener,
      EventHandler<PropertyCommentChangedEventArgs> handler)
    {
      using (this.WriteLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[(object) source];
        if (hybridDictionary == null)
          return;
        WeakEventManager.ListenerList list = (WeakEventManager.ListenerList) hybridDictionary[(object) propertyName];
        if (list != null)
        {
          if (WeakEventManager.ListenerList.PrepareForWriting(ref list))
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
        this.StopListening((object) source);
        this.Remove((object) source);
      }
    }

    private void OnPropertyCommentChanged(object sender, PropertyCommentChangedEventArgs args)
    {
      WeakEventManager.ListenerList list = (WeakEventManager.ListenerList) null;
      using (this.ReadLock)
      {
        HybridDictionary hybridDictionary = (HybridDictionary) this[sender];
        if (hybridDictionary != null)
          list = (WeakEventManager.ListenerList) hybridDictionary[(object) args.PropertyName];
        if (list == null)
          list = WeakEventManager.ListenerList.Empty;
        list.BeginUse();
      }
      try
      {
        this.DeliverEventToList(sender, (EventArgs) args, list);
      }
      finally
      {
        list.EndUse();
      }
    }
  }
}
