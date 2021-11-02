// Decompiled with JetBrains decompiler
// Type: Essence.Core.RefCountedDisposable`1
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Threading;

namespace Essence.Core
{
  public sealed class RefCountedDisposable<T> where T : class, IDisposable
  {
    private T m_disposable;
    private int m_refCount;

    private RefCountedDisposable(T disposable) => this.m_disposable = disposable;

    public sealed class Ref : IDisposable
    {
      private RefCountedDisposable<T> m_refCountedDisposable;

      public static RefCountedDisposable<T>.Ref Create(T disposable) => new RefCountedDisposable<T>.Ref(new RefCountedDisposable<T>(disposable));

      private Ref(RefCountedDisposable<T> refCountedDisposable)
      {
        this.m_refCountedDisposable = refCountedDisposable;
        if (Interlocked.Increment(ref this.m_refCountedDisposable.m_refCount) < 0)
          throw new Exception();
      }

      public Ref(RefCountedDisposable<T>.Ref @ref)
        : this(@ref.m_refCountedDisposable)
      {
      }

      public bool IsDisposed => this.m_refCountedDisposable == null;

      public bool IsUnique => this.m_refCountedDisposable != null && this.m_refCountedDisposable.m_refCount == 1;

      public T Target => this.m_refCountedDisposable != null ? this.m_refCountedDisposable.m_disposable : throw new InvalidOperationException();

      public void Dispose()
      {
        if (this.m_refCountedDisposable == null)
          return;
        if (this.m_refCountedDisposable.m_refCount <= 0)
          throw new Exception();
        if (Interlocked.Decrement(ref this.m_refCountedDisposable.m_refCount) == 0)
        {
          T disposable = this.m_refCountedDisposable.m_disposable;
          this.m_refCountedDisposable.m_disposable = default (T);
          this.m_refCountedDisposable = (RefCountedDisposable<T>) null;
          disposable.Dispose();
        }
        else
          this.m_refCountedDisposable = (RefCountedDisposable<T>) null;
      }

      public bool TryGetTarget(out T target)
      {
        if (this.m_refCountedDisposable != null)
        {
          target = this.m_refCountedDisposable.m_disposable;
          return true;
        }
        target = default (T);
        return false;
      }
    }
  }
}
