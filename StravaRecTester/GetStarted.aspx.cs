using Strava.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StravaRecTester
{
    public partial class GetStarted : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Start_Click(object sender, EventArgs e)
        {
            WebAuthentication wa = new WebAuthentication();
            wa.AuthCodeReceived += Wa_AuthCodeReceived;
            wa.AccessTokenReceived += Wa_AccessTokenReceived;
            wa.GetTokenAsync("2603", "b199b64f31661d8e53c960f6ccc16ce72b2fd821", Scope.ViewPrivate);
            //Response.Redirect("https://www.strava.com/oauth/authorize?client_id=2603&response_type=code&redirect_uri=http://www.zannderson.com&scope=view_private&approval_prompt=auto");
        }

        private void Wa_AccessTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Wa_AuthCodeReceived(object sender, AuthCodeReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}