using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyArraying.Enum
{
    public enum ArraySpreadDirection
    {
        StartToEnd = 0,      // Từ đầu đến cuối
        EndToStart = 1,      // Từ cuối đến đầu
        MiddleOutward = 2    // Từ giữa ra 2 đầu
    }
    public enum FlipDirection
    {
        Left_Outer = 0,
        Right_Inner = 1,
    }
}
