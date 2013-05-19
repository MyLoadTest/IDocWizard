using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class WizardControl
    {
        #region Constructors

        public WizardControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var definition = SapIDocDefinition.LoadHeader(this.ViewModel.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ViewModel.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var vugenXml = doc.VuGenXml();

            Console.WriteLine(vugenXml);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Reset();
        }

        #endregion
    }
}