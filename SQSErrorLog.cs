using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using SQS;
using Elmah;
using Amazon.SQS;

using ApplicationException = System.ApplicationException;
using IDictionary = System.Collections.IDictionary;
using IList = System.Collections.IList;

namespace Totem.ErrorHandling
{
    public class SQSErrorLog : ErrorLog
    {
        private readonly string _connectionString;
        private const int _maxAppNameLength = 60;
        private delegate RV Function<RV, A>(A a);
        private static Requestor requestor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSErrorLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>

        public SQSErrorLog(IDictionary config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            // We will not be connecting to the database.
            _connectionString = string.Empty;

            // Set the application name as this implementation provides
            // per-application isolation over a single store.

            string appName = NullString((string)config["applicationName"]);

            if (appName.Length > _maxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    _maxAppNameLength.ToString("N0")));
            }

            ApplicationName = appName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSErrorLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>

        public SQSErrorLog(string connectionString)
        {
            _connectionString = string.Empty;
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>

        public override string Name
        {
            get { return "Amazon SQS Error Log"; }
        }

        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Logs an error Amazons Queing Service
        /// </summary>

        public override string Log(Error error)
        {
            string id = string.Empty;
            bool EnableSQSLogging = false;

            try
            {
                EnableSQSLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSQSLogging"]);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

            if (EnableSQSLogging)
            {
                if (error == null)
                    throw new ArgumentNullException("error");

                try
                {
                    error.ApplicationName = ApplicationName;
                    string errorXml = ErrorXml.EncodeString(error);

                    requestor = new Requestor();

                    requestor.AwsID             = ConfigurationManager.AppSettings["AwsID"];
                    requestor.AWSSecretKey      = ConfigurationManager.AppSettings["AWSSecretKey"];
                    requestor.Queue             = ConfigurationManager.AppSettings["Queue"];
                    
                    id = requestor.MakeRequest(errorXml);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return id;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public override int EndGetErrors(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override ErrorLogEntry GetError(string id)
        {
            throw new NotImplementedException();
        }

        // This utility function was marked as internal, so I had to copy it locally
        public static string NullString(string s)
        {
            return s ?? string.Empty;
        }       
    }
}
