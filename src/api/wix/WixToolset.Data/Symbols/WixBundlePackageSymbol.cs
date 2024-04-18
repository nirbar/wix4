// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Data
{
    using WixToolset.Data.Symbols;

    public static partial class SymbolDefinitions
    {
        public static readonly IntermediateSymbolDefinition WixBundlePackage = new IntermediateSymbolDefinition(
            SymbolDefinitionType.WixBundlePackage,
            new[]
            {
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Type), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.PayloadRef), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Attributes), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.InstallCondition), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.RepairCondition), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Cache), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.CacheId), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.PerMachine), IntermediateFieldType.Bool),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.LogPathVariable), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.RollbackLogPathVariable), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Size), IntermediateFieldType.LargeNumber),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.InstallSize), IntermediateFieldType.LargeNumber),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Version), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Language), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.DisplayName), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.Description), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.RollbackBoundaryRef), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.RollbackBoundaryBackwardRef), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixBundlePackageSymbolFields.MsiTransactionRef), IntermediateFieldType.String),
            },
            typeof(WixBundlePackageSymbol));
    }
}

namespace WixToolset.Data.Symbols
{
    using System;

    public enum WixBundlePackageSymbolFields
    {
        Type,
        PayloadRef,
        Attributes,
        InstallCondition,
        RepairCondition,
        Cache,
        CacheId,
        PerMachine,
        LogPathVariable,
        RollbackLogPathVariable,
        Size,
        InstallSize,
        Version,
        Language,
        DisplayName,
        Description,
        RollbackBoundaryRef,
        RollbackBoundaryBackwardRef,
        MsiTransactionRef,
    }

    /// <summary>
    /// Types of bundle packages.
    /// </summary>
    public enum WixBundlePackageType
    {
        NotSet = -1,
        Bundle,
        Exe,
        Msi,
        Msp,
        Msu,
    }

    [Flags]
    public enum WixBundlePackageAttributes
    {
        None = 0x0,
        Permanent = 0x1,
        Visible = 0x2,
        Win64 = 0x4,
        Vital = 0x8,
    }

    public class WixBundlePackageSymbol : IntermediateSymbol
    {
        public WixBundlePackageSymbol() : base(SymbolDefinitions.WixBundlePackage, null, null)
        {
        }

        public WixBundlePackageSymbol(SourceLineNumber sourceLineNumber, Identifier id = null) : base(SymbolDefinitions.WixBundlePackage, sourceLineNumber, id)
        {
        }

        public IntermediateField this[WixBundlePackageSymbolFields index] => this.Fields[(int)index];

        public WixBundlePackageType Type
        {
            get
            {
                if (Enum.TryParse((string)this.Fields[(int)WixBundlePackageSymbolFields.Type], true, out WixBundlePackageType value))
                {
                    return value;
                }

                return WixBundlePackageType.NotSet;
            }
            set => this.Set((int)WixBundlePackageSymbolFields.Type, value.ToString());
        }

        public string PayloadRef
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.PayloadRef];
            set => this.Set((int)WixBundlePackageSymbolFields.PayloadRef, value);
        }

        public WixBundlePackageAttributes Attributes
        {
            get => (WixBundlePackageAttributes)(int)this.Fields[(int)WixBundlePackageSymbolFields.Attributes];
            set => this.Set((int)WixBundlePackageSymbolFields.Attributes, (int)value);
        }

        public string InstallCondition
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.InstallCondition];
            set => this.Set((int)WixBundlePackageSymbolFields.InstallCondition, value);
        }

        public string RepairCondition
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.RepairCondition];
            set => this.Set((int)WixBundlePackageSymbolFields.RepairCondition, value);
        }

        public BundleCacheType? Cache
        {
            get => Enum.TryParse((string)this.Fields[(int)WixBundlePackageSymbolFields.Cache], true, out BundleCacheType value) ? value : (BundleCacheType?)null;
            set => this.Set((int)WixBundlePackageSymbolFields.Cache, value.ToString().ToLowerInvariant());
        }

        public string CacheId
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.CacheId];
            set => this.Set((int)WixBundlePackageSymbolFields.CacheId, value);
        }

        public bool? PerMachine
        {
            get => (bool?)this.Fields[(int)WixBundlePackageSymbolFields.PerMachine];
            set => this.Set((int)WixBundlePackageSymbolFields.PerMachine, value);
        }

        public string LogPathVariable
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.LogPathVariable];
            set => this.Set((int)WixBundlePackageSymbolFields.LogPathVariable, value);
        }

        public string RollbackLogPathVariable
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.RollbackLogPathVariable];
            set => this.Set((int)WixBundlePackageSymbolFields.RollbackLogPathVariable, value);
        }

        public long Size
        {
            get => (long)this.Fields[(int)WixBundlePackageSymbolFields.Size];
            set => this.Set((int)WixBundlePackageSymbolFields.Size, value);
        }

        public long? InstallSize
        {
            get => (long?)this.Fields[(int)WixBundlePackageSymbolFields.InstallSize];
            set => this.Set((int)WixBundlePackageSymbolFields.InstallSize, value);
        }

        public string Version
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.Version];
            set => this.Set((int)WixBundlePackageSymbolFields.Version, value);
        }

        public int? Language
        {
            get => (int?)this.Fields[(int)WixBundlePackageSymbolFields.Language];
            set => this.Set((int)WixBundlePackageSymbolFields.Language, value);
        }

        public string DisplayName
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.DisplayName];
            set => this.Set((int)WixBundlePackageSymbolFields.DisplayName, value);
        }

        public string Description
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.Description];
            set => this.Set((int)WixBundlePackageSymbolFields.Description, value);
        }

        public string RollbackBoundaryRef
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.RollbackBoundaryRef];
            set => this.Set((int)WixBundlePackageSymbolFields.RollbackBoundaryRef, value);
        }

        public string RollbackBoundaryBackwardRef
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.RollbackBoundaryBackwardRef];
            set => this.Set((int)WixBundlePackageSymbolFields.RollbackBoundaryBackwardRef, value);
        }

        public string MsiTransactionRef
        {
            get => (string)this.Fields[(int)WixBundlePackageSymbolFields.MsiTransactionRef];
            set => this.Set((int)WixBundlePackageSymbolFields.MsiTransactionRef, value);
        }

        public bool Permanent
        {
            get { return this.Attributes.HasFlag(WixBundlePackageAttributes.Permanent); }
            set
            {
                if (value)
                {
                    this.Attributes |= WixBundlePackageAttributes.Permanent;
                }
                else
                {
                    this.Attributes &= ~WixBundlePackageAttributes.Permanent;
                }
            }
        }

        public bool Visible
        {
            get { return this.Attributes.HasFlag(WixBundlePackageAttributes.Visible); }
            set
            {
                if (value)
                {
                    this.Attributes |= WixBundlePackageAttributes.Visible;
                }
                else
                {
                    this.Attributes &= ~WixBundlePackageAttributes.Visible;
                }
            }
        }

        public bool Win64
        {
            get { return this.Attributes.HasFlag(WixBundlePackageAttributes.Win64); }
            set
            {
                if (value)
                {
                    this.Attributes |= WixBundlePackageAttributes.Win64;
                }
                else
                {
                    this.Attributes &= ~WixBundlePackageAttributes.Win64;
                }
            }
        }

        public bool Vital
        {
            get { return this.Attributes.HasFlag(WixBundlePackageAttributes.Vital); }
            set
            {
                if (value)
                {
                    this.Attributes |= WixBundlePackageAttributes.Vital;
                }
                else
                {
                    this.Attributes &= ~WixBundlePackageAttributes.Vital;
                }
            }
        }
    }
}
