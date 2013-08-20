using System;
namespace Nimbus.Plumbing.Interface
{
    public interface INimbusAppBus
    {

        INimbusDebugAutoAttach NimbusDebugAutoAttach { get; }
        NimbusSettings Settings { get; }
    }
}
