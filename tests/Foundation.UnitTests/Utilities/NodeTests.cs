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
            new StubNode{Id = 1, ParentId = null, Name = "root"}, // first
            new StubNode{Id = 2, ParentId = null, Name = "root2"}, // last
            new StubNode{Id = 10, ParentId = 1, Name = "child of 1"},
            new StubNode{Id = 20, ParentId = 10, Name = "child of 10"},
            new StubNode{Id = 21, ParentId = 10, Name = "child of 10"},
            new StubNode{Id = 11, ParentId = 1, Name = "child of 1"},
            new StubNode{Id = 20, ParentId = 2, Name = "child of 2"}

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

            nodes.Count().ShouldBe(2);
            nodes.First().ShouldNotBeNull();
            nodes.First().Level.ShouldBe(0); // root has level 0
            nodes.First().Value.Id.ShouldBe(1);
            nodes.Last().ShouldNotBeNull();
            nodes.Last().Level.ShouldBe(0); // root has level 0
            nodes.Last().Value.Id.ShouldBe(2);

            nodes.First().Children.All(c => c.Value.Name == "child of 1").ShouldBeTrue(); // direct children of root node
            nodes.First().Descendants.Count(c => c.Value.Name == "child of 10").ShouldBe(2); // find descendants somewhere down the tree
            nodes.First().Descendants.FirstOrDefault(c => c.Value.Name == "child of 10")?.Ancestors.Count().ShouldBe(2); // find the parents/ancestors (up)
            nodes.First().Descendants.FirstOrDefault(c => c.Value.Name == "child of 10")?.Ancestors.Select(a => a.Value.Id).ShouldContain(t => t == 11 || t == 1); // find the parents/ancestors (up)

            // add another child for item 10, note no parent or parentid is provided
            nodes.First().Children.FirstOrDefault(c => c.Value.Id == 10)?.Add(new StubNode { Id = 22, Name = "child of 10" });
            nodes.First().Children.FirstOrDefault(c => c.Value.Id == 10)?.Children.Count().ShouldBe(3);

            // get all descendants from root node (=flatten)
            nodes.First().Descendants.Count().ShouldBe(5);

            // find the root from anywhere
            nodes.First().Descendants.FirstOrDefault(c => c.Value.Name == "child of 10")?.Root.Value.Id.ShouldBe(1);
        }

        public class StubNode
        {
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public string Name { get; set; }
        }
    }
}
