using Mandrill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [AllowAnonymous]
    public class NimbusController:NimbusWebController
    {
        [HttpGet]
        public ActionResult Faq()
        {
            return View("faq"); 
        }

        [HttpGet]
        public ActionResult FaleConosco()
        {
            return View("faleconosco");
        }

        [HttpGet]
        public ActionResult QuemSomos()
        {
            return View("quemsomos");
        }

        [HttpGet]
        public ActionResult Novidades()
        {
            return View("novidades");
        }

        [HttpGet]
        public ActionResult Termosdeuso()
        {
            return View("termosdeuso");
        }


        [HttpGet]
        public ActionResult PageError404()
        {
            return View("pageerror404");
        }

        [HttpPost]
        public ActionResult SendFaleConosco() 
        {
            MandrillApi mandril = new MandrillApi(NimbusConfig.MandrillToken);
            EmailMessage mensagem = new EmailMessage();
            List<EmailAddress> address = new List<EmailAddress>();
              
            
            try
            {
                mensagem.from_email = "faleconosco@portalnimbus.com.br";
                mensagem.from_name = "Fale conosco";
                mensagem.subject = "[" + Request.Form["slcFaleConosco"] + "]";
                mensagem.text = "Usuário: " + Request.Form["iptNameFaleConosco"] + " " + Request.Form["iptLastNameFaleConosco"] + " \n" +
                                "E-mail: " + Request.Form["iptEmailFaleConosco"] + "\n" +
                                "Tipo: " + Request.Form["slcFaleConosco"] + " \n" +
                                "Mensagem: " + Request.Form["txtaMsgFaleConosco"] + "\n\n\n\n";

                address.Add(new EmailAddress("contato@portalnimbus.com.br"));
                mensagem.to = address;

                var result = mandril.SendMessage(mensagem);
                if (result[0].Status == EmailResultStatus.Sent)
                {                    
                    return Redirect("/login");
                }
                else
                    {

                        return Redirect("/login"); //tem q arrumar
                    }
            }
            catch (Exception ex)
            {                
                throw;
            }
        }
        
        
    }
}