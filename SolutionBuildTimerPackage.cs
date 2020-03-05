using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Threading;

namespace KnowledgeBaseSoftware.SolutionBuildTimer
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the 
  /// IVsPackage interface and uses the registration attributes defined in the framework to 
  /// register itself and its components with the shell.
  /// </summary>
  // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
  // a package.
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  // This attribute is used to register the information needed to show this package
  // in the Help/About dialog of Visual Studio.
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [Guid(GuidList.guidSolutionBuildTimerPkgString)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  public sealed class SolutionBuildTimerPackage : AsyncPackage
  {
    /// <summary>
    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require 
    /// any Visual Studio service because at this point the package object is created but 
    /// not sited yet inside Visual Studio environment. The place to do all the other 
    /// initialization is the Initialize method.
    /// </summary>
    public SolutionBuildTimerPackage()
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
    }

    private _DTE _applicationObject;
    private OutputWindowPane outputWindowPane;
    private BuildEvents buildEvents;
    private bool amTiming = false;
    private DateTime dtStart, dtEnd;


    /////////////////////////////////////////////////////////////////////////////
    // Overridden Package Implementation
    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      await base.InitializeAsync(cancellationToken, progress);
      // Switches to the UI thread
      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
      // We will need this object
      _applicationObject = (_DTE)await GetServiceAsync(typeof(DTE));
      // Output should go to Output window
      OutputWindow outputWindow = (OutputWindow)_applicationObject.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}").Object;
      outputWindowPane = outputWindow.OutputWindowPanes.Item("Build");
      // Add ourselves as an OnBuildBegin/OnBuildDone handler
      Events events = _applicationObject.Events;
      buildEvents = (BuildEvents)events.BuildEvents;
      buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
      buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(this.OnBuildDone);
    }

    protected override void Dispose(bool disposing)
    {
      if (buildEvents != null)
      {
        buildEvents.OnBuildBegin -= new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
        buildEvents.OnBuildDone -= new _dispBuildEvents_OnBuildDoneEventHandler(this.OnBuildDone);
      }
      base.Dispose(disposing);
    }

    // BuildEvents
    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      // Check for a solution build for Build or RebuildAll
      if (vsBuildScope.vsBuildScopeSolution == Scope &&
            (vsBuildAction.vsBuildActionBuild == Action || vsBuildAction.vsBuildActionRebuildAll == Action))
      {
        // Flag our build timer
        amTiming = true;
        dtStart = DateTime.Now;
        outputWindowPane.OutputString(String.Format("Starting timed solution build on {0}\n", dtStart));
      }
    }

    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      // Check if we are actually timing this build
      if (amTiming)
      {
        amTiming = false;
        dtEnd = DateTime.Now;
        outputWindowPane.OutputString(String.Format("Ended timed solution build on {0}\n", dtEnd));
        TimeSpan tsElapsed = dtEnd - dtStart;
        outputWindowPane.OutputString(String.Format("Total build time: {0}\n", tsElapsed));
      }
    }
    #endregion
  }
}
