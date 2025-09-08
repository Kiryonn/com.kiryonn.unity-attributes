using System;
using UnityEngine;

namespace Kiryonn.UnityAttributes
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class SerializeAbstractFieldAttribute : PropertyAttribute { }
}
