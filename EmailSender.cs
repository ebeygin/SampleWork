using System;
using HD.ExactTarget.Common;
using HD.ExactTarget.ExactTarget;
using HD.ExactTarget.Subscribers;
using HD.Infrastructure.ValResults;

namespace HD.ExactTarget.Emails
{
    /// <summary>
    /// A service that sends emails.
    /// </summary>
    public class EmailSender
    {
        private readonly SoapClientFactory _soapClientFactory;

        public EmailSender(SoapClientFactory soapClientFactory)
        {
            _soapClientFactory = soapClientFactory;
        }

        /// <summary>
        /// Trigger an email send to the specified subscriber using the 'Trigger Send Definition' key,
        /// <paramref name="triggerSendDefKey"/>.
        /// </summary>
        /// <remarks>Note that this function additionally creates a triggered send data extension and will create
        /// a duplicate even if the subscriber details are identical. It might be a good idea to fix this some
        /// day.</remarks>
        public ValResultData<int> SendEmailNow(string subscriberKey, string triggerSendDefKey, int listId,
                                               SubscriberBO subscriberBO)
        {
            if (_soapClientFactory == null)
                return ValResultDataFactory.NewFailure<int>("No SoapClientFactory provided to EmailSender");

            if (string.IsNullOrEmpty(subscriberKey))
                return ValResultDataFactory.NewFailure<int>("No subscriber key provided to EmailSender");

            if (string.IsNullOrEmpty(triggerSendDefKey))
                return ValResultDataFactory.NewFailure<int>("No trigger send definition key provided to EmailSender");

            Subscriber subscriber;
            if (subscriberBO != null)
            {
                subscriber = SubscriberFactory.New(subscriberBO, listId, _soapClientFactory);
            }
            else
            {
                subscriber = new Subscriber
                {
                    EmailAddress  = subscriberKey,
                    SubscriberKey = subscriberKey,
                };
            }

            var clientId = new ClientID
            {
                ID          = _soapClientFactory.SubAccountId,
                IDSpecified = true,
            };

            var send = new TriggeredSend
            {
                Client                  = clientId,
                Subscribers             = new[] { subscriber },
                TriggeredSendDefinition = new TriggeredSendDefinition
                {
                    CustomerKey = triggerSendDefKey,
                    Client      = clientId,
                },
            };

            APIObject[]    sends = {send};
            string         status;
            CreateResult[] results;

            try
            {
                string requestId;
                results = _soapClientFactory.Client.Create(new CreateOptions(), sends, out requestId, out status);
            }
            catch (Exception ex)
            {
                return ValResultDataFactory.NewFailure<int>("Failed to trigger email send. Exception details: {0}",
                                                            ex.Message);
            }

            if (!"OK".Equals(status, StringComparison.InvariantCultureIgnoreCase))
            {
                string errors = ETErrorResultExtractor.Extract(results);
                if (!string.IsNullOrEmpty(errors))
                    return ValResultDataFactory.NewFailure<int>("Failed to trigger email send. " +
                                                                "Exact Target errors are: {0}", errors);
            }

            if (results.Length == 0)
                return ValResultDataFactory.NewSuccess(0, "Failed to trigger email send. " +
                                                       "Unable to acquire error details.");

            return ValResultDataFactory.NewSuccess(0, "Successfully triggered email send");
        }

        /// <summary>
        /// Trigger an email send to the specified subscriber using the 'Trigger Send Definition' key,
        /// <paramref name="triggerSendDefKey"/>.
        /// </summary>
        /// <remarks>Note that this function additionally creates a triggered send data extension and will create
        /// a duplicate even if the subscriber details are identical. It might be a good idea to fix this some
        /// day.</remarks>
        public ValResultData<int> SendEmailNow(string subscriberKey, string triggerSendDefKey, int listId,
                                               SubscriberExpertQuestion subscriberExpertQuestion)
        {
            if (_soapClientFactory == null)
                return ValResultDataFactory.NewFailure<int>("No SoapClientFactory provided to EmailSender");

            if (string.IsNullOrEmpty(subscriberKey))
                return ValResultDataFactory.NewFailure<int>("No subscriber key provided to EmailSender");

            if (string.IsNullOrEmpty(triggerSendDefKey))
                return ValResultDataFactory.NewFailure<int>("No trigger send definition key provided to EmailSender");

            Subscriber subscriber;
            if (subscriberExpertQuestion != null)
            {
                subscriber = SubscriberExpertQuestionFactory.New(subscriberExpertQuestion, listId, _soapClientFactory);
            }
            else
            {
                subscriber = new Subscriber
                {
                    EmailAddress = subscriberKey,
                    SubscriberKey = subscriberKey,
                };
            }

            var clientId = new ClientID
            {
                ID = _soapClientFactory.SubAccountId,
                IDSpecified = true,
            };

            var send = new TriggeredSend
            {
                Client = clientId,
                Subscribers = new[] { subscriber },
                TriggeredSendDefinition = new TriggeredSendDefinition
                {
                    CustomerKey = triggerSendDefKey,
                    Client = clientId,
                },
            };

            APIObject[] sends = { send };
            string status;
            CreateResult[] results;

            try
            {
                string requestId;
                results = _soapClientFactory.Client.Create(new CreateOptions(), sends, out requestId, out status);
            }
            catch (Exception ex)
            {
                return ValResultDataFactory.NewFailure<int>("Failed to trigger email send. Exception details: {0}",
                                                            ex.Message);
            }

            if (!"OK".Equals(status, StringComparison.InvariantCultureIgnoreCase))
            {
                string errors = ETErrorResultExtractor.Extract(results);
                if (!string.IsNullOrEmpty(errors))
                    return ValResultDataFactory.NewFailure<int>("Failed to trigger email send. " +
                                                                "Exact Target errors are: {0}", errors);
            }

            if (results.Length == 0)
                return ValResultDataFactory.NewSuccess(0, "Failed to trigger email send. " +
                                                       "Unable to acquire error details.");

            return ValResultDataFactory.NewSuccess(0, "Successfully triggered email send");
        }
    }
}
