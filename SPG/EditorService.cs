﻿/*
 * Copyright © 2011, Denys Vuika
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *  http://www.apache.org/licenses/LICENSE-2.0
 *  
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * */

using System.ComponentModel;
using System.Windows.Controls.PropertyGrid.PropertyEditing;
using System.Windows.Media;
using System.Collections.Generic;

namespace System.Windows.Controls.PropertyGrid
{
  public static class EditorService
  {
    private static readonly Dictionary<Type, Type> _registeredEditors = new Dictionary<Type, Type>();

    public static void RegisterEditor(Type propertyType, Type editorType)
    {
      Requires.NotNull(propertyType, "propertyType");
      Requires.NotNull(editorType, "editorType");

      if (!typeof(EditorBase).IsAssignableFrom(editorType))
        throw new ArgumentException("editorType should inherit EditorBase type");

      _registeredEditors[propertyType] = editorType;
    }

    public static EditorBase GetRegisteredEditor(PropertyLabel label, PropertyItem propertyItem)
    {
      Type editorType = null;
      if (!_registeredEditors.TryGetValue(propertyItem.PropertyType, out editorType)) return null;
      var instance = Activator.CreateInstance(editorType, label, propertyItem);
      return instance as EditorBase;
    }

    public static void RegisterEditor<TPropertyType, TEditorType>() where TEditorType : EditorBase
    {
      _registeredEditors[typeof(TPropertyType)] = typeof(TEditorType);
    }

    public static EditorBase GetEditor(PropertyItem propertyItem, PropertyLabel label)
    {
      if (propertyItem == null) throw new ArgumentNullException("propertyItem");

      EditorAttribute attribute = propertyItem.GetAttribute<EditorAttribute>();
      if (attribute != null)
      {
        Type editorType = Type.GetType(attribute.EditorTypeName, false);
        if (editorType != null)
          return Activator.CreateInstance(editorType, label, propertyItem) as EditorBase;
      }

      var registeredEditor = GetRegisteredEditor(label, propertyItem);
      if (registeredEditor != null) return registeredEditor;

      Type propertyType = propertyItem.PropertyType;

      EditorBase editor = GetEditor(propertyType, label, propertyItem);

      while (editor == null && propertyType.BaseType != null)
      {
        propertyType = propertyType.BaseType;
        editor = GetEditor(propertyType, label, propertyItem);
      }

      return editor;
    }

    public static EditorBase GetEditor(Type propertyType, PropertyLabel label, PropertyItem property)
    {
      if (typeof(Boolean).IsAssignableFrom(propertyType))
        return new BooleanEditor(label, property);

      if (typeof(Enum).IsAssignableFrom(propertyType))
        return new EnumEditor(label, property);

      if (typeof(DateTime).IsAssignableFrom(propertyType))
        return new DateTimeEditor(label, property);

      if (typeof(String).IsAssignableFrom(propertyType))
        return new StringEditor(label, property);

      if (typeof(Brush).IsAssignableFrom(propertyType))
        return new BrushEditor(label, property);

      if (typeof(ValueType).IsAssignableFrom(propertyType))
        return new StringEditor(label, property);
      //if (typeof(Object).IsAssignableFrom(propertyType))
      //    return new PropertyGrid(label, property);

      return new StringEditor(label, property);
    }

  }
}
