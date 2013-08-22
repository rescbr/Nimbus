using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public class AlertSendMessage
    {
        public string SuccessMessage{ get{return "Mensagem enviada com sucesso." ;} }
        public string ErrorMessage { get { return "Não foi possível enviar sua mensagem."; } } 
    }
        
    public class AlertDelete
    {
        public string SuccessMessage{ get{return "Item deletado com sucesso." ;} }
        public string AtentionMessage { get { return "Tem certeza que deseja deletar este item?"; } } 
    }

    public class AlertGeneral
    {
        public string SuccessMessage { get { return "Operação realizada com sucesso."; } }
        public string AtentionMessage { get { return "Tem certeza que deseja realizar está operação?"; } }
        public string ErrorMessage { get { return "Erro ao tentar realizar essa operação. Tente novamente."; } } 
    }
    
}