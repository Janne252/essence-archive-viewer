using System;
using System.Threading;

namespace Essence.Core
{
  public sealed class RefCountedDisposable<T> where T : class, IDisposable
  {
    private T m_disposable;
    private int m_refCount;

    private RefCountedDisposable(T disposable) => m_disposable = disposable;

    public sealed class Ref : IDisposable
    {
      private RefCountedDisposable<T> m_refCountedDisposable;

      public static Ref Create(T disposable) => new(new RefCountedDisposable<T>(disposable));

      private Ref(RefCountedDisposable<T> refCountedDisposable)
      {
        m_refCountedDisposable = refCountedDisposable;
        if (Interlocked.Increment(ref m_refCountedDisposable.m_refCount) < 0)
          throw new Exception();
      }

      public Ref(Ref @ref)
        : this(@ref.m_refCountedDisposable)
      {
      }

      public bool IsDisposed => m_refCountedDisposable == null;

      public bool IsUnique => m_refCountedDisposable is {m_refCount: 1};

      public T Target => m_refCountedDisposable != null ? m_refCountedDisposable.m_disposable : throw new InvalidOperationException();

      public void Dispose()
      {
        if (m_refCountedDisposable == null)
          return;
        if (m_refCountedDisposable.m_refCount <= 0)
          throw new Exception();
        if (Interlocked.Decrement(ref m_refCountedDisposable.m_refCount) == 0)
        {
          var disposable = m_refCountedDisposable.m_disposable;
          m_refCountedDisposable.m_disposable = default;
          m_refCountedDisposable = null;
          disposable.Dispose();
        }
        else
          m_refCountedDisposable = null;
      }

      public bool TryGetTarget(out T target)
      {
        if (m_refCountedDisposable != null)
        {
          target = m_refCountedDisposable.m_disposable;
          return true;
        }
        target = default;
        return false;
      }
    }
  }
}
