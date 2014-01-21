using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.Security;
using Nimbus.Plumbing;
using Nimbus.Model.ORM;

namespace Nimbus.Web.Security
{
    public class DatabaseLogin
    {
        IDbConnectionFactory _dbFactory;

        public DatabaseLogin()
        {
            _dbFactory = new OrmLiteConnectionFactory
                    (NimbusConfig.DatabaseConnection,
                    SqlServerDialect.Provider);
        }

        public DatabaseLogin(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public enum AuthenticationResult
        {
            Success,
            UserDoesNotExist,
            InvalidPassword,
            GenericFail
        }

        /// <summary>
        /// Autentica um par (email, senha) com o banco de dados.
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="password">Senha em texto claro</param>
        /// <param name="dbUser">Usuário autenticado</param>
        /// <returns></returns>
        public bool Authenticate(string email, string password, out NimbusPrincipal principal, out AuthenticationResult authDetails)
        {
            principal = null;
            
            using (var db = _dbFactory.OpenDbConnection())
            {
                var dbuser = db.Where<User>(u => u.Email == email).FirstOrDefault();
                if (dbuser == null)
                {
                    authDetails = AuthenticationResult.UserDoesNotExist;
                    return false; //Usuário não existe.
                }

                NSPHash hashedPassword = new NSPHash(dbuser.Password);
                PlaintextPassword ptPassword = new PlaintextPassword(password);

                if (hashedPassword.Equals(ptPassword))
                {
                    //preenche o NimbusUser
                    principal = GetNimbusPrincipal(dbuser);
                    authDetails = AuthenticationResult.InvalidPassword;
                    return true;
                }
                else
                {
                    authDetails = AuthenticationResult.InvalidPassword;
                    return false;
                }
            }

            authDetails = AuthenticationResult.GenericFail;
            return false;
        }


        /// <summary>
        /// Obtém NimbusUser a partir de um objeto DB.User.
        /// </summary>
        public static NimbusUser GetNimbusUser(User user)
        {
            return new NimbusUser()
                    {
                        IsAuthenticated = true,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        UserId = user.Id,
                        LastName = user.LastName,
                        AvatarUrl = user.AvatarUrl
                    };
        }
        /// <summary>
        /// Obtém NimbusPrincipal a partir de um objeto DB.User.
        /// </summary>
        public static NimbusPrincipal GetNimbusPrincipal(User user)
        {
            return new NimbusPrincipal(GetNimbusUser(user));
        }
        /// <summary>
        /// Obtém NimbusUser a partir de um UserId. Faz consulta ao BD.
        /// </summary>
        public NimbusUser GetNimbusUser(int userId)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                var user = db.Where<User>(u => u.Id == userId).FirstOrDefault();
                if (user == null) throw new Exception("Invalid User ID");
                return GetNimbusUser(user);
            }
        }
        /// <summary>
        /// Obtém NimbusPrincipal a partir de um UserId. Faz consulta ao BD.
        /// </summary>
        public NimbusPrincipal GetNimbusPrincipal(int userId)
        {
            return new NimbusPrincipal(GetNimbusUser(userId));
        }
    }
}