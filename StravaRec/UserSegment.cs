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
        private int _segmentId;
        public int SegmentId
        {
            get
            {
                if(_segmentId == int.MinValue)
                {
                    if(Segment != null)
                    {
                        _segmentId = Segment.Id;
                    }
                }
                return _segmentId;
            }
        }

        public bool Starred { get; set; }

        public Segment Segment { get; set; }

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
            _segmentId = int.MinValue;
        }

        public int PlaceInLeaderboard { get; set; }

        public double LeaderboardPercentile { get; set; }

        public double SegmentWeight { get; set; }

        public double SimilarityScore { get; set; }
        
        private string _url;
        public string Url
        {
            get
            {
                if(_url == null)
                {
                    _url = string.Format("https://www.strava.com/segments/{0}", SegmentId);
                }
                return _url;
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                if(_name == null)
                {
                    if(Segment != null)
                    {
                        _name = Segment.Name;
                    }
                    else
                    {
                        _name = "Undefined";
                    }
                }
                return _name;
            }
        }
    }
}
