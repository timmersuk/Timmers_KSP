using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

public abstract class ConfigNodeStorage : IPersistenceLoad, IPersistenceSave
{
    #region Constructors
    /// <summary>
    /// Class Constructor
    /// </summary>
    public ConfigNodeStorage()
    {
        configNodeName = this.GetType().Name;
    }

    /// <summary>
    /// Class Constructor
    /// </summary>
    public ConfigNodeStorage(string name)
    {
        configNodeName = name;
    }
    #endregion

    #region Properties
    private readonly string configNodeName;
    #endregion

    #region Interface Methods
    /// <summary>
    /// Wrapper for our overridable functions
    /// </summary>
    void IPersistenceLoad.PersistenceLoad()
    {
        OnDecodeFromConfigNode();
    }
    /// <summary>
    /// Wrapper for our overridable functions
    /// </summary>
    void IPersistenceSave.PersistenceSave()
    {
        OnEncodeToConfigNode();
    }

    /// <summary>
    /// This overridable function executes whenever the object is loaded from a config node structure. Use this for complex classes that need decoding from simple confignode values
    /// </summary>
    public virtual void OnDecodeFromConfigNode() { }
    /// <summary>
    /// This overridable function executes whenever the object is encoded to a config node structure. Use this for complex classes that need encoding into simple confignode values
    /// </summary>
    public virtual void OnEncodeToConfigNode() { }
    #endregion

 

    /// <summary>
    /// Loads the object from 
    /// </summary>
    /// <param name="cnToLoad"></param>
    /// <returns></returns>
    public virtual bool Load(ConfigNode cnToLoad, bool wrapped)
    {
        if (cnToLoad == null)
        {
            return false;
        }

        KeepFit.Logging.Log_DebugOnly(this, "Load", "configNodeName[{0}]", configNodeName);

        //remove the wrapper node that names the class stored
        ConfigNode cnUnwrapped = (wrapped ? cnToLoad.GetNode(configNodeName) : cnToLoad);
        if (cnUnwrapped == null)
        {
            return false;
        }

        KeepFit.Logging.Log_DebugOnly(this, "Load", "cnUnwrapped[{0}]", cnUnwrapped);

        //plug it in to the object
        bool succeeded = ConfigNode.LoadObjectFromConfig(this, cnUnwrapped);

        KeepFit.Logging.Log_DebugOnly(this, "Load", "load complete [{0}]", (succeeded ? "succeeded" : "failed"));
        return true;
    }

    public virtual bool Save(ConfigNode parent)
    {
        KeepFit.Logging.Log_DebugOnly(this, "Save", "parent [{0}]", parent);

        try
        {
            ConfigNode cnTemp = new ConfigNode(configNodeName);
            ConfigNode cnSaveWrapper = ConfigNode.CreateConfigFromObject(this, cnTemp);

            KeepFit.Logging.Log_DebugOnly(this, "Save", "saving node [{0}]", cnSaveWrapper);

            parent.AddNode(cnSaveWrapper);

            return true;
        }
        catch (Exception ex)
        {
            LogFormatted("Failed to Save ConfigNode - Error:{1}", ex.Message);
            return false;
        }
    }
   



    #region Assembly/Class Information
    /// <summary>
    /// Name of the Assembly that is running this MonoBehaviour
    /// </summary>
    internal static string _AssemblyName
    { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }

    /// <summary>
    /// Full Path of the executing Assembly
    /// </summary>
    internal static string _AssemblyLocation
    { get { return System.Reflection.Assembly.GetExecutingAssembly().Location; } }

    /// <summary>
    /// Folder containing the executing Assembly
    /// </summary>
    internal static string _AssemblyFolder
    { get { return System.IO.Path.GetDirectoryName(_AssemblyLocation); } }

    #endregion  

    #region Logging
    /// <summary>
    /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
    /// </summary>
    /// <param name="Message">Text to be printed - can be formatted as per string.format</param>
    /// <param name="strParams">Objects to feed into a string.format</param>
    [System.Diagnostics.Conditional("DEBUG")]
    internal static void LogFormatted_DebugOnly(string Message, params object[] strParams)
    {
        LogFormatted(Message, strParams);
    }

    /// <summary>
    /// Some Structured logging to the debug file
    /// </summary>
    /// <param name="Message">Text to be printed - can be formatted as per string.format</param>
    /// <param name="strParams">Objects to feed into a string.format</param>
    internal static void LogFormatted(string Message, params object[] strParams)
    {
        Message = string.Format(Message, strParams);                  // This fills the params into the message
        string strMessageLine = string.Format("{0},{2},{1}",
            DateTime.Now, Message,
            _AssemblyName);                                           // This adds our standardised wrapper to each line
        UnityEngine.Debug.Log(strMessageLine);                        // And this puts it in the log
    }

    #endregion

}