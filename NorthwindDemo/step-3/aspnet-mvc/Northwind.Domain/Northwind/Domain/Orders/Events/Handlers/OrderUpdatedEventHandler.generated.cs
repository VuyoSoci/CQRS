﻿//------------------------------------------------------------------------------
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
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Cqrs.Events;
using Cqrs.Domain;

namespace Northwind.Domain.Orders.Events.Handlers
{
	[GeneratedCode("CQRS UML Code Generator", "1.500.0.1")]
	public  partial class OrderUpdatedEventHandler : IEventHandler<Cqrs.Authentication.ISingleSignOnToken, OrderUpdated>
	{
		#region Implementation of IEventHandler<in OrderUpdated>

		public void Handle(OrderUpdated @event)
		{
			OnHandle(@event);
		}

		#endregion

		partial void OnHandle(OrderUpdated @event);
	}
}
