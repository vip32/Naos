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
        public static void RenderConsole<T>(this IEnumerable<Node<T>> source, INodeRenderOptions options = null)
        {
            Action<T> preAction = null;
            Action<T> nameAction = t => Console.WriteLine(t.ToString());
            Action<string> indentAction = i => Console.Write(i);

            source.Render(preAction, nameAction, indentAction, options);
        }

        public static void Render<T>(this IEnumerable<Node<T>> source, Action<T> preAction, Action<T> nameAction, Action<string> indentAction, INodeRenderOptions options = null)
        {
            options ??= new DefaultNodeRenderOptions();

            foreach(var node in source.Safe())
            {
                preAction?.Invoke(node.Value);
                RenderNode(node, indent: string.Empty, preAction, nameAction, indentAction, options); // ROOT
            }
        }

        private static void RenderNode<T>(Node<T> node, string indent, Action<T> preAction, Action<T> nameAction, Action<string> indentAction, INodeRenderOptions options)
        {
            //Console.WriteLine(node.Value.ToString()); // NAME
            nameAction?.Invoke(node.Value);

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var numberOfChildren = node.Children.Count();
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children.ToList()[i]; // ? optimize
                var isLast = i == (numberOfChildren - 1);

                preAction?.Invoke(node.Value);
                RenderChildNode(child, indent, isLast, preAction, nameAction, indentAction, options);
            }
        }

        private static void RenderChildNode<T>(Node<T> node, string indent, bool isLast, Action<T> preAction, Action<T> nameAction, Action<string> indentAction, INodeRenderOptions options)
        {
            // Print the provided pipes/spaces indent
            //Console.Write(indent);
            indentAction?.Invoke(indent);

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                //Console.Write(Corner);
                indentAction?.Invoke(options.Corner);
                indent += options.Space;
            }
            else
            {
                //Console.Write(Cross);
                indentAction?.Invoke(options.Cross);
                indent += options.Vertical;
            }

            RenderNode(node, indent, preAction, nameAction, indentAction, options);
        }
    }
}
