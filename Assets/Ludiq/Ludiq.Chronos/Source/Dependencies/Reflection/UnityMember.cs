using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection
{
	[Serializable]
	public abstract class UnityMember
	{
		[SerializeField]
		private UnityObject _target;
		/// <summary>
		/// The object containing the member.
		/// </summary>
		public UnityObject target
		{
			get { return _target; }
			set { _target = value; isTargeted = false; }
		}

		[SerializeField]
		private string _component;
		/// <summary>
		/// The name of the component containing the member, if the target is a GameObject.
		/// </summary>
		public string component
		{
			get { return _component; }
			set { _component = value; isTargeted = false; isReflected = false; }
		}

		[SerializeField]
		private string _name;
		/// <summary>
		/// The name of the member.
		/// </summary>
		public string name
		{
			get { return _name; }
			set { _name = value; isReflected = false; }
		}

		/// <summary>
		/// Indicates whether the reflection target has been found and cached.
		/// </summary>
		protected bool isTargeted { get; private set; }

		/// <summary>
		/// Indicates whether the reflection data has been found and cached.
		/// </summary>
		public bool isReflected { get; protected set; }

		/// <summary>
		/// Indicates whether the member has been properly assigned.
		/// </summary>
		public bool isAssigned
		{
			get
			{
				return target != null && !string.IsNullOrEmpty(name);
			}
		}

		/// <summary>
		/// The object on which to perform reflection.
		/// For GameObjects and Component targets, this is the component of type <see cref="UnityMember.component"/> or the object itself if null.
		/// For ScriptableObjects targets, this is the object itself. 
		/// Other Unity Objects are not supported.
		/// </summary>
		protected UnityObject reflectionTarget { get; private set; }

		#region Constructors

		public UnityMember() { }

		public UnityMember(string name)
		{
			this.name = name;
		}

		public UnityMember(string name, UnityObject target)
		{
			this.name = name;
			this.target = target;

			Reflect();
		}

		public UnityMember(string component, string name)
		{
			this.component = component;
			this.name = name;
		}

		public UnityMember(string component, string name, UnityObject target)
		{
			this.component = component;
			this.name = name;
			this.target = target;

			Reflect();
		}

		#endregion

		/// <summary>
		/// Gathers and caches the reflection target for the member.
		/// </summary>
		protected void Target()
		{
			if (target == null)
			{
				throw new NullReferenceException("Target has not been specified.");
			}

			GameObject targetAsGameObject = target as GameObject;
			Component targetAsComponent = target as Component;

			if (targetAsGameObject != null || targetAsComponent != null)
			{
				// The target is a GameObject or a Component.

				if (!string.IsNullOrEmpty(component))
				{
					// If a component is specified, look for it on the target.

					Component componentObject;

					if (targetAsGameObject != null)
					{
						componentObject = targetAsGameObject.GetComponent(component);
					}
					else // if (targetAsComponent != null)
					{
						componentObject = targetAsComponent.GetComponent(component);
					}

					if (componentObject == null)
					{
						throw new UnityReflectionException(string.Format("Target object does not contain a component of type '{0}'.", component));
					}

					reflectionTarget = componentObject;
					return;
				}
				else
				{
					// Otherwise, return the GameObject itself.

					if (targetAsGameObject != null)
					{
						reflectionTarget = targetAsGameObject;
					}
					else // if (targetAsComponent != null)
					{
						reflectionTarget = targetAsComponent.gameObject;
					}

					return;
				}
			}

			ScriptableObject scriptableObject = target as ScriptableObject;

			if (scriptableObject != null)
			{
				// The real target is directly the ScriptableObject target.

				reflectionTarget = scriptableObject;
				return;
			}

			throw new UnityReflectionException("Target should be a GameObject, a Component or a ScriptableObject.");
		}

		/// <summary>
		/// Gathers and caches the reflection data for the member.
		/// </summary>
		public abstract void Reflect();

		/// <summary>
		/// Gathers the reflection data if it is not alreadypresent.
		/// </summary>
		public void EnsureReflected()
		{
			if (!isReflected)
			{
				Reflect();
			}
		}

		/// <summary>
		/// Gathers the reflection target if it is not already present.
		/// </summary>
		public void EnsureTargeted()
		{
			if (!isTargeted)
			{
				Target();
			}
		}

		/// <summary>
		/// Asserts that the member has been properly assigned.
		/// </summary>
		public void EnsureAssigned()
		{
			if (!isAssigned)
			{
				throw new UnityReflectionException("Member hasn't been properly assigned.");
			}
		}

		public virtual bool Corresponds(UnityMember other)
		{
			return
				(other != null || !this.isAssigned) &&
				this.target == other.target &&
				this.component == other.component &&
				this.name == other.name;
		}
	}
}