using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeSys.Wasp3D.Hosting;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Beesys.Wasp.Workflow;

namespace Beesys.Wasp.AddIn
{
    public class ClockAddInProvider : WProvider
    {

        #region Class Members

        private List<AddinInfo> m_lstAddinInfo = null;
        private List<IWContainer> m_lstContainer = null;
        private static string m_sAssemblyPath = string.Empty;
        private string addinName = "Clock";
        #endregion

        #region constructor

        static ClockAddInProvider()
        {
            string sLocation = string.Empty;
            string sDirectoryName = string.Empty;
            try
            {
                sLocation = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(sLocation))
                {
                   sDirectoryName = Path.GetDirectoryName(sLocation);
                    if (!string.IsNullOrEmpty(sDirectoryName))
                        m_sAssemblyPath = sDirectoryName;
                }
            }
            catch(Exception ex)
            {
                    Beesys.Wasp.Workflow.LogWriter.WriteLog(ex);
            }
            finally
            {
                sLocation = null;
                sDirectoryName = null;
            }
        }

        public ClockAddInProvider()
        {
            try
            {
                AssemblyPath = m_sAssemblyPath;
            }
            catch (Exception ex)
            {
                Workflow.LogWriter.WriteLog(ex);
            }
        }

        #endregion

        #region Properties

        public string DefaultCulture
        {
            get;
            set;

        }

        public string AssemblyPath
        {
            get;
            set;
        }
        public Beesys.Wasp.Workflow.WaspCulture CultureReader
        {
            get;
            private set;
        }

        /// <summary>
        /// Override to return type of addin contains by provider 
        /// </summary>
        public override AddinInfo[] AddinsType
        {
            get
            {
                return m_lstAddinInfo.ToArray();
            }
        }

        /// <summary>
        /// Override the type of addin to be created .
        /// Set as DataTable, as single table created having time information.
        /// </summary>
        public override WProviderType Type
        {
            get
            {
                return WProviderType.DataTable;
            }
        }

        /// <summary>
        /// override Singleton to determine whether single addin to be created or not.
        /// return false as multiple addin in local scene can be created.
        /// </summary>
        public override bool Singleton
        {
            get
            {
                return false;
            }
        }
       
        #endregion

        #region Overrided Methods

        /// <summary>
        /// Override Delete container to call containers shut down and remove container
        /// from list
        /// </summary>
        /// <param name="container"></param>
        public override void DeleteContainer(IWContainer container)
        {
            if (container != null)
            {
                if (m_lstContainer != null)
                {
                    if (m_lstContainer.Contains(container))
                    {
                        container.ShutDown();
                        m_lstContainer.Remove(container);
                    }
                }
            }
            container = null;
        }

        /// <summary>
        /// Return Conatiner Object, for new scene
        /// </summary>
        /// <returns></returns>
        public override IWContainer GetContainer()
        {
            ClockAddInContainer objClockAddInContainer = null;
            try
            {
                objClockAddInContainer = new ClockAddInContainer();
                objClockAddInContainer.Provider = this;
                objClockAddInContainer.Engine = Engine;

                if (m_lstContainer != null)
                {
                    m_lstContainer.Add(objClockAddInContainer);
                }
                return objClockAddInContainer;
            }
            finally
            {
                objClockAddInContainer = null;
            }     
        }

        /// <summary>
        /// On Addin Shutdown, release the resources by calling containers shut down
        /// </summary>
        public override void ShutDown()
        {
            try
            {
                if (m_lstContainer != null)
                {
                    foreach (ClockAddInContainer objContainer in m_lstContainer)
                    {
                        objContainer.ShutDown();
                    }
                    m_lstContainer.Clear();
                    m_lstContainer = null;
                }
                Engine = null;
            }
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ClockAddinConstants.MODULENAME, ex);   
            }
        }

        /// <summary>
        /// On Addin start up
        /// Set the culture file for addin
        /// Set the addin type info
        /// </summary>
        public override void StartUp()
        {
            try
            {
                InitCultureReader();
                WaspCulture m_WaspCulture = null;
                if (m_WaspCulture == null)
                {
                    m_WaspCulture = this.CultureReader;
                }
                if (!string.IsNullOrEmpty(m_WaspCulture.GetResourceValue("clock")))
                    addinName = m_WaspCulture.GetResourceValue("clock");
                
                m_lstContainer = new List<IWContainer>();
                m_lstAddinInfo = new List<AddinInfo>();
                m_lstAddinInfo.Add(new AddinInfo() { AddinType = "Clock", Button = true, ButtonCaption = addinName });
            } //End(try)
            catch (Exception ex)
            {
                Beesys.Wasp.Workflow.LogWriter.WriteLog(ClockAddinConstants.MODULENAME, ex);   
            }
        }//End(StartUp())

        #endregion


        public virtual void InitCultureReader()
        {
            try
            {
                if (CultureReader == null)
                {
                    if (!string.IsNullOrEmpty(this.CurrentCulture))
                        DefaultCulture = this.CurrentCulture;
                    if (!string.IsNullOrEmpty(AssemblyPath))
                    {
                        CultureReader = new Beesys.Wasp.Workflow.WaspCulture();
                        CultureReader.Initialize(AssemblyPath, DefaultCulture);
                    }


                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
