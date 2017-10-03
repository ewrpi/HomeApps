using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Title = "Let's Make Some Picks!";
            if (!Page.IsPostBack && Request.QueryString["suggestName"] != null)
            {
                txtName.Text = Request.QueryString["suggestName"].ToString();
                txtEmailAddress.Focus();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string[] validationMessages = ValidateFields();

            if (validationMessages.Length == 0)
            {
                Authentication.RegisterNewUser(txtName.Text, txtPassword.Text, Request.ServerVariables["remote_addr"], txtEmailAddress.Text, txtBirthday.Text);

                if (HomeAppsLib.LibCommon.IsValidEmail(txtEmailAddress.Text))
                {
                    var data = HomeAppsLib.LibCommon.DBModel();
                    foreach (var subType in data.emailSubscriptionTypes.Where(x => x.autoEnroll))
                        HomeAppsLib.EmailSubscriptions.AddSubscription(subType.Id, Authentication.GetAuthenticatedUserName());
                }

                Common.SetLastVisitedWeek(Common.GetDefaultWeek(toMakePicks: true));
                Response.Redirect("~/NFLPicks.aspx");
            }

            else
                lblerror.Text = string.Join("<br>", validationMessages);
        }

        private string[] ValidateFields()
        {
            List<string> validationMessageList = new List<string>();

            if (txtName.Text.Length <= 1)
                validationMessageList.Add("Name must be greater than 1 character.");

            if (Authentication.CurrentUsersList(true).Contains(txtName.Text.ToLower()))
            {
                validationMessageList.Add("User with chosen name already exists! See current users below.");
                PopulateUserList();
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
                validationMessageList.Add("Password does not match password confirmation!!");

            if (txtBirthday.Text.Trim() != string.Empty && !ValidDate(txtBirthday.Text))
                validationMessageList.Add("Invalid date entered for birthday, try format MM/DD/YY");

            return validationMessageList.ToArray();
        }

        private bool ValidDate(string dateString)
        {
            try
            {
                Convert.ToDateTime(dateString);
                return true;
            }
            catch { return false; }
        }

        private void PopulateUserList()
        {
            lblCurrentUserList.Text = string.Join("<br>", Authentication.CurrentUsersList(false));
        }

    }
}