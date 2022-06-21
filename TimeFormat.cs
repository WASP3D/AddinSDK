using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Beesys.Wasp.Workflow;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Drawing2D;
using BeeSys.Wasp3D.Hosting;
using System.Runtime.InteropServices;
using DevExpress.XtraEditors;

namespace Beesys.Wasp.AddIn
{
    public partial class TimeFormat : UserControl
    {
        [DllImport("uxtheme.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
        #region Constants

        private DataTable m_dtClock;
        public string m_sFormat = "HH:mm:ss";
        private string m_sTodaysFormat = "HH:mm:ss";
        private string m_sTomorrowFormat = "HH:mm:ss";
        private bool m_bFlag = false;
        private Thread m_thUpdateTime;
        private DataTable m_dtHelp;
        private WaspCulture m_Culture = null;
        private IWService m_objIWService = null;
        private readonly string FORMATHELP = "d@Displays the current day of the month, measured as a number between 1 and 31, inclusive. If the day is a single digit only (1-9), then it is displayed as a single digit.\r\n" +
                                                "dd@Displays the current day of the month, measured as a number between 1 and 31, inclusive. If the day is a single digit only (1-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "ddd@Displays the abbreviated name of the day for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, then the AbbreviatedDayNames property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the AbbreviatedDayNames property from the specified format provider is used.\r\n" +
                                                "dddd@(plus any number of additional \"d\" characters)	Displays the full name of the day for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, then the DayNames property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the DayNames property from the specified format provider is used.\r\n" +
                                                "M@Displays the month, measured as a number between 1 and 12, inclusive. If the month is a single digit (1-9), it is displayed as a single digit.\r\n" +
                                                "MM@Displays the month, measured as a number between 1 and 12, inclusive. If the month is a single digit (1-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "MMM@Displays the abbreviated name of the month for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, the AbbreviatedMonthNames property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the AbbreviatedMonthNames property from the specified format provider is used.\r\n" +
                                                "MMMM@Displays the full name of the month for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, then the MonthNames property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the MonthNames property from the specified format provider is used.\r\n" +
                                                "y@Displays the year for the specified DateTime as a maximum two-digit number. The first two digits of the year are omitted. If the year is a single digit (1-9), it is displayed as a single digit.\r\n" +
                                                "yy@Displays the year for the specified DateTime as a maximum two-digit number. The first two digits of the year are omitted. If the year is a single digit (1-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "yyyy@Displays the year for the specified DateTime, including the century. If the year is less than four digits in length, then preceding zeros are appended as necessary to make the displayed year four digits long.\r\n" +
                                                "h@Displays the hour for the specified DateTime in the range 1-12. The hour represents whole hours passed since either midnight (displayed as 12) or noon (also displayed as 12). If this format is used alone, then the same hour before or after noon is indistinguishable. If the hour is a single digit (1-9), it is displayed as a single digit. No rounding occurs when displaying the hour. For example, a DateTime of 5:43 returns 5.\r\n" +
                                                "hh@(plus any number of additional \"h\" characters)	Displays the hour for the specified DateTime in the range 1-12. The hour represents whole hours passed since either midnight (displayed as 12) or noon (also displayed as 12). If this format is used alone, then the same hour before or after noon is indistinguishable. If the hour is a single digit (1-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "H@Displays the hour for the specified DateTime in the range 0-23. The hour represents whole hours passed since midnight (displayed as 0). If the hour is a single digit (0-9), it is displayed as a single digit.\r\n" +
                                                "HH@(plus any number of additional \"H\" characters)	Displays the hour for the specified DateTime in the range 0-23. The hour represents whole hours passed since midnight (displayed as 0). If the hour is a single digit (0-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "m@Displays the minute for the specified DateTime in the range 0-59. The minute represents whole minutes passed since the last hour. If the minute is a single digit (0-9), it is displayed as a single digit.\r\n" +
                                                "mm@(plus any number of additional \"m\" characters)	Displays the minute for the specified DateTime in the range 0-59. The minute represents whole minutes passed since the last hour. If the minute is a single digit (0-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "s@Displays the seconds for the specified DateTime in the range 0-59. The second represents whole seconds passed since the last minute. If the second is a single digit (0-9), it is displayed as a single digit only.\r\n" +
                                                "ss@(plus any number of additional \"s\" characters)	Displays the seconds for the specified DateTime in the range 0-59. The second represents whole seconds passed since the last minute. If the second is a single digit (0-9), it is formatted with a preceding 0 (01-09).\r\n" +
                                                "f@Displays seconds fractions represented in one digit.\r\n" +
                                                "ff@Displays seconds fractions represented in two digits.\r\n" +
                                                "fff@Displays seconds fractions represented in three digits.\r\n" +
                                                "ffff@Displays seconds fractions represented in four digits.\r\n" +
                                                "fffff@Displays seconds fractions represented in five digits.\r\n" +
                                                "ffffff@Displays seconds fractions represented in six digits.\r\n" +
                                                "fffffff@Displays seconds fractions represented in seven digits\r\n" +
                                                "t@Displays the first character of the A.M./P.M. designator for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, then the AMDesignator (or PMDesignator) property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the AMDesignator (or PMDesignator) property from the specified IFormatProvider is used. If the total number of whole hours passed for the specified DateTime is less than 12, then the AMDesignator is used. Otherwise, the PMDesignator is used.\r\n" +
                                                "tt@(plus any number of additional \"t\" characters)	Displays the A.M./P.M. designator for the specified DateTime. If a specific valid format provider (a non-null object that implements IFormatProvider with the expected property) is not supplied, then the AMDesignator (or PMDesignator) property of the DateTimeFormat and its current culture associated with the current thread is used. Otherwise, the AMDesignator (or PMDesignator) property from the specified IFormatProvider is used. If the total number of whole hours passed for the specified DateTime is less than 12, then the AMDesignator is used. Otherwise, the PMDesignator is used.";


        #endregion

        #region Class Members
        private static string _timeFormat = "Time Format:";
        private static string _tomorrowDateFormat = "Tomorrow's Date Format:";
        private static string _todayDateFormat = "Today's Date Format:";
        private static string _description = "Description";
        private static string _time = "Time";
        private static string _hour = "Hour";
        private static string _minute = "Minute";
        private static string _second = "Second";
        private string m_sHelpPath = string.Empty;


        #endregion

        #region Constructor

        public TimeFormat()
        {

            InitializeComponent();
            RTFFileName = "ClockAddinRTF.rtf";
        }
        public TimeFormat(WaspCulture CultureReader)
        {
            InitializeComponent();
            if (CultureReader != null)
                m_Culture = CultureReader;

        }
        #endregion
        #region Properties
        
        /// <summary>
        /// Sets the todays time format
        /// </summary>
        public string TodaysFormat
        {
            get { return m_sTodaysFormat; }
            set
            {
                m_sTodaysFormat = value;
            }
        }

        /// <summary>
        /// Sets the tomorrow time format
        /// </summary>
        public string TomorrowFormat
        {
            get { return m_sTomorrowFormat; }
            set
            {
                m_sTomorrowFormat = value;
            }
        }

        /// <summary>
        /// Sets the Selected time format
        /// </summary>
        public string CurrentTimeFormat
        {
            get { return m_sFormat; }
            set
            {
                m_sFormat = value;
            }
        }
        /// <summary>
        /// Sets the Selected time format
        /// </summary>
        public string Format
        {
            get { return m_sFormat + "," + m_sTodaysFormat + "," + m_sTomorrowFormat; }
            set
            {
                SetValue(value);
            }
        }

        public string AssemblyPath
        {
            get;
            set;
        }

        public string ResourceCulture
        {
            get;
            set;
        }

        public string RTFFileName
        {
            get;
            set;
        }

        public IWService WService
        {
            get;
            set;
        }

        #endregion

        #region Handled Events

        public void OnOkClick()
        {
            try
            {
                m_sFormat = txtFormat.Text;
                m_sTodaysFormat = txtbxTodaysFormat.Text;
                m_sTomorrowFormat = txtbxTomorrowsFormat.Text;
            }

            catch (Exception ex)
            {

            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool bStatus = false;
            try
            {
                if (keyData == Keys.Escape)
                {
                    bStatus = true;
                    // this.Close();
                }
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
            return bStatus;
        }


        /// <summary>
        /// When Selected Index for the listbox item Changes 
        /// Sets the current Value as the text of the Textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void lstFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFormats.SelectedIndex != -1)
                txtFormat.Text = lstFormats.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(txtFormat.Text))
                m_sFormat = txtFormat.Text;

            UpdateTime();
        }//end (lstFormats_SelectedIndexChanged)

        /// <summary>
        /// on TodaysDateFormat changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void lstbxTodaysFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstbxTodaysFormats.SelectedIndex != -1)
                txtbxTodaysFormat.Text = lstbxTodaysFormats.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(txtbxTodaysFormat.Text))
                m_sTodaysFormat = txtbxTodaysFormat.Text;
        }
        //end (lstbxTodaysFormats_SelectedIndexChanged)

        /// <summary>
        /// on TmrwDtFrmt changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void lstbxTomorrowsFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstbxTomorrowsFormat.SelectedIndex != -1)
                txtbxTomorrowsFormat.Text = lstbxTomorrowsFormat.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(txtbxTomorrowsFormat.Text))
                m_sTomorrowFormat = txtbxTomorrowsFormat.Text;
        }
        //end (lstbxTomorrowsFormat_SelectedIndexChanged)

        /// <summary>
        /// On Custom Format Selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtFormat.Text))
                    m_sFormat = txtFormat.Text;
                UpdateTime();
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
        }
        //end (txtFormat_TextChanged)
        #endregion

        #region Method

        internal void ShowHelp()
        {
            string sRTFFilePath = string.Empty;
            try
            {
                if (WService != null && !string.IsNullOrEmpty(AssemblyPath) && !string.IsNullOrEmpty(ResourceCulture) && !string.IsNullOrEmpty(RTFFileName))
                {
                    sRTFFilePath = Path.Combine(new string[] { AssemblyPath, "Culture", ResourceCulture, RTFFileName });
                    WService.ShowHelp(sRTFFilePath, true);
                }

            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
        }

        /// <summary>
        /// Initialize with help file path
        /// </summary>
        /// <param name="sHelpFilePath"></param>
        private void UpdateUI()
        {
            this.lblTimeFormat.Text = Culture.GetResourceValue(ClockAddinConstants.lblTime);
            this.lblTmrwTimeFrmt.Text = Culture.GetResourceValue(ClockAddinConstants.lblTomorrowTime);
            this.lblTodaysTimeFrmt.Text = Culture.GetResourceValue(ClockAddinConstants.lblTodaysTime);
        }//end (UpdateUI)

        public void InitHelp(IWService obj)
        {
            m_objIWService = obj;
            WService = obj;

            try
            {

                m_dtHelp = new DataTable();
                m_dtHelp.Columns.Add("Format");
                m_dtHelp.Columns.Add("Description");

                string[] formats = FORMATHELP.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                foreach (string frmt in formats)
                {
                    string[] description = frmt.Split('@');

                    DataRow dr = m_dtHelp.NewRow();
                    dr[0] = description[0];
                    dr[1] = description[1];

                    m_dtHelp.Rows.Add(dr);
                }
                SetAppearenceColor(obj);
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
            //dataGridView2.DataSource = m_dtHelp;
        }//end (InitHelp)
        BeeSys.Wasp3D.Hosting.Appearance _objAppearance = null;
        private void SetAppearenceColor(IWService m_IService)
        {
            try
            {
                _objAppearance = m_IService?.GetAppearance();
                if (_objAppearance != null &&
                    string.Compare(_objAppearance.Theme, "dark", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.panel1.SuspendLayout();
                    this.panelBottom.SuspendLayout();
                    this.tblpnlCntnr.SuspendLayout();
                    this.SuspendLayout();
                    SetWindowTheme(lstbxTomorrowsFormat.Handle, "DarkMode_Explorer", null);
                    SetWindowTheme(lstbxTodaysFormats.Handle, "DarkMode_Explorer", null);
                    dataGridView1.BackgroundColor = _objAppearance.BackColor;
                    dataGridView1.ForeColor = _objAppearance.ForeColor;
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = _objAppearance.BackColor;
                    dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = _objAppearance.ForeColor;
                    dataGridView1.EnableHeadersVisualStyles = false;
                    dataGridView1.DefaultCellStyle.BackColor = _objAppearance.BackColor;
                    dataGridView1.DefaultCellStyle.ForeColor = _objAppearance.ForeColor;
                    dataGridView1.DefaultCellStyle.SelectionBackColor = _objAppearance.BackColor;
                    dataGridView1.DefaultCellStyle.SelectionForeColor = _objAppearance.ForeColor;
                    dataGridView1.RowsDefaultCellStyle.BackColor = _objAppearance.BackColor;



                    panel1.ForeColor = _objAppearance.ForeColor;
                    panel1.BackColor = _objAppearance.BackColor;

                    panelBottom.ForeColor = _objAppearance.ForeColor;
                    panelBottom.BackColor = _objAppearance.BackColor;

                    tblpnlCntnr.ForeColor = _objAppearance.ForeColor;
                    tblpnlCntnr.BackColor = _objAppearance.BackColor;

                    lstbxTodaysFormats.ForeColor = _objAppearance.ForeColor;
                    lstbxTodaysFormats.BackColor = _objAppearance.BackColor;

                    lblTimeFormat.ForeColor = _objAppearance.ForeColor;
                    lblTimeFormat.BackColor = _objAppearance.BackColor;

                    lstFormats.ForeColor = _objAppearance.ForeColor;
                    lstFormats.BackColor = _objAppearance.BackColor;

                    lstbxTomorrowsFormat.ForeColor = _objAppearance.ForeColor;
                    lstbxTomorrowsFormat.BackColor = _objAppearance.BackColor;

                    lblTodaysTimeFrmt.ForeColor = _objAppearance.ForeColor;
                    lblTodaysTimeFrmt.BackColor = _objAppearance.BackColor;

                    txtbxTodaysFormat.BackColor = _objAppearance.BackColor;
                    txtbxTodaysFormat.ForeColor = _objAppearance.ForeColor;


                    txtbxTomorrowsFormat.BackColor = _objAppearance.BackColor;
                    txtbxTomorrowsFormat.ForeColor = _objAppearance.ForeColor;

                    txtFormat.ForeColor = _objAppearance.ForeColor;
                    txtFormat.BackColor = _objAppearance.BackColor;

                    lblTmrwTimeFrmt.ForeColor = _objAppearance.ForeColor;
                    lblTmrwTimeFrmt.BackColor = _objAppearance.BackColor;

                    this.BackColor = _objAppearance.BackColor;
                    this.ForeColor = _objAppearance.ForeColor;

                    this.panel1.ResumeLayout(false);
                    this.panelBottom.ResumeLayout(false);
                    this.tblpnlCntnr.ResumeLayout(false);
                    this.tblpnlCntnr.PerformLayout();
                    this.ResumeLayout(false);
                }


            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ex);
            }
            finally
            {
                this.panel1.ResumeLayout(false);
                this.panelBottom.ResumeLayout(false);
                this.tblpnlCntnr.ResumeLayout(false);
                this.tblpnlCntnr.PerformLayout();
                this.ResumeLayout(false);
            }
        }

        /// <summary>
        /// Get Updated Time.
        /// Calculate Degree Equivalent Of Time.
        /// Update the Values in Wired Datatable.
        /// </summary>
        private void UpdateTime()
        {
            DataRow datarow;
            float fDegreeHour, fDegreeMin, fDegreeSecond = 0;
            int iHour, iMinute, iSecond = 0;
            int iRem;
            int iDiv;
            string sTime = DateTime.Now.ToString(m_sFormat);

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
            datarow = m_dtClock.Rows[0];
            datarow[ClockAddinConstants.TIME] = sTime;
            datarow[ClockAddinConstants.HOUR] = fDegreeHour;
            datarow[ClockAddinConstants.MINUTE] = fDegreeMin;
            datarow[ClockAddinConstants.SECOND] = fDegreeSecond;
            m_dtClock.AcceptChanges();
        }//end (UpdateTime)

        /// <summary>
        /// Sets the Last Selected Format
        /// </summary>
        /// <param name="svalue"></param>
        private void SetValue(string svalue)
        {
            string[] sarrFormats = null;
            int iIndex = -1;
            try
            {
                if (!string.IsNullOrEmpty(svalue))
                    sarrFormats = svalue.Split(',');
                if (sarrFormats != null && sarrFormats.Length >= 2)
                {
                    CurrentTimeFormat = sarrFormats[0];
                    TodaysFormat = sarrFormats[1];
                    TomorrowFormat = sarrFormats[2];

                    iIndex = lstFormats.Items.IndexOf(sarrFormats[0]);

                    if (iIndex > -1)
                        lstFormats.SelectedIndex = iIndex;
                    else
                        txtFormat.Text = sarrFormats[0];
                    iIndex = -1;

                    iIndex = lstbxTodaysFormats.Items.IndexOf(sarrFormats[1]);

                    //Todays Format
                    if (iIndex > -1)
                        lstbxTodaysFormats.SelectedIndex = iIndex;
                    else
                        txtbxTodaysFormat.Text = sarrFormats[1]; 

                    iIndex = -1;
                    //Tomorrow date format
                    iIndex = lstbxTomorrowsFormat.Items.IndexOf(sarrFormats[2]);
                    if (iIndex > -1)
                        lstbxTomorrowsFormat.SelectedIndex = iIndex;
                    else
                        txtbxTomorrowsFormat.Text = sarrFormats[2]; 
                }
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
        }//end (SetValue)

        /// <summary>
        /// Start Thread to update time in grid
        /// </summary>
        private void InitThread()
        {
            try
            {
                m_thUpdateTime = new Thread(new ThreadStart(delegate
                    {
                        SetTime();
                    }));
                m_thUpdateTime.IsBackground = true;
                m_thUpdateTime.Priority = ThreadPriority.Normal;
                m_thUpdateTime.Start();
            }
            finally
            {
            }
        }//end (InitThread)

        /// <summary>
        /// Call Update time
        /// </summary>
        private void SetTime()
        {
            try
            {
                while (!m_bFlag)
                {
                    UpdateTime();
                    Thread.Sleep(25);
                }
            }
            finally
            {
            }
        }//end (SetTime)

        /// <summary>
        /// On ShutDown
        /// </summary>
        public void ShutDown()
        {
            try
            {
                m_bFlag = true;
                m_thUpdateTime = null;
                m_dtClock = null;
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
        }

        /// <summary>
        ///  
        /// </summary>
        public  void Internationalization(WaspCulture CultureReader)//S.No.: -06
        {
            try
            {
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
                m_Culture = CultureReader;
                m_dtClock = new DataTable();
                m_dtClock.Columns.Add(ClockAddinConstants.TIME);
                m_dtClock.Columns.Add(ClockAddinConstants.HOUR);
                m_dtClock.Columns.Add(ClockAddinConstants.MINUTE);
                m_dtClock.Columns.Add(ClockAddinConstants.SECOND);
                DataRow dr = m_dtClock.NewRow();
                m_dtClock.Rows.Add(dr);
                dataGridView1.DataSource = m_dtClock;
                if (CultureReader != null && CultureReader.FileExist)
                {

                    dataGridView1.DataSource = m_dtClock;

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("timeformat")))
                        _timeFormat = CultureReader.GetResourceValue("timeformat");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("tomorrowDateFormat")))
                        _tomorrowDateFormat = CultureReader.GetResourceValue("tomorrowDateFormat");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("todayDateFormat")))
                        _todayDateFormat = CultureReader.GetResourceValue("todayDateFormat");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("description")))
                        _description = CultureReader.GetResourceValue("description");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("time")))
                        _time = CultureReader.GetResourceValue("time");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("hour")))
                        _hour = CultureReader.GetResourceValue("hour");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("minute")))
                        _minute = CultureReader.GetResourceValue("minute");

                    if (!string.IsNullOrEmpty(CultureReader.GetResourceValue("second")))
                        _second = CultureReader.GetResourceValue("second");
                }
                lblTimeFormat.Text = _timeFormat;
                lblTmrwTimeFrmt.Text = _tomorrowDateFormat;
                lblTodaysTimeFrmt.Text = _todayDateFormat;
                dataGridView1.Columns[0].HeaderText = _time;
                dataGridView1.Columns[1].HeaderText = _hour;
                dataGridView1.Columns[2].HeaderText = _minute;
                dataGridView1.Columns[3].HeaderText = _second;
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
            finally
            {

            }
        }
        #endregion

        internal void SetTextBoxValues()
        {

        }
    }
}