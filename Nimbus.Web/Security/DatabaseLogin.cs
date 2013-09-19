using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.Security;

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
        public bool Authenticate(string email, string password, out DB.User dbUser)
        {
            dbUser = null;
            using (var db = _dbFactory.OpenDbConnection())
            {
                var user = db.Where<Nimbus.DB.User>(u => u.Email == email).FirstOrDefault();
                if(user == null) return false; //Usuário não existe.
                NSPHash hashedPassword = new NSPHash(user.Password);
                PlaintextPassword ptPassword = new PlaintextPassword(password);

                if (hashedPassword.Equals(ptPassword))
                {
                    dbUser = user;
                    return true;
                } 
            }

            return false;
        }
    }
}