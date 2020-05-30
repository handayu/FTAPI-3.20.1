using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderConnectClient
{
    public class SampleTrdConnCallback : FTSPI_Conn
    {
        public delegate void InfosHandle(string str);
        public event InfosHandle InfosEvents;

        public void OnInitConnect(FTAPI_Conn client, long errCode, string desc)
        {
            SafeRaiseEvent("InitConnected");
            if (errCode == 0)
            {
                FTAPI_Trd trd = client as FTAPI_Trd;
                {
                    //传入Trd,Connect连接成功，发送获取账户
                    TrdGetAccList.Request req = TrdGetAccList.Request.CreateBuilder().SetC2S(TrdGetAccList.C2S.CreateBuilder().SetUserID(0)).Build();
                    uint serialNo = trd.GetAccList(req);
                    SafeRaiseEvent(string.Format("Send GetAccList: {0}", serialNo));
                    SafeRaiseEvent("ConnectedSuccessed");

                }
            }
        }

        public void OnDisconnect(FTAPI_Conn client, long errCode)
        {
            SafeRaiseEvent("DisConnected");
        }

        public void SafeRaiseEvent(string str)
        {
            if(InfosEvents != null)
            {
                InfosEvents(str);
            }
        }
    }

}
