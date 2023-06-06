// Licensed to Elasticsearch B.V under
// one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information
//
// <copyright file="ITypedDeliveryHandlerShimAction.cs" company="Datadog">
// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2017 Datadog, Inc.
// </copyright>

using System.ComponentModel;
using Elastic.Apm.Profiler.Managed.DuckTyping;

namespace Elastic.Apm.Profiler.Managed.Integrations.Kafka
{
	/// <summary>
	/// TypedDeliveryHandlerShim_Action for duck-typing
	/// </summary>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ITypedDeliveryHandlerShimAction
	{
		/// <summary>
		/// Sets the delivery report handler
		/// </summary>
		[DuckField]
		public object Handler { set; }
	}
}