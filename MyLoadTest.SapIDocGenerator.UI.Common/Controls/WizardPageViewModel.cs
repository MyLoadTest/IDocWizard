﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

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

        public void GenerateAction(IVuGenScript script, IActionScriptItem actionItem)
        {
            #region Argument Check

            if (script == null)
            {
                throw new ArgumentNullException("script");
            }

            if (actionItem == null)
            {
                throw new ArgumentNullException("actionItem");
            }

            #endregion

            var scriptFilePath = script.FileName;

            CopyIdocFiles(scriptFilePath);
            CheckAndUpdateScriptFile(scriptFilePath);
            UpdateActionFile(actionItem);
        }

        #endregion

        #region Private Methods

        private static void CheckAndUpdateScriptFile(string scriptFilePath)
        {
            Logger.InfoFormat("Checking script file '{0}'", scriptFilePath);
            using (var scriptStream = File.Open(scriptFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                var scriptIni = new IniFile();
                scriptIni.Load(scriptStream);

                var section = scriptIni[DllSectionName];
                if (section.Contains(DllName) && section[DllName] == string.Empty)
                {
                    Logger.InfoFormat("No need to update script file '{0}'", scriptFilePath);
                    return;
                }

                section[DllName] = string.Empty;

                var backupScriptFilePath = scriptFilePath + BackupExtension;
                Logger.InfoFormat(
                    "Creating backup file '{0}' of script file '{1}'",
                    backupScriptFilePath,
                    scriptFilePath);
                File.Copy(scriptFilePath, backupScriptFilePath, true);

                Logger.InfoFormat("Updating script file '{0}'", scriptFilePath);
                scriptStream.SetLength(0);
                scriptIni.Save(scriptStream);
            }
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

        private void UpdateActionFile(IActionScriptItem actionItem)
        {
            var filePath = actionItem.FullFileName;
            Logger.InfoFormat("Updating action file '{0}'", filePath);

            var definition = SapIDocDefinition.LoadHeader(this.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var actionContents = doc.GetVuGenActionContents();

            //// TODO [vmaklai] Open and modify file in editor rather than directly on disk

            #region In-editor Load

            //// In-editor Load approach does not work:
            ////    VuGen throws ObjectDisposedException with the message:
            ////        Cannot access a disposed object.
            ////        Object name: 'the instance has been already disposed and cannot be used'.

            ////var actionContentsData = Encoding.Default.GetBytes(actionContents);
            ////using (var actionContentsStream = new MemoryStream(actionContentsData, false))
            ////{
            ////    var viewContent = FileService.OpenFile(filePath, true);
            ////    if (viewContent == null)
            ////    {
            ////        throw new InvalidOperationException(
            ////            string.Format(
            ////                CultureInfo.InvariantCulture,
            ////                "Unable to open file \"{0}\".",
            ////                filePath));
            ////    }

            ////    viewContent.Load(viewContent.PrimaryFile, actionContentsStream);
            ////}

            #endregion

            File.WriteAllText(actionItem.FullFileName, actionContents, Encoding.Default);
        }

        #endregion
    }
}