using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeSys.Wasp3D.Hosting;
using BeeSys.Wasp3D.HostingX;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Beesys.Wasp.AddIn
{
    public class ClockAddInContainer:WContainer
    {

        #region Class Members

        private                           List<IWAddIn>                                 m_lstAddins;
        private                           Scene                                         m_ObjSceneGraph;
        private                           IWService                                     m_WService          = null;
        private                           IWHost                                        m_WHost             = null;
       
        private                           const string                                  XMLSTRING = "<data><addins></addins></data>";

        #endregion

        #region Constructor

        //  S.No.: -			03
        public ClockAddInContainer()
        {
            if (m_lstAddins == null)
                m_lstAddins = new List<IWAddIn>();
        }

        #endregion

        #region Properties

        public override Scene SceneGraph
        {
            get
            {
                return m_ObjSceneGraph;
            }
            set
            {
                m_ObjSceneGraph = value;
                UpdateAddinSceneGraph(); //S.No.: -			10
            }
        }

        public override IWHost Host
        {
            get
            {
                return m_WHost;
            }
            set
            {
                m_WHost = value;
                m_WService = m_WHost.GetService(typeof(IWService)) as IWService;
                UpdateAddins();
            }
        }

        #endregion

        #region Overrided Methods

        /// <summary>
        /// Delete addin if available in list
        /// </summary>
        /// <param name="addin"></param>
        public override void DeleteAddin(IWAddIn addin)
        {
            try
            {
                if (m_lstAddins != null && m_lstAddins.Contains(addin))
                {
                    if (addin is ClockAddIn)
                        ((ClockAddIn)addin).OnBeforeShutDown();
                    addin.ShutDown();
                    m_lstAddins.Remove(addin);
                    addin = null;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }

        /// <summary>
        /// Return List of addins
        /// </summary>
        /// <returns></returns>
        public override List<IWAddIn> GetAddIns()
        {
            return m_lstAddins;
        }

        /// <summary>
        /// Return new addin object
        /// </summary>
        /// <param name="addinType"></param>
        /// <returns></returns>
        public override IWAddIn GetAddin(string addinType)
        {
            ClockAddIn objClockAddIn = null;
            try
            {
                objClockAddIn = new ClockAddIn();
                objClockAddIn.Provider = Provider;
                objClockAddIn.Engine = Engine;
                objClockAddIn.SceneGraph = m_ObjSceneGraph;
                objClockAddIn.TableName = "Clock";
                objClockAddIn.Index = 0;
                objClockAddIn.RowsPerPage = 1;
                if (m_WService != null)
                    objClockAddIn.Service = m_WService;
                if (m_lstAddins != null)
                    m_lstAddins.Add(objClockAddIn);

                return objClockAddIn;
            }
            finally
            {
                objClockAddIn = null;
            }            
        }

        /// <summary>
        /// On Scenegraph opened load is called 
        /// Read xml and craete addin objects
        /// </summary>
        /// <param name="sgKey"></param>
        /// <param name="data"></param>
        public override void Load(string sgKey, string data)
        {
            IEnumerable<XElement> xeAddins = null;
            ClockAddIn objAddin = null;
            XDocument xdocAddinDetail = null;
            try
            {
                if (!string.IsNullOrEmpty(data))
                {
                    xdocAddinDetail = XDocument.Parse(data);
                    if (xdocAddinDetail != null)
                    {

                        if (m_lstAddins != null)
                        {
                            xeAddins = xdocAddinDetail.XPathSelectElements("data//addins/addin");
                            if (xeAddins != null && xeAddins.Count() > 0)
                            {
                                foreach (XElement objxem in xeAddins)
                                {
                                    objAddin = new ClockAddIn();
                                    objAddin.Provider = Provider;
                                    objAddin.Engine = Engine;
                                    objAddin.SceneGraph = m_ObjSceneGraph;
                                    objAddin.TableName = "Clock";
                                    objAddin.Index = 0;
                                    objAddin.RowsPerPage = 1;
                                 
                                    if (objxem.Attribute("addinid") != null &&
                                        objxem.Attribute("addinid").Value != null)
                                    {
                                        objAddin.InstanceId = objxem.Attribute("addinid").Value;
                                    }
                                    if (objxem.Attribute("name") != null &&
                                        objxem.Attribute("name").Value != null)
                                    {
                                        objAddin.Name = objxem.Attribute("name").Value;
                                    }
                                    if (objxem.Attribute("type") != null &&
                                        objxem.Attribute("type").Value != null)
                                    {
                                        objAddin.SetAddinType(objxem.Attribute("type").Value);
                                    }
                                    if (objxem.Value != null)
                                    {
                                        objAddin.Config = objxem.Value;
                                    }
                                    objAddin.StartUp();
                                    m_lstAddins.Add(objAddin);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                xeAddins = null;
                objAddin = null;
            }
        }

        /// <summary>
        /// on Scene Closed save addin info in xml and return in string format
        /// </summary>
        /// <param name="globalData"></param>
        /// <returns></returns>
        public override string Save(List<GlobalData> globalData)
        {
            XDocument xdocAddinDetail   = null;
            string sOnScrollerXml = string.Empty;
            string sOnContentEndXml = string.Empty;
            string sEditorXml = string.Empty;
            XElement xeAddins = null;
            try
            {
                xdocAddinDetail = XDocument.Parse(XMLSTRING);
                if (xdocAddinDetail != null)
                {
                    xeAddins = XElement.Parse("<addins></addins>");
                    xdocAddinDetail.Root.Add(xeAddins);
                    GetAddinXml(xeAddins);
                }
                return xdocAddinDetail.ToString();
            }
            finally
            {
                xeAddins = null;
            }          
        }

        /// <summary>
        /// When addins are initialized startup of container is called
        /// </summary>
        public override void StartUp()
        {
           
        }

        /// <summary>
        /// on addin Shutdown
        /// </summary>
        public override void ShutDown()
        {
            try
            {
                if (m_lstAddins != null)
                {

                    foreach(IWAddIn addin in m_lstAddins) //S.No.: -			07
                    {
                        if (addin != null)
                        {
                            if (addin is ClockAddIn)
                                ((ClockAddIn)addin).OnBeforeShutDown();
                            addin.ShutDown();
                    }
                    }
                    // S.No.: -			02
                    m_lstAddins.Clear();

                    
                    m_lstAddins = null;
                }
                m_ObjSceneGraph = null;
                m_WService = null;
                m_WHost = null;
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ClockAddinConstants.MODULENAME, ex);   
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// create node for every addin
        /// store data of addin in xml
        /// </summary>
        /// <param name="xeAddins"></param>
        private void GetAddinXml(XElement xeAddins)
        {
            XElement xeAddin = null;
            XCData xeCdata = null;
            try
            {
                if (m_lstAddins != null && m_lstAddins.Count > 0)
                {
                    foreach (IWAddIn objAddin in m_lstAddins)
                    {
                        xeAddin = new XElement("addin");
                        xeAddin.SetAttributeValue("addinid", objAddin.InstanceId);
                        xeAddin.SetAttributeValue("name", objAddin.Name);
                        xeAddin.SetAttributeValue("type", objAddin.AddinType);
                        if (!string.IsNullOrEmpty(objAddin.Config))
                        {
                            xeCdata = new XCData(objAddin.Config);
                            if (xeCdata != null)
                                xeAddin.Add(xeCdata);
                        }
                        xeAddins.Add(xeAddin);
                    }
                }
            }
            finally
            {
                xeAddin = null;
                xeCdata = null;
            }
        }

        /// <summary>
        /// Update addins when IWservice object is set
        /// </summary>
        private void UpdateAddins()
        {
            try
            {
                if (m_lstAddins != null && m_lstAddins.Count > 0)
                {
                    foreach (IWAddIn objAddin in m_lstAddins)
                    {
                        (objAddin as ClockAddIn).Service = m_WService;
                    }
                }
            }
            finally
            {

            }
        }


        private void UpdateAddinSceneGraph() //S.No.: -			10
        {
            try
            {
                if (m_lstAddins != null && m_lstAddins.Count > 0)
                {
                    foreach (IWAddIn objAddin in m_lstAddins)
                    {
                        
                        (objAddin as ClockAddIn).SceneGraph = m_ObjSceneGraph;
                    }
                }
            }
            catch(Exception ex)
            {
                LogWriter.WriteLog(ex);
            }
        }

        #endregion
    }
}
