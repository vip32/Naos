namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class NodeExtensions
    {
        // NAME1
        //  ├─NAM1a
        //  ├─NAM2a
        //  │ ├─NAM2ab
        //  │ └─NAM2bb
        //  ├─NAM3a
        //  └─NAM4a
        public static async Task RenderConsole<T>(
            this IEnumerable<Node<T>> source,
            Func<T, string> name = null,
            Func<T, string> header = null,
            Func<T, object> orderBy = null,
            INodeRenderOptions options = null)
        {
            options ??= new ConsoleNodeRenderOptions();

            await source.RenderAsync(name, header, orderBy, new ConsoleNodeRenderOptions()).AnyContext();
        }

        public static async Task RenderAsync<T>(
            this IEnumerable<Node<T>> source,
            Func<T, string> name = null,
            Func<T, string> header = null,
            Func<T, object> orderBy = null,
            INodeRenderOptions options = null)
        {
            name ??= t => t.ToString();
            orderBy ??= t => true == true;
            options ??= new ConsoleNodeRenderOptions();

            foreach(var node in source.Safe()/*.OrderBy(n => orderBy(n.Value))*/)
            {
                if (source.IndexOf(node) != 0)
                {
                    await options.WriteAsync(options.RootNodeBreak).AnyContext();
                }

                await options.WriteAsync(header?.Invoke(node.Value)).AnyContext();
                await RenderNodeAsync(node, indent: string.Empty, name: name, header: header, orderBy: orderBy, options: options).AnyContext(); // ROOT
            }
        }

        private static async Task RenderNodeAsync<T>(
            Node<T> node,
            string indent,
            Func<T, string> name,
            Func<T, string> header,
            Func<T, object> orderBy,
            INodeRenderOptions options)
        {
            await options.WriteAsync(name?.Invoke(node.Value)).AnyContext();
            await options.WriteAsync(options.ChildNodeBreak).AnyContext();

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var children = node.Children.OrderBy(n => orderBy(n.Value)).ToList();
            var childrenCount = node.Children.Count();

            for (var i = 0; i < childrenCount; i++)
            {
                await options.WriteAsync(header?.Invoke(children[i].Value)).AnyContext();
                await RenderChildNodeAsync(children[i], indent, i == (childrenCount - 1), name, header, orderBy, options).AnyContext();
            }
        }

        private static async Task RenderChildNodeAsync<T>(
            Node<T> node,
            string indent,
            bool isLast,
            Func<T, string> name,
            Func<T, string> header,
            Func<T, object> orderBy,
            INodeRenderOptions options)
        {
            await options.WriteAsync(indent).AnyContext();

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                await options.WriteAsync(options.Corner).AnyContext();
                indent += options.Space;
            }
            else
            {
                await options.WriteAsync(options.Cross).AnyContext();
                indent += options.Vertical;
            }

            await RenderNodeAsync(node, indent, name, header, orderBy, options).AnyContext();
        }
    }
}
