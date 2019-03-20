using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeSys.Wasp3D.Hosting;
using GFXWrpSceneGraph;
using Beesys.Wasp.Workflow;

namespace Beesys.Wasp.AddIn
{
    //Host Class For ClockAddIn.
    public class ClockAddInHost : MarshalByRefObject, IWaspAddIn, IWaspHostWrapper
    {

        #region Class Variables

     
        CPlayoutEngine m_ObjEngine;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Type Of AddIn
        /// </summary>
        public string AddInType
        {
            get { return ClockAddinConstants.DATASET; }
        }

        /// <summary>
        /// Maintains the details of addin
        /// </summary>
        public string ConfigString
        {
            get;
            set;
        }

        /// <summary>
        /// This property is used to get the Add-in description.
        /// </summary>
        /// <remarks>
        /// It is a readonly property of IWaspAddIn Interface.
        /// </remarks>
        public string Description
        {
            get { return "Clock AddIn"; }
        }

        /// <summary>
        /// Get Provider Version
        /// </summary>
        public string Version
        {
            get { return "4.0.0.0"; }
        }

        /// <summary>
        /// Get HelpString for Provider
        /// </summary>
        public string HelpString
        {
            get { return "Clock AddIn"; }
        }

        /// <summary>
        /// Name Of addIn
        /// </summary>
        public string Name
        {
            get { return ClockAddinConstants.CLOCK ; }
            set { }
        }

        /// <summary>
        /// Return the Name of the AddIn object
        /// </summary>
        public string ObjectName
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        ///Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }//end (InitializeLifetimeService)

        /// <summary>
        /// 
        /// </summary>
        public void ExecuteMethod()
        {
        }//end (ExecuteMethod)

        /// <summary>
        /// Return the object of Add-IN to engine.
        /// </summary>
        /// <param name="sName"></param>
        /// <returns></returns>
        public object GetObject(string sName)
        {
            ClockAddIn objClockAddIn = new ClockAddIn();
            if (m_ObjEngine != null)
                objClockAddIn.SetEngine(m_ObjEngine);
            if (m_objHost != null)
            {
                INameCreationService objINameCreationService = m_objHost.GetService(DesignerService.INameCreationService) as INameCreationService;
                if (objINameCreationService != null)
                {
                    objClockAddIn.SetEngine(m_ObjEngine);
                    objClockAddIn.ObjectName = objINameCreationService.GetName(this.Name);
                    return objClockAddIn;
                }
            }
            return objClockAddIn;
        }//end (GetObject)

        /// <summary>
        /// Initialize the addin
        /// </summary>
        /// <param name="config"></param>
        public void Init(string sConfigString)
        {
        }//end (Init)

        /// <summary>
        /// Recieves the object of engine
        /// </summary>
        /// <param name="objPlayoutEngine"></param>
        public void SetEngine(GFXWrpSceneGraph.CPlayoutEngine objPlayoutEngine)
        {
            m_ObjEngine = objPlayoutEngine;
        }//end (SetEngine)

        /// <summary>
        /// 
        /// </summary>
        public void ShowEditor()
        {
        }//end (ShowEditor)

        /// <summary>
        /// Memory CleanUp on server shutdown 
        /// </summary>
        public void ShutDown()
        {
           m_objHost = null;
           m_objSrvc = null;
        }//end (ShutDown)

        /// <summary>
        /// Calls When Container Object Created
        /// </summary>
        public void StartUp()
        {
        }//end (StartUp)

        #endregion

        // IWaspHostWrapper Interface is used to Create a Button in Drone Designer 
        #region IWaspHostWrapper Interface

        private IWaspHost m_objHost;
        private IToolButtonService m_objSrvc;

        /// <summary>
        /// Returns IWaspHost Object
        /// </summary>
        public IWaspHost WaspHost
        {
            set 
            { 
                m_objHost = value;
                //get IToolButtonService object
                m_objSrvc = (IToolButtonService)m_objHost.GetService(DesignerService.IToolButtonService);
                //Creates a Button in drone designer
                m_objSrvc.CreateButton(new WaspButton(ClockAddinConstants.CLOCK, ClockAddinConstants.CLOCK), WindowType.DataSource, false);
            }
        }

        #endregion
    }
}
