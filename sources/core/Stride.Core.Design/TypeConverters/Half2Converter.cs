// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Stride.Core.Mathematics;
using Half = Stride.Core.Mathematics.Half;

namespace Stride.Core.TypeConverters;

/// <summary>
///   Provides a type converter to convert <see cref = "T:SharpDX.Half2" /> objects to and from various
///   other representations.
/// </summary>
public class Half2Converter : ExpandableObjectConverter
{
    private readonly PropertyDescriptorCollection properties;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Half2Converter"/> class.
    /// </summary>
    public Half2Converter()
    {
        Type type = typeof(Half2);
        PropertyDescriptor[] propArray =
        [
            new FieldPropertyDescriptor(type.GetField("X")!),
            new FieldPropertyDescriptor(type.GetField("Y")!),
        ];
        properties = new PropertyDescriptorCollection(propArray);
    }

    /// <summary>
    ///   Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "sourceType">A System::Type that represents the type you want to convert from.</param>
    /// <returns>
    ///   <c>true</c> if this converter can perform the conversion; otherwise, <c>false</c>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    ///   Returns whether this converter can convert the object to the specified type, using the specified context.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "destinationType">A <see cref = "T:System.Type" /> that represents the type you want to convert to.</param>
    /// <returns>
    ///   <c>true</c> if this converter can perform the conversion; otherwise, <c>false</c>.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if ((destinationType != typeof(string)) && (destinationType != typeof(InstanceDescriptor)))
        {
            return base.CanConvertTo(context, destinationType);
        }
        return true;
    }

    //TODO: Verify that converter is properly updated to match other converters
    /// <summary>
    ///   Converts the given object to the type of this converter, using the specified context and culture information.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "culture">A <see cref = "T:System.Globalization.CultureInfo" />. If <c>null</c> is passed, the current culture is assumed.</param>
    /// <param name = "value">The <see cref = "T:System.Object" /> to convert.</param>
    /// <returns>An <see cref = "T:System.Object" /> that represents the converted value.</returns>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        culture ??= CultureInfo.CurrentCulture;
        if (value is not string str)
        {
            return base.ConvertFrom(context, culture, value);
        }
        str = str.Trim();
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Half));
        char[] separator = [culture.TextInfo.ListSeparator[0]];
        string[] stringArray = str.Split(separator);
        if (stringArray.Length != 2)
        {
            throw new ArgumentException("Invalid half format.");
        }
        Half x = (Half)converter.ConvertFromString(context, culture, stringArray[0])!;
        Half y = (Half)converter.ConvertFromString(context, culture, stringArray[1])!;
        return new Half2(x, y);
    }

    /// <summary>
    ///   Converts the given value object to the specified type, using the specified context and culture information.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "culture">A <see cref = "T:System.Globalization.CultureInfo" />. If <c>null</c> is passed, the current culture is assumed.</param>
    /// <param name = "value">The <see cref = "T:System.Object" /> to convert.</param>
    /// <param name = "destinationType">A <see cref = "T:System.Type" /> that represents the type you want to convert to.</param>
    /// <returns>An <see cref = "T:System.Object" /> that represents the converted value.</returns>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(destinationType);
#else
        if (destinationType is null) throw new ArgumentNullException(nameof(destinationType));
#endif
        culture ??= CultureInfo.CurrentCulture;

        if ((destinationType == typeof(string)) && (value is Half2 half))
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Half));
            return string.Join(culture.TextInfo.ListSeparator + " ",
            [
                converter.ConvertToString(context, culture, half.X),
                converter.ConvertToString(context, culture, half.Y),
            ]);
        }
        if ((destinationType == typeof(InstanceDescriptor)) && (value is Half2 half1))
        {
            var constructor = typeof(Half2).GetConstructor([typeof(Half), typeof(Half)]);
            if (constructor != null)
            {
                return new InstanceDescriptor(constructor, new object[] { half1.X, half1.Y });
            }
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <summary>
    ///   Creates an instance of the type that this <see cref = "T:System.ComponentModel.TypeConverter" /> is associated with, using the specified context, given a set of property values for the object.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "propertyValues">An <see cref = "T:System.Collections.IDictionary" /> of new property values.</param>
    /// <returns>An <see cref = "T:System.Object" /> representing the given <see cref = "T:System.Collections.IDictionary" />, or <c>null</c> if the object cannot be created.</returns>
    public override object CreateInstance(ITypeDescriptorContext? context, IDictionary propertyValues)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(propertyValues);
#else
        if (propertyValues is null) throw new ArgumentNullException(nameof(propertyValues));
#endif
        return new Half2((Half)propertyValues["X"]!, (Half)propertyValues["Y"]!);
    }

    /// <summary>
    ///   Returns whether changing a value on this object requires a call to <c>System::ComponentModel::TypeConverter::CreateInstance(System::Collections::IDictionary^)</c>
    ///   to create a new value, using the specified context.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <returns>
    ///   <c>false</c> if changing a property on this object requires a call to <c>System::ComponentModel::TypeConverter::CreateInstance(System::Collections::IDictionary^)</c> to create a new value; otherwise, <c>false</c>.</returns>
    public override bool GetCreateInstanceSupported(ITypeDescriptorContext? context)
    {
        return true;
    }

    /// <summary>
    ///   Creates an instance of the type that this <see cref = "T:System.ComponentModel.TypeConverter" /> is associated with, using the specified context, given a set of property values for the object.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name = "value">An <see cref = "T:System.Object" /> that specifies the type of array for which to get properties. </param>
    /// <param name = "attributes">An array of type <see cref = "T:System.Attribute" /> that is used as a filter.</param>
    /// <returns>A <see cref = "T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that are exposed for this data type, or a null reference (<c>Nothing</c> in Visual Basic) if there are no properties.</returns>
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
    {
        return properties;
    }

    /// <summary>
    ///   Returns whether this object supports properties, using the specified context.
    /// </summary>
    /// <param name = "context">A <see cref = "T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <returns>
    ///   <c>true</c> if GetProperties should be called to find the properties of this object; otherwise, <c>false</c>.</returns>
    public override bool GetPropertiesSupported(ITypeDescriptorContext? context)
    {
        return true;
    }
}
