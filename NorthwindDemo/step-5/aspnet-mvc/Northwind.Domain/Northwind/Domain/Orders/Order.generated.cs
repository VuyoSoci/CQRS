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
using Northwind.Domain.Orders.Events;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Snapshots;
using Northwind.Domain.Orders.Events;

namespace Northwind.Domain.Orders
{
	[GeneratedCode("CQRS UML Code Generator", "1.601.932")]
	public  partial class Order : AggregateRoot<Cqrs.Authentication.ISingleSignOnToken>
	{
		public Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		public bool IsLogicallyDeleted {get; set;}

		#region Implementation of IMessageWithAuthenticationToken<Cqrs.Authentication.ISingleSignOnToken>

		public Cqrs.Authentication.ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Log { get; private set; }

		public int OrderId { get; private set; }

		public string CustomerId { get; private set; }

		public int? EmployeeId { get; private set; }

		public DateTime? OrderDate { get; private set; }

		public DateTime? RequiredDate { get; private set; }

		public DateTime? ShippedDate { get; private set; }

		public int? ShipViaId { get; private set; }

		public decimal? Freight { get; private set; }

		public string ShipName { get; private set; }

		public string ShipAddress { get; private set; }

		public string ShipCity { get; private set; }

		public string ShipRegion { get; private set; }

		public string ShipPostalCode { get; private set; }

		public string ShipCountry { get; private set; }

// ReSharper disable UnusedMember.Local
		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private Order()
		{

		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private Order(IDependencyResolver dependencyResolver, ILogger log)
		{
			DependencyResolver = dependencyResolver;
			Log = log;
		}
// ReSharper restore UnusedMember.Local

		public Order(IDependencyResolver dependencyResolver, ILogger log, Guid rsn)
		{
			DependencyResolver = dependencyResolver;
			Log = log;
			Rsn = rsn;
		}

		public virtual void RemoveProduct()
		{
			Log.LogDebug("Entered", "Order/RemoveProduct");
			OnRemoveProduct();
			Log.LogDebug("Exited", "Order/RemoveProduct");
		}
		partial void OnRemoveProduct();


		/// <summary>
		/// Create a new instance of the <see cref="Order"/>
		/// </summary>
		public virtual void CreateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			Log.LogDebug("Entered", "Order/CreateOrder");
			Log.LogDebug("Pre", "Order/OnCreateOrder");
			OnCreateOrder(orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry);
			Log.LogDebug("Post", "Order/OnCreateOrder");
			Log.LogDebug("Pre", "Order/ApplyChange/Create");
			ApplyChange(new OrderCreated(Rsn, orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry));
			Log.LogDebug("Post", "Order/ApplyChange");
			Log.LogDebug("Pre", "Order/OnCreatedOrder");
			OnCreatedOrder(orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry);
			Log.LogDebug("Post", "Order/OnCreatedOrder");
			Log.LogDebug("Exited", "Order/CreateOrder");
		}

		partial void OnCreateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry);

		partial void OnCreatedOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry);

		private void Apply(OrderCreated @event)
		{
			OrderId = @event.OrderId;
			CustomerId = @event.CustomerId;
			EmployeeId = @event.EmployeeId;
			OrderDate = @event.OrderDate;
			RequiredDate = @event.RequiredDate;
			ShippedDate = @event.ShippedDate;
			ShipViaId = @event.ShipViaId;
			Freight = @event.Freight;
			ShipName = @event.ShipName;
			ShipAddress = @event.ShipAddress;
			ShipCity = @event.ShipCity;
			ShipRegion = @event.ShipRegion;
			ShipPostalCode = @event.ShipPostalCode;
			ShipCountry = @event.ShipCountry;
		}

		/// <summary>
		/// Update an existing instance of the <see cref="Order"/>
		/// </summary>
		public virtual void UpdateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			Log.LogDebug("Entered", "Order/UpdateOrder");
			Log.LogDebug("Pre", "Order/OnUpdateOrder");
			OnUpdateOrder(orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry);
			Log.LogDebug("Post", "Order/OnUpdateOrder");
			Log.LogDebug("Pre", "Order/ApplyChange/Update");
			ApplyChange(new OrderUpdated(Rsn, orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry));
			Log.LogDebug("Post", "Order/ApplyChange");
			Log.LogDebug("Pre", "Order/OnUpdatedOrder");
			OnUpdatedOrder(orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry);
			Log.LogDebug("Post", "Order/OnUpdatedOrder");
			Log.LogDebug("Exited", "Order/UpdateOrder");
		}

		partial void OnUpdateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry);

		partial void OnUpdatedOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry);

		private void Apply(OrderUpdated @event)
		{
			OrderId = @event.OrderId;
			CustomerId = @event.CustomerId;
			EmployeeId = @event.EmployeeId;
			OrderDate = @event.OrderDate;
			RequiredDate = @event.RequiredDate;
			ShippedDate = @event.ShippedDate;
			ShipViaId = @event.ShipViaId;
			Freight = @event.Freight;
			ShipName = @event.ShipName;
			ShipAddress = @event.ShipAddress;
			ShipCity = @event.ShipCity;
			ShipRegion = @event.ShipRegion;
			ShipPostalCode = @event.ShipPostalCode;
			ShipCountry = @event.ShipCountry;
		}

		/// <summary>
		/// Logically delete an existing instance of the <see cref="Order"/>
		/// </summary>
		public virtual void DeleteOrder()
		{
			Log.LogDebug("Entered", "Order/DeleteOrder");
			Log.LogDebug("Pre", "Order/OnDeleteOrder");
			OnDeleteOrder();
			Log.LogDebug("Post", "Order/OnDeleteOrder");
			Log.LogDebug("Pre", "Order/ApplyChange/Delete");
			ApplyChange(new OrderDeleted(Rsn));
			Log.LogDebug("Post", "Order/ApplyChange");
			Log.LogDebug("Pre", "Order/OnDeletedOrder");
			OnDeletedOrder();
			Log.LogDebug("Post", "Order/OnDeletedOrder");
			Log.LogDebug("Exited", "Order/DeleteOrder");
		}

		partial void OnDeleteOrder();

		partial void OnDeletedOrder();

		private void Apply(OrderDeleted @event)
		{
			IsLogicallyDeleted = true;
		}

		public class OrderSnapshot : Snapshot
		{
			public bool IsLogicallyDeleted {get; set;}

			public int OrderId { get; private set; }

			public string CustomerId { get; private set; }

			public int? EmployeeId { get; private set; }

			public DateTime? OrderDate { get; private set; }

			public DateTime? RequiredDate { get; private set; }

			public DateTime? ShippedDate { get; private set; }

			public int? ShipViaId { get; private set; }

			public decimal? Freight { get; private set; }

			public string ShipName { get; private set; }

			public string ShipAddress { get; private set; }

			public string ShipCity { get; private set; }

			public string ShipRegion { get; private set; }

			public string ShipPostalCode { get; private set; }

			public string ShipCountry { get; private set; }
		}
	}
}
