namespace Naos.Foundation.UnitTests.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class NodeTests
    {
        private IEnumerable<StubNode> Items => new List<StubNode>
        {
            new StubNode{Id = 1, ParentId = null, Name = "root"},
            new StubNode{Id = 10, ParentId = 1, Name = "child of 1"},
            new StubNode{Id = 20, ParentId = 10, Name = "child of 10"},
            new StubNode{Id = 21, ParentId = 10, Name = "child of 10"},
            new StubNode{Id = 11, ParentId = 1, Name = "child of 1"}

            // - 1 root
            //   - 10 child of 1
            //     - 20 child of 10
            //     - 21 child of 10
            //   - 11 child of 1
        };

        [Fact]
        public void CanCreateTree()
        {
            var nodes = Node<StubNode>.CreateTree(this.Items, l => l.Id, l => l.ParentId);

            nodes.Count().ShouldBe(1);
            nodes.Single().ShouldNotBeNull();
            nodes.Single().Level.ShouldBe(0); // root has level 0
            nodes.Single().Value.Id.ShouldBe(1);

            nodes.Single().Children.All(c => c.Value.Name == "child of 1").ShouldBeTrue(); // direct children of root node
            nodes.Single().Descendants.Count(c => c.Value.Name == "child of 10").ShouldBe(2); // find descendants somewhere down the tree
            nodes.Single().Descendants.FirstOrDefault(c => c.Value.Name == "child of 10")?.Ancestors.Count().ShouldBe(2); // find the parents (up)
            nodes.Single().Descendants.FirstOrDefault(c => c.Value.Name == "child of 10")?.Ancestors.Select(a => a.Value.Id).ShouldContain(t => t == 11 || t == 1); // find the parents (up)
        }

        public class StubNode
        {
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public string Name { get; set; }
        }
    }
}
