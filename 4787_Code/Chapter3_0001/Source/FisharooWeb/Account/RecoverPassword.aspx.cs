using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Fisharoo.FisharooCore.Core;
using Fisharoo.FisharooWeb.Account.Interface;
using Fisharoo.FisharooWeb.Account.Presenter;
using StructureMap;

namespace Fisharoo.FisharooWeb.Account
{
    public partial class RecoverPassword : System.Web.UI.Page, IRecoverPassword
    {
        private RecoverPasswordPresenter _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            _presenter = new RecoverPasswordPresenter();
            _presenter.Init(this);
        }

        protected void btnRecoverPassword_Click(object sender, EventArgs e)
        {
            _presenter.RecoverPassword(txtEmail.Text);
        }

        public void ShowMessage(string Message)
        {
            lblMessage.Text = Message;
        }

        public void ShowRecoverPasswordPanel(bool Value)
        {
            pnlRecoverPassword.Visible = Value;
        }
    }
}
