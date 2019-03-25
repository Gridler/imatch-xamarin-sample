using System;

namespace iMatchSample
{
    public class InvokeHelper
    {
        public static Action<Action> Invoker;

        public static void Invoke(Action action)
        {
            Invoker?.Invoke(action);
        }
    }
}
