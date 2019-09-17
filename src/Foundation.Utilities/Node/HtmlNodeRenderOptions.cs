namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HtmlNodeRenderOptions : INodeRenderOptions
    {
        public string Cross => "&nbsp;├─"; // " ├─";

        public string Corner => "&nbsp;└─"; //" └─";

        public string Vertical => "&nbsp;│&nbsp;"; //" │ ";

        public string Space => "&nbsp;&nbsp;&nbsp;"; //"   ";
    }
}
