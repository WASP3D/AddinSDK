using Beesys.Wasp.Workflow;
using BeeSys.Wasp3D.Hosting;
using DevExpress.LookAndFeel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogWriter = Beesys.Wasp.Workflow.LogWriter;

namespace Beesys.Wasp.AddIn
{
    public partial class frmConfig : WXtraForm
    {
        #region variable
        private  TimeFormat m_TimeFormatCtrl = null;
        private static string _ok = "OK";
        private static string _cancel = "Cancel";
        private static string _frmcaption = string.Empty;
        private bool _bIsBtnLoaded = false;
        #endregion



        #region constructor
        public frmConfig()
        {
            try
            {
                InitializeComponent();
                SetProperties();
                BindEvent();
                //Setskin();
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }
        static frmConfig()
        {
          
        }
        #endregion




        #region properties

        public static string m_AssemblyPath
        {
            get;
            set;
        }
        private void SetProperties()
        {
            try
            {
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ex);
            }

        }
        #endregion



        #region private method
        private void BindEvent()
        {

            try
            {
                if (btnOk != null)
                {
                    btnOk.Click -= BtnOk_Click;
                    btnOk.Click += BtnOk_Click;
                }
                

                if (btnCancel != null)
                {
                    btnCancel.Click -= BtnCancel_Click;
                    btnCancel.Click += BtnCancel_Click;
                }
                

                if (btnHelp != null)
                {
                    btnHelp.Click -= BtnHelp_Click;
                    btnHelp.Click += BtnHelp_Click;
                }
                    
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);

            }
        }
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_TimeFormatCtrl != null)
                    m_TimeFormatCtrl.ShowHelp();
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);

            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);

            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_TimeFormatCtrl != null)
                    m_TimeFormatCtrl.OnOkClick();
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);

            }
        }
        private void SetAppearenceColor()
        {
            IWService m_IService = null;
            try
            {
                m_IService = m_TimeFormatCtrl.WService;
                if (m_IService != null && m_IService?.GetAppearance() != null &&
                    string.Compare(m_IService.GetAppearance().Theme, "dark", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    btnHelp.ImageOptions.Image = Resource.HelpIcon_DARK;
                    _bIsBtnLoaded = true;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
            finally
            {
                m_IService = null;
                this.ResumeLayout(false);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            if (!_bIsBtnLoaded)
                SetAppearenceColor();
            base.OnLoad(e);
        }
        #endregion



        #region public method

        public void AddControl(TimeFormat objControl)
        {
            int width;
            int height;
            try
            {
                if (objControl != null)
                {

                    if (this.Controls.Count > 0)
                    {
                        foreach (Control ctrl in this.Controls)
                        {
                            if (ctrl != null && (ctrl is TimeFormat))
                            {
                                this.Controls.Remove(ctrl);
                                break;
                            }
                        }
                    }
                    m_TimeFormatCtrl = objControl;
                    objControl.Dock = DockStyle.Fill;
                    this.Controls.Add(objControl);

                    height = objControl.Height + 80;
                    width = objControl.Width + 10;

                    this.Size = new Size(width, height);
                    this.MaximumSize=new Size(width, height);
                    this.MinimumSize = new Size(width, height);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }


        public void Internationalization(WaspCulture CultureReader, string formcaption)//S.No.: -06
        {
            try
            {
                if (CultureReader != null && CultureReader.FileExist)
                {
                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("ok")))
                        _ok = CultureReader.GetResourceValue("ok");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("cancel")))
                        _cancel = CultureReader.GetResourceValue("cancel");

                    if (!string.IsNullOrEmpty(formcaption))
                        _frmcaption = formcaption;
                    btnOk.Text = _ok;
                    btnCancel.Text = _cancel;
                    this.Text = _frmcaption;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
            finally
            {
            }
        }
       
        #endregion

    }
}
