using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GlobalClockAttribute : PropertyAttribute
{
	public GlobalClockAttribute() { }
}
