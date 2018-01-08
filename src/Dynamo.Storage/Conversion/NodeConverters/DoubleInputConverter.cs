﻿using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Storage.Conversion.ExtraInfo;

namespace Dynamo.Storage.Conversion.NodeConverters
{
    class DoubleInputConverter: BaseConverter
    {
        public override bool ConvertsType(NodeModel type)
        {
            return type is DoubleInput;
        }

        internal override void SetExtra(NodeModel nodeModel, NodeToPublish node)
        {
            var extra = new DoubleInputExtra(nodeModel);
            node.Extra = extra;
        }
    }
}
