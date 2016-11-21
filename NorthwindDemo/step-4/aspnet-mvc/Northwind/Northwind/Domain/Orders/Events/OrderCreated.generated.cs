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
using Cqrs.Events;
using Cqrs.Messages;

namespace Northwind.Domain.Orders.Events
{
	[GeneratedCode("CQRS UML Code Generator", "1.601.909")]
	public  partial class OrderCreated : IEvent<Cqrs.Authentication.ISingleSignOnToken>
	{
		#region Implementation of IEvent

		[DataMember]
		public Guid Id
		{
			get
			{
				return Rsn;
			}
			set
			{
				Rsn = value;
			}
		}

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<Cqrs.Authentication.ISingleSignOnToken>

		public Cqrs.Authentication.ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		[DataMember]
		public FrameworkType Framework { get; set; }

		#endregion

		[DataMember]
		public Guid Rsn { get; set; }

		[DataMember]
		public int OrderId { get; private set; }

		[DataMember]
		public string CustomerId { get; private set; }

		[DataMember]
		public int? EmployeeId { get; private set; }

		[DataMember]
		public DateTime? OrderDate { get; private set; }

		[DataMember]
		public DateTime? RequiredDate { get; private set; }

		[DataMember]
		public DateTime? ShippedDate { get; private set; }

		[DataMember]
		public int? ShipViaId { get; private set; }

		[DataMember]
		public decimal? Freight { get; private set; }

		[DataMember]
		public string ShipName { get; private set; }

		[DataMember]
		public string ShipAddress { get; private set; }

		[DataMember]
		public string ShipCity { get; private set; }

		[DataMember]
		public string ShipRegion { get; private set; }

		[DataMember]
		public string ShipPostalCode { get; private set; }

		[DataMember]
		public string ShipCountry { get; private set; }


		public OrderCreated(Guid rsn, int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			Rsn = rsn;
			OrderId = orderId;
			CustomerId = customerId;
			EmployeeId = employeeId;
			OrderDate = orderDate;
			RequiredDate = requiredDate;
			ShippedDate = shippedDate;
			ShipViaId = shipViaId;
			Freight = freight;
			ShipName = shipName;
			ShipAddress = shipAddress;
			ShipCity = shipCity;
			ShipRegion = shipRegion;
			ShipPostalCode = shipPostalCode;
			ShipCountry = shipCountry;
		}
	}
}
