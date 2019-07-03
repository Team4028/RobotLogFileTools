using System;
using System.Collections.Generic;
using System.Text;

using FileHelpers;

namespace FixUpPathPlannerPaths.Entities
{
    /// <summary>
    /// Class PathSegment.
    /// </summary>
    /// <example>
    /// dt,x,y,position,velocity,acceleration,jerk,heading
    /// 0.010000,0.011986,288.000006,0.000030,0.005947,0.594707,59.470737,0.000999
    /// </example>
    [DelimitedRecord(",")]
    class PathSegmentBE
    {
        public decimal dt;

        public decimal x;

        public decimal y;

        public decimal position;

        public decimal velocity;

        public decimal acceleration;

        public decimal jerk;

        public decimal heading;
    }
}
