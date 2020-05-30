using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderConnectClient
{
    public class InsertOrder
    {
        /// <summary>
        /// Code合约
        /// </summary>
        public string StockCode
        {
            get;
            set;
        }

        /// <summary>
        /// 数量
        /// </summary>
        public double Shares
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }

        /// <summary>
        /// 买卖开平
        /// </summary>
        public TrdCommon.TrdSide OrderSide
        {
            get;
            set;
        }

        /// <summary>
        /// 买卖类型
        /// </summary>
        public TrdCommon.OrderType OrderType
        {
            get;
            set;
        }



    }
}
