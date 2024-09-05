using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeSys.Wasp3D.Hosting;
using System.Threading;
using System.Data;
using System.Windows.Forms;
using BeeSys.Wasp3D.HostingX;
using GFXWrpSceneGraph;
using System.IO;
using System.Diagnostics;
using Beesys.Wasp.Workflow;

namespace Beesys.Wasp.AddIn 
{
    public partial class ClockAddIn : WDataTable,IWAddInEx
    {

        #region Class Variables

        private DataTable                       m_dtClock;
        private CPlayoutEngine                  m_ObjEngine                      = null;
        private string                          m_sFormat                        = "HH:mm:ss";
        private IWService                       m_objWService                    = null;
        private string                          m_sAddinType                     = "Clock";
        private string                          m_sTodaysFormat                  = "HH:mm:ss";
        private string                          m_sTomorrowFormat                = "HH:mm:ss";
        private frmConfig              m_FrmBaseEngineAddin             = null;
        TimeFormat                              m_objTimeFormat                  = null;
        WaspCulture m_objWaspCulture = null;
        private object _lock; 
        private bool _shutdownCalled;
        private bool _renderCalled;

        #endregion

        #region Constructor

        public ClockAddIn()
        {
            TableBehaviour = DataTableBehaviour.None; 
            _lock = new object();
            _shutdownCalled = false;
            _renderCalled = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Config string to store the information of addin
        /// </summary>
        public override string Config
        {
            get;
            set;
        }


        public string AddinID
        {
            get
            {
                return "";
            }
        }

        public override string AddinType
        {
            get
            {
                return m_sAddinType;
            }
        }

        public override IWProvider Provider
        {
            get;
            set;
        }

        public override AddInOptionsBehaviour AddInBehaviour 
        {
            get
            {
                return AddInOptionsBehaviour.AllowAll;
            }

        }

        /// <summary>
        /// Override HideDefaultEvent to hide the Default Event 
        /// shown on addin node in Variable Pool like (OnPageChange, OnDataReady etc)
        /// </summary>
        public override bool HideDefaultEvent
        {
            get
            {
                return true;
            }
        }

        public WDateTime Today
        {
            get;
            set;
        }


        public WDateTime Tomorrow
        {
            get;
            set;
        }
        public IWService Service
        {
            get
            {
                return m_objWService;
            }
            set
            {

                m_objWService = value;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// On addin button click or addin edit
        /// Opens form to select time format
        /// </summary>
        /// <param name="showDialog"></param>
        /// <returns></returns>
        public override DialogResult Editor(bool showDialog)
        {

            DialogResult objdialogrslt = DialogResult.None;
            try
            {
                if (showDialog)
                {
                    if (m_objWaspCulture == null)
                    {
                        if ((this.Provider as ClockAddInProvider) != null && (this.Provider as ClockAddInProvider).CultureReader != null)
                            m_objWaspCulture = (this.Provider as ClockAddInProvider).CultureReader;

                    }
                    if (m_objTimeFormat == null)
                    {
                        m_objTimeFormat = new TimeFormat();
                        m_objTimeFormat.Dock = DockStyle.Fill;
                        m_objTimeFormat.Internationalization(m_objWaspCulture);
                        if (!string.IsNullOrEmpty(this.Provider.CurrentCulture))
                        {
                            m_objTimeFormat.AssemblyPath = (this.Provider as ClockAddInProvider).AssemblyPath;
                            m_objTimeFormat.ResourceCulture = this.Provider.CurrentCulture;
                        }
                        m_objTimeFormat.InitHelp(Service);
                    }

                    if (m_FrmBaseEngineAddin == null)
                    {
                        m_FrmBaseEngineAddin = new frmConfig();


                        if (!(this.Provider as ClockAddInProvider).CultureReader.FileExist)
                            m_FrmBaseEngineAddin.Text = "Time Format";

                        m_FrmBaseEngineAddin.Internationalization(m_objWaspCulture, (this.Provider as ClockAddInProvider).CultureReader.GetResourceValue("CaptionTimeFormat"));
                        m_FrmBaseEngineAddin.AddControl(m_objTimeFormat);
                        m_objTimeFormat.BringToFront();
                    }
                    m_objTimeFormat.Format = Config;
                    objdialogrslt = m_FrmBaseEngineAddin.ShowDialog();
                    if (objdialogrslt == DialogResult.OK)
                    {
                        m_sFormat = m_objTimeFormat.CurrentTimeFormat;
                        m_sTodaysFormat = m_objTimeFormat.TodaysFormat;
                        m_sTomorrowFormat = m_objTimeFormat.TomorrowFormat;
                        Config = m_sFormat + "," + m_sTodaysFormat + "," + m_sTomorrowFormat;
                        SetValue(Config);
                        UpdateTime(true);
                    }
                    m_FrmBaseEngineAddin.Close();
                }
                else
                {
                    SetValue(Config);
                    UpdateTime(true);

                }
                return objdialogrslt;
            }
            catch (Exception ex)
            {
                BeeSys.Wasp3D.Hosting.LogWriter.WriteLog(ex);
                return objdialogrslt;
            }
            finally
            {
                m_FrmBaseEngineAddin = null;
                m_objTimeFormat = null;

            }
        }

        protected override void OnShutDown()
        {
            try
            {
                m_ObjEngine = null;
                if (m_dtClock != null)
                    m_dtClock.Dispose(); 
                m_dtClock = null;
                m_objWService = null;

                m_sFormat = null;
                m_sAddinType = null;
                m_sTodaysFormat = null;
                m_sTomorrowFormat = null;

                m_objTimeFormat = null;
                m_objWaspCulture = null;
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ex);
            }
        }


        public void CleanUp() 
        {
            try
            {
                _shutdownCalled = true; 
                if (m_ObjEngine != null)
                    m_ObjEngine.m_evtEWEnginRenderInfo -= new dlgtOnEngineRenderInfo(m_ObjEngine_m_evtEWEnginPreRenderInfo);
                while (_renderCalled) 
                {
                    Thread.Sleep(2);
                }
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ClockAddinConstants.MODULENAME, ex);
            }
        }

        /// <summary>
        /// On addin initialize startup is called
        /// </summary>
        public override void StartUp()
        {
            try
            {
                SetValue(Config);

                InitWireData();
                m_ObjEngine = Engine;
                //Defect :Clock Addin is not playing in Designer on newSDK Dlls
                //https://waspsource.beesys.com/Products/Common/-/issues/3391
                //if (m_ObjEngine.DesignerMode == 0) 
                    m_ObjEngine.m_evtEWEnginRenderInfo += new dlgtOnEngineRenderInfo(m_ObjEngine_m_evtEWEnginPreRenderInfo);

                CreateVariable(ClockAddinConstants.VARTODAYDATE, "", VAR_TYPE.VAR_DATE_TIME, VariableOptionsBehaviour.AllowVisible | VariableOptionsBehaviour.AllowWiring, ControlType.Clock);
                CreateVariable(ClockAddinConstants.VARTOMORROWDATE, "", VAR_TYPE.VAR_DATE_TIME, VariableOptionsBehaviour.AllowVisible | VariableOptionsBehaviour.AllowWiring, ControlType.Clock);

                UpdateTime(true);
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ClockAddinConstants.MODULENAME, ex);
            }
        }

        /// <summary>
        /// set the type of addin
        /// </summary>
        /// <param name="sType"></param>
        public void SetAddinType(string sType)
        {
            if (!string.IsNullOrEmpty(sType))
                m_sAddinType = sType;
        }

        /// <summary>
        /// On addin shutdown
        /// </summary>
        public void OnBeforeShutDown()
        {
            lock (_lock)
            {
                CleanUp();
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize the datatable for engine.
        /// Hour,Minute and Second Columns are of Float Type
        /// that store the degree equivalent of Current Hour,Minute and Seconds.
        /// </summary>
        private void InitWireData()
        {
            try
            {
                if (m_dtClock != null)
                    m_dtClock.Dispose();

                m_dtClock = new DataTable();
                m_dtClock.Columns.Add(ClockAddinConstants.TIME, typeof(string));
                m_dtClock.Columns.Add(ClockAddinConstants.HOUR, typeof(float));
                m_dtClock.Columns.Add(ClockAddinConstants.MINUTE, typeof(float));
                m_dtClock.Columns.Add(ClockAddinConstants.SECOND, typeof(float));
                m_dtClock.Columns.Add(ClockAddinConstants.TODAY, typeof(string));
                m_dtClock.Columns.Add(ClockAddinConstants.TOMORROW, typeof(string));
                DataRow datarow = m_dtClock.NewRow();
                m_dtClock.Rows.Add(datarow);
                Mode = Mode.page;
                Sync(m_dtClock);
            }
            catch (Exception exp)
            {
                BeeSys.Wasp3D.Hosting.LogWriter.WriteLog(exp);
            }
        }//end (InitDataTable)


        /// <summary>
        /// Get Updated Time.
        /// Calculate Degree Equivalent Of Time.
        /// Update the Values in Wired Datatable.
        /// </summary>
        private void UpdateTime(bool updatevariable)
        {
            DataRow datarow = null;
            float fDegreeHour, fDegreeMin, fDegreeSecond = 0;
            int iHour, iMinute, iSecond = 0;
            int iRem;
            int iDiv;

            DateTime dtNow = DateTime.Now;
            string sTime = null;
            try
            {
                sTime = dtNow.ToString(m_sFormat);
                iHour = DateTime.Now.Hour;
                iMinute = DateTime.Now.Minute;
                iSecond = DateTime.Now.Second;
                if (iHour >= 12)
                    iHour = iHour - 12;
                fDegreeSecond = iSecond * 6;
                fDegreeMin = (iMinute * 6);
                fDegreeHour = (iHour * 30);

                //After every 10 second, move minute hand by a degree
                iDiv = Math.DivRem(iSecond, 10, out iRem);
                if (iDiv > 0 && fDegreeMin != 0)
                    fDegreeMin += iDiv;

                //After every 2 Minute, move hour hand by a degree
                iDiv = Math.DivRem(iMinute, 2, out iRem);
                if (iDiv > 0 && fDegreeHour != 0)
                    fDegreeHour += iDiv;

                if (fDegreeHour > 359)
                    fDegreeHour = 0;
                if (m_dtClock.Rows.Count > 0)
                {
                    datarow = m_dtClock.Rows[0];
                    datarow[ClockAddinConstants.TIME] = sTime;
                    datarow[ClockAddinConstants.HOUR] = fDegreeHour;
                    datarow[ClockAddinConstants.MINUTE] = fDegreeMin;
                    datarow[ClockAddinConstants.SECOND] = fDegreeSecond;
                    datarow[ClockAddinConstants.TODAY] = dtNow.ToString(m_sTodaysFormat);
                    datarow[ClockAddinConstants.TOMORROW] = dtNow.AddDays(1).ToString(m_sTomorrowFormat);
                    m_dtClock.AcceptChanges();
                    Sync(m_dtClock);

                    if (updatevariable && !_shutdownCalled)
                    {
                        //Debug.WriteLine("@#$", "calling update");
                        UpdateVariable(ClockAddinConstants.VARTODAYDATE, dtNow.ToFileTime().ToString());
                        UpdateVariable(ClockAddinConstants.VARTOMORROWDATE, dtNow.AddDays(1).ToFileTime().ToString());

                        Today = new WDateTime(dtNow.ToFileTime());
                        Tomorrow = new WDateTime(dtNow.AddDays(1).ToFileTime());
                        //Debug.WriteLine("@#$", "update call complete");
                    }
                }
            }
            finally
            {
                datarow = null;
                sTime = null;
            }
        }//end (UpdateTime)

        

        /// <summary>
        /// Sets the Last Selected Format
        /// </summary>
        /// <param name="svalue"></param>
        private void SetValue(string svalue)
        {
            string[] sarrFormats = null;
            try
            {
                if (!string.IsNullOrEmpty(svalue))
                    sarrFormats = svalue.Split(',');
                if (sarrFormats != null && sarrFormats.Length >= 2)
                {
                    m_sFormat = sarrFormats[0];
                    m_sTodaysFormat = sarrFormats[1];
                    m_sTomorrowFormat = sarrFormats[2];
                }
            }
            catch (Exception ex)
            {
                BeeSys.Wasp3D.Hosting.LogWriter.WriteLog(ex);
            }
            finally
            {
                sarrFormats = null;
            }
        }

        #endregion

        #region Events Handled

        /// <summary>
        /// Engine PreRender Event
        /// Update time on this event
        /// </summary>
        /// <param name="info"></param>
        void m_ObjEngine_m_evtEWEnginPreRenderInfo(stRenderInfo_Managed info)
        {
            try
            {
                lock (_lock)
                {
                    try
                    {
                        if (!_shutdownCalled)
                        {
                            _renderCalled = true;
                            UpdateTime(true);
                        }
                    }
                    catch (Exception lex)
                    {
                        BeeSys.Wasp3D.Hosting.LogWriter.WriteLog(lex);
                    }
                }
            }
            catch (Exception ex)
            {
                BeeSys.Wasp3D.Hosting.LogWriter.WriteLog(ex);
            }
            finally
            {
                _renderCalled = false;
            }
        }
        #endregion

    }
}
