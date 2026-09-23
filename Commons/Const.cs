using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tec_parts_replenishment_transpor_web.Commons
{
    public static class Const
    {
        // エリア
        public const int C_AREA_KUBUN = 1; // 1 組立課, 2 プレス課

        public const string C_WORK_LIFT = "Lift"; 

        public const string C_WORK_TAGNOVA= "TagNova";

        public static class Status
        {
            public const int C_PENDING_STATUS = 1;
            public const int C_START_STATUS = 2;
            public const int C_COMPLETE_STATUS = 3;
            public const int C_CANCEL_STATUS = 4;
        }
    }
}
