/* Implementação do DebugAutoAttach, baseado em
 * http://msdn.microsoft.com/en-us/library/ms230837.aspx
 * Sys.Web.HttpDebugHandler
 * webengine4.dll
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class NimbusDebugAutoAttach : Nimbus.Plumbing.Interface.INimbusDebugAutoAttach
    {
        private NimbusAppBus _nimbusAppBus;
        public NimbusDebugAutoAttach(NimbusAppBus nimbusAppBus)
        {
            _nimbusAppBus = nimbusAppBus;
        }
        
        public void DebugAutoAttach(string debugId)
        {
            if (_nimbusAppBus.Settings.IsDebug)
            {
                IDebugAutoAttach dbg = (IDebugAutoAttach)new DebugAutoAttach();
                dbg.AutoAttach(Guid.Empty,
                    Process.GetCurrentProcess().Id,
                    2, //AUTOATTACH_PROGRAM_UNKNOWN
                    0, //de acordo com o IDA
                    debugId);

            }
            else throw new Exception("Nimbus is not on Debug mode.");
        }
    }

    [ComImport]
    [Guid("e9958f1f-0a56-424a-a300-530ebb2e9865")]
    class DebugAutoAttach
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("e9958f1f-0a56-424a-a300-530ebb2e9865")]
    interface IDebugAutoAttach
    {

        void AutoAttach(
            Guid guidPort,
            int dwPid,
            int dwProgramType,
            int dwProgramId,
            [MarshalAs(UnmanagedType.LPWStr)] string pszSessionID
            );
    }

}
