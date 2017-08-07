using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using Inventor;
using InvAddIn.View;
using InvAddIn.Model;
using InvAddIn.ViewModel;

namespace AutoBreaker
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("9b6422da-2e2b-40df-9427-04dd03bdb56b")]
    public partial class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        private Inventor.Application m_inventorApplication;

        // Icons objects
        object plus16obj, plus128obj, gear16obj, gear128obj;

        // guid string
        string addInGuid = "9b6422da-2e2b-40df-9427-04dd03bdb56b";

        // UI members
        CommandManager cmdMan;
        ControlDefinitions ctrlDefs;
        CommandCategory cmdCat;
        Ribbon drawingRibbon;
        RibbonTab placeTab;
        ButtonDefinition applyButton;
        ButtonDefinition settingsButton;
        RibbonPanel panel;
        CommandControl controlApplyBreak;
        CommandControl controlSettingsBreak;

        //add-in model
        ISettingsModel model;
        IInventorHandler handler;

        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application;

            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.

            // Initialize add-in model
            model = new SettingsModel();
            handler = new InventorHandler(m_inventorApplication, model);            

            // get icon objects
            getIcons();

            // modify the ribbon
            modifyRibbon();
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects.
            m_inventorApplication = null;
            model = null;
            plus16obj = null;
            plus128obj = null;
            gear16obj = null;
            gear128obj = null;

            cmdMan = null;
            ctrlDefs = null;
            cmdCat.Delete();
            cmdCat = null;
            drawingRibbon = null;
            placeTab = null;
            applyButton.Delete();
            applyButton = null;
            settingsButton.Delete();
            settingsButton = null;
            panel.Delete();
            panel = null;
            controlApplyBreak.Delete();
            controlApplyBreak = null;
            controlSettingsBreak.Delete();
            controlSettingsBreak = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        /// <summary>
        /// gets icons from embedded resources and converts them to objects 
        /// </summary>
        private void getIcons()
        {
            //get current assembly
            Assembly thisDll = Assembly.GetExecutingAssembly();

            //get icon streams
            Stream plus16stream = thisDll.GetManifestResourceStream("InvAddIn.img.plus16.ico");
            Stream plus128stream = thisDll.GetManifestResourceStream("InvAddIn.img.plus128.ico");
            Stream gear16stream = thisDll.GetManifestResourceStream("InvAddIn.img.gear16.ico");
            Stream gear128stream = thisDll.GetManifestResourceStream("InvAddIn.img.gear16.ico");

            //get icons
            Icon plus16icon = new Icon(plus16stream);
            Icon plus128icon = new Icon(plus128stream);
            Icon gear16icon = new Icon(gear16stream);
            Icon gear128icon = new Icon(gear128stream);

            //convert to objects
            plus16obj = AxHostConverter.ImageToPictureDisp(plus16icon.ToBitmap());
            plus128obj = AxHostConverter.ImageToPictureDisp(plus128icon.ToBitmap());
            gear16obj = AxHostConverter.ImageToPictureDisp(gear16icon.ToBitmap());
            gear128obj = AxHostConverter.ImageToPictureDisp(gear128icon.ToBitmap());
        }

        /// <summary>
        /// modifies Drawing ribbon by adding two buttons used by the add-in
        /// </summary>
        private void modifyRibbon()
        {
            //get Command manager
            cmdMan = m_inventorApplication.CommandManager;

            //get control definitions
            ctrlDefs = cmdMan.ControlDefinitions;

            //define command category for add-in's buttons
            cmdCat = cmdMan.CommandCategories.Add("Auto-Breaker", "Autodesk:CmdCategory:AutoBreaker", addInGuid);

            //get 'Drawing' ribbon
            drawingRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Drawing"];

            //get 'Place Views' tab from 'Drawing' ribbon
            placeTab = drawingRibbon.RibbonTabs["id_TabPlaceViews"];

            //define 'Apply break' button
            applyButton = ctrlDefs.AddButtonDefinition("Apply!", "Autodesk:AutoBreaker:ApplyButton", CommandTypesEnum.kQueryOnlyCmdType, addInGuid, "auto-break description", "auto-break tooltip", plus16obj, plus128obj, ButtonDisplayEnum.kAlwaysDisplayText);
            applyButton.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(customAction);
            cmdCat.Add(applyButton);

            //define 'Settings' button
            settingsButton = ctrlDefs.AddButtonDefinition("Settings", "Autodesk:AutoBreaker:SettingsButton", CommandTypesEnum.kQueryOnlyCmdType, addInGuid, "auto-breaker settings description", "auto-break settings tool-tip", gear16obj, gear128obj, ButtonDisplayEnum.kAlwaysDisplayText);
            settingsButton.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(settingsClick);
            cmdCat.Add(settingsButton);

            //define panel in 'Place Views' tab
            panel = placeTab.RibbonPanels.Add("Auto-Breaker", "Autodesk:AutoBreaker:AutoBreakerPanel", addInGuid);
            controlApplyBreak = panel.CommandControls.AddButton(applyButton, true, true);
            controlSettingsBreak = panel.CommandControls.AddButton(settingsButton, true, true);
        }

        private void customAction(NameValueMap options)
        {
            handler.AutoBreak();
        }

        private void settingsClick(NameValueMap options)
        {
            SettingsView settingWindow = new SettingsView();
            settingWindow.DataContext = new SettingsViewModel(model);
            settingWindow.ShowDialog();
        }
        #endregion
    }
}
