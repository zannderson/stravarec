using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strava.Api;
using Strava.Activities;
using Strava.Authentication;
using Strava.Athletes;
using Strava.Segments;
using Strava.Clients;

namespace StravaRec
{
    public class UserSegment
    {
        public int SegmentId { get; set; }

        public bool Starred { get; set; }

        public Segment Segment { get; set; }

		public ExplorerSegment ExplorerSegment { get; set; }

        public List<SegmentEffort> Efforts { get; set; }

        public Leaderboard SegmentLeaderboard { get; set; }

        public UserSegment(Leaderboard leaderboard, bool starred, params SegmentEffort[] efforts)
        {
            Starred = starred;
            SegmentLeaderboard = leaderboard;
			if(efforts != null && efforts.Length > 0)
			{
				Efforts = new List<SegmentEffort>(efforts);
				Segment = efforts[0].Segment;
			}
        }

        public int PlaceInLeaderboard { get; set; }

        public double LeaderboardPercentile { get; set; }

        public double SegmentWeight { get; set; }

        public double SimilarityScore { get; set; }
    }
}
