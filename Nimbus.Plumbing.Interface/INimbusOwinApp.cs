using System;
namespace Nimbus.Plumbing.Interface
{
    public interface INimbusOwinApp
    {
        void Configuration(INimbusAppBus nimbusAppBus, Owin.IAppBuilder app);
    }
}
