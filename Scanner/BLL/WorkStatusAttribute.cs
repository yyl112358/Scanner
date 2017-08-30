using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scanner.Model;

namespace Scanner.BLL
{
    /// <summary>
    /// 修饰当前程序工作状态的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field,AllowMultiple =true,Inherited =true)]
    public abstract class WorkStatusAttribute : Attribute
    {
        #region  Filed
        protected List<Control> m_Controls = new List<Control>();
        protected List<string> m_EnableControlName;
        #endregion

        public WorkStatusAttribute(string[] EnabledControlNameList)
        {
            m_EnableControlName = EnabledControlNameList.ToList();
        }
        #region Property
        /// <summary>
        /// 该状态下启用的空间名称
        /// </summary>
        public List<string> EnableControlName { get { return m_EnableControlName; } }
        /// <summary>
        /// form中所有的控件的集合
        /// </summary>
        public List<Control> Controls { get { return m_Controls; } set { m_Controls = value; } }
        #endregion
        /// <summary>
        /// 将控件集合批量的设置为可用
        /// </summary>
        /// <param name="enableList">控件集合</param>
        protected void SetEabled(List<Control> enableList)
        {
            foreach (Control c in enableList)
            {
                c.Enabled = true;
            }
        }
        /// <summary>
        /// 将控件集合批量的设置为不可用
        /// </summary>
        /// <param name="disenabledList">控件集合</param>
        protected void SetDisEnabled(List<Control> disenabledList)
        {
            foreach (Control c in disenabledList)
            {
                c.Enabled = false;
            }
        }
        /// <summary>
        /// 设置该状态下的UI状态
        /// </summary>
        public virtual void SetStatusUI()
        {
            List<Control> enableList = new List<Control>();
            List<Control> disEnableList = new List<Control>();

            foreach (Control c in m_Controls)
            {
                if (m_EnableControlName.Contains(c.Name))
                {
                    enableList.Add(c);
                }
                else
                {
                    disEnableList.Add(c);
                }
            }
            SetEabled(enableList);
            SetDisEnabled(disEnableList);
        }
    }



    /// <summary>
    /// 初始（空闲）状态下的特性修饰符
    /// </summary>
    public class InitStatusAttribute : WorkStatusAttribute
    {
        #region Contstructor
        public InitStatusAttribute(string[] EnabledControlNameList) : base(EnabledControlNameList) { }
        #endregion
    }
    /// <summary>
    /// 扫描状态下的特性修饰符
    /// </summary>
    public class ScanStatusAttribute : WorkStatusAttribute
    {
        #region Contstructor
        public ScanStatusAttribute(string[] EnabledControlNameList) : base(EnabledControlNameList) { }
        #endregion
    }
    /// <summary>
    /// 发送数据状态下的特性修饰符
    /// </summary>
    public class SendingDataStatusAttribute : WorkStatusAttribute
    {
        #region Contstructor
        public SendingDataStatusAttribute(string[] EnabledControlNameList) : base(EnabledControlNameList) { }
        #endregion
    }
    /// <summary>
    /// 异常状态下的特性修饰符
    /// </summary>
    public class ExceptionStatusAttribute : WorkStatusAttribute
    {
        #region Contstructor
        public ExceptionStatusAttribute(string[] EnabledControlNameList) : base(EnabledControlNameList) { }
        #endregion
    }
    /// <summary>
    /// 扫描完成后状态下的特性修饰符
    /// </summary>
    public class ScanCompleteStatusAttribute : WorkStatusAttribute
    {
        #region Contstructor
        public ScanCompleteStatusAttribute(string[] EnabledControlNameList) : base(EnabledControlNameList) { }
        #endregion
    }
}
