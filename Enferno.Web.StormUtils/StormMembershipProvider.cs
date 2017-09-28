
using System;
using System.Web.Security;
using System.Collections.Specialized;
using Enferno.Web.StormUtils.InternalRepository;
using Customers = Enferno.StormApiClient.Customers;
using System.ServiceModel;
using Enferno.StormApiClient.Expose.ErrorMessage_v2;

namespace Enferno.Web.StormUtils
{
    public class StormMembershipProvider : MembershipProvider
    {
        private readonly IRepository repository;

        public StormMembershipProvider()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public StormMembershipProvider(IRepository repository)
        {
            this.repository = repository;
        }

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset { get { return true; } }
        public override bool EnablePasswordRetrieval { get { return false; } }
        public override bool RequiresQuestionAndAnswer { get { return false; } }
        public override bool RequiresUniqueEmail { get { return true; } }

        public override MembershipPasswordFormat PasswordFormat { get { return MembershipPasswordFormat.Clear; } }

        public override int MaxInvalidPasswordAttempts { get { return 3; } }
        public override int PasswordAttemptWindow { get { return 5; } }
        public override int MinRequiredNonAlphanumericCharacters { get { return 0; } }
        public override int MinRequiredPasswordLength { get { return 6; } }
        public override string PasswordStrengthRegularExpression { get { return @"(?=.{6,})"; } }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");

            if (string.IsNullOrWhiteSpace(name)) name = "StormMembershipProvider";

            base.Initialize(name, config);

            ApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
        }

        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return (string.IsNullOrEmpty(configValue)) ? defaultValue : configValue;
        }

        public override bool ChangePassword(string loginName, string oldPwd, string newPwd)
        {
            if (!ValidateUser(loginName, oldPwd)) return false;

            var args = new ValidatePasswordEventArgs(loginName, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null) throw args.FailureInformation;
                throw new MembershipPasswordException("Change password cancelled due to new password validation failure.");
            }

            repository.ChangePassword(loginName, oldPwd, newPwd, newPwd);
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string loginName, string password, string newPwdQuestion, string newPwdAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string loginName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var u = GetUser(loginName, false);
            if (u == null)
            {
                var customer = new Customers.Customer
                {
                    Account = new Customers.Account
                    {
                        Name = loginName, LoginName = loginName
                    },
                    FirstName = loginName,
                    Email = email,
                };

                var createdCustomer = repository.CreateCustomer(customer);

                status = MembershipCreateStatus.Success;
                return GetUser(createdCustomer);
            }

            status = MembershipCreateStatus.DuplicateUserName;
            return null;
        }

        public override bool DeleteUser(string loginName, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string loginName, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string loginName, bool userIsOnline)
        {
            if (string.IsNullOrWhiteSpace(loginName)) return null;

            var customer =  repository.GetCustomerByLoginName(loginName);
            return GetUser(customer);
        }

        public override MembershipUser GetUser(object key, bool userIsOnline)
        {
            var customer = repository.GetCustomerByKey((Guid)key, StormContext.CultureCode);
            return GetUser(customer);
        }

        private MembershipUser GetUser(Customers.Customer customer)
        {
            return customer == null ? null : new MembershipUser(this.Name, customer.Email, customer.Key, customer.Email, null, null, true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.MinValue, DateTime.MinValue);
        }

        public override bool UnlockUser(string loginName)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";

            var customer = repository.GetCustomerByEmail(email);
            return customer != null && customer.Account != null ? customer.Account.LoginName : "";
        }
 
        public override string ResetPassword(string loginName, string answer)
        {
            if (string.IsNullOrWhiteSpace(loginName)) return null;

            try
            {
                repository.SendPasswordReminder(loginName);
                return "";
            }
            catch (FaultException<ErrorMessage> ex)
            {
                if (ex.Message == "Bad Request") return null;
                throw;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string loginName, string password)
        {
            if (string.IsNullOrWhiteSpace(loginName)) return false;

            try
            {
                var customer = repository.Login(loginName, password);
                if (customer != null) StormContext.LoginUser(loginName);
                return customer != null;
            }
            catch (FaultException<ErrorMessage> ex)
            {
                if (ex.Message == "Bad Request") return false;
                throw;
            }
        }

        public override MembershipUserCollection FindUsersByName(string loginNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByEmailInternal(loginNameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByEmailInternal(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        private MembershipUserCollection FindUsersByLoginNameInternal(string loginNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            if (string.IsNullOrWhiteSpace(loginNameToMatch)) return null;

            var retval = new MembershipUserCollection();
            var customer = repository.GetCustomerByLoginName(loginNameToMatch, StormContext.CultureCode);
            if (customer != null) retval.Add(GetUser(customer));
            totalRecords = retval.Count;
            return retval;
        }

        private MembershipUserCollection FindUsersByEmailInternal(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            if (string.IsNullOrWhiteSpace(emailToMatch)) return null;

            var retval = new MembershipUserCollection();
            var customer = repository.GetCustomerByEmail(emailToMatch, StormContext.CultureCode);
            if (customer != null) retval.Add(GetUser(customer));
            totalRecords = retval.Count;
            return retval;
        }
    }
}
