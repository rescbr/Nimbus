using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.Security;
using Nimbus.Plumbing;
using Nimbus.DB.ORM;

namespace Nimbus.Web.Security
{
    public class DatabaseLogin
    {
        IDbConnectionFactory _dbFactory;

        public DatabaseLogin(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Autentica um par (email, senha) com o banco de dados.
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="password">Senha em texto claro</param>
        /// <param name="dbUser">Usuário autenticado</param>
        /// <returns></returns>
        public bool Authenticate(string email, string password, out NimbusPrincipal principal)
        {
            principal = null;
            using (var db = _dbFactory.OpenDbConnection())
            {
                var dbuser = db.Where<User>(u => u.Email == email).FirstOrDefault();
                if(dbuser == null) return false; //Usuário não existe.
                NSPHash hashedPassword = new NSPHash(dbuser.Password);
                PlaintextPassword ptPassword = new PlaintextPassword(password);

                if (hashedPassword.Equals(ptPassword))
                {
                    //preenche o NimbusUser
                    principal = new NimbusPrincipal(new NimbusUser()
                    {
                        IsAuthenticated = true,
                        AvatarUrl = dbuser.AvatarUrl,
                        Email = dbuser.Email,
                        FirstName = dbuser.FirstName,
                        UserId = dbuser.Id,
                        LastName = dbuser.LastName,
                       // TOTPKey = dbuser.TOTPKey
                    });
                    return true;
                } 
            }

            return false;
        }
    }
}