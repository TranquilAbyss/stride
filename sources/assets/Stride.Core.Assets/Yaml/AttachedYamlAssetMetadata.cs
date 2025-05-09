// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Core.Assets.Yaml;

/// <summary>
/// An interface representing an object that has <see cref="IYamlAssetMetadata"/> attached to it.
/// </summary>
public class AttachedYamlAssetMetadata
{
    private readonly Dictionary<PropertyKey, IYamlAssetMetadata> yamlMetadata = [];

    /// <summary>
    /// Attaches metadata to this object.
    /// </summary>
    /// <typeparam name="T">The type of metadata being attached.</typeparam>
    /// <param name="key">The property key that identifies this type of metadata.</param>
    /// <param name="metadata">The metadata to attach.</param>
    public void AttachMetadata<T>(PropertyKey<YamlAssetMetadata<T>> key, YamlAssetMetadata<T> metadata)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(metadata);
        yamlMetadata[key] = metadata;
    }

    /// <summary>
    /// Retrieves metadata attached to this object.
    /// </summary>
    /// <typeparam name="T">The type of metadata being attached.</typeparam>
    /// <param name="key">The property key that identifies this type of metadata.</param>
    /// <returns>The corresponding metadata attached to this object, or null if it couldn't be found.</returns>
    public YamlAssetMetadata<T>? RetrieveMetadata<T>(PropertyKey<YamlAssetMetadata<T>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        yamlMetadata.TryGetValue(key, out var metadata);
        return (YamlAssetMetadata<T>?)metadata;
    }

    public void CopyInto(AttachedYamlAssetMetadata target)
    {
        foreach (var metadata in yamlMetadata)
        {
            target.yamlMetadata.Add(metadata.Key, metadata.Value);
        }
    }

    internal PropertyContainer ToPropertyContainer()
    {
        var container = new PropertyContainer();
        foreach (var metadata in yamlMetadata)
        {
            container.SetObject(metadata.Key, metadata.Value);
        }
        return container;
    }

    internal static AttachedYamlAssetMetadata FromPropertyContainer(PropertyContainer container)
    {
        var result = new AttachedYamlAssetMetadata();
        foreach (var property in container)
        {
            result.yamlMetadata.Add(property.Key, (IYamlAssetMetadata)property.Value);
        }
        return result;
    }
}
