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
using System.Xml;
using Cqrs.Events;
using Cqrs.Services;
using Northwind.Domain.Orders.Entities;

namespace Northwind.Domain.Orders.Services
{
	/// <summary>
	/// A <see cref="DataContractResolver"/> for using  <see cref="IOrderService.CreateOrder"/> via WCF
	/// </summary>
	[GeneratedCode("CQRS UML Code Generator", "1.601.932")]
	public partial class OrderServiceCreateOrderParametersResolver : ServiceParameterResolver<IOrderService, Cqrs.Authentication.ISingleSignOnToken>
	{
		public OrderServiceCreateOrderParametersResolver(ISingleSignOnTokenResolver singleSignOnTokenResolver, IEventDataResolver eventDataResolver)
			: base(singleSignOnTokenResolver, eventDataResolver)
		{
		}

		public OrderServiceCreateOrderParametersResolver()
			: base(new Cqrs.Authentication.SingleSignOnTokenResolver(), new EventDataResolver<Cqrs.Authentication.ISingleSignOnToken>())
		{
		}

		public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceCreateOrderParameters");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			if (dataContractType == typeof(ServiceResponseWithResultData<OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceCreateOrderResponse");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			return base.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}

		protected override bool TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace)
		{
			bool result = false;
			TryResolveUnResolvedType(dataContractType, declaredType, knownTypeResolver, ref typeName, ref typeNamespace, ref result);
			return result;
		}

		partial void TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace, ref bool result);

		/// <summary>
		/// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
		/// </summary>
		/// <returns>
		/// The type the xsi:type name and namespace is mapped to. 
		/// </returns>
		/// <param name="typeName">The xsi:type name to map.</param><param name="typeNamespace">The xsi:type namespace to map.</param><param name="declaredType">The type declared in the data contract.</param><param name="knownTypeResolver">The known type resolver.</param>
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeNamespace == ServiceNamespace)
			{
				if (typeName == "OrderServiceCreateOrderParameters")
					return typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>);

				if (typeName == "OrderServiceCreateOrderResponse")
					return typeof(ServiceResponseWithResultData<OrderEntity>);
			}

			return base.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		protected override Type ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			Type result = null;
			ResolveUnResolvedName(typeName, typeNamespace, declaredType, knownTypeResolver, ref result);
			return result;
		}

		partial void ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver, ref Type result);

		public static void RegisterDataContracts()
		{
			WcfDataContractResolverConfiguration.Current.RegisterDataContract<IOrderService, OrderServiceCreateOrderParametersResolver>("CreateOrder");
		}
	}

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using  <see cref="IOrderService.UpdateOrder"/> via WCF
	/// </summary>
	[GeneratedCode("CQRS UML Code Generator", "1.601.932")]
	public partial class OrderServiceUpdateOrderParametersResolver : ServiceParameterResolver<IOrderService, Cqrs.Authentication.ISingleSignOnToken>
	{
		public OrderServiceUpdateOrderParametersResolver(ISingleSignOnTokenResolver singleSignOnTokenResolver, IEventDataResolver eventDataResolver)
			: base(singleSignOnTokenResolver, eventDataResolver)
		{
		}

		public OrderServiceUpdateOrderParametersResolver()
			: base(new Cqrs.Authentication.SingleSignOnTokenResolver(), new EventDataResolver<Cqrs.Authentication.ISingleSignOnToken>())
		{
		}

		public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceUpdateOrderParameters");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			if (dataContractType == typeof(ServiceResponseWithResultData<OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceUpdateOrderResponse");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			return base.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}

		protected override bool TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace)
		{
			bool result = false;
			TryResolveUnResolvedType(dataContractType, declaredType, knownTypeResolver, ref typeName, ref typeNamespace, ref result);
			return result;
		}

		partial void TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace, ref bool result);

		/// <summary>
		/// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
		/// </summary>
		/// <returns>
		/// The type the xsi:type name and namespace is mapped to. 
		/// </returns>
		/// <param name="typeName">The xsi:type name to map.</param><param name="typeNamespace">The xsi:type namespace to map.</param><param name="declaredType">The type declared in the data contract.</param><param name="knownTypeResolver">The known type resolver.</param>
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeNamespace == ServiceNamespace)
			{
				if (typeName == "OrderServiceUpdateOrderParameters")
					return typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>);

				if (typeName == "OrderServiceUpdateOrderResponse")
					return typeof(ServiceResponseWithResultData<OrderEntity>);
			}

			return base.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		protected override Type ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			Type result = null;
			ResolveUnResolvedName(typeName, typeNamespace, declaredType, knownTypeResolver, ref result);
			return result;
		}

		partial void ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver, ref Type result);

		public static void RegisterDataContracts()
		{
			WcfDataContractResolverConfiguration.Current.RegisterDataContract<IOrderService, OrderServiceUpdateOrderParametersResolver>("UpdateOrder");
		}
	}

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using  <see cref="IOrderService.DeleteOrder"/> via WCF
	/// </summary>
	[GeneratedCode("CQRS UML Code Generator", "1.601.932")]
	public partial class OrderServiceDeleteOrderParametersResolver : ServiceParameterResolver<IOrderService, Cqrs.Authentication.ISingleSignOnToken>
	{
		public OrderServiceDeleteOrderParametersResolver(ISingleSignOnTokenResolver singleSignOnTokenResolver, IEventDataResolver eventDataResolver)
			: base(singleSignOnTokenResolver, eventDataResolver)
		{
		}

		public OrderServiceDeleteOrderParametersResolver()
			: base(new Cqrs.Authentication.SingleSignOnTokenResolver(), new EventDataResolver<Cqrs.Authentication.ISingleSignOnToken>())
		{
		}

		public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceDeleteOrderParameters");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			if (dataContractType == typeof(ServiceResponseWithResultData<OrderEntity>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("OrderServiceDeleteOrderResponse");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			return base.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}

		protected override bool TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace)
		{
			bool result = false;
			TryResolveUnResolvedType(dataContractType, declaredType, knownTypeResolver, ref typeName, ref typeNamespace, ref result);
			return result;
		}

		partial void TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace, ref bool result);

		/// <summary>
		/// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
		/// </summary>
		/// <returns>
		/// The type the xsi:type name and namespace is mapped to. 
		/// </returns>
		/// <param name="typeName">The xsi:type name to map.</param><param name="typeNamespace">The xsi:type namespace to map.</param><param name="declaredType">The type declared in the data contract.</param><param name="knownTypeResolver">The known type resolver.</param>
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeNamespace == ServiceNamespace)
			{
				if (typeName == "OrderServiceDeleteOrderParameters")
					return typeof(ServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderEntity>);

				if (typeName == "OrderServiceDeleteOrderResponse")
					return typeof(ServiceResponseWithResultData<OrderEntity>);
			}

			return base.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		protected override Type ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			Type result = null;
			ResolveUnResolvedName(typeName, typeNamespace, declaredType, knownTypeResolver, ref result);
			return result;
		}

		partial void ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver, ref Type result);

		public static void RegisterDataContracts()
		{
			WcfDataContractResolverConfiguration.Current.RegisterDataContract<IOrderService, OrderServiceDeleteOrderParametersResolver>("DeleteOrder");
		}
	}

}
