using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StravaRec
{
    public class StravaRecommendations
    {
        public List<UserSegment> Flat { get; set; }

        public List<UserSegment> Uphill { get; set; }

        public List<UserSegment> Downhill { get; set; }

        public List<UserSegment> UpAndDown { get; set; }
    }
}
