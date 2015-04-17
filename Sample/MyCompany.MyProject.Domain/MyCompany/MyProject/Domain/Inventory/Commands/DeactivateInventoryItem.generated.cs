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
using System.Runtime.Serialization;
using Cqrs.Commands;

namespace MyCompany.MyProject.Domain.Inventory.Commands
{
	[GeneratedCode("CQRS UML Code Generator", "1.500.497.383")]
	public  partial class DeactivateInventoryItemCommand : ICommand<System.Guid>
	{
		#region Implementation of ICommand

		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<System.Guid>

		[DataMember]
		public System.Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrolationId { get; set; }

		#endregion

		[DataMember]
		public Guid Rsn { get; set; }

		public DeactivateInventoryItemCommand(Guid rsn)
		{
			Rsn = rsn;
		}
	}
}
