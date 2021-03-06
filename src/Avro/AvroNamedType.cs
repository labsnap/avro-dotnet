﻿using System;

namespace Avro
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class AvroNamedType : Attribute
    {
        public AvroNamedType(string ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public string FullName { get => $"{Namespace}{(string.IsNullOrEmpty(Namespace) ? "" : ".")}{Name}"; }
        public string Namespace { get; private set; }
        public string Name { get; private set; }
    }
}
