using System;
using UnityEngine;

namespace Chronos.Reflection
{
	[Serializable]
	public class AnimatorParameter
	{
		[SerializeField]
		private Animator _target;
		/// <summary>
		/// The animator containing the member.
		/// </summary>
		public Animator target
		{
			get { return _target; }
			set { _target = value; isLinked = false; }
		}

		[SerializeField]
		private string _name;
		/// <summary>
		/// The name of the parameter.
		/// </summary>
		public string name
		{
			get { return _name; }
			set { _name = value; isLinked = false; }
		}

		/// <summary>
		/// The underlying animator controller parameter.
		/// </summary>
		public AnimatorControllerParameter parameterInfo { get; private set; }

		/// <summary>
		/// Indicates whether the parameter has been found and analyzed.
		/// </summary>
		public bool isLinked { get; private set; }

		/// <summary>
		/// Indicates whether the animator parameter has been properly assigned.
		/// </summary>
		public bool isAssigned
		{
			get
			{
				return target != null && !string.IsNullOrEmpty(name);
			}
		}

		public AnimatorParameter() { }

		public AnimatorParameter(string name)
		{
			this.name = name;
		}

		public AnimatorParameter(string name, Animator target)
		{
			this.name = name;
			this.target = target;

			Link();
		}

		/// <summary>
		/// Fetches and caches the parameter.
		/// </summary>
		public void Link()
		{
			if (target == null)
			{
				throw new UnityException("Target has not been defined.");
			}

			foreach (AnimatorControllerParameter parameter in target.parameters)
			{
				if (parameter.name == name)
				{
					parameterInfo = parameter;
					return;
				}
			}

			throw new UnityException(string.Format("Animator parameter not found: '{0}'.", name));
		}

		/// <summary>
		/// Fetches and caches the parameter if it is not already present.
		/// </summary>
		protected void EnsureLinked()
		{
			if (!isLinked)
			{
				Link();
			}
		}

		/// <summary>
		/// Retrieves the value of the parameter.
		/// </summary>
		public object Get()
		{
			EnsureLinked();

			switch (parameterInfo.type)
			{
				case AnimatorControllerParameterType.Float: return target.GetFloat(parameterInfo.nameHash);
				case AnimatorControllerParameterType.Int: return target.GetInteger(parameterInfo.nameHash);
				case AnimatorControllerParameterType.Bool: return target.GetBool(parameterInfo.nameHash);
				case AnimatorControllerParameterType.Trigger: throw new UnityException("Cannot get the value of a trigger parameter.");
				default: throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Retrieves the value of the parameter casted to the specified type.
		/// </summary>
		public T Get<T>() where T : struct
		{
			return (T)Get();
		}

		/// <summary>
		/// Assigns a new value to the parameter.
		/// </summary>
		public void Set(object value)
		{
			EnsureLinked();

			switch (parameterInfo.type)
			{
				case AnimatorControllerParameterType.Float: target.SetFloat(parameterInfo.nameHash, (float)value); break;
				case AnimatorControllerParameterType.Int: target.SetInteger(parameterInfo.nameHash, (int)value); break;
				case AnimatorControllerParameterType.Bool: target.SetBool(parameterInfo.nameHash, (bool)value); break;
				case AnimatorControllerParameterType.Trigger: throw new UnityException("Cannot set the value of a trigger parameter.");
				default: throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Triggers the parameter.
		/// </summary>
		public void SetTrigger()
		{
			EnsureLinked();

			if (parameterInfo.type != AnimatorControllerParameterType.Trigger)
			{
				throw new UnityException("Parameter is not a trigger.");
			}

			target.SetTrigger(parameterInfo.nameHash);
		}

		/// <summary>
		/// Resets the trigger on the parameter.
		/// </summary>
		public void ResetTrigger()
		{
			EnsureLinked();

			if (parameterInfo.type != AnimatorControllerParameterType.Trigger)
			{
				throw new UnityException("Parameter is not a trigger.");
			}

			target.ResetTrigger(parameterInfo.nameHash);
		}

		/// <summary>
		/// The type of the parameter, or null if it is a trigger.
		/// </summary>
		public Type type
		{
			get
			{
				switch (parameterInfo.type)
				{
					case AnimatorControllerParameterType.Float: return typeof(float);
					case AnimatorControllerParameterType.Int: return typeof(int);
					case AnimatorControllerParameterType.Bool: return typeof(bool);
					case AnimatorControllerParameterType.Trigger: return null;
					default: throw new NotImplementedException();
				}
			}
		}

		public bool Corresponds(AnimatorParameter other)
		{
			return
				(other != null || !this.isAssigned) &&
				this.target == other.target &&
				this.name == other.name;
		}
	}
}
