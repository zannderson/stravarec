using Strava.Athletes;
using Strava.Authentication;
using Strava.Clients;
using Strava.Activities;
using Strava.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StravaRec;

namespace StravaRecConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //GET DATA
            StaticAuthentication auth = new StaticAuthentication("34834781045370ca62b4b3fc6be384ffb868a1c0");
            AthleteClient athClient = new AthleteClient(auth);
            ActivityClient actClient = new ActivityClient(auth);
            SegmentClient segClient = new SegmentClient(auth);
            EffortClient effortClient = new EffortClient(auth);
            StatsClient statClient = new StatsClient(auth);
            StreamClient sClient = new StreamClient(auth);

			ActivityType currentType = ActivityType.Run;

            Athlete me = athClient.GetAthlete();
            //var mySegmentEfforts = segClient.GetRecords(me.Id.ToString());
            var summary = actClient.GetSummaryThisYear();
            var myActivities = actClient.GetActivitiesBefore(DateTime.Now, 6, 50);
			myActivities.RemoveAll(a => a.Type != currentType);
            Dictionary<int, UserSegment> mySegments = new Dictionary<int, UserSegment>();
            var myStarred = segClient.GetStarredSegments();
            foreach (var activity in myActivities)
            {
                Console.Out.WriteLine("Activity: {0}", activity.Name);
                Activity fullActivity = null;
                try
                {
                    fullActivity = actClient.GetActivity(activity.Id.ToString(), true);
                }
                catch (Exception ex)
                {
                    //This is for a class project. I'm going to swallow all the exceptions I want. Sorry not sorry. Get over it. Technically I'm doing _something_ with it anyway by logging it. So there.
                    Console.Out.WriteLine("Error getting full activity for {0}: {1}", activity.Name, ex.Message);
                }
                if (fullActivity != null)
                {
                    foreach (var segEffort in fullActivity.SegmentEfforts)
                    {
                        Console.Out.WriteLine("Effort: {0}", segEffort.Segment.Name);
                        int segId = segEffort.Segment.Id;
                        // looks like we probably need to query the leaderboards in order to determine where this person falls on this segment
                        // we can limit the number of queries by just querying once per segment, since we don't care about their other efforts...  
                        if (!mySegments.ContainsKey(segId))
                        {
                            Leaderboard leaderboard = null;
                            try
                            {
                                leaderboard = segClient.GetFullSegmentLeaderboard(segId.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine("Error getting leaderboard for {0}: {1}", segEffort.Segment.Name, ex.Message);
                            }
                            if(leaderboard != null)
                            {
                                bool starred = myStarred.Exists(s => s.Id == segId);
                                Console.Out.WriteLine("Adding segment {0}", segEffort.Segment.Name);
                                mySegments.Add(segId, new UserSegment(leaderboard, starred, segEffort));
                            }
                        }
                        else
                        {
                            mySegments[segId].Efforts.Add(segEffort);
                        }
                    }
                }
            }

            List<UserSegment> uphill = new List<UserSegment>();
            List<UserSegment> downhill = new List<UserSegment>();
            List<UserSegment> flat = new List<UserSegment>();
			List<UserSegment> upNDown = new List<UserSegment>();

            //DO STUFF WITH DATA
            foreach (var segRecord in mySegments)
            {
                var seg = segRecord.Value.Segment;
                if(seg != null)
                {
                    if(seg.AverageGrade >= 3.0)
                    {
                        uphill.Add(segRecord.Value);
                    }
                    else if(seg.AverageGrade <= -3.0)
                    {
                        downhill.Add(segRecord.Value);
                    }
                    else
                    {
						if (segRecord.Value.Segment.MaxElevation - segRecord.Value.Segment.MinElevation >= 50 ||
							(segRecord.Value.Segment.MaxElevation - segRecord.Value.Segment.MinElevation) / segRecord.Value.Segment.Distance > 0.03)
						{
							upNDown.Add(segRecord.Value);
						}
						else
						{
							flat.Add(segRecord.Value);
						}
                    }
                }
            }

            //this is just echo debugging to see if segments are being properly categorized
			ShowMeTheMoney(uphill, "Uphill");
			ShowMeTheMoney(downhill, "Downhill");
			ShowMeTheMoney(flat, "Flat");
			ShowMeTheMoney(upNDown, "Up and Down");

            //compute average percentile within each group
            double uphillAveragePercentile = 0.0;
            double downhillAveragePercentile = 0.0;
            double flatAveragePercentile = 0.0;
            double upDownAveragePercentile = 0.0;
			DoStuffToStuff(flat, me.Id, ref flatAveragePercentile);
			DoStuffToStuff(uphill, me.Id, ref uphillAveragePercentile);
			DoStuffToStuff(downhill, me.Id, ref downhillAveragePercentile);
            DoStuffToStuff(upNDown, me.Id, ref upDownAveragePercentile);

			//precalculate weights so they can be normalized
			double lowestFlatWeight = 0.0;
			double lowestUpWeight = 0.0;
			double lowestDownWeight = 0.0;
            double lowestUpDownWeight = 0.0;
            CalculateWeights(flat, flatAveragePercentile, ref lowestFlatWeight);
            CalculateWeights(uphill, uphillAveragePercentile, ref lowestUpWeight);
            CalculateWeights(downhill, downhillAveragePercentile, ref lowestDownWeight);
            CalculateWeights(upNDown, upDownAveragePercentile, ref lowestUpDownWeight);
			lowestFlatWeight = Math.Abs(lowestFlatWeight);
			lowestUpWeight = Math.Abs(lowestUpWeight);
			lowestDownWeight = Math.Abs(lowestDownWeight);
            lowestUpDownWeight = Math.Abs(lowestUpDownWeight);

            //do the bulk of it, weights can be normalized here
            double[] uphillVec = new double[] { 0.0, 0.0 };
            double[] downhillVec = new double[] { 0.0, 0.0 };
            double[] flatVec = new double[] { 0.0, 0.0 };
            double[] upDownVec = new double[] { 0.0, 0.0 };
            BuildVectors(flat, lowestFlatWeight, ref flatVec);
            BuildVectors(uphill, lowestUpWeight, ref uphillVec);
            BuildVectors(downhill, lowestDownWeight, ref downhillVec);
            BuildVectors(upNDown, lowestUpDownWeight, ref upDownVec);

            Console.Out.WriteLine("Time to find some segments!!!");
        }

		private static int GetMyPlace(long id, Leaderboard leaderboard)
		{
			int place = 1;
			foreach (var entry in leaderboard.Entries)
			{
				if(entry.AthleteId == id)
				{
					return place;
				}
				place++;
			}
			return -1;
		}

		private static void DoStuffToStuff(List<UserSegment> segmentList, long myId, ref double average)
		{
			foreach (var segment in segmentList)
			{
				int place = GetMyPlace(myId, segment.SegmentLeaderboard);
				segment.PlaceInLeaderboard = place;
				var percentile = 1.0 - ((double)place / segment.SegmentLeaderboard.EntryCount);
				segment.LeaderboardPercentile = percentile;
				average += percentile / segmentList.Count;
			}
		}

		private static void ShowMeTheMoney(List<UserSegment> segs, string title)
		{
			Console.Out.WriteLine("$$$$$$$$$$$$${0}$$$$$$$$$$$$$", title);
			foreach (var segment in segs)
			{
				Console.Out.WriteLine(segment.Segment.Name);
			}
		}

        private static void CalculateWeights(List<UserSegment> segments, double average, ref double lowest)
        {
            Console.Out.WriteLine("Calculating weights for {0}", segments);
            foreach (var segment in segments)
            {
                segment.SegmentWeight = segment.LeaderboardPercentile - average;
                Console.Out.WriteLine("Segment: {0}, weight: {1}", segment.Segment.Name, segment.SegmentWeight);
                if (segment.SegmentWeight < lowest)
                {
                    lowest = segment.SegmentWeight;
                }
            }
        }

        private static void BuildVectors(List<UserSegment> segments, double lowestWeight, ref double[] vector)
        {
            foreach (var downSeg in segments)
            {
                downSeg.SegmentWeight = downSeg.SegmentWeight + lowestWeight;
                int starred = downSeg.Starred ? 1 : 0;
                double weighting = (downSeg.SegmentWeight + starred) / segments.Count;
                vector[0] += downSeg.Segment.Distance * weighting;
                vector[1] += downSeg.Segment.AverageGrade * weighting;
            }
        }
    }
}
