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
using Cqrs.Domain;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Domain;
using cdmdotnet.Logging;

namespace Northwind.Domain.Orders.Commands.Handlers
{
	[GeneratedCode("CQRS UML Code Generator", "1.601.909")]
	public  partial class CreateOrderCommandHandler
		
		: ICommandHandler<Cqrs.Authentication.ISingleSignOnToken, CreateOrderCommand>
	{
		protected IUnitOfWork<Cqrs.Authentication.ISingleSignOnToken> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }


		public CreateOrderCommandHandler(IUnitOfWork<Cqrs.Authentication.ISingleSignOnToken> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in CreateOrder>

		public void Handle(CreateOrderCommand command)
		{
			Order item = null;
			OnCreateOrder(command, ref item);
			if (item == null)
			{
				item = new Order(DependencyResolver, Logger, command.Rsn == Guid.Empty ? Guid.NewGuid() : command.Rsn);
				UnitOfWork.Add(item);
			}
			item.CreateOrder(command.OrderId, command.CustomerId, command.EmployeeId, command.OrderDate, command.RequiredDate, command.ShippedDate, command.ShipViaId, command.Freight, command.ShipName, command.ShipAddress, command.ShipCity, command.ShipRegion, command.ShipPostalCode, command.ShipCountry);
			OnCreatedOrder(command, item);
			OnAddToUnitOfWork(command, item);
			UnitOfWork.Add(item);
			OnAddedToUnitOfWork(command, item);
			OnCommit(command, item);
			UnitOfWork.Commit();
			OnCommited(command, item);
		}

		#endregion

		partial void OnCreateOrder(CreateOrderCommand command, ref Order item);

		partial void OnCreatedOrder(CreateOrderCommand command, Order item);

		partial void OnAddToUnitOfWork(CreateOrderCommand command, Order item);

		partial void OnAddedToUnitOfWork(CreateOrderCommand command, Order item);

		partial void OnCommit(CreateOrderCommand command, Order item);

		partial void OnCommited(CreateOrderCommand command, Order item);
	}
}
