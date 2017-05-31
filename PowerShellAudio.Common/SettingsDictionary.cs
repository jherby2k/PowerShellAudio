/*
 * Copyright © 2014-2017 Jeremy Herbison
 * 
 * This file is part of PowerShell Audio.
 * 
 * PowerShell Audio is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * PowerShell Audio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License
 * for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with PowerShell Audio.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using PowerShellAudio.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// A dictionary of settings.
    /// </summary>
    /// <remarks>
    /// Internally, this class wraps a <see cref="Dictionary{TKey,TValue}"/>. The Item property behaves differently,
    /// however, allowing for easier additions or modifications without fear of throwing an exception. The keys are
    /// also case-insensitive.
    /// </remarks>
    [Serializable]
    public class SettingsDictionary : IDictionary<string, string>
    {
        readonly IDictionary<string, string> _internalDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds an element with the specified key and value into the
        /// <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="key"/> or <paramref name="value"/> is null or empty.
        /// </exception>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public void Add([NotNull] string key, [NotNull] string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException(Resources.SettingsDictionaryKeyIsEmptyError, nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(Resources.SettingsDictionaryValueIsEmptyError, nameof(value));

            this[key] = value;
        }

        /// <summary>
        /// Determines whether the <see cref="SettingsDictionary"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="SettingsDictionary"/>.</param>
        /// <returns>
        /// true if the <see cref="SettingsDictionary"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool ContainsKey([NotNull] string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException(Resources.SettingsDictionaryKeyIsEmptyError, nameof(key));

            return _internalDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="ICollection{T}"/> containing the keys in the <see cref="SettingsDictionary"/>.
        /// </returns>
        [ItemNotNull, CollectionAccess(CollectionAccessType.Read)]
        public ICollection<string> Keys => _internalDictionary.Keys;

        /// <summary>
        /// Removes the element with the specified key from the <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if
        /// <paramref name="key"/> was not found in the <see cref="SettingsDictionary"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public bool Remove([NotNull] string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException(Resources.SettingsDictionaryKeyIsEmptyError, nameof(key));

            return _internalDictionary.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found; otherwise,
        /// <see cref="String.Empty"/>.
        /// </param>
        /// <returns>
        /// true if the <see cref="SettingsDictionary"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool TryGetValue(string key, [NotNull] out string value)
        {
            return _internalDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="ICollection{T}"/> containing the values in the <see cref="SettingsDictionary"/>.
        /// </returns>
        [ItemNotNull, CollectionAccess(CollectionAccessType.Read)]
        public ICollection<string> Values => _internalDictionary.Values;

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The value associated with the specified key. If the key is not found, returns
        /// <see cref="String.Empty"/>. Setting a null or empty value will clear the element.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="key"/> is null or empty.
        /// </exception>
        [NotNull, CollectionAccess(CollectionAccessType.UpdatedContent)]
        public virtual string this[[NotNull] string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException(Resources.SettingsDictionaryKeyIsEmptyError, nameof(key));

                return _internalDictionary.TryGetValue(key, out string result) ? result : string.Empty;
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException(Resources.SettingsDictionaryKeyIsEmptyError, nameof(key));

                if (_internalDictionary.ContainsKey(key))
                {
                    if (!string.IsNullOrEmpty(value))
                        _internalDictionary[key] = value;
                    else
                        _internalDictionary.Remove(key);
                }
                else if (!string.IsNullOrEmpty(value))
                    _internalDictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <param name="item">
        /// The <see cref="KeyValuePair{TKey, TValue}"/> to add to the <see cref="SettingsDictionary"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either of <paramref name="item"/>'s Key or Value properties are null or empty.
        /// </exception>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public void Add(KeyValuePair<string, string> item)
        {
            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
                throw new ArgumentException(Resources.SettingsDictionaryValueIsEmptyError, nameof(item));

            _internalDictionary.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="SettingsDictionary"/>.
        /// </summary>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public virtual void Clear()
        {
            _internalDictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="SettingsDictionary"/> contains a specific value.
        /// </summary>
        /// <param name="item">
        /// The <see cref="KeyValuePair{TKey, TValue}"/> to locate in the <see cref="SettingsDictionary"/>.
        /// </param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="SettingsDictionary"/>; otherwise, false.
        /// </returns>
        [CollectionAccess(CollectionAccessType.Read)]

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _internalDictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="MetadataDictionary"/> to the specified array of
        /// <see cref="KeyValuePair{TKey, TValue}"/> structures, starting at the specified index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array of <see cref="KeyValuePair{TKey, TValue}"/> structures that is the destination of
        /// the elements copied from the current <see cref="MetadataDictionary"/>.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="MetadataDictionary"/> is greater than the available space
        /// from index to the end of the destination array.
        /// </exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _internalDictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the elements of the <see cref="SettingsDictionary"/> to another <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsDictionary"/> instance being copied to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public void CopyTo([NotNull] SettingsDictionary settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            foreach (var item in _internalDictionary)
                settings[item.Key] = item.Value;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="SettingsDictionary"/>.</returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public int Count => _internalDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="SettingsDictionary"/> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="SettingsDictionary"/> is read-only; otherwise, false.</returns>
        [CollectionAccess(CollectionAccessType.None)]
        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific <see cref="KeyValuePair{TKey, TValue}"/> from the
        /// <see cref="SettingsDictionary"/>.
        /// </summary>
        /// <param name="item">
        /// The <see cref="KeyValuePair{TKey, TValue}"/> to remove from the <see cref="SettingsDictionary"/>.
        /// </param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="SettingsDictionary"/>;
        /// otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original
        /// <see cref="SettingsDictionary"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either of <paramref name="item"/>'s Key or Value properties are null or empty.
        /// </exception>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public bool Remove(KeyValuePair<string, string> item)
        {
            return _internalDictionary.Remove(item.Key);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _internalDictionary.GetEnumerator();
        }

        [CollectionAccess(CollectionAccessType.Read)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalDictionary.GetEnumerator();
        }
    }
}
