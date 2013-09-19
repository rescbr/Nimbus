using System;
namespace Nimbus.Plumbing
{
    public interface INimbusAppBus
    {

        INimbusDebugAutoAttach NimbusDebugAutoAttach { get; }
        NimbusSettings Settings { get; }
    }
}
