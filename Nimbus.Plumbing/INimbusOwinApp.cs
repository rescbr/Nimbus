using System;
namespace Nimbus.Plumbing
{
    public interface INimbusOwinApp
    {
        void Configuration(Owin.IAppBuilder app);
    }
}
