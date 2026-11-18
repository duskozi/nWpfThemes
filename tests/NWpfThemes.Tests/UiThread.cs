using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NWpfThemes.Tests;

/// <summary>
/// One STA thread that owns the WPF <see cref="Application"/> for the whole test run. WPF objects
/// must be created and used on the thread that owns them, so every test runs its body here.
/// </summary>
internal static class UiThread
{
    private static readonly Lazy<Dispatcher> s_dispatcher = new(Start, LazyThreadSafetyMode.ExecutionAndPublication);

    public static void Run(Action action) => s_dispatcher.Value.Invoke(action);

    public static T Run<T>(Func<T> func) => s_dispatcher.Value.Invoke(func);

    /// <summary>Lets bindings, templates and layout settle.</summary>
    public static void DoEvents() =>
        Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.ContextIdle);

    /// <summary>Gives the application a fresh set of resources: one theme plus the control styles.</summary>
    public static void ResetApplicationResources(Theme theme)
    {
        Application.Current.Resources = new ThemeResources { Theme = theme.Name };
    }

    private static Dispatcher Start()
    {
        Dispatcher? dispatcher = null;
        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            dispatcher = Dispatcher.CurrentDispatcher;
            ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "WPF test thread",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        return dispatcher!;
    }
}
