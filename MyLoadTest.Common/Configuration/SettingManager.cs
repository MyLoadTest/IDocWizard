using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.Configuration
{
    public sealed class SettingManager
    {
        #region Constants and Fields

        private static readonly string RepositoryPathKey = Helper.GetPropertyName(
            (SettingManager obj) => obj.RepositoryPath);

        private static readonly string ParameterPageSelectedFolderKey = Helper.GetPropertyName(
            (SettingManager obj) => obj.ParameterPageSelectedFolder);

        private static readonly SettingManager InstanceField = new SettingManager();

        private Func<string, string, string> _getSetting;
        private Action<string, string> _setSetting;

        #endregion

        #region Constructors

        private SettingManager()
        {
            _getSetting = (name, defaultValue) => { throw new NotSupportedException(); };
            _setSetting = (name, value) => { throw new NotSupportedException(); };
        }

        #endregion

        #region Public Properties

        public static SettingManager Instance
        {
            [DebuggerStepThrough]
            get
            {
                return InstanceField;
            }
        }

        public string RepositoryPath
        {
            get
            {
                return _getSetting(RepositoryPathKey, null);
            }

            set
            {
                _setSetting(RepositoryPathKey, value);
            }
        }

        public string ParameterPageSelectedFolder
        {
            get
            {
                return _getSetting(ParameterPageSelectedFolderKey, null);
            }

            set
            {
                _setSetting(ParameterPageSelectedFolderKey, value);
            }
        }

        #endregion

        #region Public Methods

        public void SetAccessors(
            Func<string, string, string> getSetting,
            Action<string, string> setSetting)
        {
            #region Argument Check

            if (getSetting == null)
            {
                throw new ArgumentNullException("getSetting");
            }

            if (setSetting == null)
            {
                throw new ArgumentNullException("setSetting");
            }

            #endregion

            _getSetting = getSetting;
            _setSetting = setSetting;
        }

        #endregion
    }
}