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

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    class MetadataSubstituter
    {
        static readonly char[] _invalidChars = Path.GetInvalidFileNameChars();
        static readonly Regex _replacer = new Regex(@"\{[^{]+\}");

        readonly MetadataDictionary _metadata;

        internal MetadataSubstituter([NotNull] MetadataDictionary metadata)
        {
            _metadata = metadata;
        }

        [ContractAnnotation("null => null")]
        internal string Substitute([CanBeNull] string path)
        {
            return path != null
                ? _replacer.Replace(path, match =>
                    new string(_metadata[match.Value.Substring(1, match.Value.Length - 2)].Where(character =>
                        !_invalidChars.Contains(character)).ToArray()))
                : null;
        }
    }
}
