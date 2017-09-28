
using System;
using System.Linq;
using System.Web.Security;
using System.Collections.Specialized;
using Enferno.Web.StormUtils.InternalRepository;

namespace Enferno.Web.StormUtils
{
    public class StormRoleProvider : RoleProvider
    {
        private readonly IRepository repository;

        public override string ApplicationName { get; set; }

        public StormRoleProvider()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public StormRoleProvider(IRepository repository)
        {
            this.repository = repository;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");

            if (string.IsNullOrWhiteSpace(name)) name = "StormRoleProvider";
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                ApplicationName = config["applicationName"];
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string rolename)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            var application = repository.GetApplication();
            return application == null ? new string[0] : application.Authorizations.Select(a => a.Value).ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            var customer = repository.GetCustomerByEmail(username);
            return customer != null && customer.Account != null ? customer.Account.Authorizations.Select(a => a.Value).ToArray() : new string[0];
        }

        public override string[] GetUsersInRole(string rolename)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string rolename)
        {
            var customer = repository.GetCustomerByEmail(username);
            return customer != null && customer.Account != null && customer.Account.Authorizations.Exists(a => a.Value.Equals(rolename, StringComparison.InvariantCultureIgnoreCase));
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string rolename)
        {
            var application = repository.GetApplication();
            return application != null && application.Authorizations.Exists(a => a.Value.Equals(rolename, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
    }
}
