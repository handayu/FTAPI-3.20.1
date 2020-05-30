using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraderConnectClient
{
    public partial class Form1 : Form
    {
        private FTAPI_Trd trd = null;
        private SampleTrdConnCallback m_stcon = new SampleTrdConnCallback();
        private SampleTrdCallback m_st = new SampleTrdCallback();
        private FileSystemWatcher m_watcher = new FileSystemWatcher();

        public Form1()
        {
            InitializeComponent();
            this.button1.Click += Button1_Click;
            this.richTextBox1.AppendText("Client Init success...");

            m_st.InfosEvents += M_InfosEvents;
            m_stcon.InfosEvents += M_InfosEvents;
        }

        private void M_InfosEvents(string str)
        {
            AppendLogInfo(str);

            if(str == "ConnectedSuccessed")
            {
                this.button1.BackColor = Color.Green;
            }

            if(str == "DisConnected")
            {
                this.button1.BackColor = Color.Red;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //API初始化
            FTAPI.Init();

            AppendLogInfo("Begin login op...");

            //交易API初始化
            trd = new FTAPI_Trd();

            //注册连接回调
            trd.SetConnCallback(m_stcon);

            //注册交易回调
            trd.SetTrdCallback(m_st);

            //设置Clint信息
            trd.SetClientInfo("FTAPI4NET_Sample", 1);

            //开始链接
            trd.InitConnect("127.0.0.1", 11111, false);

        }

        public void AppendLogInfo(string info)
        {
            if(this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendLogInfo),info);
                return;
            }

            this.richTextBox1.AppendText("\r\n" + info);
        }

        public void AppendTradeInfo(string info)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendTradeInfo), info);
                return;
            }

            this.richTextBox_TEXTSINGLEINFO.AppendText("\r\n" + info);
        }

        private void Button_PlaceOrder_Click(object sender, EventArgs e)
        {
            //trd未初始化直接返回
            if (trd == null) return;

            //创建发单对象
            //BUY-SELL-SELLSHORT买卖开平
            InsertOrder orderInsert = this.CreateInsertOrder();

            //调用trd下单
            TrdPlaceOrder.Request.Builder req = TrdPlaceOrder.Request.CreateBuilder();
            TrdPlaceOrder.C2S.Builder cs = TrdPlaceOrder.C2S.CreateBuilder();
            Common.PacketID.Builder packetID = Common.PacketID.CreateBuilder().SetConnID(trd.GetConnectID()).SetSerialNo(0);

            //设置参与的账户--真实模拟市场--参与的市场(美港A股等)
            TrdCommon.TrdHeader.Builder trdHeader = TrdCommon.TrdHeader.CreateBuilder().SetAccID(m_st.accID).SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Real).SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_US);
            //设置下单的参数
            cs.SetPacketID(packetID).SetHeader(trdHeader).SetTrdSide((int)orderInsert.OrderSide).SetOrderType((int)orderInsert.OrderType).SetCode(orderInsert.StockCode).SetQty(orderInsert.Shares).SetPrice(orderInsert.Price).SetAdjustPrice(true);
            req.SetC2S(cs);

            uint serialNo = trd.PlaceOrder(req.Build());

        }

        /// <summary>
        /// 创建交易单
        /// </summary>
        /// <returns></returns>
        private InsertOrder CreateInsertOrder()
        {
            InsertOrder order = new InsertOrder();
            order.StockCode = this.TextBox_StockCode.Text;
            order.Shares = Convert.ToDouble(this.textBox_Shres.Text);
            order.Price = Convert.ToDouble(this.textBox_Price.Text);
            order.OrderType = (TrdCommon.OrderType)this.comboBox_OrderType.SelectedIndex;
            order.OrderSide = (TrdCommon.TrdSide)this.comboBox_OrderSide.SelectedIndex;
            return order;
        }

        /// <summary>
        /// 开启的时候默认的
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Load(object sender, EventArgs e)
        {
            this.comboBox_OrderSide.SelectedIndex = 1;//买入
            this.comboBox_OrderType.SelectedIndex = 2;//市价单
        }

        /// <summary>
        /// 读取文本Path下的最后也是最新的一条数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadTxt(string path)
        {
            try
            {
                List<string> lastTxtSingleInfo = new List<string>();

                //设置文件共享方式为读写，FileShare.ReadWrite，这样的话，就可以打开了
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    lastTxtSingleInfo.Add(line.ToString());
                }
                if (lastTxtSingleInfo.Count == 0)
                {
                    return "";
                }
                else
                {
                    return lastTxtSingleInfo[lastTxtSingleInfo.Count - 1];
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// 清空文本PTH下的所有内容
        /// </summary>
        /// <param name="path"></param>
        public void ClearTxt(string path)
        {
            try
            {
                //设置文件共享方式为读写，FileShare.ReadWrite，这样的话，就可以打开了
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.SetLength(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("clear failed:" + ex.Message);
            }

        }

        private void WatcherStrat(string path, string filter)
        {
            m_watcher.Path = path;
            m_watcher.Filter = filter;
            m_watcher.Changed += new FileSystemEventHandler(OnProcess);
            m_watcher.Created += new FileSystemEventHandler(OnProcess);
            m_watcher.Deleted += new FileSystemEventHandler(OnProcess);
            m_watcher.Renamed += new RenamedEventHandler(OnRenamed);
            m_watcher.EnableRaisingEvents = true;
            //watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
            //                       | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            m_watcher.NotifyFilter = NotifyFilters.LastWrite;

            m_watcher.IncludeSubdirectories = true;
        }

        private  void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                OnCreated(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                OnChanged(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                OnDeleted(source, e);
            }

        }
        private  void OnCreated(object source, FileSystemEventArgs e)
        {
            AppendLogInfo(string.Format("文件新建事件处理逻辑 {0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name));
        }
        private  void OnChanged(object source, FileSystemEventArgs e)
        {
            m_watcher.EnableRaisingEvents = false;

            //打LOG
            AppendLogInfo(string.Format("文件改变事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name));

            //读最后一条，打LOG
            string lastInfo = ReadTxt(this.textBox_DicPATH.Text + "\\" + this.textBox_FiileName.Text);
            AppendLogInfo(string.Format("Last-Text-Info:{0}", lastInfo));
            AppendTradeInfo(lastInfo);

            //发单
            OrderAction(lastInfo);

            m_watcher.EnableRaisingEvents = true;
        }

        private void OrderAction(string lastInfo)
        {
            //拆解最后一条，生成orderInsert，发单
            string[] arrayInfos = lastInfo.Split('|');

            InsertOrder orderInsert = new InsertOrder();
            orderInsert.StockCode = arrayInfos[0].Trim();
            orderInsert.OrderSide = (TrdCommon.TrdSide)Convert.ToDouble(arrayInfos[1].Trim());
            orderInsert.OrderType = (TrdCommon.OrderType)Convert.ToDouble(arrayInfos[2].Trim());
            orderInsert.Shares = Convert.ToDouble(arrayInfos[3].Trim());
            orderInsert.Price = Convert.ToDouble(arrayInfos[4].Trim());

            //调用trd下单
            TrdPlaceOrder.Request.Builder req = TrdPlaceOrder.Request.CreateBuilder();
            TrdPlaceOrder.C2S.Builder cs = TrdPlaceOrder.C2S.CreateBuilder();
            Common.PacketID.Builder packetID = Common.PacketID.CreateBuilder().SetConnID(trd.GetConnectID()).SetSerialNo(0);

            //设置参与的账户--真实模拟市场--参与的市场(美港A股等)
            TrdCommon.TrdHeader.Builder trdHeader = TrdCommon.TrdHeader.CreateBuilder().SetAccID(m_st.accID).SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Real).SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_US);
            //设置下单的参数
            cs.SetPacketID(packetID).SetHeader(trdHeader).SetTrdSide((int)orderInsert.OrderSide).SetOrderType((int)orderInsert.OrderType).SetCode(orderInsert.StockCode).SetQty(orderInsert.Shares).SetPrice(orderInsert.Price).SetAdjustPrice(true);
            req.SetC2S(cs);

            uint serialNo = trd.PlaceOrder(req.Build());
        }

        private  void OnDeleted(object source, FileSystemEventArgs e)
        {
            AppendLogInfo(string.Format("文件删除事件处理逻辑{0}  {1}   {2}", e.ChangeType, e.FullPath, e.Name));
        }
        private  void OnRenamed(object source, RenamedEventArgs e)
        {
            AppendLogInfo(string.Format("文件重命名事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name));
        }

        private void CheckBox_FileMonitor_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_FileMonitor.Checked)
            {
                WatcherStrat(this.textBox_DicPATH.Text, "*.*");
                this.label_Monitor.BackColor = Color.Green;
                AppendTradeInfo("File is Monitoring...");
            }
            else
            {
                m_watcher.EnableRaisingEvents = false;
                this.label_Monitor.BackColor = Color.Red; 
            }
        }

    }
}
