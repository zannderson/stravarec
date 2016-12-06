using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StravaRec;

namespace StravaRecTester
{
    public partial class GenerateRecommendations : System.Web.UI.Page
    {

        // http://www.zannderson.com/?state=&code=1a912070714048a232915595884079ed198319c9
        private static string _code;

        protected void Page_Load(object sender, EventArgs e)
        {
            string code = Request.Params["code"];
            if(code != null)
            {
                if (code == "34834781045370ca62b4b3fc6be384ffb868a1c0")
                {
                    //this means testing. go for it...maybe don't do anything different here?
                }

                _code = code;
            }
            else
            {
                throw new Exception("BLOW UP");
            }

            //Recommender.DoTheThing(_code);

            RegisterAsyncTask(new PageAsyncTask(GetRecommendations));

            //We should maybe check here to see if we've already generated something for them and then redirect...
        }

        protected void DoIt_Click(object sender, EventArgs e)
        {
            DoIt.Visible = false;
            Literal_KeepWaiting.Visible = true;
            //Recommender.DoTheThing(_code);

            //Task t = new Task(() => {
            //    var recs = Recommender.DoTheThing(_code);
            //    Console.Out.WriteLine("Dude");
            //    Response.Redirect("http://www.google.com/?booger=yes");
            //    });
            //t.Start();
            //t.Wait();
        }

        private async Task GetRecommendations()
        {
            var stuff = await Recommender.DoTheThingAsync(_code);
            DoIt.Visible = false;
            Literal_KeepWaiting.Visible = false;
        }
    }
}