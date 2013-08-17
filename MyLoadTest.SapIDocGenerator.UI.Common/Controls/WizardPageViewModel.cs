using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using HP.LR.Vugen.BackEnd.Project.ProjectSystem.ScriptItems;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using HP.Utt.ProjectSystem;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class WizardPageViewModel : GeneratorControlSubViewModel
    {
        #region Constants and Fields

        private const string DllSectionName = "ManuallyExtraFiles";
        private const string DllSubdirectory = "idoc";
        private const string DllName = "idoc.dll";
        private const string BackupExtension = ".bak";

        private static readonly string[] DllFiles = new[] { DllName, "idoc.pdb" };

        private string _definitionFilePath;
        private string _exampleFilePath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WizardPageViewModel"/> class.
        /// </summary>
        public WizardPageViewModel(GeneratorControlViewModel owner)
            : base(owner)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public string DefinitionFilePath
        {
            [DebuggerNonUserCode]
            get
            {
                return _definitionFilePath;
            }

            set
            {
                if (value == _definitionFilePath)
                {
                    return;
                }

                _definitionFilePath = value;
                RaisePropertyChanged(obj => obj.DefinitionFilePath);
                RaisePropertyChanged(obj => obj.IsCreateEnabled);
            }
        }

        public string ExampleFilePath
        {
            [DebuggerNonUserCode]
            get
            {
                return _exampleFilePath;
            }

            set
            {
                if (value == _exampleFilePath)
                {
                    return;
                }

                _exampleFilePath = value;
                RaisePropertyChanged(obj => obj.ExampleFilePath);
                RaisePropertyChanged(obj => obj.IsCreateEnabled);
            }
        }

        public bool IsCreateEnabled
        {
            get
            {
                return !this.DefinitionFilePath.IsNullOrWhiteSpace()
                    && !this.ExampleFilePath.IsNullOrWhiteSpace();
            }
        }

        #endregion

        #region Public Methods

        public override void Reset(bool restoreSettings)
        {
            this.DefinitionFilePath = string.Empty;
            this.ExampleFilePath = string.Empty;
        }

        public void GenerateAction(
            IVuGenScript script,
            IActionScriptItem initActionItem,
            IActionScriptItem mainActionItem,
            IActionScriptItem endActionItem)
        {
            #region Argument Check

            if (script == null)
            {
                throw new ArgumentNullException("script");
            }

            if (initActionItem == null)
            {
                throw new ArgumentNullException("initActionItem");
            }

            if (mainActionItem == null)
            {
                throw new ArgumentNullException("mainActionItem");
            }

            if (endActionItem == null)
            {
                throw new ArgumentNullException("endActionItem");
            }

            #endregion

            var scriptFilePath = script.FileName;

            CopyIdocFiles(scriptFilePath);
            CheckAndUpdateScriptFile(script);
            UpdateActionFiles(initActionItem, mainActionItem, endActionItem);
        }

        #endregion

        #region Private Methods

        private static void CheckAndUpdateScriptFile(IVuGenScript script)
        {
            Logger.InfoFormat("Checking the script '{0}'...", script.FileName);

            var extraFiles = script.GetExtraFiles();
            var isDllReferenced = extraFiles
                .Any(item => string.Equals(item.FileName, DllName, StringComparison.OrdinalIgnoreCase));

            if (isDllReferenced)
            {
                Logger.InfoFormat("The library '{0}' is referenced; no update is needed.", DllName);
                return;
            }

            Logger.InfoFormat("The library '{0}' is NOT referenced; updating and reopening the script.", DllName);

            var dllFilePath = Path.Combine(Path.GetDirectoryName(script.FileName).EnsureNotNull(), DllName);
            var extraFileScriptItem = script.GenerateManuallyExtraFileScriptItem(dllFilePath);
            script.AddExtraFile(extraFileScriptItem);
            script.Save(true);

            #region Work-around: VuGen UI does not reflect the changes unless the project is reloaded

            ProjectService.CloseSolution();
            ProjectService.LoadSolutionOrProject(script.FileName);

            #endregion

            ////Logger.InfoFormat("Checking script file '{0}'", scriptFilePath);
            ////using (var scriptStream = File.Open(scriptFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            ////{
            ////    var scriptIni = new IniFile();
            ////    scriptIni.Load(scriptStream);

            ////    var section = scriptIni[DllSectionName];
            ////    if (section.Contains(DllName) && section[DllName] == string.Empty)
            ////    {
            ////        Logger.InfoFormat("No need to update script file '{0}'", scriptFilePath);
            ////        return;
            ////    }

            ////    section[DllName] = string.Empty;

            ////    var backupScriptFilePath = scriptFilePath + BackupExtension;
            ////    Logger.InfoFormat(
            ////        "Creating backup file '{0}' of script file '{1}'",
            ////        backupScriptFilePath,
            ////        scriptFilePath);
            ////    File.Copy(scriptFilePath, backupScriptFilePath, true);

            ////    Logger.InfoFormat("Updating script file '{0}'", scriptFilePath);
            ////    scriptStream.SetLength(0);
            ////    scriptIni.Save(scriptStream);
            ////}
        }

        private void RaisePropertyChanged<T>(Expression<Func<WizardPageViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<WizardPageViewModel, T>(propertyGetterExpression);
        }

        private void CopyIdocFiles(string scriptFilePath)
        {
            var dllDirectory = Path.Combine(GetType().Assembly.GetDirectory(), DllSubdirectory);
            var scriptDirectory = Path.GetDirectoryName(scriptFilePath).EnsureNotNull();

            Logger.Info("Copying IDoc files");
            foreach (var dllFile in DllFiles)
            {
                var sourceFilePath = Path.Combine(dllDirectory, dllFile);
                var targetFilePath = Path.Combine(scriptDirectory, dllFile);

                Logger.InfoFormat("Copying file '{0}' to '{1}'", sourceFilePath, targetFilePath);

                if (File.Exists(targetFilePath))
                {
                    File.SetAttributes(targetFilePath, FileAttributes.Normal);
                }

                File.Copy(sourceFilePath, targetFilePath, true);
            }
        }

        private void UpdateActionFiles(
            IActionScriptItem initActionItem,
            IActionScriptItem mainActionItem,
            IActionScriptItem endActionItem)
        {
            Logger.InfoFormat(
                "Updating action files '{0}' and '{1}'.",
                mainActionItem.FullFileName,
                initActionItem.FullFileName);

            var definition = SapIDocDefinition.LoadHeader(this.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var generatedActions = doc.GetVuGenActionContents().EnsureNotNull();

            //// TODO [vmaklai] Open and modify files in editor rather than directly on disk

            #region In-editor Load

            //// In-editor Load approach does not work:
            ////    VuGen throws ObjectDisposedException with the message:
            ////        Cannot access a disposed object.
            ////        Object name: 'the instance has been already disposed and cannot be used'.

            //////(!)var initActionContentsData = Encoding.Default.GetBytes(generatedActions.InitActionContents);
            ////var actionContentsData = Encoding.Default.GetBytes(generatedActions.MainActionContents);
            //////(!)var endActionContentsData = Encoding.Default.GetBytes(generatedActions.EndActionContents);
            ////using (var actionContentsStream = new MemoryStream(actionContentsData, false))
            ////{
            ////    var viewContent = FileService.OpenFile(mainActionItem.FullFileName, true);
            ////    if (viewContent == null)
            ////    {
            ////        throw new InvalidOperationException(
            ////            string.Format(
            ////                CultureInfo.InvariantCulture,
            ////                "Unable to open file \"{0}\".",
            ////                mainActionItem.FullFileName));
            ////    }

            ////    viewContent.Load(viewContent.PrimaryFile, actionContentsStream);
            ////}

            #endregion

            File.WriteAllText(initActionItem.FullFileName, generatedActions.InitActionContents, Encoding.Default);
            File.WriteAllText(mainActionItem.FullFileName, generatedActions.MainActionContents, Encoding.Default);
            File.WriteAllText(endActionItem.FullFileName, generatedActions.EndActionContents, Encoding.Default);
        }

        #endregion
    }
}