using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TraderConnectClient
{
    public class SampleTrdCallback : FTSPI_Trd
    {
        public delegate void InfosHandle(string str);
        public event InfosHandle InfosEvents;

        public ulong accID;

        public void OnReply_GetAccList(FTAPI_Conn client, int nSerialNo, TrdGetAccList.Response rsp)
        {
            SafeRaiseEvent(string.Format("Recv GetAccList: {0} {1}", nSerialNo, rsp));

            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                SafeRaiseEvent(string.Format("error code is {0}", rsp.RetMsg));
            }
            else
            {
                this.accID = rsp.S2C.AccListList[1].AccID;
                FTAPI_Trd trd = client as FTAPI_Trd;
                MD5 md5 = MD5.Create();
                byte[] encryptionBytes = md5.ComputeHash(Encoding.UTF8.GetBytes("196910"));
                string unlockPwdMd5 = BitConverter.ToString(encryptionBytes).Replace("-", "").ToLower();
                TrdUnlockTrade.Request req = TrdUnlockTrade.Request.CreateBuilder().SetC2S(TrdUnlockTrade.C2S.CreateBuilder().SetUnlock(true).SetPwdMD5(unlockPwdMd5)).Build();
                uint serialNo = trd.UnlockTrade(req);
                SafeRaiseEvent(string.Format("Send UnlockTrade: {0}", serialNo));

            }
        }

        public void OnReply_UnlockTrade(FTAPI_Conn client, int nSerialNo, TrdUnlockTrade.Response rsp)
        {
            SafeRaiseEvent(string.Format("Recv UnlockTrade: {0} {1}", nSerialNo, rsp));

            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                SafeRaiseEvent(string.Format("error code is {0}", rsp.RetMsg));
            }
            else
            {

                SafeRaiseEvent(string.Format("OnReply_UnlockTrade is Succeed{0}", rsp.RetMsg));

                //FTAPI_Trd trd = client as FTAPI_Trd;

                //TrdPlaceOrder.Request.Builder req = TrdPlaceOrder.Request.CreateBuilder();
                //TrdPlaceOrder.C2S.Builder cs = TrdPlaceOrder.C2S.CreateBuilder();
                //Common.PacketID.Builder packetID = Common.PacketID.CreateBuilder().SetConnID(trd.GetConnectID()).SetSerialNo(0);
                //TrdCommon.TrdHeader.Builder trdHeader = TrdCommon.TrdHeader.CreateBuilder().SetAccID(this.accID).SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Real).SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_US);
                //cs.SetPacketID(packetID).SetHeader(trdHeader).SetTrdSide((int)TrdCommon.TrdSide.TrdSide_Sell).SetOrderType((int)TrdCommon.OrderType.OrderType_Market).SetCode("AAPL").SetQty(100.00).SetPrice(10.2).SetAdjustPrice(true);
                //req.SetC2S(cs);

                //uint serialNo = trd.PlaceOrder(req.Build());
                //SafeRaiseEvent(string.Format("Send PlaceOrder: {0}, {1}", serialNo, req));
            }

        }

        public void OnReply_SubAccPush(FTAPI_Conn client, int nSerialNo, TrdSubAccPush.Response rsp)
        {

        }

        public void OnReply_GetFunds(FTAPI_Conn client, int nSerialNo, TrdGetFunds.Response rsp)
        {

        }

        public void OnReply_GetPositionList(FTAPI_Conn client, int nSerialNo, TrdGetPositionList.Response rsp)
        {

        }

        public void OnReply_GetMaxTrdQtys(FTAPI_Conn client, int nSerialNo, TrdGetMaxTrdQtys.Response rsp)
        {

        }

        public void OnReply_GetOrderList(FTAPI_Conn client, int nSerialNo, TrdGetOrderList.Response rsp)
        {

        }

        public void OnReply_GetOrderFillList(FTAPI_Conn client, int nSerialNo, TrdGetOrderFillList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderList(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderFillList(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderFillList.Response rsp)
        {

        }

        public void OnReply_UpdateOrder(FTAPI_Conn client, int nSerialNo, TrdUpdateOrder.Response rsp)
        {
            SafeRaiseEvent(string.Format("Recv UpdateOrder: {0} {1}", nSerialNo, rsp));

        }

        public void OnReply_UpdateOrderFill(FTAPI_Conn client, int nSerialNo, TrdUpdateOrderFill.Response rsp)
        {
            SafeRaiseEvent(string.Format("Recv UpdateOrderFill: {0} {1}", nSerialNo, rsp));

        }

        public void OnReply_PlaceOrder(FTAPI_Conn client, int nSerialNo, TrdPlaceOrder.Response rsp)
        {
            SafeRaiseEvent(string.Format("Recv PlaceOrder: {0} {1}", nSerialNo, rsp));

            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                SafeRaiseEvent(string.Format("error code is {0}", rsp.RetMsg));
            }
        }

        public void OnReply_ModifyOrder(FTAPI_Conn client, int nSerialNo, TrdModifyOrder.Response rsp)
        {
        }

        public void SafeRaiseEvent(string str)
        {
            if (InfosEvents != null)
            {
                InfosEvents(str);
            }
        }
    }
   
}
