namespace SitecoreGadgets.log4net.Appender
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using global::log4net.Appender;
    using global::log4net.helpers;

    public class SitecoreRollingLogFileAppender : RollingFileAppender
    {
        #region properties

        private DateTime CurrentDate { get; set; }
        private string OriginalFileName { get; set; }

        public override string File
        {
            get { return base.File; }
            set
            {
                if (this.OriginalFileName == null)
                {
                    string str = value;
                    Dictionary<string, string> variables = ConfigReader.GetVariables();
                    foreach (string str2 in variables.Keys)
                    {
                        string oldValue = "$(" + str2 + ")";
                        str = str.Replace(oldValue, variables[str2]);
                    }
                    this.OriginalFileName = Sitecore.IO.FileUtil.MapPath(str.Trim());
                }
                base.File = this.OriginalFileName;
            }
        }

        #endregion properties

        #region ctor

        public SitecoreRollingLogFileAppender()
        {
            CurrentDate = DateTime.Now;
            // by default roll over size
            RollingStyle = RollingMode.Size;
            // by default keep all logs
            MaxSizeRollBackups = -1;
        }

        #endregion  ctor

        #region methods

        protected override void Append(global::log4net.spi.LoggingEvent loggingEvent)
        {
            var now = DateTime.Now;
            if (CurrentDate.Date != now.Date || (RollingStyle == RollingMode.Size && (((CountingQuietTextWriter)base.m_qtw).Count >= MaxFileSize)))
            {
                lock (this)
                {
                    base.CloseFile();
                    CurrentDate = DateTime.Now;
                    this.OpenFile(string.Empty, false);
                }
            }

            base.Append(loggingEvent);
        }

        protected override void OpenFile(string fileName, bool append)
        {

            fileName = this.OriginalFileName;
            fileName = fileName.Replace("{date}", CurrentDate.ToString("yyyyMMdd"));
            fileName = fileName.Replace("{time}", CurrentDate.ToString("HHmmss"));
            fileName = fileName.Replace("{processid}", GetCurrentProcessId().ToString());
            if (System.IO.File.Exists(fileName))
            {
                fileName = this.GetTimedFileName(fileName);
            }
            base.OpenFile(fileName, append);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentProcessId();

        private string GetTimedFileName(string fileName)
        {
            int length = fileName.LastIndexOf('.');
            if (length < 0)
            {
                return fileName;
            }
            return string.Concat(new object[] { fileName.Substring(0, length), '.', CurrentDate.ToString("HHmmss"), fileName.Substring(length) });
        }




        #endregion methods
    }
}
