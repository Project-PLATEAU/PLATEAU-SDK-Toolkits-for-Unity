using System;

namespace PlateauToolkit
{
    public class CallbackDisposable : IDisposable
    {
        readonly Action m_OnDispose;

        public CallbackDisposable(Action onDispose)
        {
            m_OnDispose = onDispose;
        }

        public void Dispose()
        {
            m_OnDispose.Invoke();
        }
    }
}