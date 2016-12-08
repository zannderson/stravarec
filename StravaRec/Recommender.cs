using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strava.Athletes;
using Strava.Authentication;
using Strava.Clients;
using Strava.Common;
using Strava.Activities;
using Strava.Segments;


namespace StravaRec
{
    public class Recommender
    {
        private const int MAX_SEARCH_EXTENT = 5000; //5k in each direction gives a 10k x 10k square. Seems reasonable.
        private const int TOP_N = 5;

        private static double _metersPerDegreeLongitude = 0;
        private static double _metersPerDegreeLatitude = 0;

        public static async Task<StravaRecommendations> DoTheThingAsync(string code)
        {
            //GET DATA
            StaticAuthentication auth = new StaticAuthentication(code);
            AthleteClient athClient = new AthleteClient(auth);
            ActivityClient actClient = new ActivityClient(auth);
            SegmentClient segClient = new SegmentClient(auth);
            EffortClient effortClient = new EffortClient(auth);
            StatsClient statClient = new StatsClient(auth);
            StreamClient sClient = new StreamClient(auth);

            ActivityType currentType = ActivityType.Run;

            Athlete me = await athClient.GetAthleteAsync();
            //var mySegmentEfforts = segClient.GetRecords(me.Id.ToString());
            //var summary = await actClient.GetSummaryThisYearAsync();
            var myActivities = await actClient.GetActivitiesBeforeAsync(DateTime.Now, 1, 100);
            myActivities.RemoveAll(a => a.Type != currentType);
            Dictionary<int, UserSegment> mySegments = new Dictionary<int, UserSegment>();
            var myStarred = await segClient.GetStarredSegmentsAsync();
            
            List<Task<Activity>> getActivities = new List<Task<Activity>>();
            foreach (var activity in myActivities)
            {
                getActivities.Add(actClient.GetActivityAsync(activity.Id.ToString(), true));
            }

            try
            {
                await Task.WhenAll(getActivities);
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            foreach (var activity in myActivities)
            {
                Console.Out.WriteLine("Activity: {0}", activity.Name);
                Activity fullActivity = null;
                try
                {
                    fullActivity = getActivities.Where(a => a.Result.Id == activity.Id).First().Result;
                }
                catch (Exception ex)
                {
                    //This is for a class project. I'm going to swallow all the exceptions I want. Sorry not sorry. Get over it. Technically I'm doing _something_ with it anyway by logging it. So there.
                    Console.Out.WriteLine("Error getting full activity for {0}: {1}", activity.Name, ex.Message);
                }
                if (fullActivity != null)
                {
                    List<Task<Leaderboard>> getLeaderboards = new List<Task<Leaderboard>>();
                    foreach (var segEffort in fullActivity.SegmentEfforts)
                    {
                        int segId = segEffort.Segment.Id;
                        getLeaderboards.Add(segClient.GetFullSegmentLeaderboardAsync(segId.ToString()));
                    }

                    try
                    { 
                        await Task.WhenAll(getLeaderboards);
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
                    }
                    foreach (var segEffort in fullActivity.SegmentEfforts)
                    {
                        int segId = segEffort.Segment.Id;
                        Console.Out.WriteLine("Effort: {0}", segEffort.Segment.Name);

                        // looks like we probably need to query the leaderboards in order to determine where this person falls on this segment
                        // we can limit the number of queries by just querying once per segment, since we don't care about their other efforts...  
                        if (!mySegments.ContainsKey(segId))
                        {
                            Leaderboard leaderboard = null;
                            try
                            {
                                leaderboard = getLeaderboards[fullActivity.SegmentEfforts.IndexOf(segEffort)].Result;
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine("Error getting leaderboard for {0}: {1}", segEffort.Segment.Name, ex.Message);
                            }
                            if (leaderboard != null)
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

            SortSegmentsIntoGroups(mySegments, ref uphill, ref downhill, ref flat, ref upNDown);

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

            var startLat = 41.74647;
            var startLon = -111.80923;

            CalculateMetersPerDegreeLatLon(startLat);

            var overallSouth = startLat - MetersToLatitude(MAX_SEARCH_EXTENT);
            var overallWest = startLon - MetersToLongitude(MAX_SEARCH_EXTENT);
            var overallNorth = startLat + MetersToLatitude(MAX_SEARCH_EXTENT);
            var overallEast = startLon + MetersToLongitude(MAX_SEARCH_EXTENT);

            double latDiff = Math.Abs(overallSouth - overallNorth);
            double lonDiff = Math.Abs(overallWest - overallEast);

            int[] squareSizes = new int[] { 500, 1000, 2000, 2500, 5000 };


            List<Task<ExplorerResult>> exploreThese = new List<Task<ExplorerResult>>();
            for (int i = 0; i < squareSizes.Length; i++)
            {
                for (int latDistance = 0; latDistance < MAX_SEARCH_EXTENT * 2; latDistance += squareSizes[i])
                {
                    for (int lonDistance = 0; lonDistance < MAX_SEARCH_EXTENT * 2; lonDistance += squareSizes[i])
                    {
                        Coordinate southWest = new Coordinate(overallSouth + MetersToLatitude(latDistance), overallWest + MetersToLongitude(lonDistance));
                        Coordinate northEast = new Coordinate(overallSouth + MetersToLatitude(latDistance + squareSizes[i]), overallWest + MetersToLongitude(lonDistance + squareSizes[i]));
                        //Console.Out.WriteLine("Square size: {0}, latDistance: {1}, lonDistance: {2}, southWest: {3}, northEast: {4}", squareSizes[i], latDistance, lonDistance, southWest, northEast);
                        exploreThese.Add(segClient.ExploreSegmentsAsync(southWest, northEast, true));
                        //ExplorerResult segments = await segClient.ExploreSegmentsAsync(southWest, northEast);
                        //Console.Out.WriteLine("RESULTS: {0}", segments.Results.Count);
                        //FoldInResults(segments.Results, segsToCompare);
                    }
                }
            }

            exploreThese.Add(segClient.ExploreSegmentsAsync(new Coordinate(overallSouth, overallWest), new Coordinate(overallNorth, overallEast), true));
            //ExplorerResult overallResult = await segClient.ExploreSegmentsAsync(new Coordinate(overallSouth, overallWest), new Coordinate(overallNorth, overallEast));

            //FoldInResults(overallResult.Results, segsToCompare);

            try
            { 
                await Task.WhenAll(exploreThese);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }


            Dictionary<int, ExplorerSegment> segsToCompare = new Dictionary<int, ExplorerSegment>();
            foreach (var task in exploreThese)
            {
                FoldInResults(task.Result.Results, segsToCompare);
            }

            List<Task<Segment>> exploredSegs = new List<Task<Segment>>();
            //List<Task<Leaderboard>> exploredLeaderboards = new List<Task<Leaderboard>>();
            foreach (var task in segsToCompare)
            {
                exploredSegs.Add(segClient.GetSegmentAsync(task.Key.ToString()));
                //exploredLeaderboards.Add(segClient.GetFullSegmentLeaderboardAsync(task.Key.ToString()));
            }

            try
            {
                await Task.WhenAll(exploredSegs);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Problem getting explored segment data: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            //try
            //{
            //    await Task.WhenAll(exploredLeaderboards);
            //}
            //catch (Exception ex)
            //{
            //    Console.Out.WriteLine("Problem getting explored segment leaderboard data: {0}\r\n{1}", ex.Message, ex.StackTrace);
            //}

            Dictionary<int, UserSegment> finalCompareList = new Dictionary<int, UserSegment>();

            foreach (var segment in exploredSegs)
            {
                UserSegment newUserSeg = new UserSegment(null, false, null);
                newUserSeg.Segment = segment.Result;
                finalCompareList.Add(segment.Id, newUserSeg);
            }

            List<UserSegment> uphillNew = new List<UserSegment>();
            List<UserSegment> downhillNew = new List<UserSegment>();
            List<UserSegment> flatNew = new List<UserSegment>();
            List<UserSegment> upNDownNew = new List<UserSegment>();

            SortSegmentsIntoGroups(finalCompareList, ref uphillNew, ref downhillNew, ref flatNew, ref upNDownNew);

            Console.Out.WriteLine("Now time to compare 'em!!");

            foreach (var segment in uphillNew)
            {
                segment.SimilarityScore = CosineSimilarity(uphillVec, new double[] { segment.Segment.Distance, segment.Segment.AverageGrade });
            }

            foreach (var segment in downhillNew)
            {
                segment.SimilarityScore = CosineSimilarity(downhillVec, new double[] { segment.Segment.Distance, segment.Segment.AverageGrade });
            }

            foreach (var segment in flatNew)
            {
                segment.SimilarityScore = CosineSimilarity(flatVec, new double[] { segment.Segment.Distance, segment.Segment.AverageGrade });
            }

            foreach (var segment in upNDownNew)
            {
                segment.SimilarityScore = CosineSimilarity(upDownVec, new double[] { segment.Segment.Distance, segment.Segment.AverageGrade });
            }

            uphillNew = uphillNew.OrderByDescending(s => s.SimilarityScore).Take(TOP_N).ToList();
            downhillNew = downhillNew.OrderByDescending(s => s.SimilarityScore).Take(TOP_N).ToList();
            flatNew = flatNew.OrderByDescending(s => s.SimilarityScore).Take(TOP_N).ToList();
            upNDownNew = upNDownNew.OrderByDescending(s => s.SimilarityScore).Take(TOP_N).ToList();

            List<Task<Leaderboard>> getUphillLBs = new List<Task<Leaderboard>>();
            List<Task<Leaderboard>> getDownhillLBs = new List<Task<Leaderboard>>();
            List<Task<Leaderboard>> getFlatLBs = new List<Task<Leaderboard>>();
            List<Task<Leaderboard>> getRollingLBs = new List<Task<Leaderboard>>();
            //foreach (var segment in uphillNew)
            //{
            //    getUphillLBs.Add(segClient.GetFullSegmentLeaderboardAsync(segment.SegmentId.ToString()));
            //}
            //foreach (var segment in downhillNew)
            //{
            //    getDownhillLBs.Add(segClient.GetFullSegmentLeaderboardAsync(segment.SegmentId.ToString()));
            //}
            //foreach (var segment in flatNew)
            //{
            //    getFlatLBs.Add(segClient.GetFullSegmentLeaderboardAsync(segment.SegmentId.ToString()));
            //}
            //foreach (var segment in upNDownNew)
            //{
            //    getRollingLBs.Add(segClient.GetFullSegmentLeaderboardAsync(segment.SegmentId.ToString()));
            //}

            //try
            //{
            //    await Task.WhenAll(getUphillLBs);
            //}
            //catch (Exception ex)
            //{
            //    Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            //}
            //try
            //{
            //    await Task.WhenAll(getDownhillLBs);
            //}
            //catch (Exception ex)
            //{
            //    Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            //}
            //try
            //{
            //    await Task.WhenAll(getFlatLBs);
            //}
            //catch (Exception ex)
            //{
            //    Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            //}
            //try
            //{
            //    await Task.WhenAll(getRollingLBs);
            //}
            //catch (Exception ex)
            //{
            //    Console.Out.WriteLine("Problem getting all activities: {0}\r\n{1}", ex.Message, ex.StackTrace);
            //}

            Console.Out.WriteLine("How'd we do?");

            return new StravaRecommendations
            {
                Flat = flatNew,
                Uphill = uphillNew,
                Downhill = downhillNew,
                UpAndDown = upNDownNew
            };
        }

        private static void CalculatePopularityWeights()
        {

        }

        private static int GetMyPlace(long id, Leaderboard leaderboard)
        {
            int place = 1;
            foreach (var entry in leaderboard.Entries)
            {
                if (entry.AthleteId == id)
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

        private static void FoldInResults(List<ExplorerSegment> segments, Dictionary<int, ExplorerSegment> container)
        {
            foreach (var seg in segments)
            {
                if (!container.ContainsKey(seg.Id))
                {
                    container.Add(seg.Id, seg);
                }
            }
        }

        private static void CalculatePopularityScores(ref List<UserSegment> segments)
        {
        }

        private static void SortSegmentsIntoGroups(Dictionary<int, UserSegment> mySegments, ref List<UserSegment> uphill, ref List<UserSegment> downhill, ref List<UserSegment> flat, ref List<UserSegment> upNDown)
        {
            foreach (var segRecord in mySegments)
            {
                double averageGrade = double.MinValue;
                double elevationDifference = double.MinValue;
                double distance = double.MinValue;
                if (segRecord.Value.Segment != null)
                {
                    averageGrade = segRecord.Value.Segment.AverageGrade;
                    elevationDifference = segRecord.Value.Segment.MaxElevation - segRecord.Value.Segment.MinElevation;
                    distance = segRecord.Value.Segment.Distance;
                }
                if (averageGrade != double.MinValue)
                {
                    if (averageGrade >= 3.0)
                    {
                        uphill.Add(segRecord.Value);
                    }
                    else if (averageGrade <= -3.0)
                    {
                        downhill.Add(segRecord.Value);
                    }
                    else
                    {
                        if (elevationDifference >= 50 ||
                            elevationDifference / distance > 0.03)
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
        }

        private static double CosineSimilarity(double[] mainVec, double[] comparisonVec)
        {
            if (mainVec.Length != 2 || comparisonVec.Length != 2)
            {
                throw new Exception("You blew it!!! One of these vectors doesn't have the right number of components!!");
            }

            double numerator = mainVec[0] * comparisonVec[0] + mainVec[1] * comparisonVec[1];
            double denominator1 = Math.Sqrt(mainVec[0] * mainVec[0] + mainVec[1] * mainVec[1]);
            double denominator2 = Math.Sqrt(comparisonVec[0] * comparisonVec[0] + comparisonVec[1] * comparisonVec[1]);
            return numerator / (denominator1 * denominator2);
        }

        #region Conversions and other Utilities

        private static double GetLatitudeInMeters(double latitude)
        {
            return latitude * _metersPerDegreeLatitude;
        }

        private static double GetLongitudeInMeters(double longitude)
        {
            return longitude * _metersPerDegreeLongitude;
        }

        private static double MetersToLatitude(double meters)
        {
            return meters / _metersPerDegreeLatitude;
        }

        private static double MetersToLongitude(double meters)
        {
            return meters / _metersPerDegreeLongitude;
        }

        private static void CalculateMetersPerDegreeLatLon(double latitude)
        {
            double lat = (latitude * Math.PI) / 180.0;
            double m1 = 111132.92;     // latitude calculation term 1
            double m2 = -559.82;       // latitude calculation term 2
            double m3 = 1.175;         // latitude calculation term 3
            double m4 = -0.0023;       // latitude calculation term 4
            double p1 = 111412.84;     // longitude calculation term 1
            double p2 = -93.5;         // longitude calculation term 
            double p3 = 0.118;         // longitude calculation term 3

            // Calculate the length of a degree of latitude and longitude in meters
            _metersPerDegreeLatitude = m1 + (m2 * Math.Cos(2 * lat)) + (m3 * Math.Cos(4 * lat)) +
                    (m4 * Math.Cos(6 * lat));
            //_metersPerDegreeLatitude = 110574.0;
            _metersPerDegreeLongitude = (p1 * Math.Cos(lat)) + (p2 * Math.Cos(3 * lat)) +
                        (p3 * Math.Cos(5 * lat));
            //_metersPerDegreeLongitude = GetMetersPerDegreeLongitude(lat);
        }

        #endregion Conversions
    }
}
