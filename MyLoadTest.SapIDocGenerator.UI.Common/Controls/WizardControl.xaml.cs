using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using HP.LR.Vugen.BackEnd.Project.ProjectSystem.ScriptItems;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using HP.Utt.ProjectSystem.Persistence;
using ICSharpCode.SharpDevelop.Project;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class WizardControl
    {
        #region Constants and Fields

        private const string MainActionName = "Action";

        #endregion

        #region Constructors

        public WizardControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var currentProject = ProjectService.CurrentProject as IVuGenProject;
            if (currentProject == null)
            {
                this.ShowErrorBox("No VuGen project is loaded.");
                return;
            }

            if (currentProject.ReadOnly)
            {
                this.ShowErrorBox("The current VuGen project is read-only.");
                return;
            }

            var script = currentProject.Script;
            if (script == null)
            {
                this.ShowErrorBox("A VuGen script is not assigned in the current project.");
                return;
            }

            var action = script.GetActionByName(MainActionName);
            if (action == null)
            {
                this.ShowErrorBox(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "VuGen script action '{0}' is not found.",
                        MainActionName));
                return;
            }

            var confirmMessage = string.Format(
                CultureInfo.InvariantCulture,
                "The file '{0}' will be overwritten with the generated contents.{1}{1}Do you want to continue?",
                action.FullFileName,
                Environment.NewLine);
            var messageBoxResult = this.ShowMessageBox(
                confirmMessage,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            var definition = SapIDocDefinition.LoadHeader(this.ViewModel.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ViewModel.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var actionContents = doc.GetVuGenActionContents();

            File.WriteAllText(action.FullFileName, actionContents, Encoding.Default);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Reset();
        }

        #endregion
    }
}