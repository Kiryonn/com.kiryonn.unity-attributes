using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Debug = UnityEngine.Debug;

namespace Kiryonn.UnityAttributes.Editor
{
	[CustomPropertyDrawer(typeof(SerializeAbstractFieldAttribute))]
	public class SerializeAbstractFieldDrawer : PropertyDrawer
	{
		private SerializedProperty _property;
		private VisualElement _container;
		private VisualElement _propertyField;
		private Type _fieldType;

		/// <summary>
		/// The root block of the type tree.
		/// </summary>
		private TypeBlock _rootNode;

		private static readonly Dictionary<Type, List<Type>> _derivedTypesCache = new();
		/// <summary>
		/// Key = GenericTypeArg, Value = Constraints
		/// </summary>
		private static readonly Dictionary<Type, List<Type>> _genericConstraintsCache = new();
		private static List<Type> _nonAbstractTypes;

		// A pool of nesting colors for the UI.
		private static readonly Color[] NestingColors = new[]
		{
			new Color(0.18f, 0.20f, 0.25f, 1f),
			new Color(0.22f, 0.24f, 0.30f, 1f),
			new Color(0.26f, 0.28f, 0.35f, 1f),
			new Color(0.30f, 0.32f, 0.40f, 1f),
			new Color(0.34f, 0.36f, 0.45f, 1f),
		};

		private static readonly Color BORDER_COLOR = new(0.7f, 0.7f, 0.8f, 1f);

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			_fieldType = fieldInfo.FieldType;
			if (!(_fieldType.IsAbstract || _fieldType.IsInterface))
			{
				return new PropertyField(property);
			}

			_property = property;

			_container = new VisualElement { style = { flexGrow = 1 } };

			var foldout = new Foldout() { text = property.displayName, style = { marginTop = 5 } };
			_container.Add(foldout);

			// Create type related structure
			var derivedTypes = GetDerivedTypes(_fieldType);
			_rootNode = new TypeBlock(null, derivedTypes, "Type", 20);
			foldout.Add(_rootNode);

			// Create value related structure
			_propertyField = new PropertyField { style = { marginTop = 5 } };
			foldout.Add(_propertyField);

			// If property already have a value, initialize dropdowns from current type
			if (_property.managedReferenceValue != null)
			{
				InitializeCurrentType(_rootNode, _property.managedReferenceValue.GetType());
			}

			// Style
			var foldoutContainerStyle = foldout.contentContainer.style;
			ApplyColorStyle(foldoutContainerStyle, 0);
			foldoutContainerStyle.marginLeft = 0;
			foldoutContainerStyle.paddingLeft = 15;

			RegisterEventsRecursively(_rootNode);
			RedrawPropertyField();

			return _container;
		}

		private void RegisterEvent(TypeBlock node)
		{
			node.OnTypeSelected += TypeSelectedHandler;
		}

		private void RegisterEventsRecursively(TypeBlock currentNode)
		{
			currentNode ??= _rootNode;
			RegisterEvent(currentNode);
			foreach (var child in currentNode.ChildNodes)
				RegisterEventsRecursively(child);
		}

		private void InitializeCurrentType(TypeBlock parentNode, Type currentSelectedType)
		{
			if (currentSelectedType == null) return;

			var selectedType = currentSelectedType.IsGenericType ? currentSelectedType.GetGenericTypeDefinition() : currentSelectedType;
			var genericArgs = currentSelectedType.GetGenericArguments();

			parentNode.SelectedType = selectedType;

			if (selectedType.IsGenericTypeDefinition)
			{
				var newBlocks = CreateGenericBranches(parentNode, selectedType);
				for (int i = 0; i < newBlocks.Length; i++)
				{
					InitializeCurrentType(newBlocks[i], genericArgs[i]);
				}
			}
		}

		private static TypeBlock[] CreateGenericBranches(TypeBlock parentBranch, Type genericTypeDef)
		{
			parentBranch.ClearChildren();

			var genericArgs = genericTypeDef.GetGenericArguments();
			var newBlocs = new TypeBlock[genericArgs.Length];
			var bgColorIndex = parentBranch.Depth + 1;
			for (int i = 0; i < genericArgs.Length; i++)
			{
				var arg = genericArgs[i];
				var choices = GetValidGenericArgTypes(arg);
				var bloc = new TypeBlock(parentBranch, choices, arg.Name, 20);
				ApplyColorStyle(bloc.style, bgColorIndex);
				newBlocs[i] = bloc;
			}
			return newBlocs;
		}

		private void RedrawPropertyField()
		{
			_propertyField.RemoveFromHierarchy();
			if (_property.managedReferenceValue != null)
			{
				_propertyField = new PropertyField(_property, "Value");
				_propertyField.Bind(_property.serializedObject);
				_container.hierarchy[0].Add(_propertyField);

				// Use the fabulous tech of "Bash your head until it works"
				// Because at this point, the PropertyField is empty
				// Thanks Unity
				_propertyField.RegisterCallback<GeometryChangedEvent>(RedrawPropertyFoldout);
				void RedrawPropertyFoldout(GeometryChangedEvent _)
				{
					if (_propertyField.childCount == 0) return;

					if (_propertyField.hierarchy[0] is not Foldout foldout)
					{
						_propertyField.UnregisterCallback<GeometryChangedEvent>(RedrawPropertyFoldout);
						return;
					}

					foldout.value = true;
					if (foldout.childCount == 0) return;

					// Can't use foldout[i] because it looks at the #unity-content
					// Foldout [ Toggle [ ... ],  VisualElement #unity-content [ ... ]]
					var content = foldout.hierarchy[1];

					if (content == null) return;

					var contentStyle = content.style;
					contentStyle.Margin(10, 10, 10, 20);
					contentStyle.Padding(5);
					ApplyColorStyle(contentStyle, 1);

					_propertyField.UnregisterCallback<GeometryChangedEvent>(RedrawPropertyFoldout);
				}
			}
		}



		private List<Type> GetDerivedTypes(Type baseType)
		{
			if (_derivedTypesCache.TryGetValue(baseType, out var derivedTypes))
				return derivedTypes;
			derivedTypes = TypeCache.GetTypesDerivedFrom(baseType).ToList();

			_derivedTypesCache[baseType] = derivedTypes;
			return derivedTypes;
		}

		private static List<Type> GetValidGenericArgTypes(Type genericArg)
		{
			if (_genericConstraintsCache.TryGetValue(genericArg, out var validArgTypes))
				return validArgTypes;

			_nonAbstractTypes ??= AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => !t.IsAbstract && !t.IsInterface)
				.ToList();

			var constraints = genericArg.GetGenericParameterConstraints();
			validArgTypes = _nonAbstractTypes
				.Where(t => constraints.All(c => c.IsAssignableFrom(t)))
				.ToList();

			_genericConstraintsCache[genericArg] = validArgTypes;
			return validArgTypes;
		}

		private void TypeSelectedHandler(TypeBlock typeBlock)
		{
			typeBlock.ClearChildren();

			if (typeBlock.SelectedType == null)
			{
				// If the root type is set to null, clear the property value
				if (typeBlock == _rootNode)
				{
					_property.managedReferenceValue = null;
					_property.serializedObject.ApplyModifiedProperties();
					RedrawPropertyField();
				}
				return;
			}


			if (typeBlock.SelectedType.IsGenericTypeDefinition)
			{
				var newBlocks = CreateGenericBranches(typeBlock, typeBlock.SelectedType);
				foreach (var newBlock in newBlocks)
					RegisterEvent(newBlock);
				return;
			}

			// Attempt to create the final type
			var constructedType = _rootNode.CreateFinalType();

			// Some generic args are still undefined
			if (constructedType == null) return;

			// The type defined is already the same
			if (_property.managedReferenceValue.GetType() == constructedType) return;

			// Try to create an instance of the final type
			// This will fail if there is no default constructor
			try
			{
				_property.managedReferenceValue = Activator.CreateInstance(constructedType);
				_property.serializedObject.ApplyModifiedProperties();
				RedrawPropertyField();
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not create an instance of type '{constructedType.FullName}'. Ensure it has a public parameterless constructor.\n{e}");
				return;
			}
		}

		private static void ApplyColorStyle(IStyle style, int i)
		{
			style.backgroundColor = NestingColors[i % NestingColors.Length];
			style.BorderColor(BORDER_COLOR);
			style.BorderWidth(1);
		}
	}
}