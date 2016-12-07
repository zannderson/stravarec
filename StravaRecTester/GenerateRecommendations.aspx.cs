using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StravaRec;
using System.Text;
using System.IO;

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
            Panel_Results.Visible = true;
            Repeater_Uphill.DataSource = stuff.Uphill;
            Repeater_Uphill.DataBind();
            Repeater_Downhill.DataSource = stuff.Downhill;
            Repeater_Downhill.DataBind();
            Repeater_Flat.DataSource = stuff.Flat;
            Repeater_Flat.DataBind();
            Repeater_Rolling.DataSource = stuff.UpAndDown;
            Repeater_Rolling.DataBind();
        }

        protected void Button_SubmitMyRankings_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            int index = 1;
            sb.AppendLine("Uphill:");
            foreach (RepeaterItem item in Repeater_Uphill.Items)
            {
                TextBox userRanking = (TextBox)item.FindControl("TextBox_UserRank");
                CheckBox keepOrNot = (CheckBox)item.FindControl("CheckBox_WouldYouPick");
                string ranking = "ERROR";
                string keep = "ERROR";
                if(userRanking != null)
                {
                    ranking = userRanking.Text;
                }
                if(keepOrNot != null)
                {
                    keep = keepOrNot.Checked.ToString();
                }
                sb.AppendLine(string.Format("{0}: keep: {1} ranking: {2}", index, keepOrNot, userRanking));
                index++;
            }
            sb.AppendLine("Downhill:");
            foreach (RepeaterItem item in Repeater_Uphill.Items)
            {
                TextBox userRanking = (TextBox)item.FindControl("TextBox_UserRank");
                CheckBox keepOrNot = (CheckBox)item.FindControl("CheckBox_WouldYouPick");
                string ranking = "ERROR";
                string keep = "ERROR";
                if (userRanking != null)
                {
                    ranking = userRanking.Text;
                }
                if (keepOrNot != null)
                {
                    keep = keepOrNot.Checked.ToString();
                }
                sb.AppendLine(string.Format("{0}: keep: {1} ranking: {2}", index, keepOrNot, userRanking));
                index++;
            }
            sb.AppendLine("Flat:");
            foreach (RepeaterItem item in Repeater_Uphill.Items)
            {
                TextBox userRanking = (TextBox)item.FindControl("TextBox_UserRank");
                CheckBox keepOrNot = (CheckBox)item.FindControl("CheckBox_WouldYouPick");
                string ranking = "ERROR";
                string keep = "ERROR";
                if (userRanking != null)
                {
                    ranking = userRanking.Text;
                }
                if (keepOrNot != null)
                {
                    keep = keepOrNot.Checked.ToString();
                }
                sb.AppendLine(string.Format("{0}: keep: {1} ranking: {2}", index, keepOrNot, userRanking));
                index++;
            }
            sb.AppendLine("UpAndDown:");
            foreach (RepeaterItem item in Repeater_Uphill.Items)
            {
                TextBox userRanking = (TextBox)item.FindControl("TextBox_UserRank");
                CheckBox keepOrNot = (CheckBox)item.FindControl("CheckBox_WouldYouPick");
                string ranking = "ERROR";
                string keep = "ERROR";
                if (userRanking != null)
                {
                    ranking = userRanking.Text;
                }
                if (keepOrNot != null)
                {
                    keep = keepOrNot.Checked.ToString();
                }
                sb.AppendLine(string.Format("{0}: keep: {1} ranking: {2}", index, keepOrNot, userRanking));
                index++;
            }



            string filename = DateTime.Now.Ticks.ToString();
            using (FileStream fs = new FileStream("C:/putfileshere/" + filename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sb.ToString());
                    sw.Flush();
                }
            }
        }
    }
}