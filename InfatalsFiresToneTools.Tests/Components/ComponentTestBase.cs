using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace InfatalsFirestoneTools.Tests.Components;

/// <summary>
/// Provides a base TestContext with a stub localizer that echoes the key as the value.
/// This avoids needing real resource files in the test project.
/// </summary>
public abstract class ComponentTestBase : BunitContext
{
    protected ComponentTestBase()
    {
        // Stub out IStringLocalizer so component markup can render
        _ = new        // Stub out IStringLocalizer so component markup can render
        StubLocalizer();
        _ = Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StubLocalizer<>));
        _ = Services.AddSingleton<IStringLocalizerFactory, StubLocalizerFactory>();
    }

    // Returns the resource key as the display value
    private sealed class StubLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments]
            => new(name, string.Format(name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }
    }

    private sealed class StubLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments]
            => new(name, string.Format(name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }
    }

    private sealed class StubLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource)
        {
            return new StubLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new StubLocalizer();
        }
    }
}