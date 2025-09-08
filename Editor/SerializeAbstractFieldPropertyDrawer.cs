using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace Kiryonn.UnityAttributes.Editor
{
	[CustomPropertyDrawer(typeof(SerializeAbstractFieldAttribute))]
    public class SerializeAbstractFieldDrawer : PropertyDrawer
    {
    	// Caches derived types for abstract fields.
    	private static readonly Dictionary<Type, List<Type>> derivedTypesCache = new();
    
    	// Caches valid types for generic parameters.
    	private static readonly Dictionary<Type, List<Type>> genericArgumentTypesCache = new();
    
    	public override VisualElement CreatePropertyGUI(SerializedProperty property)
    	{
    		var root = new VisualElement();
    		var fieldType = fieldInfo.FieldType;
    
    		var container = new VisualElement { style = { flexGrow = 1 } };
    		container.Add(new Label(property.displayName));
    		root.Add(container);
    
    		var typeDropdown = new DropdownField("Type", new List<string>(), 0);
    		typeDropdown.style.flexGrow = 1;
    		typeDropdown.style.marginLeft = 15;
    		container.Add(typeDropdown);
    
    		var objectContainer = new VisualElement();
    		objectContainer.style.marginLeft = 20;
    		container.Add(objectContainer);
    
    		Action drawProperties = () =>
    		{
    			objectContainer.Clear();
    			if (property.managedReferenceValue != null)
    			{
    				var objectProperty = property.serializedObject.FindProperty(property.propertyPath);
    				objectContainer.Add(new PropertyField(objectProperty));
    			}
    		};
    
    		PopulateDropdown(typeDropdown, fieldType, property);
    
    		typeDropdown.RegisterValueChangedCallback(evt =>
    		{
    			var selectedTypeName = evt.newValue;
    			if (string.IsNullOrEmpty(selectedTypeName) || selectedTypeName == "None")
    			{
    				property.managedReferenceValue = null;
    			}
    			else
    			{
    				var allDerivedTypes = GetDerivedTypes(fieldType);
    				var selectedType = allDerivedTypes.FirstOrDefault(t => t.FullName == selectedTypeName);
    
    				if (selectedType != null)
    				{
    					if (selectedType.IsGenericTypeDefinition)
    					{
    						HandleGenericTypeSelection(selectedType, container, property, drawProperties);
    					}
    					else
    					{
    						property.managedReferenceValue = Activator.CreateInstance(selectedType);
    					}
    				}
    			}
    
    			property.serializedObject.ApplyModifiedProperties();
    			drawProperties();
    		});
    
    		if (property.managedReferenceValue != null)
    		{
    			var currentType = property.managedReferenceValue.GetType();
    			typeDropdown.value = currentType.FullName;
    		}
    
    		drawProperties();
    		return root;
    	}
    
    	private void PopulateDropdown(DropdownField dropdown, Type baseType, SerializedProperty property)
    	{
    		var derivedTypes = GetDerivedTypes(baseType);
    		var typeNames = derivedTypes.Select(t => t.FullName).ToList();
    
    		typeNames.Insert(0, "None");
    		dropdown.choices = typeNames;
    
    		if (property.managedReferenceValue != null)
    		{
    			var currentType = property.managedReferenceValue.GetType();
    			var index = typeNames.IndexOf(currentType.FullName);
    			if (index != -1)
    			{
    				dropdown.index = index;
    			}
    		}
    		else
    		{
    			dropdown.index = 0;
    		}
    	}
    
    	private List<Type> GetDerivedTypes(Type baseType)
    	{
    		if (derivedTypesCache.TryGetValue(baseType, out var cachedTypes))
    		{
    			return cachedTypes;
    		}
    
    		var types = AppDomain.CurrentDomain.GetAssemblies()
    			.SelectMany(assembly => assembly.GetTypes())
    			.Where(type =>
    				(baseType.IsGenericTypeDefinition
    					? type.IsSubclassOfRawGeneric(baseType)
    					: baseType.IsAssignableFrom(type)) &&
    				!type.IsAbstract &&
    				!type.IsInterface)
    			.ToList();
    
    		derivedTypesCache[baseType] = types;
    		return types;
    	}
    
    	private void HandleGenericTypeSelection(Type genericTypeDefinition, VisualElement parentContainer,
    		SerializedProperty property, Action drawProperties)
    	{
    		if (!genericArgumentTypesCache.TryGetValue(genericTypeDefinition, out var validTypes))
    		{
    			var genericArgumentType = genericTypeDefinition.GetGenericArguments()[0];
    			var constraints = genericArgumentType.GetGenericParameterConstraints();
    
    			validTypes = AppDomain.CurrentDomain.GetAssemblies()
    				.SelectMany(assembly => assembly.GetTypes())
    				.Where(type =>
    					!type.IsAbstract &&
    					!type.IsInterface &&
    					(constraints.Length == 0 || constraints.All(c => c.IsAssignableFrom(type))))
    				.ToList();
    
    			genericArgumentTypesCache[genericTypeDefinition] = validTypes;
    		}
    
    		// Clear existing generic UI elements.
    		var existingDropdowns = parentContainer.Query<VisualElement>("", "generic-ui").ToList();
    		foreach (var element in existingDropdowns)
    		{
    			element.RemoveFromHierarchy();
    		}
    
    		// Container for the generic type selection UI.
    		var genericContainer = new VisualElement();
    		genericContainer.AddToClassList("generic-ui");
    		genericContainer.style.marginLeft = 30;
    
    		// Search bar.
    		var searchBar = new TextField("Search")
    		{
    			style =
    			{
    				marginBottom = 5
    			}
    		};
    		genericContainer.Add(searchBar);
    
    		// Dropdown for the filtered types.
    		var genericDropdown = new DropdownField("Generic Type", new List<string>(), 0)
    		{
    			style =
    			{
    				flexGrow = 1
    			}
    		};
    		genericContainer.Add(genericDropdown);
    
    		// Function to update the dropdown choices based on the search query.
    		void UpdateDropdown(string query)
    		{
    			var filteredTypes = validTypes.Where(t => t.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
    				.Take(10) // Limit to the first 10 results.
    				.ToList();
    
    			genericDropdown.choices = filteredTypes.Select(t => t.Name).ToList();
    		}
    
    		// Initial population of the dropdown.
    		UpdateDropdown(string.Empty);
    
    		// Register the search bar's change event.
    		searchBar.RegisterValueChangedCallback(evt => { UpdateDropdown(evt.newValue); });
    
    		genericDropdown.RegisterValueChangedCallback(evt =>
    		{
    			var selectedArgumentTypeName = evt.newValue;
    			var selectedArgumentType = validTypes.FirstOrDefault(t => t.Name == selectedArgumentTypeName);
    
    			if (selectedArgumentType != null)
    			{
    				var constructedType = genericTypeDefinition.MakeGenericType(selectedArgumentType);
    				property.managedReferenceValue = Activator.CreateInstance(constructedType);
    				property.serializedObject.ApplyModifiedProperties();
    			}
    
    			drawProperties();
    		});
    
    		parentContainer.Add(genericContainer);
    	}
    }
    
	public static class TypeExtensions
	{
		public static bool IsSubclassOfRawGeneric(this Type type, Type genericTypeDefinition)
		{
			while (type != null && type != typeof(object))
			{
				var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
				if (genericTypeDefinition == current)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}
	}
}