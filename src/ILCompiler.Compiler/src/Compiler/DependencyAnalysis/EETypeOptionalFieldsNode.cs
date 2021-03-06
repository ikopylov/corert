// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Runtime;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis
{
    internal class EETypeOptionalFieldsNode : ObjectNode, ISymbolNode
    {
        private EETypeNode _owner;

        public EETypeOptionalFieldsNode(EETypeNode owner)
        {
            _owner = owner;
        }

        public override ObjectNodeSection Section
        {
            get
            {
                if (_owner.Type.Context.Target.IsWindows)
                    return ObjectNodeSection.ReadOnlyDataSection;
                else
                    return ObjectNodeSection.DataSection;
            }
        }

        public override bool ShouldShareNodeAcrossModules(NodeFactory factory)
        {
            return true;
        }

        public override bool StaticDependenciesAreComputed
        {
            get
            {
                return true;
            }
        }

        int ISymbolNode.Offset
        {
            get
            {
                return 0;
            }
        }

        string ISymbolNode.MangledName
        {
            get
            {
                return "__optionalfields_" + ((ISymbolNode)_owner).MangledName;
            }
        }

        protected override string GetName()
        {
            return ((ISymbolNode)this).MangledName;
        }

        public override bool ShouldSkipEmittingObjectNode(NodeFactory factory)
        {
            return _owner.ShouldSkipEmittingObjectNode(factory) || !_owner.HasOptionalFields;
        }

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            ObjectDataBuilder objData = new ObjectDataBuilder(factory);
            objData.RequirePointerAlignment();
            objData.DefinedSymbols.Add(this);

            if (!relocsOnly)
            {
                objData.EmitBytes(_owner.GetOptionalFieldsData());
            }
            
            return objData.ToObjectData();
        }
    }
}
