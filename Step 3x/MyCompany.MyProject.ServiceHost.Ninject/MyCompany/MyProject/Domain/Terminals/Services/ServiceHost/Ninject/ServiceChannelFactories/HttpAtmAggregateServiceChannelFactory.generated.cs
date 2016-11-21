﻿










//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#region Copyright
// -----------------------------------------------------------------------
// <copyright company="cdmdotnet Limited">
//     Copyright cdmdotnet Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion
using Cqrs.Domain;
using MyCompany.MyProject.Domain.Terminals.Events;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Cqrs.Services;
using MyCompany.MyProject.Domain.Terminals.Services;

namespace MyCompany.MyProject.Domain.Terminals.Services.ServiceHost.Ninject.ServiceChannelFactories
{

	/// <summary>
	/// A <see cref="ServiceChannelFactory{TService}"/> for using  <see cref="IAtmService.CreateAtm"/> via WCF
	/// </summary>
	public partial class HttpAtmServiceChannelFactory : ServiceChannelFactory<IAtmService>
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpAtmServiceChannelFactory"/> class with the default endpoint configuration name of HttpAtmService.
		/// </summary>
		public HttpAtmServiceChannelFactory()
			: this("HttpAtmService")
		{
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpAtmServiceChannelFactory"/> class with a specified endpoint configuration name.
		/// </summary>
		/// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
		public HttpAtmServiceChannelFactory(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		protected override void RegisterDataContracts()
		{
			RegisterAggregateServiceDataContracts();
			RegisterServiceDataContracts();
		}

		partial void RegisterServiceDataContracts();
		partial void RegisterAggregateServiceDataContracts();
		partial void RegisterAggregateServiceDataContracts()
		{
	



		}
	}
}
