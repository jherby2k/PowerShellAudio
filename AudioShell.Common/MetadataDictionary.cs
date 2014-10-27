/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using AudioShell.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AudioShell
{
    /// <summary>
    /// A <see cref="SettingsDictionary"/> of metadata items.
    /// </summary>
    /// <remarks>
    /// This class only accepts a restricted set of keys, and (depending on the key) performs formatting and/or
    /// validation on the values that are set.
    /// </remarks>
    public class MetadataDictionary : SettingsDictionary
    {
        readonly IDictionary<string, Func<string, string>> _acceptedKeys = InitializeAcceptedKeys();

        /// <summary>
        /// Gets the accepted keys.
        /// </summary>
        /// <value>The accepted keys.</value>
        public IReadOnlyCollection<string> AcceptedKeys
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                List<string> result = _acceptedKeys.Keys.ToList();
                result.Sort();
                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found, returns
        /// <see cref="String.Empty"/>.
        /// </returns>
        /// <exception cref="ArgumentException">The specified key is not supported.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="key"/> or <paramref name="value"/> are null or empty.
        /// </exception>
        public override string this[string key]
        {
            get { return base[key]; }
            set
            {
                foreach (var item in _acceptedKeys)
                    if (string.Compare(key, item.Key, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        base[item.Key] = item.Value(value);
                        return;
                    }
                
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unsupported key '{0}'", key));
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_acceptedKeys != null);
        }

        static Dictionary<string, Func<string, string>> InitializeAcceptedKeys()
        {
            Contract.Ensures(Contract.Result<Dictionary<string, Func<string, string>>>() != null);
            Contract.Ensures(Contract.Result<Dictionary<string, Func<string, string>>>().Count > 0);

            var result = new Dictionary<string, Func<string, string>>(12);

            Func<string, string> validateDefault = new Func<string, string>(value => value);
            Func<string, string> validateGain = new Func<string, string>(value => string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", Convert.ToSingle(value.Replace(" dB", string.Empty), CultureInfo.InvariantCulture)));
            Func<string, string> validatePeak = new Func<string, string>(value => string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", Convert.ToSingle(value, CultureInfo.InvariantCulture)));
            Func<string, string> validateTrackNumber = new Func<string, string>(value => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString("00", CultureInfo.InvariantCulture));
            Func<string, string> validateYear = new Func<string, string>(value =>
            {
                // Accept any years from 1000 through 2999:
                if (Regex.IsMatch(value, "^[12][0-9]{3}$"))
                    return value;
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataDictionaryYearError, value));
            });

            result.Add("Album", validateDefault);
            result.Add("AlbumGain", validateGain);
            result.Add("AlbumPeak", validatePeak);
            result.Add("Artist", validateDefault);
            result.Add("Comment", validateDefault);
            result.Add("Genre", validateDefault);
            result.Add("Title", validateDefault);
            result.Add("TrackCount", validateTrackNumber);
            result.Add("TrackGain", validateGain);
            result.Add("TrackNumber", validateTrackNumber);
            result.Add("TrackPeak", validatePeak);
            result.Add("Year", validateYear);

            return result;
        }
    }
}
