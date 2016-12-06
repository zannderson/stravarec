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
            Response.Redirect("https://www.strava.com/oauth/authorize?client_id=2603&response_type=code&redirect_uri=http://www.zannderson.com&scope=view_private&approval_prompt=auto");
        }
    }
}