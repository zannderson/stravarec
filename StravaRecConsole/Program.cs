using Strava.Athletes;
using Strava.Authentication;
using Strava.Clients;
using Strava.Common;
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
		private const int MAX_SEARCH_EXTENT = 5000; //5k in each direction gives a 10k x 10k square. Seems reasonable.

		private static double _metersPerDegreeLongitude = 0;
		private static double _metersPerDegreeLatitude = 0;

        static void Main(string[] args)
        {
            var recs = Recommender.DoTheThing("34834781045370ca62b4b3fc6be384ffb868a1c0");
        }
    }
}
