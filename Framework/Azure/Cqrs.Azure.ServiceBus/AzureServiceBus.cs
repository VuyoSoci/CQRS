﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;

#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#else
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif

#if NET452
#else
using Microsoft.Identity.Client;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <remarks>
	/// https://markheath.net/post/migrating-to-new-servicebus-sdk
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription
	/// https://stackoverflow.com/questions/47427361/azure-service-bus-read-messages-sent-by-net-core-2-with-brokeredmessage-getbo
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </remarks>
	public abstract class AzureServiceBus<TAuthenticationToken>
		: AzureBus<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the private <see cref="TopicClient"/> publisher.
		/// </summary>
		protected TopicClient PrivateServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the public <see cref="TopicClient"/> publisher.
		/// </summary>
		protected TopicClient PublicServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the private <see cref="IMessageReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, IMessageReceiver> PrivateServiceBusReceivers { get; private set; }

		/// <summary>
		/// Gets the public <see cref="IMessageReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, IMessageReceiver> PublicServiceBusReceivers { get; private set; }

		/// <summary>
		/// The name of the private topic.
		/// </summary>
		protected string PrivateTopicName { get; private set; }

		/// <summary>
		/// The name of the public topic.
		/// </summary>
		protected string PublicTopicName { get; private set; }

		/// <summary>
		/// The name of the subscription in the private topic.
		/// </summary>
		protected string PrivateTopicSubscriptionName { get; private set; }

		/// <summary>
		/// The name of the subscription in the public topic.
		/// </summary>
		protected string PublicTopicSubscriptionName { get; private set; }

		/// <summary>
		/// The configuration key for the message bus connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string MessageBusConnectionStringConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the message bus connection endpoint as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string MessageBusConnectionEndpointConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the message bus connection Application Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string MessageBusConnectionApplicationIdConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the message bus connection Client Key/Secret as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string MessageBusConnectionClientKeyConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the message bus connection Tenant Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string MessageBusConnectionTenantIdConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string SigningTokenConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateTopicNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicTopicNameConfigurationKey { get; }

		/// <summary>
		/// The default name of the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPrivateTopicName { get; }

		/// <summary>
		/// The default name of the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPublicTopicName { get; }

		/// <summary>
		/// The configuration key for the name of the subscription in the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateTopicSubscriptionNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the subscription in the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicTopicSubscriptionNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key that
		/// specifies if an <see cref="Exception"/> is thrown if the network lock is lost
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey { get; }

		/// <summary>
		/// Specifies if an <see cref="Exception"/> is thrown if the network lock is lost.
		/// </summary>
		protected bool ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete { get; private set; }

		/// <summary>
		/// The default name of the subscription in the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPrivateTopicSubscriptionName = "Root";

		/// <summary>
		/// The default name of the subscription in the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPublicTopicSubscriptionName = "Root";

#if NETSTANDARD2_0
		/// <summary>
		/// The <see cref="Action{IMessageReceiver, TBrokeredMessage}">handler</see> used for <see cref="MessageReceiver.RegisterMessageHandler(Func{BrokeredMessage, CancellationToken, Task}, MessageHandlerOptions)"/> on each receiver.
		/// </summary>
		protected Action<IMessageReceiver, BrokeredMessage> ReceiverMessageHandler { get; set; }
#else
		/// <summary>
		/// The <see cref="Action{TBrokeredMessage}">handler</see> used for <see cref="IMessageReceiver.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected Action<IMessageReceiver, BrokeredMessage> ReceiverMessageHandler { get; set; }
#endif

#if NETSTANDARD2_0
		/// <summary>
		/// The <see cref="MessageHandlerOptions" /> used for <see cref="MessageReceiver.RegisterMessageHandler(Func{BrokeredMessage, CancellationToken, Task}, MessageHandlerOptions)"/> on each receiver.
		/// </summary>
		protected MessageHandlerOptions ReceiverMessageHandlerOptions { get; set; }
#else
		/// <summary>
		/// The <see cref="OnMessageOptions" /> used for <see cref="IMessageReceiver.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected OnMessageOptions ReceiverMessageHandlerOptions { get; set; }
#endif

		/// <summary>
		/// Gets the <see cref="IBusHelper"/>.
		/// </summary>
		protected IBusHelper BusHelper { get; private set; }

		/// <summary>
		/// Gets the <see cref="IAzureBusHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		/// <summary>
		/// Gets the <see cref="ITelemetryHelper"/>.
		/// </summary>
		protected ITelemetryHelper TelemetryHelper { get; set; }

		/// <summary>
		/// The maximum number of time a retry is tried if a <see cref="System.TimeoutException"/> is thrown while sending messages.
		/// </summary>
		protected short TimeoutOnSendRetryMaximumCount { get; private set; }

		/// <summary>
		/// The <see cref="IHashAlgorithmFactory"/> to use to sign messages.
		/// </summary>
		protected IHashAlgorithmFactory Signer { get; private set; }

		/// <summary>
		/// A list of namespaces to exclude when trying to automatically determine the container.
		/// </summary>
		protected IList<string> ExclusionNamespaces { get; private set; }

#if NET452
#else
		/// <summary>
		/// Gets an access token from Active Directory when using RBAC based connections.
		/// </summary>
		protected AzureActiveDirectoryTokenProvider.AuthenticationCallback GetActiveDirectoryToken { get; private set; }
#endif

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureServiceBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
			BusHelper = busHelper;
			TelemetryHelper = new NullTelemetryHelper();
			PrivateServiceBusReceivers = new Dictionary<int, IMessageReceiver>();
			PublicServiceBusReceivers = new Dictionary<int, IMessageReceiver>();
			TimeoutOnSendRetryMaximumCount = 1;
			string timeoutOnSendRetryMaximumCountValue;
			short timeoutOnSendRetryMaximumCount;
			if (ConfigurationManager.TryGetSetting("Cqrs.Azure.Servicebus.TimeoutOnSendRetryMaximumCount", out timeoutOnSendRetryMaximumCountValue) && !string.IsNullOrWhiteSpace(timeoutOnSendRetryMaximumCountValue) && short.TryParse(timeoutOnSendRetryMaximumCountValue, out timeoutOnSendRetryMaximumCount))
				TimeoutOnSendRetryMaximumCount = timeoutOnSendRetryMaximumCount;
			ExclusionNamespaces = new SynchronizedCollection<string> { "Cqrs", "System" };
			Signer = hashAlgorithmFactory;

			Instantiate();
		}

		private void Instantiate()
		{
#if NET452
#else
			GetActiveDirectoryToken = async (audience, authority, state) =>
			{
				string applicationId = ConfigurationManager.GetSetting(MessageBusConnectionApplicationIdConfigurationKey);
				string clientKey = ConfigurationManager.GetSetting(MessageBusConnectionClientKeyConfigurationKey);

				IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(applicationId)
					.WithAuthority(authority)
					.WithClientSecret(clientKey)
					.Build();

				var authResult = await app
					.AcquireTokenForClient(new string[] { "https://servicebus.azure.net/.default" })
					.ExecuteAsync();

				return authResult.AccessToken;
			};
#endif
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		/// <summary>
		/// Gets the connection string for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override string GetConnectionString()
		{
			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionStringConfigurationKey, out string connectionString))
				connectionString = null;
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				string connectionEndpoint = ConfigurationManager.GetSetting(MessageBusConnectionEndpointConfigurationKey);
				// double check an endpoint isn't provided, if it is, then we're using endpoints, but if not, we'll assume a connection string is prefered as it's easier
				if (string.IsNullOrWhiteSpace(connectionEndpoint))
					throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{MessageBusConnectionStringConfigurationKey}' is defined and has a valid connection string value.");
			}
			return connectionString;
		}

		/// <summary>
		/// Gets the RBAC connection settings for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override AzureBusRbacSettings GetRbacConnectionSettings()
		{
			// double check an endpoint isn't provided, if it is, then we're using endpoints, but if not, we'll assume a connection string is prefered as it's easier
			bool isUsingConnectionString;
			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionStringConfigurationKey, out string connectionString))
				isUsingConnectionString = false;
			else
				isUsingConnectionString = !string.IsNullOrWhiteSpace(connectionString);

			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionEndpointConfigurationKey, out string endpoint))
				endpoint = null;
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(endpoint))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{MessageBusConnectionEndpointConfigurationKey}' is defined and has a valid connection endpoint value.");

			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionApplicationIdConfigurationKey, out string applicationId))
				applicationId = null;
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(applicationId))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{MessageBusConnectionApplicationIdConfigurationKey}' is defined and has a valid application id value.");

			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionClientKeyConfigurationKey, out string clientKey))
				clientKey = null;
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(clientKey))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{MessageBusConnectionClientKeyConfigurationKey}' is defined and has a valid client key/secret value.");

			if (!ConfigurationManager.TryGetSetting(MessageBusConnectionTenantIdConfigurationKey, out string tenantId))
				tenantId = null;
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(tenantId))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{MessageBusConnectionTenantIdConfigurationKey}' is defined and has a valid tenant id value.");

			return new AzureBusRbacSettings
			{
				Endpoint = endpoint,
				ApplicationId = applicationId,
				ClientKey = clientKey,
				TenantId = tenantId
			};
		}

#endregion

		/// <summary>
		/// Instantiate publishing on this bus by
		/// calling <see cref="CheckPrivateTopicExists"/> and <see cref="CheckPublicTopicExists"/>
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiatePublishing()
		{
#if NET452
#else
			if (GetActiveDirectoryToken == null)
				Instantiate();
#endif

			Manager manager;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = new Manager(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
#else
#if NET452
			manager = Manager.CreateFromConnectionString(ConnectionString);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = Manager.CreateFromConnectionString(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, new Uri(rbacSettings.Endpoint), rbacSettings.GetDefaultAuthority()));
#endif
#endif
			CheckPrivateTopicExists(manager, false);
			CheckPublicTopicExists(manager, false);

#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				PrivateServiceBusPublisher = new TopicClient(ConnectionString, PrivateTopicName);
				PublicServiceBusPublisher = new TopicClient(ConnectionString, PublicTopicName);
			}
			else
			{
				PrivateServiceBusPublisher = new TopicClient(rbacSettings.Endpoint, PrivateTopicName, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
				PublicServiceBusPublisher = new TopicClient(rbacSettings.Endpoint, PublicTopicName, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
			}
#else
#if NET452
			PrivateServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PrivateTopicName);
			PublicServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				PrivateServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PrivateTopicName);
				PublicServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
			}
			else
			{
				PrivateServiceBusPublisher = TopicClient.CreateWithAzureActiveDirectory(new Uri(rbacSettings.Endpoint), PrivateTopicName, GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority());
				PublicServiceBusPublisher = TopicClient.CreateWithAzureActiveDirectory(new Uri(rbacSettings.Endpoint), PublicTopicName, GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority());
			}
#endif
#endif
			StartSettingsChecking();
		}

		/// <summary>
		/// Instantiate receiving on this bus by
		/// calling <see cref="CheckPrivateTopicExists"/> and <see cref="CheckPublicTopicExists"/>
		/// then InstantiateReceiving for private and public topics,
		/// calls <see cref="CleanUpDeadLetters"/> for the private and public topics,
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiateReceiving()
		{
#if NET452
#else
			if (GetActiveDirectoryToken == null)
				Instantiate();
#endif

			Manager manager;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = new Manager(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
#else
#if NET452
			manager = Manager.CreateFromConnectionString(ConnectionString);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = Manager.CreateFromConnectionString(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, new Uri(rbacSettings.Endpoint), rbacSettings.GetDefaultAuthority()));
#endif
#endif

			CheckPrivateTopicExists(manager);
			CheckPublicTopicExists(manager);

			try
			{
				InstantiateReceiving(manager, PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the private Service Bus receivers may be invalid.", exception);
			}
			try
			{
				InstantiateReceiving(manager, PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the public Service Bus receivers may be invalid.", exception);
			}

			bool enableDeadLetterCleanUp;
			string enableDeadLetterCleanUpValue = ConfigurationManager.GetSetting("Cqrs.Azure.Servicebus.EnableDeadLetterCleanUp");
			if (bool.TryParse(enableDeadLetterCleanUpValue, out enableDeadLetterCleanUp) && enableDeadLetterCleanUp)
			{
				CleanUpDeadLetters(PrivateTopicName, PrivateTopicSubscriptionName);
				CleanUpDeadLetters(PublicTopicName, PublicTopicSubscriptionName);
			}

			// If this is also a publisher, then it will the check over there and that will handle this
			// we only need to check one of these
			if (PublicServiceBusPublisher != null)
				return;

			StartSettingsChecking();
		}

#if NETSTANDARD2_0
		/// <summary>
		/// Creates <see cref="AzureBus{TAuthenticationToken}.NumberOfReceiversCount"/> <see cref="IMessageReceiver"/>.
		/// If flushing is required, any flushed <see cref="IMessageReceiver"/> has <see cref="ClientEntity.CloseAsync()"/> called on it first.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#else
		/// <summary>
		/// Creates <see cref="AzureBus{TAuthenticationToken}.NumberOfReceiversCount"/> <see cref="IMessageReceiver"/>.
		/// If flushing is required, any flushed <see cref="IMessageReceiver"/> has <see cref="ClientEntity.Close()"/> called on it first.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#endif
		protected virtual void InstantiateReceiving(Manager manager, IDictionary<int, IMessageReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			for (int i = 0; i < NumberOfReceiversCount; i++)
			{
				IMessageReceiver serviceBusReceiver;
				string connectionString = ConnectionString;
				AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
				if (!string.IsNullOrWhiteSpace(connectionString))
					serviceBusReceiver = new MessageReceiver(ConnectionString, EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName));
				else
					serviceBusReceiver = new MessageReceiver(rbacSettings.Endpoint, EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName), TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
#else
#if NET452
				serviceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, topicSubscriptionName);
#else
				if (!string.IsNullOrWhiteSpace(connectionString))
					serviceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, topicSubscriptionName);
				else
					serviceBusReceiver = SubscriptionClient.CreateWithAzureActiveDirectory(new Uri(rbacSettings.Endpoint), topicName, topicSubscriptionName, GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority());
#endif
#endif
				if (serviceBusReceivers.ContainsKey(i))
					serviceBusReceivers[i] = serviceBusReceiver;
				else
					serviceBusReceivers.Add(i, serviceBusReceiver);
			}
			// Remove any if the number has decreased
			for (int i = NumberOfReceiversCount; i < serviceBusReceivers.Count; i++)
			{
				IMessageReceiver serviceBusReceiver;
				if (serviceBusReceivers.TryGetValue(i, out serviceBusReceiver))
				{
#if NETSTANDARD2_0
					serviceBusReceiver.CloseAsync().Wait(1500);
#else
					serviceBusReceiver.Close();
#endif
				}
				serviceBusReceivers.Remove(i);
			}
		}

		/// <summary>
		/// Checks if the private topic and subscription name exists as per <see cref="PrivateTopicName"/> and <see cref="PrivateTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="createSubscriptionIfNotExists">Create a subscription if there isn't one</param>
		protected virtual void CheckPrivateTopicExists(Manager manager, bool createSubscriptionIfNotExists = true)
		{
			CheckTopicExists(manager, PrivateTopicName = ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName, createSubscriptionIfNotExists);
		}

		/// <summary>
		/// Checks if the public topic and subscription name exists as per <see cref="PublicTopicName"/> and <see cref="PublicTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="createSubscriptionIfNotExists">Create a subscription if there isn't one</param>
		protected virtual void CheckPublicTopicExists(Manager manager, bool createSubscriptionIfNotExists = true)
		{
			CheckTopicExists(manager, PublicTopicName = ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName, createSubscriptionIfNotExists);
		}

		/// <summary>
		/// Checks if a topic by the provided <paramref name="topicName"/> exists and
		/// Checks if a subscription name by the provided <paramref name="subscriptionName"/> exists.
		/// </summary>
		protected virtual void CheckTopicExists(Manager manager, string topicName, string subscriptionName, bool createSubscriptionIfNotExists = true)
		{
			// Configure Queue Settings
			var eventTopicDescription = new TopicDescription(topicName)
			{
#if NETSTANDARD2_0
				MaxSizeInMB = 5120,
#else
				MaxSizeInMegabytes = 5120,
#endif
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true,
			};

#if NETSTANDARD2_0
			Task<bool> checkTask = manager.TopicExistsAsync(topicName);
			checkTask.Wait(1500);
			if (!checkTask.Result)
			{
				Task<TopicDescription> createTopicTask = manager.CreateTopicAsync(eventTopicDescription);
				createTopicTask.Wait(1500);
			}

			checkTask = manager.SubscriptionExistsAsync(topicName, subscriptionName);
			checkTask.Wait(1500);
			if (createSubscriptionIfNotExists && !checkTask.Result)
			{
				var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName)
				{
					DefaultMessageTimeToLive = eventTopicDescription.DefaultMessageTimeToLive,
					EnableBatchedOperations = eventTopicDescription.EnableBatchedOperations,
				};
				Task<SubscriptionDescription> createTask = manager.CreateSubscriptionAsync(subscriptionDescription);
				createTask.Wait(1500);
			}
#else
			// Create the topic if it does not exist already
			if (!manager.TopicExists(eventTopicDescription.Path))
				manager.CreateTopic(eventTopicDescription);

			if (createSubscriptionIfNotExists && !manager.SubscriptionExists(eventTopicDescription.Path, subscriptionName))
				manager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, subscriptionName)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);
#endif
		}

		/// <summary>
		/// First runs <see cref="AzureBus{TAuthenticationToken}.ValidateSettingsHaveChanged"/> then checks
		/// <see cref="PublicTopicName"/>, <see cref="PublicTopicSubscriptionName"/>,
		/// <see cref="PrivateTopicName"/> or <see cref="PrivateTopicSubscriptionName"/> have changed.
		/// </summary>
		/// <returns></returns>
		protected override bool ValidateSettingsHaveChanged()
		{
			if (base.ValidateSettingsHaveChanged())
				return true;
			return PublicTopicName != (ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName)
				||
			PublicTopicSubscriptionName != (ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName)
				||
			PrivateTopicName != (ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName)
				||
			PrivateTopicSubscriptionName != (ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName);
		}

		/// <summary>
		/// Triggers settings checking on both public and private publishers and receivers,
		/// then calls <see cref="InstantiatePublishing"/> if <see cref="PublicServiceBusPublisher"/> is not null.
		/// </summary>
		protected override void TriggerSettingsChecking()
		{
			// First refresh the EventBlackListProcessing property
			bool throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;
			if (!ConfigurationManager.TryGetSetting(ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey, out throwExceptionOnReceiverMessageLockLostExceptionDuringComplete))
				throwExceptionOnReceiverMessageLockLostExceptionDuringComplete = true;
			ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete = throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;

			TriggerSettingsChecking(PrivateServiceBusPublisher, PrivateServiceBusReceivers);
			TriggerSettingsChecking(PublicServiceBusPublisher, PublicServiceBusReceivers);

			// Restart configuration, we order this intentionally with the publisher second as if this triggers the cancellation there's nothing else to process here
			// we also only need to check one of the publishers
			if (PublicServiceBusPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
			}
		}

		/// <summary>
		/// Triggers settings checking on the provided <paramref name="serviceBusPublisher"/> and <paramref name="serviceBusReceivers"/>,
		/// then calls <see cref="InstantiateReceiving()"/>.
		/// </summary>
		protected virtual void TriggerSettingsChecking(TopicClient serviceBusPublisher, IDictionary<int, IMessageReceiver> serviceBusReceivers)
		{
			// Let's wrap up using this message bus and start the switch
			if (serviceBusPublisher != null)
			{
#if NETSTANDARD2_0
				serviceBusPublisher.CloseAsync().Wait(1500);
#else
				serviceBusPublisher.Close();
#endif
				Logger.LogDebug("Publishing service bus closed.");
			}
			foreach (IMessageReceiver serviceBusReceiver in serviceBusReceivers.Values)
			{
				// Let's wrap up using this message bus and start the switch
				if (serviceBusReceiver != null)
				{
#if NETSTANDARD2_0
					serviceBusReceiver.CloseAsync().Wait(1500);
#else
					serviceBusReceiver.Close();
#endif
					Logger.LogDebug("Receiving service bus closed.");
				}
				// Restart configuration, we order this intentionally with the receiver first as if this triggers the cancellation we know this isn't a publisher as well
				if (serviceBusReceiver != null)
				{
					Logger.LogDebug("Recursively calling into InstantiateReceiving.");
					InstantiateReceiving();

					// This will be the case of a connection setting change re-connection
					if (ReceiverMessageHandler != null && ReceiverMessageHandlerOptions != null)
					{
						// Callback to handle received messages
						Logger.LogDebug("Re-registering onMessage handler.");
						ApplyReceiverMessageHandler();
					}
					else
						Logger.LogWarning("No onMessage handler was found to re-bind.");
				}
			}
		}

		/// <summary>
		/// Registers the provided <paramref name="receiverMessageHandler"/> with the provided <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
#if NETSTANDARD2_0
		protected virtual void RegisterReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, MessageHandlerOptions receiverMessageHandlerOptions)
#else
		protected virtual void RegisterReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
#endif
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
#if NETSTANDARD2_0
		protected virtual void StoreReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, MessageHandlerOptions receiverMessageHandlerOptions)
#else
		protected virtual void StoreReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
#endif
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;
		}

		/// <summary>
		/// Applies the stored ReceiverMessageHandler and ReceiverMessageHandlerOptions to all receivers in
		/// <see cref="PrivateServiceBusReceivers"/> and <see cref="PublicServiceBusReceivers"/>.
		/// </summary>
		protected override void ApplyReceiverMessageHandler()
		{
			foreach (IMessageReceiver serviceBusReceiver in PrivateServiceBusReceivers.Values)
			{
#if NETSTANDARD2_0
				serviceBusReceiver
					.RegisterMessageHandler
					(
						(message, cancellationToken) =>
						{
							return Task.Factory.StartNewSafely(() => {
								BusHelper.SetWasPrivateBusUsed(true);
								ReceiverMessageHandler(serviceBusReceiver, message);
							});
						},
						ReceiverMessageHandlerOptions
					);
#else
				serviceBusReceiver
					.OnMessage
					(
						message =>
						{
							BusHelper.SetWasPrivateBusUsed(true);
							ReceiverMessageHandler(serviceBusReceiver, message);
						},
						ReceiverMessageHandlerOptions
					);
#endif
			}
			foreach (IMessageReceiver serviceBusReceiver in PublicServiceBusReceivers.Values)
			{
#if NETSTANDARD2_0
				serviceBusReceiver
					.RegisterMessageHandler
					(
						(message, cancellationToken) =>
						{
							return Task.Factory.StartNewSafely(() => {
								BusHelper.SetWasPrivateBusUsed(false);
								ReceiverMessageHandler(serviceBusReceiver, message);
							});
						},
						ReceiverMessageHandlerOptions
					);
#else
				serviceBusReceiver
					.OnMessage
						(
							message =>
							{
								BusHelper.SetWasPrivateBusUsed(false);
								ReceiverMessageHandler(serviceBusReceiver, message);
							},
							ReceiverMessageHandlerOptions
						);
#endif
			}
		}

		/// <summary>
		/// Using a <see cref="Task"/>, clears all dead letters from the topic and subscription of the 
		/// provided <paramref name="topicName"/> and <paramref name="topicSubscriptionName"/>.
		/// </summary>
		/// <param name="topicName">The name of the topic.</param>
		/// <param name="topicSubscriptionName">The name of the subscription.</param>
		/// <returns></returns>
		protected virtual CancellationTokenSource CleanUpDeadLetters(string topicName, string topicSubscriptionName)
		{
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			int lockIssues = 0;

#if NETSTANDARD2_0
			Action<IMessageReceiver, BrokeredMessage, IMessage> leaveDeadlLetterInQueue = (client, deadLetterBrokeredMessage, deadLetterMessage) =>
#else
			Action<BrokeredMessage, IMessage> leaveDeadlLetterInQueue = (deadLetterBrokeredMessage, deadLetterMessage) =>
#endif
			{
				// Remove message from queue
				try
				{
#if NETSTANDARD2_0
					client.AbandonAsync(deadLetterBrokeredMessage.SystemProperties.LockToken).Wait(1500);
#else
					deadLetterBrokeredMessage.Abandon();
#endif
					lockIssues = 0;
				}
				catch (MessageLockLostException)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId));
				}
				Logger.LogDebug(string.Format("A dead-letter message of type {0} arrived with the id '{1}' but left in the queue due to settings.", deadLetterMessage.GetType().FullName, deadLetterBrokeredMessage.MessageId));
			};
#if NETSTANDARD2_0
			Action<IMessageReceiver, BrokeredMessage> removeDeadlLetterFromQueue = (client, deadLetterBrokeredMessage) =>
#else
			Action<BrokeredMessage> removeDeadlLetterFromQueue = (deadLetterBrokeredMessage) =>
#endif
			{
				// Remove message from queue
				try
				{
#if NETSTANDARD2_0
					client.CompleteAsync(deadLetterBrokeredMessage.SystemProperties.LockToken).Wait(1500);
#else
					deadLetterBrokeredMessage.Complete();
#endif
					lockIssues = 0;
				}
				catch (MessageLockLostException)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for complete for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId));
				}
				Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}' but was removed as processing was skipped due to settings.", deadLetterBrokeredMessage.MessageId));
			};

			Task.Factory.StartNewSafely(() =>
			{
				int loop = 0;
				while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
				{
					lockIssues = 0;
					IEnumerable<BrokeredMessage> brokeredMessages;

					string connectionString = ConnectionString;
					AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
					string deadLetterPath = EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName));
					MessageReceiver client;
					if (!string.IsNullOrWhiteSpace(connectionString))
						client = new MessageReceiver(ConnectionString, deadLetterPath, ReceiveMode.PeekLock);
					else
						client = new MessageReceiver(rbacSettings.Endpoint, deadLetterPath, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()), receiveMode: ReceiveMode.PeekLock);
					Task<IList<BrokeredMessage>> receiveTask = client.ReceiveAsync(1000);
					receiveTask.Wait(10000);
					if (receiveTask.IsCompleted && receiveTask.Result != null)
						brokeredMessages = receiveTask.Result;
					else
						brokeredMessages = Enumerable.Empty<BrokeredMessage>();
#else
					MessagingFactory factory;
#if NET452
					factory = MessagingFactory.CreateFromConnectionString(ConnectionString);
#else
					if (!string.IsNullOrWhiteSpace(connectionString))
						factory = MessagingFactory.CreateFromConnectionString(ConnectionString);
					else
						factory = MessagingFactory.Create(new Uri(rbacSettings.Endpoint), TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, null, rbacSettings.GetDefaultAuthority()));
#endif

					string deadLetterPath = SubscriptionClient.FormatDeadLetterPath(topicName, topicSubscriptionName);
					MessageReceiver client = factory.CreateMessageReceiver(deadLetterPath, ReceiveMode.PeekLock);
					brokeredMessages = client.ReceiveBatch(1000);
#endif

					foreach (BrokeredMessage brokeredMessage in brokeredMessages)
					{
						if (lockIssues > 10)
							break;
						try
						{
							Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}'.", brokeredMessage.MessageId));
#if NET462
							string messageBody = brokeredMessage.GetBody<string>();
#else
							string messageBody = brokeredMessage.GetBodyAsString();
#endif

							// Closure protection
							BrokeredMessage message = brokeredMessage;
							try
							{
								AzureBusHelper.ReceiveEvent
								(
#if NETSTANDARD2_0
									client,
#else
									null,
#endif
									messageBody,
									@event =>
									{
										bool isRequired = BusHelper.IsEventRequired(@event.GetType());
										if (!isRequired)
										{
#if NETSTANDARD2_0
											removeDeadlLetterFromQueue(client, message);
#else
											removeDeadlLetterFromQueue(message);
#endif
										}
										else
										{
#if NETSTANDARD2_0
											leaveDeadlLetterInQueue(client, message, @event);
#else
											leaveDeadlLetterInQueue(message, @event);
#endif
										}
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
#if NETSTANDARD2_0
										removeDeadlLetterFromQueue(client, message);
#else
										removeDeadlLetterFromQueue(message);
#endif
									},
									() => { }
								);
							}
							catch
							{
								AzureBusHelper.ReceiveCommand
								(
#if NETSTANDARD2_0
									client,
#else
									null,
#endif
									messageBody,
									command =>
									{
										bool isRequired = BusHelper.IsEventRequired(command.GetType());
										if (!isRequired)
										{
#if NETSTANDARD2_0
											removeDeadlLetterFromQueue(client, message);
#else
											removeDeadlLetterFromQueue(message);
#endif
										}
										else
										{
#if NETSTANDARD2_0
											leaveDeadlLetterInQueue(client, message, command);
#else
											leaveDeadlLetterInQueue(message, command);
#endif
										}
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
#if NETSTANDARD2_0
										removeDeadlLetterFromQueue(client, message);
#else
										removeDeadlLetterFromQueue(message);
#endif
									},
									() => { }
								);
							}
						}
						catch (Exception exception)
						{
							TelemetryHelper.TrackException(exception, null, telemetryProperties);
							// Indicates a problem, unlock message in queue
							Logger.LogError(string.Format("A dead-letter message arrived with the id '{0}' but failed to be process.", brokeredMessage.MessageId), exception: exception);
							try
							{
#if NETSTANDARD2_0
								client.AbandonAsync(brokeredMessage.SystemProperties.LockToken).Wait(1500);
#else
								brokeredMessage.Abandon();
#endif
							}
							catch (MessageLockLostException)
							{
								lockIssues++;
								Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", brokeredMessage.MessageId));
							}
						}
					}
#if NETSTANDARD2_0
					client.CloseAsync().Wait(1500);
#else
					client.Close();
#endif

					if (loop++ % 5 == 0)
					{
						loop = 0;
						Thread.Yield();
					}
					else
						Thread.Sleep(500);
				}
				try
				{
					brokeredMessageRenewCancellationTokenSource.Dispose();
				}
				catch (ObjectDisposedException) { }
			}, brokeredMessageRenewCancellationTokenSource.Token);

			return brokeredMessageRenewCancellationTokenSource;
		}

#if NETSTANDARD2_0
		DataContractBinarySerializer brokeredMessageSerialiser = new DataContractBinarySerializer(typeof(string));
#endif
		/// <summary>
		/// Create a <see cref="BrokeredMessage"/> with additional properties to aid routing and tracing
		/// </summary>
		protected virtual BrokeredMessage CreateBrokeredMessage<TMessage>(Func<TMessage, string> serialiserFunction, Type messageType, TMessage message)
		{
			string messageBody = serialiserFunction(message);
#if NETSTANDARD2_0
			byte[] messageBodyData;
			using (var stream = new MemoryStream())
			{
				brokeredMessageSerialiser.WriteObject(stream, messageBody);
				stream.Flush();
				stream.Position = 0;
				messageBodyData = stream.ToArray();
			}

			var brokeredMessage = new BrokeredMessage(messageBodyData)
#else
			var brokeredMessage = new BrokeredMessage(messageBody)
#endif
			{
				CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
			};
			brokeredMessage.AddUserProperty("CorrelationId", brokeredMessage.CorrelationId);
			brokeredMessage.AddUserProperty("Type", messageType.FullName);
			brokeredMessage.AddUserProperty("Source", string.Format("{0}/{1}/{2}/{3}", Logger.LoggerSettings.ModuleName, Logger.LoggerSettings.Instance, Logger.LoggerSettings.Environment, Logger.LoggerSettings.EnvironmentInstance));
#if NETSTANDARD2_0
			brokeredMessage.AddUserProperty("Framework", ".NET Core");
#else
			brokeredMessage.AddUserProperty("Framework", ".NET Framework");
#endif

			// see https://github.com/Chinchilla-Software-Com/CQRS/wiki/Inter-process-function-security</remarks>
			string configurationKey = string.Format("{0}.SigningToken", messageType.FullName);
			string signingToken;
			HashAlgorithm signer = Signer.Create();
			if (!ConfigurationManager.TryGetSetting(configurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
				if (!ConfigurationManager.TryGetSetting(SigningTokenConfigurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
					signingToken = Guid.Empty.ToString("N");
			if (!string.IsNullOrWhiteSpace(signingToken))
				using (var hashStream = new MemoryStream(Encoding.UTF8.GetBytes($"{signingToken}{messageBody}")))
					brokeredMessage.AddUserProperty("Signature", Convert.ToBase64String(signer.ComputeHash(hashStream)));

			try
			{
				var stackTrace = new StackTrace();
				StackFrame[] stackFrames = stackTrace.GetFrames();
				if (stackFrames != null)
				{
					foreach (StackFrame frame in stackFrames)
					{
						MethodBase method = frame.GetMethod();
						if (method.ReflectedType == null)
							continue;

						try
						{
							if (ExclusionNamespaces.All(@namespace => !method.ReflectedType.FullName.StartsWith(@namespace)))
							{
								brokeredMessage.AddUserProperty("Source-Method", string.Format("{0}.{1}", method.ReflectedType.FullName, method.Name));
								break;
							}
						}
						catch
						{
							// Just move on
						}
					}
				}
			}
			catch
			{
				// Just move on
			}

			return brokeredMessage;
		}

		/// <summary>
		/// Extract any telemetry properties from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual IDictionary<string, string> ExtractTelemetryProperties(BrokeredMessage message, string baseCommunicationType)
		{
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", baseCommunicationType } };
			object value;
			if (message.TryGetUserPropertyValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			if (message.TryGetUserPropertyValue("Source", out value))
				telemetryProperties.Add("MessageSource", value.ToString());
			if (message.TryGetUserPropertyValue("Source-Method", out value))
				telemetryProperties.Add("MessageSourceMethod", value.ToString());
			if (message.TryGetUserPropertyValue("CorrelationId", out value) && !telemetryProperties.ContainsKey("CorrelationId"))
				telemetryProperties.Add("CorrelationId", value.ToString());

			return telemetryProperties;
		}

		/// <summary>
		/// Extract the signature from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual string ExtractSignature(BrokeredMessage message)
		{
			object value;
			if (message.TryGetUserPropertyValue("Signature", out value))
				return value.ToString();
			return null;
		}
	}
}