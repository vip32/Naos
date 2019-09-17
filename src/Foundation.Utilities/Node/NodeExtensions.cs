namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class NodeExtensions
    {
        // NAME1
        //  ├─NAM1a
        //  ├─NAM2a
        //  │ ├─NAM2ab
        //  │ └─NAM2bb
        //  ├─NAM3a
        //  └─NAM4a
        private const string Cross = "&nbsp;├─"; // " ├─";
        private const string Corner = "&nbsp;└─"; //" └─";
        private const string Vertical = "&nbsp;│&nbsp;"; //" │ ";
        private const string Space = "&nbsp;&nbsp;&nbsp;"; //"   ";

        public static void Render<T>(this IEnumerable<Node<T>> source, Action<T> preAction, Action<T> nameAction, Action<string> indentAction)
        {
            foreach(var node in source.Safe())
            {
                preAction(node.Value); // PRE
                PrintNode(node, indent: string.Empty, preAction, nameAction, indentAction); // ROOT
            }
        }

        private static void PrintNode<T>(Node<T> node, string indent, Action<T> preAction, Action<T> nameAction, Action<string> indentAction)
        {
            //Console.WriteLine(node.Value.ToString()); // NAME
            nameAction(node.Value);

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var numberOfChildren = node.Children.Count();
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children.ToList()[i]; // ? optimize
                var isLast = i == (numberOfChildren - 1);

                preAction(node.Value); // PRE
                PrintChildNode(child, indent, isLast, preAction, nameAction, indentAction);
            }
        }

        private static void PrintChildNode<T>(Node<T> node, string indent, bool isLast, Action<T> preAction, Action<T> nameAction, Action<string> indentAction)
        {
            // Print the provided pipes/spaces indent
            //Console.Write(indent);
            indentAction(indent);

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                //Console.Write(Corner);
                indentAction(Corner);
                indent += Space;
            }
            else
            {
                //Console.Write(Cross);
                indentAction(Cross);
                indent += Vertical;
            }

            PrintNode(node, indent, preAction, nameAction, indentAction);
        }
    }
}
