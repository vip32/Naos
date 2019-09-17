namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultNodeRenderOptions : INodeRenderOptions
    {
        public string Cross => " ├─"; // " ├─";

        public string Corner => " └─"; //" └─";

        public string Vertical => " │ "; //" │ ";

        public string Space => "   "; //"   ";
    }
}
