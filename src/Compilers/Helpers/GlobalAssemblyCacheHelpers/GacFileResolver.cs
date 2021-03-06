﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting
{
    /// <summary>
    /// Extends MetadataFileReferenceResolver to enable resolution of assembly
    /// simple names in the GAC.
    /// </summary>
    internal sealed class GacFileResolver : IEquatable<GacFileResolver>
    {
        /// <summary>
        /// Architecture filter used when resolving assembly references.
        /// </summary>
        public ImmutableArray<ProcessorArchitecture> Architectures { get; }

        /// <summary>
        /// CultureInfo used when resolving assembly references.
        /// </summary>
        public CultureInfo PreferredCulture { get; }

        /// <summary>
        /// A resolver that is configured to resolve against the GAC associated
        /// with the bitness of the currently executing process.
        /// </summary>
        internal static GacFileResolver Default = new GacFileResolver(
            architectures: GlobalAssemblyCache.CurrentArchitectures,
            preferredCulture: null);

        /// <summary>
        /// Constructs an instance of a <see cref="GacFileResolver"/>
        /// </summary>
        /// <param name="architectures">Supported architectures used to filter GAC assemblies.</param>
        /// <param name="preferredCulture">A culture to use when choosing the best assembly from 
        /// among the set filtered by <paramref name="architectures"/></param>
        public GacFileResolver(ImmutableArray<ProcessorArchitecture> architectures, CultureInfo preferredCulture)
        {
            Architectures = architectures;
            PreferredCulture = preferredCulture;
        }

        public string Resolve(string assemblyName)
        {
            string path;
            GlobalAssemblyCache.ResolvePartialName(assemblyName, out path, Architectures, this.PreferredCulture);
            return File.Exists(path) ? path : null;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(PreferredCulture, Hash.CombineValues(Architectures));
        }

        public bool Equals(GacFileResolver other)
        {
            return ReferenceEquals(this, other) ||
                other != null &&
                Architectures.SequenceEqual(other.Architectures) &&
                PreferredCulture == other.PreferredCulture;
        }

        public override bool Equals(object obj) => Equals(obj as GacFileResolver);
    }
}
