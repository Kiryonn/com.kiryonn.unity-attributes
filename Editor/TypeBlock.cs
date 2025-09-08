using System;
using System.Collections.Generic;
using System.Linq;
using Kiryonn.Searchables;
using UnityEngine.UIElements;

namespace Kiryonn.UnityAttributes.Editor
{

	/// <summary>
	/// Helper class to represent a node in the generic type tree.
	/// </summary>
	public class TypeBlock : VisualElement
	{
		public event Action<TypeBlock> OnTypeSelected;
		public TypeBlock Parent;
		private readonly SearchableDropdown _dropdown;

		private Type _selectedType;
		/// <summary>
		/// Can be a concrete type or a generic type definition
		/// </summary>
		public Type SelectedType
		{
			get => _selectedType;
			set
			{
				var i = Choices.IndexOf(value);
				_dropdown.index = i;
				// fixes race condition
				_selectedType = Choices[i];
			}
		}
		public List<TypeBlock> ChildNodes = new();
		public readonly List<Type> Choices;
		public int Depth => Parent == null ? 0 : Parent.Depth + 1;

		/// <summary>
		/// Creates a new dropdown node with a container and searchable dropdown element.
		/// </summary>
		/// <param name="parent">The parent node of this one.</param>
		/// <param name="choices">The list of acceptable types.</param>
		/// <param name="label">The label for the dropdown.</param>
		/// <param name="maxVisibleItems">The maximum number of items to show in the dropdown list.</param>
		public TypeBlock(TypeBlock parent, List<Type> choices, string label, int maxVisibleItems, Type initialType = null)
		{
			if (choices[0] != null)
				choices.Insert(0, null);

			// Set properties
			Choices = choices;
			Parent = parent;
			_selectedType = initialType;

			var index = choices.FindIndex(t => t == initialType);
			// Add Visual Elements
			_dropdown = new SearchableDropdown(label, choices.Select(GetTypeNameWithNamespace))
			{
				MaxVisibleItems = maxVisibleItems,
				index = index // Trigers initial selection
			};
			Add(_dropdown);
			parent?.AddChild(this);

			// events
			_dropdown.RegisterValueChangedCallback(OnDropdownChanged);

			// Apply Style
			style.marginLeft = Depth * 20;
			style.marginTop = 2;
			style.marginBottom = 2;
			style.paddingTop = 2;
			style.paddingBottom = 2;
		}

		/// <summary>
		/// Called when the dropdown selection changes.
		/// </summary>
		/// <remarks>Only called when there's an actual change</remarks>
		/// <param name="evt">The change event.</param>
		private void OnDropdownChanged(ChangeEvent<SearchableDropdown.State> evt)
		{
			var index = evt.newValue.index;
			_selectedType = index > 0 ? Choices[index] : null;
			OnTypeSelected?.Invoke(this);
		}

		/// <summary>
		/// Clears all child branches.
		/// </summary>
		public void ClearChildren()
		{
			ChildNodes.ForEach(child => child.RemoveFromHierarchy());
			ChildNodes.Clear();
		}

		private void AddChild(TypeBlock child)
		{
			ChildNodes.Add(child);
			Add(child);
		}

		/// <summary>
		/// Recursively checks if there are any undefined generic types in this branch or its children.
		/// </summary>
		public bool HasUndefinedTypes()
		{
			return (_selectedType == null) || (_selectedType.IsGenericTypeDefinition && ChildNodes.Any(c => c.HasUndefinedTypes()));
		}

		/// <summary>
		/// Recursively searches for the first <see cref="TypeBlock"/> that matches the given predicate.
		/// </summary>
		/// <param name="predicate">The predicate to match.</param>
		public TypeBlock FirstOrDefault(Func<TypeBlock, bool> predicate)
		{
			return predicate(this) ? this : ChildNodes.FirstOrDefault(predicate);
		}

		/// <summary>
		/// Recursively constructs the final selected type from this branch and its children.
		/// </summary>
		public Type CreateFinalType()
		{
			if (HasUndefinedTypes())
			{
				return null;
			}
			if (_selectedType.IsGenericTypeDefinition)
			{
				var genericArguments = ChildNodes.Select(c => c.CreateFinalType()).ToArray();
				if (genericArguments.Any(arg => arg == null))
				{
					return null;
				}
				var res = _selectedType.MakeGenericType(genericArguments);
				return res;
			}
			return _selectedType;
		}

		private static string GetTypeNameWithNamespace(Type type)
		{
			if (type == null)
				return "None";
			System.Text.StringBuilder sb = new(GetTypeName(type));
			if (!string.IsNullOrEmpty(type.Namespace))
				sb.Append(" (").Append(type.Namespace).Append(")");
			return sb.ToString();
		}

		private static string GetTypeName(Type type)
		{
			if (type == null)
				return "None";
			if (!type.IsGenericType)
				return type.Name;
			var genericTypeDef = type.GetGenericTypeDefinition();
			var genericArgsNames = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
			return $"{genericTypeDef.Name.Split('`')[0]}<{genericArgsNames}>";
		}
	}
}