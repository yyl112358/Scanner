using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scanner.Model;
using Scanner.BLL;

namespace Scanner
{
    public partial class MainForm : Form
    {
        #region Filed
        List<Control> m_controls= new List<Control>();
        Scanner.BLL.Scanner scanner = new Scanner.BLL.Scanner();
        int m_StartPort = 0;
        int m_EndPort = 0;
        #endregion

        public MainForm()
        {
            Init();
        }
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init()
        {
            InitializeComponent();
            foreach (Control c in this.Controls)
            {
                m_controls.Add(c);
            }
            SetUIStatus(WorkStatus.Init);
        }
        /// <summary>
        /// 将UI的可交互性设置为指定的程序状态下的状态
        /// </summary>
        /// <param name="status">当前程序所处的状态</param>
        private void SetUIStatus(WorkStatus status)
        {
            Type type = status.GetType();
            FieldInfo filed = type.GetField(status.ToString());
            if (filed != null)
            {
                object[] bindedAttribute =  filed.GetCustomAttributes(typeof(WorkStatusAttribute), true);
                foreach (var attr in bindedAttribute)
                {
                    WorkStatusAttribute w = attr as WorkStatusAttribute;
                    if (w != null)
                    {
                        w.Controls = m_controls;
                        w.SetStatusUI();
                    }
                }
            }
        }

        private void btn_StartScan_Click(object sender, EventArgs e)
        {
            string domain = txtBox_ScannerInput.Text;
            if (!string.IsNullOrEmpty(domain))
            {
                SetUIStatus(WorkStatus.Scan);
                scanner.Domain = domain;
                scanner.OnScanProgress += OnScanProgress;
                scanner.OnScanPortComplete += OnScanPortComplete;
                scanner.OnScanedCanConnect += (portInfo) => {
                    if (list_CanUsePortList.InvokeRequired)
                    {
                        list_CanUsePortList.Invoke(new Action<PortInfo>((port) => { list_CanUsePortList.Items.Add(port); }), portInfo);
                    }
                    else
                    {
                        list_CanUsePortList.Items.Add(portInfo);
                    }
                };
                scanner.Scanning();
            }
            else
            {
                MessageBox.Show("请输入要扫描的domain");
            }
        }
        /// <summary>
        /// 逻辑层扫描事件
        /// </summary>
        /// <param name="pg">端口进度</param>
        private void OnScanProgress(int pg)
        {
            if (pg_ScannerPg.InvokeRequired)
            {
                pg_ScannerPg.Invoke(new Action<int>((t)=> { pg_ScannerPg.Value = (int)(((pg/(m_EndPort-m_StartPort * 1.0))*100)); }), pg);
            }
            else
            {
                pg_ScannerPg.Value = (int)(((pg / (m_EndPort - m_StartPort * 1.0) ) * 100));
            }
            if (lbl_ScannerPort.InvokeRequired)
            {
                lbl_ScannerPort.Invoke(new Action<int>((t) => { lbl_ScannerPort.Text = pg.ToString(); }), pg);
            }
            else
            {
                lbl_ScannerPort.Text = pg.ToString();
            }
            if (lbl_SannerPercent.InvokeRequired)
            {
                lbl_SannerPercent.Invoke(new Action<int>((t) => { lbl_SannerPercent.Text = ((t / (m_EndPort - m_StartPort * 1.0) ) *100).ToString("F2") + "%"; }), pg);
            }
            else
            {
                lbl_SannerPercent.Text = ((pg / (m_EndPort - m_StartPort * 1.0)) *100).ToString("F2") + "%";
            }
        }
        /// <summary>
        /// 逻辑层扫描完成事件
        /// </summary>
        /// <param name="info">可用端口集合</param>
        private void OnScanPortComplete(List<PortInfo> info)
        {
            SetUIStatus(WorkStatus.ScanComplete);
        }

        /// <summary>
        /// 验证输入的端口数据
        /// </summary>
        private void OnValidatePortInput(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t != null)
            {
                try
                {
                    int outer = 0;
                    bool parseResult = int.TryParse(t.Text, out outer);
                    if (parseResult)
                    {
                        if (t.Name == "txt_portFrom")
                        {
                            scanner.StartPort = outer;
                            m_StartPort = outer;
                        }
                        else
                        {
                            scanner.EndPort = outer;
                            m_EndPort = outer;
                        }
                    }
                    else
                    {
                        throw new Exception("输入的端口非法");
                    }
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }               
            }
        }
    }
}
